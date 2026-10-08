''' <summary>
''' Shared acquisition-rate mapping for the control board and both measurement instruments.
''' </summary>
Public NotInheritable Class InstrumentRateProfile
    Private ReadOnly _settingName As String
    Private ReadOnly _controllerSpeedValue As UShort
    Private ReadOnly _hiokiSamplingCommand As String
    Private ReadOnly _keysightNplc As String
    Private ReadOnly _keysightAutoZero As Boolean

    Private Sub New(settingName As String,
                    controllerSpeedValue As UShort,
                    hiokiSamplingCommand As String,
                    keysightNplc As String,
                    keysightAutoZero As Boolean)
        _settingName = settingName
        _controllerSpeedValue = controllerSpeedValue
        _hiokiSamplingCommand = hiokiSamplingCommand
        _keysightNplc = keysightNplc
        _keysightAutoZero = keysightAutoZero
    End Sub

    Public ReadOnly Property SettingName As String
        Get
            Return _settingName
        End Get
    End Property

    Public ReadOnly Property ControllerSpeedValue As UShort
        Get
            Return _controllerSpeedValue
        End Get
    End Property

    Public ReadOnly Property HiokiSamplingCommand As String
        Get
            Return _hiokiSamplingCommand
        End Get
    End Property

    Public ReadOnly Property KeysightNplc As String
        Get
            Return _keysightNplc
        End Get
    End Property

    Public ReadOnly Property KeysightAutoZero As Boolean
        Get
            Return _keysightAutoZero
        End Get
    End Property

    Public Shared Function FromSettingName(settingName As String) As InstrumentRateProfile
        Select Case If(settingName, String.Empty).Trim()
            Case "", "超快", "高速档"
                Return New InstrumentRateProfile("超快", CUShort(3), "EXF", "0.2", True)
            Case "低速", "观察档"
                Return New InstrumentRateProfile("低速", CUShort(0), "SLOW", "0.2", True)
            Case "中速", "稳定档"
                Return New InstrumentRateProfile("中速", CUShort(1), "MED", "0.2", True)
            Case "高速", "快速档"
                Return New InstrumentRateProfile("高速", CUShort(2), "FAST", "0.2", True)
            Case Else
                Throw New ArgumentException("不支持的仪表速率模式: " & settingName, "settingName")
        End Select
    End Function

    ''' <summary>
    ''' Uses a fixed 10 V Keysight range regardless of configured acceptance limits.
    ''' </summary>
    Public Shared Function SelectKeysightVoltageRange(upLimitMillivolts As Single,
                                                      downLimitMillivolts As Single) As Integer
        Return 10
    End Function
End Class
