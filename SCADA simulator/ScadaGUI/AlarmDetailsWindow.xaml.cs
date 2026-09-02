/*
 * Prikazuje alarme vezane za izabrani analogni ulazni tag.
 * Omogucava korisniku pregled trenutnog stanja alarma
 * i acknowledge aktivnih alarma.
 */

using DataConcentrator;
using DataConcentrator.Model;
using System;
using System.Windows;

namespace ScadaGUI
{
    public partial class AlarmDetailsWindow : Window
    {
        private readonly AnalogInput tag;
        private readonly DataConcentratorManager manager;

        public AlarmDetailsWindow(AnalogInput selectedTag)
        {
            InitializeComponent();

            tag = selectedTag;
            manager = DataConcentratorManager.Instance;

            tagNameTextBlock.Text =
                "Alarms - " + tag.Name;

            LoadAlarms();

            // Automatsko osvezavanje kada se aktivira novi alarm.
            manager.AlarmActivated += OnAlarmActivated;

            Closed += AlarmDetailsWindow_Closed;
        }

        private void LoadAlarms()
        {
            alarmsDataGrid.ItemsSource =
                manager.GetAlarmsForTag(tag.Name);
        }

        private void Acknowledge_Click(
            object sender,
            RoutedEventArgs e)
        {
            Alarm selectedAlarm =
                alarmsDataGrid.SelectedItem as Alarm;

            if (selectedAlarm == null)
            {
                MessageBox.Show(
                    "Select an alarm first.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (selectedAlarm.State != AlarmState.Active)
            {
                MessageBox.Show(
                    "Only an active alarm can be acknowledged.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            bool success =
                manager.AcknowledgeAlarm(selectedAlarm.Id);

            if (!success)
            {
                MessageBox.Show(
                    "Alarm could not be acknowledged.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            LoadAlarms();
        }

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadAlarms();
        }

        private void OnAlarmActivated(int alarmId)
        {
            // Alarm event dolazi sa scan thread-a,
            // pa GUI osvezavamo preko Dispatcher-a.
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    LoadAlarms();
                }));
        }

        private void AlarmDetailsWindow_Closed(
            object sender,
            EventArgs e)
        {
            manager.AlarmActivated -= OnAlarmActivated;
        }

        private void Close_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}