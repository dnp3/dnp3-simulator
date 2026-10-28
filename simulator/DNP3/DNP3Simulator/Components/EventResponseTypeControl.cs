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
    public partial class EventResponseTypeControl : ResponseTypeControl
    {
        public EventResponseTypeControl()
        {
            InitializeComponent();

            this.ComboBoxBinary.DataSource = Enum.GetValues(typeof(EventBinaryVariation));
            this.ComboBoxDoubleBinary.DataSource = Enum.GetValues(typeof(EventDoubleBinaryVariation));
            this.ComboBoxCounter.DataSource = Enum.GetValues(typeof(EventCounterVariation));
            this.ComboBoxFrozenCounter.DataSource = Enum.GetValues(typeof(EventFrozenCounterVariation));
            this.ComboBoxAnalog.DataSource = Enum.GetValues(typeof(EventAnalogVariation));
            this.ComboBoxBinaryOutputStatus.DataSource = Enum.GetValues(typeof(EventBinaryOutputStatusVariation));
            this.ComboBoxAnalogOutputStatus.DataSource = Enum.GetValues(typeof(EventAnalogOutputStatusVariation));
        }

        public void LoadTemplate(DatabaseTemplate template)
        {
            SelectVariation(ComboBoxBinary, template.binary.Values.Select(r => r.eventVariation));
            SelectVariation(ComboBoxDoubleBinary, template.doubleBinary.Values.Select(r => r.eventVariation));
            SelectVariation(ComboBoxCounter, template.counter.Values.Select(r => r.eventVariation));
            SelectVariation(ComboBoxFrozenCounter, template.frozenCounter.Values.Select(r => r.eventVariation));
            SelectVariation(ComboBoxAnalog, template.analog.Values.Select(r => r.eventVariation));
            SelectVariation(ComboBoxBinaryOutputStatus, template.binaryOutputStatus.Values.Select(r => r.eventVariation));
            SelectVariation(ComboBoxAnalogOutputStatus, template.analogOutputStatus.Values.Select(r => r.eventVariation));
        }

        public void Configure(DatabaseTemplate template)
        {
            if (ComboBoxBinary.SelectedItem != null)
                foreach (var record in template.binary.Values) record.eventVariation = (EventBinaryVariation)ComboBoxBinary.SelectedItem;
            if (ComboBoxBinaryOutputStatus.SelectedItem != null)
                foreach (var record in template.binaryOutputStatus.Values) record.eventVariation = (EventBinaryOutputStatusVariation)ComboBoxBinaryOutputStatus.SelectedItem;
            if (ComboBoxDoubleBinary.SelectedItem != null)
                foreach (var record in template.doubleBinary.Values) record.eventVariation = (EventDoubleBinaryVariation)ComboBoxDoubleBinary.SelectedItem;
            if (ComboBoxCounter.SelectedItem != null)
                foreach (var record in template.counter.Values) record.eventVariation = (EventCounterVariation)ComboBoxCounter.SelectedItem;
            if (ComboBoxFrozenCounter.SelectedItem != null)
                foreach (var record in template.frozenCounter.Values) record.eventVariation = (EventFrozenCounterVariation)ComboBoxFrozenCounter.SelectedItem;
            if (ComboBoxAnalog.SelectedItem != null)
                foreach (var record in template.analog.Values) record.eventVariation = (EventAnalogVariation)ComboBoxAnalog.SelectedItem;
            if (ComboBoxAnalogOutputStatus.SelectedItem != null)
                foreach (var record in template.analogOutputStatus.Values) record.eventVariation = (EventAnalogOutputStatusVariation)ComboBoxAnalogOutputStatus.SelectedItem;
        }
    }
}
