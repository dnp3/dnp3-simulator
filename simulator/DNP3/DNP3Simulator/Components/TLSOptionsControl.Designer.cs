namespace Automatak.Simulator.DNP3
{
    partial class TLSOptionsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxTLS = new System.Windows.Forms.GroupBox();
            this.checkBoxV1_2 = new System.Windows.Forms.CheckBox();
            this.checkBoxV1_1 = new System.Windows.Forms.CheckBox();
            this.checkBoxV1_0 = new System.Windows.Forms.CheckBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.browseTrustedCertButton = new System.Windows.Forms.Button();
            this.trustedCertTextBox = new System.Windows.Forms.TextBox();
            this.browsePrivateKeyButton = new System.Windows.Forms.Button();
            this.privateKeyTextBox = new System.Windows.Forms.TextBox();
            this.browseCertButton = new System.Windows.Forms.Button();
            this.localCertificateTextBox = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.cipherListTextBox = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.maxDepthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxTLS.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxDepthNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxTLS
            // 
            this.groupBoxTLS.Controls.Add(this.checkBoxV1_2);
            this.groupBoxTLS.Controls.Add(this.checkBoxV1_1);
            this.groupBoxTLS.Controls.Add(this.checkBoxV1_0);
            this.groupBoxTLS.Controls.Add(this.label20);
            this.groupBoxTLS.Controls.Add(this.label21);
            this.groupBoxTLS.Controls.Add(this.label22);
            this.groupBoxTLS.Controls.Add(this.browseTrustedCertButton);
            this.groupBoxTLS.Controls.Add(this.trustedCertTextBox);
            this.groupBoxTLS.Controls.Add(this.browsePrivateKeyButton);
            this.groupBoxTLS.Controls.Add(this.privateKeyTextBox);
            this.groupBoxTLS.Controls.Add(this.browseCertButton);
            this.groupBoxTLS.Controls.Add(this.localCertificateTextBox);
            this.groupBoxTLS.Controls.Add(this.label23);
            this.groupBoxTLS.Controls.Add(this.label24);
            this.groupBoxTLS.Controls.Add(this.cipherListTextBox);
            this.groupBoxTLS.Controls.Add(this.label25);
            this.groupBoxTLS.Controls.Add(this.maxDepthNumericUpDown);
            this.groupBoxTLS.Location = new System.Drawing.Point(16, 17);
            this.groupBoxTLS.Name = "groupBoxTLS";
            this.groupBoxTLS.Size = new System.Drawing.Size(494, 259);
            this.groupBoxTLS.TabIndex = 30;
            this.groupBoxTLS.TabStop = false;
            this.groupBoxTLS.Text = "TLS Options";
            // 
            // checkBoxV1_2
            // 
            this.checkBoxV1_2.AutoSize = true;
            this.checkBoxV1_2.Checked = true;
            this.checkBoxV1_2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxV1_2.Location = new System.Drawing.Point(269, 163);
            this.checkBoxV1_2.Name = "checkBoxV1_2";
            this.checkBoxV1_2.Size = new System.Drawing.Size(57, 21);
            this.checkBoxV1_2.TabIndex = 47;
            this.checkBoxV1_2.Text = "v1.2";
            this.checkBoxV1_2.UseVisualStyleBackColor = true;
            // 
            // checkBoxV1_1
            // 
            this.checkBoxV1_1.AutoSize = true;
            this.checkBoxV1_1.Checked = true;
            this.checkBoxV1_1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxV1_1.Location = new System.Drawing.Point(206, 163);
            this.checkBoxV1_1.Name = "checkBoxV1_1";
            this.checkBoxV1_1.Size = new System.Drawing.Size(57, 21);
            this.checkBoxV1_1.TabIndex = 46;
            this.checkBoxV1_1.Text = "v1.1";
            this.checkBoxV1_1.UseVisualStyleBackColor = true;
            // 
            // checkBoxV1_0
            // 
            this.checkBoxV1_0.AutoSize = true;
            this.checkBoxV1_0.Checked = true;
            this.checkBoxV1_0.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxV1_0.Location = new System.Drawing.Point(143, 163);
            this.checkBoxV1_0.Name = "checkBoxV1_0";
            this.checkBoxV1_0.Size = new System.Drawing.Size(57, 21);
            this.checkBoxV1_0.TabIndex = 45;
            this.checkBoxV1_0.Text = "v1.0";
            this.checkBoxV1_0.UseVisualStyleBackColor = true;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(20, 104);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(124, 17);
            this.label20.TabIndex = 44;
            this.label20.Text = "Trusted Certificate";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(20, 69);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(80, 17);
            this.label21.TabIndex = 43;
            this.label21.Text = "Private Key";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(20, 37);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(71, 17);
            this.label22.TabIndex = 42;
            this.label22.Text = "Certificate";
            // 
            // browseTrustedCertButton
            // 
            this.browseTrustedCertButton.Location = new System.Drawing.Point(399, 97);
            this.browseTrustedCertButton.Margin = new System.Windows.Forms.Padding(4);
            this.browseTrustedCertButton.Name = "browseTrustedCertButton";
            this.browseTrustedCertButton.Size = new System.Drawing.Size(79, 28);
            this.browseTrustedCertButton.TabIndex = 41;
            this.browseTrustedCertButton.Text = "Browse...";
            this.browseTrustedCertButton.UseVisualStyleBackColor = true;
            this.browseTrustedCertButton.Click += new System.EventHandler(this.browseTrustedCertButton_Click);
            // 
            // trustedCertTextBox
            // 
            this.trustedCertTextBox.Location = new System.Drawing.Point(145, 101);
            this.trustedCertTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.trustedCertTextBox.Name = "trustedCertTextBox";
            this.trustedCertTextBox.Size = new System.Drawing.Size(244, 22);
            this.trustedCertTextBox.TabIndex = 40;
            // 
            // browsePrivateKeyButton
            // 
            this.browsePrivateKeyButton.Location = new System.Drawing.Point(399, 63);
            this.browsePrivateKeyButton.Margin = new System.Windows.Forms.Padding(4);
            this.browsePrivateKeyButton.Name = "browsePrivateKeyButton";
            this.browsePrivateKeyButton.Size = new System.Drawing.Size(79, 28);
            this.browsePrivateKeyButton.TabIndex = 39;
            this.browsePrivateKeyButton.Text = "Browse...";
            this.browsePrivateKeyButton.UseVisualStyleBackColor = true;
            this.browsePrivateKeyButton.Click += new System.EventHandler(this.browsePrivateKeyButton_Click);
            // 
            // privateKeyTextBox
            // 
            this.privateKeyTextBox.Location = new System.Drawing.Point(145, 67);
            this.privateKeyTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.privateKeyTextBox.Name = "privateKeyTextBox";
            this.privateKeyTextBox.Size = new System.Drawing.Size(244, 22);
            this.privateKeyTextBox.TabIndex = 38;
            // 
            // browseCertButton
            // 
            this.browseCertButton.Location = new System.Drawing.Point(399, 31);
            this.browseCertButton.Margin = new System.Windows.Forms.Padding(4);
            this.browseCertButton.Name = "browseCertButton";
            this.browseCertButton.Size = new System.Drawing.Size(79, 28);
            this.browseCertButton.TabIndex = 37;
            this.browseCertButton.Text = "Browse...";
            this.browseCertButton.UseVisualStyleBackColor = true;
            this.browseCertButton.Click += new System.EventHandler(this.browseCertButton_Click);
            // 
            // localCertificateTextBox
            // 
            this.localCertificateTextBox.Location = new System.Drawing.Point(145, 35);
            this.localCertificateTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.localCertificateTextBox.Name = "localCertificateTextBox";
            this.localCertificateTextBox.Size = new System.Drawing.Size(244, 22);
            this.localCertificateTextBox.TabIndex = 36;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(20, 163);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(103, 17);
            this.label23.TabIndex = 34;
            this.label23.Text = "TLS Version(s)";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(19, 191);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(75, 17);
            this.label24.TabIndex = 33;
            this.label24.Text = "Cipher List";
            // 
            // cipherListTextBox
            // 
            this.cipherListTextBox.Location = new System.Drawing.Point(143, 191);
            this.cipherListTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.cipherListTextBox.Multiline = true;
            this.cipherListTextBox.Name = "cipherListTextBox";
            this.cipherListTextBox.Size = new System.Drawing.Size(335, 45);
            this.cipherListTextBox.TabIndex = 32;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(19, 136);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(75, 17);
            this.label25.TabIndex = 31;
            this.label25.Text = "Max Depth";
            // 
            // maxDepthNumericUpDown
            // 
            this.maxDepthNumericUpDown.Location = new System.Drawing.Point(145, 133);
            this.maxDepthNumericUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.maxDepthNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.maxDepthNumericUpDown.Name = "maxDepthNumericUpDown";
            this.maxDepthNumericUpDown.Size = new System.Drawing.Size(107, 22);
            this.maxDepthNumericUpDown.TabIndex = 30;
            this.maxDepthNumericUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Certificates | *.crt; *.key; *.pem";
            this.openFileDialog.InitialDirectory = "C:\\";
            // 
            // TLSOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxTLS);
            this.Name = "TLSOptionsControl";
            this.Size = new System.Drawing.Size(526, 291);
            this.groupBoxTLS.ResumeLayout(false);
            this.groupBoxTLS.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxDepthNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTLS;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button browseTrustedCertButton;
        private System.Windows.Forms.TextBox trustedCertTextBox;
        private System.Windows.Forms.Button browsePrivateKeyButton;
        private System.Windows.Forms.TextBox privateKeyTextBox;
        private System.Windows.Forms.Button browseCertButton;
        private System.Windows.Forms.TextBox localCertificateTextBox;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox cipherListTextBox;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.NumericUpDown maxDepthNumericUpDown;
        private System.Windows.Forms.CheckBox checkBoxV1_2;
        private System.Windows.Forms.CheckBox checkBoxV1_1;
        private System.Windows.Forms.CheckBox checkBoxV1_0;
        private System.Windows.Forms.OpenFileDialog openFileDialog;


    }
}
