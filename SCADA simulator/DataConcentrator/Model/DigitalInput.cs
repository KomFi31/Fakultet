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
    }
}