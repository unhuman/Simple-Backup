using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Simple_Backup
{
    public partial class BackupProgressDialog : Window
    {
  private string _sourceDirectory;
        private string _destinationDirectory;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private List<FileInfo> _filesToCopy = new List<FileInfo>();
    private List<string> _directoriesToCreate = new List<string>();
        private long _totalBytes = 0;
        private long _copiedBytes = 0;
      private bool _isOperationComplete = false;
      private bool _wasCancelledMidOperation = false;
    private ConflictResolutionDialog.ConflictResolution _conflictResolution = ConflictResolutionDialog.ConflictResolution.None;
    private bool _shouldEjectDriveAfterBackup = false;

    public BackupProgressDialog(string sourceDirectory, string destinationDirectory, bool shouldEjectDrive = false)
     {
   InitializeComponent();
       _sourceDirectory = sourceDirectory;
 _destinationDirectory = destinationDirectory;
     _shouldEjectDriveAfterBackup = shouldEjectDrive;
    
  // Ensure button states are correct at startup
    CancelButton.IsEnabled = true;
   CloseButton.IsEnabled = false;
     }

        public async void StartBackup()
        {
  try
 {
       // Phase 1: Research (keep on background thread since it's just enumerating)
     await ResearchPhase();

     if (_cancellationTokenSource.Token.IsCancellationRequested)
       {
          OverallStatusText.Text = "Status: Backup cancelled by user";
         _isOperationComplete = true;
   UpdateButtonStates();
          return;
     }

             // Phase 2: Copy (run on UI thread so dialogs work properly)
     await CopyPhaseOnUIThread();

   // Check if cancelled mid-operation
        if (_wasCancelledMidOperation)
                {
              OverallStatusText.Text = "Status: Backup cancelled by user";
           }
       else
        {
          OverallStatusText.Text = "Status: Backup completed successfully!";
      
       // Eject drive if requested and backup was successful
if (_shouldEjectDriveAfterBackup)
    {
  System.Diagnostics.Debug.WriteLine("Attempting to eject drive...");
     
   // Show eject dialog with retry logic
   var ejectDialog = new EjectDialog(_destinationDirectory);
      ejectDialog.Owner = this;
    ejectDialog.Show();
     ejectDialog.StartEject();
     }
      }
      _isOperationComplete = true;
     UpdateButtonStates();
            }
            catch (Exception ex)
          {
      OverallStatusText.Text = $"Status: Error - {ex.Message}";
      _isOperationComplete = true;
        UpdateButtonStates();
         MessageBox.Show($"Backup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
        }

        private async Task ResearchPhase()
        {
    await Task.Run(() =>
            {
       try
          {
        _filesToCopy.Clear();
    _directoriesToCreate.Clear();
      _totalBytes = 0;

        Dispatcher.Invoke(() =>
             {
         ResearchProgressBar.Value = 0;
            ResearchStatusText.Text = "Scanning for files...";
         OverallStatusText.Text = "Status: Researching files in source directory...";
      });

          // Recursively find all files and directories
          EnumerateFilesAndDirectories(_sourceDirectory, _filesToCopy, _directoriesToCreate);

    // Calculate total bytes
 foreach (var file in _filesToCopy)
           {
      _totalBytes += file.Length;
    }

Dispatcher.Invoke(() =>
        {
            ResearchProgressBar.Value = 100;
          ResearchStatusText.Text = "Research complete!";
   ResearchCountText.Text = $"Files found: {_filesToCopy.Count} ({FormatBytes(_totalBytes)}) | Directories: {_directoriesToCreate.Count}";
       OverallStatusText.Text = "Status: Research phase complete. Starting copy phase...";

                // Enable copy phase UI
           CopyProgressBar.Opacity = 1.0;
  CopyStatusText.Opacity = 1.0;
            CopyCountText.Opacity = 1.0;
      });
          }
                catch (Exception ex)
       {
    Dispatcher.Invoke(() =>
        {
         ResearchStatusText.Text = $"Error: {ex.Message}";
  });
                    throw;
      }
   }, _cancellationTokenSource.Token);
        }

  private async Task CopyPhaseOnUIThread()
        {
 try
            {
                CopyProgressBar.Value = 0;
                CopyStatusText.Text = "Creating directories...";
                OverallStatusText.Text = "Status: Creating directory structure...";

                // Run the entire copy operation on a background thread
                await Task.Run(() =>
                {
                    try
                    {
                        // First, create all directories
                        foreach (var directory in _directoriesToCreate)
                        {
                            if (_cancellationTokenSource.Token.IsCancellationRequested)
                                break;

                            try
                            {
                                Directory.CreateDirectory(directory);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error creating directory {directory}: {ex.Message}");
                            }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            CopyStatusText.Text = "Copying files...";
                            OverallStatusText.Text = "Status: Copying files...";
                        });

                        int filesCopied = 0;
                        _copiedBytes = 0;

                        foreach (var file in _filesToCopy)
                        {
                            if (_cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                _wasCancelledMidOperation = true;
                                break;
                            }

                            try
                            {
                                string relativePath = file.FullName.Substring(_sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                                string destinationPath = Path.Combine(_destinationDirectory, relativePath);
                                string destinationDir = Path.GetDirectoryName(destinationPath);

                                if (!Directory.Exists(destinationDir))
                                {
                                    Directory.CreateDirectory(destinationDir);
                                }

                                // Check if file already exists and handle conflict
                                if (File.Exists(destinationPath))
                                {
                                    System.Diagnostics.Debug.WriteLine($"Conflict detected for: {destinationPath}");
                                    
                                    ConflictResolutionDialog.ConflictResolution resolution = ConflictResolutionDialog.ConflictResolution.Skip;
                                    
                                    // Use Dispatcher to show dialog on UI thread and wait for result
                                    Dispatcher.Invoke(() =>
                                    {
                                        CopyStatusText.Text = $"Conflict: {file.Name} - showing dialog...";
                                    });

                                    // Show dialog and get resolution
                                    Dispatcher.Invoke(async () =>
                                    {
                                        resolution = await HandleFileConflictAsync(destinationPath);
                                    });

                                    System.Diagnostics.Debug.WriteLine($"Conflict resolution: {resolution}");

                                    if (resolution == ConflictResolutionDialog.ConflictResolution.Skip)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Skipping file: {destinationPath}");
                                        continue;
                                    }
                                }

                                System.Diagnostics.Debug.WriteLine($"Copying file: {file.FullName} -> {destinationPath}");
                                File.Copy(file.FullName, destinationPath, true);
                                _copiedBytes += file.Length;
                                filesCopied++;

                                double progressPercent = _totalBytes > 0 ? (_copiedBytes / (double)_totalBytes) * 100 : 0;

                                // Update UI on UI thread
                                Dispatcher.Invoke(() =>
                                {
                                    CopyProgressBar.Value = Math.Min(progressPercent, 100);
                                    CopyStatusText.Text = $"Copying: {file.Name}";
                                    CopyCountText.Text = $"Files copied: {filesCopied} / {_filesToCopy.Count} ({FormatBytes(_copiedBytes)}/{FormatBytes(_totalBytes)})";
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Exception in copy loop: {ex}");
                                Dispatcher.Invoke(() =>
                                {
                                    CopyStatusText.Text = $"Error copying {file.Name}: {ex.Message}";
                                });
                            }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            CopyProgressBar.Value = 100;
                            CopyStatusText.Text = "Copy phase complete!";
                            System.Diagnostics.Debug.WriteLine("Copy phase complete");
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Exception in CopyPhaseOnUIThread: {ex}");
                        Dispatcher.Invoke(() =>
                        {
                            CopyStatusText.Text = $"Error: {ex.Message}";
                        });
                        throw;
                    }
                }, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in CopyPhaseOnUIThread: {ex}");
                CopyStatusText.Text = $"Error: {ex.Message}";
                throw;
            }
        }

        private void EnumerateFilesAndDirectories(string directory, List<FileInfo> fileList, List<string> directoryList)
        {
            try
            {
          DirectoryInfo dir = new DirectoryInfo(directory);

           // Add all files in current directory
    foreach (var file in dir.GetFiles())
            {
 fileList.Add(file);
    }

    // Add all subdirectories (including empty ones)
      foreach (var subdir in dir.GetDirectories())
      {
      // Calculate the relative path for the destination
  string relativePath = subdir.FullName.Substring(_sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
 string destinationPath = Path.Combine(_destinationDirectory, relativePath);
        directoryList.Add(destinationPath);

        // Recursively process subdirectories
       EnumerateFilesAndDirectories(subdir.FullName, fileList, directoryList);
    }
   }
            catch (UnauthorizedAccessException ex)
    {
       // Skip directories we can't access
   System.Diagnostics.Debug.WriteLine($"Access denied: {directory} - {ex.Message}");
          }
            catch (Exception ex)
            {
System.Diagnostics.Debug.WriteLine($"Error enumerating {directory}: {ex.Message}");
         }
   }

        private string FormatBytes(long bytes)
        {
        string[] sizes = { "B", "KB", "MB", "GB" };
    double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
      {
     order++;
                len = len / 1024;
       }
  return $"{len:0.##} {sizes[order]}";
}

  private async Task<ConflictResolutionDialog.ConflictResolution> HandleFileConflictAsync(string filePath)
        {
  System.Diagnostics.Debug.WriteLine($"HandleFileConflictAsync called for: {filePath}");
            
   // If user has already selected a "All" option, use it
            if (_conflictResolution == ConflictResolutionDialog.ConflictResolution.SkipAll)
     {
    System.Diagnostics.Debug.WriteLine("Using SkipAll preference");
                return ConflictResolutionDialog.ConflictResolution.Skip;
   }
            if (_conflictResolution == ConflictResolutionDialog.ConflictResolution.OverwriteAll)
  {
      System.Diagnostics.Debug.WriteLine("Using OverwriteAll preference");
          return ConflictResolutionDialog.ConflictResolution.Overwrite;
            }

            System.Diagnostics.Debug.WriteLine("Showing conflict dialog");
      
       var dialog = new ConflictResolutionDialog(Path.GetFileName(filePath));
     dialog.Owner = this;
        dialog.ShowDialog();
          
       _conflictResolution = dialog.Resolution;
      System.Diagnostics.Debug.WriteLine($"User selected: {dialog.Resolution}");
            
   if (dialog.Resolution == ConflictResolutionDialog.ConflictResolution.Skip)
                return ConflictResolutionDialog.ConflictResolution.Skip;
          else if (dialog.Resolution == ConflictResolutionDialog.ConflictResolution.SkipAll)
     return ConflictResolutionDialog.ConflictResolution.SkipAll;
            else if (dialog.Resolution == ConflictResolutionDialog.ConflictResolution.Overwrite)
    return ConflictResolutionDialog.ConflictResolution.Overwrite;
         else if (dialog.Resolution == ConflictResolutionDialog.ConflictResolution.OverwriteAll)
        return ConflictResolutionDialog.ConflictResolution.OverwriteAll;
  else
      return ConflictResolutionDialog.ConflictResolution.Skip;
    }

        private void UpdateButtonStates()
        {
      Dispatcher.Invoke(() =>
     {
      if (_isOperationComplete)
       {
              CancelButton.IsEnabled = false;
   CloseButton.IsEnabled = true;
       }
          else
       {
         CancelButton.IsEnabled = true;
 CloseButton.IsEnabled = false;
}
         });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
     if (_isOperationComplete)
     {
     // If operation is complete, close the dialog
    this.Close();
      return;
            }

    // Check if operation is still in flight
     if (!_isOperationComplete)
 {
     var confirmDialog = new CancellationConfirmDialog();
     confirmDialog.Owner = this;
confirmDialog.ShowDialog();

  if (confirmDialog.Result == CancellationConfirmDialog.CancellationResult.Cancel)
      {
  _cancellationTokenSource.Cancel();
          CancelButton.IsEnabled = false;
   OverallStatusText.Text = "Status: Cancellation requested...";
     }
   }
        }

      private void CloseButton_Click(object sender, RoutedEventArgs e)
   {
        this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
     // If operation is complete, allow close
 if (_isOperationComplete)
        {
      return;
   }

    // If cancel button is still enabled and we're trying to close, warn the user
   if (CancelButton.IsEnabled && !_isOperationComplete)
   {
  var confirmDialog = new CancellationConfirmDialog();
  confirmDialog.Owner = this;
   confirmDialog.ShowDialog();

    if (confirmDialog.Result == CancellationConfirmDialog.CancellationResult.Continue)
       {
        e.Cancel = true;
      }
      else if (confirmDialog.Result == CancellationConfirmDialog.CancellationResult.Cancel)
      {
           _cancellationTokenSource.Cancel();
           CancelButton.IsEnabled = false;
           OverallStatusText.Text = "Status: Cancellation requested...";
       e.Cancel = true;
   }
  }
   else if (!_isOperationComplete && !CancelButton.IsEnabled)
    {
        // Cancel button is already disabled (cancellation in progress), allow close
   return;
   }
        }
    }
}
