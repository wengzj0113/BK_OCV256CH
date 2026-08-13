<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmName
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
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Cmd_OK = New System.Windows.Forms.Button()
        Me.Cmd_NO = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(134, 38)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(184, 21)
        Me.TextBox1.TabIndex = 1
        '
        'Cmd_OK
        '
        Me.Cmd_OK.Location = New System.Drawing.Point(140, 116)
        Me.Cmd_OK.Name = "Cmd_OK"
        Me.Cmd_OK.Size = New System.Drawing.Size(78, 26)
        Me.Cmd_OK.TabIndex = 2
        Me.Cmd_OK.Text = "确定"
        Me.Cmd_OK.UseVisualStyleBackColor = True
        '
        'Cmd_NO
        '
        Me.Cmd_NO.Location = New System.Drawing.Point(244, 116)
        Me.Cmd_NO.Name = "Cmd_NO"
        Me.Cmd_NO.Size = New System.Drawing.Size(78, 26)
        Me.Cmd_NO.TabIndex = 3
        Me.Cmd_NO.Text = "取消"
        Me.Cmd_NO.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.TextBox1)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(324, 107)
        Me.GroupBox1.TabIndex = 4
        Me.GroupBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(15, 41)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(113, 12)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "录入新的条件名称："
        '
        'frmName
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(340, 154)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Cmd_NO)
        Me.Controls.Add(Me.Cmd_OK)
        Me.Name = "frmName"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Cmd_OK As System.Windows.Forms.Button
    Friend WithEvents Cmd_NO As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
