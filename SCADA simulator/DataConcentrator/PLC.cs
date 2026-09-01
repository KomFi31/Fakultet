using PLCSimulator;

namespace DataConcentrator
{
    /*
     * Predstavlja posrednicku klasu izmedju Data Concentrator-a i PLC simulatora.
     * Omogucava citanje ulaznih i upis izlaznih vrednosti bez direktnog pristupa simulatoru.
     */
    public class PLC
    {
        private static PLC instance;
        private PLCSimulatorManager plcSimulator;

        public static PLC Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PLC();
                }

                return instance;
            }
        }

        private PLC()
        {
            plcSimulator = new PLCSimulatorManager();
        }

        public void StartSimulator()
        {
            plcSimulator.StartPLCSimulator();
        }

        public void StopSimulator()
        {
            plcSimulator.Abort();
        }

        public double ReadAnalogInputValue(string address)
        {
            return plcSimulator.GetAnalogValue(address);
        }

        public bool ReadDigitalInputValue(string address)
        {
            return plcSimulator.GetDigitalValue(address) == 1;
        }

        public void WriteAnalogOutputValue(string address, double value)
        {
            plcSimulator.SetAnalogValue(address, value);
        }

        public void WriteDigitalOutputValue(string address, bool value)
        {
            plcSimulator.SetDigitalValue(address, value ? 1 : 0);
        }
    }
}