using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons
{
    public class ListviewDatabaseAdapter : IDatabase
    {
        readonly ListBox listBox;

        ListviewDatabaseAdapter(ListBox listBox)
        {
            this.listBox = listBox;
        }

        public static void Process(IChangeSet changes, ListBox listBox)
        {
            IDatabase adapter = new ListviewDatabaseAdapter(listBox);

            listBox.SuspendLayout();
            changes.Apply(adapter);
            listBox.ResumeLayout();
        }        

        void Add(Measurement meas, string label)
        {
            var text = string.Format("{0} ({1}) - {2} - {3}", label, meas.Index, meas.Value, meas.ShortFlags);        
            listBox.Items.Add(text);            
        }

        void IDatabase.Update(Binary update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "Binary");
        }

        void IDatabase.Update(DoubleBitBinary update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "DoubleBitBinary");
        }

        void IDatabase.Update(Analog update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "Analog");
        }

        void IDatabase.Update(Counter update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "Counter");
        }

        void IDatabase.FreezeCounter(ushort index, bool clear, EventMode mode)
        {
        }

        void IDatabase.Update(BinaryOutputStatus update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "BinaryOutputStatus");
        }

        void IDatabase.Update(AnalogOutputStatus update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "AnalogOutputStatus");
        }

        void IDatabase.Update(OctetString update, ushort index, EventMode mode)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "OctetString");
        }

        void IDatabase.Update(TimeAndInterval update, ushort index)
        {
            this.Add(update.ToMeasurement(index, TimestampQuality.SYNCHRONIZED), "TimeAndInterval");
        }         
    }
}
