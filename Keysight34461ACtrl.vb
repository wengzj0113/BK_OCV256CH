
Imports Ivi.Visa
Imports Ivi.Visa.Interop
Imports System.Threading

Public Class Keysight34461ACtrl

    Dim rm As ResourceManager = New ResourceManager()

    ' 连接到34465A示波器
    Dim dmm As FormattedIO488 = New FormattedIO488()
    Dim resourceString As String = "TCPIP0::192.168.66.81::inst0::INSTR" ' 替换为你的示波器VISA地址

    ' 初始化VISA会话
    Public Sub InitializeVisaSession()

        dmm.IO = rm.Open(resourceString)
    End Sub

    ' 设置外部触发
    Public Sub SetExternalTrigger()
        dmm.Write(":TRIG:SOUR EXT")
    End Sub

    ' 采集电池电压值
    Public Function SampleBatteryVoltage() As Double
        Dim response As String = dmm.Query(":MEAS:VOLT:DC?")
        Return CDbl(response)
    End Function

    ' 保存数据到仪器内存
    Public Sub SaveDataToInstrumentMemory(index As Integer, voltage As Double)
        dmm.Write($":MEM:STORE {index}, {voltage}")
    End Sub

    ' 读取所有数据到数组
    Public Function ReadDataToArray() As Double()
        Dim data As New List(Of Double)()

        For i As Integer = 1 To 64
            Dim response As String = dmm.Query($":MEM:READ? {i}")
            data.Add(CDbl(response))
        Next

        Return data.ToArray()
    End Function

    ' 关闭VISA会话
    Public Sub CloseVisaSession()
        dmm.Dispose()
    End Sub
End Class

