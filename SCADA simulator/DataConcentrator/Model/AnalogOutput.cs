namespace DataConcentrator.Model
{
    /*
     * Predstavlja analogni izlazni tag.
     * Sadrzi granicne vrednosti, jedinicu mere i pocetnu vrednost izlaza.
     * Koristi se za upis analognih vrednosti iz SCADA aplikacije ka PLC simulatoru.
     */
    public class AnalogOutput : Tag
    {
        private double lowLimit;
        private double highLimit;
        private string units;
        private double initialValue;

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

        // Vrednost koja se postavlja prilikom kreiranja output taga
        public double InitialValue
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