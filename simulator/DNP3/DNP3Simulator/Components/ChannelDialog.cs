using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using Automatak.DNP3.Interface;
using System.IO;

namespace Automatak.Simulator.DNP3.Components
{
    public partial class ChannelDialog : Form
    {
        public ChannelDialog()
        {
            InitializeComponent();            
            this.comboBoxSerialDeviceName.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            this.comboBoxParity.DataSource = Enum.GetValues(typeof(Parity));
            this.comboBoxStopBits.DataSource = Enum.GetValues(typeof(StopBits));
            this.comboBoxFlowControl.DataSource = Enum.GetValues(typeof(FlowControl));

            this.comboBoxParity.SelectedItem = Parity.None;
            this.comboBoxStopBits.SelectedItem = StopBits.One;
            this.comboBoxFlowControl.SelectedItem = FlowControl.None;           
        }

        public ChannelDialog(ChannelConfiguration config) : this()
        {
            Text = "Channel settings";
            buttonADD.Text = "Apply";
            textBoxID.Text = config.Alias;
            tabControlChannelType.SelectedIndex = config.Type;
            numericUpDownMinRetryMS.Value = (decimal)config.MinRetryMs;
            numericUpDownMaxRetryMS.Value = (decimal)config.MaxRetryMs;
            textBoxHost.Text = config.ClientHost;
            numericUpDownPort.Value = config.ClientPort;
            textBoxServerHost.Text = config.ServerHost;
            numericUpDownServerPort.Value = config.ServerPort;
            clientEnableTls.Checked = config.ClientTls;
            serverEnableTls.Checked = config.ServerTls;
            clientTLSOptionsControl.SavedConfiguration = config.ClientTlsOptions;
            serverTLSOptionsControl.SavedConfiguration = config.ServerTlsOptions;
            if (!string.IsNullOrEmpty(config.SerialDevice) && !comboBoxSerialDeviceName.Items.Contains(config.SerialDevice))
                comboBoxSerialDeviceName.Items.Add(config.SerialDevice);
            comboBoxSerialDeviceName.Text = config.SerialDevice;
            numericUpDownBaud.Value = config.Baud;
            numericUpDownDataBits.Value = config.DataBits;
            comboBoxParity.SelectedItem = config.Parity;
            comboBoxStopBits.SelectedItem = config.StopBits;
            comboBoxFlowControl.SelectedItem = config.FlowControl;
            logLevelControl1.SetFlags(config.LogFlags);
        }

        public ChannelConfiguration Configuration
        {
            get
            {
                return new ChannelConfiguration
                {
                    Alias = textBoxID.Text, Type = tabControlChannelType.SelectedIndex,
                    LogFlags = logLevelControl1.Filters.Flags,
                    MinRetryMs = (double)numericUpDownMinRetryMS.Value,
                    MaxRetryMs = (double)numericUpDownMaxRetryMS.Value,
                    ClientHost = textBoxHost.Text, ClientPort = (ushort)numericUpDownPort.Value,
                    ServerHost = textBoxServerHost.Text, ServerPort = (ushort)numericUpDownServerPort.Value,
                    ClientTls = clientEnableTls.Checked, ServerTls = serverEnableTls.Checked,
                    ClientTlsOptions = clientTLSOptionsControl.SavedConfiguration,
                    ServerTlsOptions = serverTLSOptionsControl.SavedConfiguration,
                    SerialDevice = comboBoxSerialDeviceName.Text, Baud = (int)numericUpDownBaud.Value,
                    DataBits = (int)numericUpDownDataBits.Value, Parity = (Parity)comboBoxParity.SelectedItem,
                    StopBits = (StopBits)comboBoxStopBits.SelectedItem, FlowControl = (FlowControl)comboBoxFlowControl.SelectedItem
                };
            }
        }

        private void buttonADD_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration.Validate();
                create = GetCreateFunctorMaybeNull();
                if (create == null)
                {
                    toolStripStatusLabel1.Text = "Unable to create channel";
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating channel");
            }
        }

