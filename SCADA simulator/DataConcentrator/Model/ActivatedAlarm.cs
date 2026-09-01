using System;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator.Model
{
    public class ActivatedAlarm // Klasa prakticno vraca povratnu informaciju o alarmu koji se aktivira detaljno
    {
        [Key]

        // Ne treba pomesati sa AlarmID koji predstavlja ID alarma koji se aktivirao
        // Dok ActivatedAlarmId predstavlja ID notifikacije za aktiviran alarm
        public int ActivatedAlarmId { get; set; }
                                                  

        // ID alarma koji se aktivirao
        public int AlarmId { get; set; }

        public string TagName { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}