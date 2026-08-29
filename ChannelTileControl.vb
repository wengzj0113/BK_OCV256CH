Imports System.Drawing
Imports System.Windows.Forms

Public Structure ChannelDisplayData
    Public ChannelNo As Integer
    Public Row As Integer
    Public Column As Integer
    Public Barcode As String
    Public Ocv1 As Nullable(Of Single)
    Public Ir1 As Nullable(Of Single)
    Public StatusText As String
    Public ResultText As String
End Structure

Public Class ChannelSelectedEventArgs
    Inherits EventArgs
    Public ReadOnly Property Data As ChannelDisplayData
    Public Sub New(ByVal data As ChannelDisplayData)
        Me.Data = data
    End Sub
End Class

Public Class ChannelHoverEventArgs
    Inherits EventArgs
    Public ReadOnly Property Data As ChannelDisplayData
    Public Sub New(ByVal data As ChannelDisplayData)
        Me.Data = data
    End Sub
End Class

Public Class ChannelTileControl
    Inherits Control

    Private currentData As ChannelDisplayData
    Private hasDisplayData As Boolean
    Private selected As Boolean
    Private suppressNextClick As Boolean
    Public Event ChannelSelected As EventHandler(Of ChannelSelectedEventArgs)
    Public Event ChannelHovered As EventHandler(Of ChannelHoverEventArgs)
    Public Event ChannelHoverEnded As EventHandler
    Public Event TileMouseDown As MouseEventHandler
    Public Event TileMouseMove As MouseEventHandler
    Public Event TileMouseUp As MouseEventHandler

    Public Sub New()
        DoubleBuffered = True
        Margin = New Padding(2, 2, 2, 2)
        Padding = New Padding(2)
        MinimumSize = New Size(0, 0)
        BackColor = UiTheme.CardBackground
        ForeColor = UiTheme.TextPrimary
        Cursor = Cursors.Hand
        AddHandler Me.Click, AddressOf HandleSelected
        AddHandler Me.MouseEnter, AddressOf HandleMouseEnter
        AddHandler Me.MouseLeave, AddressOf HandleMouseLeave
        AddHandler Me.MouseDown, AddressOf HandleTileMouseDown
        AddHandler Me.MouseMove, AddressOf HandleTileMouseMove
        AddHandler Me.MouseUp, AddressOf HandleTileMouseUp
    End Sub

    Public ReadOnly Property ChannelNumber As Integer
        Get
            Return currentData.ChannelNo
        End Get
    End Property

    Public Property IsSelected As Boolean
        Get
            Return selected
        End Get
        Set(ByVal value As Boolean)
            If selected = value Then Return
            selected = value
            Invalidate()
        End Set
    End Property

    Public Sub UpdateDisplay(ByVal data As ChannelDisplayData)
        currentData = data
        hasDisplayData = True
        Text = data.Row.ToString() & "-" & data.Column.ToString()
        Dim background As Color
        Dim foreground As Color
        GetStatusColors(data.StatusText, background, foreground)
        BackColor = background
        ForeColor = foreground
        Invalidate()
    End Sub

    Private Sub HandleSelected(ByVal sender As Object, ByVal e As EventArgs)
        If suppressNextClick Then
            suppressNextClick = False
            Return
        End If
        RaiseEvent ChannelSelected(Me, New ChannelSelectedEventArgs(currentData))
    End Sub

    Private Sub HandleMouseEnter(ByVal sender As Object, ByVal e As EventArgs)
        If Not hasDisplayData Then Return
        RaiseEvent ChannelHovered(Me, New ChannelHoverEventArgs(currentData))
    End Sub

    Private Sub HandleMouseLeave(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent ChannelHoverEnded(Me, EventArgs.Empty)
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Using backgroundBrush As New SolidBrush(BackColor)
            e.Graphics.FillRectangle(backgroundBrush, ClientRectangle)
        End Using
        If Not String.IsNullOrWhiteSpace(Text) Then
            TextRenderer.DrawText(e.Graphics, Text, UiTheme.UiFont, ClientRectangle, ForeColor,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        End If
        If selected Then
            Using pen As New Pen(UiTheme.Primary, 2.0F)
                Dim bounds As Rectangle = ClientRectangle
                bounds.Width -= 1
                bounds.Height -= 1
                e.Graphics.DrawRectangle(pen, bounds)
            End Using
        End If
    End Sub

    Private Sub HandleTileMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then Capture = True
        RaiseEvent TileMouseDown(Me, e)
    End Sub

    Private Sub HandleTileMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        RaiseEvent TileMouseMove(Me, e)
    End Sub

    Private Sub HandleTileMouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then Capture = False
        RaiseEvent TileMouseUp(Me, e)
        If e.Button = MouseButtons.Left AndAlso hasDisplayData Then
            suppressNextClick = True
            RaiseEvent ChannelSelected(Me, New ChannelSelectedEventArgs(currentData))
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

End Class
