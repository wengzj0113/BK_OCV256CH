Friend NotInheritable Class EngineerProcessSettingsPolicy
    Private Sub New()
    End Sub

    Friend Shared Function ApplyTo(currentSettings As Initset,
                                   profile As TestStandardProfile,
                                   voltageCorrection As Single,
                                   resistanceCorrection As Single,
                                   channelDelay As Decimal,
                                   probeLifespan As Integer,
                                   probeUsedCount As Integer,
                                   barcodeLength As Integer,
                                   calibrationResistance As Single,
                                   overTemperature As Integer) As Initset
        If profile Is Nothing Then Throw New ArgumentNullException(NameOf(profile))
        profile.Validate()
        If channelDelay < Decimal.Zero Then Throw New ArgumentOutOfRangeException(NameOf(channelDelay))
        If probeLifespan < 1 Then Throw New ArgumentOutOfRangeException(NameOf(probeLifespan))
        If probeUsedCount < 0 Then Throw New ArgumentOutOfRangeException(NameOf(probeUsedCount))
        If barcodeLength < 1 Then Throw New ArgumentOutOfRangeException(NameOf(barcodeLength))
        If calibrationResistance <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(calibrationResistance))
        If overTemperature < 1 Then Throw New ArgumentOutOfRangeException(NameOf(overTemperature))

        Dim updatedSettings As Initset = currentSettings
        updatedSettings.downResistance = CSng(profile.ResistanceMin)
        updatedSettings.upResistance = CSng(profile.ResistanceMax)
        updatedSettings.downVoltage = CSng(profile.VoltageMin)
        updatedSettings.upVoltage = CSng(profile.VoltageMax)
        updatedSettings.sngV_B = voltageCorrection
        updatedSettings.sngR_B = resistanceCorrection
        updatedSettings.delayTime = channelDelay.ToString(Globalization.CultureInfo.InvariantCulture)
        updatedSettings.Probelifespan = probeLifespan
        updatedSettings.ProbeUsedCount = probeUsedCount
        updatedSettings.txtCodeLenth = barcodeLength
        updatedSettings.cailibrateModelValue = calibrationResistance
        updatedSettings.overTemperateValue = overTemperature
        Return updatedSettings
    End Function
End Class
