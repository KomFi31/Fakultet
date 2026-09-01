namespace DataConcentrator.Model
{
    /*
    * Predstavlja analogni ulazni tag.
    * Sadrzi parametre za skeniranje, granicne vrednosti i alarmnu logiku
    * kao sto su deadband i hysteresis.
    */
    public class AnalogInput : Tag
    {
        private double scanTime;
        private bool onScan;
        private double lowLimit;
        private double highLimit;
        private string units;
        private double deadband;
        private double hysteresis;

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

        public double LowLimit
        {
            get { return lowLimit; }
            set
            {
                lowLimit = value;
                OnPropertyChanged("LowLimit");
            }
        }

        public double HighLimit
        {
            get { return highLimit; }
            set
            {
                highLimit = value;
                OnPropertyChanged("HighLimit");
            }
        }

        public string Units
        {
            get { return units; }
            set
            {
                units = value;
                OnPropertyChanged("Units");
            }
        }

        public double Deadband
        {
            get { return deadband; }
            set
            {
                deadband = value;
                OnPropertyChanged("Deadband");
            }
        }

        public double Hysteresis
        {
            get { return hysteresis; }
            set
            {
                hysteresis = value;
                OnPropertyChanged("Hysteresis");
            }
        }
    }
}