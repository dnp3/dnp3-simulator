using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automatak.Simulator.DNP3.Components
{
    public partial class ResponseTypeControl : UserControl
    {
        public ResponseTypeControl()
        {
            InitializeComponent();            
        }

        protected static void SelectVariation<T>(ComboBox control, IEnumerable<T> variations)
        {
            var values = variations.Distinct().ToList();
            // A blank selection represents mixed per-point variations; preserve them on Apply.
            control.SelectedItem = values.Count == 1 ? (object)values[0] : null;
        }

        protected ComboBox ComboBoxBinary
        {
            get
            {
                return comboBoxBinary;
            }
        }

        protected ComboBox ComboBoxDoubleBinary
        {
            get
            {
                return comboBoxDoubleBinary;
            }
        }

        protected ComboBox ComboBoxCounter
        {
            get
            {
                return comboBoxCounter;
            }
        }

        protected ComboBox ComboBoxFrozenCounter
        {
            get
            {
                return comboBoxFrozenCounter;
            }
        }

        protected ComboBox ComboBoxAnalog
        {
            get
            {
                return comboBoxAnalog;
            }
        }

        protected ComboBox ComboBoxBinaryOutputStatus
        {
            get
            {
                return comboBoxBinaryOutputStatus;
            }
        }

        protected ComboBox ComboBoxAnalogOutputStatus
        {
            get
            {
                return comboBoxAnalogOutputStatus;
            }
        }
    }
}
