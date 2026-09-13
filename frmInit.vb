Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Drawing
Public Class frmInit
    Dim TempInitsetting As Initset
    Private channelPassColor As Color
    Private channelTestingColor As Color
    Private channelIdleColor As Color
    Private channelResistanceNgColor As Color
    Private channelVoltageNgColor As Color

    Private Sub SetScmTimingOperationState(isBusy As Boolean)
        btnScmRead.Enabled = Not isBusy
        btnScmApply.Enabled = Not isBusy
        CmdOK.Enabled = Not isBusy
        Button1.Enabled = Not isBusy
    End Sub

    Private Function NormalizeScmTimingSettings() As ScmTimingSettings
        Dim settings As New ScmTimingSettings()

        If Initsetting.ScmRelaySettleMs >= ScmTimingSettings.MinRelaySettleMs AndAlso
           Initsetting.ScmRelaySettleMs <= ScmTimingSettings.MaxRelaySettleMs Then
            settings.RelaySettleMs = Initsetting.ScmRelaySettleMs
        End If
        If Initsetting.ScmChannelSettleMs >= ScmTimingSettings.MinChannelSettleMs AndAlso
           Initsetting.ScmChannelSettleMs <= ScmTimingSettings.MaxChannelSettleMs Then
            settings.ChannelSettleMs = Initsetting.ScmChannelSettleMs
        End If
        If Initsetting.ScmAutoStepMs >= ScmTimingSettings.MinAutoStepMs AndAlso
           Initsetting.ScmAutoStepMs <= ScmTimingSettings.MaxAutoStepMs Then
            settings.AutoStepMs = Initsetting.ScmAutoStepMs
        End If
        If Initsetting.ScmTriggerPulseMs >= ScmTimingSettings.MinTriggerPulseMs AndAlso
           Initsetting.ScmTriggerPulseMs <= ScmTimingSettings.MaxTriggerPulseMs Then
            settings.TriggerPulseMs = Initsetting.ScmTriggerPulseMs
        End If
        If Initsetting.ScmPhaseGuardMs >= ScmTimingSettings.MinPhaseGuardMs AndAlso
           Initsetting.ScmPhaseGuardMs <= ScmTimingSettings.MaxPhaseGuardMs Then
            settings.PhaseGuardMs = Initsetting.ScmPhaseGuardMs
        End If
        Return settings
    End Function

    Private Sub AssignScmTimingSettings(ByRef target As Initset, settings As ScmTimingSettings)
        target.ScmRelaySettleMs = settings.RelaySettleMs
        target.ScmChannelSettleMs = settings.ChannelSettleMs
        target.ScmAutoStepMs = settings.AutoStepMs
        target.ScmTriggerPulseMs = settings.TriggerPulseMs
        target.ScmPhaseGuardMs = settings.PhaseGuardMs
    End Sub

    Private Sub SetScmTimingText(settings As ScmTimingSettings)
        txtScmRelaySettleMs.Text = settings.RelaySettleMs.ToString()
        txtScmChannelSettleMs.Text = settings.ChannelSettleMs.ToString()
        txtScmAutoStepMs.Text = settings.AutoStepMs.ToString()
        txtScmTriggerPulseMs.Text = settings.TriggerPulseMs.ToString()
        txtScmPhaseGuardMs.Text = settings.PhaseGuardMs.ToString()
    End Sub

    Private Function TryGetScmTimingText(ByRef settings As ScmTimingSettings) As Boolean
        Dim relaySettleMs As Integer
        Dim channelSettleMs As Integer
        Dim autoStepMs As Integer
        Dim triggerPulseMs As Integer
        Dim phaseGuardMs As Integer

        If Not Integer.TryParse(txtScmRelaySettleMs.Text.Trim(), relaySettleMs) Then
            ShowScmTimingValidationError("继电器稳定时间必须是整数，允许范围为5-5000 ms。")
            Return False
        End If
        If Not Integer.TryParse(txtScmChannelSettleMs.Text.Trim(), channelSettleMs) Then
            ShowScmTimingValidationError("通道稳定时间必须是整数，允许范围为0-5000 ms。")
            Return False
        End If
        If Not Integer.TryParse(txtScmAutoStepMs.Text.Trim(), autoStepMs) Then
            ShowScmTimingValidationError("仪表等待时间必须是整数，允许范围为10-10000 ms。")
            Return False
        End If
        If Not Integer.TryParse(txtScmTriggerPulseMs.Text.Trim(), triggerPulseMs) Then
            ShowScmTimingValidationError("触发脉宽时间必须是整数，允许范围为1-1000 ms。")
            Return False
        End If
        If Not Integer.TryParse(txtScmPhaseGuardMs.Text.Trim(), phaseGuardMs) Then
            ShowScmTimingValidationError("遍末保护时间必须是整数，允许范围为100-10000 ms。")
            Return False
        End If

        settings = New ScmTimingSettings(relaySettleMs,
                                          channelSettleMs,
                                          autoStepMs,
                                          triggerPulseMs,
                                          phaseGuardMs)
        Dim validationError As String = settings.Validate()
        If Not String.IsNullOrWhiteSpace(validationError) Then
            ShowScmTimingValidationError(validationError)
            Return False
        End If
        Return True
    End Function

    Private Sub ShowScmTimingValidationError(message As String)
        lblScmStatus.Text = message
        MsgBox(message, vbOKOnly, "SCM参数错误")
    End Sub

    Private Sub btnScmRead_Click(sender As Object, e As EventArgs) Handles btnScmRead.Click
        SetScmTimingOperationState(True)
        lblScmStatus.Text = "正在读取 SCM 当前时间参数..."
        Try
            Dim client As New SerialMiddleMachineClient(frmMDI.Com_SCM)
            Dim settings As ScmTimingSettings = client.ReadScmTimingSettings()
            SetScmTimingText(settings)
            lblScmStatus.Text = "已读取 SCM 当前值，尚未保存到本机配置。"
        Catch ex As Exception
            lblScmStatus.Text = "读取 SCM 失败：" & ex.Message
            MsgBox(lblScmStatus.Text, vbOKOnly, "SCM读取失败")
        Finally
            SetScmTimingOperationState(False)
        End Try
    End Sub

    Private Sub btnScmApply_Click(sender As Object, e As EventArgs) Handles btnScmApply.Click
        Dim settings As ScmTimingSettings = Nothing
        If Not TryGetScmTimingText(settings) Then Return

        Dim previousSettings As New ScmTimingSettings(
            Initsetting.ScmRelaySettleMs,
            Initsetting.ScmChannelSettleMs,
            Initsetting.ScmAutoStepMs,
            Initsetting.ScmTriggerPulseMs,
            Initsetting.ScmPhaseGuardMs)

        SetScmTimingOperationState(True)
        lblScmStatus.Text = "正在向 SCM 写入 5 项时间参数..."
        Try
            Dim client As New SerialMiddleMachineClient(frmMDI.Com_SCM)
            client.WriteScmTimingSettings(settings)

            AssignScmTimingSettings(Initsetting, settings)
            AssignScmTimingSettings(TempInitsetting, settings)
            SaveInitFile()
            SetScmTimingText(settings)
            lblScmStatus.Text = "SCM 5 项时间参数应用成功，已保存到本机配置。"
        Catch ex As ScmTimingWriteException
            AssignScmTimingSettings(Initsetting, previousSettings)
            AssignScmTimingSettings(TempInitsetting, previousSettings)
            lblScmStatus.Text = String.Format(
                "SCM部分写入失败：已成功{0}项；第{1}项(0x{2:X4})失败，值={3}。本机配置未保存。{4}",
                ex.CompletedCount,
                ex.ParameterName,
                ex.RegisterAddress,
                ex.Value,
                ex.InnerException.Message)
            MsgBox(lblScmStatus.Text, vbOKOnly, "SCM应用失败")
        Catch ex As Exception
            AssignScmTimingSettings(Initsetting, previousSettings)
            AssignScmTimingSettings(TempInitsetting, previousSettings)
            lblScmStatus.Text = "SCM应用失败，本机配置未保存：" & ex.Message
            MsgBox(lblScmStatus.Text, vbOKOnly, "SCM应用失败")
        Finally
            SetScmTimingOperationState(False)
        End Try
    End Sub

    Private Sub CmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdOK.Click
        Dim Rebutton As Microsoft.VisualBasic.MsgBoxResult
        If Not TextToStruct() Then Return
        Rebutton = MsgBox("新的设置将在下次启动时生效！如果想使设置马上生效，请退出本程序并重新进入，确定退出本程序吗？", vbYesNo)
        If Rebutton = vbYes Then
            Initsetting = TempInitsetting

            Call SaveInitFile()
            ApplyColorsToOpenTestForm()
            End

        Else
            Initsetting = TempInitsetting
            Call SaveInitFile()
            ApplyColorsToOpenTestForm()
            Me.Close()
        End If
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub ApplyColorsToOpenTestForm()
        If frmMDI Is Nothing OrElse frmMDI.IsDisposed Then Return
        For Each child As Form In frmMDI.MdiChildren
            Dim testForm As frmTest = TryCast(child, frmTest)
            If testForm IsNot Nothing AndAlso Not testForm.IsDisposed Then
                testForm.ApplyConfiguredChannelColors()
            End If
        Next
    End Sub

    Private Sub LoadChannelColors()
        Dim palette As ChannelStatusColors = ChannelColorSettings.FromInitset(Initsetting)
        channelPassColor = palette.PassColor
        channelTestingColor = palette.TestingColor
        channelIdleColor = palette.IdleColor
        channelResistanceNgColor = palette.ResistanceNgColor
        channelVoltageNgColor = palette.VoltageNgColor
        RefreshChannelColorPreview()
    End Sub

    Private Sub RefreshChannelColorPreview()
        SetChannelColorPreview(pnlPassColor, txtPassColor, channelPassColor)
        SetChannelColorPreview(pnlTestingColor, txtTestingColor, channelTestingColor)
        SetChannelColorPreview(pnlIdleColor, txtIdleColor, channelIdleColor)
        SetChannelColorPreview(pnlResistanceNgColor, txtResistanceNgColor, channelResistanceNgColor)
        SetChannelColorPreview(pnlVoltageNgColor, txtVoltageNgColor, channelVoltageNgColor)
    End Sub

    Private Shared Sub SetChannelColorPreview(preview As Panel, valueText As TextBox, value As Color)
        Dim opaqueValue As Color = ChannelStatusColors.NormalizeOpaque(value)
        preview.BackColor = opaqueValue
        valueText.Text = String.Format("#{0:X2}{1:X2}{2:X2}", opaqueValue.R, opaqueValue.G, opaqueValue.B)
    End Sub

    Private Sub ChooseChannelColor(current As Color, ByRef target As Color, preview As Panel, valueText As TextBox)
        colorDialogChannel.Color = current
        If colorDialogChannel.ShowDialog(Me) <> DialogResult.OK Then Return
        target = ChannelStatusColors.NormalizeOpaque(colorDialogChannel.Color)
        SetChannelColorPreview(preview, valueText, target)
    End Sub

    Private Sub btnPassColor_Click(sender As Object, e As EventArgs) Handles btnPassColor.Click
        ChooseChannelColor(channelPassColor, channelPassColor, pnlPassColor, txtPassColor)
    End Sub

    Private Sub btnTestingColor_Click(sender As Object, e As EventArgs) Handles btnTestingColor.Click
        ChooseChannelColor(channelTestingColor, channelTestingColor, pnlTestingColor, txtTestingColor)
    End Sub

    Private Sub btnIdleColor_Click(sender As Object, e As EventArgs) Handles btnIdleColor.Click
        ChooseChannelColor(channelIdleColor, channelIdleColor, pnlIdleColor, txtIdleColor)
    End Sub

    Private Sub btnResistanceNgColor_Click(sender As Object, e As EventArgs) Handles btnResistanceNgColor.Click
        ChooseChannelColor(channelResistanceNgColor, channelResistanceNgColor, pnlResistanceNgColor, txtResistanceNgColor)
    End Sub

    Private Sub btnVoltageNgColor_Click(sender As Object, e As EventArgs) Handles btnVoltageNgColor.Click
        ChooseChannelColor(channelVoltageNgColor, channelVoltageNgColor, pnlVoltageNgColor, txtVoltageNgColor)
    End Sub

    Private Sub btnResetChannelColors_Click(sender As Object, e As EventArgs) Handles btnResetChannelColors.Click
        Dim palette As ChannelStatusColors = ChannelColorSettings.CreateDefaultPalette()
        channelPassColor = palette.PassColor
        channelTestingColor = palette.TestingColor
        channelIdleColor = palette.IdleColor
        channelResistanceNgColor = palette.ResistanceNgColor
        channelVoltageNgColor = palette.VoltageNgColor
        RefreshChannelColorPreview()
    End Sub

    Private Sub frmInit_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim I As Int16
        InitializeRetest3562Controls()
        LoadChannelColors()
        CmbRecom.Items.Clear()
        cmb_ScanerPort.Items.Clear()
        CmbIORecom.Items.Clear()
        cmb_SCMPort.Items.Clear()
        ComRate.Items.Clear()
        cmbRerate.Items.Clear()
        cmb_SCMRate.Items.Clear()
        cmbIORerate.Items.Clear()
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
        Next
        For I = 0 To 63
            cmb_SCMPort.Items.Add((I + 1).ToString)
            cmb_PlcPort.Items.Add((I + 1).ToString)
        Next
        cmb_SCMPort.SelectedIndex = 0
        cmb_PlcPort.SelectedIndex = 0
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
            .Items.Add("19200")
            .Items.Add("57600")
            .SelectedIndex = 1
        End With
        With cmb_SCMRate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmb_PlcRate
            .Items.Clear()
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmbIORerate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmb_ScanerRate
            .Items.Add("9600")
            .Items.Add("19200")
            .Items.Add("57600")
            .SelectedIndex = 0
        End With
        With cmbRetestPort
            .Items.Clear()
            For I = 0 To 63
                .Items.Add((I + 1).ToString())
            Next
        End With
        With cmbRetestRate
            .Items.Clear()
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
        End With
        Call structToText()
        UpdatePlcTransportControls()
    End Sub
    Private Function TextToStruct() As Boolean
        TempInitsetting = Initsetting
        If Not ChbPLCIP.Checked AndAlso Not ChbPlcCom.Checked Then
            ChbPlcCom.Checked = True
        End If

        If ChbPLCIP.Checked Then
            Dim tcpPort As Integer
            If String.IsNullOrWhiteSpace(Txt_PlcIP.Text) OrElse Not Integer.TryParse(txt_PlcPort.Text, tcpPort) OrElse tcpPort < 1 OrElse tcpPort > 65535 Then
                MsgBox("PLC TCP 模式需要填写有效的 IP 地址和端口号。", vbOKOnly, "PLC 参数错误")
                Return False
            End If
            TempInitsetting.PLCMode = 1
            TempInitsetting.PLCIP = Txt_PlcIP.Text.Trim()
            TempInitsetting.PLCTcpPort = tcpPort
        Else
            If cmb_PlcPort.SelectedIndex < 0 OrElse cmb_PlcRate.SelectedIndex < 0 Then
                MsgBox("PLC COM 模式需要选择串口号和波特率。", vbOKOnly, "PLC 参数错误")
                Return False
            End If
            TempInitsetting.PLCMode = 0
            TempInitsetting.PLCComPort = cmb_PlcPort.SelectedIndex
            TempInitsetting.PLCComRate = CInt(cmb_PlcRate.SelectedItem)
        End If

        '通讯参数
        TempInitsetting.ScanerPort = cmb_ScanerPort.SelectedIndex
        TempInitsetting.ScanerRate = cmb_ScanerRate.Items.Item(cmb_ScanerRate.SelectedIndex)

        'Dim SCMIpAdd As String   '中位机IP地址  weng 20220802 添加
        'Dim SCM_port As Integer  '中位机网络端口
        TempInitsetting.SCMIpAdd = Txt_ScmIP.Text
        TempInitsetting.SCM_port = txt_ScmPort.Text
        TempInitsetting.SCMComPort = cmb_SCMPort.SelectedIndex
        TempInitsetting.SCMComRate = CInt(cmb_SCMRate.Items.Item(cmb_SCMRate.SelectedIndex))

        If cmbRetestPort Is Nothing OrElse cmbRetestPort.SelectedIndex < 0 OrElse cmbRetestRate.SelectedIndex < 0 Then
            MsgBox("NG复测3562需要选择串口号和波特率。", vbOKOnly, "复测3562配置错误")
            Return False
        End If
        TempInitsetting.Retest3562Port = cmbRetestPort.SelectedIndex
        TempInitsetting.Retest3562Rate = CInt(cmbRetestRate.SelectedItem)

        TempInitsetting.IpAddOf34461 = txt34461.Text

        TempInitsetting.IOPort = CmbIORecom.SelectedIndex
        TempInitsetting.IORate = cmbIORerate.Items.Item(cmbIORerate.SelectedIndex)


        TempInitsetting.ComRateMode = ComRate.Text
        TempInitsetting.Recom = CmbRecom.SelectedIndex
        TempInitsetting.ReRate = cmbRerate.Items.Item(cmbRerate.SelectedIndex)
        '所有通讯下拉框已经写入 TempInitsetting 后，再校验复测3562串口冲突。
        If TempInitsetting.Retest3562Port = TempInitsetting.Recom OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.ScanerPort OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.SCMComPort OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.IOPort OrElse
           (Not ChbPLCIP.Checked AndAlso TempInitsetting.Retest3562Port = TempInitsetting.PLCComPort) Then
            MsgBox("NG复测3562串口不能与主3562、扫码枪、中位机、IO卡或PLC串口相同。", vbOKOnly, "复测3562配置错误")
            Return False
        End If
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
        ElseIf radioOCV1Set.Checked Then
            TempInitsetting.stationSelect = 1
        ElseIf radioOCV2Set.Checked Then 
            TempInitsetting.stationSelect = 2
        End If
        ChannelColorSettings.ApplyToInitset(
            TempInitsetting,
            New ChannelStatusColors(channelPassColor,
                                    channelTestingColor,
                                    channelIdleColor,
                                    channelResistanceNgColor,
                                    channelVoltageNgColor))
        Return True
    End Function

    Private Sub UpdatePlcTransportControls()
        Dim tcpSelected As Boolean = ChbPLCIP.Checked
        Dim comSelected As Boolean = ChbPlcCom.Checked
        If Not tcpSelected AndAlso Not comSelected Then
            ChbPlcCom.Checked = True
            comSelected = True
        End If

        Txt_PlcIP.Visible = tcpSelected
        txt_PlcPort.Visible = tcpSelected
        Label46.Visible = tcpSelected
        Label47.Visible = tcpSelected
        cmb_PlcPort.Visible = comSelected
        cmb_PlcRate.Visible = comSelected
        Label45.Visible = comSelected
        Label44.Visible = comSelected
    End Sub

    Private Sub ChbPLCIP_CheckedChanged(sender As Object, e As EventArgs) Handles ChbPLCIP.CheckedChanged
        If ChbPLCIP.Checked Then
            ChbPlcCom.Checked = False
        ElseIf Not ChbPlcCom.Checked Then
            ChbPlcCom.Checked = True
        End If
        UpdatePlcTransportControls()
    End Sub

    Private Sub ChbPlcCom_CheckedChanged(sender As Object, e As EventArgs) Handles ChbPlcCom.CheckedChanged
        If ChbPlcCom.Checked Then
            ChbPLCIP.Checked = False
        ElseIf Not ChbPLCIP.Checked Then
            ChbPLCIP.Checked = True
        End If
        UpdatePlcTransportControls()
    End Sub
    Private Sub structToText()
        Dim intLoop As Int16
        Dim scmTimingSettings As ScmTimingSettings = NormalizeScmTimingSettings()
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
        cmbRetestPort.SelectedIndex = Math.Max(0, Math.Min(cmbRetestPort.Items.Count - 1, Initsetting.Retest3562Port))
        For intLoop = 0 To cmbRetestRate.Items.Count - 1
            If CInt(cmbRetestRate.Items(intLoop)) = If(Initsetting.Retest3562Rate > 0, Initsetting.Retest3562Rate, 9600) Then
                cmbRetestRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmbRetestRate.SelectedIndex < 0 Then cmbRetestRate.SelectedIndex = 0
        Txt_PlcIP.Text = If(String.IsNullOrWhiteSpace(Initsetting.PLCIP), "127.0.0.1", Initsetting.PLCIP)
        txt_PlcPort.Text = If(Initsetting.PLCTcpPort > 0, Initsetting.PLCTcpPort.ToString(), "502")
        cmb_PlcPort.SelectedIndex = Math.Max(0, Math.Min(cmb_PlcPort.Items.Count - 1, Initsetting.PLCComPort))
        For intLoop = 0 To cmb_PlcRate.Items.Count - 1
            If CInt(cmb_PlcRate.Items.Item(intLoop)) = If(Initsetting.PLCComRate > 0, Initsetting.PLCComRate, 115200) Then
                cmb_PlcRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If Initsetting.PLCMode = 1 Then
            ChbPLCIP.Checked = True
        Else
            ChbPlcCom.Checked = True
        End If
        UpdatePlcTransportControls()
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
        SetScmTimingText(scmTimingSettings)

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
    Private Sub txtDatabase_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDatabase.Validated, txtMesApiAddress.Validated, txtMesDatabaseServerIP.Validated
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
        InitializeRetest3562Controls()
        ' 在 InitializeComponent() 调用之后添加任何初始化。
    End Sub

    Private Sub InitializeRetest3562Controls()
        If cmbRetestPort IsNot Nothing Then
            AddHandler btnRetestConnect.Click, AddressOf RetestConnectClick
            Return
        End If
        Dim group As New GroupBox() With {
            .Name = "grpRetest3562",
            .Text = "NG复测3562",
            .Location = New Point(11, 409),
            .Size = New Size(361, 60)
        }
        Dim portLabel As New Label() With {.Text = "串口", .AutoSize = True, .Location = New Point(8, 25)}
        Dim rateLabel As New Label() With {.Text = "波特率", .AutoSize = True, .Location = New Point(122, 25)}
        cmbRetestPort = New ComboBox() With {.Name = "cmbRetest3562Port", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(42, 20), .Width = 72}
        cmbRetestRate = New ComboBox() With {.Name = "cmbRetest3562Rate", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(178, 20), .Width = 85}
        btnRetestConnect = New Button() With {.Name = "btnRetest3562Connect", .Text = "测试连接", .Location = New Point(278, 19), .Size = New Size(82, 25)}
        group.Controls.AddRange(New Control() {portLabel, rateLabel, cmbRetestPort, cmbRetestRate, btnRetestConnect})
        AddHandler btnRetestConnect.Click, AddressOf RetestConnectClick
        TabPage1.Controls.Add(group)
    End Sub

    Private Sub RetestConnectClick(ByVal sender As Object, ByVal e As EventArgs)
        If cmbRetestPort Is Nothing OrElse cmbRetestPort.SelectedIndex < 0 OrElse cmbRetestRate.SelectedIndex < 0 Then
            btnRetestConnect.Text = "参数无效"
            Return
        End If
        Try
            Using service As New Retest3562Service()
                service.Connect("COM" & (cmbRetestPort.SelectedIndex + 1).ToString(), CInt(cmbRetestRate.SelectedItem))
            End Using
            btnRetestConnect.Text = "连接成功"
        Catch ex As Exception
            btnRetestConnect.Text = "连接失败"
            MessageBox.Show(ex.Message, "NG复测3562连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
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
