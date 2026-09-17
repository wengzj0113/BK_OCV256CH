Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO.Ports

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
            Case DebugDeviceId.Hioki3562 : Return "日置 3562 内阻仪"
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
                Return "COM" & (Initsetting.SCMComPort + 1).ToString() & " · " & If(Initsetting.SCMComRate > 0, Initsetting.SCMComRate, 115200).ToString() & " 8N1"
            Case DebugDeviceId.Hioki3562
                Return "COM" & (Initsetting.Retest3562Port + 1).ToString() & " · " & If(Initsetting.Retest3562Rate > 0, Initsetting.Retest3562Rate, 9600).ToString()
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
                Return "COM" & (Initsetting.ScanerPort + 1).ToString()
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
                If ConnectFor3561(True) <> 0 Then Return Failure(deviceId, "未收到 HIOKI 身份响应")
                Return Success(deviceId, "SCPI 身份响应正常")

            Case DebugDeviceId.Keysight34461
                If CheckCom_34461() <> 0 Then Return Failure(deviceId, "VISA 身份检查失败")
                Return Success(deviceId, "VISA / *IDN? 响应正常")

            Case DebugDeviceId.PlcTray
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
                Return ProbeSerial(deviceId, Com_scaner, "扫码器串口已打开", "扫码器串口未由当前进程打开")

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

    Private Shared Function ProbeSerial(deviceId As DebugDeviceId, port As SerialPort, connectedMessage As String, missingMessage As String) As DebugDeviceResult
        If port IsNot Nothing AndAlso port.IsOpen Then Return Success(deviceId, connectedMessage)
        Return New DebugDeviceResult With {
            .DeviceId = deviceId,
            .State = DebugDeviceState.Unavailable,
            .Message = missingMessage,
            .InterfaceDescription = GetInterfaceDescription(deviceId)
        }
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
