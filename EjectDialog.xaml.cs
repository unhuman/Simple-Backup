using System;
using System.Threading.Tasks;
using System.Windows;

namespace Simple_Backup
{
    public partial class EjectDialog : Window
    {
 private string _drivePath;
  private bool _isEjecting = true;
        private const int MaxRetries = 10;
        private const int RetryDelayMs = 1000;

        public EjectDialog(string drivePath)
        {
 InitializeComponent();
  _drivePath = drivePath;
        }

     public async void StartEject()
    {
    await AttemptEject();
        }

        private async Task AttemptEject()
 {
   int attempts = 0;

    while (attempts < MaxRetries && _isEjecting)
     {
          attempts++;
    StatusText.Text = $"Attempting to eject drive... (Attempt {attempts}/{MaxRetries})";
       System.Diagnostics.Debug.WriteLine($"Eject attempt {attempts}");

    bool success = DriveUtility.EjectDrive(_drivePath);

  if (success)
   {
       StatusText.Text = "Drive ejected successfully!";
       ProgressBar.IsIndeterminate = false;
  ProgressBar.Value = 100;
    ActionButton.Content = "Close";
        _isEjecting = false;
     return;
    }

     if (attempts < MaxRetries)
      {
    await Task.Delay(RetryDelayMs);
 }
            }

   // If we get here, eject failed
         StatusText.Text = "Eject failed after multiple attempts. You can safely remove the drive.";
     ProgressBar.IsIndeterminate = false;
   ProgressBar.Value = 0;
        ActionButton.Content = "Close";
_isEjecting = false;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
    if (_isEjecting)
           {
       // Cancel requested
 _isEjecting = false;
           StatusText.Text = "Eject cancelled.";
  ProgressBar.IsIndeterminate = false;
   ActionButton.Content = "Close";
    }
            else
            {
     // Close button clicked
        this.Close();
 }
        }
    }
}
