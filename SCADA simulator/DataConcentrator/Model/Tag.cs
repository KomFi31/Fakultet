using DataConcentrator.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator
{
    // napraviti AnalogInput, AnalogOuput, DigitalInput i 
    // DigitalOutput klase koje nasledjuju Tag klasu
    public class Tag : INotifyPropertyChanged
    {
       
        private string name;

        private string description;

        private string ioAddress;

        private TagType type;


        #region Properties

        [Key]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }


        public string IOAddress
        {
            get { return ioAddress; }
            set
            {
                ioAddress = value;
                OnPropertyChanged("IOAddress");
            }
        }

        public TagType Type
        {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
