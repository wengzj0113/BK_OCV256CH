<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmUserManage
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.PasswordTextBox = New System.Windows.Forms.TextBox()
        Me.ComboBoxUsername = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnChangePWD = New System.Windows.Forms.Button()
        Me.ComboBoxRole = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnDelete
        '
        Me.btnDelete.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.btnDelete.Location = New System.Drawing.Point(247, 215)
        Me.btnDelete.Margin = New System.Windows.Forms.Padding(2)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(87, 44)
        Me.btnDelete.TabIndex = 25
        Me.btnDelete.Text = "删除用户"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'btnAdd
        '
        Me.btnAdd.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.btnAdd.Location = New System.Drawing.Point(87, 215)
        Me.btnAdd.Margin = New System.Windows.Forms.Padding(2)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(87, 44)
        Me.btnAdd.TabIndex = 26
        Me.btnAdd.Text = "添加用户"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'PasswordTextBox
        '
        Me.PasswordTextBox.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.PasswordTextBox.Location = New System.Drawing.Point(109, 107)
        Me.PasswordTextBox.Name = "PasswordTextBox"
        Me.PasswordTextBox.Size = New System.Drawing.Size(197, 29)
        Me.PasswordTextBox.TabIndex = 24
        '
        'ComboBoxUsername
        '
        Me.ComboBoxUsername.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.ComboBoxUsername.FormattingEnabled = True
        Me.ComboBoxUsername.Items.AddRange(New Object() {"系统管理员", "操作员1", "操作员2"})
        Me.ComboBoxUsername.Location = New System.Drawing.Point(109, 48)
        Me.ComboBoxUsername.Name = "ComboBoxUsername"
        Me.ComboBoxUsername.Size = New System.Drawing.Size(197, 29)
        Me.ComboBoxUsername.TabIndex = 23
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.Label2.Location = New System.Drawing.Point(60, 107)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(42, 21)
        Me.Label2.TabIndex = 22
        Me.Label2.Text = "密码"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.Label1.Location = New System.Drawing.Point(60, 54)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(42, 21)
        Me.Label1.TabIndex = 21
        Me.Label1.Text = "用户"
        '
        'btnChangePWD
        '
        Me.btnChangePWD.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.btnChangePWD.Location = New System.Drawing.Point(311, 107)
        Me.btnChangePWD.Margin = New System.Windows.Forms.Padding(2)
        Me.btnChangePWD.Name = "btnChangePWD"
        Me.btnChangePWD.Size = New System.Drawing.Size(64, 29)
        Me.btnChangePWD.TabIndex = 26
        Me.btnChangePWD.Text = "修改"
        Me.btnChangePWD.UseVisualStyleBackColor = True
        '
        'ComboBoxRole
        '
        Me.ComboBoxRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxRole.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.ComboBoxRole.FormattingEnabled = True
        Me.ComboBoxRole.Items.AddRange(New Object() {"操作员", "工程师", "系统管理员"})
        Me.ComboBoxRole.Location = New System.Drawing.Point(109, 150)
        Me.ComboBoxRole.Name = "ComboBoxRole"
        Me.ComboBoxRole.Size = New System.Drawing.Size(197, 29)
        Me.ComboBoxRole.TabIndex = 27
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("微软雅黑", 12.0!)
        Me.Label3.Location = New System.Drawing.Point(60, 150)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(42, 21)
        Me.Label3.TabIndex = 28
        Me.Label3.Text = "角色"
        '
        'frmUserManage
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(386, 285)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnChangePWD)
        Me.Controls.Add(Me.btnAdd)
        Me.Controls.Add(Me.ComboBoxRole)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.PasswordTextBox)
        Me.Controls.Add(Me.ComboBoxUsername)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "frmUserManage"
        Me.Text = "frmUserManage"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnDelete As Button
    Friend WithEvents btnAdd As Button
    Friend WithEvents PasswordTextBox As TextBox
    Friend WithEvents ComboBoxUsername As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents btnChangePWD As Button
    Friend WithEvents ComboBoxRole As ComboBox
    Friend WithEvents Label3 As Label
End Class
