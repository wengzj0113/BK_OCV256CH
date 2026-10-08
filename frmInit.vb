Imports System.IO
Imports System.IO.Ports
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Globalization
Public Class frmInit
    Dim TempInitsetting As Initset
    Private testStandardFileStore As TestStandardFileStore
    Private channelPassColor As Color
    Private channelTestingColor As Color
    Private channelIdleColor As Color
    Private channelResistanceNgColor As Color
    Private channelVoltageNgColor As Color
    Private channelSelectedColor As Color
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
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return
        Dim applyBlockReason As String = String.Empty
        If frmMDI IsNot Nothing AndAlso Not frmMDI.CanApplyInitialSettings(InitSettingsChangeKind.DeviceConnection, applyBlockReason) Then
            lblScmStatus.Text = applyBlockReason
            MsgBox(applyBlockReason, vbOKOnly, "SCM参数读取")
            Return
        End If
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
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return
        Dim settings As ScmTimingSettings = Nothing
        If Not TryGetScmTimingText(settings) Then Return
        Dim applyBlockReason As String = String.Empty
        If frmMDI IsNot Nothing AndAlso Not frmMDI.CanApplyInitialSettings(InitSettingsChangeKind.DeviceConnection, applyBlockReason) Then
            lblScmStatus.Text = applyBlockReason
            MsgBox(applyBlockReason, vbOKOnly, "SCM参数应用")
            Return
        End If

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
            BusinessLogService.Write("操作", "成功", "应用SCM时间参数", "SCM参数", "已应用并保存5项SCM时间参数。", String.Format("继电器稳定={0}ms；通道稳定={1}ms；自动步进={2}ms；触发脉冲={3}ms；相位保护={4}ms。", settings.RelaySettleMs, settings.ChannelSettleMs, settings.AutoStepMs, settings.TriggerPulseMs, settings.PhaseGuardMs), "初始设置")
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
            BusinessLogService.Write("操作", "失败", "应用SCM时间参数", "SCM参数", "SCM时间参数应用失败，本机配置未保存。", lblScmStatus.Text, "初始设置")
            MsgBox(lblScmStatus.Text, vbOKOnly, "SCM应用失败")
        Catch ex As Exception
            AssignScmTimingSettings(Initsetting, previousSettings)
            AssignScmTimingSettings(TempInitsetting, previousSettings)
            lblScmStatus.Text = "SCM应用失败，本机配置未保存：" & ex.Message
            BusinessLogService.Write("操作", "失败", "应用SCM时间参数", "SCM参数", "SCM时间参数应用失败，本机配置未保存。", ex.Message, "初始设置")
            MsgBox(lblScmStatus.Text, vbOKOnly, "SCM应用失败")
        Finally
            SetScmTimingOperationState(False)
        End Try
    End Sub

    Private Sub CmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdOK.Click
        If AuthorizationService.CurrentRole = UserRole.Engineer Then
            EngineerSaveProcessSettings()
            Return
        End If
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return

        If Not TextToStruct() Then Return
        Dim previousSettings As Initset = Initsetting
        Dim currentSettings As Initset = TempInitsetting
        Dim resultMessage As String = String.Empty
        If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then
            If Not frmMDI.SaveAndApplyInitialSettings(currentSettings, resultMessage) Then
                MsgBox(resultMessage, vbOKOnly, "初始设置未保存")
                Return
            End If
        Else
            Try
                SaveInitFile(currentSettings)
                Initsetting = currentSettings
                ApplyColorsToOpenTestForm()
                resultMessage = "初始设置已保存。"
            Catch ex As Exception
                Initsetting = previousSettings
                BusinessLogService.Write("操作", "失败", "保存初始设置", "系统参数", "初始设置保存失败。", ex.ToString(), "初始设置")
                MsgBox("保存初始设置失败。" & Environment.NewLine & ex.Message, vbOKOnly, "保存失败")
                Return
            End Try
        End If

        MsgBox(resultMessage, vbOKOnly, "初始设置")
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Function EngineerSaveProcessSettings() As Boolean
        If Not AuthorizationService.RequirePermission(AppPermission.ManageProcessParameters, Me) Then Return False

        Dim profile As TestStandardProfile = Nothing
        If Not TryCreateTestStandardProfile(profile) Then Return False

        Dim voltageCorrection As Single
        Dim resistanceCorrection As Single
        Dim channelDelay As Decimal
        Dim probeLifespan As Integer
        Dim probeUsedCount As Integer
        Dim barcodeLength As Integer
        Dim calibrationResistance As Single
        Dim overTemperature As Integer

        If Not Single.TryParse(txt_V_B.Text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, voltageCorrection) Then
            ShowTestStandardValidationError("电压修正值必须是有效数字。")
            Return False
        End If
        If Not Single.TryParse(txt_R_B.Text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, resistanceCorrection) Then
            ShowTestStandardValidationError("内阻修正值必须是有效数字。")
            Return False
        End If
        If Not Decimal.TryParse(Txt_DelayTime.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, channelDelay) OrElse channelDelay < 0 Then
            ShowTestStandardValidationError("通道切换时间必须是大于或等于 0 的有效数字。")
            Return False
        End If
        If Not Integer.TryParse(txtProbelifespan.Text.Trim(), probeLifespan) OrElse probeLifespan < 1 Then
            ShowTestStandardValidationError("探针寿命必须是大于 0 的整数。")
            Return False
        End If
        If Not Integer.TryParse(txtProbeUsed.Text.Trim(), probeUsedCount) OrElse probeUsedCount < 0 Then
            ShowTestStandardValidationError("探针已使用次数必须是大于或等于 0 的整数。")
            Return False
        End If
        If Not Integer.TryParse(txtCodeLenth.Text.Trim(), barcodeLength) OrElse barcodeLength < 1 Then
            ShowTestStandardValidationError("条码长度必须是大于 0 的整数。")
            Return False
        End If
        If Not Single.TryParse(txtCailiModelValue.Text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, calibrationResistance) OrElse calibrationResistance <= 0 Then
            ShowTestStandardValidationError("校准模块阻值必须是大于 0 的有效数字。")
            Return False
        End If
        If Not Integer.TryParse(txtOverTemperate.Text.Trim(), overTemperature) OrElse overTemperature < 1 Then
            ShowTestStandardValidationError("超温报警值必须是大于 0 的整数。")
            Return False
        End If

        Dim updatedSettings As Initset = EngineerProcessSettingsPolicy.ApplyTo(Initsetting,
                                                                               profile,
                                                                               voltageCorrection,
                                                                               resistanceCorrection,
                                                                               channelDelay,
                                                                               probeLifespan,
                                                                               probeUsedCount,
                                                                               barcodeLength,
                                                                               calibrationResistance,
                                                                               overTemperature)

        Dim previousSettings As Initset = Initsetting
        Dim resultMessage As String = String.Empty
        Try
            If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then
                If Not frmMDI.SaveAndApplyInitialSettings(updatedSettings, resultMessage) Then
                    MessageBox.Show(Me, resultMessage, "工艺参数未保存", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return False
                End If
            Else
                SaveInitFile(updatedSettings)
                Initsetting = updatedSettings
                resultMessage = "工艺参数已保存。"
            End If
            BusinessLogService.Write("操作", "成功", "保存工艺参数", "测试工艺", "测试标准和工艺参数已保存。",
                                     String.Format(CultureInfo.InvariantCulture,
                                                   "内阻={0}-{1} mΩ；电压={2}-{3} mV；电压修正={4}；内阻修正={5}；通道切换={6}；探针寿命={7}、已用={8}；条码长度={9}；校准模块阻值={10} mΩ；超温报警={11}℃。",
                                                   profile.ResistanceMin, profile.ResistanceMax, profile.VoltageMin, profile.VoltageMax,
                                                   voltageCorrection, resistanceCorrection, channelDelay, probeLifespan, probeUsedCount,
                                                   barcodeLength, calibrationResistance, overTemperature),
                                     "工艺参数")
            MessageBox.Show(Me, resultMessage, "工艺参数", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()
            Return True
        Catch ex As Exception
            Initsetting = previousSettings
            BusinessLogService.Write("操作", "失败", "保存工艺参数", "测试工艺", "工艺参数保存失败。", ex.Message, "工艺参数")
            MessageBox.Show(Me, "保存工艺参数失败：" & ex.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function
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
        channelSelectedColor = palette.SelectedColor
        RefreshChannelColorPreview()
    End Sub

    Private Sub RefreshChannelColorPreview()
        SetChannelColorPreview(pnlPassColor, txtPassColor, channelPassColor)
        SetChannelColorPreview(pnlTestingColor, txtTestingColor, channelTestingColor)
        SetChannelColorPreview(pnlIdleColor, txtIdleColor, channelIdleColor)
        SetChannelColorPreview(pnlResistanceNgColor, txtResistanceNgColor, channelResistanceNgColor)
        SetChannelColorPreview(pnlVoltageNgColor, txtVoltageNgColor, channelVoltageNgColor)
        SetChannelColorPreview(pnlSelectedColor, txtSelectedColor, channelSelectedColor)
    End Sub

    Private Shared Sub SetChannelColorPreview(preview As Panel, valueText As TextBox, value As Color)
        Dim opaqueValue As Color = ChannelStatusColors.NormalizeOpaque(value)
        preview.BackColor = opaqueValue
        valueText.Text = String.Format("#{0:X2}{1:X2}{2:X2}", opaqueValue.R, opaqueValue.G, opaqueValue.B)
    End Sub

    Private Sub ChooseChannelColor(current As Color, ByRef target As Color, preview As Panel, valueText As TextBox)
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return
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

    Private Sub btnSelectedColor_Click(sender As Object, e As EventArgs) Handles btnSelectedColor.Click
        ChooseChannelColor(channelSelectedColor, channelSelectedColor, pnlSelectedColor, txtSelectedColor)
    End Sub

    Private Sub btnResetChannelColors_Click(sender As Object, e As EventArgs) Handles btnResetChannelColors.Click
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return
        Dim palette As ChannelStatusColors = ChannelColorSettings.CreateDefaultPalette()
        channelPassColor = palette.PassColor
        channelTestingColor = palette.TestingColor
        channelIdleColor = palette.IdleColor
        channelResistanceNgColor = palette.ResistanceNgColor
        channelVoltageNgColor = palette.VoltageNgColor
        channelSelectedColor = palette.SelectedColor
        RefreshChannelColorPreview()
    End Sub

    Private Sub frmInit_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If Not (Me.DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime) AndAlso Not AuthorizationService.RequirePermission(AppPermission.ManageProcessParameters, Me) Then
            Close()
            Return
        End If
        ApplyRoleSpecificTabs()
        Dim I As Int16
        InitializeRetest3562Controls()
        LoadChannelColors()
        CmbRecom.Items.Clear()
        cmb_ScanerPort.Items.Clear()
        CmbIORecom.Items.Clear()
        cmb_SCMPort.Items.Clear()
        cmb_TemPort.Items.Clear()
        ComRate.Items.Clear()
        cmbRerate.Items.Clear()
        cmb_SCMRate.Items.Clear()
        cmb_TempRate.Items.Clear()
        cmbIORerate.Items.Clear()
        cmb_ScanerRate.Items.Clear()

        For I = 0 To 32
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
            cmb_TemPort.Items.Add((I + 1).ToString)
        Next
        cmb_SCMPort.SelectedIndex = 0
        cmb_PlcPort.SelectedIndex = 0
        cmb_TemPort.SelectedIndex = 0
        With ComRate
            '.Items.Clear()
            .Items.Add("低速")
            .Items.Add("中速")
            .Items.Add("高速")
            .Items.Add("超快")
            .SelectedIndex = 3
        End With
        With cmbRerate
            .Items.Add(DefaultMain3562BaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        With cmb_SCMRate
            .Items.Add(DefaultScmBaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        With cmb_TempRate
            .Items.Add(DefaultTemperatureBaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        With cmb_PlcRate
            .Items.Clear()
            .Items.Add(DefaultPlcBaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        With cmbIORerate
            .Items.Add(DefaultIoCardBaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        With cmb_ScanerRate
            .Items.Add(DefaultScannerBaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        With cmbRetestPort
            .Items.Clear()
            For I = 0 To 63
                .Items.Add((I + 1).ToString())
            Next
        End With
        With cmbRetestRate
            .Items.Clear()
            .Items.Add(DefaultRetest3562BaudRate.ToString())
            .SelectedIndex = 0
            .Enabled = False
        End With
        Call structToText()
        UpdatePlcTransportControls()
        UpdateScmTransportControls()
    End Sub
    Private Function TextToStruct() As Boolean
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return False
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
            If cmb_PlcPort.SelectedIndex < 0 Then
                MsgBox("PLC COM 模式需要选择串口号。", vbOKOnly, "PLC 参数错误")
                Return False
            End If
            TempInitsetting.PLCMode = 0
            TempInitsetting.PLCComPort = cmb_PlcPort.SelectedIndex
            TempInitsetting.PLCComRate = DefaultPlcBaudRate
        End If

        '通讯参数
        TempInitsetting.ScanerPort = cmb_ScanerPort.SelectedIndex
        TempInitsetting.ScanerRate = DefaultScannerBaudRate

        'Dim SCMIpAdd As String   '中位机IP地址  weng 20220802 添加
        'Dim SCM_port As Integer  '中位机网络端口
        TempInitsetting.SCMIpAdd = Txt_ScmIP.Text
        TempInitsetting.SCM_port = txt_ScmPort.Text
        TempInitsetting.SCMComPort = cmb_SCMPort.SelectedIndex
        TempInitsetting.SCMComRate = DefaultScmBaudRate

        If cmb_TemPort.SelectedIndex < 0 Then
            MsgBox("温度采集模块需要选择通讯端口。", vbOKOnly, "温度模块参数错误")
            Return False
        End If
        TempInitsetting.TemperatureComPort = cmb_TemPort.SelectedIndex
        TempInitsetting.TemperatureComRate = DefaultTemperatureBaudRate

        If cmbRetestPort Is Nothing OrElse cmbRetestPort.SelectedIndex < 0 Then
            MsgBox("NG复测3562需要选择串口号。", vbOKOnly, "复测3562配置错误")
            Return False
        End If
        TempInitsetting.Retest3562Port = cmbRetestPort.SelectedIndex
        TempInitsetting.Retest3562Rate = DefaultRetest3562BaudRate

        TempInitsetting.IpAddOf34461 = txt34461.Text

        TempInitsetting.IOPort = CmbIORecom.SelectedIndex
        TempInitsetting.IORate = DefaultIoCardBaudRate


        TempInitsetting.ComRateMode = ComRate.Text
        TempInitsetting.Recom = CmbRecom.SelectedIndex
        TempInitsetting.ReRate = DefaultMain3562BaudRate
        If Not HasInstrumentSettingsSelection() Then
            MsgBox("请选择仪表量程、采样速度和测量功能。", vbOKOnly, "仪表参数不完整")
            Return False
        End If
        TempInitsetting.Dmm34461VoltageRange = SelectedInstrumentValue(cmb34461Range)
        TempInitsetting.Dmm34461Nplc = SelectedInstrumentValue(cmb34461Nplc)
        TempInitsetting.Dmm34461AutoZero = SelectedInstrumentValue(cmb34461AutoZero)
        TempInitsetting.Main3562Function = SelectedInstrumentValue(cmbMain3562Function)
        TempInitsetting.Main3562ResistanceRange = SelectedInstrumentValue(cmbMain3562ResistanceRange)
        TempInitsetting.Main3562VoltageRange = SelectedInstrumentValue(cmbMain3562VoltageRange)
        TempInitsetting.Main3562SampleRate = SelectedInstrumentValue(cmbMain3562SampleRate)
        TempInitsetting.Retest3562Function = SelectedInstrumentValue(cmbRetest3562Function)
        TempInitsetting.Retest3562ResistanceRange = SelectedInstrumentValue(cmbRetest3562ResistanceRange)
        TempInitsetting.Retest3562VoltageRange = SelectedInstrumentValue(cmbRetest3562VoltageRange)
        TempInitsetting.Retest3562SampleRate = SelectedInstrumentValue(cmbRetest3562SampleRate)
        '所有通讯下拉框已经写入 TempInitsetting 后，再校验复测3562串口冲突。
        If TempInitsetting.Retest3562Port = TempInitsetting.Recom OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.ScanerPort OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.SCMComPort OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.IOPort OrElse
           (Not ChbPLCIP.Checked AndAlso TempInitsetting.Retest3562Port = TempInitsetting.PLCComPort) Then
            MsgBox("NG复测3562串口不能与主3562、扫码枪、中位机、IO卡或PLC串口相同。", vbOKOnly, "复测3562配置错误")
            Return False
        End If
        If TempInitsetting.TemperatureComPort = TempInitsetting.SCMComPort OrElse
           TempInitsetting.TemperatureComPort = TempInitsetting.ScanerPort OrElse
           TempInitsetting.TemperatureComPort = TempInitsetting.Recom OrElse
           TempInitsetting.TemperatureComPort = TempInitsetting.Retest3562Port OrElse
           TempInitsetting.TemperatureComPort = TempInitsetting.IOPort OrElse
           (TempInitsetting.PLCMode = 0 AndAlso TempInitsetting.TemperatureComPort = TempInitsetting.PLCComPort) Then
            MsgBox("温度采集模块串口不能与其他设备串口相同。", vbOKOnly, "温度模块串口冲突")
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
                                    channelVoltageNgColor,
                                    channelSelectedColor))
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

    Private Sub UpdateScmTransportControls()
        Dim tcpSelected As Boolean = ChbSCMIP.Checked
        Dim comSelected As Boolean = ChbScmCom.Checked
        Txt_ScmIP.Visible = tcpSelected
        txt_ScmPort.Visible = tcpSelected
        Label20.Visible = tcpSelected
        Label21.Visible = tcpSelected
        cmb_SCMPort.Visible = comSelected
        cmb_SCMRate.Visible = comSelected
        lbl_SCMPort.Visible = comSelected
        lbl_SCMRate.Visible = comSelected
    End Sub

    Private Sub ChbSCMIP_CheckedChanged(sender As Object, e As EventArgs) Handles ChbSCMIP.CheckedChanged
        If ChbSCMIP.Checked Then ChbScmCom.Checked = False
        UpdateScmTransportControls()
    End Sub

    Private Sub ChbScmCom_CheckedChanged(sender As Object, e As EventArgs) Handles ChbScmCom.CheckedChanged
        If ChbScmCom.Checked Then ChbSCMIP.Checked = False
        UpdateScmTransportControls()
    End Sub
    Private Sub structToText()
        Dim intLoop As Int16
        Dim scmTimingSettings As ScmTimingSettings = NormalizeScmTimingSettings()
        Try
            Dim rateProfile As InstrumentRateProfile = InstrumentRateProfile.FromSettingName(Initsetting.ComRateMode)
            ComRate.SelectedItem = rateProfile.SettingName
        Catch ex As ArgumentException
            ComRate.SelectedIndex = 3
            SerialMiddleMachineClient.LogMessage("Invalid saved instrument rate mode; defaulted settings form to 超快: " & ex.Message)
        End Try
        SelectInstrumentChoice(cmb34461Range, Initsetting.Dmm34461VoltageRange, "10")
        SelectInstrumentChoice(cmb34461Nplc, Initsetting.Dmm34461Nplc, "0.2")
        SelectInstrumentChoice(cmb34461AutoZero, Initsetting.Dmm34461AutoZero, "ON")
        SelectInstrumentChoice(cmbMain3562Function, Initsetting.Main3562Function, "RV")
        SelectInstrumentChoice(cmbMain3562ResistanceRange, Initsetting.Main3562ResistanceRange, "300E-3")
        SelectInstrumentChoice(cmbMain3562VoltageRange, Initsetting.Main3562VoltageRange, "6")
        SelectInstrumentChoice(cmbMain3562SampleRate, Initsetting.Main3562SampleRate, "EXF")
        SelectInstrumentChoice(cmbRetest3562Function, Initsetting.Retest3562Function, "RV")
        SelectInstrumentChoice(cmbRetest3562ResistanceRange, Initsetting.Retest3562ResistanceRange, "300E-3")
        SelectInstrumentChoice(cmbRetest3562VoltageRange, Initsetting.Retest3562VoltageRange, "6")
        SelectInstrumentChoice(cmbRetest3562SampleRate, Initsetting.Retest3562SampleRate, "EXF")
        '**************************
        '通讯参数
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
            If cmb_ScanerRate.Items.Item(intLoop) = Initsetting.ScanerRate Then
                cmb_ScanerRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmb_ScanerRate.SelectedIndex < 0 Then cmb_ScanerRate.SelectedIndex = 0
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
        cmb_TemPort.SelectedIndex = Math.Max(0, Math.Min(cmb_TemPort.Items.Count - 1, Initsetting.TemperatureComPort))
        For intLoop = 0 To cmb_TempRate.Items.Count - 1
            If CInt(cmb_TempRate.Items(intLoop)) = If(Initsetting.TemperatureComRate > 0, Initsetting.TemperatureComRate, 9600) Then
                cmb_TempRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmb_TempRate.SelectedIndex < 0 Then cmb_TempRate.SelectedItem = "9600"
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
            If CInt(cmb_PlcRate.Items.Item(intLoop)) = If(Initsetting.PLCComRate > 0, Initsetting.PLCComRate, 38400) Then
                cmb_PlcRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmb_PlcRate.SelectedIndex < 0 Then cmb_PlcRate.SelectedIndex = 0
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

    Private Sub ApplyRoleSpecificTabs()
        If AuthorizationService.CanAccess(AppPermission.ManageSystemSettings) Then Return

        TabControl1.TabPages.Clear()
        TabControl1.TabPages.Add(TabPage2)
        GroupBox8.Visible = False
        txtPath.Visible = False
        Label15.Visible = False
        Label8.Visible = False
        CmdOK.Text = "保存工艺参数"
        Me.Text = "工艺参数设置"
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
        InitializeInstrumentSettingsControls()
        testStandardFileStore = New TestStandardFileStore(Application.StartupPath)
        ' 在 InitializeComponent() 调用之后添加任何初始化。
    End Sub

    Private Sub btnImportTestStandard_Click(sender As Object, e As EventArgs) Handles btnImportTestStandard.Click
        If Not AuthorizationService.RequirePermission(AppPermission.ManageTestStandards, Me) Then Return
        If Not EnsureTestStandardDirectory() Then Return

        Using dialog As New OpenFileDialog()
            dialog.Title = "导入测试标准"
            dialog.InitialDirectory = testStandardFileStore.DirectoryPath
            dialog.Filter = "测试标准文件 (*.set)|*.set"
            dialog.CheckFileExists = True
            dialog.CheckPathExists = True
            dialog.Multiselect = False
            dialog.RestoreDirectory = True
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            Try
                Dim profile As TestStandardProfile = testStandardFileStore.Read(dialog.FileName)
                Dim resistanceMinText As String = profile.ResistanceMin.ToString(CultureInfo.InvariantCulture)
                Dim resistanceMaxText As String = profile.ResistanceMax.ToString(CultureInfo.InvariantCulture)
                Dim voltageMinText As String = profile.VoltageMin.ToString(CultureInfo.InvariantCulture)
                Dim voltageMaxText As String = profile.VoltageMax.ToString(CultureInfo.InvariantCulture)

                txtRes1.Text = resistanceMinText
                txtRes2.Text = resistanceMaxText
                txtVol1.Text = voltageMinText
                txtVol2.Text = voltageMaxText
            Catch ex As Exception
                MessageBox.Show(Me,
                                "导入测试标准失败，当前参数未更改。" & Environment.NewLine & ex.Message,
                                "测试标准导入失败",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Sub btnSaveTestStandard_Click(sender As Object, e As EventArgs) Handles btnSaveTestStandard.Click
        If Not AuthorizationService.RequirePermission(AppPermission.ManageTestStandards, Me) Then Return
        Dim profile As TestStandardProfile = Nothing
        If Not TryCreateTestStandardProfile(profile) Then Return
        If Not EnsureTestStandardDirectory() Then Return

        Using dialog As New SaveFileDialog()
            dialog.Title = "保存测试标准"
            dialog.InitialDirectory = testStandardFileStore.DirectoryPath
            dialog.Filter = "测试标准文件 (*.set)|*.set"
            dialog.DefaultExt = "set"
            dialog.AddExtension = True
            dialog.OverwritePrompt = True
            dialog.CheckPathExists = True
            dialog.RestoreDirectory = True
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            Try
                Dim selectedDirectory As String = Path.GetFullPath(Path.GetDirectoryName(dialog.FileName))
                Dim standardDirectory As String = Path.GetFullPath(testStandardFileStore.DirectoryPath)
                If Not String.Equals(selectedDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                                     standardDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                                     StringComparison.OrdinalIgnoreCase) Then
                    MessageBox.Show(Me,
                                    "测试标准只能保存到应用目录下的 SETS 文件夹：" & Environment.NewLine & standardDirectory,
                                    "保存位置无效",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information)
                    Return
                End If

                Dim standardName As String = Path.GetFileNameWithoutExtension(dialog.FileName)
                Dim savedPath As String = testStandardFileStore.Save(profile, standardName, overwrite:=True)
                MessageBox.Show(Me,
                                "测试标准已保存：" & Path.GetFileName(savedPath),
                                "保存成功",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show(Me,
                                "保存测试标准失败。请检查标准名称和 SETS 文件夹的写入权限。" & Environment.NewLine & ex.Message,
                                "测试标准保存失败",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Function EnsureTestStandardDirectory() As Boolean
        Try
            Directory.CreateDirectory(testStandardFileStore.DirectoryPath)
            Return True
        Catch ex As Exception
            MessageBox.Show(Me,
                            "无法访问测试标准目录：" & Environment.NewLine & testStandardFileStore.DirectoryPath & Environment.NewLine & ex.Message,
                            "测试标准目录不可用",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return False
        End Try
    End Function

    Private Function TryCreateTestStandardProfile(ByRef profile As TestStandardProfile) As Boolean
        Dim resistanceMin As Decimal
        Dim resistanceMax As Decimal
        Dim voltageMin As Decimal
        Dim voltageMax As Decimal
        Dim numberStyles As NumberStyles = NumberStyles.Number

        If Not Decimal.TryParse(txtRes1.Text.Trim(), numberStyles, CultureInfo.CurrentCulture, resistanceMin) Then
            ShowTestStandardValidationError("内阻最小值必须是有效数字。")
            Return False
        End If
        If Not Decimal.TryParse(txtRes2.Text.Trim(), numberStyles, CultureInfo.CurrentCulture, resistanceMax) Then
            ShowTestStandardValidationError("内阻最大值必须是有效数字。")
            Return False
        End If
        If Not Decimal.TryParse(txtVol1.Text.Trim(), numberStyles, CultureInfo.CurrentCulture, voltageMin) Then
            ShowTestStandardValidationError("电压最小值必须是有效数字。")
            Return False
        End If
        If Not Decimal.TryParse(txtVol2.Text.Trim(), numberStyles, CultureInfo.CurrentCulture, voltageMax) Then
            ShowTestStandardValidationError("电压最大值必须是有效数字。")
            Return False
        End If

        profile = New TestStandardProfile With {
            .ResistanceMin = resistanceMin,
            .ResistanceMax = resistanceMax,
            .VoltageMin = voltageMin,
            .VoltageMax = voltageMax
        }
        Try
            profile.Validate()
            Return True
        Catch ex As ArgumentException
            ShowTestStandardValidationError(ex.Message)
            Return False
        End Try
    End Function

    Private Sub ShowTestStandardValidationError(message As String)
        MessageBox.Show(Me,
                        message,
                        "测试标准参数无效",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)
    End Sub

    Private Sub InitializeInstrumentSettingsControls()
        PopulateInstrumentChoice(cmb34461Range,
            {New InstrumentSettingChoice("100 mV", "0.1"), New InstrumentSettingChoice("1 V", "1"), New InstrumentSettingChoice("10 V", "10"), New InstrumentSettingChoice("100 V", "100"), New InstrumentSettingChoice("1000 V", "1000")})
        PopulateInstrumentChoice(cmb34461Nplc,
            {New InstrumentSettingChoice("0.001", "0.001"), New InstrumentSettingChoice("0.002", "0.002"), New InstrumentSettingChoice("0.006", "0.006"), New InstrumentSettingChoice("0.02", "0.02"), New InstrumentSettingChoice("0.06", "0.06"), New InstrumentSettingChoice("0.2", "0.2"), New InstrumentSettingChoice("1", "1"), New InstrumentSettingChoice("10", "10"), New InstrumentSettingChoice("100", "100")})
        PopulateInstrumentChoice(cmb34461AutoZero,
            {New InstrumentSettingChoice("开启", "ON"), New InstrumentSettingChoice("关闭", "OFF"), New InstrumentSettingChoice("单次归零", "ONCE")})
        PopulateInstrumentChoice(cmbMain3562Function,
            {New InstrumentSettingChoice("内阻 + 电压（ΩV）", "RV"), New InstrumentSettingChoice("仅内阻（Ω）", "RES")})
        PopulateInstrumentChoice(cmbMain3562ResistanceRange, Get3562ResistanceRanges())
        PopulateInstrumentChoice(cmbMain3562VoltageRange, Get3562VoltageRanges())
        PopulateInstrumentChoice(cmbMain3562SampleRate, Get3562SampleRates())
        PopulateInstrumentChoice(cmbRetest3562Function,
            {New InstrumentSettingChoice("内阻 + 电压（ΩV）", "RV")})
        PopulateInstrumentChoice(cmbRetest3562ResistanceRange, Get3562ResistanceRanges())
        PopulateInstrumentChoice(cmbRetest3562VoltageRange, Get3562VoltageRanges())
        PopulateInstrumentChoice(cmbRetest3562SampleRate, Get3562SampleRates())
    End Sub

    Private Shared Function Get3562ResistanceRanges() As InstrumentSettingChoice()
        Return {New InstrumentSettingChoice("3 mΩ", "3E-3"), New InstrumentSettingChoice("30 mΩ", "30E-3"), New InstrumentSettingChoice("300 mΩ", "300E-3"), New InstrumentSettingChoice("3 Ω", "3"), New InstrumentSettingChoice("30 Ω", "30"), New InstrumentSettingChoice("300 Ω", "300"), New InstrumentSettingChoice("3 kΩ", "3000")}
    End Function

    Private Shared Function Get3562VoltageRanges() As InstrumentSettingChoice()
        Return {New InstrumentSettingChoice("6 V", "6"), New InstrumentSettingChoice("60 V", "60"), New InstrumentSettingChoice("100 V（BT3562A）", "100")}
    End Function

    Private Shared Function Get3562SampleRates() As InstrumentSettingChoice()
        Return {New InstrumentSettingChoice("超高速（EX.FAST）", "EXF"), New InstrumentSettingChoice("高速（FAST）", "FAST"), New InstrumentSettingChoice("中速（MEDIUM）", "MED"), New InstrumentSettingChoice("低速（SLOW）", "SLOW")}
    End Function

    Private Shared Sub PopulateInstrumentChoice(combo As ComboBox, choices As InstrumentSettingChoice())
        combo.Items.Clear()
        combo.Items.AddRange(choices)
    End Sub

    Private Function HasInstrumentSettingsSelection() As Boolean
        Dim combos As ComboBox() = {cmb34461Range, cmb34461Nplc, cmb34461AutoZero, cmbMain3562Function,
                                    cmbMain3562ResistanceRange, cmbMain3562VoltageRange, cmbMain3562SampleRate,
                                    cmbRetest3562Function, cmbRetest3562ResistanceRange, cmbRetest3562VoltageRange,
                                    cmbRetest3562SampleRate}
        For Each combo As ComboBox In combos
            If combo Is Nothing OrElse combo.SelectedIndex < 0 Then Return False
        Next
        Return True
    End Function

    Private Shared Function SelectedInstrumentValue(combo As ComboBox) As String
        Return DirectCast(combo.SelectedItem, InstrumentSettingChoice).Value
    End Function

    Private Shared Sub SelectInstrumentChoice(combo As ComboBox, value As String, defaultValue As String)
        Dim target As String = If(String.IsNullOrWhiteSpace(value), defaultValue, value.Trim())
        For index As Integer = 0 To combo.Items.Count - 1
            If String.Equals(DirectCast(combo.Items(index), InstrumentSettingChoice).Value, target, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedIndex = index
                Return
            End If
        Next
        SelectInstrumentChoice(combo, defaultValue, defaultValue)
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
        If cmbRetestPort Is Nothing OrElse cmbRetestPort.SelectedIndex < 0 Then
            btnRetestConnect.Text = "参数无效"
            Return
        End If
        Try
            Using service As New Retest3562Service()
                service.Connect("COM" & (cmbRetestPort.SelectedIndex + 1).ToString())
            End Using
            btnRetestConnect.Text = "连接成功"
        Catch ex As Exception
            btnRetestConnect.Text = "连接失败"
            MessageBox.Show(ex.Message, "NG复测3562连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub RunConnectionTest(sender As Object, deviceName As String, testAction As Action)
        If Not AuthorizationService.RequirePermission(AppPermission.ManageSystemSettings, Me) Then Return
        Dim button As Button = TryCast(sender, Button)
        If button IsNot Nothing Then
            button.Enabled = False
            button.Text = "测试中..."
        End If
        Try
            testAction.Invoke()
            If button IsNot Nothing Then button.Text = "连接成功"
        Catch ex As Exception
            If button IsNot Nothing Then button.Text = "连接失败"
            MessageBox.Show(ex.Message, deviceName & "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            If button IsNot Nothing Then button.Enabled = True
        End Try
    End Sub

    Private Sub ScmConnectTestClick(sender As Object, e As EventArgs) Handles btnScmConnectTest.Click
        RunConnectionTest(sender, "中位机（SCM）", AddressOf TestScmConnection)
    End Sub

    Private Sub TestScmConnection()
        If ChbSCMIP.Checked = ChbScmCom.Checked Then
            Throw New InvalidOperationException("请只选择一种中位机通讯接口（TCP 或 COM）。")
        End If
        If ChbSCMIP.Checked Then
            If String.IsNullOrWhiteSpace(Txt_ScmIP.Text) Then Throw New InvalidOperationException("请填写中位机 TCP 地址。")
            Dim port As Integer
            If Not Integer.TryParse(txt_ScmPort.Text, port) OrElse port < 1 OrElse port > 65535 Then
                Throw New InvalidOperationException("请填写有效的中位机 TCP 端口号。")
            End If
            Using client As New TcpClient()
                Dim connectTask = client.ConnectAsync(Txt_ScmIP.Text.Trim(), port)
                If Not connectTask.Wait(3000) OrElse Not client.Connected Then
                    Throw New IOException("中位机 TCP 连接超时或连接失败。")
                End If
            End Using
            Return
        End If

        If cmb_SCMPort.SelectedIndex < 0 Then
            Throw New InvalidOperationException("请先选择中位机串口号。")
        End If
        Using port As New SerialPort("COM" & (cmb_SCMPort.SelectedIndex + 1).ToString())
            Dim scmClient As New SerialMiddleMachineClient(port)
            If Not scmClient.CheckConnection(3000) Then Throw New IOException("控制板未返回有效协议响应。")
        End Using
    End Sub

    Private Sub PlcConnectTestClick(sender As Object, e As EventArgs) Handles btnPlcConnectTest.Click
        RunConnectionTest(sender, "PLC", AddressOf TestPlcConnection)
    End Sub

    Private Sub TestPlcConnection()
        If ChbPLCIP.Checked = ChbPlcCom.Checked Then
            Throw New InvalidOperationException("请只选择一种 PLC 通讯接口（TCP 或 COM）。")
        End If
        Dim settings As Initset = Initsetting
        If ChbPLCIP.Checked Then
            Dim tcpPort As Integer
            If String.IsNullOrWhiteSpace(Txt_PlcIP.Text) OrElse Not Integer.TryParse(txt_PlcPort.Text, tcpPort) OrElse tcpPort < 1 OrElse tcpPort > 65535 Then
                Throw New InvalidOperationException("请填写有效的 PLC TCP 地址和端口。")
            End If
            settings.PLCMode = 1
            settings.PLCIP = Txt_PlcIP.Text.Trim()
            settings.PLCTcpPort = tcpPort
        Else
            If cmb_PlcPort.SelectedIndex < 0 Then
                Throw New InvalidOperationException("请先选择 PLC 串口号。")
            End If
            settings.PLCMode = 0
            settings.PLCComPort = cmb_PlcPort.SelectedIndex
            settings.PLCComRate = DefaultPlcBaudRate
        End If

        Dim controller As IPlcController = PlcControllerFactory.Create(settings)
        Try
            If Not controller.Connect() Then Throw New IOException("无法连接 PLC。")
            controller.ReadTrayStatus()
        Finally
            controller.Disconnect()
        End Try
    End Sub

    Private Sub DmmConnectTestClick(sender As Object, e As EventArgs) Handles btn34461ConnectTest.Click
        RunConnectionTest(sender, "34461A", AddressOf TestDmmConnection)
    End Sub

    Private Sub TestDmmConnection()
        If String.IsNullOrWhiteSpace(txt34461.Text) Then Throw New InvalidOperationException("请填写 34461A 的 VISA 地址。")
        Dim rm As Ivi.Visa.Interop.ResourceManager = Nothing
        Dim dmm As Ivi.Visa.Interop.FormattedIO488 = Nothing
        Try
            rm = New Ivi.Visa.Interop.ResourceManager()
            dmm = New Ivi.Visa.Interop.FormattedIO488()
            dmm.IO = rm.Open(txt34461.Text.Trim(), Ivi.Visa.Interop.AccessMode.NO_LOCK, 3000, "")
            dmm.IO.Timeout = 3000
            dmm.WriteString("*IDN?")
            Dim identity As String = dmm.ReadString()
            If identity.IndexOf("Keysight Technologies", StringComparison.OrdinalIgnoreCase) < 0 Then
                Throw New IOException("设备身份不匹配：" & identity)
            End If
        Finally
            Try
                If dmm IsNot Nothing AndAlso dmm.IO IsNot Nothing Then dmm.IO.Close()
            Catch
            End Try
            Try
                If dmm IsNot Nothing Then Marshal.FinalReleaseComObject(dmm)
            Catch
            End Try
            Try
                If rm IsNot Nothing Then Marshal.FinalReleaseComObject(rm)
            Catch
            End Try
        End Try
    End Sub

    Private Sub Main3562ConnectTestClick(sender As Object, e As EventArgs) Handles btnMain3562ConnectTest.Click
        RunConnectionTest(sender, "主3562", AddressOf TestMain3562Connection)
    End Sub

    Private Sub TestMain3562Connection()
        If CmbRecom.SelectedIndex < 0 Then
            Throw New InvalidOperationException("请先选择主3562串口号。")
        End If
        Using port As New SerialPort("COM" & (CmbRecom.SelectedIndex + 1).ToString(), DefaultMain3562BaudRate, Parity.None, 8, StopBits.One)
            port.ReadTimeout = 2000
            port.WriteTimeout = 1000
            port.NewLine = ControlChars.Lf
            port.Open()
            port.DiscardInBuffer()
            port.WriteLine("*IDN?")
            Dim identity As String = port.ReadLine()
            If identity.IndexOf("HIOKI", StringComparison.OrdinalIgnoreCase) < 0 Then
                Throw New IOException("未收到3562设备身份响应：" & identity)
            End If
        End Using
    End Sub

    Private Sub ScannerConnectTestClick(sender As Object, e As EventArgs) Handles btnScannerConnectTest.Click
        RunConnectionTest(sender, "扫描枪", AddressOf TestScannerConnection)
    End Sub

    Private Sub TestScannerConnection()
        If cmb_ScanerPort.SelectedIndex < 0 Then
            Throw New InvalidOperationException("请先选择扫描枪串口号。")
        End If
        Dim settings As Initset = Initsetting
        settings.ScanerPort = cmb_ScanerPort.SelectedIndex
        settings.ScanerRate = DefaultScannerBaudRate
        Using scanner As New TrayScannerClient(settings)
            scanner.Open()
        End Using
    End Sub

    Private Sub TemperatureConnectTestClick(sender As Object, e As EventArgs) Handles btnTemperatureConnectTest.Click
        RunConnectionTest(sender, "温度采集模块", AddressOf TestTemperatureConnection)
    End Sub

    Private Sub TestTemperatureConnection()
        If cmb_TemPort.SelectedIndex < 0 Then
            Throw New InvalidOperationException("请先选择温度采集模块串口号。")
        End If
        Using client As New NtcTemperatureModuleClient(cmb_TemPort.SelectedIndex)
            Dim readings As NtcTemperatureReading() = client.ReadTemperatures()
            If readings Is Nothing OrElse readings.Length <> 2 Then Throw New IOException("温度模块未返回前2路温度数据。")
        End Using
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

End Class

Friend NotInheritable Class InstrumentSettingChoice
    Public Sub New(displayText As String, settingValue As String)
        Me.DisplayText = displayText
        Me.Value = settingValue
    End Sub

    Public ReadOnly Property DisplayText As String
    Public ReadOnly Property Value As String

    Public Overrides Function ToString() As String
        Return DisplayText
    End Function
End Class
