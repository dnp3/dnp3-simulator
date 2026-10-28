using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons
{
    public static class MeasActions
    {
        public static Action<IDatabase> GetBinaryAction(bool value, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.Update(new Binary(value, new Flags(quality), new DNPTime(timestamp)), index, EventMode.Detect);
            };
        }

        public static Action<IDatabase> GetDoubleBinaryAction(DoubleBit bits, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.Update(new DoubleBitBinary(bits, new Flags(quality), new DNPTime(timestamp)), index, EventMode.Detect);
            };
        }

        public static Action<IDatabase> GetBinaryOutputStatusAction(bool value, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.Update(new BinaryOutputStatus(value, new Flags(quality), new DNPTime(timestamp)), index, EventMode.Detect);
            };
        }

        public static Action<IDatabase> GetAnalogAction(double value, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.Update(new Analog(value, new Flags(quality), new DNPTime(timestamp)), index, EventMode.Detect);
            };
        }

        public static Action<IDatabase> GetAnalogOutputStatusAction(double value, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.Update(new AnalogOutputStatus(value, new Flags(quality), new DNPTime(timestamp)), index, EventMode.Detect);
            };
        }

        public static Action<IDatabase> GetCounterAction(uint value, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.Update(new Counter(value, new Flags(quality), new DNPTime(timestamp)), index, EventMode.Detect);
            };
        }

        public static Action<IDatabase> GetFrozenCounterAction(uint value, byte quality, DateTime timestamp, ushort index)
        {
            return (IDatabase db) =>
            {
                db.FreezeCounter(index, false, EventMode.Detect);
            };
        }
    }
}
