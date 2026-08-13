Imports System.Threading
Imports Ivi.Visa
Imports Ivi.Visa.Interop

Public Class Frm34461Ctrl
    Private rm As ResourceManager = New ResourceManager()

    ' 连接到34465A示波器
    Private dmm As FormattedIO488 = New FormattedIO488()
    Private resourceString As String

    Public Sub InitializeVisaSession()
        resourceString = "TCPIP0::192.168.66.81::inst0::INSTR" ' 替换为你的示波器VISA地址
        dmm.IO = rm.Open(resourceString)
    End Sub

    Private Sub ConnectAndCtrl()

    End Sub
    Private Sub ConnectAndCtrl1()
        ' 在窗体加载时执行初始化操作
        Try


        Catch ex As Exception
            MessageBox.Show($"发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Frm34461Ctrl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '初始化连接34461
        InitializeVisaSession()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim rm As Ivi.Visa.Interop.ResourceManager = New Ivi.Visa.Interop.ResourceManager()   '打开一个新的资源管理器
        Dim myDmm As Ivi.Visa.Interop.FormattedIO488 = New Ivi.Visa.Interop.FormattedIO488() '打开新的格式化IO 488会话
        Dim firstCellNum = 0
        Dim testNum = 20
        Dim delayTim As Integer = testNum * 100
        Dim commString As String
        Try

            Dim DutAddr As String = "TCPIP0::192.168.66.81"   'LAN的示例字符串
            DutAddr = Initsetting.IpAddOf34461
            myDmm.IO = rm.Open(DutAddr, AccessMode.NO_LOCK, 2000, "") '打开DMM的手柄，超时2秒
            myDmm.IO.Timeout = 100000  '您也可以通过执行此命令设置超时，设置为10秒.要计算，不通点位数不同

            '先复位
            myDmm.IO.Clear()  '首先发送一个设备清除，以停止任何正在进行的测量
            myDmm.WriteString("*RST")  'Reset the device
            myDmm.WriteString("*IDN?")  'Get the IDN string                
            Dim IDN As String = myDmm.ReadString()
            Console.WriteLine(IDN)  'report the DMM's identity

            '配置DCV 100V范围，10mV分辨率这是一个快速读数
            myDmm.WriteString("CONF:VOLT:DC")

            '配置触发器计数
            commString = "TRIG:COUN " + testNum.ToString()
            myDmm.WriteString("TRIG:COUN 20")  ' Set the trigger count to 20. 

            '配置触发器源
            myDmm.WriteString("TRIG:SOUR EXT")
            '配置Ext触发器以查找正斜率信号。
            myDmm.WriteString("TRIG:SLOP POS")

            '设置电压表完整连接器以输出正信号。测量完成后，此接头总是发出信号。
            '此命令仅决定信号的极性。它不会打开或关闭信号。
            myDmm.WriteString("OUTP:TRIG:SLOP POS")

            myDmm.WriteString("READ?") '将DMM设置为等待触发状态，并在下一个触发信号后获取读数。
            Dim DCVResult As Object() = myDmm.ReadList(IEEEASCIIType.ASCIIType_Any)  '获取数组中的读数

            For Each rdg As Double In DCVResult '将每个结果写入控制台
                Console.WriteLine(rdg.ToString())
            Next
            '继续拆分数组元素，关键字为“，”，拆分三项：arr(0)通道号，arr(1)成电阻值，arr(2)电压值
            'Dim arr() = DCVResult.Split(",")
            '定义ch ==元素素组数组（0）-1
            Dim ch As Integer = firstCellNum

            '把读出的电压数据放在RecieveData(ch).Voltage中

            For ch = firstCellNum To firstCellNum + testNum - 1
                RecieveData(ch).Voltage = DCVResult(ch - firstCellNum)
                If RecieveData(ch).Voltage > 10000 Then
                    RecieveData(ch).Voltage = 10000
                End If
                TextBox1.AppendText((RecieveData(ch).Voltage).ToString() & vbCrLf)
            Next
        Catch ex As Exception
            Console.WriteLine("Error occured: " + ex.Message)
        Finally

            '关闭您的资源
            Try
                myDmm.IO.Close()
            Catch ex As Exception

            End Try
            Try
                System.Runtime.InteropServices.Marshal.ReleaseComObject(myDmm)
            Catch ex As Exception

            End Try

            Try
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rm)
            Catch ex As Exception

            End Try
        End Try
    End Sub
End Class
