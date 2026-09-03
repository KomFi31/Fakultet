/*
 * Predstavlja glavni prozor SCADA aplikacije.
 * Pokrece PLC simulator i skeniranje ulaznih tagova pri pokretanju aplikacije.
 * Pri zatvaranju prozora zaustavlja background procese i cuva stanje baze.
 */

using System.Windows;
using DataConcentrator;
using DataConcentrator.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SystemLogger.Log(
                "SCADA application started.");

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

        private void Details_Click(
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

            AnalogInput analogInput =
                selectedTag as AnalogInput;

            if (analogInput == null)
            {
                MessageBox.Show(
                    "Alarm details are available only for analog input tags.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            AlarmDetailsWindow window =
                new AlarmDetailsWindow(analogInput);

            window.Owner = this;
            window.ShowDialog();
        }

        private void EditTag_Click(
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

            AddWindow editWindow =
                new AddWindow(selectedTag);

            editWindow.Owner = this;

            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                LoadTags();
            }
        }

        private void History_Click(
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

            AnalogInput analogInput =
                selectedTag as AnalogInput;

            if (analogInput == null)
            {
                MessageBox.Show(
                    "History is available only for analog input tags.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            HistoryWindow window =
                new HistoryWindow(analogInput);

            window.Owner = this;
            window.ShowDialog();
        }

        private void Report_Click(
            object sender,
            RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog =
                new SaveFileDialog
                {
                    Filter = "Text file (*.txt)|*.txt",
                    FileName = "SCADA_Report.txt"
                };

            if (saveFileDialog.ShowDialog() != true)
                return;

            try
            {
                List<string> reportLines =
                    new List<string>();

                reportLines.Add("SCADA ANALOG INPUT REPORT");
                reportLines.Add(
                    "Generated: " +
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                reportLines.Add("");

                List<AnalogInput> analogInputs =
                    DataConcentratorManager.Instance
                        .GetAllTags()
                        .OfType<AnalogInput>()
                        .ToList();

                foreach (AnalogInput tag in analogInputs)
                {
                    double middleValue =
                        (tag.HighLimit + tag.LowLimit) / 2.0;

                    double lowerBound =
                        middleValue - 5;

                    double upperBound =
                        middleValue + 5;

                    List<AnalogValueRecord> history =
                        DataConcentratorManager.Instance
                            .GetAnalogHistory(tag.Name);

                    List<AnalogValueRecord> matchingValues =
                        history
                            .Where(record =>
                                record.Value >= lowerBound &&
                                record.Value <= upperBound)
                            .ToList();

                    reportLines.Add(
                        "Tag: " + tag.Name);

                    reportLines.Add(
                        "Required range: " +
                        lowerBound.ToString("F2") +
                        " - " +
                        upperBound.ToString("F2") +
                        " " +
                        tag.Units);

                    if (matchingValues.Count == 0)
                    {
                        reportLines.Add(
                            "No recorded values in required range.");
                    }
                    else
                    {
                        foreach (AnalogValueRecord record
                            in matchingValues)
                        {
                            reportLines.Add(
                                record.TimeStamp
                                    .ToString("yyyy-MM-dd HH:mm:ss.fff") +
                                " | " +
                                record.Value.ToString("F2") +
                                " " +
                                tag.Units);
                        }
                    }

                    reportLines.Add("");
                }

                File.WriteAllLines(
                    saveFileDialog.FileName,
                    reportLines);

                SystemLogger.Log(
                    "Report generated: " +
                    saveFileDialog.FileName);

                MessageBox.Show(
                    "Report successfully generated.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                SystemLogger.LogError(ex);

                MessageBox.Show(
                    "Error while generating report: " +
                    ex.Message,
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

            SystemLogger.Log(
                "SCADA application closed.");
        }
    }
}