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
        Me.components = New System.ComponentModel.Container()
        Me.outerLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.titleLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.hoverDetailsLabel = New System.Windows.Forms.Label()
        Me.cornerLabel = New System.Windows.Forms.Label()
        Me.columnHeader = New System.Windows.Forms.TableLayoutPanel()
        Me.rowHeader = New System.Windows.Forms.TableLayoutPanel()
        Me.gridSurface = New ChannelGridSurface()
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
        Me.SuspendLayout()
        Me.outerLayout.SuspendLayout()
        Me.titleLayout.SuspendLayout()
        Me.columnHeader.SuspendLayout()
        Me.rowHeader.SuspendLayout()
        ConfigureCoordinateLabel(Me.lblColumn1, "1")
        ConfigureCoordinateLabel(Me.lblColumn2, "2")
        ConfigureCoordinateLabel(Me.lblColumn3, "3")
        ConfigureCoordinateLabel(Me.lblColumn4, "4")
        ConfigureCoordinateLabel(Me.lblColumn5, "5")
        ConfigureCoordinateLabel(Me.lblColumn6, "6")
        ConfigureCoordinateLabel(Me.lblColumn7, "7")
        ConfigureCoordinateLabel(Me.lblColumn8, "8")
        ConfigureCoordinateLabel(Me.lblColumn9, "9")
        ConfigureCoordinateLabel(Me.lblColumn10, "10")
        ConfigureCoordinateLabel(Me.lblColumn11, "11")
        ConfigureCoordinateLabel(Me.lblColumn12, "12")
        ConfigureCoordinateLabel(Me.lblColumn13, "13")
        ConfigureCoordinateLabel(Me.lblColumn14, "14")
        ConfigureCoordinateLabel(Me.lblColumn15, "15")
        ConfigureCoordinateLabel(Me.lblColumn16, "16")
        ConfigureCoordinateLabel(Me.lblRow1, "1")
        ConfigureCoordinateLabel(Me.lblRow2, "2")
        ConfigureCoordinateLabel(Me.lblRow3, "3")
        ConfigureCoordinateLabel(Me.lblRow4, "4")
        ConfigureCoordinateLabel(Me.lblRow5, "5")
        ConfigureCoordinateLabel(Me.lblRow6, "6")
        ConfigureCoordinateLabel(Me.lblRow7, "7")
        ConfigureCoordinateLabel(Me.lblRow8, "8")
        ConfigureCoordinateLabel(Me.lblRow9, "9")
        ConfigureCoordinateLabel(Me.lblRow10, "10")
        ConfigureCoordinateLabel(Me.lblRow11, "11")
        ConfigureCoordinateLabel(Me.lblRow12, "12")
        ConfigureCoordinateLabel(Me.lblRow13, "13")
        ConfigureCoordinateLabel(Me.lblRow14, "14")
        ConfigureCoordinateLabel(Me.lblRow15, "15")
        ConfigureCoordinateLabel(Me.lblRow16, "16")
        Me.outerLayout.BackColor = UiTheme.CardBackground
        Me.outerLayout.ColumnCount = 2
        Me.outerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44.0!))
        Me.outerLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.outerLayout.Controls.Add(Me.titleLayout, 0, 0)
        Me.outerLayout.Controls.Add(Me.cornerLabel, 0, 1)
        Me.outerLayout.Controls.Add(Me.columnHeader, 1, 1)
        Me.outerLayout.Controls.Add(Me.rowHeader, 0, 2)
        Me.outerLayout.Controls.Add(Me.gridSurface, 1, 2)
        Me.outerLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.outerLayout.Location = New System.Drawing.Point(0, 0)
        Me.outerLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.outerLayout.Name = "outerLayout"
        Me.outerLayout.Padding = New System.Windows.Forms.Padding(0)
        Me.outerLayout.RowCount = 3
        Me.outerLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        Me.outerLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.0!))
        Me.outerLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.0!))
        Me.outerLayout.Size = New System.Drawing.Size(560, 420)
        Me.outerLayout.TabIndex = 0
        Me.outerLayout.SetColumnSpan(Me.titleLayout, 2)
        Me.titleLayout.BackColor = UiTheme.CardBackground
        Me.titleLayout.ColumnCount = 2
        Me.titleLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        Me.titleLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.titleLayout.Controls.Add(Me.titleLabel, 0, 0)
        Me.titleLayout.Controls.Add(Me.hoverDetailsLabel, 1, 0)
        Me.titleLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.titleLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.titleLayout.Name = "titleLayout"
        Me.titleLayout.RowCount = 1
        Me.titleLayout.Size = New System.Drawing.Size(560, 34)
        Me.titleLayout.TabIndex = 0
        Me.titleLabel.AutoSize = False
        Me.titleLabel.BackColor = UiTheme.CardBackground
        Me.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.titleLabel.Font = UiTheme.UiFontBold
        Me.titleLabel.ForeColor = UiTheme.Primary
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Text = "256通道状态总览（16 × 16）"
        Me.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.hoverDetailsLabel.AutoEllipsis = True
        Me.hoverDetailsLabel.AutoSize = False
        Me.hoverDetailsLabel.BackColor = UiTheme.CardBackground
        Me.hoverDetailsLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.hoverDetailsLabel.Font = UiTheme.UiFont
        Me.hoverDetailsLabel.ForeColor = UiTheme.TextPrimary
        Me.hoverDetailsLabel.Name = "hoverDetailsLabel"
        Me.hoverDetailsLabel.Padding = New System.Windows.Forms.Padding(8, 0, 4, 0)
        Me.hoverDetailsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cornerLabel.AutoSize = True
        Me.cornerLabel.BackColor = UiTheme.CardBackground
        Me.cornerLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.cornerLabel.Font = UiTheme.UiFontBold
        Me.cornerLabel.ForeColor = UiTheme.TextSecondary
        Me.cornerLabel.Name = "cornerLabel"
        Me.cornerLabel.Text = "状态"
        Me.cornerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.columnHeader.BackColor = UiTheme.CardBackground
        Me.columnHeader.ColumnCount = 16
        Me.columnHeader.Dock = System.Windows.Forms.DockStyle.Fill
        Me.columnHeader.Margin = New System.Windows.Forms.Padding(0)
        Me.columnHeader.Name = "columnHeader"
        Me.columnHeader.RowCount = 1
        Me.columnHeader.Size = New System.Drawing.Size(516, 25)
        Me.columnHeader.TabIndex = 1
        Me.rowHeader.BackColor = UiTheme.CardBackground
        Me.rowHeader.ColumnCount = 1
        Me.rowHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rowHeader.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rowHeader.Margin = New System.Windows.Forms.Padding(0)
        Me.rowHeader.Name = "rowHeader"
        Me.rowHeader.RowCount = 16
        Me.rowHeader.Size = New System.Drawing.Size(44, 361)
        Me.rowHeader.TabIndex = 2
        Me.gridSurface.BackColor = UiTheme.CardBackground
        Me.gridSurface.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridSurface.Margin = New System.Windows.Forms.Padding(0)
        Me.gridSurface.Name = "gridSurface"
        Me.gridSurface.TabIndex = 3
        AddColumnLabels()
        AddRowLabels()
        Me.BackColor = UiTheme.CardBackground
        Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Controls.Add(Me.outerLayout)
        Me.Name = "ChannelGridControl"
        Me.Padding = New System.Windows.Forms.Padding(10, 8, 10, 10)
        Me.Size = New System.Drawing.Size(560, 420)
        Me.outerLayout.ResumeLayout(False)
        Me.titleLayout.ResumeLayout(False)
        Me.columnHeader.ResumeLayout(False)
        Me.rowHeader.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Private Sub ConfigureCoordinateLabel(ByVal label As System.Windows.Forms.Label, ByVal text As String)
        label.AutoSize = False
        label.BackColor = UiTheme.CardBackground
        label.Dock = System.Windows.Forms.DockStyle.Fill
        label.Font = UiTheme.UiFontBold
        label.ForeColor = UiTheme.Primary
        label.Text = text
        label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
    End Sub

    Private Sub AddColumnLabels()
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
        For index As Integer = 0 To 15
            Me.columnHeader.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Next
    End Sub

    Private Sub AddRowLabels()
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
        For index As Integer = 0 To 15
            Me.rowHeader.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.25!))
        Next
    End Sub

    Private outerLayout As System.Windows.Forms.TableLayoutPanel
    Private titleLayout As System.Windows.Forms.TableLayoutPanel
    Private titleLabel As System.Windows.Forms.Label
    Private hoverDetailsLabel As System.Windows.Forms.Label
    Private cornerLabel As System.Windows.Forms.Label
    Private columnHeader As System.Windows.Forms.TableLayoutPanel
    Private rowHeader As System.Windows.Forms.TableLayoutPanel
    Private gridSurface As ChannelGridSurface
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
