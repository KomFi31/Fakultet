/*
 * Predstavlja centralnu logiku Data Concentrator komponente.
 * Upravlja tagovima i predstavlja vezu izmedju baze podataka,
 * PLC simulatora i SCADA korisnickog interfejsa.
 */

using DataConcentrator.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

namespace DataConcentrator
{
    public class DataConcentratorManager
    {
        private static DataConcentratorManager instance;

        private Thread scanThread;
        private volatile bool scanRunning;

        // Obavestava GUI da se alarm sa prosledjenim ID-em aktivirao.
        public event Action<int> AlarmActivated;

        private readonly object scanLock = new object();

        private readonly List<Tag> scanTags = new List<Tag>();

        private readonly Dictionary<string, DateTime> lastScanTimes =
            new Dictionary<string, DateTime>();

        private readonly object dbLock = new object();

        private readonly object historyLock = new object();

        private readonly Dictionary<string, List<AnalogValueRecord>> analogHistory =
            new Dictionary<string, List<AnalogValueRecord>>();

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
            lock (dbLock)
            {
                return ContextClass.Instance.Tags.ToList();
            }
        }

        public Tag GetTag(string name)
        {
            lock (dbLock)
            {
                return ContextClass.Instance.Tags
                    .FirstOrDefault(tag => tag.Name == name);
            }
        }

        public bool AddTag(Tag tag)
        {
            if (tag == null)
                return false;

            if (GetTag(tag.Name) != null)
                return false;

            SetTagType(tag);

            lock (dbLock)
            {
                ContextClass.Instance.Tags.Add(tag);
                ContextClass.Instance.SaveChanges();
            }

            if (scanRunning && (tag is AnalogInput || tag is DigitalInput))
            {
                lock (scanLock)
                {
                    scanTags.Add(tag);
                    lastScanTimes[tag.Name] = DateTime.MinValue;
                }
            }

            return true;
        }

        public bool RemoveTag(string name)
        {
            Tag tag = GetTag(name);

            if (tag == null)
                return false;

            lock (scanLock)
            {
                scanTags.RemoveAll(t => t.Name == name);
                lastScanTimes.Remove(name);
            }

            lock (historyLock)
            {
                analogHistory.Remove(name);
            }

            // Brisanjem taga uklanjaju se i alarmi vezani za njega.
            List<Alarm> tagAlarms = GetAlarmsForTag(name);

            lock (dbLock)
            {
                ContextClass.Instance.Alarms.RemoveRange(tagAlarms);
                ContextClass.Instance.Tags.Remove(tag);

                ContextClass.Instance.SaveChanges();
            }

            return true;
        }
        public bool UpdateTag(Tag updatedTag)
        {
            if (updatedTag == null)
                return false;

            Tag existingTag = GetTag(updatedTag.Name);

            if (existingTag == null)
                return false;

            // Nije dozvoljena promena vrste taga tokom update-a.
            if (existingTag.GetType() != updatedTag.GetType())
                return false;

            lock (dbLock)
            {
                // Zajednicka svojstva svih tagova.
                existingTag.Description = updatedTag.Description;
                existingTag.IOAddress = updatedTag.IOAddress;

                if (existingTag is AnalogInput existingAI &&
                    updatedTag is AnalogInput updatedAI)
                {
                    existingAI.ScanTime = updatedAI.ScanTime;
                    existingAI.OnScan = updatedAI.OnScan;
                    existingAI.LowLimit = updatedAI.LowLimit;
                    existingAI.HighLimit = updatedAI.HighLimit;
                    existingAI.Units = updatedAI.Units;
                    existingAI.Deadband = updatedAI.Deadband;
                    existingAI.Hysteresis = updatedAI.Hysteresis;
                }
                else if (existingTag is AnalogOutput existingAO &&
                         updatedTag is AnalogOutput updatedAO)
                {
                    existingAO.LowLimit = updatedAO.LowLimit;
                    existingAO.HighLimit = updatedAO.HighLimit;
                    existingAO.Units = updatedAO.Units;
                    existingAO.InitialValue = updatedAO.InitialValue;
                }
                else if (existingTag is DigitalInput existingDI &&
                         updatedTag is DigitalInput updatedDI)
                {
                    existingDI.ScanTime = updatedDI.ScanTime;
                    existingDI.OnScan = updatedDI.OnScan;
                }
                else if (existingTag is DigitalOutput existingDO &&
                         updatedTag is DigitalOutput updatedDO)
                {
                    existingDO.InitialValue = updatedDO.InitialValue;
                }
                lock (dbLock)
                {
                    ContextClass.Instance.SaveChanges();
                }
            }

            return true;
        }

