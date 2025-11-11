using System.Windows;

namespace Simple_Backup
{
    public partial class ConflictResolutionDialog : Window
    {
  public enum ConflictResolution { None, Skip, SkipAll, Overwrite, OverwriteAll }

        public ConflictResolution Resolution { get; set; } = ConflictResolution.None;

        public ConflictResolutionDialog(string fileName)
        {
InitializeComponent();
      FileNameTextBlock.Text = $"File: {fileName}";
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
       Resolution = ConflictResolution.Skip;
             this.Close();
    }

     private void SkipAllButton_Click(object sender, RoutedEventArgs e)
       {
      Resolution = ConflictResolution.SkipAll;
        this.Close();
        }

        private void OverwriteButton_Click(object sender, RoutedEventArgs e)
        {
         Resolution = ConflictResolution.Overwrite;
      this.Close();
   }

        private void OverwriteAllButton_Click(object sender, RoutedEventArgs e)
 {
  Resolution = ConflictResolution.OverwriteAll;
      this.Close();
   }
    }
}