        private Func<IDNP3Manager, IChannelListener, IChannel> GetCreateFunctorMaybeNull()
        {
            var min = TimeSpan.FromMilliseconds(Decimal.ToDouble(numericUpDownMinRetryMS.Value));
            var max = TimeSpan.FromMilliseconds(Decimal.ToDouble(numericUpDownMaxRetryMS.Value));

            switch (tabControlChannelType.SelectedIndex)
            { 
                case(0):                                        
                    return GetTCPClientFunctor(min, max, clientEnableTls.Checked);
                case(1):                    
                    return GetTCPServerFunctor(min, max, serverEnableTls.Checked);
                case(2):
                    return GetSerialFunctor(min, max);
                default:
                    return null;
            }
        }        

        private Func<IDNP3Manager, IChannelListener, IChannel> GetSerialFunctor(TimeSpan min, TimeSpan max)
        {
            var name = this.comboBoxSerialDeviceName.Text;
            var baud = Decimal.ToInt32(this.numericUpDownBaud.Value);
            
            var dataBits = Decimal.ToInt32(this.numericUpDownDataBits.Value);
            var parity = (Parity)comboBoxParity.SelectedValue;
            var flow = (FlowControl) comboBoxFlowControl.SelectedValue;
            var stopBits = (StopBits) comboBoxStopBits.SelectedValue;

            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max, min);
            var ss = new SerialSettings(name, baud, dataBits, stopBits, parity, flow);
            return (IDNP3Manager manager, IChannelListener listener) => manager.AddSerial(this.textBoxID.Text, flags, retry, ss, listener);
        }

        private Func<IDNP3Manager, IChannelListener, IChannel> GetTCPClientFunctor(TimeSpan min, TimeSpan max, bool useTLS)
        {
            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max, min);
            var host = textBoxHost.Text;
            var port = Decimal.ToUInt16(numericUpDownPort.Value);
            IList<IPEndpoint> remotes = new List<IPEndpoint> { new IPEndpoint(host, port) };
            if (useTLS)
            {
                var config = this.clientTLSOptionsControl.Configuration;
                return (IDNP3Manager manager, IChannelListener listener) =>
                    manager.AddTLSClient(this.textBoxID.Text, flags, retry, remotes, config, listener);
            }
            else
            {
                return (IDNP3Manager manager, IChannelListener listener) => manager.AddTCPClient(this.textBoxID.Text, flags, retry, remotes, listener);
            }
        }

        private Func<IDNP3Manager, IChannelListener, IChannel> GetTCPServerFunctor(TimeSpan min, TimeSpan max, bool useTLS)
        {
            var flags = logLevelControl1.Filters.Flags;
            var host = textBoxServerHost.Text;
            var port = Decimal.ToUInt16(numericUpDownServerPort.Value);
            var endpoint = new IPEndpoint(host, port);
            if (useTLS)
            {
                var config = this.serverTLSOptionsControl.Configuration;
                return (IDNP3Manager manager, IChannelListener listener) =>
                    manager.AddTLSServer(this.textBoxID.Text, flags, ServerAcceptMode.CloseExisting, endpoint, config, listener);
            }
            else
            {
                return (IDNP3Manager manager, IChannelListener listener) =>
                    manager.AddTCPServer(this.textBoxID.Text, flags, ServerAcceptMode.CloseExisting, endpoint, listener);
            }
        } 

        public Func<IDNP3Manager, IChannelListener, IChannel> ChannelAction
        {
            get
            {
                return create;
            }
        }

        public String SelectedAlias
        {
            get
            {
                return textBoxID.Text;
            }
        }

        private Func<IDNP3Manager, IChannelListener, IChannel> create = null;

        /*
        private void clientTlsBrowseCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                clientTlsCert.Text = openFileDialog.FileName;
            }
        }

        private void clientTlsBrowsePrivateKey_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                clientTlsPrivateKey.Text = openFileDialog.FileName;
            }
        }

        private void clientTlsBrowseTrustedCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                clientTlsTrustedCert.Text = openFileDialog.FileName;
            }
        }

        private void serverTlsBrowseCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                serverTlsCert.Text = openFileDialog.FileName;
            }
        }

        private void serverTlsBrowsePrivateKey_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                serverTlsPrivateKey.Text = openFileDialog.FileName;
            }
        }

        private void serverTlsBrowseTrustedCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                serverTlsTrustedCert.Text = openFileDialog.FileName;
            }
        }
        */
    }
}
