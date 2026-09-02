using System;
using System.Windows;
using System.Windows.Threading;
using DataConcentrator;
using DataConcentrator.Model;
using PLCSimulator;
using System.Linq;

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

            DataConcentratorManager manager =
                DataConcentratorManager.Instance;

            try
            {
                manager.RemoveTag("SCAN_TEST_AI");

                AnalogInput testTag = new AnalogInput
                {
                    Name = "SCAN_TEST_AI",
                    Description = "Scan test analog input",
                    IOAddress = "ADDR001",

                    // Ocitavanje na svakih 100 ms
                    ScanTime = 0.1,
                    OnScan = true,

                    LowLimit = 0,
                    HighLimit = 100,
                    Units = "%",

                    // Za potrebe testa prihvatamo svaku promenu.
                    Deadband = 0,
                    Hysteresis = 2
                };

                manager.AddTag(testTag);

                Alarm testAlarm = new Alarm
                {
                    TagName = "SCAN_TEST_AI",

                    // ADDR001 generise vrednost >= 0,
                    // pa ce alarm sigurno biti aktiviran.
                    Limit = -1,

                    Condition = AlarmCondition.Above,
                    Message = "Scan test alarm"
                };

                manager.AddAlarm(testAlarm);

                int activatedBefore =
                    manager.GetActivatedAlarmsForTag("SCAN_TEST_AI").Count;

                int alarmEvents = 0;

                manager.AlarmActivated += (alarmId) =>
                {
                    System.Threading.Interlocked.Increment(ref alarmEvents);
                };

                manager.StartPLCSimulator();
                manager.StartScanning();

                // Dovoljno vremena za nekoliko ciklusa skeniranja.
                System.Threading.Thread.Sleep(1200);

                AnalogInput loadedTag =
                    manager.GetTag("SCAN_TEST_AI") as AnalogInput;

                Alarm loadedAlarm =
                    manager.GetAlarmsForTag("SCAN_TEST_AI")
                           .FirstOrDefault();

                int activatedAfter =
                    manager.GetActivatedAlarmsForTag("SCAN_TEST_AI").Count;

                manager.StopScanning();
                manager.StopPLCSimulator();

                MessageBox.Show(
                    "Current value: " + loadedTag?.CurrentValue +
                    "\nAlarm state: " + loadedAlarm?.State +
                    "\nActivated before: " + activatedBefore +
                    "\nActivated after: " + activatedAfter +
                    "\nNew activated records: " +
                        (activatedAfter - activatedBefore) +
                    "\nAlarm events: " + alarmEvents
                );

                // Brisemo samo definiciju taga i alarma.
                // ActivatedAlarm ostaje kao istorijski zapis.
                manager.RemoveTag("SCAN_TEST_AI");
            }
            catch (Exception ex)
            {
                manager.StopScanning();
                manager.StopPLCSimulator();

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