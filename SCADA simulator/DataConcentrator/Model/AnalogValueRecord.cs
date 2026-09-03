/*
 * Predstavlja jedan istorijski uzorak analogne ulazne vrednosti.
 * Cuva vrednost AI taga i vremenski trenutak kada je ocitana.
 */

using System;

namespace DataConcentrator.Model
{
    public class AnalogValueRecord
    {
        public DateTime TimeStamp { get; set; }

        public double Value { get; set; }
    }
}