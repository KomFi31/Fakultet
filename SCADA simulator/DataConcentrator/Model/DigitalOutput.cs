namespace DataConcentrator.Model
{
    public class DigitalOutput : Tag
    {
        private bool initialValue;

        // Pocetna vrednost digitalnog izlaza
        public bool InitialValue
        {
            get { return initialValue; }
            set
            {
                initialValue = value;
                OnPropertyChanged("InitialValue");
            }
        }
    }
}