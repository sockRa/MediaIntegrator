using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace MediaIntegrator
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Handles the browse button click for both xml and csv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButtons_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;

            var browserDialog = new FolderBrowserDialog();

            if (browserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            switch (button.Name)
            {
                case nameof(Buttons.CsvBrowseButton):
                    Converter.CsvDirectoryPath = browserDialog.SelectedPath;
                    CsvPathText.Text = Converter.CsvDirectoryPath;
                    break;
                case nameof(Buttons.XmlBrowseButton):
                    Converter.XmlDirectoryPath = browserDialog.SelectedPath;
                    XmlPathText.Text = Converter.XmlDirectoryPath;
                    break;
            }
        }

        /// <summary>
        ///     Handles the start and stop functionality of the converter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartStopConverterButtons_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;

            switch (button.Name)
            {
                case nameof(Buttons.StartConverterButton):
                    if (!ValidateInput()) return;
                    UpdateConverterStatus(ConverterStatus.Active);

                    // Create a new thread because there is an infinite while loop in the Start method.
                    // This ensures that the gui can work alongside the logic surrounding the file system watcher logic 
                    Task.Run(Converter.Start);
                    break;

                case nameof(Buttons.StopConverterButton):
                    UpdateConverterStatus(ConverterStatus.Inactive);
                    break;
            }
        }

        private static bool ValidateInput()
        {
            if (PathsHaveBeenSelected()) return true;

            MessageBox.Show("You need to fill in the location of the csv and the save location for the xml file");
            return false;
        }

        private static bool PathsHaveBeenSelected()
        {
            return Converter.CsvDirectoryPath != null && Converter.XmlDirectoryPath != null;
        }

        private void UpdateConverterStatus(ConverterStatus state)
        {
            ConverterStatusText.Text = $"Status: {state}";
            Converter.ConverterActive = state == ConverterStatus.Active;
        }

        private enum Buttons
        {
            StartConverterButton,
            StopConverterButton,
            CsvBrowseButton,
            XmlBrowseButton
        }

        private enum ConverterStatus
        {
            Active,
            Inactive
        }
    }
}