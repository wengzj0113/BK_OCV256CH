<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ChannelGridControl
    Inherits System.Windows.Forms.UserControl

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
        Me.outerLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.titleLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.hoverDetailsLabel = New System.Windows.Forms.Label()
        Me.cornerLabel = New System.Windows.Forms.Label()
        Me.columnHeader = New System.Windows.Forms.TableLayoutPanel()
        Me.lblColumn1 = New System.Windows.Forms.Label()
        Me.lblColumn2 = New System.Windows.Forms.Label()
        Me.lblColumn3 = New System.Windows.Forms.Label()
        Me.lblColumn4 = New System.Windows.Forms.Label()
        Me.lblColumn5 = New System.Windows.Forms.Label()
        Me.lblColumn6 = New System.Windows.Forms.Label()
        Me.lblColumn7 = New System.Windows.Forms.Label()
        Me.lblColumn8 = New System.Windows.Forms.Label()
        Me.lblColumn9 = New System.Windows.Forms.Label()
        Me.lblColumn10 = New System.Windows.Forms.Label()
        Me.lblColumn11 = New System.Windows.Forms.Label()
        Me.lblColumn12 = New System.Windows.Forms.Label()
        Me.lblColumn13 = New System.Windows.Forms.Label()
        Me.lblColumn14 = New System.Windows.Forms.Label()
        Me.lblColumn15 = New System.Windows.Forms.Label()
        Me.lblColumn16 = New System.Windows.Forms.Label()
        Me.rowHeader = New System.Windows.Forms.TableLayoutPanel()
        Me.lblRow1 = New System.Windows.Forms.Label()
        Me.lblRow2 = New System.Windows.Forms.Label()
        Me.lblRow3 = New System.Windows.Forms.Label()
        Me.lblRow4 = New System.Windows.Forms.Label()
        Me.lblRow5 = New System.Windows.Forms.Label()
        Me.lblRow6 = New System.Windows.Forms.Label()
        Me.lblRow7 = New System.Windows.Forms.Label()
        Me.lblRow8 = New System.Windows.Forms.Label()
        Me.lblRow9 = New System.Windows.Forms.Label()
        Me.lblRow10 = New System.Windows.Forms.Label()
        Me.lblRow11 = New System.Windows.Forms.Label()
        Me.lblRow12 = New System.Windows.Forms.Label()
        Me.lblRow13 = New System.Windows.Forms.Label()
        Me.lblRow14 = New System.Windows.Forms.Label()
        Me.lblRow15 = New System.Windows.Forms.Label()
        Me.lblRow16 = New System.Windows.Forms.Label()
        Me.gridSurface = New System.Windows.Forms.Panel()
        Me.outerLayout.SuspendLayout()
        Me.titleLayout.SuspendLayout()
        Me.columnHeader.SuspendLayout()
        Me.rowHeader.SuspendLayout()
        Me.SuspendLayout()
        '
        'outerLayout
        '
        Me.outerLayout.BackColor = System.Drawing.Color.White
        Me.outerLayout.ColumnCount = 2
        Me.outerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44.0!))
        Me.outerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.outerLayout.Controls.Add(Me.titleLayout, 0, 0)
        Me.outerLayout.Controls.Add(Me.cornerLabel, 0, 1)
        Me.outerLayout.Controls.Add(Me.columnHeader, 1, 1)
        Me.outerLayout.Controls.Add(Me.rowHeader, 0, 2)
        Me.outerLayout.Controls.Add(Me.gridSurface, 1, 2)
        Me.outerLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.outerLayout.Location = New System.Drawing.Point(10, 8)
        Me.outerLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.outerLayout.Name = "outerLayout"
        Me.outerLayout.RowCount = 3
        Me.outerLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.outerLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.0!))
        Me.outerLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.0!))
        Me.outerLayout.Size = New System.Drawing.Size(540, 402)
        Me.outerLayout.TabIndex = 0
        '
        'titleLayout
        '
        Me.titleLayout.BackColor = System.Drawing.Color.White
        Me.titleLayout.ColumnCount = 2
        Me.outerLayout.SetColumnSpan(Me.titleLayout, 2)
        Me.titleLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.titleLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.titleLayout.Controls.Add(Me.titleLabel, 0, 0)
        Me.titleLayout.Controls.Add(Me.hoverDetailsLabel, 1, 0)
        Me.titleLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.titleLayout.Location = New System.Drawing.Point(0, 0)
        Me.titleLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.titleLayout.Name = "titleLayout"
        Me.titleLayout.RowCount = 1
        Me.titleLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.titleLayout.Size = New System.Drawing.Size(540, 34)
        Me.titleLayout.TabIndex = 0
        '
        'titleLabel
        '
        Me.titleLabel.BackColor = System.Drawing.Color.White
        Me.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.titleLabel.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.titleLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.titleLabel.Location = New System.Drawing.Point(3, 0)
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Size = New System.Drawing.Size(100, 34)
        Me.titleLabel.TabIndex = 0
        Me.titleLabel.Text = "256通道状态总览（16 × 16）"
        Me.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'hoverDetailsLabel
        '
        Me.hoverDetailsLabel.AutoEllipsis = True
        Me.hoverDetailsLabel.BackColor = System.Drawing.Color.White
        Me.hoverDetailsLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.hoverDetailsLabel.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.hoverDetailsLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.hoverDetailsLabel.Location = New System.Drawing.Point(109, 0)
        Me.hoverDetailsLabel.Name = "hoverDetailsLabel"
        Me.hoverDetailsLabel.Padding = New System.Windows.Forms.Padding(8, 0, 4, 0)
        Me.hoverDetailsLabel.Size = New System.Drawing.Size(428, 34)
        Me.hoverDetailsLabel.TabIndex = 1
        Me.hoverDetailsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'cornerLabel
        '
        Me.cornerLabel.AutoSize = True
        Me.cornerLabel.BackColor = System.Drawing.Color.White
        Me.cornerLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cornerLabel.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.cornerLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(99, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.cornerLabel.Location = New System.Drawing.Point(3, 34)
        Me.cornerLabel.Name = "cornerLabel"
        Me.cornerLabel.Size = New System.Drawing.Size(38, 23)
        Me.cornerLabel.TabIndex = 1
        Me.cornerLabel.Text = "状态"
        Me.cornerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'columnHeader
        '
        Me.columnHeader.BackColor = System.Drawing.Color.White
        Me.columnHeader.ColumnCount = 16
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.columnHeader.Controls.Add(Me.lblColumn1, 0, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn2, 1, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn3, 2, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn4, 3, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn5, 4, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn6, 5, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn7, 6, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn8, 7, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn9, 8, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn10, 9, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn11, 10, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn12, 11, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn13, 12, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn14, 13, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn15, 14, 0)
        Me.columnHeader.Controls.Add(Me.lblColumn16, 15, 0)
        Me.columnHeader.Dock = System.Windows.Forms.DockStyle.Fill
        Me.columnHeader.Location = New System.Drawing.Point(44, 34)
        Me.columnHeader.Margin = New System.Windows.Forms.Padding(0)
        Me.columnHeader.Name = "columnHeader"
        Me.columnHeader.RowCount = 1
        Me.columnHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.columnHeader.Size = New System.Drawing.Size(496, 23)
        Me.columnHeader.TabIndex = 1
        '
        'lblColumn1
        '
        Me.lblColumn1.BackColor = System.Drawing.Color.White
        Me.lblColumn1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn1.Location = New System.Drawing.Point(3, 0)
        Me.lblColumn1.Name = "lblColumn1"
        Me.lblColumn1.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn1.TabIndex = 0
        Me.lblColumn1.Text = "1"
        Me.lblColumn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn2
        '
        Me.lblColumn2.BackColor = System.Drawing.Color.White
        Me.lblColumn2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn2.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn2.Location = New System.Drawing.Point(34, 0)
        Me.lblColumn2.Name = "lblColumn2"
        Me.lblColumn2.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn2.TabIndex = 1
        Me.lblColumn2.Text = "2"
        Me.lblColumn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn3
        '
        Me.lblColumn3.BackColor = System.Drawing.Color.White
        Me.lblColumn3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn3.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn3.Location = New System.Drawing.Point(65, 0)
        Me.lblColumn3.Name = "lblColumn3"
        Me.lblColumn3.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn3.TabIndex = 2
        Me.lblColumn3.Text = "3"
        Me.lblColumn3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn4
        '
        Me.lblColumn4.BackColor = System.Drawing.Color.White
        Me.lblColumn4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn4.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn4.Location = New System.Drawing.Point(96, 0)
        Me.lblColumn4.Name = "lblColumn4"
        Me.lblColumn4.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn4.TabIndex = 3
        Me.lblColumn4.Text = "4"
        Me.lblColumn4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn5
        '
        Me.lblColumn5.BackColor = System.Drawing.Color.White
        Me.lblColumn5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn5.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn5.Location = New System.Drawing.Point(127, 0)
        Me.lblColumn5.Name = "lblColumn5"
        Me.lblColumn5.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn5.TabIndex = 4
        Me.lblColumn5.Text = "5"
        Me.lblColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn6
        '
        Me.lblColumn6.BackColor = System.Drawing.Color.White
        Me.lblColumn6.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn6.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn6.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn6.Location = New System.Drawing.Point(158, 0)
        Me.lblColumn6.Name = "lblColumn6"
        Me.lblColumn6.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn6.TabIndex = 5
        Me.lblColumn6.Text = "6"
        Me.lblColumn6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn7
        '
        Me.lblColumn7.BackColor = System.Drawing.Color.White
        Me.lblColumn7.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn7.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn7.Location = New System.Drawing.Point(189, 0)
        Me.lblColumn7.Name = "lblColumn7"
        Me.lblColumn7.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn7.TabIndex = 6
        Me.lblColumn7.Text = "7"
        Me.lblColumn7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn8
        '
        Me.lblColumn8.BackColor = System.Drawing.Color.White
        Me.lblColumn8.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn8.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn8.Location = New System.Drawing.Point(220, 0)
        Me.lblColumn8.Name = "lblColumn8"
        Me.lblColumn8.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn8.TabIndex = 7
        Me.lblColumn8.Text = "8"
        Me.lblColumn8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn9
        '
        Me.lblColumn9.BackColor = System.Drawing.Color.White
        Me.lblColumn9.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn9.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn9.Location = New System.Drawing.Point(251, 0)
        Me.lblColumn9.Name = "lblColumn9"
        Me.lblColumn9.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn9.TabIndex = 8
        Me.lblColumn9.Text = "9"
        Me.lblColumn9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn10
        '
        Me.lblColumn10.BackColor = System.Drawing.Color.White
        Me.lblColumn10.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn10.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn10.Location = New System.Drawing.Point(282, 0)
        Me.lblColumn10.Name = "lblColumn10"
        Me.lblColumn10.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn10.TabIndex = 9
        Me.lblColumn10.Text = "10"
        Me.lblColumn10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn11
        '
        Me.lblColumn11.BackColor = System.Drawing.Color.White
        Me.lblColumn11.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn11.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn11.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn11.Location = New System.Drawing.Point(313, 0)
        Me.lblColumn11.Name = "lblColumn11"
        Me.lblColumn11.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn11.TabIndex = 10
        Me.lblColumn11.Text = "11"
        Me.lblColumn11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn12
        '
        Me.lblColumn12.BackColor = System.Drawing.Color.White
        Me.lblColumn12.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn12.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn12.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn12.Location = New System.Drawing.Point(344, 0)
        Me.lblColumn12.Name = "lblColumn12"
        Me.lblColumn12.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn12.TabIndex = 11
        Me.lblColumn12.Text = "12"
        Me.lblColumn12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn13
        '
        Me.lblColumn13.BackColor = System.Drawing.Color.White
        Me.lblColumn13.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn13.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn13.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn13.Location = New System.Drawing.Point(375, 0)
        Me.lblColumn13.Name = "lblColumn13"
        Me.lblColumn13.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn13.TabIndex = 12
        Me.lblColumn13.Text = "13"
        Me.lblColumn13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn14
        '
        Me.lblColumn14.BackColor = System.Drawing.Color.White
        Me.lblColumn14.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn14.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn14.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn14.Location = New System.Drawing.Point(406, 0)
        Me.lblColumn14.Name = "lblColumn14"
        Me.lblColumn14.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn14.TabIndex = 13
        Me.lblColumn14.Text = "14"
        Me.lblColumn14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn15
        '
        Me.lblColumn15.BackColor = System.Drawing.Color.White
        Me.lblColumn15.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn15.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn15.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn15.Location = New System.Drawing.Point(437, 0)
        Me.lblColumn15.Name = "lblColumn15"
        Me.lblColumn15.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn15.TabIndex = 14
        Me.lblColumn15.Text = "15"
        Me.lblColumn15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblColumn16
        '
        Me.lblColumn16.BackColor = System.Drawing.Color.White
        Me.lblColumn16.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblColumn16.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblColumn16.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblColumn16.Location = New System.Drawing.Point(468, 0)
        Me.lblColumn16.Name = "lblColumn16"
        Me.lblColumn16.Size = New System.Drawing.Size(25, 23)
        Me.lblColumn16.TabIndex = 15
        Me.lblColumn16.Text = "16"
        Me.lblColumn16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'rowHeader
        '
        Me.rowHeader.BackColor = System.Drawing.Color.White
        Me.rowHeader.ColumnCount = 1
        Me.rowHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rowHeader.Controls.Add(Me.lblRow1, 0, 0)
        Me.rowHeader.Controls.Add(Me.lblRow2, 0, 1)
        Me.rowHeader.Controls.Add(Me.lblRow3, 0, 2)
        Me.rowHeader.Controls.Add(Me.lblRow4, 0, 3)
        Me.rowHeader.Controls.Add(Me.lblRow5, 0, 4)
        Me.rowHeader.Controls.Add(Me.lblRow6, 0, 5)
        Me.rowHeader.Controls.Add(Me.lblRow7, 0, 6)
        Me.rowHeader.Controls.Add(Me.lblRow8, 0, 7)
        Me.rowHeader.Controls.Add(Me.lblRow9, 0, 8)
        Me.rowHeader.Controls.Add(Me.lblRow10, 0, 9)
        Me.rowHeader.Controls.Add(Me.lblRow11, 0, 10)
        Me.rowHeader.Controls.Add(Me.lblRow12, 0, 11)
        Me.rowHeader.Controls.Add(Me.lblRow13, 0, 12)
        Me.rowHeader.Controls.Add(Me.lblRow14, 0, 13)
        Me.rowHeader.Controls.Add(Me.lblRow15, 0, 14)
        Me.rowHeader.Controls.Add(Me.lblRow16, 0, 15)
        Me.rowHeader.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rowHeader.Location = New System.Drawing.Point(0, 57)
        Me.rowHeader.Margin = New System.Windows.Forms.Padding(0)
        Me.rowHeader.Name = "rowHeader"
        Me.rowHeader.RowCount = 16
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Me.rowHeader.Size = New System.Drawing.Size(44, 345)
        Me.rowHeader.TabIndex = 2
        '
        'lblRow1
        '
        Me.lblRow1.BackColor = System.Drawing.Color.White
        Me.lblRow1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow1.Location = New System.Drawing.Point(3, 0)
        Me.lblRow1.Name = "lblRow1"
        Me.lblRow1.Size = New System.Drawing.Size(38, 21)
        Me.lblRow1.TabIndex = 0
        Me.lblRow1.Text = "1"
        Me.lblRow1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow2
        '
        Me.lblRow2.BackColor = System.Drawing.Color.White
        Me.lblRow2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow2.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow2.Location = New System.Drawing.Point(3, 21)
        Me.lblRow2.Name = "lblRow2"
        Me.lblRow2.Size = New System.Drawing.Size(38, 21)
        Me.lblRow2.TabIndex = 1
        Me.lblRow2.Text = "2"
        Me.lblRow2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow3
        '
        Me.lblRow3.BackColor = System.Drawing.Color.White
        Me.lblRow3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow3.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow3.Location = New System.Drawing.Point(3, 42)
        Me.lblRow3.Name = "lblRow3"
        Me.lblRow3.Size = New System.Drawing.Size(38, 21)
        Me.lblRow3.TabIndex = 2
        Me.lblRow3.Text = "3"
        Me.lblRow3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow4
        '
        Me.lblRow4.BackColor = System.Drawing.Color.White
        Me.lblRow4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow4.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow4.Location = New System.Drawing.Point(3, 63)
        Me.lblRow4.Name = "lblRow4"
        Me.lblRow4.Size = New System.Drawing.Size(38, 21)
        Me.lblRow4.TabIndex = 3
        Me.lblRow4.Text = "4"
        Me.lblRow4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow5
        '
        Me.lblRow5.BackColor = System.Drawing.Color.White
        Me.lblRow5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow5.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow5.Location = New System.Drawing.Point(3, 84)
        Me.lblRow5.Name = "lblRow5"
        Me.lblRow5.Size = New System.Drawing.Size(38, 21)
        Me.lblRow5.TabIndex = 4
        Me.lblRow5.Text = "5"
        Me.lblRow5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow6
        '
        Me.lblRow6.BackColor = System.Drawing.Color.White
        Me.lblRow6.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow6.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow6.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow6.Location = New System.Drawing.Point(3, 105)
        Me.lblRow6.Name = "lblRow6"
        Me.lblRow6.Size = New System.Drawing.Size(38, 21)
        Me.lblRow6.TabIndex = 5
        Me.lblRow6.Text = "6"
        Me.lblRow6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow7
        '
        Me.lblRow7.BackColor = System.Drawing.Color.White
        Me.lblRow7.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow7.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow7.Location = New System.Drawing.Point(3, 126)
        Me.lblRow7.Name = "lblRow7"
        Me.lblRow7.Size = New System.Drawing.Size(38, 21)
        Me.lblRow7.TabIndex = 6
        Me.lblRow7.Text = "7"
        Me.lblRow7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow8
        '
        Me.lblRow8.BackColor = System.Drawing.Color.White
        Me.lblRow8.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow8.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow8.Location = New System.Drawing.Point(3, 147)
        Me.lblRow8.Name = "lblRow8"
        Me.lblRow8.Size = New System.Drawing.Size(38, 21)
        Me.lblRow8.TabIndex = 7
        Me.lblRow8.Text = "8"
        Me.lblRow8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow9
        '
        Me.lblRow9.BackColor = System.Drawing.Color.White
        Me.lblRow9.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow9.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow9.Location = New System.Drawing.Point(3, 168)
        Me.lblRow9.Name = "lblRow9"
        Me.lblRow9.Size = New System.Drawing.Size(38, 21)
        Me.lblRow9.TabIndex = 8
        Me.lblRow9.Text = "9"
        Me.lblRow9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow10
        '
        Me.lblRow10.BackColor = System.Drawing.Color.White
        Me.lblRow10.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow10.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow10.Location = New System.Drawing.Point(3, 189)
        Me.lblRow10.Name = "lblRow10"
        Me.lblRow10.Size = New System.Drawing.Size(38, 21)
        Me.lblRow10.TabIndex = 9
        Me.lblRow10.Text = "10"
        Me.lblRow10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow11
        '
        Me.lblRow11.BackColor = System.Drawing.Color.White
        Me.lblRow11.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow11.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow11.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow11.Location = New System.Drawing.Point(3, 210)
        Me.lblRow11.Name = "lblRow11"
        Me.lblRow11.Size = New System.Drawing.Size(38, 21)
        Me.lblRow11.TabIndex = 10
        Me.lblRow11.Text = "11"
        Me.lblRow11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow12
        '
        Me.lblRow12.BackColor = System.Drawing.Color.White
        Me.lblRow12.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow12.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow12.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow12.Location = New System.Drawing.Point(3, 231)
        Me.lblRow12.Name = "lblRow12"
        Me.lblRow12.Size = New System.Drawing.Size(38, 21)
        Me.lblRow12.TabIndex = 11
        Me.lblRow12.Text = "12"
        Me.lblRow12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow13
        '
        Me.lblRow13.BackColor = System.Drawing.Color.White
        Me.lblRow13.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow13.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow13.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow13.Location = New System.Drawing.Point(3, 252)
        Me.lblRow13.Name = "lblRow13"
        Me.lblRow13.Size = New System.Drawing.Size(38, 21)
        Me.lblRow13.TabIndex = 12
        Me.lblRow13.Text = "13"
        Me.lblRow13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow14
        '
        Me.lblRow14.BackColor = System.Drawing.Color.White
        Me.lblRow14.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow14.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow14.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow14.Location = New System.Drawing.Point(3, 273)
        Me.lblRow14.Name = "lblRow14"
        Me.lblRow14.Size = New System.Drawing.Size(38, 21)
        Me.lblRow14.TabIndex = 13
        Me.lblRow14.Text = "14"
        Me.lblRow14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow15
        '
        Me.lblRow15.BackColor = System.Drawing.Color.White
        Me.lblRow15.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow15.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow15.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow15.Location = New System.Drawing.Point(3, 294)
        Me.lblRow15.Name = "lblRow15"
        Me.lblRow15.Size = New System.Drawing.Size(38, 21)
        Me.lblRow15.TabIndex = 14
        Me.lblRow15.Text = "15"
        Me.lblRow15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblRow16
        '
        Me.lblRow16.BackColor = System.Drawing.Color.White
        Me.lblRow16.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRow16.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblRow16.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.lblRow16.Location = New System.Drawing.Point(3, 315)
        Me.lblRow16.Name = "lblRow16"
        Me.lblRow16.Size = New System.Drawing.Size(38, 30)
        Me.lblRow16.TabIndex = 15
        Me.lblRow16.Text = "16"
        Me.lblRow16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'gridSurface
        '
        Me.gridSurface.BackColor = System.Drawing.Color.White
        Me.gridSurface.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridSurface.Location = New System.Drawing.Point(44, 57)
        Me.gridSurface.Margin = New System.Windows.Forms.Padding(0)
        Me.gridSurface.Name = "gridSurface"
        Me.gridSurface.Size = New System.Drawing.Size(496, 345)
        Me.gridSurface.TabIndex = 3
        '
        'ChannelGridControl
        '
        Me.BackColor = System.Drawing.Color.White
        Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Controls.Add(Me.outerLayout)
        Me.Name = "ChannelGridControl"
        Me.Padding = New System.Windows.Forms.Padding(10, 8, 10, 10)
        Me.Size = New System.Drawing.Size(560, 420)
        Me.outerLayout.ResumeLayout(False)
        Me.outerLayout.PerformLayout()
        Me.titleLayout.ResumeLayout(False)
        Me.columnHeader.ResumeLayout(False)
        Me.rowHeader.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private outerLayout As System.Windows.Forms.TableLayoutPanel
    Private titleLayout As System.Windows.Forms.TableLayoutPanel
    Private titleLabel As System.Windows.Forms.Label
    Private hoverDetailsLabel As System.Windows.Forms.Label
    Private cornerLabel As System.Windows.Forms.Label
    Private columnHeader As System.Windows.Forms.TableLayoutPanel
    Private rowHeader As System.Windows.Forms.TableLayoutPanel
    Private gridSurface As System.Windows.Forms.Panel
    Private lblColumn1 As System.Windows.Forms.Label
    Private lblColumn2 As System.Windows.Forms.Label
    Private lblColumn3 As System.Windows.Forms.Label
    Private lblColumn4 As System.Windows.Forms.Label
    Private lblColumn5 As System.Windows.Forms.Label
    Private lblColumn6 As System.Windows.Forms.Label
    Private lblColumn7 As System.Windows.Forms.Label
    Private lblColumn8 As System.Windows.Forms.Label
    Private lblColumn9 As System.Windows.Forms.Label
    Private lblColumn10 As System.Windows.Forms.Label
    Private lblColumn11 As System.Windows.Forms.Label
    Private lblColumn12 As System.Windows.Forms.Label
    Private lblColumn13 As System.Windows.Forms.Label
    Private lblColumn14 As System.Windows.Forms.Label
    Private lblColumn15 As System.Windows.Forms.Label
    Private lblColumn16 As System.Windows.Forms.Label
    Private lblRow1 As System.Windows.Forms.Label
    Private lblRow2 As System.Windows.Forms.Label
    Private lblRow3 As System.Windows.Forms.Label
    Private lblRow4 As System.Windows.Forms.Label
    Private lblRow5 As System.Windows.Forms.Label
    Private lblRow6 As System.Windows.Forms.Label
    Private lblRow7 As System.Windows.Forms.Label
    Private lblRow8 As System.Windows.Forms.Label
    Private lblRow9 As System.Windows.Forms.Label
    Private lblRow10 As System.Windows.Forms.Label
    Private lblRow11 As System.Windows.Forms.Label
    Private lblRow12 As System.Windows.Forms.Label
    Private lblRow13 As System.Windows.Forms.Label
    Private lblRow14 As System.Windows.Forms.Label
    Private lblRow15 As System.Windows.Forms.Label
    Private lblRow16 As System.Windows.Forms.Label
End Class
