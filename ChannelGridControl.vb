Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Enum ChannelSelectionMode
    SingleChannel
    Rectangle
End Enum

Public Class ChannelGridControl
    Inherits UserControl

    Private Const GridSize As Integer = 16
    Private Const TotalChannels As Integer = GridSize * GridSize
    Private ReadOnly displayDataByChannel As New Dictionary(Of Integer, ChannelDisplayData)()
    Private ReadOnly gridSurface As New ChannelGridSurface()
    Private ReadOnly hoverDetailsLabel As Label
    Private ReadOnly selectedChannels As New HashSet(Of Integer)()
    Private batchUpdateDepth As Integer
    Private batchGridWasVisible As Boolean
    Private selectionModeValue As ChannelSelectionMode = ChannelSelectionMode.SingleChannel
    Private isSelectingRectangle As Boolean
    Private rectangleMoved As Boolean
    Private selectionStart As Point
    Private selectionCurrent As Point

    Public Event ChannelSelected As EventHandler(Of ChannelSelectedEventArgs)
    Public Event SelectedChannelsChanged As EventHandler

    Public Sub New()
        BackColor = UiTheme.CardBackground
        BorderStyle = BorderStyle.FixedSingle
        Padding = New Padding(10, 8, 10, 10)

        Dim outerLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .AutoSize = False,
            .MinimumSize = New Size(0, 0),
            .ColumnCount = 2,
            .RowCount = 3,
            .BackColor = UiTheme.CardBackground,
            .Padding = New Padding(0)
        }
        outerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 44.0F))
        outerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        outerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        outerLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 6.0F))
        outerLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 89.0F))

        Dim title As New Label With {
            .Dock = DockStyle.Fill,
            .AutoSize = False,
            .Text = "256通道状态总览（16 × 16）",
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = UiTheme.UiFontBold,
            .ForeColor = UiTheme.Primary,
            .BackColor = UiTheme.CardBackground
        }
        hoverDetailsLabel = New Label With {
            .Dock = DockStyle.Fill,
            .AutoSize = False,
            .AutoEllipsis = True,
            .TextAlign = ContentAlignment.MiddleRight,
            .Font = UiTheme.UiFont,
            .ForeColor = UiTheme.TextPrimary,
            .BackColor = UiTheme.CardBackground,
            .Padding = New Padding(8, 0, 4, 0)
        }
        Dim titleLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = UiTheme.CardBackground,
            .Margin = New Padding(0)
        }
        titleLayout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        titleLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        titleLayout.Controls.Add(title, 0, 0)
        titleLayout.Controls.Add(hoverDetailsLabel, 1, 0)
        outerLayout.Controls.Add(titleLayout, 0, 0)
        outerLayout.SetColumnSpan(titleLayout, 2)

        Dim corner As New Label With {
            .Dock = DockStyle.Fill,
            .AutoSize = True,
            .BackColor = UiTheme.CardBackground,
            .Text = "状态",
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = UiTheme.UiFontBold,
            .ForeColor = UiTheme.TextSecondary
        }
        outerLayout.Controls.Add(corner, 0, 1)

        Dim columnHeader As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = GridSize,
            .RowCount = 1,
            .BackColor = UiTheme.CardBackground,
            .Margin = New Padding(0)
        }
        For column As Integer = 1 To GridSize
            columnHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / GridSize))
            columnHeader.Controls.Add(CreateCoordinateLabel(column.ToString()), column - 1, 0)
        Next
        outerLayout.Controls.Add(columnHeader, 1, 1)

        Dim rowHeader As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = GridSize,
            .BackColor = UiTheme.CardBackground
        }
        rowHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        For row As Integer = 1 To GridSize
            rowHeader.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / GridSize))
            rowHeader.Controls.Add(CreateCoordinateLabel(row.ToString()), 0, row - 1)
        Next
        outerLayout.Controls.Add(rowHeader, 0, 2)

        gridSurface.Dock = DockStyle.Fill
        gridSurface.BackColor = UiTheme.CardBackground
        gridSurface.Margin = New Padding(0)
        AddHandler gridSurface.Paint, AddressOf HandleGridPaint
        AddHandler gridSurface.MouseDown, AddressOf HandleGridMouseDown
        AddHandler gridSurface.MouseMove, AddressOf HandleGridMouseMove
        AddHandler gridSurface.MouseUp, AddressOf HandleGridMouseUp
        AddHandler gridSurface.MouseLeave, AddressOf HandleGridMouseLeave
        outerLayout.Controls.Add(gridSurface, 1, 2)
        Controls.Add(outerLayout)
    End Sub

    Public ReadOnly Property TileCount As Integer
        Get
            Return TotalChannels
        End Get
    End Property

    Public Property SelectionMode As ChannelSelectionMode
        Get
            Return selectionModeValue
        End Get
        Set(ByVal value As ChannelSelectionMode)
            selectionModeValue = value
        End Set
    End Property

    Public ReadOnly Property SelectedChannelNumbers As List(Of Integer)
        Get
            Return selectedChannels.OrderBy(Function(channel) channel).ToList()
        End Get
    End Property

    Public ReadOnly Property SelectedChannelCount As Integer
        Get
            Return selectedChannels.Count
        End Get
    End Property

    Public Sub BeginBatchUpdate()
        If batchUpdateDepth = 0 Then
            batchGridWasVisible = gridSurface.Visible
            SuspendLayout()
            gridSurface.SuspendLayout()
            gridSurface.Visible = False
        End If
        batchUpdateDepth += 1
    End Sub

    Public Sub EndBatchUpdate()
        If batchUpdateDepth <= 0 Then Return

        batchUpdateDepth -= 1
        If batchUpdateDepth > 0 Then Return

        gridSurface.Visible = batchGridWasVisible
        gridSurface.ResumeLayout(False)
        ResumeLayout(False)
        If batchGridWasVisible Then
            gridSurface.Invalidate()
            Invalidate()
        End If
    End Sub

    Public Sub SelectAll()
        selectedChannels.Clear()
        For channelNo As Integer = 1 To TotalChannels
            selectedChannels.Add(channelNo)
        Next
        gridSurface.Invalidate()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub ClearSelection()
        If selectedChannels.Count = 0 Then Return
        selectedChannels.Clear()
        gridSurface.Invalidate()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub UpdateChannel(ByVal channelNo As Integer, ByVal data As ChannelDisplayData)
        If channelNo < 1 OrElse channelNo > TotalChannels Then Return
        displayDataByChannel(channelNo) = data
        If batchUpdateDepth = 0 Then gridSurface.Invalidate()
    End Sub

    Public Sub UpdateAll(ByVal data As IEnumerable(Of ChannelDisplayData))
        If data Is Nothing Then Return
        BeginBatchUpdate()
        Try
            For Each channel As ChannelDisplayData In data
                UpdateChannel(channel.ChannelNo, channel)
            Next
        Finally
            EndBatchUpdate()
        End Try
    End Sub

    Private Sub HandleGridMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then Return
        If selectionModeValue = ChannelSelectionMode.Rectangle Then
            isSelectingRectangle = True
            rectangleMoved = False
            selectionStart = e.Location
            selectionCurrent = selectionStart
            gridSurface.Capture = True
            gridSurface.Invalidate()
            Return
        End If

        Dim channelNo As Integer = GetChannelAtPoint(e.Location)
        Dim data As ChannelDisplayData = Nothing
        If channelNo > 0 AndAlso displayDataByChannel.TryGetValue(channelNo, data) Then
            ToggleSelection(channelNo)
            RaiseEvent ChannelSelected(Me, New ChannelSelectedEventArgs(data))
        End If
    End Sub

    Private Sub HandleGridMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If isSelectingRectangle Then
            selectionCurrent = e.Location
            rectangleMoved = rectangleMoved OrElse selectionCurrent <> selectionStart
            gridSurface.Invalidate()
            Return
        End If

        Dim channelNo As Integer = GetChannelAtPoint(e.Location)
        Dim data As ChannelDisplayData = Nothing
        If channelNo > 0 AndAlso displayDataByChannel.TryGetValue(channelNo, data) Then
            hoverDetailsLabel.Text = BuildHoverDetailsText(data)
        Else
            hoverDetailsLabel.Text = String.Empty
        End If
    End Sub

    Private Sub HandleGridMouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        If Not isSelectingRectangle OrElse e.Button <> MouseButtons.Left Then Return
        selectionCurrent = e.Location
        isSelectingRectangle = False
        gridSurface.Capture = False

        Dim selectionRectangle As Rectangle = NormalizeRectangle(selectionStart, selectionCurrent)
        selectedChannels.Clear()
        For channelNo As Integer = 1 To TotalChannels
            If selectionRectangle.IntersectsWith(GetCellBounds(channelNo)) Then selectedChannels.Add(channelNo)
        Next
        gridSurface.Invalidate()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub HandleGridMouseLeave(ByVal sender As Object, ByVal e As EventArgs)
        If Not isSelectingRectangle Then hoverDetailsLabel.Text = String.Empty
    End Sub

    Private Sub ToggleSelection(ByVal channelNo As Integer)
        If selectedChannels.Contains(channelNo) Then
            selectedChannels.Remove(channelNo)
        Else
            selectedChannels.Add(channelNo)
        End If
        gridSurface.Invalidate()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Private Function GetChannelAtPoint(ByVal point As Point) As Integer
        If point.X < 0 OrElse point.Y < 0 OrElse point.X >= gridSurface.ClientSize.Width OrElse point.Y >= gridSurface.ClientSize.Height Then Return 0
        Dim column As Integer = Math.Min(GridSize - 1, point.X * GridSize \ Math.Max(1, gridSurface.ClientSize.Width))
        Dim row As Integer = Math.Min(GridSize - 1, point.Y * GridSize \ Math.Max(1, gridSurface.ClientSize.Height))
        Return column * GridSize + row + 1
    End Function

    Private Function GetCellBounds(ByVal channelNo As Integer) As Rectangle
        Dim index As Integer = channelNo - 1
        Dim column As Integer = index \ GridSize
        Dim row As Integer = index Mod GridSize
        Dim width As Integer = Math.Max(1, gridSurface.ClientSize.Width \ GridSize)
        Dim height As Integer = Math.Max(1, gridSurface.ClientSize.Height \ GridSize)
        Return New Rectangle(column * width, row * height, If(column = GridSize - 1, gridSurface.ClientSize.Width - column * width, width), If(row = GridSize - 1, gridSurface.ClientSize.Height - row * height, height))
    End Function

    Private Sub HandleGridPaint(ByVal sender As Object, ByVal e As PaintEventArgs)
        For channelNo As Integer = 1 To TotalChannels
            Dim bounds As Rectangle = GetCellBounds(channelNo)
            bounds.Inflate(-2, -2)
            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Continue For

            Dim data As ChannelDisplayData = Nothing
            Dim background As Color = UiTheme.IdleBackground
            Dim foreground As Color = UiTheme.TextPrimary
            If displayDataByChannel.TryGetValue(channelNo, data) Then
                GetStatusColors(data.StatusText, background, foreground)
            End If

            Using brush As New SolidBrush(background)
                e.Graphics.FillRectangle(brush, bounds)
            End Using
            Using pen As New Pen(If(selectedChannels.Contains(channelNo), UiTheme.Primary, UiTheme.CardBorder), If(selectedChannels.Contains(channelNo), 2.0F, 1.0F))
                e.Graphics.DrawRectangle(pen, bounds)
            End Using

            Dim text As String = If(displayDataByChannel.ContainsKey(channelNo), data.Row.ToString() & "-" & data.Column.ToString(), channelNo.ToString())
            TextRenderer.DrawText(e.Graphics, text, UiTheme.UiFont, bounds, foreground,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        Next

        If isSelectingRectangle Then
            Dim selectionRectangle As Rectangle = NormalizeRectangle(selectionStart, selectionCurrent)
            Using fillBrush As New SolidBrush(Color.FromArgb(45, UiTheme.Primary))
                e.Graphics.FillRectangle(fillBrush, selectionRectangle)
            End Using
            Using borderPen As New Pen(UiTheme.Primary, 1.0F)
                e.Graphics.DrawRectangle(borderPen, selectionRectangle)
            End Using
        End If
    End Sub

    Private Shared Sub GetStatusColors(ByVal status As String, ByRef background As Color, ByRef foreground As Color)
        Select Case If(status, String.Empty).Trim().ToUpperInvariant()
            Case "PASS", "OK"
                background = UiTheme.SuccessBackground
                foreground = UiTheme.Success
            Case "NG", "ERROR"
                background = UiTheme.Danger
                foreground = Color.White
            Case "TESTING"
                background = UiTheme.TestingBackground
                foreground = Color.White
            Case "WAITING"
                background = ColorTranslator.FromHtml("#FFF2C7")
                foreground = UiTheme.TextPrimary
            Case "DISABLED"
                background = UiTheme.IdleBackground
                foreground = UiTheme.TextSecondary
            Case Else
                background = UiTheme.IdleBackground
                foreground = UiTheme.TextPrimary
        End Select
    End Sub

    Private Shared Function NormalizeRectangle(ByVal first As Point, ByVal second As Point) As Rectangle
        Return Rectangle.FromLTRB(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Max(first.X, second.X), Math.Max(first.Y, second.Y))
    End Function

    Private Shared Function BuildHoverDetailsText(ByVal data As ChannelDisplayData) As String
        Dim ocvText As String = If(data.Ocv1.HasValue, data.Ocv1.Value.ToString("0.00") & " mV", "-")
        Dim irText As String = If(data.Ir1.HasValue, data.Ir1.Value.ToString("0.000") & " mΩ", "-")
        Dim barcode As String = If(String.IsNullOrWhiteSpace(data.Barcode), "-", data.Barcode)
        Dim statusText As String = If(String.IsNullOrWhiteSpace(data.StatusText), "IDLE", data.StatusText)
        Dim resultText As String = If(String.IsNullOrWhiteSpace(data.ResultText), "-", data.ResultText)
        Return String.Format("通道 {0}-{1}：{2}，电池条码：{3}，OCV1：{4}，IR1：{5}，结果：{6}", data.Row, data.Column, statusText, barcode, ocvText, irText, resultText)
    End Function

    Private Shared Function CreateCoordinateLabel(ByVal text As String) As Label
        Return New Label With {
            .Text = text,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = UiTheme.UiFontBold,
            .ForeColor = UiTheme.Primary,
            .BackColor = UiTheme.CardBackground
        }
    End Function

    Private NotInheritable Class ChannelGridSurface
        Inherits Control

        Public Sub New()
            SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        End Sub
    End Class
End Class
