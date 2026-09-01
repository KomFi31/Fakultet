using System;
using System.Collections.Generic;
using System.Threading;

namespace PLCSimulator
{
    /*
     * Upravlja simulacijom PLC uredjaja.
     * Generise i cuva trenutne vrednosti analognih i digitalnih adresa
     * i omogucava thread-safe citanje i pisanje tih vrednosti.
     */
    public class PLCSimulatorManager
    {
        private Dictionary<string, double> addressValues;
        private object locker = new object();

        private Thread t1;
        private Thread t2;

        private Random random = new Random();

        public PLCSimulatorManager()
        {
            addressValues = new Dictionary<string, double>();

            // Analog inputs
            addressValues.Add("ADDR001", 0);
            addressValues.Add("ADDR002", 0);
            addressValues.Add("ADDR003", 0);
            addressValues.Add("ADDR004", 0);

            // Analog outputs
            addressValues.Add("ADDR005", 0);
            addressValues.Add("ADDR006", 0);
            addressValues.Add("ADDR007", 0);
            addressValues.Add("ADDR008", 0);

            // Digital inputs
            addressValues.Add("ADDR009", 0);
            addressValues.Add("ADDR011", 0);
            addressValues.Add("ADDR012", 0);
            addressValues.Add("ADDR013", 0);

            // Digital outputs
            addressValues.Add("ADDR010", 0);
            addressValues.Add("ADDR014", 0);
            addressValues.Add("ADDR015", 0);
            addressValues.Add("ADDR016", 0);
        }

        public void StartPLCSimulator()
        {
            t1 = new Thread(GeneratingAnalogInputs);
            t1.Start();

            t2 = new Thread(GeneratingDigitalInputs);
            t2.Start();
        }

        private void GeneratingAnalogInputs()
        {
            while (true)
            {
                Thread.Sleep(100);

                lock (locker)
                {
                    addressValues["ADDR001"] =
                        100 * Math.Sin((double)DateTime.Now.Second / 60 * Math.PI);

                    addressValues["ADDR002"] =
                        100 * DateTime.Now.Second / 60;

                    addressValues["ADDR003"] =
                        50 * Math.Cos((double)DateTime.Now.Second / 60 * Math.PI);

                    addressValues["ADDR004"] =
                        RandomNumberBetween(0, 50);
                }
            }
        }

        private void GeneratingDigitalInputs()
        {
            while (true)
            {
                Thread.Sleep(1000);

                lock (locker)
                {
                    ToggleDigitalValue("ADDR009");
                    ToggleDigitalValue("ADDR011");
                    ToggleDigitalValue("ADDR012");
                    ToggleDigitalValue("ADDR013");
                }
            }
        }

        private void ToggleDigitalValue(string address)
        {
            if (addressValues[address] == 0)
                addressValues[address] = 1;
            else
                addressValues[address] = 0;
        }

        public double GetAnalogValue(string address)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                    return addressValues[address];

                return -1;
            }
        }

        public double GetDigitalValue(string address)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                    return addressValues[address];

                return -1;
            }
        }

        public void SetAnalogValue(string address, double value)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                    addressValues[address] = value;
            }
        }

        public void SetDigitalValue(string address, double value)
        {
            lock (locker)
            {
                if (addressValues.ContainsKey(address))
                    addressValues[address] = value;
            }
        }

        private double RandomNumberBetween(double minValue, double maxValue)
        {
            return minValue +
                   random.NextDouble() * (maxValue - minValue);
        }

        public void Abort()
        {
            t1?.Abort();
            t2?.Abort();
        }
    }
}