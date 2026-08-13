<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmInput
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmInput))
        Me.txtbarNum = New System.Windows.Forms.TextBox()
        Me.lblBatCode = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Btn_scanCode = New System.Windows.Forms.Button()
        Me.bnt_close = New System.Windows.Forms.Button()
        Me.lblContain = New System.Windows.Forms.Label()
        Me.CmdFresh = New System.Windows.Forms.Button()
        Me.txtTray = New System.Windows.Forms.TextBox()
        Me.txtLast = New System.Windows.Forms.TextBox()
        Me.txtFirst = New System.Windows.Forms.TextBox()
        Me.lblFirst = New System.Windows.Forms.Label()
        Me.lblLast = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtbarNum
        '
        Me.txtbarNum.Location = New System.Drawing.Point(111, 200)
        Me.txtbarNum.Margin = New System.Windows.Forms.Padding(4)
        Me.txtbarNum.Multiline = True
        Me.txtbarNum.Name = "txtbarNum"
        Me.txtbarNum.Size = New System.Drawing.Size(147, 99)
        Me.txtbarNum.TabIndex = 8
        '
        'lblBatCode
        '
        Me.lblBatCode.BackColor = System.Drawing.Color.Gainsboro
        Me.lblBatCode.Location = New System.Drawing.Point(108, 181)
        Me.lblBatCode.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblBatCode.Name = "lblBatCode"
        Me.lblBatCode.Size = New System.Drawing.Size(151, 15)
        Me.lblBatCode.TabIndex = 9
        Me.lblBatCode.Text = "Label1"
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.PictureBox1.Location = New System.Drawing.Point(7, 98)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(1299, 664)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.PictureBox1.TabIndex = 7
        Me.PictureBox1.TabStop = False
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.GroupBox1.Controls.Add(Me.Btn_scanCode)
        Me.GroupBox1.Controls.Add(Me.bnt_close)
        Me.GroupBox1.Controls.Add(Me.lblContain)
        Me.GroupBox1.Controls.Add(Me.CmdFresh)
        Me.GroupBox1.Controls.Add(Me.txtTray)
        Me.GroupBox1.Controls.Add(Me.txtLast)
        Me.GroupBox1.Controls.Add(Me.txtFirst)
        Me.GroupBox1.Controls.Add(Me.lblFirst)
        Me.GroupBox1.Controls.Add(Me.lblLast)
        Me.GroupBox1.Location = New System.Drawing.Point(7, 10)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(1311, 80)
        Me.GroupBox1.TabIndex = 15
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "由头尾条码自动生成所有条码"
        '
        'Btn_scanCode
        '
        Me.Btn_scanCode.Location = New System.Drawing.Point(323, 29)
        Me.Btn_scanCode.Name = "Btn_scanCode"
        Me.Btn_scanCode.Size = New System.Drawing.Size(98, 31)
        Me.Btn_scanCode.TabIndex = 20
        Me.Btn_scanCode.Text = "扫  码"
        Me.Btn_scanCode.UseVisualStyleBackColor = True
        '
        'bnt_close
        '
        Me.bnt_close.Location = New System.Drawing.Point(1189, 29)
        Me.bnt_close.Margin = New System.Windows.Forms.Padding(4)
        Me.bnt_close.Name = "bnt_close"
        Me.bnt_close.Size = New System.Drawing.Size(75, 31)
        Me.bnt_close.TabIndex = 19
        Me.bnt_close.Text = "退出"
        Me.bnt_close.UseVisualStyleBackColor = True
        '
        'lblContain
        '
        Me.lblContain.AutoSize = True
        Me.lblContain.Location = New System.Drawing.Point(11, 37)
        Me.lblContain.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblContain.Name = "lblContain"
        Me.lblContain.Size = New System.Drawing.Size(52, 15)
        Me.lblContain.TabIndex = 9
        Me.lblContain.Text = "托盘号"
        '
        'CmdFresh
        '
        Me.CmdFresh.Location = New System.Drawing.Point(1051, 29)
        Me.CmdFresh.Margin = New System.Windows.Forms.Padding(4)
        Me.CmdFresh.Name = "CmdFresh"
        Me.CmdFresh.Size = New System.Drawing.Size(98, 31)
        Me.CmdFresh.TabIndex = 18
        Me.CmdFresh.Text = "保存条码"
        Me.CmdFresh.UseVisualStyleBackColor = True
        '
        'txtTray
        '
        Me.txtTray.Location = New System.Drawing.Point(70, 32)
        Me.txtTray.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTray.Name = "txtTray"
        Me.txtTray.Size = New System.Drawing.Size(235, 25)
        Me.txtTray.TabIndex = 10
        '
        'txtLast
        '
        Me.txtLast.Location = New System.Drawing.Point(799, 32)
        Me.txtLast.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLast.Name = "txtLast"
        Me.txtLast.Size = New System.Drawing.Size(174, 25)
        Me.txtLast.TabIndex = 15
        Me.txtLast.Text = "64"
        '
        'txtFirst
        '
        Me.txtFirst.Location = New System.Drawing.Point(536, 32)
        Me.txtFirst.Margin = New System.Windows.Forms.Padding(4)
        Me.txtFirst.Name = "txtFirst"
        Me.txtFirst.Size = New System.Drawing.Size(175, 25)
        Me.txtFirst.TabIndex = 13
        Me.txtFirst.Text = "01"
        '
        'lblFirst
        '
        Me.lblFirst.AutoSize = True
        Me.lblFirst.Location = New System.Drawing.Point(473, 37)
        Me.lblFirst.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblFirst.Name = "lblFirst"
        Me.lblFirst.Size = New System.Drawing.Size(52, 15)
        Me.lblFirst.TabIndex = 12
        Me.lblFirst.Text = "首位置"
        '
        'lblLast
        '
        Me.lblLast.AutoSize = True
        Me.lblLast.Location = New System.Drawing.Point(736, 37)
        Me.lblLast.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLast.Name = "lblLast"
        Me.lblLast.Size = New System.Drawing.Size(52, 15)
        Me.lblLast.TabIndex = 14
        Me.lblLast.Text = "末位置"
        '
        'GroupBox2
        '
        Me.GroupBox2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.GroupBox2.Location = New System.Drawing.Point(556, 354)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Size = New System.Drawing.Size(41, 26)
        Me.GroupBox2.TabIndex = 16
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "托盘编码"
        Me.GroupBox2.Visible = False
        '
        'frmInput
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1323, 775)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.lblBatCode)
        Me.Controls.Add(Me.txtbarNum)
        Me.Controls.Add(Me.PictureBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmInput"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "扫描条形码"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents txtbarNum As System.Windows.Forms.TextBox
    Friend WithEvents lblBatCode As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtLast As System.Windows.Forms.TextBox
    Friend WithEvents txtFirst As System.Windows.Forms.TextBox
    Friend WithEvents lblFirst As System.Windows.Forms.Label
    Friend WithEvents lblLast As System.Windows.Forms.Label
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents txtTray As System.Windows.Forms.TextBox
    Friend WithEvents lblContain As System.Windows.Forms.Label
    Friend WithEvents CmdFresh As System.Windows.Forms.Button
    Friend WithEvents bnt_close As System.Windows.Forms.Button
    Friend WithEvents Btn_scanCode As Button
End Class
