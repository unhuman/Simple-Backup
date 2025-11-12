using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Text.Json;
using Ookii.Dialogs.Wpf;

namespace Simple_Backup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
  public partial class MainWindow : Window
    {
     private const string ConfigFileName = "backup_config.json";
     private string ConfigFilePath => Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
     "SimpleBackup",
     ConfigFileName);

        private BackupConfig _config = null!;

   public MainWindow()
        {
            try
            {
                InitializeComponent();
                LoadConfiguration();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error initializing MainWindow: {ex.GetType().Name}\n\nMessage: {ex.Message}\n\nStackTrace: {ex.StackTrace}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

     private void LoadConfiguration()
        {
            try
    {
       if (File.Exists(ConfigFilePath))
       {
        string json = File.ReadAllText(ConfigFilePath);
    _config = JsonSerializer.Deserialize<BackupConfig>(json) ?? new BackupConfig();
  }
            else
    {
       _config = new BackupConfig();
   }

   // Load values into UI - normalize paths
     BackupDirectoryTextBox.Text = NormalizePath(_config.BackupDirectory ?? string.Empty);
      RestoreDirectoryTextBox.Text = NormalizePath(_config.RestoreDirectory ?? string.Empty);
   EjectDriveCheckBox.IsChecked = _config.ShouldEjectAfterBackup;
   }
   catch (Exception ex)
   {
      System.Windows.MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         _config = new BackupConfig();
   }
        }

        private void SaveConfiguration()
     {
            try
      {
 // Create directory if it doesn't exist
string? configDir = Path.GetDirectoryName(ConfigFilePath);
      if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
           {
   Directory.CreateDirectory(configDir);
       }

    // Update config from UI - normalize paths
_config.BackupDirectory = NormalizePath(BackupDirectoryTextBox.Text);
      _config.RestoreDirectory = NormalizePath(RestoreDirectoryTextBox.Text);
   _config.ShouldEjectAfterBackup = EjectDriveCheckBox.IsChecked ?? false;

       // Save to file
   string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
           File.WriteAllText(ConfigFilePath, json);
  }
 catch (Exception ex)
      {
         System.Windows.MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
        }

        private string NormalizePath(string path)
     {
   if (string.IsNullOrWhiteSpace(path))
      return string.Empty;

       return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
        }

        private void BackupBrowseButton_Click(object sender, RoutedEventArgs e)
        {
    string? selectedPath = BrowseFolder("Select Source Directory", BackupDirectoryTextBox.Text);
  if (!string.IsNullOrEmpty(selectedPath))
  {
       BackupDirectoryTextBox.Text = selectedPath;
       SaveConfiguration();
        }
     }

   private void RestoreBrowseButton_Click(object sender, RoutedEventArgs e)
        {
         string? selectedPath = BrowseFolder("Select Destination Directory", RestoreDirectoryTextBox.Text);
      if (!string.IsNullOrEmpty(selectedPath))
 {
RestoreDirectoryTextBox.Text = selectedPath;
       SaveConfiguration();
     
    // Check if the selected directory is on a removable drive
bool isRemovable = DriveUtility.IsRemovableDrive(selectedPath);
     System.Diagnostics.Debug.WriteLine($"RestoreBrowseButton_Click: selectedPath={selectedPath}, isRemovable={isRemovable}");
  }
    }

      private string? BrowseFolder(string title, string initialPath = "")
    {
            var dialog = new VistaFolderBrowserDialog
         {
  Description = title
          };

            // Normalize and set initial folder path if provided and exists
   if (!string.IsNullOrEmpty(initialPath))
   {
         // Normalize the path
  string normalizedPath = Path.GetFullPath(initialPath).TrimEnd(Path.DirectorySeparatorChar);

       if (Directory.Exists(normalizedPath))
    {
       // Add trailing slash to force navigation into the directory instead of selecting it
          dialog.SelectedPath = normalizedPath + Path.DirectorySeparatorChar;
   }
 }

      if (dialog.ShowDialog(this) == true)
         {
   // Return the selected path without trailing separator for consistency
        return dialog.SelectedPath.TrimEnd(Path.DirectorySeparatorChar);
  }

         return null;
      }

   private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
        // Validate that both directories are set
 if (string.IsNullOrWhiteSpace(BackupDirectoryTextBox.Text))
  {
 System.Windows.MessageBox.Show("Please select a Source Directory.", "Backup", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }

            if (string.IsNullOrWhiteSpace(RestoreDirectoryTextBox.Text))
{
     System.Windows.MessageBox.Show("Please select a Destination Directory.", "Backup", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
       }

  // Validate directories exist
 if (!Directory.Exists(BackupDirectoryTextBox.Text))
  {
 System.Windows.MessageBox.Show("Source Directory does not exist.", "Backup", MessageBoxButton.OK, MessageBoxImage.Error);
    return;
       }

   if (!Directory.Exists(RestoreDirectoryTextBox.Text))
    {
  System.Windows.MessageBox.Show("Destination Directory does not exist.", "Backup", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
  }

    // Save eject preference
     _config.ShouldEjectAfterBackup = EjectDriveCheckBox.IsChecked ?? false;
    SaveConfiguration();

    System.Diagnostics.Debug.WriteLine($"BackupButton_Click: EjectDrive={EjectDriveCheckBox.IsChecked ?? false}");

    // Show backup progress dialog
     BackupProgressDialog progressDialog = new BackupProgressDialog(
BackupDirectoryTextBox.Text,
   RestoreDirectoryTextBox.Text,
  EjectDriveCheckBox.IsChecked ?? false);
      progressDialog.Owner = this;
  progressDialog.Show();
         progressDialog.StartBackup();
   }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
 SaveConfiguration();
        System.Windows.Application.Current.Shutdown();
  }
 }

    public class BackupConfig
    {
  public string? BackupDirectory { get; set; }
        public string? RestoreDirectory { get; set; }
   public bool ShouldEjectAfterBackup { get; set; } = false;
  }
}
