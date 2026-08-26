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
    Private ReadOnly tileControls As New Dictionary(Of Integer, ChannelTileControl)()
    Private ReadOnly gridLayout As New TableLayoutPanel()
    Private ReadOnly hoverDetailsLabel As Label
    Private ReadOnly selectedChannels As New HashSet(Of Integer)()
    Private selectionModeValue As ChannelSelectionMode = ChannelSelectionMode.SingleChannel
    Private isSelectingRectangle As Boolean
    Private rectangleMoved As Boolean
    Private selectionStart As Point
    Private selectionCurrent As Point

    Public Event ChannelSelected As EventHandler(Of ChannelSelectedEventArgs)
    Public Event SelectedChannelsChanged As EventHandler

    Public Sub New()
        DoubleBuffered = True
        BackColor = UiTheme.CardBackground
        BorderStyle = BorderStyle.FixedSingle
        Padding = New Padding(10, 8, 10, 10)

        Dim outerLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 3,
            .BackColor = UiTheme.CardBackground,
            .Padding = New Padding(0)
        }
        '行号/状态竖条固定 44px 排水沟,任何分辨率下都可读(不属于 tile 行列);
        '矩阵主体列占剩余宽度。
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
            .BackColor = UiTheme.CardBackground,
            .Margin = New Padding(0)
        }
        '行号列必须显式声明列样式:缺省时单列会按内容 PreferredSize 撑宽(如 117px),
        '导致行号右溢被矩阵主体覆盖、文字被裁成碎片。
        rowHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        For row As Integer = 1 To GridSize
            rowHeader.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / GridSize))
            rowHeader.Controls.Add(CreateCoordinateLabel(row.ToString()), 0, row - 1)
        Next
        outerLayout.Controls.Add(rowHeader, 0, 2)

        gridLayout.ColumnCount = GridSize
        gridLayout.RowCount = GridSize
        gridLayout.Dock = DockStyle.Fill
        gridLayout.BackColor = UiTheme.CardBackground
        gridLayout.Margin = New Padding(0)
        AddHandler gridLayout.Paint, AddressOf HandleGridPaint
        For i As Integer = 0 To GridSize - 1
            gridLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / GridSize))
            gridLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / GridSize))
        Next
        BuildTiles()
        outerLayout.Controls.Add(gridLayout, 1, 2)
        Controls.Add(outerLayout)
    End Sub

    Public ReadOnly Property TileCount As Integer
        Get
            Return tileControls.Count
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

    Public Sub SelectAll()
        selectedChannels.Clear()
        For channelNo As Integer = 1 To GridSize * GridSize
            selectedChannels.Add(channelNo)
        Next
        ApplySelectionToTiles()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub ClearSelection()
        If selectedChannels.Count = 0 Then Return
        selectedChannels.Clear()
        ApplySelectionToTiles()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub UpdateChannel(ByVal channelNo As Integer, ByVal data As ChannelDisplayData)
        Dim tile As ChannelTileControl = Nothing
        If tileControls.TryGetValue(channelNo, tile) Then
            tile.UpdateDisplay(data)
            tile.IsSelected = selectedChannels.Contains(channelNo)
        End If
    End Sub

    Public Sub UpdateAll(ByVal data As IEnumerable(Of ChannelDisplayData))
        If data Is Nothing Then Return
        For Each channel As ChannelDisplayData In data
            UpdateChannel(channel.ChannelNo, channel)
        Next
    End Sub

    Private Sub BuildTiles()
        For column As Integer = 1 To GridSize
            For row As Integer = 1 To GridSize
                Dim channelNo As Integer = (column - 1) * GridSize + row
                Dim tile As New ChannelTileControl With {.Dock = DockStyle.Fill}
                AddHandler tile.ChannelSelected, AddressOf HandleChannelSelected
                AddHandler tile.ChannelHovered, AddressOf HandleChannelHovered
                AddHandler tile.ChannelHoverEnded, AddressOf HandleChannelHoverEnded
                AddHandler tile.TileMouseDown, AddressOf HandleTileMouseDown
                AddHandler tile.TileMouseMove, AddressOf HandleTileMouseMove
                AddHandler tile.TileMouseUp, AddressOf HandleTileMouseUp
                tileControls.Add(channelNo, tile)
                gridLayout.Controls.Add(tile, column - 1, row - 1)
            Next
        Next
    End Sub

    Private Sub HandleChannelSelected(ByVal sender As Object, ByVal e As ChannelSelectedEventArgs)
        If e Is Nothing Then Return
        If selectionModeValue = ChannelSelectionMode.SingleChannel Then
            ToggleSelection(e.Data.ChannelNo)
        ElseIf rectangleMoved Then
            rectangleMoved = False
            Return
        End If
        RaiseEvent ChannelSelected(Me, e)
    End Sub

    Private Sub ToggleSelection(ByVal channelNo As Integer)
        If selectedChannels.Contains(channelNo) Then
            selectedChannels.Remove(channelNo)
        ElseIf channelNo >= 1 AndAlso channelNo <= GridSize * GridSize Then
            selectedChannels.Add(channelNo)
        End If
        ApplySelectionToTiles()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub ApplySelectionToTiles()
        For Each tile As ChannelTileControl In tileControls.Values
            tile.IsSelected = selectedChannels.Contains(tile.ChannelNumber)
        Next
        gridLayout.Invalidate()
    End Sub

    Private Sub HandleTileMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If selectionModeValue <> ChannelSelectionMode.Rectangle OrElse e.Button <> MouseButtons.Left Then Return
        isSelectingRectangle = True
        rectangleMoved = False
        selectionStart = GetGridPoint(sender, e.Location)
        selectionCurrent = selectionStart
        gridLayout.Invalidate()
    End Sub

    Private Sub HandleTileMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If Not isSelectingRectangle Then Return
        selectionCurrent = GetGridPoint(sender, e.Location)
        rectangleMoved = rectangleMoved OrElse selectionCurrent <> selectionStart
        gridLayout.Invalidate()
    End Sub

    Private Sub HandleTileMouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        If Not isSelectingRectangle OrElse e.Button <> MouseButtons.Left Then Return
        selectionCurrent = GetGridPoint(sender, e.Location)
        isSelectingRectangle = False
        rectangleMoved = True
        Dim selectionRectangle As Rectangle = NormalizeRectangle(selectionStart, selectionCurrent)
        selectedChannels.Clear()
        For Each item As KeyValuePair(Of Integer, ChannelTileControl) In tileControls
            Dim tileBounds As Rectangle = item.Value.Bounds
            Dim tileLocation As Point = gridLayout.PointToClient(item.Value.Parent.PointToScreen(item.Value.Location))
            tileBounds.Location = tileLocation
            If selectionRectangle.IntersectsWith(tileBounds) Then selectedChannels.Add(item.Key)
        Next
        ApplySelectionToTiles()
        RaiseEvent SelectedChannelsChanged(Me, EventArgs.Empty)
        gridLayout.Invalidate()
    End Sub

    Private Function GetGridPoint(ByVal sender As Object, ByVal location As Point) As Point
        Dim control As Control = TryCast(sender, Control)
        If control Is Nothing Then Return location
        Return gridLayout.PointToClient(control.PointToScreen(location))
    End Function

    Private Shared Function NormalizeRectangle(ByVal first As Point, ByVal second As Point) As Rectangle
        Return Rectangle.FromLTRB(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Max(first.X, second.X), Math.Max(first.Y, second.Y))
    End Function

    Private Sub HandleGridPaint(ByVal sender As Object, ByVal e As PaintEventArgs)
        If Not isSelectingRectangle Then Return
        Dim selectionRectangle As Rectangle = NormalizeRectangle(selectionStart, selectionCurrent)
        Using fillBrush As New SolidBrush(Color.FromArgb(45, UiTheme.Primary))
            e.Graphics.FillRectangle(fillBrush, selectionRectangle)
        End Using
        Using borderPen As New Pen(UiTheme.Primary, 1.0F)
            e.Graphics.DrawRectangle(borderPen, selectionRectangle)
        End Using
    End Sub

    Private Sub HandleChannelHovered(ByVal sender As Object, ByVal e As ChannelHoverEventArgs)
        If e Is Nothing Then Return
        hoverDetailsLabel.Text = BuildHoverDetailsText(e.Data)
    End Sub

    Private Sub HandleChannelHoverEnded(ByVal sender As Object, ByVal e As EventArgs)
        hoverDetailsLabel.Text = String.Empty
    End Sub

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
End Class
