/*
 * Predstavlja centralnu logiku Data Concentrator komponente.
 * Upravlja tagovima i predstavlja vezu izmedju baze podataka,
 * PLC simulatora i SCADA korisnickog interfejsa.
 */

using DataConcentrator.Model;
using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    public class DataConcentratorManager
    {
        private static DataConcentratorManager instance;

        public static DataConcentratorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataConcentratorManager();
                }

                return instance;
            }
        }

        private DataConcentratorManager()
        {
        }
        #region Tag Management
        public List<Tag> GetAllTags()
        {
            return ContextClass.Instance.Tags.ToList();
        }

        public Tag GetTag(string name)
        {
            return ContextClass.Instance.Tags
                .FirstOrDefault(tag => tag.Name == name);
        }

        public bool AddTag(Tag tag)
        {
            if (tag == null)
                return false;

            if (GetTag(tag.Name) != null)
                return false;

            ContextClass.Instance.Tags.Add(tag);
            ContextClass.Instance.SaveChanges();

            return true;
        }

        public bool RemoveTag(string name)
        {
            Tag tag = GetTag(name);

            if (tag == null)
                return false;

            // Brisanjem taga uklanjaju se i alarmi vezani za njega.
            List<Alarm> tagAlarms = GetAlarmsForTag(name);

            ContextClass.Instance.Alarms.RemoveRange(tagAlarms);
            ContextClass.Instance.Tags.Remove(tag);

            ContextClass.Instance.SaveChanges();

            return true;
        }

        #endregion

        #region Alarm Management

        public List<Alarm> GetAllAlarms()
        {
            return ContextClass.Instance.Alarms.ToList();
        }

        public Alarm GetAlarm(int id)
        {
            return ContextClass.Instance.Alarms
                .FirstOrDefault(alarm => alarm.Id == id);
        }

        public List<Alarm> GetAlarmsForTag(string tagName)
        {
            return ContextClass.Instance.Alarms
                .Where(alarm => alarm.TagName == tagName)
                .ToList();
        }

        public bool AddAlarm(Alarm alarm)
        {
            if (alarm == null)
                return false;

            // Alarm moze biti vezan iskljucivo za analogni ulazni tag.
            Tag tag = GetTag(alarm.TagName);

            if (!(tag is AnalogInput))
                return false;

            alarm.State = AlarmState.Inactive;

            ContextClass.Instance.Alarms.Add(alarm);
            ContextClass.Instance.SaveChanges();

            return true;
        }

        public bool RemoveAlarm(int id)
        {
            Alarm alarm = GetAlarm(id);

            if (alarm == null)
                return false;

            ContextClass.Instance.Alarms.Remove(alarm);
            ContextClass.Instance.SaveChanges();

            return true;
        }

        public bool AcknowledgeAlarm(int id)
        {
            Alarm alarm = GetAlarm(id);

            if (alarm == null || alarm.State != AlarmState.Active)
                return false;

            alarm.State = AlarmState.Acknowledged;
            ContextClass.Instance.SaveChanges();

            return true;
        }

        #endregion

        #region PLC Read/Write

        // Pokretanje i zaustavljanje PLC simulatora.
        public void StartPLCSimulator()
        {
            PLC.Instance.StartSimulator();
        }

        public void StopPLCSimulator()
        {
            PLC.Instance.StopSimulator();
        }

        // Citanje vrednosti dozvoljeno je samo za analogne ulazne tagove.
        public double? ReadAnalogInput(string tagName)
        {
            AnalogInput tag = GetTag(tagName) as AnalogInput;

            if (tag == null)
                return null;

            return PLC.Instance.ReadAnalogInputValue(tag.IOAddress);
        }

        // Citanje vrednosti dozvoljeno je samo za digitalne ulazne tagove.
        public bool? ReadDigitalInput(string tagName)
        {
            DigitalInput tag = GetTag(tagName) as DigitalInput;

            if (tag == null)
                return null;

            return PLC.Instance.ReadDigitalInputValue(tag.IOAddress);
        }

        // Analogna izlazna vrednost mora biti unutar definisanih granica taga.
        public bool WriteAnalogOutput(string tagName, double value)
        {
            AnalogOutput tag = GetTag(tagName) as AnalogOutput;

            if (tag == null)
                return false;

            if (value < tag.LowLimit || value > tag.HighLimit)
                return false;

            PLC.Instance.WriteAnalogOutputValue(tag.IOAddress, value);

            return true;
        }

        // Digitalni izlaz moze imati samo stanje true ili false.
        public bool WriteDigitalOutput(string tagName, bool value)
        {
            DigitalOutput tag = GetTag(tagName) as DigitalOutput;

            if (tag == null)
                return false;

            PLC.Instance.WriteDigitalOutputValue(tag.IOAddress, value);

            return true;
        }

        #endregion
    }
}