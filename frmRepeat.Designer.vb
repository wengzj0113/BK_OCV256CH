<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmRepeat
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
        Me.btnClose = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.lblFtest = New System.Windows.Forms.TextBox()
        Me.lblSTest = New System.Windows.Forms.TextBox()
        Me.lblTTest = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(185, 303)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(75, 23)
        Me.btnClose.TabIndex = 7
        Me.btnClose.Text = "关闭"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(10, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(77, 12)
        Me.Label1.TabIndex = 8
        Me.Label1.Text = "复测的次数："
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(87, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(41, 12)
        Me.Label2.TabIndex = 9
        Me.Label2.Text = "Label2"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(11, 31)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(101, 12)
        Me.Label3.TabIndex = 10
        Me.Label3.Text = "第一次复测的点："
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 121)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(101, 12)
        Me.Label5.TabIndex = 12
        Me.Label5.Text = "第二次复测的点："
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(10, 212)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(101, 12)
        Me.Label6.TabIndex = 13
        Me.Label6.Text = "第三次复测的点："
        '
        'lblFtest
        '
        Me.lblFtest.Location = New System.Drawing.Point(109, 34)
        Me.lblFtest.Multiline = True
        Me.lblFtest.Name = "lblFtest"
        Me.lblFtest.Size = New System.Drawing.Size(309, 72)
        Me.lblFtest.TabIndex = 16
        '
        'lblSTest
        '
        Me.lblSTest.Location = New System.Drawing.Point(109, 119)
        Me.lblSTest.Multiline = True
        Me.lblSTest.Name = "lblSTest"
        Me.lblSTest.Size = New System.Drawing.Size(309, 76)
        Me.lblSTest.TabIndex = 17
        '
        'lblTTest
        '
        Me.lblTTest.Location = New System.Drawing.Point(109, 212)
        Me.lblTTest.Multiline = True
        Me.lblTTest.Name = "lblTTest"
        Me.lblTTest.Size = New System.Drawing.Size(309, 76)
        Me.lblTTest.TabIndex = 18
        '
        'frmRepeat
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(430, 343)
        Me.Controls.Add(Me.lblTTest)
        Me.Controls.Add(Me.lblSTest)
        Me.Controls.Add(Me.lblFtest)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnClose)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "frmRepeat"
        Me.Text = "自动复测结果显示"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents lblFtest As System.Windows.Forms.TextBox
    Friend WithEvents lblSTest As System.Windows.Forms.TextBox
    Friend WithEvents lblTTest As System.Windows.Forms.TextBox
End Class
