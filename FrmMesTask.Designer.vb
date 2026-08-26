<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMesTask
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
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.chkResultDataUploadAssembly = New System.Windows.Forms.CheckBox()
        Me.chkWIPInStationCheck = New System.Windows.Forms.CheckBox()
        Me.chkAlarmUpload = New System.Windows.Forms.CheckBox()
        Me.chkStateAndStopReason = New System.Windows.Forms.CheckBox()
        Me.chkHeartbeat = New System.Windows.Forms.CheckBox()
        Me.listBoxOutput = New System.Windows.Forms.ListBox()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.txtProcessNum = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.txtOperatorName = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.Gainsboro
        Me.GroupBox1.Controls.Add(Me.chkResultDataUploadAssembly)
        Me.GroupBox1.Controls.Add(Me.chkWIPInStationCheck)
        Me.GroupBox1.Controls.Add(Me.chkAlarmUpload)
        Me.GroupBox1.Controls.Add(Me.chkStateAndStopReason)
        Me.GroupBox1.Controls.Add(Me.chkHeartbeat)
        Me.GroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.GroupBox1.Location = New System.Drawing.Point(690, 24)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(264, 272)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "程序接口"
        '
        'chkResultDataUploadAssembly
        '
        Me.chkResultDataUploadAssembly.AutoSize = True
        Me.chkResultDataUploadAssembly.Checked = True
        Me.chkResultDataUploadAssembly.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkResultDataUploadAssembly.Location = New System.Drawing.Point(39, 210)
        Me.chkResultDataUploadAssembly.Name = "chkResultDataUploadAssembly"
        Me.chkResultDataUploadAssembly.Size = New System.Drawing.Size(162, 16)
        Me.chkResultDataUploadAssembly.TabIndex = 0
        Me.chkResultDataUploadAssembly.Text = "产品结果数据上传-电芯段"
        Me.chkResultDataUploadAssembly.UseVisualStyleBackColor = True
        '
        'chkWIPInStationCheck
        '
        Me.chkWIPInStationCheck.AutoSize = True
        Me.chkWIPInStationCheck.Location = New System.Drawing.Point(39, 190)
        Me.chkWIPInStationCheck.Name = "chkWIPInStationCheck"
        Me.chkWIPInStationCheck.Size = New System.Drawing.Size(108, 16)
        Me.chkWIPInStationCheck.TabIndex = 0
        Me.chkWIPInStationCheck.Text = "在制品进站校验"
        Me.chkWIPInStationCheck.UseVisualStyleBackColor = True
        Me.chkWIPInStationCheck.Visible = False
        '
        '
        'chkAlarmUpload
        '
        Me.chkAlarmUpload.AutoSize = True
        Me.chkAlarmUpload.Checked = True
        Me.chkAlarmUpload.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAlarmUpload.Location = New System.Drawing.Point(39, 120)
        Me.chkAlarmUpload.Name = "chkAlarmUpload"
        Me.chkAlarmUpload.Size = New System.Drawing.Size(96, 16)
        Me.chkAlarmUpload.TabIndex = 0
        Me.chkAlarmUpload.Text = "设备报警上传"
        Me.chkAlarmUpload.UseVisualStyleBackColor = True
        '
        'chkStateAndStopReason
        '
        Me.chkStateAndStopReason.AutoSize = True
        Me.chkStateAndStopReason.Checked = True
        Me.chkStateAndStopReason.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkStateAndStopReason.Location = New System.Drawing.Point(39, 75)
        Me.chkStateAndStopReason.Name = "chkStateAndStopReason"
        Me.chkStateAndStopReason.Size = New System.Drawing.Size(150, 16)
        Me.chkStateAndStopReason.TabIndex = 0
        Me.chkStateAndStopReason.Text = "设备状态+停机原因上传"
        Me.chkStateAndStopReason.UseVisualStyleBackColor = True
        '
        'chkHeartbeat
        '
        Me.chkHeartbeat.AutoSize = True
        Me.chkHeartbeat.Checked = True
        Me.chkHeartbeat.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkHeartbeat.Location = New System.Drawing.Point(39, 30)
        Me.chkHeartbeat.Name = "chkHeartbeat"
        Me.chkHeartbeat.Size = New System.Drawing.Size(96, 16)
        Me.chkHeartbeat.TabIndex = 0
        Me.chkHeartbeat.Text = "设备在线检测"
        Me.chkHeartbeat.UseVisualStyleBackColor = True
        '
        'listBoxOutput
        '
        Me.listBoxOutput.Dock = System.Windows.Forms.DockStyle.Left
        Me.listBoxOutput.FormattingEnabled = True
        Me.listBoxOutput.ItemHeight = 12
        Me.listBoxOutput.Location = New System.Drawing.Point(0, 0)
        Me.listBoxOutput.Name = "listBoxOutput"
        Me.listBoxOutput.Size = New System.Drawing.Size(650, 450)
        Me.listBoxOutput.TabIndex = 1
        '
        'btnStart
        '
        Me.btnStart.Location = New System.Drawing.Point(704, 376)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(75, 23)
        Me.btnStart.TabIndex = 1
        Me.btnStart.Text = "启动"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnStop
        '
        Me.btnStop.Location = New System.Drawing.Point(855, 376)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(75, 23)
        Me.btnStop.TabIndex = 2
        Me.btnStop.Text = "停止"
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'txtProcessNum
        '
        Me.txtProcessNum.Location = New System.Drawing.Point(747, 300)
        Me.txtProcessNum.Name = "txtProcessNum"
        Me.txtProcessNum.Size = New System.Drawing.Size(199, 21)
        Me.txtProcessNum.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(688, 305)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 12)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "工序编码"
        '
        'Button3
        '
        Me.Button3.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.Button3.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Button3.Font = New System.Drawing.Font("微软雅黑", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Button3.Location = New System.Drawing.Point(650, 405)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(337, 45)
        Me.Button3.TabIndex = 5
        Me.Button3.Text = "隐藏窗口"
        Me.Button3.UseVisualStyleBackColor = False
        '
        'txtOperatorName
        '
        Me.txtOperatorName.Location = New System.Drawing.Point(747, 330)
        Me.txtOperatorName.Name = "txtOperatorName"
        Me.txtOperatorName.Size = New System.Drawing.Size(199, 21)
        Me.txtOperatorName.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(688, 335)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(41, 12)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "操作员"
        '
        'FrmMesTask
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ControlLight
        Me.ClientSize = New System.Drawing.Size(987, 450)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtOperatorName)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtProcessNum)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.listBoxOutput)
        Me.Controls.Add(Me.GroupBox1)
        Me.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "FrmMesTask"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "FrmMesTask"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents chkResultDataUploadAssembly As CheckBox
    Friend WithEvents chkWIPInStationCheck As CheckBox
    Friend WithEvents chkAlarmUpload As CheckBox
    Friend WithEvents chkStateAndStopReason As CheckBox
    Friend WithEvents chkHeartbeat As CheckBox
    Friend WithEvents listBoxOutput As ListBox
    Friend WithEvents btnStart As Button
    Friend WithEvents btnStop As Button
    Friend WithEvents txtProcessNum As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Button3 As Button
    Friend WithEvents txtOperatorName As TextBox
    Friend WithEvents Label2 As Label
End Class
