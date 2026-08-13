<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDebug
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDebug))
        Me.lblDataSend = New System.Windows.Forms.Label()
        Me.txtSend = New System.Windows.Forms.TextBox()
        Me.txtRecieve_SCM = New System.Windows.Forms.TextBox()
        Me.lblRecieve_SCM = New System.Windows.Forms.Label()
        Me.txtRecieve = New System.Windows.Forms.TextBox()
        Me.lblRecieve = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.btnSockTest = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.btnReset = New System.Windows.Forms.Button()
        Me.btnTestR = New System.Windows.Forms.Button()
        Me.btnDTest = New System.Windows.Forms.Button()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.txtInfo = New System.Windows.Forms.TextBox()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.btnOpen = New System.Windows.Forms.Button()
        Me.btnTrayState = New System.Windows.Forms.Button()
        Me.Pic_right = New System.Windows.Forms.PictureBox()
        Me.Pic_left = New System.Windows.Forms.PictureBox()
        Me.btnRead = New System.Windows.Forms.Button()
        Me.btnSend = New System.Windows.Forms.Button()
        Me.lblChannel = New System.Windows.Forms.Label()
        Me.cmbChannel = New System.Windows.Forms.ComboBox()
        Me.lblThe = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.TextJIG2 = New System.Windows.Forms.TextBox()
        Me.TextJIG1 = New System.Windows.Forms.TextBox()
        Me.lblposition = New System.Windows.Forms.Label()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnShow1 = New System.Windows.Forms.Button()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        CType(Me.Pic_right, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Pic_left, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblDataSend
        '
        Me.lblDataSend.AutoSize = True
        Me.lblDataSend.Location = New System.Drawing.Point(9, 151)
        Me.lblDataSend.Name = "lblDataSend"
        Me.lblDataSend.Size = New System.Drawing.Size(65, 12)
        Me.lblDataSend.TabIndex = 4
        Me.lblDataSend.Text = "发送数据："
        '
        'txtSend
        '
        Me.txtSend.Location = New System.Drawing.Point(11, 166)
        Me.txtSend.Multiline = True
        Me.txtSend.Name = "txtSend"
        Me.txtSend.Size = New System.Drawing.Size(539, 69)
        Me.txtSend.TabIndex = 5
        '
        'txtRecieve_SCM
        '
        Me.txtRecieve_SCM.Location = New System.Drawing.Point(12, 257)
        Me.txtRecieve_SCM.Multiline = True
        Me.txtRecieve_SCM.Name = "txtRecieve_SCM"
        Me.txtRecieve_SCM.Size = New System.Drawing.Size(539, 79)
        Me.txtRecieve_SCM.TabIndex = 9
        '
        'lblRecieve_SCM
        '
        Me.lblRecieve_SCM.AutoSize = True
        Me.lblRecieve_SCM.Location = New System.Drawing.Point(10, 242)
        Me.lblRecieve_SCM.Name = "lblRecieve_SCM"
        Me.lblRecieve_SCM.Size = New System.Drawing.Size(113, 12)
        Me.lblRecieve_SCM.TabIndex = 8
        Me.lblRecieve_SCM.Text = "从单片机接收数据："
        '
        'txtRecieve
        '
        Me.txtRecieve.Location = New System.Drawing.Point(12, 358)
        Me.txtRecieve.Multiline = True
        Me.txtRecieve.Name = "txtRecieve"
        Me.txtRecieve.Size = New System.Drawing.Size(539, 70)
        Me.txtRecieve.TabIndex = 11
        '
        'lblRecieve
        '
        Me.lblRecieve.AutoSize = True
        Me.lblRecieve.Location = New System.Drawing.Point(10, 343)
        Me.lblRecieve.Name = "lblRecieve"
        Me.lblRecieve.Size = New System.Drawing.Size(137, 12)
        Me.lblRecieve.TabIndex = 10
        Me.lblRecieve.Text = "从日置内阻仪接收数据："
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Location = New System.Drawing.Point(13, 12)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(413, 131)
        Me.TabControl1.TabIndex = 16
        '
        'TabPage1
        '
        Me.TabPage1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.TabPage1.Controls.Add(Me.btnShow1)
        Me.TabPage1.Controls.Add(Me.Button3)
        Me.TabPage1.Controls.Add(Me.TextBox1)
        Me.TabPage1.Controls.Add(Me.Button2)
        Me.TabPage1.Controls.Add(Me.btnSockTest)
        Me.TabPage1.Controls.Add(Me.Button1)
        Me.TabPage1.Controls.Add(Me.btnReset)
        Me.TabPage1.Controls.Add(Me.btnTestR)
        Me.TabPage1.Controls.Add(Me.btnDTest)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.TabPage1.Size = New System.Drawing.Size(405, 105)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "通讯测试"
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(242, 61)
        Me.Button3.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(56, 18)
        Me.Button3.TabIndex = 21
        Me.Button3.Text = "Test34461"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(242, 27)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(76, 21)
        Me.TextBox1.TabIndex = 18
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(314, 61)
        Me.Button2.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(75, 18)
        Me.Button2.TabIndex = 17
        Me.Button2.Text = "TestComm16"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'btnSockTest
        '
        Me.btnSockTest.Location = New System.Drawing.Point(169, 61)
        Me.btnSockTest.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.btnSockTest.Name = "btnSockTest"
        Me.btnSockTest.Size = New System.Drawing.Size(60, 18)
        Me.btnSockTest.TabIndex = 17
        Me.btnSockTest.Text = "sockTest"
        Me.btnSockTest.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(333, 30)
        Me.Button1.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(56, 18)
        Me.Button1.TabIndex = 17
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'btnReset
        '
        Me.btnReset.Location = New System.Drawing.Point(169, 27)
        Me.btnReset.Name = "btnReset"
        Me.btnReset.Size = New System.Drawing.Size(44, 23)
        Me.btnReset.TabIndex = 16
        Me.btnReset.Text = "复位"
        Me.btnReset.UseVisualStyleBackColor = True
        Me.btnReset.Visible = False
        '
        'btnTestR
        '
        Me.btnTestR.Location = New System.Drawing.Point(17, 56)
        Me.btnTestR.Name = "btnTestR"
        Me.btnTestR.Size = New System.Drawing.Size(136, 23)
        Me.btnTestR.TabIndex = 15
        Me.btnTestR.Text = "内阻仪通信测试"
        Me.btnTestR.UseVisualStyleBackColor = True
        '
        'btnDTest
        '
        Me.btnDTest.Location = New System.Drawing.Point(17, 27)
        Me.btnDTest.Name = "btnDTest"
        Me.btnDTest.Size = New System.Drawing.Size(136, 23)
        Me.btnDTest.TabIndex = 14
        Me.btnDTest.Text = "单片机通信测试"
        Me.btnDTest.UseVisualStyleBackColor = True
        '
        'TabPage2
        '
        Me.TabPage2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.TabPage2.Controls.Add(Me.txtInfo)
        Me.TabPage2.Controls.Add(Me.btnClose)
        Me.TabPage2.Controls.Add(Me.btnOpen)
        Me.TabPage2.Controls.Add(Me.btnTrayState)
        Me.TabPage2.Controls.Add(Me.Pic_right)
        Me.TabPage2.Controls.Add(Me.Pic_left)
        Me.TabPage2.Controls.Add(Me.btnRead)
        Me.TabPage2.Controls.Add(Me.btnSend)
        Me.TabPage2.Controls.Add(Me.lblChannel)
        Me.TabPage2.Controls.Add(Me.cmbChannel)
        Me.TabPage2.Controls.Add(Me.lblThe)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.TabPage2.Size = New System.Drawing.Size(405, 105)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "通道测试"
        '
        'txtInfo
        '
        Me.txtInfo.Location = New System.Drawing.Point(194, 39)
        Me.txtInfo.Multiline = True
        Me.txtInfo.Name = "txtInfo"
        Me.txtInfo.Size = New System.Drawing.Size(204, 61)
        Me.txtInfo.TabIndex = 67
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnClose.Location = New System.Drawing.Point(333, 6)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(65, 22)
        Me.btnClose.TabIndex = 66
        Me.btnClose.Text = "闭合托盘"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'btnOpen
        '
        Me.btnOpen.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnOpen.Location = New System.Drawing.Point(262, 6)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(65, 22)
        Me.btnOpen.TabIndex = 65
        Me.btnOpen.Text = "开启托盘"
        Me.btnOpen.UseVisualStyleBackColor = False
        '
        'btnTrayState
        '
        Me.btnTrayState.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnTrayState.Location = New System.Drawing.Point(194, 6)
        Me.btnTrayState.Name = "btnTrayState"
        Me.btnTrayState.Size = New System.Drawing.Size(65, 22)
        Me.btnTrayState.TabIndex = 64
        Me.btnTrayState.Text = "托盘状态"
        Me.btnTrayState.UseVisualStyleBackColor = False
        '
        'Pic_right
        '
        Me.Pic_right.BackgroundImage = CType(resources.GetObject("Pic_right.BackgroundImage"), System.Drawing.Image)
        Me.Pic_right.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.Pic_right.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Pic_right.Location = New System.Drawing.Point(152, 27)
        Me.Pic_right.Name = "Pic_right"
        Me.Pic_right.Size = New System.Drawing.Size(19, 15)
        Me.Pic_right.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.Pic_right.TabIndex = 19
        Me.Pic_right.TabStop = False
        '
        'Pic_left
        '
        Me.Pic_left.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Pic_left.BackgroundImage = CType(resources.GetObject("Pic_left.BackgroundImage"), System.Drawing.Image)
        Me.Pic_left.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.Pic_left.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Pic_left.Location = New System.Drawing.Point(127, 27)
        Me.Pic_left.Name = "Pic_left"
        Me.Pic_left.Size = New System.Drawing.Size(19, 15)
        Me.Pic_left.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.Pic_left.TabIndex = 18
        Me.Pic_left.TabStop = False
        '
        'btnRead
        '
        Me.btnRead.Location = New System.Drawing.Point(93, 58)
        Me.btnRead.Name = "btnRead"
        Me.btnRead.Size = New System.Drawing.Size(74, 24)
        Me.btnRead.TabIndex = 17
        Me.btnRead.Text = "读测试结果"
        Me.btnRead.UseVisualStyleBackColor = True
        '
        'btnSend
        '
        Me.btnSend.Location = New System.Drawing.Point(13, 57)
        Me.btnSend.Name = "btnSend"
        Me.btnSend.Size = New System.Drawing.Size(74, 24)
        Me.btnSend.TabIndex = 16
        Me.btnSend.Text = "发送测试"
        Me.btnSend.UseVisualStyleBackColor = True
        '
        'lblChannel
        '
        Me.lblChannel.AutoSize = True
        Me.lblChannel.Location = New System.Drawing.Point(91, 27)
        Me.lblChannel.Name = "lblChannel"
        Me.lblChannel.Size = New System.Drawing.Size(29, 12)
        Me.lblChannel.TabIndex = 15
        Me.lblChannel.Text = "通道"
        '
        'cmbChannel
        '
        Me.cmbChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbChannel.FormattingEnabled = True
        Me.cmbChannel.Location = New System.Drawing.Point(39, 22)
        Me.cmbChannel.Name = "cmbChannel"
        Me.cmbChannel.Size = New System.Drawing.Size(46, 20)
        Me.cmbChannel.TabIndex = 14
        '
        'lblThe
        '
        Me.lblThe.AutoSize = True
        Me.lblThe.Location = New System.Drawing.Point(11, 27)
        Me.lblThe.Name = "lblThe"
        Me.lblThe.Size = New System.Drawing.Size(17, 12)
        Me.lblThe.TabIndex = 13
        Me.lblThe.Text = "第"
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.GroupBox1.Controls.Add(Me.TextJIG2)
        Me.GroupBox1.Controls.Add(Me.TextJIG1)
        Me.GroupBox1.Controls.Add(Me.lblposition)
        Me.GroupBox1.Location = New System.Drawing.Point(432, 34)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(119, 109)
        Me.GroupBox1.TabIndex = 18
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "电池位置"
        '
        'TextJIG2
        '
        Me.TextJIG2.Location = New System.Drawing.Point(5, 74)
        Me.TextJIG2.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.TextJIG2.Name = "TextJIG2"
        Me.TextJIG2.Size = New System.Drawing.Size(110, 21)
        Me.TextJIG2.TabIndex = 18
        '
        'TextJIG1
        '
        Me.TextJIG1.Location = New System.Drawing.Point(5, 39)
        Me.TextJIG1.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.TextJIG1.Name = "TextJIG1"
        Me.TextJIG1.Size = New System.Drawing.Size(110, 21)
        Me.TextJIG1.TabIndex = 18
        '
        'lblposition
        '
        Me.lblposition.AutoSize = True
        Me.lblposition.Location = New System.Drawing.Point(6, 18)
        Me.lblposition.Name = "lblposition"
        Me.lblposition.Size = New System.Drawing.Size(71, 12)
        Me.lblposition.TabIndex = 18
        Me.lblposition.Text = "lblposition"
        '
        'btnClear
        '
        Me.btnClear.Location = New System.Drawing.Point(126, 434)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(69, 23)
        Me.btnClear.TabIndex = 19
        Me.btnClear.Text = "清空"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(270, 434)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(62, 23)
        Me.btnCancel.TabIndex = 20
        Me.btnCancel.Text = "关闭"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnShow1
        '
        Me.btnShow1.Location = New System.Drawing.Point(169, 84)
        Me.btnShow1.Name = "btnShow1"
        Me.btnShow1.Size = New System.Drawing.Size(60, 18)
        Me.btnShow1.TabIndex = 22
        Me.btnShow1.Text = "Button4"
        Me.btnShow1.UseVisualStyleBackColor = True
        '
        'frmDebug
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(567, 468)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.txtRecieve)
        Me.Controls.Add(Me.lblRecieve)
        Me.Controls.Add(Me.txtRecieve_SCM)
        Me.Controls.Add(Me.lblRecieve_SCM)
        Me.Controls.Add(Me.txtSend)
        Me.Controls.Add(Me.lblDataSend)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "frmDebug"
        Me.Tag = "调试"
        Me.Text = "调试"
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        CType(Me.Pic_right, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Pic_left, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblDataSend As System.Windows.Forms.Label
    Friend WithEvents txtSend As System.Windows.Forms.TextBox
    Friend WithEvents txtRecieve_SCM As System.Windows.Forms.TextBox
    Friend WithEvents lblRecieve_SCM As System.Windows.Forms.Label
    Friend WithEvents txtRecieve As System.Windows.Forms.TextBox
    Friend WithEvents lblRecieve As System.Windows.Forms.Label
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents btnReset As System.Windows.Forms.Button
    Friend WithEvents btnTestR As System.Windows.Forms.Button
    Friend WithEvents btnDTest As System.Windows.Forms.Button
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents btnRead As System.Windows.Forms.Button
    Friend WithEvents btnSend As System.Windows.Forms.Button
    Friend WithEvents lblChannel As System.Windows.Forms.Label
    Friend WithEvents cmbChannel As System.Windows.Forms.ComboBox
    Friend WithEvents lblThe As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents lblposition As System.Windows.Forms.Label
    Friend WithEvents Pic_right As System.Windows.Forms.PictureBox
    Friend WithEvents Pic_left As System.Windows.Forms.PictureBox
    Friend WithEvents btnClear As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents txtInfo As System.Windows.Forms.TextBox
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents btnOpen As System.Windows.Forms.Button
    Friend WithEvents btnTrayState As System.Windows.Forms.Button
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents btnSockTest As Button
    Friend WithEvents TextJIG2 As TextBox
    Friend WithEvents TextJIG1 As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents btnShow1 As Button
End Class
