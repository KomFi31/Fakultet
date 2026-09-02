/*
 * Predstavlja glavni prozor SCADA aplikacije.
 * Pokrece PLC simulator i skeniranje ulaznih tagova pri pokretanju aplikacije.
 * Pri zatvaranju prozora zaustavlja background procese i cuva stanje baze.
 */

using System.Windows;
using DataConcentrator;
using DataConcentrator.Model;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataConcentratorManager manager =
                DataConcentratorManager.Instance;

            manager.StartPLCSimulator();
            manager.StartScanning();

            LoadTags();
        }

        private void LoadTags()
        {
            tagsDataGrid.ItemsSource =
                DataConcentratorManager.Instance.GetAllTags();
        }

        private void AddTag_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();

            addWindow.Owner = this;

            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                LoadTags();
            }
        }

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadTags();
        }

        private void ToggleScan_Click(
            object sender,
            RoutedEventArgs e)
        {
            Tag selectedTag =
                tagsDataGrid.SelectedItem as Tag;

            if (selectedTag == null)
            {
                MessageBox.Show(
                    "Select a tag first.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            bool newScanState;

            if (selectedTag is AnalogInput analogInput)
            {
                newScanState = !analogInput.OnScan;
            }
            else if (selectedTag is DigitalInput digitalInput)
            {
                newScanState = !digitalInput.OnScan;
            }
            else
            {
                MessageBox.Show(
                    "Scan can only be enabled or disabled for input tags.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            DataConcentratorManager.Instance
                .SetInputScan(selectedTag.Name, newScanState);

            tagsDataGrid.Items.Refresh();
        }

        private void DeleteTag_Click(
            object sender,
            RoutedEventArgs e)
        {
            Tag selectedTag =
                tagsDataGrid.SelectedItem as Tag;

            if (selectedTag == null)
            {
                MessageBox.Show(
                    "Select a tag first.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    "Are you sure you want to delete tag '" +
                    selectedTag.Name + "'?",
                    "Delete tag",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            bool removed =
                DataConcentratorManager.Instance
                    .RemoveTag(selectedTag.Name);

            if (!removed)
            {
                MessageBox.Show(
                    "Tag could not be deleted.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            LoadTags();
        }

        private void WriteValue_Click(
            object sender,
            RoutedEventArgs e)
        {
            Tag selectedTag =
                tagsDataGrid.SelectedItem as Tag;

            if (selectedTag == null)
            {
                MessageBox.Show(
                    "Select a tag first.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (!(selectedTag is AnalogOutput) &&
                !(selectedTag is DigitalOutput))
            {
                MessageBox.Show(
                    "Values can only be written to output tags.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            WriteValueWindow window =
                new WriteValueWindow(selectedTag);

            window.Owner = this;

            window.ShowDialog();
        }

        private void Window_Closing(
            object sender,
            System.ComponentModel.CancelEventArgs e)
        {
            DataConcentratorManager manager =
                DataConcentratorManager.Instance;

            manager.StopScanning();
            manager.StopPLCSimulator();

            ContextClass.Instance.SaveChanges();
            ContextClass.Instance.Dispose();
        }
    }
}