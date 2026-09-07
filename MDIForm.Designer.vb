<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MDIForm
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub


    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MDIForm))
        Me.MenuStrip = New System.Windows.Forms.MenuStrip()
        Me.FileMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.init = New System.Windows.Forms.ToolStripMenuItem()
        Me.btnDebug = New System.Windows.Forms.ToolStripMenuItem()
        Me.userMenage = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitTool = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.TStripMenuCode = New System.Windows.Forms.ToolStripMenuItem()
        Me.OnLineTest = New System.Windows.Forms.ToolStripMenuItem()
        Me.ManualTestToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NgRetestToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TStripMenuInQuery = New System.Windows.Forms.ToolStripMenuItem()
        Me.TlStripMenuG = New System.Windows.Forms.ToolStripMenuItem()
        Me.TlStripMenuLog = New System.Windows.Forms.ToolStripMenuItem()
        Me.EMS上传ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.calibrationMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.loginMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.bottomStatusControl = New BottomStatusControl()
        Me.ToolStrip = New System.Windows.Forms.ToolStrip()
        Me.btnInit = New System.Windows.Forms.ToolStripButton()
        Me.ToolDebug = New System.Windows.Forms.ToolStripButton()
        Me.TStripbtn_Barcode = New System.Windows.Forms.ToolStripButton()
        Me.btnTest = New System.Windows.Forms.ToolStripButton()
        Me.TStripbtnInQuery = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.Com_SCM = New System.IO.Ports.SerialPort(Me.components)
        Me.Com_3561 = New System.IO.Ports.SerialPort(Me.components)
        Me.Com_scaner = New System.IO.Ports.SerialPort(Me.components)
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.Com_IOCard = New System.IO.Ports.SerialPort(Me.components)
        Me.txtTesting = New System.Windows.Forms.Label()
        Me.MenuStrip.SuspendLayout()
        Me.ToolStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStrip
        '
        Me.MenuStrip.BackColor = System.Drawing.Color.FromArgb(6, 29, 59)
        Me.MenuStrip.ForeColor = System.Drawing.Color.White
        Me.MenuStrip.Font = New System.Drawing.Font("Microsoft YaHei UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.MenuStrip.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileMenu, Me.EditMenu, Me.TStripMenuInQuery, Me.calibrationMenu, Me.EMS上传ToolStripMenuItem, Me.loginMenu})
        Me.MenuStrip.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip.Name = "MenuStrip"
        Me.MenuStrip.AutoSize = False
        Me.MenuStrip.Padding = New System.Windows.Forms.Padding(16, 0, 0, 0)
        Me.MenuStrip.Size = New System.Drawing.Size(1429, 46)
        Me.MenuStrip.TabIndex = 5
        Me.MenuStrip.Text = "MenuStrip"
        '
        'FileMenu
        '
        Me.FileMenu.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.init, Me.btnDebug, Me.userMenage, Me.ExitTool})
        Me.FileMenu.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder
        Me.FileMenu.Name = "FileMenu"
        Me.FileMenu.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.I), System.Windows.Forms.Keys)
        Me.FileMenu.Size = New System.Drawing.Size(101, 25)
        Me.FileMenu.Text = "系统设置(&I)"
        '
        'init
        '
        Me.init.Image = CType(resources.GetObject("init.Image"), System.Drawing.Image)
        Me.init.Name = "init"
        Me.init.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)
        Me.init.Size = New System.Drawing.Size(225, 26)
        Me.init.Text = "初始设置(&S)"
        '
        'btnDebug
        '
        Me.btnDebug.Image = CType(resources.GetObject("btnDebug.Image"), System.Drawing.Image)
        Me.btnDebug.Name = "btnDebug"
        Me.btnDebug.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.D), System.Windows.Forms.Keys)
        Me.btnDebug.Size = New System.Drawing.Size(225, 26)
        Me.btnDebug.Text = "调试(&D)"
        '
        'userMenage
        '
        Me.userMenage.Image = Global.software.My.Resources.Resources.search
        Me.userMenage.Name = "userMenage"
        Me.userMenage.Size = New System.Drawing.Size(225, 26)
        Me.userMenage.Text = "用户管理(&U)"
        '
        'ExitTool
        '
        Me.ExitTool.Image = Global.software.My.Resources.Resources.退出
        Me.ExitTool.Name = "ExitTool"
        Me.ExitTool.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.X), System.Windows.Forms.Keys)
        Me.ExitTool.Size = New System.Drawing.Size(225, 26)
        Me.ExitTool.Text = "退出(&X)"
        '
        'EditMenu
        '
        Me.EditMenu.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TStripMenuCode, Me.OnLineTest, Me.ManualTestToolStripMenuItem, Me.NgRetestToolStripMenuItem})
        Me.EditMenu.Name = "EditMenu"
        Me.EditMenu.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.T), System.Windows.Forms.Keys)
        Me.EditMenu.Size = New System.Drawing.Size(73, 25)
        Me.EditMenu.Text = "测试(&T)"
        '
        'TStripMenuCode
        '
        Me.TStripMenuCode.Image = CType(resources.GetObject("TStripMenuCode.Image"), System.Drawing.Image)
        Me.TStripMenuCode.Name = "TStripMenuCode"
        Me.TStripMenuCode.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.C), System.Windows.Forms.Keys)
        Me.TStripMenuCode.Size = New System.Drawing.Size(245, 26)
        Me.TStripMenuCode.Text = "扫描条形码(&C)"
        Me.TStripMenuCode.Visible = False
        '
        'OnLineTest
        '
        Me.OnLineTest.Image = Global.software.My.Resources.Resources.Pnp3
        Me.OnLineTest.Name = "OnLineTest"
        Me.OnLineTest.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.T), System.Windows.Forms.Keys)
        Me.OnLineTest.Size = New System.Drawing.Size(245, 26)
        Me.OnLineTest.Text = "联机测试(&T)"
        '
        'ManualTestToolStripMenuItem
        '
        Me.ManualTestToolStripMenuItem.Name = "ManualTestToolStripMenuItem"
        Me.ManualTestToolStripMenuItem.Size = New System.Drawing.Size(245, 26)
        Me.ManualTestToolStripMenuItem.Text = "手动测试"
        '
        'NgRetestToolStripMenuItem
        '
        Me.NgRetestToolStripMenuItem.Name = "NgRetestToolStripMenuItem"
        Me.NgRetestToolStripMenuItem.Size = New System.Drawing.Size(245, 26)
        Me.NgRetestToolStripMenuItem.Text = "NG复测"
        '
        'TStripMenuInQuery
        '
        Me.TStripMenuInQuery.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TlStripMenuG, Me.TlStripMenuLog})
        Me.TStripMenuInQuery.Name = "TStripMenuInQuery"
        Me.TStripMenuInQuery.Size = New System.Drawing.Size(54, 25)
        Me.TStripMenuInQuery.Text = "查询"
        '
        'TlStripMenuG
        '
        Me.TlStripMenuG.Image = CType(resources.GetObject("TlStripMenuG.Image"), System.Drawing.Image)
        Me.TlStripMenuG.Name = "TlStripMenuG"
        Me.TlStripMenuG.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.G), System.Windows.Forms.Keys)
        Me.TlStripMenuG.Size = New System.Drawing.Size(231, 26)
        Me.TlStripMenuG.Text = "查询统计(&G)"
        '
        'TlStripMenuLog
        '
        Me.TlStripMenuLog.Name = "TlStripMenuLog"
        Me.TlStripMenuLog.Size = New System.Drawing.Size(231, 26)
        Me.TlStripMenuLog.Text = "日志查询"
        '
        'EMS上传ToolStripMenuItem
        '
        Me.EMS上传ToolStripMenuItem.Name = "EMS上传ToolStripMenuItem"
        Me.EMS上传ToolStripMenuItem.Size = New System.Drawing.Size(88, 25)
        Me.EMS上传ToolStripMenuItem.Text = "MES上传"
        '
        'calibrationMenu
        '
        Me.calibrationMenu.Name = "calibrationMenu"
        Me.calibrationMenu.Size = New System.Drawing.Size(80, 42)
        Me.calibrationMenu.Text = "校准"
        Me.calibrationMenu.ToolTipText = "通道校准"
        '
        'loginMenu
        '
        Me.loginMenu.Name = "loginMenu"
        Me.loginMenu.Size = New System.Drawing.Size(80, 42)
        Me.loginMenu.Text = "登录"
        Me.loginMenu.ToolTipText = "登录或切换当前用户"
        Me.FileMenu.ForeColor = System.Drawing.Color.White
        Me.EditMenu.ForeColor = System.Drawing.Color.White
        Me.TStripMenuInQuery.ForeColor = System.Drawing.Color.White
        Me.calibrationMenu.ForeColor = System.Drawing.Color.White
        Me.EMS上传ToolStripMenuItem.ForeColor = System.Drawing.Color.White
        Me.loginMenu.ForeColor = System.Drawing.Color.White
        Me.FileMenu.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        Me.EditMenu.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        Me.TStripMenuInQuery.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        Me.calibrationMenu.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        Me.EMS上传ToolStripMenuItem.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        Me.loginMenu.Margin = New System.Windows.Forms.Padding(8, 0, 8, 0)
        '
        'ToolStrip
        '
        Me.ToolStrip.ImageScalingSize = New System.Drawing.Size(44, 44)
        Me.ToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnInit, Me.ToolDebug, Me.TStripbtn_Barcode, Me.btnTest, Me.TStripbtnInQuery, Me.ToolStripSeparator1, Me.ToolStripButton1})
        Me.ToolStrip.Location = New System.Drawing.Point(0, 29)
        Me.ToolStrip.Name = "ToolStrip"
        Me.ToolStrip.Size = New System.Drawing.Size(1429, 51)
        Me.ToolStrip.TabIndex = 6
        Me.ToolStrip.Text = "ToolStrip"
        '
        'btnInit
        '
        Me.btnInit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnInit.Image = CType(resources.GetObject("btnInit.Image"), System.Drawing.Image)
        Me.btnInit.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnInit.Name = "btnInit"
        Me.btnInit.Size = New System.Drawing.Size(48, 48)
        Me.btnInit.Text = "初始设置"
        '
        'ToolDebug
        '
        Me.ToolDebug.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolDebug.Image = CType(resources.GetObject("ToolDebug.Image"), System.Drawing.Image)
        Me.ToolDebug.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolDebug.Name = "ToolDebug"
        Me.ToolDebug.Size = New System.Drawing.Size(48, 48)
        Me.ToolDebug.ToolTipText = "调试"
        Me.ToolDebug.Visible = False
        '
        'TStripbtn_Barcode
        '
        Me.TStripbtn_Barcode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.TStripbtn_Barcode.Image = CType(resources.GetObject("TStripbtn_Barcode.Image"), System.Drawing.Image)
        Me.TStripbtn_Barcode.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.TStripbtn_Barcode.Name = "TStripbtn_Barcode"
        Me.TStripbtn_Barcode.Size = New System.Drawing.Size(48, 48)
        Me.TStripbtn_Barcode.Text = "扫码"
        Me.TStripbtn_Barcode.Visible = False
        '
        'btnTest
        '
        Me.btnTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnTest.Image = Global.software.My.Resources.Resources.Pnp3
        Me.btnTest.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnTest.Name = "btnTest"
        Me.btnTest.Size = New System.Drawing.Size(48, 48)
        Me.btnTest.Text = "测试"
        '
        'TStripbtnInQuery
        '
        Me.TStripbtnInQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.TStripbtnInQuery.Image = CType(resources.GetObject("TStripbtnInQuery.Image"), System.Drawing.Image)
        Me.TStripbtnInQuery.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.TStripbtnInQuery.Name = "TStripbtnInQuery"
        Me.TStripbtnInQuery.Size = New System.Drawing.Size(48, 48)
        Me.TStripbtnInQuery.Text = "查询"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 51)
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = Global.software.My.Resources.Resources.退出
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(48, 48)
        Me.ToolStripButton1.Text = "退出"
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'txtTesting
        '
        Me.txtTesting.AutoSize = True
        Me.txtTesting.Font = New System.Drawing.Font("微软雅黑", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTesting.ForeColor = System.Drawing.Color.Blue
        Me.txtTesting.Location = New System.Drawing.Point(1179, 41)
        Me.txtTesting.Name = "txtTesting"
        Me.txtTesting.Size = New System.Drawing.Size(99, 26)
        Me.txtTesting.TabIndex = 9
        Me.txtTesting.Text = "测试中......"
        '
        'MDIForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = False
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.ClientSize = New System.Drawing.Size(1429, 761)
        Me.Controls.Add(Me.ToolStrip)
        Me.Controls.Add(Me.MenuStrip)
        Me.Controls.Add(Me.bottomStatusControl)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.IsMdiContainer = True
        Me.MainMenuStrip = Me.MenuStrip
        Me.Name = "MDIForm"
        Me.Text = "OCV电池电压/内阻测试系统  V1.0"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.MenuStrip.ResumeLayout(False)
        Me.MenuStrip.PerformLayout()
        Me.ToolStrip.ResumeLayout(False)
        Me.ToolStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents ToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents init As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FileMenu As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MenuStrip As System.Windows.Forms.MenuStrip
    Friend WithEvents EditMenu As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitTool As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OnLineTest As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ManualTestToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NgRetestToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnInit As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnTest As System.Windows.Forms.ToolStripButton
    Friend WithEvents Com_SCM As System.IO.Ports.SerialPort
    Friend WithEvents Com_3561 As System.IO.Ports.SerialPort
    Friend WithEvents Com_scaner As System.IO.Ports.SerialPort
    Friend WithEvents btnDebug As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolDebug As System.Windows.Forms.ToolStripButton
    Friend WithEvents TStripMenuInQuery As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TStripbtnInQuery As System.Windows.Forms.ToolStripButton
    Friend WithEvents TStripMenuCode As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TlStripMenuG As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TlStripMenuLog As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents TStripbtn_Barcode As System.Windows.Forms.ToolStripButton
    Friend WithEvents Com_IOCard As IO.Ports.SerialPort
    Friend WithEvents userMenage As ToolStripMenuItem
    Friend WithEvents txtTesting As Label
    Friend WithEvents EMS上传ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents calibrationMenu As ToolStripMenuItem
    Friend WithEvents loginMenu As ToolStripMenuItem
    Friend WithEvents bottomStatusControl As BottomStatusControl
End Class
