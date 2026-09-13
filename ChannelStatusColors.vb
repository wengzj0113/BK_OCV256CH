Imports System.Drawing

Public Enum ChannelStatusKind
    Pass
    Testing
    Idle
    ResistanceNg
    VoltageNg
End Enum

Public NotInheritable Class ChannelStatusColors
    Public ReadOnly Property PassColor As Color
    Public ReadOnly Property TestingColor As Color
    Public ReadOnly Property IdleColor As Color
    Public ReadOnly Property ResistanceNgColor As Color
    Public ReadOnly Property VoltageNgColor As Color

    Public Sub New(passColor As Color,
                   testingColor As Color,
                   idleColor As Color,
                   resistanceNgColor As Color,
                   voltageNgColor As Color)
        Me.PassColor = NormalizeOpaque(passColor)
        Me.TestingColor = NormalizeOpaque(testingColor)
        Me.IdleColor = NormalizeOpaque(idleColor)
        Me.ResistanceNgColor = NormalizeOpaque(resistanceNgColor)
        Me.VoltageNgColor = NormalizeOpaque(voltageNgColor)
    End Sub

    Public Shared Function ForStatus(status As ChannelStatusKind,
                                     colors As ChannelStatusColors,
                                     ByRef foreground As Color) As Color
        Dim palette As ChannelStatusColors = If(colors, CreateDefault())
        Dim background As Color
        Select Case status
            Case ChannelStatusKind.Pass
                background = palette.PassColor
            Case ChannelStatusKind.Testing
                background = palette.TestingColor
            Case ChannelStatusKind.ResistanceNg
                background = palette.ResistanceNgColor
            Case ChannelStatusKind.VoltageNg
                background = palette.VoltageNgColor
            Case Else
                background = palette.IdleColor
        End Select
        foreground = If(GetLuma(background) < 145, Color.White, Color.Black)
        Return background
    End Function

    Public Shared Function NormalizeOpaque(value As Color) As Color
        Return Color.FromArgb(255, value.R, value.G, value.B)
    End Function

    Public Shared Function CreateDefault() As ChannelStatusColors
        Return New ChannelStatusColors(
            Color.FromArgb(255, 24, 169, 104),
            Color.FromArgb(255, 23, 105, 232),
            Color.FromArgb(255, 101, 113, 132),
            Color.FromArgb(255, 239, 61, 67),
            Color.FromArgb(255, 242, 118, 61))
    End Function

    Private Shared Function GetLuma(value As Color) As Integer
        Return CInt((299 * value.R + 587 * value.G + 114 * value.B) / 1000)
    End Function
End Class
