Imports System.Drawing
Imports System.Windows.Forms

Public Class ChannelGridControl
    Inherits UserControl

    Private Const GridSize As Integer = 16
    Private ReadOnly tileControls As New Dictionary(Of Integer, ChannelTileControl)()
    Private ReadOnly gridLayout As New TableLayoutPanel()
    Private ReadOnly rowLabels As New List(Of Label)()
    Private ReadOnly columnLabels As New List(Of Label)()

    Public Event ChannelSelected As EventHandler(Of ChannelSelectedEventArgs)

    Public Sub New()
        DoubleBuffered = True
        BackColor = UiTheme.CardBackground
        Padding = New Padding(12)
        Dock = DockStyle.Fill

        Dim outerLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 2,
            .BackColor = UiTheme.CardBackground,
            .Padding = New Padding(0)
        }
        outerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 28.0F))
        outerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        outerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        outerLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        Dim corner As New Label With {.Dock = DockStyle.Fill, .BackColor = UiTheme.CardBackground}
        outerLayout.Controls.Add(corner, 0, 0)

        Dim columnHeader As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = GridSize,
            .RowCount = 1,
            .BackColor = UiTheme.CardBackground,
            .Margin = New Padding(0)
        }
        For column As Integer = 1 To GridSize
            columnHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / GridSize))
            Dim label As Label = CreateCoordinateLabel(column.ToString())
            columnLabels.Add(label)
            columnHeader.Controls.Add(label, column - 1, 0)
        Next
        outerLayout.Controls.Add(columnHeader, 1, 0)

        Dim rowHeader As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = GridSize,
            .BackColor = UiTheme.CardBackground,
            .Margin = New Padding(0)
        }
        For row As Integer = 1 To GridSize
            rowHeader.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / GridSize))
            Dim label As Label = CreateCoordinateLabel(row.ToString())
            rowLabels.Add(label)
            rowHeader.Controls.Add(label, 0, row - 1)
        Next
        outerLayout.Controls.Add(rowHeader, 0, 1)

        gridLayout.ColumnCount = GridSize
        gridLayout.RowCount = GridSize
        gridLayout.Dock = DockStyle.Fill
        gridLayout.BackColor = UiTheme.CardBackground
        gridLayout.Margin = New Padding(0)
        For i As Integer = 0 To GridSize - 1
            gridLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / GridSize))
            gridLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / GridSize))
        Next
        BuildTiles()
        outerLayout.Controls.Add(gridLayout, 1, 1)
        Controls.Add(outerLayout)
    End Sub

    Public ReadOnly Property TileCount As Integer
        Get
            Return tileControls.Count
        End Get
    End Property

    Public Sub UpdateChannel(ByVal channelNo As Integer, ByVal data As ChannelDisplayData)
        Dim tile As ChannelTileControl = Nothing
        If tileControls.TryGetValue(channelNo, tile) Then
            tile.UpdateDisplay(data)
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
                tileControls.Add(channelNo, tile)
                gridLayout.Controls.Add(tile, column - 1, row - 1)
            Next
        Next
    End Sub

    Private Sub HandleChannelSelected(ByVal sender As Object, ByVal e As ChannelSelectedEventArgs)
        RaiseEvent ChannelSelected(Me, e)
    End Sub

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
