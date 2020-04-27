using System;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace MediaIntegrator
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
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
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Debug
            Converter.CsvPath =
                $"E:\\Projekt\\C#\\Distanskurs\\Laboration 3\\StoreSimulator\\StoreSimulator\\bin\\Debug\\Database";
            Converter.XmlPath = $"C:\\Users\\manni\\Desktop";
        }
        
        /// <summary>
        /// Handles the browse button click for both xml and csv
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
                    Converter.CsvPath = browserDialog.SelectedPath;
                    CsvPathText.Text = Converter.CsvPath;
                    break;
                case nameof(Buttons.XmlBrowseButton):
                    Converter.XmlPath = browserDialog.SelectedPath;
                    XmlPathText.Text = Converter.XmlPath;
                    break;
            }
        }
        
        /// <summary>
        /// Handles the start and stop functionality of the converter
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
                    Task.Run(Converter.Start);
                    break;
                
                case nameof(Buttons.StopConverterButton):
                    UpdateConverterStatus(ConverterStatus.Inactive);
                    break;
            }
        }

        private bool ValidateInput()
        {
            if (PathsHaveBeenSelected()) return true;
            
            MessageBox.Show("You need to fill in the location of the csv and the save location for the xml file");
            return false;

        }

        private bool PathsHaveBeenSelected()
        {
            return Converter.CsvPath != null && Converter.XmlPath != null;
        }
        
        private void UpdateConverterStatus(ConverterStatus state)
        {
            ConverterStatusText.Text = $"Status: {state}";
            Converter.ConverterActive = state == ConverterStatus.Active;
        }
    }
}