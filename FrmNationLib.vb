Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports Ivi.Visa.Interop

Public Class FrmNationLib
    Dim rm As ResourceManager
    Dim instr As FormattedIO488

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 初始化资源管理器
        rm = New ResourceManager()

        ' 打开与34461A的通信会话
        instr = New FormattedIO488()
        instr.IO = rm.Open("TCPIP0::192.168.1.100::inst0::INSTR")

        ' 配置34461A以使用外部触发
        instr.WriteString(":TRIG:SOUR EXT")

        ' 设置34461A以读取电压值
        instr.WriteString(":SENSe:FUNC 'VOLT:DC'")

        ' 连续触发64次，每次采样一次，数据存储在缓存中
        instr.WriteString(":TRIG:COUN 64")
        instr.WriteString(":TRIG:DEL 0")
        instr.WriteString(":SAMP:COUN 1")
        instr.WriteString(":TRIG:CLE")
        instr.WriteString(":INIT")
    End Sub

    Private Sub TriggerButton_Click(sender As Object, e As EventArgs) Handles TriggerButton.Click
        Try
            ' 等待34461A完成触发和采样
            instr.WriteString("*OPC?")
            instr.ReadString()

            ' 读取所有数据一次并存储在结果字符串中
            instr.WriteString(":DATA:POINTS 64")
            Dim data As String = instr.ReadString()
            Dim dataArray() As String = data.Split(","c)

            ' 将结果显示在TextBox1中
            TextBox1.Text = String.Join(Environment.NewLine, dataArray)
        Catch ex As Exception
            MessageBox.Show("发生错误: " & ex.Message)
        End Try
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' 关闭通信会话
        instr.IO.Close()
        rm.Close()
    End Sub
End Class
