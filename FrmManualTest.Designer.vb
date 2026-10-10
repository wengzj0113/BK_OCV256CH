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
    Friend WithEvents btnClampClose As System.Windows.Forms.Button
    Friend WithEvents btnClampOpen As System.Windows.Forms.Button
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
        If disposing AndAlso components IsNot Nothing Then components.Dispose()
        MyBase.Dispose(disposing)
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
        Me.btnClampClose = New System.Windows.Forms.Button()
        Me.btnClampOpen = New System.Windows.Forms.Button()
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
        Me.channelColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.groupColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.voltageColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.resistanceColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.resultColumn = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.bottomPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblRepeatCount = New System.Windows.Forms.Label()
        Me.nudRepeatCount = New System.Windows.Forms.NumericUpDown()
        Me.btnMultiple = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.channelToolTip = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.resultGrid, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudRepeatCount, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.rootLayoutPanel.SuspendLayout()
        Me.commandPanel.SuspendLayout()
        Me.bottomPanel.SuspendLayout()
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
        Me.rootLayoutPanel.Name = "rootLayoutPanel"
        Me.rootLayoutPanel.RowCount = 4
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112.0!))
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58.0!))
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.0!))
        Me.rootLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
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
        Me.commandPanel.Controls.Add(Me.btnClampClose)
        Me.commandPanel.Controls.Add(Me.btnClampOpen)
        Me.commandPanel.Controls.Add(Me.lblRangeStart)
        Me.commandPanel.Controls.Add(Me.txtRangeStart)
        Me.commandPanel.Controls.Add(Me.lblRangeEnd)
        Me.commandPanel.Controls.Add(Me.txtRangeEnd)
        Me.commandPanel.Controls.Add(Me.btnAddRange)
        Me.commandPanel.Controls.Add(Me.lblChannelList)
        Me.commandPanel.Controls.Add(Me.txtChannelList)
        Me.commandPanel.Controls.Add(Me.btnAddList)
        Me.commandPanel.Controls.Add(Me.lblSummary)
        Me.commandPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.commandPanel.Name = "commandPanel"
        Me.commandPanel.Padding = New System.Windows.Forms.Padding(10)
        Me.commandPanel.TabIndex = 0
        '
        'btnSelectAll
        '
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.Size = New System.Drawing.Size(82, 30)
        Me.btnSelectAll.TabIndex = 0
        Me.btnSelectAll.Text = "全选"
        AddHandler Me.btnSelectAll.Click, AddressOf Me.SelectAllChannels
        '
        'btnClearSelection
        '
        Me.btnClearSelection.Name = "btnClearSelection"
        Me.btnClearSelection.Size = New System.Drawing.Size(82, 30)
        Me.btnClearSelection.TabIndex = 1
        Me.btnClearSelection.Text = "清空"
        AddHandler Me.btnClearSelection.Click, AddressOf Me.ClearSelection
        '
        'btnInvertSelection
        '
        Me.btnInvertSelection.Name = "btnInvertSelection"
        Me.btnInvertSelection.Size = New System.Drawing.Size(82, 30)
        Me.btnInvertSelection.TabIndex = 2
        Me.btnInvertSelection.Text = "反选"
        AddHandler Me.btnInvertSelection.Click, AddressOf Me.InvertSelection
        '
        'btnSelectG1
        '
        Me.btnSelectG1.Name = "btnSelectG1"
        Me.btnSelectG1.Size = New System.Drawing.Size(82, 30)
        Me.btnSelectG1.TabIndex = 3
        Me.btnSelectG1.Text = "G1全选"
        AddHandler Me.btnSelectG1.Click, AddressOf Me.SelectG1Channels
        '
        'btnSelectG2
        '
        Me.btnSelectG2.Name = "btnSelectG2"
        Me.btnSelectG2.Size = New System.Drawing.Size(82, 30)
        Me.btnSelectG2.TabIndex = 4
        Me.btnSelectG2.Text = "G2全选"
        AddHandler Me.btnSelectG2.Click, AddressOf Me.SelectG2Channels
        '
        'btnClampClose
        '
        Me.btnClampClose.Name = "btnClampClose"
        Me.btnClampClose.Size = New System.Drawing.Size(82, 30)
        Me.btnClampClose.TabIndex = 5
        Me.btnClampClose.Text = "夹具压合"
        AddHandler Me.btnClampClose.Click, AddressOf Me.ClampClose
        '
        'btnClampOpen
        '
        Me.btnClampOpen.Name = "btnClampOpen"
        Me.btnClampOpen.Size = New System.Drawing.Size(82, 30)
        Me.btnClampOpen.TabIndex = 6
        Me.btnClampOpen.Text = "夹具松开"
        AddHandler Me.btnClampOpen.Click, AddressOf Me.ClampOpen
        '
        'lblRangeStart
        '
        Me.lblRangeStart.AutoSize = True
        Me.lblRangeStart.Name = "lblRangeStart"
        Me.lblRangeStart.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblRangeStart.TabIndex = 7
        Me.lblRangeStart.Text = "起始"
        '
        'txtRangeStart
        '
        Me.txtRangeStart.Name = "txtRangeStart"
        Me.txtRangeStart.Size = New System.Drawing.Size(56, 21)
        Me.txtRangeStart.TabIndex = 8
        '
        'lblRangeEnd
        '
        Me.lblRangeEnd.AutoSize = True
        Me.lblRangeEnd.Name = "lblRangeEnd"
        Me.lblRangeEnd.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblRangeEnd.TabIndex = 9
        Me.lblRangeEnd.Text = "结束"
        '
        'txtRangeEnd
        '
        Me.txtRangeEnd.Name = "txtRangeEnd"
        Me.txtRangeEnd.Size = New System.Drawing.Size(56, 21)
        Me.txtRangeEnd.TabIndex = 10
        '
        'btnAddRange
        '
        Me.btnAddRange.Name = "btnAddRange"
        Me.btnAddRange.Size = New System.Drawing.Size(82, 30)
        Me.btnAddRange.TabIndex = 11
        Me.btnAddRange.Text = "添加范围"
        AddHandler Me.btnAddRange.Click, AddressOf Me.AddRangeSelection
        '
        'lblChannelList
        '
        Me.lblChannelList.AutoSize = True
        Me.lblChannelList.Name = "lblChannelList"
        Me.lblChannelList.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblChannelList.TabIndex = 12
        Me.lblChannelList.Text = "零星通道"
        '
        'txtChannelList
        '
        Me.txtChannelList.Name = "txtChannelList"
        Me.txtChannelList.Size = New System.Drawing.Size(260, 21)
        Me.txtChannelList.TabIndex = 13
        '
        'btnAddList
        '
        Me.btnAddList.Name = "btnAddList"
        Me.btnAddList.Size = New System.Drawing.Size(82, 30)
        Me.btnAddList.TabIndex = 14
        Me.btnAddList.Text = "添加列表"
        AddHandler Me.btnAddList.Click, AddressOf Me.AddListSelection
        '
        'lblSummary
        '
        Me.lblSummary.AutoSize = True
        Me.lblSummary.Name = "lblSummary"
        Me.lblSummary.Padding = New System.Windows.Forms.Padding(10, 8, 0, 0)
        Me.lblSummary.TabIndex = 15
        '
        'matrixPanel
        '
        Me.matrixPanel.ColumnCount = 16
        For columnIndex As Integer = 0 To 15
            Me.matrixPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
            Me.matrixPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Next
        Me.matrixPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.matrixPanel.Name = "matrixPanel"
        Me.matrixPanel.Padding = New System.Windows.Forms.Padding(10)
        Me.matrixPanel.RowCount = 16
        Me.matrixPanel.TabIndex = 1
        '
        'resultGrid
        '
        Me.resultGrid.AllowUserToAddRows = False
        Me.resultGrid.AllowUserToDeleteRows = False
        Me.resultGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.resultGrid.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.channelColumn, Me.groupColumn, Me.voltageColumn, Me.resistanceColumn, Me.resultColumn})
        Me.resultGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.resultGrid.Name = "resultGrid"
        Me.resultGrid.ReadOnly = True
        Me.resultGrid.RowHeadersVisible = False
        Me.resultGrid.TabIndex = 2
        '
        'channelColumn
        '
        Me.channelColumn.HeaderText = "通道"
        Me.channelColumn.Name = "Channel"
        '
        'groupColumn
        '
        Me.groupColumn.HeaderText = "分组"
        Me.groupColumn.Name = "Group"
        '
        'voltageColumn
        '
        Me.voltageColumn.HeaderText = "电压 mV"
        Me.voltageColumn.Name = "Voltage"
        '
        'resistanceColumn
        '
        Me.resistanceColumn.HeaderText = "内阻 mΩ"
        Me.resistanceColumn.Name = "Resistance"
        '
        'resultColumn
        '
        Me.resultColumn.HeaderText = "判定"
        Me.resultColumn.Name = "Result"
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
        Me.bottomPanel.Name = "bottomPanel"
        Me.bottomPanel.Padding = New System.Windows.Forms.Padding(10, 7, 10, 7)
        Me.bottomPanel.RowCount = 1
        Me.bottomPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.bottomPanel.TabIndex = 3
        '
        'lblStatus
        '
        Me.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.TabIndex = 0
        Me.lblStatus.Text = "空闲"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblRepeatCount
        '
        Me.lblRepeatCount.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRepeatCount.Name = "lblRepeatCount"
        Me.lblRepeatCount.TabIndex = 1
        Me.lblRepeatCount.Text = "重复次数"
        Me.lblRepeatCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'nudRepeatCount
        '
        Me.nudRepeatCount.Dock = System.Windows.Forms.DockStyle.Fill
        Me.nudRepeatCount.Increment = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudRepeatCount.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.nudRepeatCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudRepeatCount.Name = "nudRepeatCount"
        Me.nudRepeatCount.TabIndex = 2
        Me.nudRepeatCount.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'btnMultiple
        '
        Me.btnMultiple.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnMultiple.Name = "btnMultiple"
        Me.btnMultiple.TabIndex = 3
        Me.btnMultiple.Text = "多次测试"
        AddHandler Me.btnMultiple.Click, AddressOf Me.StartMultipleManualTests
        '
        'btnExport
        '
        Me.btnExport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnExport.Enabled = False
        Me.btnExport.Name = "btnExport"
        Me.btnExport.TabIndex = 4
        Me.btnExport.Text = "导出CSV"
        AddHandler Me.btnExport.Click, AddressOf Me.ExportManualTestResults
        '
        'btnStart
        '
        Me.btnStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStart.Name = "btnStart"
        Me.btnStart.TabIndex = 5
        Me.btnStart.Text = "开始测试"
        AddHandler Me.btnStart.Click, AddressOf Me.StartManualTest
        '
        'btnClose
        '
        Me.btnClose.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnClose.Name = "btnClose"
        Me.btnClose.TabIndex = 6
        Me.btnClose.Text = "关闭"
        AddHandler Me.btnClose.Click, Sub(sender, e) Me.Close()
        '
        'FrmManualTest
        '
        Me.Controls.Add(Me.rootLayoutPanel)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(1225, 850)
        Me.Name = "FrmManualTest"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "手动测试"
        Me.Size = New System.Drawing.Size(1475, 975)
        CType(Me.resultGrid, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudRepeatCount, System.ComponentModel.ISupportInitialize).EndInit()
        Me.rootLayoutPanel.ResumeLayout(False)
        Me.commandPanel.ResumeLayout(False)
        Me.commandPanel.PerformLayout()
        Me.bottomPanel.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub
End Class