        private void SetTagType(Tag tag)
        {
            if (tag is AnalogInput)
                tag.Type = TagType.AI;
            else if (tag is AnalogOutput)
                tag.Type = TagType.AO;
            else if (tag is DigitalInput)
                tag.Type = TagType.DI;
            else if (tag is DigitalOutput)
                tag.Type = TagType.DO;
        }

        #endregion

        #region Alarm Management

        public List<Alarm> GetAllAlarms()
        {
            lock (dbLock)
            {
                return ContextClass.Instance.Alarms.ToList();
            }
        }

        public Alarm GetAlarm(int id)
        {
            lock (dbLock)
            {
                return ContextClass.Instance.Alarms
                .FirstOrDefault(alarm => alarm.Id == id);
            }
        }

        public List<Alarm> GetAlarmsForTag(string tagName)
        {
            lock (dbLock)
            {
                return ContextClass.Instance.Alarms
                .Where(alarm => alarm.TagName == tagName)
                .ToList();
            }
        }

        public bool AddAlarm(Alarm alarm)
        {
            if (alarm == null)
                return false;

            // Alarm moze biti vezan iskljucivo za analogni ulazni tag.
            Tag tag = GetTag(alarm.TagName);

            if (!(tag is AnalogInput))
                return false;

            lock (dbLock)
            {
                alarm.State = AlarmState.Inactive;

                ContextClass.Instance.Alarms.Add(alarm);
                ContextClass.Instance.SaveChanges();
            }

            return true;
        }

        public bool RemoveAlarm(int id)
        {
            Alarm alarm = GetAlarm(id);

            if (alarm == null)
                return false;

            lock (dbLock)
            {
                ContextClass.Instance.Alarms.Remove(alarm);
                ContextClass.Instance.SaveChanges();
            }

            return true;
        }

        public bool AcknowledgeAlarm(int id)
        {
            Alarm alarm = GetAlarm(id);

            if (alarm == null || alarm.State != AlarmState.Active)
                return false;

            lock (dbLock)
            {
                alarm.State = AlarmState.Acknowledged;

                ContextClass.Instance.SaveChanges();
            }

            return true;
        }

