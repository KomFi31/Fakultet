using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator.Model
{
     /*
     * Predstavlja definiciju alarma vezanog za analogni ulazni tag.
     * Sadrzi granicu, uslov aktivacije, poruku i trenutno stanje alarma.
     * Koristi se za proveru da li se analogna vrednost nalazi u alarmnoj zoni.
     */

    // Above - aktivacija pri prekoracenju granice, Below - aktivacija prilikom pada ispod granice
    public enum AlarmCondition
    {
        Above,
        Below
    }

    public enum AlarmState
    {
        Inactive,
        Active,
        Acknowledged
    }

    public class Alarm : INotifyPropertyChanged
    {
        private int id;
        private string tagName;
        private double limit;
        private AlarmCondition condition;
        private string message;
        private AlarmState state;

        [Key]
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        // Ime AnalogInput taga za koji je alarm vezan
        public string TagName
        {
            get { return tagName; }
            set
            {
                tagName = value;
                OnPropertyChanged("TagName");
            }
        }

        public double Limit
        {
            get { return limit; }
            set
            {
                limit = value;
                OnPropertyChanged("Limit");
            }
        }

        public AlarmCondition Condition
        {
            get { return condition; }
            set
            {
                condition = value;
                OnPropertyChanged("Condition");
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }

        public AlarmState State
        {
            get { return state; }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}