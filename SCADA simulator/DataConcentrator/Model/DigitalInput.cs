namespace DataConcentrator.Model
{
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