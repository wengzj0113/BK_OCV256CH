<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class StartupDeviceFailureDialog
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.picWarning = New System.Windows.Forms.PictureBox()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.autoCloseTimer = New System.Windows.Forms.Timer(Me.components)
        CType(Me.picWarning, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picWarning
        '
        Me.picWarning.Image = System.Drawing.SystemIcons.Warning.ToBitmap()
        Me.picWarning.Location = New System.Drawing.Point(24, 34)
        Me.picWarning.Name = "picWarning"
        Me.picWarning.Size = New System.Drawing.Size(32, 32)
        Me.picWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.picWarning.TabIndex = 0
        Me.picWarning.TabStop = False
        '
        'lblMessage
        '
        Me.lblMessage.Location = New System.Drawing.Point(68, 24)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New System.Drawing.Size(404, 142)
        Me.lblMessage.TabIndex = 1
        Me.lblMessage.Text = ""
        '
        'btnClose
        '
        Me.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnClose.Location = New System.Drawing.Point(388, 184)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(84, 32)
        Me.btnClose.TabIndex = 2
        Me.btnClose.Text = "确定"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'autoCloseTimer
        '
        Me.autoCloseTimer.Interval = 5000
        '
        'StartupDeviceFailureDialog
        '
        Me.AcceptButton = Me.btnClose
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnClose
        Me.ClientSize = New System.Drawing.Size(496, 232)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.picWarning)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "StartupDeviceFailureDialog"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "设备连接检查"
        Me.TopMost = True
        CType(Me.picWarning, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents picWarning As System.Windows.Forms.PictureBox
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents autoCloseTimer As System.Windows.Forms.Timer
End Class
