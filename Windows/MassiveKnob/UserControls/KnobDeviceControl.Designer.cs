
namespace MassiveKnob.UserControls
{
    partial class KnobDeviceControl
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
            this.DeviceCombobox = new System.Windows.Forms.ComboBox();
            this.KnobIndexLabel = new System.Windows.Forms.Label();
            this.DeviceLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DeviceCombobox
            // 
            this.DeviceCombobox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DeviceCombobox.DropDownHeight = 300;
            this.DeviceCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeviceCombobox.FormattingEnabled = true;
            this.DeviceCombobox.IntegralHeight = false;
            this.DeviceCombobox.ItemHeight = 13;
            this.DeviceCombobox.Location = new System.Drawing.Point(59, 24);
            this.DeviceCombobox.Name = "DeviceCombobox";
            this.DeviceCombobox.Size = new System.Drawing.Size(286, 21);
            this.DeviceCombobox.TabIndex = 0;
            this.DeviceCombobox.SelectedIndexChanged += new System.EventHandler(this.DeviceCombobox_SelectedIndexChanged);
            // 
            // KnobIndexLabel
            // 
            this.KnobIndexLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KnobIndexLabel.Location = new System.Drawing.Point(3, 8);
            this.KnobIndexLabel.Name = "KnobIndexLabel";
            this.KnobIndexLabel.Size = new System.Drawing.Size(342, 13);
            this.KnobIndexLabel.TabIndex = 1;
            this.KnobIndexLabel.Text = "Knob X";
            // 
            // DeviceLabel
            // 
            this.DeviceLabel.AutoSize = true;
            this.DeviceLabel.Location = new System.Drawing.Point(3, 27);
            this.DeviceLabel.Name = "DeviceLabel";
            this.DeviceLabel.Size = new System.Drawing.Size(41, 13);
            this.DeviceLabel.TabIndex = 2;
            this.DeviceLabel.Text = "Device";
            // 
            // KnobDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DeviceLabel);
            this.Controls.Add(this.KnobIndexLabel);
            this.Controls.Add(this.DeviceCombobox);
            this.Name = "KnobDeviceControl";
            this.Size = new System.Drawing.Size(351, 56);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox DeviceCombobox;
        private System.Windows.Forms.Label KnobIndexLabel;
        private System.Windows.Forms.Label DeviceLabel;
    }
}
