Imports System.Drawing

Friend NotInheritable Class ChannelColorSettings
    Public Shared ReadOnly DefaultPass As Color = Color.FromArgb(255, 24, 169, 104)
    Public Shared ReadOnly DefaultTesting As Color = Color.FromArgb(255, 23, 105, 232)
    Public Shared ReadOnly DefaultIdle As Color = Color.FromArgb(255, 128, 128, 128)
    Public Shared ReadOnly DefaultResistanceNg As Color = Color.FromArgb(255, 239, 61, 67)
    Public Shared ReadOnly DefaultVoltageNg As Color = Color.FromArgb(255, 242, 118, 61)
    Public Shared ReadOnly DefaultSelected As Color = Color.FromArgb(255, 101, 113, 132)
    Private Shared ReadOnly LegacyDefaultIdle As Color = Color.FromArgb(255, 101, 113, 132)

    Public Shared Function FromInitset(value As Initset) As ChannelStatusColors
        Dim idleColor As Color = SafeColor(value.ChannelIdleColorArgb, DefaultIdle)
        If idleColor.ToArgb() = LegacyDefaultIdle.ToArgb() Then idleColor = DefaultIdle

        Return New ChannelStatusColors(
            SafeColor(value.ChannelPassColorArgb, DefaultPass),
            SafeColor(value.ChannelTestingColorArgb, DefaultTesting),
            idleColor,
            SafeColor(value.ChannelResistanceNgColorArgb, DefaultResistanceNg),
            SafeColor(value.ChannelVoltageNgColorArgb, DefaultVoltageNg),
            SafeColor(value.ChannelSelectedColorArgb, DefaultSelected))
    End Function

    Public Shared Sub ApplyToInitset(ByRef target As Initset, colors As ChannelStatusColors)
        Dim palette As ChannelStatusColors = If(colors, CreateDefaultPalette())
        target.ChannelPassColorArgb = palette.PassColor.ToArgb()
        target.ChannelTestingColorArgb = palette.TestingColor.ToArgb()
        target.ChannelIdleColorArgb = palette.IdleColor.ToArgb()
        target.ChannelResistanceNgColorArgb = palette.ResistanceNgColor.ToArgb()
        target.ChannelVoltageNgColorArgb = palette.VoltageNgColor.ToArgb()
        target.ChannelSelectedColorArgb = palette.SelectedColor.ToArgb()
    End Sub

    Public Shared Function CreateDefaultPalette() As ChannelStatusColors
        Return New ChannelStatusColors(DefaultPass, DefaultTesting, DefaultIdle, DefaultResistanceNg, DefaultVoltageNg, DefaultSelected)
    End Function

    Private Shared Function SafeColor(argb As Integer, fallback As Color) As Color
        If argb = 0 Then Return fallback
        Try
            Dim value As Color = Color.FromArgb(argb)
            If value.A = 0 Then Return fallback
            Return ChannelStatusColors.NormalizeOpaque(value)
        Catch
            Return fallback
        End Try
    End Function
End Class
