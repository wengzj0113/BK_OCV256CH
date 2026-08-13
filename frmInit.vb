Imports System.IO
Imports System.Runtime.InteropServices
Public Class frmInit
    Dim TempInitsetting As Initset
    Private Sub CmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdOK.Click
        Dim Rebutton As Microsoft.VisualBasic.MsgBoxResult
        Call TextToStruct()
        Rebutton = MsgBox("新的设置将在下次启动时生效！如果想使设置马上生效，请退出本程序并重新进入，确定退出本程序吗？", vbYesNo)
        If Rebutton = vbYes Then
            Initsetting = TempInitsetting

            Call SaveInitFile()
            End

        Else
            Me.Close()
        End If
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub
    Private Sub frmInit_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim I As Int16
        CmbRecom.Items.Clear()
        cmb_ScanerPort.Items.Clear()
        CmbIORecom.Items.Clear()
        CmbEmmRecom.Items.Clear()
        cmb_SCMPort.Items.Clear()
        ComRate.Items.Clear()
        cmbRerate.Items.Clear()
        cmb_SCMRate.Items.Clear()
        cmbIORerate.Items.Clear()
        cmbEmmRerate.Items.Clear()
        cmb_ScanerRate.Items.Clear()

        For I = 0 To 19
            With CmbRecom
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
            With cmb_ScanerPort
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
            With CmbIORecom
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
            With CmbEmmRecom
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
        Next
        For I = 0 To 63
            cmb_SCMPort.Items.Add((I + 1).ToString)
        Next
        cmb_SCMPort.SelectedIndex = 0
        With ComRate
            '.Items.Clear()
            .Items.Add("低速")
            .Items.Add("中速")
            .Items.Add("高速")
            .Items.Add("超快")
            .SelectedIndex = 2
        End With
        With cmbRerate
            .Items.Add("9600")
            .Items.Add("57600")
            .SelectedIndex = 0
        End With
        With cmb_SCMRate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmbIORerate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 0
        End With
        With cmbEmmRerate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 0
        End With
        With cmb_ScanerRate
            .Items.Add("9600")
            .Items.Add("19200")
            .Items.Add("57600")
            .SelectedIndex = 0
        End With
        Call structToText()
    End Sub
    Private Sub TextToStruct()
        '通讯参数
        TempInitsetting.ScanerPort = cmb_ScanerPort.SelectedIndex
        TempInitsetting.ScanerRate = cmb_ScanerRate.Items.Item(cmb_ScanerRate.SelectedIndex)

        'Dim SCMIpAdd As String   '中位机IP地址  weng 20220802 添加
        'Dim SCM_port As Integer  '中位机网络端口
        TempInitsetting.SCMIpAdd = Txt_ScmIP.Text
        TempInitsetting.SCM_port = txt_ScmPort.Text
        TempInitsetting.SCMComPort = cmb_SCMPort.SelectedIndex
        TempInitsetting.SCMComRate = CInt(cmb_SCMRate.Items.Item(cmb_SCMRate.SelectedIndex))

        TempInitsetting.IpAddOf34461 = txt34461.Text

        TempInitsetting.IOPort = CmbIORecom.SelectedIndex
        TempInitsetting.IORate = cmbIORerate.Items.Item(cmbIORerate.SelectedIndex)

        TempInitsetting.EmmPort = CmbEmmRecom.SelectedIndex
        TempInitsetting.EmmRate = cmbEmmRerate.Items.Item(cmbEmmRerate.SelectedIndex)

        TempInitsetting.ComRateMode = ComRate.Text
        TempInitsetting.Recom = CmbRecom.SelectedIndex
        TempInitsetting.ReRate = cmbRerate.Items.Item(cmbRerate.SelectedIndex)
        '电压内阻合格范围
        If Val(txtRes1.Text) - Val(txtRes2.Text) >= 0 Then
            TempInitsetting.upResistance = Val(txtRes1.Text)
            TempInitsetting.downResistance = Val(txtRes2.Text)
        Else
            TempInitsetting.upResistance = Val(txtRes2.Text)
            TempInitsetting.downResistance = Val(txtRes1.Text)
        End If
        If Val(txtVol1.Text) - Val(txtVol2.Text) >= 0 Then
            TempInitsetting.upVoltage = Val(txtVol1.Text)
            TempInitsetting.downVoltage = Val(txtVol2.Text)
        Else
            TempInitsetting.upVoltage = Val(txtVol2.Text)
            TempInitsetting.downVoltage = Val(txtVol1.Text)
        End If
        '数据库
        TempInitsetting.chkDatabase = chkAuto.Checked
        TempInitsetting.strDatabase = txtDatabase.Text
        '文件路径
        TempInitsetting.sPath = txtPath.Text
        TempInitsetting.autoUpdate = chkAuto.Checked
        '电压内阻B值(校准值）
        TempInitsetting.sngV_B = CSng(txt_V_B.Text)
        TempInitsetting.sngR_B = CSng(txt_R_B.Text)
        '条码长度
        TempInitsetting.txtCodeLenth = Val(txtCodeLenth.Text)

        'database userID and password
        TempInitsetting.strUserID = txtUser.Text
        TempInitsetting.strpwd = txtpwd.Text
        TempInitsetting.delayTime = Txt_DelayTime.Text
        '探针的使用次数管理
        TempInitsetting.Probelifespan = txtProbelifespan.Text
        TempInitsetting.ProbeUsedCount = txtProbeUsed.Text
        '用户管理
        TempInitsetting.PsdOfAdmin = txtPsdAdmin.Text
        TempInitsetting.PsdOfOper = txtPsdOper.Text
        TempInitsetting.PsdOfGuest = txtPsdGuest.Text
        'mes 管理
        TempInitsetting.MesApiAddress = txtMesApiAddress.Text
        '校准模块阻值
        TempInitsetting.cailibrateModelValue = txtCailiModelValue.Text
        '超温报警
        TempInitsetting.overTemperateValue = txtOverTemperate.Text
        '工作站选择
        If radioOCV0Set.Checked Then
            TempInitsetting.stationSelect = 0
        End If

        If radioOCV1Set.Checked Then
            TempInitsetting.stationSelect = 1
        Else
            TempInitsetting.stationSelect = 2
        End If
    End Sub
    Private Sub structToText()
        Dim intLoop As Int16
        '**************************
        '通讯参数
        'ComRate.SelectedIndex = Initsetting.SCM_com
        'For intLoop = 0 To ComRate.Items.Count - 1
        '    If cmbRerate.Items.Item(intLoop) = Initsetting.ComRateMode Then
        '        ComRate.SelectedIndex = intLoop
        '        Exit For
        '    End If
        'Next
        'CmbIORecom

        CmbIORecom.SelectedIndex = Initsetting.IOPort
        For intLoop = 0 To cmbIORerate.Items.Count - 1
            If cmbIORerate.Items.Item(intLoop) = Initsetting.IORate Then
                cmbIORerate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        CmbEmmRecom.SelectedIndex = Initsetting.EmmPort
        For intLoop = 0 To cmbEmmRerate.Items.Count - 1
            If cmbEmmRerate.Items.Item(intLoop) = Initsetting.EmmRate Then
                cmbEmmRerate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        CmbRecom.SelectedIndex = Initsetting.Recom
        For intLoop = 0 To cmbRerate.Items.Count - 1
            If cmbRerate.Items.Item(intLoop) = Initsetting.ReRate Then
                cmbRerate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        cmb_ScanerPort.SelectedIndex = Initsetting.ScanerPort
        For intLoop = 0 To cmb_ScanerRate.Items.Count - 1
            If cmb_ScanerRate.Items.Item(intLoop) = Initsetting.ScanerPort Then
                cmb_ScanerRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        'ReadInitFile()
        Txt_ScmIP.Text = Initsetting.SCMIpAdd
        txt_ScmPort.Text = Initsetting.SCM_port
        cmb_SCMPort.SelectedIndex = Math.Max(0, Math.Min(cmb_SCMPort.Items.Count - 1, Initsetting.SCMComPort))
        For intLoop = 0 To cmb_SCMRate.Items.Count - 1
            If CInt(cmb_SCMRate.Items.Item(intLoop)) = Initsetting.SCMComRate Then
                cmb_SCMRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmb_SCMRate.SelectedIndex < 0 Then
            cmb_SCMRate.SelectedIndex = 2
        End If
        '*********************************
        '内阻电压上下限
        txtRes1.Text = Initsetting.downResistance
        txtRes2.Text = Initsetting.upResistance
        txtVol1.Text = Initsetting.downVoltage
        txtVol2.Text = Initsetting.upVoltage
        '**********************************
        '数据库IP
        chkAuto.Checked = Initsetting.chkDatabase
        txtDatabase.Text = Initsetting.strDatabase
        '文件名
        txtPath.Text = Initsetting.sPath
        'TempInitsetting.sngV_B
        chkAuto.Checked = Initsetting.autoUpdate
        '电压内阻B值
        txt_V_B.Text = Initsetting.sngV_B
        txt_R_B.Text = Initsetting.sngR_B
        '条码长度
        txtCodeLenth.Text = Initsetting.txtCodeLenth
        'DataBase userID and password
        txtUser.Text = Initsetting.strUserID
        txtpwd.Text = Initsetting.strpwd
        '通道切换时间
        Txt_DelayTime.Text = Initsetting.delayTime
        '探针使用寿命统计
        txtProbelifespan.Text = Initsetting.Probelifespan
        txtProbeUsed.Text = Initsetting.ProbeUsedCount
        '用户管理
        txtPsdAdmin.Text = Initsetting.PsdOfAdmin
        txtPsdOper.Text = Initsetting.PsdOfOper
        txtPsdGuest.Text = Initsetting.PsdOfGuest
        '万用表的ip地址
        txt34461.Text = Initsetting.IpAddOf34461
        'MES api 地址
        txtMesApiAddress.Text = Initsetting.MesApiAddress
        '校准模块阻值
        txtCailiModelValue.Text = Initsetting.cailibrateModelValue
        '超温报警
        txtOverTemperate.Text = Initsetting.overTemperateValue
        '工作站选择
        If Initsetting.stationSelect = 0 Then
            radioOCV0Set.Checked = True
        End If
        If Initsetting.stationSelect = 1 Then
            radioOCV1Set.Checked = True
        End If
        If Initsetting.stationSelect = 2 Then
            radioOCV2Set.Checked = True
        End If

    End Sub
    Private Sub txtRes1_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRes1.Validated
        If Not IsNumeric(txtRes1.Text) Then
            txtRes1.Clear()
        End If
    End Sub
    Private Sub txtRes2_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRes2.Validated
        If Not IsNumeric(txtRes2.Text) Then
            txtRes2.Clear()
        End If
    End Sub
    Private Sub txtVol1_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtVol1.Validated
        If Not IsNumeric(txtVol1.Text) Then
            txtVol1.Clear()
        End If
    End Sub
    Private Sub txtVol2_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtVol2.Validated, txtOverTemperate.Validated
        If Not IsNumeric(txtVol2.Text) Then
            txtVol2.Clear()
        End If
    End Sub
    Private Sub txtDatabase_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDatabase.Validated, txtMesApiAddress.Validated
        If txtDatabase.Text = "" Then
            txtDatabase.Focus()
        End If
    End Sub
    Private Sub TextBox1_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtPath.Validated
        Dim sPath As String
        sPath = Trim(txtPath.Text)
        If String.IsNullOrEmpty(sPath) Then
            txtPath.Text = "C:\"
        End If
    End Sub

    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()
        ' 在 InitializeComponent() 调用之后添加任何初始化。
    End Sub
    Private Sub txt_V_B_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_V_B.Validated
        If Not IsNumeric(txt_V_B.Text) Then
            txt_V_B.Clear()
        End If
    End Sub
    Private Sub txt_R_B_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_R_B.Validated
        If Not IsNumeric(txt_R_B.Text) Then
            txt_R_B.Clear()
        End If
    End Sub

    Private Sub Label15_Click(sender As Object, e As EventArgs) Handles Label15.Click

    End Sub

    Private Sub Label17_Click(sender As Object, e As EventArgs) Handles Label17.Click, Label41.Click

    End Sub

    Private Sub Label16_Click(sender As Object, e As EventArgs) Handles Label16.Click

    End Sub

    Private Sub txt_V_B_TextChanged(sender As Object, e As EventArgs) Handles txt_V_B.TextChanged

    End Sub

    Private Sub txt_R_B_TextChanged(sender As Object, e As EventArgs) Handles txt_R_B.TextChanged

    End Sub
End Class
