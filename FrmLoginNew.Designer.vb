<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmLoginNew
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
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.PasswordTextBox = New System.Windows.Forms.TextBox()
        Me.ComboBoxUsername = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.LoginButton = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'btnDelete
        '
        Me.btnDelete.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.btnDelete.Location = New System.Drawing.Point(348, 90)
        Me.btnDelete.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(10, 30)
        Me.btnDelete.TabIndex = 19
        Me.btnDelete.Text = "删除用户"
        Me.btnDelete.UseVisualStyleBackColor = True
        Me.btnDelete.Visible = False
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.Button2.Location = New System.Drawing.Point(348, 32)
        Me.Button2.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(10, 32)
        Me.Button2.TabIndex = 20
        Me.Button2.Text = "添加用户"
        Me.Button2.UseVisualStyleBackColor = True
        Me.Button2.Visible = False
        '
        'PasswordTextBox
        '
        Me.PasswordTextBox.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.PasswordTextBox.Location = New System.Drawing.Point(86, 91)
        Me.PasswordTextBox.Name = "PasswordTextBox"
        Me.PasswordTextBox.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.PasswordTextBox.Size = New System.Drawing.Size(242, 25)
        Me.PasswordTextBox.TabIndex = 18
        '
        'ComboBoxUsername
        '
        Me.ComboBoxUsername.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.ComboBoxUsername.FormattingEnabled = True
        Me.ComboBoxUsername.Items.AddRange(New Object() {"系统管理员", "操作员1", "操作员2"})
        Me.ComboBoxUsername.Location = New System.Drawing.Point(86, 32)
        Me.ComboBoxUsername.Name = "ComboBoxUsername"
        Me.ComboBoxUsername.Size = New System.Drawing.Size(242, 28)
        Me.ComboBoxUsername.TabIndex = 17
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.Label2.Location = New System.Drawing.Point(47, 91)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(37, 20)
        Me.Label2.TabIndex = 16
        Me.Label2.Text = "密码"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.Label1.Location = New System.Drawing.Point(47, 38)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(37, 20)
        Me.Label1.TabIndex = 15
        Me.Label1.Text = "用户"
        '
        'LoginButton
        '
        Me.LoginButton.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.LoginButton.Location = New System.Drawing.Point(73, 166)
        Me.LoginButton.Name = "LoginButton"
        Me.LoginButton.Size = New System.Drawing.Size(106, 36)
        Me.LoginButton.TabIndex = 14
        Me.LoginButton.Text = "确定"
        Me.LoginButton.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Font = New System.Drawing.Font("微软雅黑", 10.2!)
        Me.Button1.Location = New System.Drawing.Point(236, 166)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(106, 36)
        Me.Button1.TabIndex = 13
        Me.Button1.Text = "取消"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'FrmLoginNew
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(410, 252)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.PasswordTextBox)
        Me.Controls.Add(Me.ComboBoxUsername)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.LoginButton)
        Me.Controls.Add(Me.Button1)
        Me.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.Name = "FrmLoginNew"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "FrmLoginNew"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnDelete As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents PasswordTextBox As TextBox
    Friend WithEvents ComboBoxUsername As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents LoginButton As Button
    Friend WithEvents Button1 As Button
End Class
