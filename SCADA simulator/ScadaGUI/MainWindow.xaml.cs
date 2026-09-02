/*
 * Predstavlja glavni prozor SCADA aplikacije.
 * Pokrece PLC simulator i skeniranje ulaznih tagova pri pokretanju aplikacije.
 * Pri zatvaranju prozora zaustavlja background procese i cuva stanje baze.
 */

using System.Windows;
using DataConcentrator;

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
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();

            addWindow.Owner = this;

            addWindow.ShowDialog();
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