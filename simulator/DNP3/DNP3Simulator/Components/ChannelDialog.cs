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

            this.comboBoxParity.SelectedItem = Parity.NONE;
            this.comboBoxStopBits.SelectedItem = StopBits.ONE;
            this.comboBoxFlowControl.SelectedItem = FlowControl.NONE;

            for (int i = 0; i < 3; i++)
            {
                this.clientTlsVersion.SetItemChecked(i, true);
                this.serverTlsVersion.SetItemChecked(i, true);
            }
        }

        private void buttonADD_Click(object sender, EventArgs e)
        {            
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

        private Func<IDNP3Manager, IChannel> GetCreateFunctorMaybeNull()
        {
            var min = TimeSpan.FromMilliseconds(Decimal.ToDouble(numericUpDownMinRetryMS.Value));
            var max = TimeSpan.FromMilliseconds(Decimal.ToDouble(numericUpDownMaxRetryMS.Value));

            switch (tabControlChannelType.SelectedIndex)
            { 
                case(0):
                    if (clientEnableTls.Checked &&
                        !TlsSettingsValid(clientTlsTrustedCert.Text, clientTlsCert.Text, clientTlsCert.Text,
                                          clientTlsVersion.GetItemChecked(0), clientTlsVersion.GetItemChecked(1), clientTlsVersion.GetItemChecked(2)))
                    {
                        return null;
                    }
                    return GetTCPClientFunctor(min, max, clientEnableTls.Checked);
                case(1):
                    if (serverEnableTls.Checked &&
                        !TlsSettingsValid(serverTlsTrustedCert.Text, serverTlsCert.Text, serverTlsCert.Text,
                                          serverTlsVersion.GetItemChecked(0), serverTlsVersion.GetItemChecked(1), serverTlsVersion.GetItemChecked(2)))
                    {
                        return null;
                    }
                    return GetTCPServerFunctor(min, max, serverEnableTls.Checked);
                case(2):
                    return GetSerialFunctor(min, max);
                default:
                    return null;
            }
        }

        private bool TlsSettingsValid(string trustedCertPath, string certificatePath, string privateKeyPath,
            bool tlsVersion1_0, bool tlsVersion1_1, bool tlsVersion1_2)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trustedCertPath) ||
                    string.IsNullOrWhiteSpace(Path.GetFullPath(trustedCertPath)))
                {
                    MessageBox.Show("Invalid Trusted Certificate File Path", "Invalid TLS Options");
                    return false;
                }
            }
            catch 
            {
                MessageBox.Show("Invalid Trusted Certificate File Path", "Invalid TLS Options");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(certificatePath) ||
                    string.IsNullOrWhiteSpace(Path.GetFullPath(certificatePath)))
                {
                    MessageBox.Show("Invalid Certificate File Path", "Invalid TLS Options");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Invalid Certificate File Path", "Invalid TLS Options");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(privateKeyPath) ||
                    string.IsNullOrWhiteSpace(Path.GetFullPath(privateKeyPath)))
                {
                    MessageBox.Show("Invalid Private Key File Path", "Invalid TLS Options");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Invalid Private Key File Path", "Invalid TLS Options");
                return false;
            }

            if (!tlsVersion1_0 &&
                !tlsVersion1_1 &&
                !tlsVersion1_2)
            {
                MessageBox.Show("At least one version must be selected.", "Invalid TLS Options");
                return false;
            }
            return true;
        }

        private Func<IDNP3Manager, IChannel> GetSerialFunctor(TimeSpan min, TimeSpan max)
        {
            var name = this.comboBoxSerialDeviceName.Text;
            var baud = Decimal.ToInt32(this.numericUpDownBaud.Value);
            
            var dataBits = Decimal.ToInt32(this.numericUpDownDataBits.Value);
            var parity = (Parity)comboBoxParity.SelectedValue;
            var flow = (FlowControl) comboBoxFlowControl.SelectedValue;
            var stopBits = (StopBits) comboBoxStopBits.SelectedValue;

            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max);
            var ss = new SerialSettings(name, baud, dataBits, stopBits, parity, flow);
            return (IDNP3Manager manager) => manager.AddSerial(this.textBoxID.Text, flags, retry, ss);
        }

        private Func<IDNP3Manager, IChannel> GetTCPClientFunctor(TimeSpan min, TimeSpan max, bool tlsRequired)
        {
            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max);
            if (tlsRequired)
                return (IDNP3Manager manager) => 
                    manager.AddTLSClient(this.textBoxID.Text, flags, retry, textBoxHost.Text, Decimal.ToUInt16(numericUpDownPort.Value),
                        new TLSConfig(
                            clientTlsTrustedCert.Text,
                            clientTlsCert.Text,
                            clientTlsPrivateKey.Text,
                            Convert.ToInt32(clientTlsMaxDepth.Value),
                            clientTlsVersion.GetItemChecked(0),
                            clientTlsVersion.GetItemChecked(1),
                            clientTlsVersion.GetItemChecked(2),
                            clientTlsCipherList.Text));
            else
                return (IDNP3Manager manager) =>
                    manager.AddTCPClient(this.textBoxID.Text, flags, retry, textBoxHost.Text, Decimal.ToUInt16(numericUpDownPort.Value));
        }

        private Func<IDNP3Manager, IChannel> GetTCPServerFunctor(TimeSpan min, TimeSpan max, bool tlsRequired)
        {
            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max);
            if (tlsRequired)
                return (IDNP3Manager manager) => 
                    manager.AddTLSServer(this.textBoxID.Text, flags, retry, textBoxServerHost.Text, Decimal.ToUInt16(numericUpDownServerPort.Value),
                        new TLSConfig(
                            serverTlsTrustedCert.Text,
                            serverTlsCert.Text,
                            serverTlsPrivateKey.Text,
                            Convert.ToInt32(serverTlsMaxDepth.Value),
                            serverTlsVersion.GetItemChecked(0),
                            serverTlsVersion.GetItemChecked(1),
                            serverTlsVersion.GetItemChecked(2),
                            serverTlsCipherList.Text));
            else
                return (IDNP3Manager manager) => 
                    manager.AddTCPServer(this.textBoxID.Text, flags, retry, textBoxServerHost.Text, Decimal.ToUInt16(numericUpDownServerPort.Value));
        } 

        public Func<IDNP3Manager, IChannel> ChannelAction
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

        private Func<IDNP3Manager, IChannel> create = null;

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
    }
}
