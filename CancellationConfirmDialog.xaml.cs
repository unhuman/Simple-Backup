using System.Windows;

namespace Simple_Backup
{
    public partial class CancellationConfirmDialog : Window
    {
        public enum CancellationResult { None, Continue, Cancel }

        public CancellationResult Result { get; set; } = CancellationResult.None;

        public CancellationConfirmDialog()
        {
    InitializeComponent();
        }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
      Result = CancellationResult.Continue;
    this.Close();
        }

     private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = CancellationResult.Cancel;
         this.Close();
        }
    }
}
