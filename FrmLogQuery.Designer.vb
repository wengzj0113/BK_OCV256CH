Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmLogQuery
    Inherits Form

    Private components As System.ComponentModel.IContainer
    Friend WithEvents rootLayout As TableLayoutPanel
    Friend WithEvents headerPanel As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents btnRefresh As Button
    Friend WithEvents bodyLayout As TableLayoutPanel
    Friend WithEvents railPanel As Panel
    Friend WithEvents lblCategoryCaption As Label
    Friend WithEvents btnAll As Button
    Friend WithEvents btnAlarms As Button
    Friend WithEvents btnOperations As Button
    Friend WithEvents btnCalibration As Button
    Friend WithEvents lblAllCount As Label
    Friend WithEvents lblAlarmCount As Label
    Friend WithEvents lblOperationCount As Label
    Friend WithEvents lblCalibrationCount As Label
    Friend WithEvents lblQuickCaption As Label
    Friend WithEvents btnToday As Button
    Friend WithEvents btnAbnormal As Button
    Friend WithEvents lblStorageInfo As Label
    Friend WithEvents workspaceLayout As TableLayoutPanel
    Friend WithEvents filterPanel As Panel
    Friend WithEvents filterLayout As TableLayoutPanel
    Friend WithEvents lblFrom As Label
    Friend WithEvents lblTo As Label
    Friend WithEvents lblLevel As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents lblKeyword As Label
    Friend WithEvents dateFrom As DateTimePicker
    Friend WithEvents dateTo As DateTimePicker
    Friend WithEvents levelFilter As ComboBox
    Friend WithEvents userFilter As ComboBox
    Friend WithEvents keywordFilter As TextBox
    Friend WithEvents btnSearch As Button
    Friend WithEvents btnReset As Button
    Friend WithEvents btnExport As Button
    Friend WithEvents statsLayout As TableLayoutPanel
    Friend WithEvents resultStatValue As Label
    Friend WithEvents alarmStatValue As Label
    Friend WithEvents operationStatValue As Label
    Friend WithEvents calibrationStatValue As Label
    Friend WithEvents statPanelResult As Panel
    Friend WithEvents statPanelAlarm As Panel
    Friend WithEvents statPanelOperation As Panel
    Friend WithEvents statPanelCalibration As Panel
    Friend WithEvents lblStatResult As Label
    Friend WithEvents lblStatAlarm As Label
    Friend WithEvents lblStatOperation As Label
    Friend WithEvents lblStatCalibration As Label
    Friend WithEvents split As SplitContainer
    Friend WithEvents resultsLayout As TableLayoutPanel
    Friend WithEvents grid As DataGridView
    Friend WithEvents footerLayout As TableLayoutPanel
    Friend WithEvents pageLabel As Label
    Friend WithEvents pageSizeFilter As ComboBox
    Friend WithEvents btnPrev As Button
    Friend WithEvents btnNext As Button
    Friend WithEvents detailPanel As Panel
    Friend WithEvents detailLayout As TableLayoutPanel
    Friend WithEvents detailHeader As Panel
    Friend WithEvents detailTitle As Label
    Friend WithEvents detailStatus As Label
    Friend WithEvents detailFields As TableLayoutPanel
    Friend WithEvents lblDetailKeyTime As Label
    Friend WithEvents lblDetailKeyCategory As Label
    Friend WithEvents lblDetailKeyUser As Label
    Friend WithEvents lblDetailKeyTarget As Label
    Friend WithEvents lblDetailKeyAction As Label
    Friend WithEvents lblDetailKeySource As Label
    Friend WithEvents detailTime As Label
    Friend WithEvents detailCategory As Label
    Friend WithEvents detailUser As Label
    Friend WithEvents detailTarget As Label
    Friend WithEvents detailAction As Label
    Friend WithEvents detailSource As Label
    Friend WithEvents detailCaption As Label
    Friend WithEvents detailText As TextBox

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
        Me.rootLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.headerPanel = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblSubtitle = New System.Windows.Forms.Label()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.bodyLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.railPanel = New System.Windows.Forms.Panel()
        Me.lblCategoryCaption = New System.Windows.Forms.Label()
        Me.btnAll = New System.Windows.Forms.Button()
        Me.btnAlarms = New System.Windows.Forms.Button()
        Me.btnOperations = New System.Windows.Forms.Button()
        Me.btnCalibration = New System.Windows.Forms.Button()
        Me.lblAllCount = New System.Windows.Forms.Label()
        Me.lblAlarmCount = New System.Windows.Forms.Label()
        Me.lblOperationCount = New System.Windows.Forms.Label()
        Me.lblCalibrationCount = New System.Windows.Forms.Label()
        Me.lblQuickCaption = New System.Windows.Forms.Label()
        Me.btnToday = New System.Windows.Forms.Button()
        Me.btnAbnormal = New System.Windows.Forms.Button()
        Me.lblStorageInfo = New System.Windows.Forms.Label()
        Me.workspaceLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.filterPanel = New System.Windows.Forms.Panel()
        Me.filterLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.lblFrom = New System.Windows.Forms.Label()
        Me.lblTo = New System.Windows.Forms.Label()
        Me.lblLevel = New System.Windows.Forms.Label()
        Me.lblUser = New System.Windows.Forms.Label()
        Me.lblKeyword = New System.Windows.Forms.Label()
        Me.dateFrom = New System.Windows.Forms.DateTimePicker()
        Me.dateTo = New System.Windows.Forms.DateTimePicker()
        Me.levelFilter = New System.Windows.Forms.ComboBox()
        Me.userFilter = New System.Windows.Forms.ComboBox()
        Me.keywordFilter = New System.Windows.Forms.TextBox()
        Me.btnSearch = New System.Windows.Forms.Button()
        Me.btnReset = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.statsLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.resultStatValue = New System.Windows.Forms.Label()
        Me.alarmStatValue = New System.Windows.Forms.Label()
        Me.operationStatValue = New System.Windows.Forms.Label()
        Me.calibrationStatValue = New System.Windows.Forms.Label()
        Me.statPanelResult = New System.Windows.Forms.Panel()
        Me.statPanelAlarm = New System.Windows.Forms.Panel()
        Me.statPanelOperation = New System.Windows.Forms.Panel()
        Me.statPanelCalibration = New System.Windows.Forms.Panel()
        Me.lblStatResult = New System.Windows.Forms.Label()
        Me.lblStatAlarm = New System.Windows.Forms.Label()
        Me.lblStatOperation = New System.Windows.Forms.Label()
        Me.lblStatCalibration = New System.Windows.Forms.Label()
        Me.split = New System.Windows.Forms.SplitContainer()
        Me.resultsLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.grid = New System.Windows.Forms.DataGridView()
        Me.footerLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.pageLabel = New System.Windows.Forms.Label()
        Me.pageSizeFilter = New System.Windows.Forms.ComboBox()
        Me.btnPrev = New System.Windows.Forms.Button()
        Me.btnNext = New System.Windows.Forms.Button()
        Me.detailPanel = New System.Windows.Forms.Panel()
        Me.detailLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.detailHeader = New System.Windows.Forms.Panel()
        Me.detailTitle = New System.Windows.Forms.Label()
        Me.detailStatus = New System.Windows.Forms.Label()
        Me.detailFields = New System.Windows.Forms.TableLayoutPanel()
        Me.lblDetailKeyTime = New System.Windows.Forms.Label()
        Me.lblDetailKeyCategory = New System.Windows.Forms.Label()
        Me.lblDetailKeyUser = New System.Windows.Forms.Label()
        Me.lblDetailKeyTarget = New System.Windows.Forms.Label()
        Me.lblDetailKeyAction = New System.Windows.Forms.Label()
        Me.lblDetailKeySource = New System.Windows.Forms.Label()
        Me.detailTime = New System.Windows.Forms.Label()
        Me.detailCategory = New System.Windows.Forms.Label()
        Me.detailUser = New System.Windows.Forms.Label()
        Me.detailTarget = New System.Windows.Forms.Label()
        Me.detailAction = New System.Windows.Forms.Label()
        Me.detailSource = New System.Windows.Forms.Label()
        Me.detailCaption = New System.Windows.Forms.Label()
        Me.detailText = New System.Windows.Forms.TextBox()
        Me.rootLayout.SuspendLayout()
        Me.headerPanel.SuspendLayout()
        Me.bodyLayout.SuspendLayout()
        Me.railPanel.SuspendLayout()
        Me.workspaceLayout.SuspendLayout()
        Me.filterPanel.SuspendLayout()
        Me.filterLayout.SuspendLayout()
        CType(Me.split, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.split.Panel1.SuspendLayout()
        Me.split.Panel2.SuspendLayout()
        Me.split.SuspendLayout()
        Me.resultsLayout.SuspendLayout()
        CType(Me.grid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.footerLayout.SuspendLayout()
        Me.detailPanel.SuspendLayout()
        Me.detailLayout.SuspendLayout()
        Me.detailHeader.SuspendLayout()
        Me.detailFields.SuspendLayout()
        Me.SuspendLayout()
        '
        'rootLayout
        '
        Me.rootLayout.ColumnCount = 1
        Me.rootLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rootLayout.Controls.Add(Me.headerPanel, 0, 0)
        Me.rootLayout.Controls.Add(Me.bodyLayout, 0, 1)
        Me.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rootLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.rootLayout.RowCount = 2
        Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72.0!))
        Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rootLayout.Name = "rootLayout"
        '
        'headerPanel
        '
        Me.headerPanel.BackColor = System.Drawing.Color.FromArgb(9, 38, 74)
        Me.headerPanel.Controls.Add(Me.lblTitle)
        Me.headerPanel.Controls.Add(Me.lblSubtitle)
        Me.headerPanel.Controls.Add(Me.btnRefresh)
        Me.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.headerPanel.Padding = New System.Windows.Forms.Padding(22, 8, 18, 6)
        Me.headerPanel.Name = "headerPanel"
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 17.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(22, 8)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Text = "日志管理"
        '
        'lblSubtitle
        '
        Me.lblSubtitle.AutoSize = True
        Me.lblSubtitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!)
        Me.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(205, 220, 239)
        Me.lblSubtitle.Location = New System.Drawing.Point(24, 43)
        Me.lblSubtitle.Name = "lblSubtitle"
        Me.lblSubtitle.Text = "集中查询报警、操作与校准记录"
        '
        'btnRefresh
        '
        Me.btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right
        Me.btnRefresh.BackColor = System.Drawing.Color.FromArgb(20, 57, 99)
        Me.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(67, 99, 137)
        Me.btnRefresh.ForeColor = System.Drawing.Color.White
        Me.btnRefresh.Location = New System.Drawing.Point(1256, 19)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(86, 32)
        Me.btnRefresh.Text = "↻ 刷新"
        '
        'bodyLayout
        '
        Me.bodyLayout.ColumnCount = 2
        Me.bodyLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220.0!))
        Me.bodyLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.bodyLayout.Controls.Add(Me.railPanel, 0, 0)
        Me.bodyLayout.Controls.Add(Me.workspaceLayout, 1, 0)
        Me.bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.bodyLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.bodyLayout.RowCount = 1
        Me.bodyLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.bodyLayout.Name = "bodyLayout"
        '
        'railPanel
        '
        Me.railPanel.BackColor = System.Drawing.Color.White
        Me.railPanel.Controls.Add(Me.lblCategoryCaption)
        Me.railPanel.Controls.Add(Me.btnAll)
        Me.railPanel.Controls.Add(Me.btnAlarms)
        Me.railPanel.Controls.Add(Me.btnOperations)
        Me.railPanel.Controls.Add(Me.btnCalibration)
        Me.railPanel.Controls.Add(Me.lblAllCount)
        Me.railPanel.Controls.Add(Me.lblAlarmCount)
        Me.railPanel.Controls.Add(Me.lblOperationCount)
        Me.railPanel.Controls.Add(Me.lblCalibrationCount)
        Me.railPanel.Controls.Add(Me.lblQuickCaption)
        Me.railPanel.Controls.Add(Me.btnToday)
        Me.railPanel.Controls.Add(Me.btnAbnormal)
        Me.railPanel.Controls.Add(Me.lblStorageInfo)
        Me.railPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.railPanel.Padding = New System.Windows.Forms.Padding(12, 13, 10, 12)
        Me.railPanel.Name = "railPanel"
        '
        'category navigation
        '
        Me.lblCategoryCaption.AutoSize = True
        Me.lblCategoryCaption.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblCategoryCaption.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblCategoryCaption.Location = New System.Drawing.Point(20, 18)
        Me.lblCategoryCaption.Name = "lblCategoryCaption"
        Me.lblCategoryCaption.Text = "日志分类"
        Me.btnAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAll.FlatAppearance.BorderSize = 0
        Me.btnAll.Location = New System.Drawing.Point(19, 48)
        Me.btnAll.Name = "btnAll"
        Me.btnAll.Size = New System.Drawing.Size(138, 39)
        Me.btnAll.Text = "▦    全部日志"
        Me.btnAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblAllCount.Location = New System.Drawing.Point(157, 54)
        Me.lblAllCount.Name = "lblAllCount"
        Me.lblAllCount.Size = New System.Drawing.Size(35, 27)
        Me.lblAllCount.Text = "0"
        Me.lblAllCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblAllCount.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.btnAlarms.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAlarms.FlatAppearance.BorderSize = 0
        Me.btnAlarms.Location = New System.Drawing.Point(19, 91)
        Me.btnAlarms.Name = "btnAlarms"
        Me.btnAlarms.Size = New System.Drawing.Size(138, 39)
        Me.btnAlarms.Text = "⚠    报警日志"
        Me.btnAlarms.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblAlarmCount.Location = New System.Drawing.Point(157, 97)
        Me.lblAlarmCount.Name = "lblAlarmCount"
        Me.lblAlarmCount.Size = New System.Drawing.Size(35, 27)
        Me.lblAlarmCount.Text = "0"
        Me.lblAlarmCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblAlarmCount.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.btnOperations.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnOperations.FlatAppearance.BorderSize = 0
        Me.btnOperations.Location = New System.Drawing.Point(19, 134)
        Me.btnOperations.Name = "btnOperations"
        Me.btnOperations.Size = New System.Drawing.Size(138, 39)
        Me.btnOperations.Text = "⌘    操作日志"
        Me.btnOperations.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblOperationCount.Location = New System.Drawing.Point(157, 140)
        Me.lblOperationCount.Name = "lblOperationCount"
        Me.lblOperationCount.Size = New System.Drawing.Size(35, 27)
        Me.lblOperationCount.Text = "0"
        Me.lblOperationCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblOperationCount.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.btnCalibration.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCalibration.FlatAppearance.BorderSize = 0
        Me.btnCalibration.Location = New System.Drawing.Point(19, 177)
        Me.btnCalibration.Name = "btnCalibration"
        Me.btnCalibration.Size = New System.Drawing.Size(138, 39)
        Me.btnCalibration.Text = "⊙    校准日志"
        Me.btnCalibration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblCalibrationCount.Location = New System.Drawing.Point(157, 183)
        Me.lblCalibrationCount.Name = "lblCalibrationCount"
        Me.lblCalibrationCount.Size = New System.Drawing.Size(35, 27)
        Me.lblCalibrationCount.Text = "0"
        Me.lblCalibrationCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.lblCalibrationCount.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        '
        'quick filters and storage note
        '
        Me.lblQuickCaption.AutoSize = True
        Me.lblQuickCaption.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblQuickCaption.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblQuickCaption.Location = New System.Drawing.Point(20, 232)
        Me.lblQuickCaption.Name = "lblQuickCaption"
        Me.lblQuickCaption.Text = "常用筛选"
        Me.btnToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnToday.FlatAppearance.BorderSize = 0
        Me.btnToday.Location = New System.Drawing.Point(19, 260)
        Me.btnToday.Name = "btnToday"
        Me.btnToday.Size = New System.Drawing.Size(178, 36)
        Me.btnToday.Text = "◷    今日记录"
        Me.btnToday.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnAbnormal.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAbnormal.FlatAppearance.BorderSize = 0
        Me.btnAbnormal.Location = New System.Drawing.Point(19, 302)
        Me.btnAbnormal.Name = "btnAbnormal"
        Me.btnAbnormal.Size = New System.Drawing.Size(178, 36)
        Me.btnAbnormal.Text = "!     异常与失败"
        Me.btnAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblStorageInfo.Anchor = System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left
        Me.lblStorageInfo.AutoEllipsis = True
        Me.lblStorageInfo.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblStorageInfo.Location = New System.Drawing.Point(20, 376)
        Me.lblStorageInfo.Name = "lblStorageInfo"
        Me.lblStorageInfo.Size = New System.Drawing.Size(180, 105)
        Me.lblStorageInfo.Text = "日志位置" & Global.Microsoft.VisualBasic.Constants.vbCrLf & "旧版 AlarmLog.txt 会自动纳入报警查询。"
        '
        'workspaceLayout
        '
        Me.workspaceLayout.BackColor = System.Drawing.Color.FromArgb(242, 245, 249)
        Me.workspaceLayout.ColumnCount = 1
        Me.workspaceLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.workspaceLayout.Controls.Add(Me.filterPanel, 0, 0)
        Me.workspaceLayout.Controls.Add(Me.statsLayout, 0, 1)
        Me.workspaceLayout.Controls.Add(Me.split, 0, 2)
        Me.workspaceLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.workspaceLayout.Padding = New System.Windows.Forms.Padding(15, 13, 16, 14)
        Me.workspaceLayout.RowCount = 3
        Me.workspaceLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112.0!))
        Me.workspaceLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78.0!))
        Me.workspaceLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.workspaceLayout.Name = "workspaceLayout"
        '
        'filterPanel
        '
        Me.filterPanel.BackColor = System.Drawing.Color.White
        Me.filterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.filterPanel.Controls.Add(Me.filterLayout)
        Me.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.filterPanel.Padding = New System.Windows.Forms.Padding(12, 10, 12, 8)
        Me.filterPanel.Name = "filterPanel"
        Me.filterLayout.ColumnCount = 8
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 145.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78.0!))
        Me.filterLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78.0!))
        Me.filterLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.filterLayout.RowCount = 2
        Me.filterLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24.0!))
        Me.filterLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.filterLayout.Name = "filterLayout"
        Me.filterLayout.Controls.Add(Me.lblFrom, 0, 0)
        Me.filterLayout.Controls.Add(Me.lblTo, 1, 0)
        Me.filterLayout.Controls.Add(Me.lblLevel, 2, 0)
        Me.filterLayout.Controls.Add(Me.lblUser, 3, 0)
        Me.filterLayout.Controls.Add(Me.lblKeyword, 4, 0)
        Me.filterLayout.Controls.Add(Me.dateFrom, 0, 1)
        Me.filterLayout.Controls.Add(Me.dateTo, 1, 1)
        Me.filterLayout.Controls.Add(Me.levelFilter, 2, 1)
        Me.filterLayout.Controls.Add(Me.userFilter, 3, 1)
        Me.filterLayout.Controls.Add(Me.keywordFilter, 4, 1)
        Me.filterLayout.Controls.Add(Me.btnSearch, 5, 1)
        Me.filterLayout.Controls.Add(Me.btnReset, 6, 1)
        Me.filterLayout.Controls.Add(Me.btnExport, 7, 1)
        Me.lblFrom.Text = "开始日期"
        Me.lblTo.Text = "结束日期"
        Me.lblLevel.Text = "状态"
        Me.lblUser.Text = "操作人"
        Me.lblKeyword.Text = "关键字"
        Me.lblFrom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblFrom.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblFrom.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblFrom.Name = "lblFrom"
        Me.lblTo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblTo.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblTo.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblTo.Name = "lblTo"
        Me.lblLevel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblLevel.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblLevel.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblLevel.Name = "lblLevel"
        Me.lblUser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblUser.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblUser.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblUser.Name = "lblUser"
        Me.lblKeyword.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblKeyword.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblKeyword.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblKeyword.Name = "lblKeyword"
        Me.dateFrom.CustomFormat = "yyyy-MM-dd"
        Me.dateFrom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dateFrom.Name = "dateFrom"
        Me.dateTo.CustomFormat = "yyyy-MM-dd"
        Me.dateTo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dateTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dateTo.Name = "dateTo"
        Me.levelFilter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.levelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.levelFilter.Items.AddRange(New Object() {"全部状态", "成功", "失败", "报警", "已复位", "异常"})
        Me.levelFilter.Name = "levelFilter"
        Me.userFilter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.userFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.userFilter.Items.Add("全部人员")
        Me.userFilter.Name = "userFilter"
        Me.keywordFilter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.keywordFilter.Name = "keywordFilter"
        Me.btnSearch.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnSearch.Name = "btnSearch"
        Me.btnSearch.Text = "查询"
        Me.btnSearch.BackColor = System.Drawing.Color.FromArgb(23, 105, 210)
        Me.btnSearch.ForeColor = System.Drawing.Color.White
        Me.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnReset.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnReset.Name = "btnReset"
        Me.btnReset.Text = "重置"
        Me.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnExport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Text = "导出"
        Me.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        '
        'statsLayout
        '
        Me.statsLayout.ColumnCount = 4
        Me.statsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.statsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.statsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.statsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.statsLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statsLayout.Padding = New System.Windows.Forms.Padding(0, 8, 0, 6)
        Me.statsLayout.RowCount = 1
        Me.statsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.statsLayout.Name = "statsLayout"
        Me.statsLayout.Controls.Add(Me.statPanelResult, 0, 0)
        Me.statsLayout.Controls.Add(Me.statPanelAlarm, 1, 0)
        Me.statsLayout.Controls.Add(Me.statPanelOperation, 2, 0)
        Me.statsLayout.Controls.Add(Me.statPanelCalibration, 3, 0)
        Me.statPanelResult.BackColor = System.Drawing.Color.White
        Me.statPanelResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.statPanelResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statPanelResult.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        Me.statPanelResult.Name = "statPanelResult"
        Me.statPanelResult.Controls.Add(Me.lblStatResult)
        Me.statPanelResult.Controls.Add(Me.resultStatValue)
        Me.statPanelAlarm.BackColor = System.Drawing.Color.White
        Me.statPanelAlarm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.statPanelAlarm.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statPanelAlarm.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        Me.statPanelAlarm.Name = "statPanelAlarm"
        Me.statPanelAlarm.Controls.Add(Me.lblStatAlarm)
        Me.statPanelAlarm.Controls.Add(Me.alarmStatValue)
        Me.statPanelOperation.BackColor = System.Drawing.Color.White
        Me.statPanelOperation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.statPanelOperation.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statPanelOperation.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        Me.statPanelOperation.Name = "statPanelOperation"
        Me.statPanelOperation.Controls.Add(Me.lblStatOperation)
        Me.statPanelOperation.Controls.Add(Me.operationStatValue)
        Me.statPanelCalibration.BackColor = System.Drawing.Color.White
        Me.statPanelCalibration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.statPanelCalibration.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statPanelCalibration.Margin = New System.Windows.Forms.Padding(0)
        Me.statPanelCalibration.Name = "statPanelCalibration"
        Me.statPanelCalibration.Controls.Add(Me.lblStatCalibration)
        Me.statPanelCalibration.Controls.Add(Me.calibrationStatValue)
        Me.lblStatResult.AutoSize = True
        Me.lblStatResult.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblStatResult.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblStatResult.Location = New System.Drawing.Point(10, 7)
        Me.lblStatResult.Name = "lblStatResult"
        Me.lblStatResult.Text = "当前筛选结果"
        Me.lblStatAlarm.AutoSize = True
        Me.lblStatAlarm.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblStatAlarm.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblStatAlarm.Location = New System.Drawing.Point(10, 7)
        Me.lblStatAlarm.Name = "lblStatAlarm"
        Me.lblStatAlarm.Text = "范围内报警"
        Me.lblStatOperation.AutoSize = True
        Me.lblStatOperation.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblStatOperation.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblStatOperation.Location = New System.Drawing.Point(10, 7)
        Me.lblStatOperation.Name = "lblStatOperation"
        Me.lblStatOperation.Text = "范围内操作"
        Me.lblStatCalibration.AutoSize = True
        Me.lblStatCalibration.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.lblStatCalibration.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblStatCalibration.Location = New System.Drawing.Point(10, 7)
        Me.lblStatCalibration.Name = "lblStatCalibration"
        Me.lblStatCalibration.Text = "范围内校准"
        Me.resultStatValue.AutoSize = True
        Me.resultStatValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.resultStatValue.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.resultStatValue.Location = New System.Drawing.Point(10, 26)
        Me.resultStatValue.Name = "resultStatValue"
        Me.resultStatValue.Text = "0"
        Me.alarmStatValue.AutoSize = True
        Me.alarmStatValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.alarmStatValue.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.alarmStatValue.Location = New System.Drawing.Point(10, 26)
        Me.alarmStatValue.Name = "alarmStatValue"
        Me.alarmStatValue.Text = "0"
        Me.operationStatValue.AutoSize = True
        Me.operationStatValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.operationStatValue.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.operationStatValue.Location = New System.Drawing.Point(10, 26)
        Me.operationStatValue.Name = "operationStatValue"
        Me.operationStatValue.Text = "0"
        Me.calibrationStatValue.AutoSize = True
        Me.calibrationStatValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.calibrationStatValue.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.calibrationStatValue.Location = New System.Drawing.Point(10, 26)
        Me.calibrationStatValue.Name = "calibrationStatValue"
        Me.calibrationStatValue.Text = "0"
        '
        'split and results
        '
        Me.split.Dock = System.Windows.Forms.DockStyle.Fill
        Me.split.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.split.Name = "split"
        Me.split.Orientation = System.Windows.Forms.Orientation.Vertical
        Me.split.Size = New System.Drawing.Size(1000, 500)
        Me.split.Panel1MinSize = 470
        Me.split.Panel2MinSize = 260
        Me.split.SplitterDistance = 690
        Me.split.SplitterWidth = 6
        Me.split.Panel1.Controls.Add(Me.resultsLayout)
        Me.split.Panel2.Controls.Add(Me.detailPanel)
        Me.resultsLayout.ColumnCount = 1
        Me.resultsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.resultsLayout.Controls.Add(Me.grid, 0, 0)
        Me.resultsLayout.Controls.Add(Me.footerLayout, 0, 1)
        Me.resultsLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.resultsLayout.RowCount = 2
        Me.resultsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.resultsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        Me.resultsLayout.Name = "resultsLayout"
        Me.grid.AllowUserToAddRows = False
        Me.grid.AllowUserToDeleteRows = False
        Me.grid.AllowUserToResizeRows = False
        Me.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.grid.BackgroundColor = System.Drawing.Color.White
        Me.grid.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.grid.ColumnHeadersHeight = 38
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.EnableHeadersVisualStyles = False
        Me.grid.MultiSelect = False
        Me.grid.Name = "grid"
        Me.grid.ReadOnly = True
        Me.grid.RowHeadersVisible = False
        Me.grid.RowTemplate.Height = 38
        Me.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.grid.Columns.Add("Time", "发生时间")
        Me.grid.Columns.Add("Category", "类型")
        Me.grid.Columns.Add("Level", "状态")
        Me.grid.Columns.Add("User", "操作人")
        Me.grid.Columns.Add("Target", "对象")
        Me.grid.Columns.Add("Summary", "事件摘要")
        Me.grid.Columns("Time").FillWeight = 125.0!
        Me.grid.Columns("Category").FillWeight = 58.0!
        Me.grid.Columns("Level").FillWeight = 56.0!
        Me.grid.Columns("User").FillWeight = 75.0!
        Me.grid.Columns("Target").FillWeight = 90.0!
        Me.grid.Columns("Summary").FillWeight = 205.0!
        Me.grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 249, 252)
        Me.grid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.grid.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.grid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(234, 243, 255)
        Me.grid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.grid.GridColor = System.Drawing.Color.FromArgb(237, 241, 245)
        Me.footerLayout.ColumnCount = 4
        Me.footerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.footerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96.0!))
        Me.footerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82.0!))
        Me.footerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82.0!))
        Me.footerLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.footerLayout.Name = "footerLayout"
        Me.footerLayout.Controls.Add(Me.pageLabel, 0, 0)
        Me.footerLayout.Controls.Add(Me.pageSizeFilter, 1, 0)
        Me.footerLayout.Controls.Add(Me.btnPrev, 2, 0)
        Me.footerLayout.Controls.Add(Me.btnNext, 3, 0)
        Me.pageLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pageLabel.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.pageLabel.Name = "pageLabel"
        Me.pageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.pageSizeFilter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pageSizeFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.pageSizeFilter.Items.AddRange(New Object() {"50 条/页", "100 条/页", "200 条/页"})
        Me.pageSizeFilter.Name = "pageSizeFilter"
        Me.btnPrev.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnPrev.Name = "btnPrev"
        Me.btnPrev.Text = "‹ 上一页"
        Me.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnNext.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnNext.Name = "btnNext"
        Me.btnNext.Text = "下一页 ›"
        Me.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        '
        'detailPanel
        '
        Me.detailPanel.BackColor = System.Drawing.Color.White
        Me.detailPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.detailPanel.Controls.Add(Me.detailLayout)
        Me.detailPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailPanel.Padding = New System.Windows.Forms.Padding(14)
        Me.detailPanel.Name = "detailPanel"
        Me.detailLayout.ColumnCount = 1
        Me.detailLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.detailLayout.Controls.Add(Me.detailHeader, 0, 0)
        Me.detailLayout.Controls.Add(Me.detailFields, 0, 1)
        Me.detailLayout.Controls.Add(Me.detailCaption, 0, 2)
        Me.detailLayout.Controls.Add(Me.detailText, 0, 3)
        Me.detailLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailLayout.RowCount = 4
        Me.detailLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        Me.detailLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 188.0!))
        Me.detailLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0!))
        Me.detailLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.detailLayout.Name = "detailLayout"
        Me.detailHeader.Controls.Add(Me.detailTitle)
        Me.detailHeader.Controls.Add(Me.detailStatus)
        Me.detailHeader.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailHeader.Name = "detailHeader"
        Me.detailTitle.AutoSize = True
        Me.detailTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 11.0!, System.Drawing.FontStyle.Bold)
        Me.detailTitle.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailTitle.Location = New System.Drawing.Point(0, 1)
        Me.detailTitle.Name = "detailTitle"
        Me.detailTitle.Text = "事件详情"
        Me.detailStatus.AutoSize = True
        Me.detailStatus.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!, System.Drawing.FontStyle.Bold)
        Me.detailStatus.ForeColor = System.Drawing.Color.FromArgb(22, 132, 91)
        Me.detailStatus.Location = New System.Drawing.Point(0, 27)
        Me.detailStatus.Name = "detailStatus"
        Me.detailFields.ColumnCount = 2
        Me.detailFields.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76.0!))
        Me.detailFields.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.detailFields.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailFields.Padding = New System.Windows.Forms.Padding(0, 4, 0, 8)
        Me.detailFields.RowCount = 6
        Me.detailFields.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.detailFields.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.detailFields.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.detailFields.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.detailFields.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.detailFields.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.detailFields.Name = "detailFields"
        Me.lblDetailKeyTime.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDetailKeyTime.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblDetailKeyTime.Name = "lblDetailKeyTime"
        Me.lblDetailKeyTime.Text = "发生时间"
        Me.lblDetailKeyTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailTime.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailTime.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailTime.Name = "detailTime"
        Me.detailTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailTime.AutoEllipsis = True
        Me.detailFields.Controls.Add(Me.lblDetailKeyTime, 0, 0)
        Me.detailFields.Controls.Add(Me.detailTime, 1, 0)
        Me.lblDetailKeyCategory.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDetailKeyCategory.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblDetailKeyCategory.Name = "lblDetailKeyCategory"
        Me.lblDetailKeyCategory.Text = "日志类型"
        Me.lblDetailKeyCategory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailCategory.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailCategory.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailCategory.Name = "detailCategory"
        Me.detailCategory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailCategory.AutoEllipsis = True
        Me.detailFields.Controls.Add(Me.lblDetailKeyCategory, 0, 1)
        Me.detailFields.Controls.Add(Me.detailCategory, 1, 1)
        Me.lblDetailKeyUser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDetailKeyUser.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblDetailKeyUser.Name = "lblDetailKeyUser"
        Me.lblDetailKeyUser.Text = "操作人员"
        Me.lblDetailKeyUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailUser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailUser.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailUser.Name = "detailUser"
        Me.detailUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailUser.AutoEllipsis = True
        Me.detailFields.Controls.Add(Me.lblDetailKeyUser, 0, 2)
        Me.detailFields.Controls.Add(Me.detailUser, 1, 2)
        Me.lblDetailKeyTarget.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDetailKeyTarget.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblDetailKeyTarget.Name = "lblDetailKeyTarget"
        Me.lblDetailKeyTarget.Text = "关联对象"
        Me.lblDetailKeyTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailTarget.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailTarget.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailTarget.Name = "detailTarget"
        Me.detailTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailTarget.AutoEllipsis = True
        Me.detailFields.Controls.Add(Me.lblDetailKeyTarget, 0, 3)
        Me.detailFields.Controls.Add(Me.detailTarget, 1, 3)
        Me.lblDetailKeyAction.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDetailKeyAction.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblDetailKeyAction.Name = "lblDetailKeyAction"
        Me.lblDetailKeyAction.Text = "操作动作"
        Me.lblDetailKeyAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailAction.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailAction.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailAction.Name = "detailAction"
        Me.detailAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailAction.AutoEllipsis = True
        Me.detailFields.Controls.Add(Me.lblDetailKeyAction, 0, 4)
        Me.detailFields.Controls.Add(Me.detailAction, 1, 4)
        Me.lblDetailKeySource.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDetailKeySource.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.lblDetailKeySource.Name = "lblDetailKeySource"
        Me.lblDetailKeySource.Text = "事件来源"
        Me.lblDetailKeySource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailSource.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailSource.ForeColor = System.Drawing.Color.FromArgb(29, 41, 57)
        Me.detailSource.Name = "detailSource"
        Me.detailSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailSource.AutoEllipsis = True
        Me.detailFields.Controls.Add(Me.lblDetailKeySource, 0, 5)
        Me.detailFields.Controls.Add(Me.detailSource, 1, 5)
        Me.detailCaption.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailCaption.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!, System.Drawing.FontStyle.Bold)
        Me.detailCaption.ForeColor = System.Drawing.Color.FromArgb(102, 117, 138)
        Me.detailCaption.Name = "detailCaption"
        Me.detailCaption.Text = "事件说明"
        Me.detailCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.detailText.BackColor = System.Drawing.Color.White
        Me.detailText.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.detailText.Dock = System.Windows.Forms.DockStyle.Fill
        Me.detailText.ForeColor = System.Drawing.Color.FromArgb(53, 70, 93)
        Me.detailText.Multiline = True
        Me.detailText.Name = "detailText"
        Me.detailText.ReadOnly = True
        Me.detailText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.detailText.Text = "选择一条日志查看完整内容。"
        '
        'FrmLogQuery
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(242, 245, 249)
        Me.ClientSize = New System.Drawing.Size(1360, 820)
        Me.Controls.Add(Me.rootLayout)
        Me.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.MinimumSize = New System.Drawing.Size(1120, 680)
        Me.Name = "FrmLogQuery"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "日志管理"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.rootLayout.ResumeLayout(False)
        Me.headerPanel.ResumeLayout(False)
        Me.headerPanel.PerformLayout()
        Me.bodyLayout.ResumeLayout(False)
        Me.railPanel.ResumeLayout(False)
        Me.railPanel.PerformLayout()
        Me.workspaceLayout.ResumeLayout(False)
        Me.filterPanel.ResumeLayout(False)
        Me.filterLayout.ResumeLayout(False)
        Me.filterLayout.PerformLayout()
        Me.split.Panel1.ResumeLayout(False)
        Me.split.Panel2.ResumeLayout(False)
        CType(Me.split, System.ComponentModel.ISupportInitialize).EndInit()
        Me.split.ResumeLayout(False)
        Me.resultsLayout.ResumeLayout(False)
        CType(Me.grid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.footerLayout.ResumeLayout(False)
        Me.detailPanel.ResumeLayout(False)
        Me.detailLayout.ResumeLayout(False)
        Me.detailHeader.ResumeLayout(False)
        Me.detailHeader.PerformLayout()
        Me.detailFields.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub
End Class
