using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator.Model
{
     /*
     * Predstavlja digitalni izlazni tag.
     * Sadrzi pocetnu vrednost i koristi se za upis binarnih komandi iz SCADA aplikacije.
     */
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

        private bool? currentValue;

        [NotMapped]
        public bool? CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                OnPropertyChanged(nameof(CurrentValue));
            }
        }
    }
}