Public NotInheritable Class TrayChannelMapping
    Private Const GridSize As Integer = 16
    Private Const ChannelCount As Integer = GridSize * GridSize
    Private Shared ReadOnly deviceByStandard As Integer() = BuildTable()

    Private Sub New()
    End Sub

    Public Shared Function ToDeviceChannel(ByVal standardChannel As Integer) As Integer
        ValidateChannel(standardChannel, NameOf(standardChannel))
        Return deviceByStandard(standardChannel - 1)
    End Function

    Public Shared Function ToStandardChannel(ByVal deviceChannel As Integer) As Integer
        ValidateChannel(deviceChannel, NameOf(deviceChannel))
        Dim standardIndex0 As Integer = Array.IndexOf(deviceByStandard, deviceChannel)
        If standardIndex0 < 0 Then Throw New InvalidOperationException("设备通道映射不完整。")
        Return standardIndex0 + 1
    End Function

    Private Shared Function BuildTable() As Integer()
        Dim table(ChannelCount - 1) As Integer
        Dim seenDevice(ChannelCount) As Boolean
        '同一物理点：标准编号按行从左到右，设备通道按列从右到左。
        For row As Integer = 1 To GridSize
            For column As Integer = 1 To GridSize
                Dim standardChannel As Integer = (row - 1) * GridSize + column
                Dim deviceChannel As Integer = (GridSize - column) * GridSize + row
                If seenDevice(deviceChannel) Then Throw New InvalidOperationException("设备通道映射重复。")
                table(standardChannel - 1) = deviceChannel
                seenDevice(deviceChannel) = True
            Next
        Next
        For deviceChannel As Integer = 1 To ChannelCount
            If Not seenDevice(deviceChannel) Then Throw New InvalidOperationException("设备通道映射不完整。")
        Next
        Return table
    End Function

    Private Shared Sub ValidateChannel(ByVal channel As Integer, ByVal parameterName As String)
        If channel < 1 OrElse channel > ChannelCount Then
            Throw New ArgumentOutOfRangeException(parameterName, "通道号必须在1到256之间。")
        End If
    End Sub
End Class
