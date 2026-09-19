<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmTestProgress
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.mainLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.lblProgressTitle = New System.Windows.Forms.Label()
        Me.lblProgressStage = New System.Windows.Forms.Label()
        Me.progressTest = New System.Windows.Forms.ProgressBar()
        Me.lblProgressCount = New System.Windows.Forms.Label()
        Me.lblProgressElapsed = New System.Windows.Forms.Label()
        Me.lblProgressEta = New System.Windows.Forms.Label()
        Me.lblProgressDetail = New System.Windows.Forms.Label()
        Me.lblProgressLogTitle = New System.Windows.Forms.Label()
        Me.txtProgressLog = New System.Windows.Forms.TextBox()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.mainLayout.SuspendLayout()
        Me.SuspendLayout()
        '
        'mainLayout
        '
        Me.mainLayout.ColumnCount = 4
        Me.mainLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.mainLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.mainLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.mainLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.mainLayout.Controls.Add(Me.lblProgressTitle, 0, 0)
        Me.mainLayout.Controls.Add(Me.lblProgressStage, 0, 1)
        Me.mainLayout.Controls.Add(Me.progressTest, 0, 2)
        Me.mainLayout.Controls.Add(Me.lblProgressCount, 0, 3)
        Me.mainLayout.Controls.Add(Me.lblProgressElapsed, 1, 3)
        Me.mainLayout.Controls.Add(Me.lblProgressEta, 2, 3)
        Me.mainLayout.Controls.Add(Me.lblProgressDetail, 0, 4)
        Me.mainLayout.Controls.Add(Me.lblProgressLogTitle, 0, 5)
        Me.mainLayout.Controls.Add(Me.txtProgressLog, 0, 6)
        Me.mainLayout.Controls.Add(Me.btnClose, 3, 7)
        Me.mainLayout.SetColumnSpan(Me.lblProgressTitle, 4)
        Me.mainLayout.SetColumnSpan(Me.lblProgressStage, 4)
        Me.mainLayout.SetColumnSpan(Me.progressTest, 4)
        Me.mainLayout.SetColumnSpan(Me.lblProgressDetail, 4)
        Me.mainLayout.SetColumnSpan(Me.lblProgressLogTitle, 4)
        Me.mainLayout.SetColumnSpan(Me.txtProgressLog, 4)
        Me.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mainLayout.Location = New System.Drawing.Point(0, 0)
        Me.mainLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.mainLayout.Name = "mainLayout"
        Me.mainLayout.Padding = New System.Windows.Forms.Padding(22, 18, 22, 18)
        Me.mainLayout.RowCount = 8
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.mainLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.mainLayout.Size = New System.Drawing.Size(620, 390)
        Me.mainLayout.TabIndex = 0
        '
        'lblProgressTitle
        '
        Me.lblProgressTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 13.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblProgressTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.lblProgressTitle.Name = "lblProgressTitle"
        Me.lblProgressTitle.Size = New System.Drawing.Size(576, 30)
        Me.lblProgressTitle.TabIndex = 0
        Me.lblProgressTitle.Text = "测试进度"
        Me.lblProgressTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblProgressStage
        '
        Me.lblProgressStage.AutoEllipsis = True
        Me.lblProgressStage.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressStage.Font = New System.Drawing.Font("Microsoft YaHei UI", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblProgressStage.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblProgressStage.Name = "lblProgressStage"
        Me.lblProgressStage.Size = New System.Drawing.Size(576, 30)
        Me.lblProgressStage.TabIndex = 1
        Me.lblProgressStage.Text = "等待测试"
        Me.lblProgressStage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'progressTest
        '
        Me.progressTest.Dock = System.Windows.Forms.DockStyle.Fill
        Me.progressTest.MarqueeAnimationSpeed = 30
        Me.progressTest.Name = "progressTest"
        Me.progressTest.Size = New System.Drawing.Size(576, 22)
        Me.progressTest.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.progressTest.TabIndex = 2
        '
        'lblProgressCount
        '
        Me.lblProgressCount.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressCount.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.lblProgressCount.Name = "lblProgressCount"
        Me.lblProgressCount.Size = New System.Drawing.Size(144, 30)
        Me.lblProgressCount.TabIndex = 3
        Me.lblProgressCount.Text = "通道：0/0"
        Me.lblProgressCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblProgressElapsed
        '
        Me.lblProgressElapsed.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressElapsed.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.lblProgressElapsed.Name = "lblProgressElapsed"
        Me.lblProgressElapsed.Size = New System.Drawing.Size(144, 30)
        Me.lblProgressElapsed.TabIndex = 4
        Me.lblProgressElapsed.Text = "用时 00:00"
        Me.lblProgressElapsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblProgressEta
        '
        Me.lblProgressEta.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressEta.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.lblProgressEta.Name = "lblProgressEta"
        Me.lblProgressEta.Size = New System.Drawing.Size(144, 30)
        Me.lblProgressEta.TabIndex = 5
        Me.lblProgressEta.Text = "剩余 --"
        Me.lblProgressEta.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblProgressDetail
        '
        Me.lblProgressDetail.AutoEllipsis = True
        Me.lblProgressDetail.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressDetail.ForeColor = System.Drawing.Color.FromArgb(CType(CType(90, Byte), Integer), CType(CType(104, Byte), Integer), CType(CType(120, Byte), Integer))
        Me.lblProgressDetail.Name = "lblProgressDetail"
        Me.lblProgressDetail.Size = New System.Drawing.Size(576, 30)
        Me.lblProgressDetail.TabIndex = 6
        Me.lblProgressDetail.Text = "等待开始"
        Me.lblProgressDetail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblProgressLogTitle
        '
        Me.lblProgressLogTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblProgressLogTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblProgressLogTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblProgressLogTitle.Name = "lblProgressLogTitle"
        Me.lblProgressLogTitle.Size = New System.Drawing.Size(576, 24)
        Me.lblProgressLogTitle.TabIndex = 7
        Me.lblProgressLogTitle.Text = "执行信息"
        Me.lblProgressLogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtProgressLog
        '
        Me.txtProgressLog.BackColor = System.Drawing.Color.FromArgb(CType(CType(248, Byte), Integer), CType(CType(250, Byte), Integer), CType(CType(252, Byte), Integer))
        Me.txtProgressLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtProgressLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtProgressLog.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtProgressLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtProgressLog.Multiline = True
        Me.txtProgressLog.Name = "txtProgressLog"
        Me.txtProgressLog.ReadOnly = True
        Me.txtProgressLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtProgressLog.Size = New System.Drawing.Size(576, 100)
        Me.txtProgressLog.TabIndex = 8
        '
        'btnClose
        '
        Me.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnClose.Enabled = False
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(110, 30)
        Me.btnClose.TabIndex = 9
        Me.btnClose.Text = "关闭"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'frmTestProgress
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(620, 390)
        Me.ControlBox = False
        Me.Controls.Add(Me.mainLayout)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmTestProgress"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "测试进度"
        Me.TopMost = True
        Me.mainLayout.ResumeLayout(False)
        Me.mainLayout.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private mainLayout As System.Windows.Forms.TableLayoutPanel
    Private lblProgressTitle As System.Windows.Forms.Label
    Friend WithEvents lblProgressStage As System.Windows.Forms.Label
    Friend WithEvents progressTest As System.Windows.Forms.ProgressBar
    Friend WithEvents lblProgressCount As System.Windows.Forms.Label
    Friend WithEvents lblProgressElapsed As System.Windows.Forms.Label
    Friend WithEvents lblProgressEta As System.Windows.Forms.Label
    Friend WithEvents lblProgressDetail As System.Windows.Forms.Label
    Private lblProgressLogTitle As System.Windows.Forms.Label
    Friend WithEvents txtProgressLog As System.Windows.Forms.TextBox
    Friend WithEvents btnClose As System.Windows.Forms.Button
End Class
