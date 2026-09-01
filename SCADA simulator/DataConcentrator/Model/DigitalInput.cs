using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator.Model
{
    /*
     * Predstavlja digitalni ulazni tag.
     * Sadrzi parametre za skeniranje i koristi se za ocitavanje binarnih stanja iz PLC simulatora.
     */
    public class DigitalInput : Tag
    {
        private double scanTime;
        private bool onScan;

        private bool? currentValue;

        public double ScanTime
        {
            get { return scanTime; }
            set
            {
                scanTime = value;
                OnPropertyChanged("ScanTime");
            }
        }

        public bool OnScan
        {
            get { return onScan; }
            set
            {
                onScan = value;
                OnPropertyChanged("OnScan");
            }
        }

        // Trenutno digitalno stanje ocitano iz PLC simulatora.
        // Vrednost se ne perzistira u bazu.
        [NotMapped]
        public bool? CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                OnPropertyChanged("CurrentValue");
            }
        }
    }
}