using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Automatak.DNP3.Interface;
using Automatak.Simulator.DNP3.API;

namespace Automatak.Simulator.DNP3.Components
{
    partial class OutstationDialog : Form
    {
        readonly IDNP3Config config;
        readonly bool allowTemplateEdit;
        readonly OutstationStackConfig initialConfig;

        public OutstationDialog(IDNP3Config config, IOutstationModule module)
        {
            InitializeComponent();

            this.config = config;
            this.initialConfig = module.DefaultConfig;
            this.allowTemplateEdit = module.AllowTemplateEditing;
            this.textBoxID.Text = module.DefaultLogName;

            this.linkConfigControl.Configuration = initialConfig.link;            

            if (allowTemplateEdit)
            {
                comboBoxTemplate.DataSource = config.Templates.Select(kv => kv.Key).ToList();
            }
            else
            {
                groupBoxDatabase.Enabled = false;   
                staticResponseTypeControl1.LoadTemplate(initialConfig.databaseTemplate);
                eventResponseTypeControl1.LoadTemplate(initialConfig.databaseTemplate);
            }
        }

        public OutstationDialog(IDNP3Config config, IOutstationModule module, string alias, OutstationStackConfig settings)
            : this(config, module)
        {
            Text = "Outstation settings";
            buttonAdd.Text = "Apply";
            initialConfig = ConfigurationXml.Clone(settings);
            textBoxID.Text = alias;
            linkConfigControl.Configuration = settings.link;
            var parameters = settings.outstation.config;
            checkBoxEnableUnsol.Checked = parameters.allowUnsolicited;
            numericUpDownMaxControls.Value = parameters.maxControlsPerRequest;
            numericUpDownMaxTxFrag.Value = parameters.maxTxFragSize;
            numericUpDownSelectTimeout.Value = (decimal)parameters.selectTimeout.TotalMilliseconds;
            numericUpDownUnsolConfirmTimeout.Value = (decimal)parameters.unsolConfirmTimeout.TotalMilliseconds;
            numericUpDownSolConfirmTimeout.Value = (decimal)parameters.solicitedConfirmTimeout.TotalMilliseconds;
            eventBufferConfigControl1.Configuration = settings.outstation.buffer;
            staticResponseTypeControl1.LoadTemplate(settings.databaseTemplate);
            eventResponseTypeControl1.LoadTemplate(settings.databaseTemplate);
            if (allowTemplateEdit)
            {
                // Work on a private copy so Cancel cannot alter the running database/template.
                var templateName = alias + " (current database)";
                while (config.ContainsTemplateId(templateName)) templateName += "_";
                config.AddTemplate(templateName, ConfigurationXml.Clone(settings.databaseTemplate));
                comboBoxTemplate.DataSource = config.Templates.Select(kv => kv.Key).ToList();
                comboBoxTemplate.SelectedItem = templateName;
            }
        }

        private void buttonADD_Click(object sender, EventArgs e)
        {            
            if (string.IsNullOrWhiteSpace(textBoxID.Text))
            {
                MessageBox.Show(this, "Specify an outstation alias.", "Outstation settings");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public String SelectedAlias
        {
            get
            {
                return textBoxID.Text;
            }
        }

        OutstationParams OutstationParameters
        {
            get
            {
                OutstationParams ret = ConfigurationXml.Clone(initialConfig.outstation.config);
                ret.allowUnsolicited = this.checkBoxEnableUnsol.Checked;
                ret.maxControlsPerRequest = Decimal.ToByte(this.numericUpDownMaxControls.Value);
                ret.maxTxFragSize = Decimal.ToUInt16(this.numericUpDownMaxTxFrag.Value);
                ret.selectTimeout = TimeSpan.FromMilliseconds(Decimal.ToDouble(this.numericUpDownSelectTimeout.Value));
                ret.unsolConfirmTimeout = TimeSpan.FromMilliseconds(Decimal.ToDouble(this.numericUpDownUnsolConfirmTimeout.Value));
                ret.solicitedConfirmTimeout = TimeSpan.FromMilliseconds(Decimal.ToDouble(this.numericUpDownSolConfirmTimeout.Value));
                return ret;
            }
        }

        public OutstationStackConfig Configuration
        {
            get
            {
                var oc = ConfigurationXml.Clone(initialConfig);
                oc.link = this.linkConfigControl.Configuration;
                
                oc.outstation.config = this.OutstationParameters;
                oc.outstation.buffer = this.eventBufferConfigControl1.Configuration;                
                            
                if (this.allowTemplateEdit)
                {
                    var templateId = this.comboBoxTemplate.SelectedItem.ToString();
                    var template = config.GetTemplateMaybeNull(templateId);
                    oc.databaseTemplate = ConfigurationXml.Clone(template ?? initialConfig.databaseTemplate);
                }
                else
                {
                    oc.databaseTemplate = ConfigurationXml.Clone(initialConfig.databaseTemplate);
                }
                staticResponseTypeControl1.Configure(oc.databaseTemplate);
                eventResponseTypeControl1.Configure(oc.databaseTemplate);

                return oc;
            }
        }

        void CheckState()
        {
            if (this.allowTemplateEdit && comboBoxTemplate.SelectedValue == null)
            {
                this.buttonEdit.Enabled = false;
                this.buttonAdd.Enabled = false;
                this.toolStripStatusLabel1.Text = "You must select a device template";
            }
            else
            {
                this.buttonEdit.Enabled = true;
                this.buttonAdd.Enabled = true;
                this.toolStripStatusLabel1.Text = "";
            }
        }

        void OutstationDialog_Load(object sender, EventArgs e)
        {
            CheckState();
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            using (var dialog = new TemplateDialog("template1", new DatabaseTemplate()))
            {                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var template = dialog.ConfiguredTemplate;

                    // TODO - let the dialog config these internally
                    this.staticResponseTypeControl1.Configure(template);
                    this.eventResponseTypeControl1.Configure(template);


                    config.AddTemplate(dialog.SelectedAlias, template);
                    this.comboBoxTemplate.DataSource = config.Templates.Select(kvp => kvp.Key).ToList();                    
                }
            }
        }       

        private void comboBoxTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.CheckState();
            if (config != null && comboBoxTemplate.SelectedItem != null)
            {
                var template = config.GetTemplateMaybeNull(comboBoxTemplate.SelectedItem.ToString());
                if (template != null)
                {
                    staticResponseTypeControl1.LoadTemplate(template);
                    eventResponseTypeControl1.LoadTemplate(template);
                }
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            var alias = this.comboBoxTemplate.SelectedItem.ToString();
            var edited = config.GetTemplateMaybeNull(alias);

            if (edited != null)
            {
                using (var dialog = new TemplateDialog(alias, edited))
                {
                    dialog.ShowDialog();
                    if (dialog.DialogResult == DialogResult.OK)
                    {
                        var template = dialog.ConfiguredTemplate;
                        config.AddTemplate(dialog.SelectedAlias, template);
                        this.comboBoxTemplate.DataSource = config.Templates.Select(kvp => kvp.Key).ToList();
                    }
                }
            }
           
        }
    }
}
