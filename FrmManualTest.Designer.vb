<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Public Class FrmManualTest
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer
    Friend WithEvents rootLayoutPanel As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents commandPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents matrixPanel As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents resultGrid As System.Windows.Forms.DataGridView
    Friend WithEvents bottomPanel As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnSelectAll As System.Windows.Forms.Button
    Friend WithEvents btnClearSelection As System.Windows.Forms.Button
    Friend WithEvents btnInvertSelection As System.Windows.Forms.Button
    Friend WithEvents btnSelectG1 As System.Windows.Forms.Button
    Friend WithEvents btnSelectG2 As System.Windows.Forms.Button
    Friend WithEvents lblRangeStart As System.Windows.Forms.Label
    Friend WithEvents txtRangeStart As System.Windows.Forms.TextBox
    Friend WithEvents lblRangeEnd As System.Windows.Forms.Label
    Friend WithEvents txtRangeEnd As System.Windows.Forms.TextBox
    Friend WithEvents btnAddRange As System.Windows.Forms.Button
    Friend WithEvents lblChannelList As System.Windows.Forms.Label
    Friend WithEvents txtChannelList As System.Windows.Forms.TextBox
    Friend WithEvents btnAddList As System.Windows.Forms.Button
    Friend WithEvents lblSummary As System.Windows.Forms.Label
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblRepeatCount As System.Windows.Forms.Label
    Friend WithEvents nudRepeatCount As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnMultiple As System.Windows.Forms.Button
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents channelToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents channelColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents groupColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents voltageColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents resistanceColumn As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents resultColumn As System.Windows.Forms.DataGridViewTextBoxColumn

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.rootLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.commandPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnSelectAll = New System.Windows.Forms.Button()
        Me.btnClearSelection = New System.Windows.Forms.Button()
        Me.btnInvertSelection = New System.Windows.Forms.Button()
        Me.btnSelectG1 = New System.Windows.Forms.Button()
        Me.btnSelectG2 = New System.Windows.Forms.Button()
        Me.lblRangeStart = New System.Windows.Forms.Label()
        Me.txtRangeStart = New System.Windows.Forms.TextBox()
        Me.lblRangeEnd = New System.Windows.Forms.Label()
        Me.txtRangeEnd = New System.Windows.Forms.TextBox()
        Me.btnAddRange = New System.Windows.Forms.Button()
        Me.lblChannelList = New System.Windows.Forms.Label()
        Me.txtChannelList = New System.Windows.Forms.TextBox()
        Me.btnAddList = New System.Windows.Forms.Button()
        Me.lblSummary = New System.Windows.Forms.Label()
        Me.matrixPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.resultGrid = New System.Windows.Forms.DataGridView()
        Me.Channel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Group = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Voltage = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Resistance = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Result = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.bottomPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblRepeatCount = New System.Windows.Forms.Label()
        Me.nudRepeatCount = New System.Windows.Forms.NumericUpDown()
        Me.btnMultiple = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.channelToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.lblBarcodeStatus = New System.Windows.Forms.Label()
        Me.batteryCodeColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.txtTrayCode = New System.Windows.Forms.TextBox()
        Me.lblTrayCode = New System.Windows.Forms.Label()
        Me.btnGetBarcode = New System.Windows.Forms.Button()
        Me.rootLayoutPanel.SuspendLayout()
        Me.commandPanel.SuspendLayout()
        CType(Me.resultGrid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.bottomPanel.SuspendLayout()
        CType(Me.nudRepeatCount, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'rootLayoutPanel
        '
        Me.rootLayoutPanel.ColumnCount = 1
        Me.rootLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rootLayoutPanel.Controls.Add(Me.commandPanel, 0, 0)
        Me.rootLayoutPanel.Controls.Add(Me.matrixPanel, 0, 1)
        Me.rootLayoutPanel.Controls.Add(Me.resultGrid, 0, 2)
        Me.rootLayoutPanel.Controls.Add(Me.bottomPanel, 0, 3)
        Me.rootLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rootLayoutPanel.Location = New System.Drawing.Point(0, 0)
        Me.rootLayoutPanel.Name = "rootLayoutPanel"
        Me.rootLayoutPanel.RowCount = 4
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 101.0!))
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58.0!))
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.0!))
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        Me.rootLayoutPanel.Size = New System.Drawing.Size(1459, 936)
        Me.rootLayoutPanel.TabIndex = 0
        '
        'commandPanel
        '
        Me.commandPanel.AutoScroll = True
        Me.commandPanel.Controls.Add(Me.btnSelectAll)
        Me.commandPanel.Controls.Add(Me.btnClearSelection)
        Me.commandPanel.Controls.Add(Me.btnInvertSelection)
        Me.commandPanel.Controls.Add(Me.btnSelectG1)
        Me.commandPanel.Controls.Add(Me.btnSelectG2)
        Me.commandPanel.Controls.Add(Me.lblRangeStart)
        Me.commandPanel.Controls.Add(Me.txtRangeStart)
        Me.commandPanel.Controls.Add(Me.lblRangeEnd)
        Me.commandPanel.Controls.Add(Me.txtRangeEnd)
        Me.commandPanel.Controls.Add(Me.btnAddRange)
        Me.commandPanel.Controls.Add(Me.lblChannelList)
        Me.commandPanel.Controls.Add(Me.txtChannelList)
        Me.commandPanel.Controls.Add(Me.btnAddList)
        Me.commandPanel.Controls.Add(Me.lblTrayCode)
        Me.commandPanel.Controls.Add(Me.txtTrayCode)
        Me.commandPanel.Controls.Add(Me.btnGetBarcode)
        Me.commandPanel.Controls.Add(Me.lblSummary)
        Me.commandPanel.Controls.Add(Me.lblBarcodeStatus)
        Me.commandPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.commandPanel.Location = New System.Drawing.Point(3, 3)
        Me.commandPanel.Name = "commandPanel"
        Me.commandPanel.Padding = New System.Windows.Forms.Padding(10)
        Me.commandPanel.Size = New System.Drawing.Size(1453, 95)
        Me.commandPanel.TabIndex = 0
        '
        'btnSelectAll
        '
        Me.btnSelectAll.Location = New System.Drawing.Point(182, 13)
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.Size = New System.Drawing.Size(64, 30)
        Me.btnSelectAll.TabIndex = 0
        Me.btnSelectAll.Text = "全选"
        '
        'btnClearSelection
        '
        Me.btnClearSelection.Location = New System.Drawing.Point(270, 13)
        Me.btnClearSelection.Name = "btnClearSelection"
        Me.btnClearSelection.Size = New System.Drawing.Size(64, 30)
        Me.btnClearSelection.TabIndex = 1
        Me.btnClearSelection.Text = "清空"
        '
        'btnInvertSelection
        '
        Me.btnInvertSelection.Location = New System.Drawing.Point(358, 13)
        Me.btnInvertSelection.Name = "btnInvertSelection"
        Me.btnInvertSelection.Size = New System.Drawing.Size(64, 30)
        Me.btnInvertSelection.TabIndex = 2
        Me.btnInvertSelection.Text = "反选"
        '
        'btnSelectG1
        '
        Me.btnSelectG1.Location = New System.Drawing.Point(446, 13)
        Me.btnSelectG1.Name = "btnSelectG1"
        Me.btnSelectG1.Size = New System.Drawing.Size(64, 30)
        Me.btnSelectG1.TabIndex = 3
        Me.btnSelectG1.Text = "G1全选"
        '
        'btnSelectG2
        '
        Me.btnSelectG2.Location = New System.Drawing.Point(534, 13)
        Me.btnSelectG2.Name = "btnSelectG2"
        Me.btnSelectG2.Size = New System.Drawing.Size(64, 30)
        Me.btnSelectG2.TabIndex = 4
        Me.btnSelectG2.Text = "G2全选"
        '
        'lblRangeStart
        '
        Me.lblRangeStart.AutoSize = True
        Me.lblRangeStart.Location = New System.Drawing.Point(684, 10)
        Me.lblRangeStart.Name = "lblRangeStart"
        Me.lblRangeStart.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblRangeStart.Size = New System.Drawing.Size(39, 20)
        Me.lblRangeStart.TabIndex = 7
        Me.lblRangeStart.Text = "起始"
        '
        'txtRangeStart
        '
        Me.txtRangeStart.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtRangeStart.Location = New System.Drawing.Point(622, 13)
        Me.txtRangeStart.Name = "txtRangeStart"
        Me.txtRangeStart.Size = New System.Drawing.Size(48, 23)
        Me.txtRangeStart.TabIndex = 8
        '
        'lblRangeEnd
        '
        Me.lblRangeEnd.AutoSize = True
        Me.lblRangeEnd.Location = New System.Drawing.Point(729, 10)
        Me.lblRangeEnd.Name = "lblRangeEnd"
        Me.lblRangeEnd.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblRangeEnd.Size = New System.Drawing.Size(39, 20)
        Me.lblRangeEnd.TabIndex = 9
        Me.lblRangeEnd.Text = "结束"
        '
        'txtRangeEnd
        '
        Me.txtRangeEnd.Location = New System.Drawing.Point(774, 13)
        Me.txtRangeEnd.Name = "txtRangeEnd"
        Me.txtRangeEnd.Size = New System.Drawing.Size(48, 21)
        Me.txtRangeEnd.TabIndex = 10
        '
        'btnAddRange
        '
        Me.btnAddRange.Location = New System.Drawing.Point(836, 13)
        Me.btnAddRange.Name = "btnAddRange"
        Me.btnAddRange.Size = New System.Drawing.Size(76, 30)
        Me.btnAddRange.TabIndex = 11
        Me.btnAddRange.Text = "添加范围"
        '
        'lblChannelList
        '
        Me.lblChannelList.AutoSize = True
        Me.lblChannelList.Location = New System.Drawing.Point(924, 10)
        Me.lblChannelList.Name = "lblChannelList"
        Me.lblChannelList.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblChannelList.Size = New System.Drawing.Size(63, 20)
        Me.lblChannelList.TabIndex = 12
        Me.lblChannelList.Text = "零星通道"
        '
        'txtChannelList
        '
        Me.txtChannelList.Location = New System.Drawing.Point(1009, 13)
        Me.txtChannelList.Name = "txtChannelList"
        Me.txtChannelList.Size = New System.Drawing.Size(100, 21)
        Me.txtChannelList.TabIndex = 13
        '
        'btnAddList
        '
        Me.btnAddList.Location = New System.Drawing.Point(13, 13)
        Me.btnAddList.Name = "btnAddList"
        Me.btnAddList.Size = New System.Drawing.Size(76, 30)
        Me.btnAddList.TabIndex = 14
        Me.btnAddList.Text = "添加列表"
        '
        'lblSummary
        '
        Me.lblSummary.AutoSize = True
        Me.lblSummary.Location = New System.Drawing.Point(993, 10)
        Me.lblSummary.Name = "lblSummary"
        Me.lblSummary.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblSummary.Size = New System.Drawing.Size(10, 20)
        Me.lblSummary.TabIndex = 15
        '
        'matrixPanel
        '
        Me.matrixPanel.ColumnCount = 16
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.matrixPanel.Location = New System.Drawing.Point(3, 104)
        Me.matrixPanel.Name = "matrixPanel"
        Me.matrixPanel.Padding = New System.Windows.Forms.Padding(10)
        Me.matrixPanel.RowCount = 16
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.matrixPanel.Size = New System.Drawing.Size(1453, 451)
        Me.matrixPanel.TabIndex = 1
        '
        'resultGrid
        '
        Me.resultGrid.AllowUserToAddRows = False
        Me.resultGrid.AllowUserToDeleteRows = False
        Me.resultGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.resultGrid.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Channel, Me.Group, Me.batteryCodeColumn, Me.Voltage, Me.Resistance, Me.Result})
        Me.resultGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.resultGrid.Location = New System.Drawing.Point(3, 561)
        Me.resultGrid.Name = "resultGrid"
        Me.resultGrid.ReadOnly = True
        Me.resultGrid.RowHeadersVisible = False
        Me.resultGrid.Size = New System.Drawing.Size(1453, 325)
        Me.resultGrid.TabIndex = 2
        '
        'Channel
        '
        Me.Channel.HeaderText = "通道"
        Me.Channel.Name = "Channel"
        Me.Channel.ReadOnly = True
        '
        'Group
        '
        Me.Group.HeaderText = "分组"
        Me.Group.Name = "Group"
        Me.Group.ReadOnly = True
        '
        'Voltage
        '
        Me.Voltage.HeaderText = "电压 mV"
        Me.Voltage.Name = "Voltage"
        Me.Voltage.ReadOnly = True
        '
        'Resistance
        '
        Me.Resistance.HeaderText = "内阻 mΩ"
        Me.Resistance.Name = "Resistance"
        Me.Resistance.ReadOnly = True
        '
        'Result
        '
        Me.Result.HeaderText = "判定"
        Me.Result.Name = "Result"
        Me.Result.ReadOnly = True
        '
        'bottomPanel
        '
        Me.bottomPanel.ColumnCount = 7
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62.0!))
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 66.0!))
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112.0!))
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112.0!))
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112.0!))
        Me.bottomPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92.0!))
        Me.bottomPanel.Controls.Add(Me.lblStatus, 0, 0)
        Me.bottomPanel.Controls.Add(Me.lblRepeatCount, 1, 0)
        Me.bottomPanel.Controls.Add(Me.nudRepeatCount, 2, 0)
        Me.bottomPanel.Controls.Add(Me.btnMultiple, 3, 0)
        Me.bottomPanel.Controls.Add(Me.btnExport, 4, 0)
        Me.bottomPanel.Controls.Add(Me.btnStart, 5, 0)
        Me.bottomPanel.Controls.Add(Me.btnClose, 6, 0)
        Me.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.bottomPanel.Location = New System.Drawing.Point(3, 892)
        Me.bottomPanel.Name = "bottomPanel"
        Me.bottomPanel.Padding = New System.Windows.Forms.Padding(10, 7, 10, 7)
        Me.bottomPanel.RowCount = 1
        Me.bottomPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.bottomPanel.Size = New System.Drawing.Size(1453, 41)
        Me.bottomPanel.TabIndex = 3
        '
        'lblStatus
        '
        Me.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblStatus.Location = New System.Drawing.Point(13, 7)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(871, 27)
        Me.lblStatus.TabIndex = 0
        Me.lblStatus.Text = "空闲"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblRepeatCount
        '
        Me.lblRepeatCount.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRepeatCount.Location = New System.Drawing.Point(890, 7)
        Me.lblRepeatCount.Name = "lblRepeatCount"
        Me.lblRepeatCount.Size = New System.Drawing.Size(56, 27)
        Me.lblRepeatCount.TabIndex = 1
        Me.lblRepeatCount.Text = "重复次数"
        Me.lblRepeatCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'nudRepeatCount
        '
        Me.nudRepeatCount.Dock = System.Windows.Forms.DockStyle.Fill
        Me.nudRepeatCount.Location = New System.Drawing.Point(952, 10)
        Me.nudRepeatCount.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudRepeatCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudRepeatCount.Name = "nudRepeatCount"
        Me.nudRepeatCount.Size = New System.Drawing.Size(60, 21)
        Me.nudRepeatCount.TabIndex = 2
        Me.nudRepeatCount.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'btnMultiple
        '
        Me.btnMultiple.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnMultiple.Location = New System.Drawing.Point(1018, 10)
        Me.btnMultiple.Name = "btnMultiple"
        Me.btnMultiple.Size = New System.Drawing.Size(106, 21)
        Me.btnMultiple.TabIndex = 3
        Me.btnMultiple.Text = "多次测试"
        '
        'btnExport
        '
        Me.btnExport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnExport.Enabled = False
        Me.btnExport.Location = New System.Drawing.Point(1130, 10)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(106, 21)
        Me.btnExport.TabIndex = 4
        Me.btnExport.Text = "导出CSV"
        '
        'btnStart
        '
        Me.btnStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStart.Location = New System.Drawing.Point(1242, 10)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(106, 21)
        Me.btnStart.TabIndex = 5
        Me.btnStart.Text = "开始测试"
        '
        'btnClose
        '
        Me.btnClose.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnClose.Location = New System.Drawing.Point(1354, 10)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(86, 21)
        Me.btnClose.TabIndex = 6
        Me.btnClose.Text = "关闭"
        '
        'txtTrayCode
        '
        Me.txtTrayCode.Location = New System.Drawing.Point(1184, 13)
        Me.txtTrayCode.Name = "txtTrayCode"
        Me.txtTrayCode.Size = New System.Drawing.Size(145, 21)
        Me.txtTrayCode.TabIndex = 17
        '
        'lblTrayCode
        '
        Me.lblTrayCode.AutoSize = True
        Me.lblTrayCode.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblTrayCode.Location = New System.Drawing.Point(1129, 10)
        Me.lblTrayCode.Name = "lblTrayCode"
        Me.lblTrayCode.Size = New System.Drawing.Size(49, 14)
        Me.lblTrayCode.TabIndex = 18
        Me.lblTrayCode.Text = "托盘码"
        Me.lblTrayCode.TextAlign = System.Drawing.ContentAlignment.BottomRight
        '
        'btnGetBarcode
        '
        Me.btnGetBarcode.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnGetBarcode.Location = New System.Drawing.Point(1363, 13)
        Me.btnGetBarcode.Name = "btnGetBarcode"
        Me.btnGetBarcode.Size = New System.Drawing.Size(75, 23)
        Me.btnGetBarcode.TabIndex = 19
        Me.btnGetBarcode.Text = "获取条码"
        Me.btnGetBarcode.UseVisualStyleBackColor = True
        '
        Me.batteryCodeColumn.Name = "BatteryCode"
        Me.batteryCodeColumn.HeaderText = "电芯条码"
        Me.batteryCodeColumn.ReadOnly = True
        Me.batteryCodeColumn.FillWeight = 180.0!
        Me.lblBarcodeStatus.Name = "lblBarcodeStatus"
        Me.lblBarcodeStatus.AutoSize = True
        Me.lblBarcodeStatus.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblBarcodeStatus.Text = "请获取当前托盘的电芯条码"
        Me.commandPanel.SetFlowBreak(Me.btnGetBarcode, True)
        'FrmManualTest
        '
        Me.ClientSize = New System.Drawing.Size(1459, 936)
        Me.Controls.Add(Me.rootLayoutPanel)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(1225, 850)
        Me.Name = "FrmManualTest"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "手动测试"
        Me.rootLayoutPanel.ResumeLayout(False)
        Me.commandPanel.ResumeLayout(False)
        Me.commandPanel.PerformLayout()
        CType(Me.resultGrid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.bottomPanel.ResumeLayout(False)
        CType(Me.nudRepeatCount, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Channel As DataGridViewTextBoxColumn
    Friend WithEvents Group As DataGridViewTextBoxColumn
    Friend WithEvents Voltage As DataGridViewTextBoxColumn
    Friend WithEvents Resistance As DataGridViewTextBoxColumn
    Friend WithEvents Result As DataGridViewTextBoxColumn
    Friend WithEvents batteryCodeColumn As DataGridViewTextBoxColumn
    Friend WithEvents lblBarcodeStatus As Label
    Friend WithEvents txtTrayCode As TextBox
    Friend WithEvents lblTrayCode As Label
    Friend WithEvents btnGetBarcode As Button
End Class
