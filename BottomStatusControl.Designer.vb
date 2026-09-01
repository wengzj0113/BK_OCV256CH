<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BottomStatusControl
    Inherits System.Windows.Forms.UserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.statusLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.stationLabel = New System.Windows.Forms.Label()
        Me.controlBoardLabel = New System.Windows.Forms.Label()
        Me.plcLabel = New System.Windows.Forms.Label()
        Me.voltageLabel = New System.Windows.Forms.Label()
        Me.resistanceLabel = New System.Windows.Forms.Label()
        Me.databaseLabel = New System.Windows.Forms.Label()
        Me.mesLabel = New System.Windows.Forms.Label()
        Me.userLabel = New System.Windows.Forms.Label()
        Me.timeLabel = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        Me.statusLayout.SuspendLayout()
        ConfigureStatusLabel(Me.stationLabel)
        ConfigureStatusLabel(Me.controlBoardLabel)
        ConfigureStatusLabel(Me.plcLabel)
        ConfigureStatusLabel(Me.voltageLabel)
        ConfigureStatusLabel(Me.resistanceLabel)
        ConfigureStatusLabel(Me.databaseLabel)
        ConfigureStatusLabel(Me.mesLabel)
        ConfigureStatusLabel(Me.userLabel)
        ConfigureStatusLabel(Me.timeLabel)
        Me.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.statusLayout.BackColor = UiTheme.HeaderBackground
        Me.statusLayout.ColumnCount = 9
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.0!))
        Me.statusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.statusLayout.Controls.Add(Me.stationLabel, 0, 0)
        Me.statusLayout.Controls.Add(Me.controlBoardLabel, 1, 0)
        Me.statusLayout.Controls.Add(Me.plcLabel, 2, 0)
        Me.statusLayout.Controls.Add(Me.voltageLabel, 3, 0)
        Me.statusLayout.Controls.Add(Me.resistanceLabel, 4, 0)
        Me.statusLayout.Controls.Add(Me.databaseLabel, 5, 0)
        Me.statusLayout.Controls.Add(Me.mesLabel, 6, 0)
        Me.statusLayout.Controls.Add(Me.userLabel, 7, 0)
        Me.statusLayout.Controls.Add(Me.timeLabel, 8, 0)
        Me.statusLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statusLayout.Location = New System.Drawing.Point(24, 0)
        Me.statusLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.statusLayout.Name = "statusLayout"
        Me.statusLayout.Padding = New System.Windows.Forms.Padding(0)
        Me.statusLayout.RowCount = 1
        Me.statusLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.statusLayout.Size = New System.Drawing.Size(952, 40)
        Me.statusLayout.TabIndex = 0
        Me.BackColor = UiTheme.HeaderBackground
        Me.Controls.Add(Me.statusLayout)
        Me.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Name = "BottomStatusControl"
        Me.Padding = New System.Windows.Forms.Padding(24, 0, 24, 0)
        Me.Size = New System.Drawing.Size(1000, 40)
        Me.statusLayout.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Private Sub ConfigureStatusLabel(ByVal label As System.Windows.Forms.Label)
        label.AutoEllipsis = True
        label.Dock = System.Windows.Forms.DockStyle.Fill
        label.Font = UiTheme.UiFont
        label.ForeColor = System.Drawing.Color.White
        label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    End Sub

    Private statusLayout As System.Windows.Forms.TableLayoutPanel
    Private stationLabel As System.Windows.Forms.Label
    Private controlBoardLabel As System.Windows.Forms.Label
    Private plcLabel As System.Windows.Forms.Label
    Private voltageLabel As System.Windows.Forms.Label
    Private resistanceLabel As System.Windows.Forms.Label
    Private databaseLabel As System.Windows.Forms.Label
    Private mesLabel As System.Windows.Forms.Label
    Private userLabel As System.Windows.Forms.Label
    Private timeLabel As System.Windows.Forms.Label
End Class