        // Metode ce biti korisna kod rada sa GUI
        public List<ActivatedAlarm> GetActivatedAlarmsForTag(string tagName)
        {
            lock (dbLock)
            {
                return ContextClass.Instance.ActivatedAlarms
                    .Where(alarm => alarm.TagName == tagName)
                    .ToList();
            }
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

        #region Input Scanning

        public bool SetInputScan(string tagName, bool enabled)
        {
            Tag tag = GetTag(tagName);

            if (tag == null)
                return false;

            lock (dbLock)
            {
                if (tag is AnalogInput analogInput)
                {
                    analogInput.OnScan = enabled;
                }
                else if (tag is DigitalInput digitalInput)
                {
                    digitalInput.OnScan = enabled;
                }
                else
                {
                    // Output tagovi nemaju scan.
                    return false;
                }

                ContextClass.Instance.SaveChanges();
            }

            return true;
        }

        public void StartScanning()
        {
            if (scanRunning)
                return;

            lock (scanLock)
            {
                scanTags.Clear();
                lastScanTimes.Clear();

                // Skeniraju se samo ulazni tagovi.
                scanTags.AddRange(
                    GetAllTags()
                        .Where(tag => tag is AnalogInput || tag is DigitalInput)
                );

                foreach (Tag tag in scanTags)
                {
                    lastScanTimes[tag.Name] = DateTime.MinValue;
                }
            }

            scanRunning = true;

            scanThread = new Thread(ScanLoop);
            scanThread.IsBackground = true;
            scanThread.Start();
        }

        public void StopScanning()
        {
            scanRunning = false;

            if (scanThread != null && scanThread.IsAlive)
            {
                scanThread.Join(1000);
            }

            scanThread = null;
        }

        private void ScanLoop()
        {
            while (scanRunning)
            {
                Tag[] tags;

                // Pravimo kopiju kako dodavanje/brisanje taga
                // ne bi menjalo kolekciju tokom foreach petlje.
                lock (scanLock)
                {
                    tags = scanTags.ToArray();
                }

                foreach (Tag tag in tags)
                {
                    if (tag is AnalogInput analogInput)
                    {
                        ScanAnalogInput(analogInput);
                    }
                    else if (tag is DigitalInput digitalInput)
                    {
                        ScanDigitalInput(digitalInput);
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void ScanAnalogInput(AnalogInput tag)
        {
            if (!tag.OnScan)
                return;

            if (!IsScanTimeReached(tag.Name, tag.ScanTime))
                return;

            double newValue =
                PLC.Instance.ReadAnalogInputValue(tag.IOAddress);

            // Svako uspesno ocitavanje AI taga cuva se u istoriji.
            AddAnalogHistoryValue(tag.Name, newValue);

            // Prvo ocitavanje se uvek prihvata.
            if (!tag.CurrentValue.HasValue)
            {
                tag.CurrentValue = newValue;
                CheckAlarms(tag);
                return;
            }

            // Promena se prihvata samo ako je veca ili jednaka deadband-u.
            if (Math.Abs(newValue - tag.CurrentValue.Value) >= tag.Deadband)
            {
                tag.CurrentValue = newValue;

                CheckAlarms(tag);
            }
        }

        private void ScanDigitalInput(DigitalInput tag)
        {
            if (!tag.OnScan)
                return;

            if (!IsScanTimeReached(tag.Name, tag.ScanTime))
                return;

            bool newValue =
                PLC.Instance.ReadDigitalInputValue(tag.IOAddress);

            if (!tag.CurrentValue.HasValue ||
                tag.CurrentValue.Value != newValue)
            {
                tag.CurrentValue = newValue;
            }
        }

        private bool IsScanTimeReached(string tagName, double scanTime)
        {
            if (scanTime <= 0)
                return false;

            DateTime now = DateTime.UtcNow;

            lock (scanLock)
            {
                if (!lastScanTimes.ContainsKey(tagName))
                {
                    lastScanTimes[tagName] = DateTime.MinValue;
                }

                double elapsed =
                    (now - lastScanTimes[tagName]).TotalSeconds;

                if (elapsed < scanTime)
                    return false;

                lastScanTimes[tagName] = now;

                return true;
            }
        }

        #endregion

        #region Alarm Processing
        private void CheckAlarms(AnalogInput tag)
        {
            if (!tag.CurrentValue.HasValue)
                return;

            double value = tag.CurrentValue.Value;

            List<Alarm> alarms = GetAlarmsForTag(tag.Name);

            foreach (Alarm alarm in alarms)
            {
                bool shouldActivate = false;
                bool shouldDeactivate = false;

                if (alarm.Condition == AlarmCondition.Above)
                {
                    // Alarm se aktivira kada vrednost predje iznad granice.
                    shouldActivate = value > alarm.Limit;

                    // Aktivni alarm se gasi tek kada vrednost padne
                    // ispod granice umanjene za hysteresis.
                    shouldDeactivate =
                        value <= alarm.Limit - tag.Hysteresis;
                }
                else if (alarm.Condition == AlarmCondition.Below)
                {
                    // Alarm se aktivira kada vrednost padne ispod granice.
                    shouldActivate = value < alarm.Limit;

                    // Aktivni alarm se gasi tek kada vrednost poraste
                    // iznad granice uvecane za hysteresis.
                    shouldDeactivate =
                        value >= alarm.Limit + tag.Hysteresis;
                }

                if (alarm.State == AlarmState.Inactive && shouldActivate)
                {
                    ActivateAlarm(alarm);
                }
                else if (alarm.State != AlarmState.Inactive && shouldDeactivate)
                {
                    alarm.State = AlarmState.Inactive;

                    lock (dbLock)
                    {
                        ContextClass.Instance.SaveChanges();
                    }
                }
            }
        }

        private void ActivateAlarm(Alarm alarm)
        {
            alarm.State = AlarmState.Active;

            ActivatedAlarm activatedAlarm = new ActivatedAlarm
            {
                AlarmId = alarm.Id,
                TagName = alarm.TagName,
                Message = alarm.Message,
                TimeStamp = DateTime.Now
            };

            lock (dbLock)
            {
                ContextClass.Instance.ActivatedAlarms.Add(activatedAlarm);
                ContextClass.Instance.SaveChanges();
            }

            // GUI dobija informaciju koji alarm se aktivirao.
            AlarmActivated?.Invoke(alarm.Id);
        }

        #endregion

        #region Analog History

        public List<AnalogValueRecord> GetAnalogHistory(string tagName)
        {
            lock (historyLock)
            {
                if (!analogHistory.ContainsKey(tagName))
                    return new List<AnalogValueRecord>();

                // Vraca se kopija liste da GUI ne menja originalnu kolekciju.
                return analogHistory[tagName].ToList();
            }
        }

        private void AddAnalogHistoryValue(
            string tagName,
            double value)
        {
            lock (historyLock)
            {
                if (!analogHistory.ContainsKey(tagName))
                {
                    analogHistory[tagName] =
                        new List<AnalogValueRecord>();
                }

                analogHistory[tagName].Add(
                    new AnalogValueRecord
                    {
                        TimeStamp = DateTime.Now,
                        Value = value
                    });
            }
        }

        #endregion

    }
}