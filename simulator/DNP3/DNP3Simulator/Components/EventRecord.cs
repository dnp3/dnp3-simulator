using System;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Components
{
    public class EventRecord
    {
        public EventRecord(ushort index, PointClass clazz)
        {
            this.index = index;
            this.clazz = clazz;
        }

        public ushort index;
        public PointClass clazz;
    }
}
