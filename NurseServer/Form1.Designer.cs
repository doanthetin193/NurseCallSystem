namespace NurseServer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        
        private System.Windows.Forms.DataGridView dgvBeds;
        private System.Windows.Forms.ListBox lbLogs;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnCodeBlue;
        private System.Windows.Forms.Label lblStatus;
        
        private System.Windows.Forms.ContextMenuStrip ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemResolve;
        private System.Windows.Forms.ToolStripMenuItem menuItemDelete;
        
        private System.Windows.Forms.GroupBox grpAddRoom;
        private System.Windows.Forms.Button btnAddRoom;
        private System.Windows.Forms.CheckBox chkEnableEdit;
        private System.Windows.Forms.Label lblMaxFloors;
        private System.Windows.Forms.NumericUpDown numMaxFloors;
        private System.Windows.Forms.Label lblAutoSection;
        private System.Windows.Forms.Label lblManualSection;
        private System.Windows.Forms.Label lblPhong;
        private System.Windows.Forms.TextBox txtManualRoom;
        private System.Windows.Forms.ComboBox cmbManualBed;
        private System.Windows.Forms.Button btnManualAdd;

        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.ComboBox cmbFilterFloor;
        private System.Windows.Forms.ComboBox cmbFilterStatus;
        
        private System.Windows.Forms.Button btnHistory;

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
            this.components = new System.ComponentModel.Container();
            this.dgvBeds = new System.Windows.Forms.DataGridView();
            this.lbLogs = new System.Windows.Forms.ListBox();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnCodeBlue = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            
            this.ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemResolve = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            
            this.grpAddRoom = new System.Windows.Forms.GroupBox();
            this.btnAddRoom = new System.Windows.Forms.Button();
            this.chkEnableEdit = new System.Windows.Forms.CheckBox();
            this.lblMaxFloors = new System.Windows.Forms.Label();
            this.numMaxFloors = new System.Windows.Forms.NumericUpDown();
            this.lblAutoSection = new System.Windows.Forms.Label();
            this.lblManualSection = new System.Windows.Forms.Label();
            this.lblPhong = new System.Windows.Forms.Label();
            this.txtManualRoom = new System.Windows.Forms.TextBox();
            this.cmbManualBed = new System.Windows.Forms.ComboBox();
            this.btnManualAdd = new System.Windows.Forms.Button();

            this.lblFilter = new System.Windows.Forms.Label();
            this.cmbFilterFloor = new System.Windows.Forms.ComboBox();
            this.cmbFilterStatus = new System.Windows.Forms.ComboBox();
            
            this.btnHistory = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvBeds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFloors)).BeginInit();
            this.ctxMenu.SuspendLayout();
            this.grpAddRoom.SuspendLayout();
            this.SuspendLayout();
            
            // lblFilter
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(12, 17);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(69, 13);
            this.lblFilter.Text = "Lọc hiển thị:";
            
            // cmbFilterFloor
            this.cmbFilterFloor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterFloor.Location = new System.Drawing.Point(85, 14);
            this.cmbFilterFloor.Size = new System.Drawing.Size(150, 21);
            this.cmbFilterFloor.Items.AddRange(new object[] { "Tất cả các tầng" });
            this.cmbFilterFloor.SelectedIndex = 0;
            this.cmbFilterFloor.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);
            
            // cmbFilterStatus
            this.cmbFilterStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterStatus.Location = new System.Drawing.Point(260, 14);
            this.cmbFilterStatus.Size = new System.Drawing.Size(200, 21);
            this.cmbFilterStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Chỉ các máy BÁO ĐỘNG", "🔴 Chỉ CẤP CỨU", "🟡 Chỉ THAY DỊCH", "🟢 Chỉ HỖ TRỢ VỆ SINH" });
            this.cmbFilterStatus.SelectedIndex = 0;
            this.cmbFilterStatus.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);
            
            // btnHistory
            this.btnHistory.Location = new System.Drawing.Point(620, 450);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(155, 40);
            this.btnHistory.TabIndex = 6;
            this.btnHistory.Text = "📂 XEM LỊCH SỬ\r\n(Truy xuất DB)";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            
            // ctxMenu
            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemResolve,
            this.menuItemDelete});
            this.ctxMenu.Name = "ctxMenu";
            this.ctxMenu.Size = new System.Drawing.Size(260, 48);
            this.ctxMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ctxMenu_Opening);
            
            // menuItemResolve
            this.menuItemResolve.Name = "menuItemResolve";
            this.menuItemResolve.Size = new System.Drawing.Size(259, 22);
            this.menuItemResolve.Text = "Đánh dấu: ĐÃ ĐẾN GIƯỜNG XỬ LÝ XONG";
            this.menuItemResolve.Click += new System.EventHandler(this.menuItemResolve_Click);
            
            // menuItemDelete
            this.menuItemDelete.Name = "menuItemDelete";
            this.menuItemDelete.Size = new System.Drawing.Size(259, 22);
            this.menuItemDelete.Text = "XÓA VĨNH VIỄN khỏi Hệ thống";
            this.menuItemDelete.Click += new System.EventHandler(this.menuItemDelete_Click);
            
            // dgvBeds
            this.dgvBeds.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBeds.Location = new System.Drawing.Point(12, 45);
            this.dgvBeds.Name = "dgvBeds";
            this.dgvBeds.RowHeadersVisible = false;
            this.dgvBeds.Size = new System.Drawing.Size(600, 275);
            this.dgvBeds.TabIndex = 0;
            this.dgvBeds.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBeds.ContextMenuStrip = this.ctxMenu;
            this.dgvBeds.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvBeds_DataBindingComplete);
            
            // lbLogs
            this.lbLogs.FormattingEnabled = true;
            this.lbLogs.Location = new System.Drawing.Point(12, 330);
            this.lbLogs.Name = "lbLogs";
            this.lbLogs.Size = new System.Drawing.Size(600, 160);
            this.lbLogs.TabIndex = 1;
            
            // btnStartServer
            this.btnStartServer.Location = new System.Drawing.Point(630, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(140, 50);
            this.btnStartServer.TabIndex = 2;
            this.btnStartServer.Text = "Khởi động Server\r\n(Auto-Discovery)";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            
            // btnCodeBlue
            this.btnCodeBlue.BackColor = System.Drawing.Color.Red;
            this.btnCodeBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnCodeBlue.ForeColor = System.Drawing.Color.White;
            this.btnCodeBlue.Location = new System.Drawing.Point(630, 80);
            this.btnCodeBlue.Name = "btnCodeBlue";
            this.btnCodeBlue.Size = new System.Drawing.Size(140, 60);
            this.btnCodeBlue.TabIndex = 3;
            this.btnCodeBlue.Text = "BÁO ĐỘNG TOÀN VIỆN";
            this.btnCodeBlue.UseVisualStyleBackColor = false;
            this.btnCodeBlue.Click += new System.EventHandler(this.btnCodeBlue_Click);
            
            // chkEnableEdit
            this.chkEnableEdit.AutoSize = true;
            this.chkEnableEdit.Location = new System.Drawing.Point(630, 145);
            this.chkEnableEdit.Name = "chkEnableEdit";
            this.chkEnableEdit.Size = new System.Drawing.Size(140, 17);
            this.chkEnableEdit.TabIndex = 7;
            this.chkEnableEdit.Text = "Bật Chế Độ Cài Đặt";
            this.chkEnableEdit.UseVisualStyleBackColor = true;
            this.chkEnableEdit.CheckedChanged += new System.EventHandler(this.chkEnableEdit_CheckedChanged);
            
            // grpAddRoom
            this.grpAddRoom.Controls.Add(this.btnManualAdd);
            this.grpAddRoom.Controls.Add(this.cmbManualBed);
            this.grpAddRoom.Controls.Add(this.txtManualRoom);
            this.grpAddRoom.Controls.Add(this.lblPhong);
            this.grpAddRoom.Controls.Add(this.lblManualSection);
            this.grpAddRoom.Controls.Add(this.btnAddRoom);
            this.grpAddRoom.Controls.Add(this.lblAutoSection);
            this.grpAddRoom.Controls.Add(this.numMaxFloors);
            this.grpAddRoom.Controls.Add(this.lblMaxFloors);
            this.grpAddRoom.Location = new System.Drawing.Point(620, 165);
            this.grpAddRoom.Name = "grpAddRoom";
            this.grpAddRoom.Size = new System.Drawing.Size(160, 280);
            this.grpAddRoom.Text = "Cài Đặt Phòng / Giường";
            this.grpAddRoom.Visible = false;
            
            // lblMaxFloors
            this.lblMaxFloors.AutoSize = true;
            this.lblMaxFloors.Location = new System.Drawing.Point(5, 25);
            this.lblMaxFloors.Name = "lblMaxFloors";
            this.lblMaxFloors.Text = "Tầng tối đa:";

            // numMaxFloors
            this.numMaxFloors.Location = new System.Drawing.Point(95, 22);
            this.numMaxFloors.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxFloors.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.numMaxFloors.Value = new decimal(new int[] { 6, 0, 0, 0 });
            this.numMaxFloors.Name = "numMaxFloors";
            this.numMaxFloors.Size = new System.Drawing.Size(55, 20);

            // lblAutoSection
            this.lblAutoSection.AutoSize = false;
            this.lblAutoSection.Location = new System.Drawing.Point(5, 50);
            this.lblAutoSection.Name = "lblAutoSection";
            this.lblAutoSection.Size = new System.Drawing.Size(148, 13);
            this.lblAutoSection.Text = "\u2500\u2500 T\u1ecbnh ti\u1ebfn t\u1ef1 \u0111\u1ed9ng \u2500\u2500";
            this.lblAutoSection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // btnAddRoom
            this.btnAddRoom.Location = new System.Drawing.Point(5, 67);
            this.btnAddRoom.Name = "btnAddRoom";
            this.btnAddRoom.Size = new System.Drawing.Size(148, 48);
            this.btnAddRoom.Text = "\u2795 T\u1ecbnh Ti\u1ebfn\n(T\u1ef1 T\u00ecm v\u00e0 Th\u00eam K\u1ec1)";
            this.btnAddRoom.Click += new System.EventHandler(this.btnAddRoom_Click);

            // lblManualSection
            this.lblManualSection.AutoSize = false;
            this.lblManualSection.Location = new System.Drawing.Point(5, 122);
            this.lblManualSection.Name = "lblManualSection";
            this.lblManualSection.Size = new System.Drawing.Size(148, 13);
            this.lblManualSection.Text = "\u2500\u2500 Th\u00eam gi\u01b0\u1eddng tay \u2500\u2500";
            this.lblManualSection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblPhong
            this.lblPhong.AutoSize = true;
            this.lblPhong.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblPhong.Location = new System.Drawing.Point(5, 143);
            this.lblPhong.Name = "lblPhong";
            this.lblPhong.Text = "Ph\u00f2ng";

            // txtManualRoom
            this.txtManualRoom.Location = new System.Drawing.Point(58, 140);
            this.txtManualRoom.Name = "txtManualRoom";
            this.txtManualRoom.Size = new System.Drawing.Size(95, 20);

            // cmbManualBed
            this.cmbManualBed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbManualBed.Items.AddRange(new object[] { "Gi\u01b0\u1eddng A", "Gi\u01b0\u1eddng B", "Gi\u01b0\u1eddng C", "Gi\u01b0\u1eddng D", "Gi\u01b0\u1eddng E" });
            this.cmbManualBed.SelectedIndex = 0;
            this.cmbManualBed.Location = new System.Drawing.Point(5, 167);
            this.cmbManualBed.Name = "cmbManualBed";
            this.cmbManualBed.Size = new System.Drawing.Size(148, 21);

            // btnManualAdd
            this.btnManualAdd.BackColor = System.Drawing.Color.SteelBlue;
            this.btnManualAdd.ForeColor = System.Drawing.Color.White;
            this.btnManualAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnManualAdd.Location = new System.Drawing.Point(5, 196);
            this.btnManualAdd.Name = "btnManualAdd";
            this.btnManualAdd.Size = new System.Drawing.Size(148, 40);
            this.btnManualAdd.Text = "\u2714 Th\u00eam Gi\u01b0\u1eddng";
            this.btnManualAdd.UseVisualStyleBackColor = false;
            this.btnManualAdd.Click += new System.EventHandler(this.btnManualAdd_Click);
            
            // btnReset - REMOVED
            
            
            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 545);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(161, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status: Đang đợi kích hoạt Server";
            
            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 570);
            this.Controls.Add(this.btnHistory);
            this.Controls.Add(this.cmbFilterStatus);
            this.Controls.Add(this.cmbFilterFloor);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.chkEnableEdit);
            this.Controls.Add(this.grpAddRoom);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCodeBlue);
            this.Controls.Add(this.btnStartServer);
            this.Controls.Add(this.lbLogs);
            this.Controls.Add(this.dgvBeds);
            this.Name = "Form1";
            this.Text = "Hệ thống Quản lý Y tá - Máy chủ Trực";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFloors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBeds)).EndInit();
            this.ctxMenu.ResumeLayout(false);
            this.grpAddRoom.ResumeLayout(false);
            this.grpAddRoom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
