using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Components
{
    public partial class StaticResponseTypeControl : ResponseTypeControl
    {
        public StaticResponseTypeControl()
        {
            InitializeComponent();

            this.ComboBoxBinary.DataSource = Enum.GetValues(typeof(StaticBinaryVariation));
            this.ComboBoxDoubleBinary.DataSource = Enum.GetValues(typeof(StaticDoubleBinaryVariation));
            this.ComboBoxCounter.DataSource = Enum.GetValues(typeof(StaticCounterVariation));
            this.ComboBoxFrozenCounter.DataSource = Enum.GetValues(typeof(StaticFrozenCounterVariation));
            this.ComboBoxAnalog.DataSource = Enum.GetValues(typeof(StaticAnalogVariation));
            this.ComboBoxBinaryOutputStatus.DataSource = Enum.GetValues(typeof(StaticBinaryOutputStatusVariation));
            this.ComboBoxAnalogOutputStatus.DataSource = Enum.GetValues(typeof(StaticAnalogOutputStatusVariation));
        }

        public void LoadTemplate(DatabaseTemplate template)
        {
            SelectVariation(ComboBoxBinary, template.binary.Values.Select(r => r.staticVariation));
            SelectVariation(ComboBoxDoubleBinary, template.doubleBinary.Values.Select(r => r.staticVariation));
            SelectVariation(ComboBoxCounter, template.counter.Values.Select(r => r.staticVariation));
            SelectVariation(ComboBoxFrozenCounter, template.frozenCounter.Values.Select(r => r.staticVariation));
            SelectVariation(ComboBoxAnalog, template.analog.Values.Select(r => r.staticVariation));
            SelectVariation(ComboBoxBinaryOutputStatus, template.binaryOutputStatus.Values.Select(r => r.staticVariation));
            SelectVariation(ComboBoxAnalogOutputStatus, template.analogOutputStatus.Values.Select(r => r.staticVariation));
        }

        public void Configure(DatabaseTemplate template)
        {
            if (ComboBoxBinary.SelectedItem != null)
                foreach (var record in template.binary.Values) record.staticVariation = (StaticBinaryVariation)ComboBoxBinary.SelectedItem;
            if (ComboBoxBinaryOutputStatus.SelectedItem != null)
                foreach (var record in template.binaryOutputStatus.Values) record.staticVariation = (StaticBinaryOutputStatusVariation)ComboBoxBinaryOutputStatus.SelectedItem;
            if (ComboBoxDoubleBinary.SelectedItem != null)
                foreach (var record in template.doubleBinary.Values) record.staticVariation = (StaticDoubleBinaryVariation)ComboBoxDoubleBinary.SelectedItem;
            if (ComboBoxCounter.SelectedItem != null)
                foreach (var record in template.counter.Values) record.staticVariation = (StaticCounterVariation)ComboBoxCounter.SelectedItem;
            if (ComboBoxFrozenCounter.SelectedItem != null)
                foreach (var record in template.frozenCounter.Values) record.staticVariation = (StaticFrozenCounterVariation)ComboBoxFrozenCounter.SelectedItem;
            if (ComboBoxAnalog.SelectedItem != null)
                foreach (var record in template.analog.Values) record.staticVariation = (StaticAnalogVariation)ComboBoxAnalog.SelectedItem;
            if (ComboBoxAnalogOutputStatus.SelectedItem != null)
                foreach (var record in template.analogOutputStatus.Values) record.staticVariation = (StaticAnalogOutputStatusVariation)ComboBoxAnalogOutputStatus.SelectedItem;
        }
    }
}
