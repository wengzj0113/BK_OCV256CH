Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO.Ports
Imports System.Globalization
Imports System.Text
Imports System.Threading

''' <summary>
''' 调试中心使用的只返回结果、不弹窗的设备检查入口。
''' </summary>
Public NotInheritable Class DebugProbeService
    Private Sub New()
    End Sub

    Public Shared Function ProbeAll() As List(Of DebugDeviceResult)
        Dim results As New List(Of DebugDeviceResult)()
        Dim deviceIds() As DebugDeviceId = {
            DebugDeviceId.ControlBoard,
            DebugDeviceId.MiddleMachine,
            DebugDeviceId.Hioki3562,
            DebugDeviceId.Keysight34461,
            DebugDeviceId.PlcTray,
            DebugDeviceId.IoCard,
            DebugDeviceId.Scanner,
            DebugDeviceId.Retest3562,
            DebugDeviceId.Database,
            DebugDeviceId.Mes
        }

        For Each deviceId As DebugDeviceId In deviceIds
            results.Add(Probe(deviceId))
        Next
        Return results
    End Function

    Public Shared Function Probe(deviceId As DebugDeviceId) As DebugDeviceResult
        Dim started As Stopwatch = Stopwatch.StartNew()
        Dim result As DebugDeviceResult = Nothing

        If isRuning Then
            result = New DebugDeviceResult With {
                .DeviceId = deviceId,
                .State = DebugDeviceState.Unavailable,
                .Message = "正式测试运行中，已跳过检查",
                .InterfaceDescription = GetInterfaceDescription(deviceId)
            }
        Else
            Try
                result = ProbeDevice(deviceId)
            Catch ex As Exception
                result = New DebugDeviceResult With {
                    .DeviceId = deviceId,
                    .State = DebugDeviceState.Failed,
                    .Message = ex.Message,
                    .InterfaceDescription = GetInterfaceDescription(deviceId)
                }
                AppLog.Write("Debug", GetDeviceName(deviceId) & " probe failed: " & ex.ToString())
            End Try
        End If

        started.Stop()
        result.CheckedAt = DateTime.Now
        result.ElapsedMilliseconds = started.ElapsedMilliseconds
        If String.IsNullOrWhiteSpace(result.InterfaceDescription) Then
            result.InterfaceDescription = GetInterfaceDescription(deviceId)
        End If
        Return result
    End Function

    Public Shared Function GetDeviceName(deviceId As DebugDeviceId) As String
        Select Case deviceId
            Case DebugDeviceId.ControlBoard : Return "控制板 / 单片机"
            Case DebugDeviceId.MiddleMachine : Return "中位机 / RS485"
            Case DebugDeviceId.Hioki3562 : Return "日置 3562（正式测试）"
            Case DebugDeviceId.Retest3562 : Return "日置 3562（NG复测）"
            Case DebugDeviceId.Keysight34461 : Return "Keysight 34461A"
            Case DebugDeviceId.PlcTray : Return "PLC / 托盘夹具"
            Case DebugDeviceId.IoCard : Return "IO 卡"
            Case DebugDeviceId.Scanner : Return "扫码器"
            Case DebugDeviceId.Database : Return "数据库"
            Case DebugDeviceId.Mes : Return "MES 接口"
            Case Else : Return deviceId.ToString()
        End Select
    End Function

    Public Shared Function GetInterfaceDescription(deviceId As DebugDeviceId) As String
        Select Case deviceId
            Case DebugDeviceId.ControlBoard, DebugDeviceId.MiddleMachine
                Return "COM" & (Initsetting.SCMComPort + 1).ToString() & " · " & DefaultScmBaudRate.ToString() & " 8N1"
            Case DebugDeviceId.Hioki3562
                Return "COM" & (Initsetting.Recom + 1).ToString() & " · " & DefaultMain3562BaudRate.ToString()
            Case DebugDeviceId.Keysight34461
                Return If(String.IsNullOrWhiteSpace(Initsetting.IpAddOf34461), "VISA 地址未配置", Initsetting.IpAddOf34461)
            Case DebugDeviceId.PlcTray
                If Initsetting.PLCMode = 1 Then
                    Return "Modbus TCP · " & If(String.IsNullOrWhiteSpace(Initsetting.PLCIP), "地址未配置", Initsetting.PLCIP) & ":" & If(Initsetting.PLCTcpPort > 0, Initsetting.PLCTcpPort, 502).ToString()
                End If
                Return "Modbus RTU · COM" & (Initsetting.PLCComPort + 1).ToString()
            Case DebugDeviceId.IoCard
                Return "由控制板管理"
            Case DebugDeviceId.Scanner
                Return "COM" & (Initsetting.ScanerPort + 1).ToString() & " · " & DefaultScannerBaudRate.ToString() & " 8N1"
            Case DebugDeviceId.Retest3562
                Return "COM" & (Initsetting.Retest3562Port + 1).ToString() & " · " & DefaultRetest3562BaudRate.ToString() & " 8N1"
            Case DebugDeviceId.Database
                Return "应用启动连接"
            Case DebugDeviceId.Mes
                Return If(String.IsNullOrWhiteSpace(Initsetting.MesApiAddress), "接口地址未配置", Initsetting.MesApiAddress)
            Case Else
                Return String.Empty
        End Select
    End Function

    Private Shared Function ProbeDevice(deviceId As DebugDeviceId) As DebugDeviceResult
        Select Case deviceId
            Case DebugDeviceId.ControlBoard
                If CheckCom_SCM(True) <> 1 Then Return Failure(deviceId, "SCM 协议握手失败")
                Return Success(deviceId, "协议握手成功")

            Case DebugDeviceId.MiddleMachine
                If controlBoardConnected Then Return Success(deviceId, "与 SCM 共用串口，协议已验证")
                Return Failure(deviceId, "等待控制板协议检查")

            Case DebugDeviceId.Hioki3562
                Return ProbeMain3562()

            Case DebugDeviceId.Keysight34461
                Return Probe34461()

            Case DebugDeviceId.PlcTray
                If plcAutomaticModeActive Then Return Success(deviceId, "由 PLC 自动测试流程独占")
                If plcController Is Nothing Then plcController = PlcControllerFactory.Create(Initsetting)
                If Not plcController.Connect() Then Return Failure(deviceId, "PLC 连接失败")
                plcConnected = True
                Dim trayClosed As Boolean = plcController.ReadTrayStatus()
                Return Success(deviceId, If(trayClosed, "已连接 · 托盘闭合", "已连接 · 托盘开启"))

            Case DebugDeviceId.IoCard
                Return New DebugDeviceResult With {
                    .DeviceId = deviceId,
                    .State = DebugDeviceState.Managed,
                    .Message = "由控制板统一管理，无独立串口",
                    .InterfaceDescription = GetInterfaceDescription(deviceId)
                }

            Case DebugDeviceId.Scanner
                Return ProbeScanner()

            Case DebugDeviceId.Retest3562
                Return ProbeRetest3562()

            Case DebugDeviceId.Database
                If databaseConnected Then Return Success(deviceId, "数据库连接状态正常")
                Return New DebugDeviceResult With {
                    .DeviceId = deviceId,
                    .State = DebugDeviceState.Unavailable,
                    .Message = "等待应用层连接检查",
                    .InterfaceDescription = GetInterfaceDescription(deviceId)
                }

            Case DebugDeviceId.Mes
                Return New DebugDeviceResult With {
                    .DeviceId = deviceId,
                    .State = DebugDeviceState.Unavailable,
                    .Message = "按正式流程调用，调试页不发送业务请求",
                    .InterfaceDescription = GetInterfaceDescription(deviceId)
                }
        End Select

        Return Failure(deviceId, "未定义的设备检查")
    End Function

    Private Shared Function ProbeScanner() As DebugDeviceResult
        Dim scannerPort As SerialPort = Com_scaner
        If scannerPort Is Nothing OrElse Not scannerPort.IsOpen Then
            Return New DebugDeviceResult With {
                .DeviceId = DebugDeviceId.Scanner,
                .State = DebugDeviceState.Unavailable,
                .Message = "扫码枪串口未打开",
                .InterfaceDescription = GetInterfaceDescription(DebugDeviceId.Scanner)
            }
        End If

        Dim scannedCode As String = String.Empty
        SyncLock scannerPort
            scannerPort.DiscardInBuffer()
            scannerPort.DiscardOutBuffer()
            scannerPort.Write("start")

            Dim response As New StringBuilder()
            Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(1500)
            Dim quietSince As DateTime = DateTime.UtcNow
            Do
                If scannerPort.BytesToRead > 0 Then
                    Dim chunk As String = scannerPort.ReadExisting()
                    response.Append(chunk)
                    quietSince = DateTime.UtcNow
                    If chunk.IndexOf(ControlChars.Cr) >= 0 OrElse chunk.IndexOf(ControlChars.Lf) >= 0 Then Exit Do
                ElseIf response.Length > 0 AndAlso (DateTime.UtcNow - quietSince).TotalMilliseconds >= 80 Then
                    Exit Do
                Else
                    Thread.Sleep(5)
                End If
            Loop While DateTime.UtcNow < deadline
            scannedCode = response.ToString().Trim(ControlChars.Cr, ControlChars.Lf, " "c, Microsoft.VisualBasic.ChrW(0))
        End SyncLock

        If String.IsNullOrWhiteSpace(scannedCode) Then Return Failure(DebugDeviceId.Scanner, "已发送 start，但未收到条码（1500 ms 超时）")
        Return Success(DebugDeviceId.Scanner, "扫码成功：" & scannedCode)
    End Function

    Private Shared Function ProbeMain3562() As DebugDeviceResult
        If CheckCom_3561(True) <> 1 OrElse Com_3561 Is Nothing OrElse Not Com_3561.IsOpen Then
            Return Failure(DebugDeviceId.Hioki3562, "主 3562 串口连接失败")
        End If

        Dim port As SerialPort = Com_3561
        Dim oldNewLine As String = port.NewLine
        Dim oldReadTimeout As Integer = port.ReadTimeout
        Try
            SyncLock port
                port.NewLine = ControlChars.Lf
                port.ReadTimeout = 4000
                port.DiscardInBuffer()
                port.DiscardOutBuffer()
                port.Write("*IDN?" & ControlChars.Lf)
                Dim identity As String = port.ReadLine()
                If identity.IndexOf("HIOKI", StringComparison.OrdinalIgnoreCase) < 0 Then
                    Return Failure(DebugDeviceId.Hioki3562, "未收到主 3562 身份响应：" & identity.Trim())
                End If

                port.DiscardInBuffer()
                port.DiscardOutBuffer()
                port.Write("SAMPle:RATE " & If(String.IsNullOrWhiteSpace(Initsetting.Main3562SampleRate), "EXF", Initsetting.Main3562SampleRate) & ControlChars.Lf)
                port.Write(":FUNC RV" & ControlChars.Lf)
                port.Write(":AUT OFF" & ControlChars.Lf)
                port.Write(":RES:RANG " & If(String.IsNullOrWhiteSpace(Initsetting.Main3562ResistanceRange), "300E-3", Initsetting.Main3562ResistanceRange) & ControlChars.Lf)
                port.Write(":VOLT:RANG " & If(String.IsNullOrWhiteSpace(Initsetting.Main3562VoltageRange), "6", Initsetting.Main3562VoltageRange) & ControlChars.Lf)
                port.Write("TRIG:SOUR IMM" & ControlChars.Lf)
                port.Write(":FETC?" & ControlChars.Lf)
                Dim fields As String() = port.ReadLine().Trim().Split(","c)
                If fields.Length < 2 Then Throw New FormatException("主 3562 测量响应字段不足。")

                Dim resistanceOhm As Double
                Dim voltage As Double
                If Not Double.TryParse(fields(0).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, resistanceOhm) OrElse
                   Not Double.TryParse(fields(1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, voltage) Then
                    Throw New FormatException("主 3562 返回的电压或内阻不是有效数值。")
                End If

                Return Success(DebugDeviceId.Hioki3562, String.Format(CultureInfo.InvariantCulture, "测量成功：电压 {0:F6} V；内阻 {1:F3} mΩ", voltage, resistanceOhm * 1000.0R))
            End SyncLock
        Finally
            port.NewLine = oldNewLine
            port.ReadTimeout = oldReadTimeout
            InitRes(False)
        End Try
    End Function

    Private Shared Function Probe34461() As DebugDeviceResult
        Dim address As String = Initsetting.IpAddOf34461
        If String.IsNullOrWhiteSpace(address) Then Return Failure(DebugDeviceId.Keysight34461, "VISA 地址未配置")

        Dim manager As Ivi.Visa.Interop.ResourceManager = Nothing
        Dim instrument As Ivi.Visa.Interop.FormattedIO488 = Nothing
        Try
            manager = New Ivi.Visa.Interop.ResourceManager()
            instrument = New Ivi.Visa.Interop.FormattedIO488()
            instrument.IO = manager.Open(address, Ivi.Visa.Interop.AccessMode.NO_LOCK, 2000, "")
            instrument.IO.Timeout = 10000
            instrument.WriteString("*IDN?")
            Dim identity As String = instrument.ReadString()
            If identity.IndexOf("Keysight Technologies", StringComparison.OrdinalIgnoreCase) < 0 Then
                Return Failure(DebugDeviceId.Keysight34461, "未收到 Keysight 34461A 身份响应：" & identity.Trim())
            End If

            instrument.WriteString("MEAS:VOLT:DC?")
            Dim reading As Double
            Dim response As String = instrument.ReadString().Trim()
            If Not Double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, reading) Then
                Throw New FormatException("34461A 返回的电压不是有效数值：" & response)
            End If
            Return Success(DebugDeviceId.Keysight34461, String.Format(CultureInfo.InvariantCulture, "测量成功：电压 {0:F6} V", reading))
        Finally
            If instrument IsNot Nothing Then
                Try
                    If instrument.IO IsNot Nothing Then instrument.IO.Close()
                Catch
                End Try
                Try
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(instrument)
                Catch
                End Try
            End If
            If manager IsNot Nothing Then
                Try
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(manager)
                Catch
                End Try
            End If
        End Try
    End Function

    Private Shared Function ProbeRetest3562() As DebugDeviceResult
        Using meter As New Retest3562Service()
            meter.Connect("COM" & (Initsetting.Retest3562Port + 1).ToString())
            Dim measurement As RetestMeasurement = meter.Measure()
            Return Success(DebugDeviceId.Retest3562,
                           String.Format(CultureInfo.InvariantCulture, "测量成功：电压 {0:F6} V；内阻 {1:F3} mΩ", measurement.Voltage, measurement.Resistance))
        End Using
    End Function

    Private Shared Function Success(deviceId As DebugDeviceId, message As String) As DebugDeviceResult
        Return New DebugDeviceResult With {
            .DeviceId = deviceId,
            .State = DebugDeviceState.Connected,
            .Message = message,
            .InterfaceDescription = GetInterfaceDescription(deviceId)
        }
    End Function

    Private Shared Function Failure(deviceId As DebugDeviceId, message As String) As DebugDeviceResult
        Return New DebugDeviceResult With {
            .DeviceId = deviceId,
            .State = DebugDeviceState.Failed,
            .Message = message,
            .InterfaceDescription = GetInterfaceDescription(deviceId)
        }
    End Function
End Class
