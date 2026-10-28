using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Components
{
    public class ScanInfo
    {
        public ScanInfo(IMasterScan scan, string description, TimeSpan period, string details, byte classes)
        {
            this.scan = scan;
            this.description = description;
            this.period = period;
            this.details = details;
            this.classes = classes;
        }

        public readonly IMasterScan scan;
        public readonly byte classes;
        public readonly string description;
        public readonly TimeSpan period;
        public readonly string details;
    }
}
