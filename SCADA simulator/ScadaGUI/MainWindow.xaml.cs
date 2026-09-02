using System;
using System.Windows;
using System.Windows.Threading;
using DataConcentrator;
using DataConcentrator.Model;
using PLCSimulator;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                DataConcentratorManager manager =
                    DataConcentratorManager.Instance;

                // Cistimo test podatke ako su ostali od prethodnog pokretanja.
                manager.RemoveTag("TEST_AI");

                AnalogInput testTag = new AnalogInput
                {
                    Name = "TEST_AI",
                    Description = "Initial description",
                    IOAddress = "ADDR001",
                    ScanTime = 1,
                    OnScan = true,
                    LowLimit = 0,
                    HighLimit = 100,
                    Units = "C",
                    Deadband = 1,
                    Hysteresis = 2
                };

                bool added = manager.AddTag(testTag);

                AnalogInput updatedTag = new AnalogInput
                {
                    Name = "TEST_AI",
                    Description = "Updated description",
                    IOAddress = "ADDR001",
                    ScanTime = 0.5,
                    OnScan = true,
                    LowLimit = 0,
                    HighLimit = 120,
                    Units = "C",
                    Deadband = 0.5,
                    Hysteresis = 3
                };

                bool updated = manager.UpdateTag(updatedTag);

                AnalogInput loadedTag =
                    manager.GetTag("TEST_AI") as AnalogInput;

                Alarm testAlarm = new Alarm
                {
                    TagName = "TEST_AI",
                    Limit = 80,
                    Condition = AlarmCondition.Above,
                    Message = "Test temperature alarm"
                };

                bool alarmAdded = manager.AddAlarm(testAlarm);

                int alarmsBeforeDelete =
                    manager.GetAlarmsForTag("TEST_AI").Count;

                bool removed = manager.RemoveTag("TEST_AI");

                Tag tagAfterDelete =
                    manager.GetTag("TEST_AI");

                int alarmsAfterDelete =
                    manager.GetAlarmsForTag("TEST_AI").Count;

                MessageBox.Show(
                    "Tag added: " + added +
                    "\nTag updated: " + updated +
                    "\nDescription: " + loadedTag?.Description +
                    "\nScan time: " + loadedTag?.ScanTime +
                    "\nAlarm added: " + alarmAdded +
                    "\nAlarms before delete: " + alarmsBeforeDelete +
                    "\nTag removed: " + removed +
                    "\nTag exists after delete: " + (tagAfterDelete != null) +
                    "\nAlarms after delete: " + alarmsAfterDelete
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Window_Closing(
            object sender,
            System.ComponentModel.CancelEventArgs e)
        {
            // Za sada ostavljamo handler jer je povezan sa MainWindow.xaml.
            // Kasnije cemo ovde zaustaviti scanning i PLC simulator.
        }
    }
}