namespace PatientClient
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnEmergency;
        private System.Windows.Forms.Button btnIVFluid;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label lblRoomName;
        private System.Windows.Forms.Label lblStatus;

        private System.Windows.Forms.Panel pnlSetup;
        private System.Windows.Forms.Label lblSetupTitle;
        private System.Windows.Forms.Label lblSetupRoom;
        private System.Windows.Forms.ComboBox cmbRoom;
        private System.Windows.Forms.Label lblSetupBed;
        private System.Windows.Forms.ComboBox cmbBed;
        private System.Windows.Forms.Button btnSaveSetup;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnEmergency = new System.Windows.Forms.Button();
            this.btnIVFluid = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.lblRoomName = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            
            this.pnlSetup = new System.Windows.Forms.Panel();
            this.lblSetupTitle = new System.Windows.Forms.Label();
            this.lblSetupRoom = new System.Windows.Forms.Label();
            this.cmbRoom = new System.Windows.Forms.ComboBox();
            this.lblSetupBed = new System.Windows.Forms.Label();
            this.cmbBed = new System.Windows.Forms.ComboBox();
            this.btnSaveSetup = new System.Windows.Forms.Button();
            
            this.pnlSetup.SuspendLayout();
            this.SuspendLayout();
            
            // pnlSetup
            this.pnlSetup.BackColor = System.Drawing.Color.LightBlue;
            this.pnlSetup.Controls.Add(this.btnSaveSetup);
            this.pnlSetup.Controls.Add(this.cmbBed);
            this.pnlSetup.Controls.Add(this.lblSetupBed);
            this.pnlSetup.Controls.Add(this.cmbRoom);
            this.pnlSetup.Controls.Add(this.lblSetupRoom);
            this.pnlSetup.Controls.Add(this.lblSetupTitle);
            this.pnlSetup.Location = new System.Drawing.Point(60, 60);
            this.pnlSetup.Name = "pnlSetup";
            this.pnlSetup.Size = new System.Drawing.Size(360, 240);
            this.pnlSetup.TabIndex = 5;
            this.pnlSetup.Visible = false;
            
            // lblSetupTitle
            this.lblSetupTitle.AutoSize = true;
            this.lblSetupTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblSetupTitle.Location = new System.Drawing.Point(30, 20);
            this.lblSetupTitle.Name = "lblSetupTitle";
            this.lblSetupTitle.Text = "Cài đặt Thiết bị Đầu giường";
            
            // lblSetupRoom
            this.lblSetupRoom.AutoSize = true;
            this.lblSetupRoom.Location = new System.Drawing.Point(40, 70);
            this.lblSetupRoom.Text = "CHỌN Tên Phòng trong Danh sách:";
            
            // cmbRoom
            this.cmbRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRoom.Location = new System.Drawing.Point(40, 90);
            this.cmbRoom.Size = new System.Drawing.Size(280, 21);
            
            // lblSetupBed
            this.lblSetupBed.AutoSize = true;
            this.lblSetupBed.Location = new System.Drawing.Point(40, 130);
            this.lblSetupBed.Text = "CHỌN Ký hiệu Giường:";
            
            // cmbBed
            this.cmbBed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBed.Location = new System.Drawing.Point(40, 150);
            this.cmbBed.Size = new System.Drawing.Size(280, 21);
            
            // btnSaveSetup
            this.btnSaveSetup.BackColor = System.Drawing.Color.SteelBlue;
            this.btnSaveSetup.ForeColor = System.Drawing.Color.White;
            this.btnSaveSetup.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveSetup.Location = new System.Drawing.Point(40, 190);
            this.btnSaveSetup.Size = new System.Drawing.Size(280, 35);
            this.btnSaveSetup.Text = "Lưu & Bắt đầu Kết nối";
            this.btnSaveSetup.UseVisualStyleBackColor = false;
            this.btnSaveSetup.Click += new System.EventHandler(this.btnSaveSetup_Click);

            // lblRoomName
            this.lblRoomName.AutoSize = true;
            this.lblRoomName.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold);
            this.lblRoomName.Location = new System.Drawing.Point(60, 20);
            this.lblRoomName.Size = new System.Drawing.Size(340, 37);
            this.lblRoomName.Text = "Phòng - Giường";

            // btnEmergency
            this.btnEmergency.BackColor = System.Drawing.Color.DarkRed;
            this.btnEmergency.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold);
            this.btnEmergency.ForeColor = System.Drawing.Color.White;
            this.btnEmergency.Location = new System.Drawing.Point(12, 80);
            this.btnEmergency.Size = new System.Drawing.Size(460, 100);
            this.btnEmergency.Text = "🔴 CẤP CỨU MỞ RỘNG";
            this.btnEmergency.UseVisualStyleBackColor = false;
            this.btnEmergency.Click += new System.EventHandler(this.btnEmergency_Click);

            // btnIVFluid
            this.btnIVFluid.BackColor = System.Drawing.Color.Orange;
            this.btnIVFluid.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.btnIVFluid.Location = new System.Drawing.Point(12, 195);
            this.btnIVFluid.Size = new System.Drawing.Size(220, 80);
            this.btnIVFluid.Text = "🟡 CẦN THAY DỊCH";
            this.btnIVFluid.UseVisualStyleBackColor = false;
            this.btnIVFluid.Click += new System.EventHandler(this.btnIVFluid_Click);

            // btnHelp
            this.btnHelp.BackColor = System.Drawing.Color.LimeGreen;
            this.btnHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.btnHelp.Location = new System.Drawing.Point(252, 195);
            this.btnHelp.Size = new System.Drawing.Size(220, 80);
            this.btnHelp.Text = "🟢 HỖ TRỢ VỆ SINH";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 330);
            this.lblStatus.Text = "Status: Đang khởi động mạng...";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.pnlSetup);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnIVFluid);
            this.Controls.Add(this.btnEmergency);
            this.Controls.Add(this.lblRoomName);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bảng Điều Khiển Đầu Giường";
            this.Load += new System.EventHandler(this.Form1_Load);
            
            this.pnlSetup.ResumeLayout(false);
            this.pnlSetup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
