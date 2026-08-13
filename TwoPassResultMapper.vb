Friend Class TwoPassResultMapper
    Public Sub Apply(selection As TestChannelSelection,
                     hiokiResistanceMilliohm As IList(Of Single),
                     dmmVoltage As IList(Of Single),
                     calibrationMilliohm As IList(Of Double),
                     targetData As Bat_info())
        If selection Is Nothing Then
            Throw New ArgumentNullException("selection")
        End If
        If hiokiResistanceMilliohm Is Nothing Then
            Throw New ArgumentNullException("hiokiResistanceMilliohm")
        End If
        If dmmVoltage Is Nothing Then
            Throw New ArgumentNullException("dmmVoltage")
        End If
        If calibrationMilliohm Is Nothing Then
            Throw New ArgumentNullException("calibrationMilliohm")
        End If
        If targetData Is Nothing Then
            Throw New ArgumentNullException("targetData")
        End If
        If hiokiResistanceMilliohm.Count < selection.ExpectedHiokiCount Then
            Throw New InvalidOperationException("3562 cached resistance count is less than expected.")
        End If
        If dmmVoltage.Count < selection.ExpectedDmmCount Then
            Throw New InvalidOperationException("34461 cached voltage count is less than expected.")
        End If
        If calibrationMilliohm.Count < CalibrationService.ChannelCount Then
            Throw New InvalidOperationException("Calibration array must contain 256 values.")
        End If
        If targetData.Length < CalibrationService.ChannelCount Then
            Throw New InvalidOperationException("Receive data array must contain 256 channels.")
        End If

        MapHiokiResistance(selection, hiokiResistanceMilliohm, calibrationMilliohm, targetData)
        MapDmmVoltage(selection, dmmVoltage, targetData)
    End Sub

    Private Shared Sub MapHiokiResistance(selection As TestChannelSelection,
                                          hiokiResistanceMilliohm As IList(Of Single),
                                          calibrationMilliohm As IList(Of Double),
                                          targetData As Bat_info())
        Dim offset As Integer = 0
        For i As Integer = 0 To selection.G1PhysicalChannels.Count - 1
            Dim channel As Integer = selection.G1PhysicalChannels(i)
            ApplyResistance(channel, hiokiResistanceMilliohm(offset + i), calibrationMilliohm, targetData)
        Next

        offset += selection.G1PhysicalChannels.Count
        For i As Integer = 0 To selection.G2PhysicalChannels.Count - 1
            Dim channel As Integer = selection.G2PhysicalChannels(i)
            ApplyResistance(channel, hiokiResistanceMilliohm(offset + i), calibrationMilliohm, targetData)
        Next
    End Sub

    Private Shared Sub MapDmmVoltage(selection As TestChannelSelection,
                                     dmmVoltage As IList(Of Single),
                                     targetData As Bat_info())
        Dim offset As Integer = 0
        For i As Integer = 0 To selection.G2PhysicalChannels.Count - 1
            Dim channel As Integer = selection.G2PhysicalChannels(i)
            ApplyVoltage(channel, dmmVoltage(offset + i), targetData)
        Next

        offset += selection.G2PhysicalChannels.Count
        For i As Integer = 0 To selection.G1PhysicalChannels.Count - 1
            Dim channel As Integer = selection.G1PhysicalChannels(i)
            ApplyVoltage(channel, dmmVoltage(offset + i), targetData)
        Next
    End Sub

    Private Shared Sub ApplyResistance(channel As Integer,
                                       measuredMilliohm As Single,
                                       calibrationMilliohm As IList(Of Double),
                                       targetData As Bat_info())
        Dim channelIndex0 As Integer = channel - 1
        targetData(channelIndex0).Resistance = CSng(measuredMilliohm + calibrationMilliohm(channelIndex0))
        If targetData(channelIndex0).Resistance > 10000 Then
            targetData(channelIndex0).Resistance = 10000
        End If
        targetData(channelIndex0).TestTime = Now
    End Sub

    Private Shared Sub ApplyVoltage(channel As Integer,
                                    measuredVoltage As Single,
                                    targetData As Bat_info())
        Dim channelIndex0 As Integer = channel - 1
        targetData(channelIndex0).Voltage = measuredVoltage
        If targetData(channelIndex0).Voltage > 10000 Then
            targetData(channelIndex0).Voltage = 10000
        End If
        targetData(channelIndex0).TestTime = Now
    End Sub
End Class
