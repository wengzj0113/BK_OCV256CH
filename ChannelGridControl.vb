Imports System.Drawing
Imports System.Linq
Imports System.Reflection
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
    Private ReadOnly selectedChannels As New HashSet(Of Integer)()
    Private statusColors As ChannelStatusColors = ChannelStatusColors.CreateDefault()
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
        InitializeComponent()
        hoverDetailsLabel.ForeColor = UiTheme.TextPrimary
        GetType(Panel).GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic).SetValue(gridSurface, True, Nothing)
        AddHandler gridSurface.Paint, AddressOf HandleGridPaint
        AddHandler gridSurface.MouseDown, AddressOf HandleGridMouseDown
        AddHandler gridSurface.MouseMove, AddressOf HandleGridMouseMove
        AddHandler gridSurface.MouseUp, AddressOf HandleGridMouseUp
        AddHandler gridSurface.MouseLeave, AddressOf HandleGridMouseLeave
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

    Public Sub ApplyStatusColors(ByVal colors As ChannelStatusColors)
        statusColors = If(colors, ChannelStatusColors.CreateDefault())
        If gridSurface IsNot Nothing Then gridSurface.Invalidate()
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
            Using pen As New Pen(If(selectedChannels.Contains(channelNo), statusColors.SelectedColor, UiTheme.CardBorder), If(selectedChannels.Contains(channelNo), 2.0F, 1.0F))
                e.Graphics.DrawRectangle(pen, bounds)
            End Using

            Dim text As String = If(displayDataByChannel.ContainsKey(channelNo), data.Row.ToString() & "-" & data.Column.ToString(), channelNo.ToString())
            TextRenderer.DrawText(e.Graphics, text, UiTheme.UiFont, bounds, foreground,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        Next

        If isSelectingRectangle Then
            Dim selectionRectangle As Rectangle = NormalizeRectangle(selectionStart, selectionCurrent)
            Using fillBrush As New SolidBrush(Color.FromArgb(45, statusColors.SelectedColor))
                e.Graphics.FillRectangle(fillBrush, selectionRectangle)
            End Using
            Using borderPen As New Pen(statusColors.SelectedColor, 1.0F)
                e.Graphics.DrawRectangle(borderPen, selectionRectangle)
            End Using
        End If
    End Sub

    Private Sub GetStatusColors(ByVal status As String, ByRef background As Color, ByRef foreground As Color)
        Dim kind As ChannelStatusKind = ChannelStatusKind.Idle
        Select Case If(status, String.Empty).Trim().ToUpperInvariant()
            Case "PASS", "OK"
                kind = ChannelStatusKind.Pass
            Case "NG", "ERROR"
                kind = ChannelStatusKind.ResistanceNg
            Case "TESTING"
                kind = ChannelStatusKind.Testing
            Case "RESISTANCE_NG", "RESISTANCE NG"
                kind = ChannelStatusKind.ResistanceNg
            Case "VOLTAGE_NG", "VOLTAGE NG"
                kind = ChannelStatusKind.VoltageNg
        End Select
        background = ChannelStatusColors.ForStatus(kind, statusColors, foreground)
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

End Class
