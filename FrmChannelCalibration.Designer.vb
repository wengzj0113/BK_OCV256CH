<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmChannelCalibration
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer
    Friend WithEvents grid As DataGridView
    Friend WithEvents txtStandardResistance As TextBox
    Friend WithEvents txtStartChannel As TextBox
    Friend WithEvents txtEndChannel As TextBox
    Friend WithEvents lblStatus As Label
    Friend WithEvents progress As ProgressBar
    Friend WithEvents btnReload As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnSelectRange As Button
    Friend WithEvents btnSelectAll As Button
    Friend WithEvents btnClearSelection As Button
    Friend WithEvents btnCalibrateSelected As Button
    Friend WithEvents btnStop As Button
    Friend WithEvents topPanel As FlowLayoutPanel
    Friend WithEvents bottomPanel As Panel
    Friend WithEvents lblStandard As Label
    Friend WithEvents lblStartChannel As Label
    Friend WithEvents lblEndChannel As Label

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then components.Dispose()
        MyBase.Dispose(disposing)
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.grid = New System.Windows.Forms.DataGridView()
        Me.txtStandardResistance = New System.Windows.Forms.TextBox()
        Me.txtStartChannel = New System.Windows.Forms.TextBox()
        Me.txtEndChannel = New System.Windows.Forms.TextBox()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.progress = New System.Windows.Forms.ProgressBar()
        Me.btnReload = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnSelectRange = New System.Windows.Forms.Button()
        Me.btnSelectAll = New System.Windows.Forms.Button()
        Me.btnClearSelection = New System.Windows.Forms.Button()
        Me.btnCalibrateSelected = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.topPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblStandard = New System.Windows.Forms.Label()
        Me.lblStartChannel = New System.Windows.Forms.Label()
        Me.lblEndChannel = New System.Windows.Forms.Label()
        Me.bottomPanel = New System.Windows.Forms.Panel()
        Me.DataGridViewTextBoxColumn7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewCheckBoxColumn1 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        CType(Me.grid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.topPanel.SuspendLayout()
        Me.bottomPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'grid
        '
        Me.grid.AllowUserToAddRows = False
        Me.grid.AllowUserToDeleteRows = False
        Me.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.grid.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewCheckBoxColumn1, Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2, Me.DataGridViewTextBoxColumn3, Me.DataGridViewTextBoxColumn4, Me.DataGridViewTextBoxColumn5, Me.DataGridViewTextBoxColumn6, Me.DataGridViewTextBoxColumn7})
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.Location = New System.Drawing.Point(0, 86)
        Me.grid.Name = "grid"
        Me.grid.RowHeadersVisible = False
        Me.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.grid.Size = New System.Drawing.Size(1544, 521)
        Me.grid.TabIndex = 0
        '
        'txtStandardResistance
        '
        Me.txtStandardResistance.Location = New System.Drawing.Point(100, 11)
        Me.txtStandardResistance.Name = "txtStandardResistance"
        Me.txtStandardResistance.Size = New System.Drawing.Size(90, 21)
        Me.txtStandardResistance.TabIndex = 1
        Me.txtStandardResistance.Text = "3.75"
        '
        'txtStartChannel
        '
        Me.txtStartChannel.Location = New System.Drawing.Point(264, 11)
        Me.txtStartChannel.Name = "txtStartChannel"
        Me.txtStartChannel.Size = New System.Drawing.Size(56, 21)
        Me.txtStartChannel.TabIndex = 3
        Me.txtStartChannel.Text = "1"
        '
        'txtEndChannel
        '
        Me.txtEndChannel.Location = New System.Drawing.Point(390, 11)
        Me.txtEndChannel.Name = "txtEndChannel"
        Me.txtEndChannel.Size = New System.Drawing.Size(56, 21)
        Me.txtEndChannel.TabIndex = 5
        Me.txtEndChannel.Text = "256"
        '
        'lblStatus
        '
        Me.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblStatus.Location = New System.Drawing.Point(8, 4)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(1308, 26)
        Me.lblStatus.TabIndex = 0
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'progress
        '
        Me.progress.Dock = System.Windows.Forms.DockStyle.Right
        Me.progress.Location = New System.Drawing.Point(1316, 4)
        Me.progress.Maximum = 256
        Me.progress.Name = "progress"
        Me.progress.Size = New System.Drawing.Size(220, 26)
        Me.progress.TabIndex = 1
        '
        'btnReload
        '
        Me.btnReload.Location = New System.Drawing.Point(806, 11)
        Me.btnReload.Name = "btnReload"
        Me.btnReload.Size = New System.Drawing.Size(112, 28)
        Me.btnReload.TabIndex = 9
        Me.btnReload.Text = "读取校准值"
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(924, 11)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(112, 28)
        Me.btnSave.TabIndex = 10
        Me.btnSave.Text = "保存校准值"
        '
        'btnClear
        '
        Me.btnClear.Location = New System.Drawing.Point(1042, 11)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(112, 28)
        Me.btnClear.TabIndex = 11
        Me.btnClear.Text = "全部清零"
        '
        'btnSelectRange
        '
        Me.btnSelectRange.Location = New System.Drawing.Point(452, 11)
        Me.btnSelectRange.Name = "btnSelectRange"
        Me.btnSelectRange.Size = New System.Drawing.Size(112, 28)
        Me.btnSelectRange.TabIndex = 6
        Me.btnSelectRange.Text = "选择范围"
        '
        'btnSelectAll
        '
        Me.btnSelectAll.Location = New System.Drawing.Point(570, 11)
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.Size = New System.Drawing.Size(112, 28)
        Me.btnSelectAll.TabIndex = 7
        Me.btnSelectAll.Text = "全选"
        '
        'btnClearSelection
        '
        Me.btnClearSelection.Location = New System.Drawing.Point(688, 11)
        Me.btnClearSelection.Name = "btnClearSelection"
        Me.btnClearSelection.Size = New System.Drawing.Size(112, 28)
        Me.btnClearSelection.TabIndex = 8
        Me.btnClearSelection.Text = "取消选择"
        '
        'btnCalibrateSelected
        '
        Me.btnCalibrateSelected.Location = New System.Drawing.Point(1160, 11)
        Me.btnCalibrateSelected.Name = "btnCalibrateSelected"
        Me.btnCalibrateSelected.Size = New System.Drawing.Size(112, 28)
        Me.btnCalibrateSelected.TabIndex = 12
        Me.btnCalibrateSelected.Text = "校准选中通道"
        '
        'btnStop
        '
        Me.btnStop.Enabled = False
        Me.btnStop.Location = New System.Drawing.Point(1278, 11)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(112, 28)
        Me.btnStop.TabIndex = 13
        Me.btnStop.Text = "停止"
        '
        'topPanel
        '
        Me.topPanel.AutoScroll = True
        Me.topPanel.Controls.Add(Me.lblStandard)
        Me.topPanel.Controls.Add(Me.txtStandardResistance)
        Me.topPanel.Controls.Add(Me.lblStartChannel)
        Me.topPanel.Controls.Add(Me.txtStartChannel)
        Me.topPanel.Controls.Add(Me.lblEndChannel)
        Me.topPanel.Controls.Add(Me.txtEndChannel)
        Me.topPanel.Controls.Add(Me.btnSelectRange)
        Me.topPanel.Controls.Add(Me.btnSelectAll)
        Me.topPanel.Controls.Add(Me.btnClearSelection)
        Me.topPanel.Controls.Add(Me.btnReload)
        Me.topPanel.Controls.Add(Me.btnSave)
        Me.topPanel.Controls.Add(Me.btnClear)
        Me.topPanel.Controls.Add(Me.btnCalibrateSelected)
        Me.topPanel.Controls.Add(Me.btnStop)
        Me.topPanel.Dock = System.Windows.Forms.DockStyle.Top
        Me.topPanel.Location = New System.Drawing.Point(0, 0)
        Me.topPanel.Name = "topPanel"
        Me.topPanel.Padding = New System.Windows.Forms.Padding(8)
        Me.topPanel.Size = New System.Drawing.Size(1544, 86)
        Me.topPanel.TabIndex = 2
        Me.topPanel.WrapContents = False
        '
        'lblStandard
        '
        Me.lblStandard.AutoSize = True
        Me.lblStandard.Location = New System.Drawing.Point(11, 15)
        Me.lblStandard.Margin = New System.Windows.Forms.Padding(3, 7, 3, 3)
        Me.lblStandard.Name = "lblStandard"
        Me.lblStandard.Size = New System.Drawing.Size(83, 12)
        Me.lblStandard.TabIndex = 0
        Me.lblStandard.Text = "标准阻值(mΩ)"
        '
        'lblStartChannel
        '
        Me.lblStartChannel.AutoSize = True
        Me.lblStartChannel.Location = New System.Drawing.Point(205, 15)
        Me.lblStartChannel.Margin = New System.Windows.Forms.Padding(12, 7, 3, 3)
        Me.lblStartChannel.Name = "lblStartChannel"
        Me.lblStartChannel.Size = New System.Drawing.Size(53, 12)
        Me.lblStartChannel.TabIndex = 2
        Me.lblStartChannel.Text = "起始通道"
        '
        'lblEndChannel
        '
        Me.lblEndChannel.AutoSize = True
        Me.lblEndChannel.Location = New System.Drawing.Point(331, 15)
        Me.lblEndChannel.Margin = New System.Windows.Forms.Padding(8, 7, 3, 3)
        Me.lblEndChannel.Name = "lblEndChannel"
        Me.lblEndChannel.Size = New System.Drawing.Size(53, 12)
        Me.lblEndChannel.TabIndex = 4
        Me.lblEndChannel.Text = "结束通道"
        '
        'bottomPanel
        '
        Me.bottomPanel.Controls.Add(Me.lblStatus)
        Me.bottomPanel.Controls.Add(Me.progress)
        Me.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.bottomPanel.Location = New System.Drawing.Point(0, 607)
        Me.bottomPanel.Name = "bottomPanel"
        Me.bottomPanel.Padding = New System.Windows.Forms.Padding(8, 4, 8, 4)
        Me.bottomPanel.Size = New System.Drawing.Size(1544, 34)
        Me.bottomPanel.TabIndex = 1
        '
        'DataGridViewTextBoxColumn7
        '
        Me.DataGridViewTextBoxColumn7.Name = "DataGridViewTextBoxColumn7"
        '
        'DataGridViewTextBoxColumn6
        '
        Me.DataGridViewTextBoxColumn6.Name = "DataGridViewTextBoxColumn6"
        '
        'DataGridViewTextBoxColumn5
        '
        Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'DataGridViewCheckBoxColumn1
        '
        Me.DataGridViewCheckBoxColumn1.Name = "DataGridViewCheckBoxColumn1"
        '
        'FrmChannelCalibration
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1544, 641)
        Me.Controls.Add(Me.grid)
        Me.Controls.Add(Me.bottomPanel)
        Me.Controls.Add(Me.topPanel)
        Me.MinimumSize = New System.Drawing.Size(1560, 560)
        Me.Name = "FrmChannelCalibration"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "通道校准"
        CType(Me.grid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.topPanel.ResumeLayout(False)
        Me.topPanel.PerformLayout()
        Me.bottomPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents DataGridViewCheckBoxColumn1 As DataGridViewCheckBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn6 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn7 As DataGridViewTextBoxColumn
End Class
