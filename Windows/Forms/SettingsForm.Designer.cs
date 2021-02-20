
namespace MassiveKnob.Forms
{
    partial class SettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.NotifyIconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.QuitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CommunicationGroupbox = new System.Windows.Forms.GroupBox();
            this.SerialPortStatusLabel = new System.Windows.Forms.Label();
            this.SerialPortCombobox = new System.Windows.Forms.ComboBox();
            this.SerialPortLabel = new System.Windows.Forms.Label();
            this.DevicesGroupbox = new System.Windows.Forms.GroupBox();
            this.DevicesPanel = new System.Windows.Forms.Panel();
            this.DeviceCountUnknownLabel = new System.Windows.Forms.Label();
            this.CloseButton = new System.Windows.Forms.Button();
            this.NotifyIconMenu.SuspendLayout();
            this.CommunicationGroupbox.SuspendLayout();
            this.DevicesGroupbox.SuspendLayout();
            this.DevicesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenuStrip = this.NotifyIconMenu;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Text = "Massive Knob";
            this.NotifyIcon.Visible = true;
            this.NotifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
            // 
            // NotifyIconMenu
            // 
            this.NotifyIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingsToolStripMenuItem,
            this.QuitToolStripMenuItem});
            this.NotifyIconMenu.Name = "NotifyIconMenu";
            this.NotifyIconMenu.Size = new System.Drawing.Size(121, 48);
            // 
            // SettingsToolStripMenuItem
            // 
            this.SettingsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
            this.SettingsToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.SettingsToolStripMenuItem.Text = "&Settings";
            this.SettingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // QuitToolStripMenuItem
            // 
            this.QuitToolStripMenuItem.Name = "QuitToolStripMenuItem";
            this.QuitToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.QuitToolStripMenuItem.Text = "&Quit";
            this.QuitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItem_Click);
            // 
            // CommunicationGroupbox
            // 
            this.CommunicationGroupbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommunicationGroupbox.Controls.Add(this.SerialPortStatusLabel);
            this.CommunicationGroupbox.Controls.Add(this.SerialPortCombobox);
            this.CommunicationGroupbox.Controls.Add(this.SerialPortLabel);
            this.CommunicationGroupbox.Location = new System.Drawing.Point(12, 12);
            this.CommunicationGroupbox.Name = "CommunicationGroupbox";
            this.CommunicationGroupbox.Size = new System.Drawing.Size(455, 52);
            this.CommunicationGroupbox.TabIndex = 1;
            this.CommunicationGroupbox.TabStop = false;
            this.CommunicationGroupbox.Text = " Communication ";
            // 
            // SerialPortStatusLabel
            // 
            this.SerialPortStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SerialPortStatusLabel.AutoEllipsis = true;
            this.SerialPortStatusLabel.Location = new System.Drawing.Point(261, 22);
            this.SerialPortStatusLabel.Name = "SerialPortStatusLabel";
            this.SerialPortStatusLabel.Size = new System.Drawing.Size(188, 18);
            this.SerialPortStatusLabel.TabIndex = 2;
            this.SerialPortStatusLabel.Text = "[runtime]";
            // 
            // SerialPortCombobox
            // 
            this.SerialPortCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SerialPortCombobox.FormattingEnabled = true;
            this.SerialPortCombobox.Location = new System.Drawing.Point(107, 19);
            this.SerialPortCombobox.Name = "SerialPortCombobox";
            this.SerialPortCombobox.Size = new System.Drawing.Size(148, 21);
            this.SerialPortCombobox.TabIndex = 1;
            this.SerialPortCombobox.SelectedIndexChanged += new System.EventHandler(this.SerialPortCombobox_SelectedIndexChanged);
            // 
            // SerialPortLabel
            // 
            this.SerialPortLabel.AutoSize = true;
            this.SerialPortLabel.Location = new System.Drawing.Point(10, 22);
            this.SerialPortLabel.Name = "SerialPortLabel";
            this.SerialPortLabel.Size = new System.Drawing.Size(54, 13);
            this.SerialPortLabel.TabIndex = 0;
            this.SerialPortLabel.Text = "Serial port";
            // 
            // DevicesGroupbox
            // 
            this.DevicesGroupbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DevicesGroupbox.Controls.Add(this.DevicesPanel);
            this.DevicesGroupbox.Location = new System.Drawing.Point(12, 70);
            this.DevicesGroupbox.Name = "DevicesGroupbox";
            this.DevicesGroupbox.Size = new System.Drawing.Size(455, 57);
            this.DevicesGroupbox.TabIndex = 2;
            this.DevicesGroupbox.TabStop = false;
            this.DevicesGroupbox.Text = " Audio devices ";
            // 
            // DevicesPanel
            // 
            this.DevicesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DevicesPanel.Controls.Add(this.DeviceCountUnknownLabel);
            this.DevicesPanel.Location = new System.Drawing.Point(13, 19);
            this.DevicesPanel.Name = "DevicesPanel";
            this.DevicesPanel.Size = new System.Drawing.Size(436, 32);
            this.DevicesPanel.TabIndex = 1;
            // 
            // DeviceCountUnknownLabel
            // 
            this.DeviceCountUnknownLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DeviceCountUnknownLabel.Location = new System.Drawing.Point(0, 0);
            this.DeviceCountUnknownLabel.Name = "DeviceCountUnknownLabel";
            this.DeviceCountUnknownLabel.Size = new System.Drawing.Size(436, 32);
            this.DeviceCountUnknownLabel.TabIndex = 1;
            this.DeviceCountUnknownLabel.Text = "Insert Massive Knob to continue...";
            this.DeviceCountUnknownLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(392, 133);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(479, 168);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.DevicesGroupbox);
            this.Controls.Add(this.CommunicationGroupbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Massive Knob - Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.NotifyIconMenu.ResumeLayout(false);
            this.CommunicationGroupbox.ResumeLayout(false);
            this.CommunicationGroupbox.PerformLayout();
            this.DevicesGroupbox.ResumeLayout(false);
            this.DevicesPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.ContextMenuStrip NotifyIconMenu;
        private System.Windows.Forms.ToolStripMenuItem QuitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsToolStripMenuItem;
        private System.Windows.Forms.GroupBox CommunicationGroupbox;
        private System.Windows.Forms.Label SerialPortStatusLabel;
        private System.Windows.Forms.ComboBox SerialPortCombobox;
        private System.Windows.Forms.Label SerialPortLabel;
        private System.Windows.Forms.GroupBox DevicesGroupbox;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Panel DevicesPanel;
        private System.Windows.Forms.Label DeviceCountUnknownLabel;
    }
}

