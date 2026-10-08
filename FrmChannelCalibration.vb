Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports System.Windows.Forms

Partial Public Class FrmChannelCalibration
    Inherits Form

    Private ReadOnly service As New CalibrationService()
    Private ReadOnly runner As New ChannelCalibrationRunner(service)
    ' 控件由 FrmChannelCalibration.Designer.vb 管理。
#If False Then
    Private ReadOnly grid As New DataGridView()
    Private ReadOnly txtStandardResistance As New TextBox()
    Private ReadOnly txtStartChannel As New TextBox()
    Private ReadOnly txtEndChannel As New TextBox()
    Private ReadOnly lblStatus As New Label()
    Private ReadOnly progress As New ProgressBar()
    Private ReadOnly btnReload As New Button()
    Private ReadOnly btnSave As New Button()
    Private ReadOnly btnClear As New Button()
    Private ReadOnly btnSelectRange As New Button()
    Private ReadOnly btnSelectAll As New Button()
    Private ReadOnly btnClearSelection As New Button()
    Private ReadOnly btnCalibrateSelected As New Button()
    Private ReadOnly btnStop As New Button()
#End If
    Private stopRequested As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

#If False Then
    Private Sub InitializeCalibrationForm()
        Text = T("256{0}{1}", W(&H901A, &H9053), W(&H6821, &H51C6))
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(980, 680)
        MinimumSize = New Size(860, 560)

        Dim topPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 86,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(8)
        }

        Dim lblStandard As New Label With {
            .Text = T("{0}{1}(mOhm)", W(&H6807, &H51C6), W(&H963B, &H503C)),
            .AutoSize = True,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(3, 7, 3, 3)
        }
        txtStandardResistance.Width = 90
        txtStandardResistance.Text = GetDefaultStandardResistance().ToString(CultureInfo.InvariantCulture)

        Dim lblStartChannel As New Label With {
            .Text = W(&H8D77, &H59CB, &H901A, &H9053),
            .AutoSize = True,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(12, 7, 3, 3)
        }
        txtStartChannel.Width = 56
        txtStartChannel.Text = "1"

        Dim lblEndChannel As New Label With {
            .Text = W(&H7ED3, &H675F, &H901A, &H9053),
            .AutoSize = True,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(8, 7, 3, 3)
        }
        txtEndChannel.Width = 56
        txtEndChannel.Text = CalibrationService.ChannelCount.ToString(CultureInfo.InvariantCulture)

        ConfigureButton(btnSelectRange, W(&H9009, &H62E9, &H8303, &H56F4))
        ConfigureButton(btnSelectAll, W(&H5168, &H9009))
        ConfigureButton(btnClearSelection, W(&H53D6, &H6D88, &H9009, &H62E9))
        ConfigureButton(btnReload, T("{0}{1}", W(&H8BFB, &H53D6), W(&H6821, &H51C6, &H503C)))
        ConfigureButton(btnSave, T("{0}{1}", W(&H4FDD, &H5B58), W(&H6821, &H51C6, &H503C)))
        ConfigureButton(btnClear, W(&H5168, &H90E8, &H6E05, &H96F6))
        ConfigureButton(btnCalibrateSelected, T("{0}{1}", W(&H6821, &H51C6), W(&H9009, &H4E2D, &H901A, &H9053)))
        ConfigureButton(btnStop, W(&H505C, &H6B62))
        btnStop.Enabled = False

        topPanel.Controls.Add(lblStandard)
        topPanel.Controls.Add(txtStandardResistance)
        topPanel.Controls.Add(lblStartChannel)
        topPanel.Controls.Add(txtStartChannel)
        topPanel.Controls.Add(lblEndChannel)
        topPanel.Controls.Add(txtEndChannel)
        topPanel.Controls.Add(btnSelectRange)
        topPanel.Controls.Add(btnSelectAll)
        topPanel.Controls.Add(btnClearSelection)
        topPanel.Controls.Add(btnReload)
        topPanel.Controls.Add(btnSave)
        topPanel.Controls.Add(btnClear)
        topPanel.Controls.Add(btnCalibrateSelected)
        topPanel.Controls.Add(btnStop)

        grid.Dock = DockStyle.Fill
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = True
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.Columns.Add(CreateCheckColumn("Selected", W(&H9009, &H62E9)))
        grid.Columns.Add(CreateTextColumn("ChannelNo", W(&H901A, &H9053), True))
        grid.Columns.Add(CreateTextColumn("CurrentValue", T("{0}(mOhm)", W(&H5F53, &H524D, &H8865, &H507F)), False))
        grid.Columns.Add(CreateTextColumn("Measured", T("{0}(mOhm)", W(&H5B9E, &H6D4B, &H503C)), True))
        grid.Columns.Add(CreateTextColumn("Standard", T("{0}(mOhm)", W(&H6807, &H51C6, &H503C)), True))
        grid.Columns.Add(CreateTextColumn("Compensation", T("{0}(mOhm)", W(&H8BA1, &H7B97, &H8865, &H507F)), True))
        grid.Columns.Add(CreateTextColumn("CalibratedAt", W(&H6821, &H51C6, &H65F6, &H95F4), True))
        grid.Columns.Add(CreateTextColumn("Status", W(&H72B6, &H6001), True))
        grid.Columns("Selected").FillWeight = 35
        grid.Columns("ChannelNo").FillWeight = 45
        grid.Columns("Status").FillWeight = 90

        Dim bottomPanel As New Panel With {
            .Dock = DockStyle.Bottom,
            .Height = 34,
            .Padding = New Padding(8, 4, 8, 4)
        }
        progress.Dock = DockStyle.Right
        progress.Width = 220
        progress.Minimum = 0
        progress.Maximum = CalibrationService.ChannelCount
        lblStatus.Dock = DockStyle.Fill
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        bottomPanel.Controls.Add(lblStatus)
        bottomPanel.Controls.Add(progress)

        Controls.Add(grid)
        Controls.Add(bottomPanel)
        Controls.Add(topPanel)

        AddHandler Load, AddressOf FrmChannelCalibration_Load
        AddHandler btnReload.Click, AddressOf btnReload_Click
        AddHandler btnSave.Click, AddressOf btnSave_Click
        AddHandler btnClear.Click, AddressOf btnClear_Click
        AddHandler btnSelectRange.Click, AddressOf btnSelectRange_Click
        AddHandler btnSelectAll.Click, AddressOf btnSelectAll_Click
        AddHandler btnClearSelection.Click, AddressOf btnClearSelection_Click
        AddHandler btnCalibrateSelected.Click, AddressOf btnCalibrateSelected_Click
        AddHandler btnStop.Click, AddressOf btnStop_Click
    End Sub
#End If

    Private Sub FrmChannelCalibration_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not AuthorizationService.RequirePermission(AppPermission.CalibrateChannels, Me) Then
            Close()
            Return
        End If
        LoadGrid()
    End Sub

    Private Function EnsureCalibrationPermission() As Boolean
        Return AuthorizationService.RequirePermission(AppPermission.CalibrateChannels, Me)
    End Function

    Private Function CreateTextColumn(name As String, headerText As String, readOnlyColumn As Boolean) As DataGridViewTextBoxColumn
        Return New DataGridViewTextBoxColumn With {
            .Name = name,
            .HeaderText = headerText,
            .ReadOnly = readOnlyColumn
        }
    End Function

    Private Function CreateCheckColumn(name As String, headerText As String) As DataGridViewCheckBoxColumn
        Return New DataGridViewCheckBoxColumn With {
            .Name = name,
            .HeaderText = headerText,
            .ReadOnly = False,
            .TrueValue = True,
            .FalseValue = False
        }
    End Function

    Private Sub ConfigureButton(button As Button, text As String)
        button.Text = text
        button.Width = 112
        button.Height = 28
    End Sub

    Private Function GetDefaultStandardResistance() As Double
        If Initsetting.cailibrateModelValue > 0 Then
            Return Initsetting.cailibrateModelValue
        End If

        Return 3.75
    End Function

    Private Function GetStandardResistance() As Double
        Dim standardResistance As Double
        If Not Double.TryParse(txtStandardResistance.Text.Trim(), standardResistance) Then
            Throw New InvalidOperationException(T("{0}{1}", W(&H8BF7, &H8F93, &H5165, &H6709, &H6548, &H7684), W(&H6807, &H51C6, &H963B, &H503C)))
        End If

        If standardResistance <= 0 Then
            Throw New InvalidOperationException(T("{0} > 0", W(&H6807, &H51C6, &H963B, &H503C)))
        End If

        Return standardResistance
    End Function

    Private Function GetChannelNumber(textBox As TextBox, fieldName As String) As Integer
        Dim channelNo As Integer
        If Not Integer.TryParse(textBox.Text.Trim(), channelNo) Then
            Throw New InvalidOperationException(T("{0}{1}", fieldName, W(&H65E0, &H6548)))
        End If

        If channelNo < 1 OrElse channelNo > CalibrationService.ChannelCount Then
            Throw New InvalidOperationException(T("{0}: 1-{1}", fieldName, CalibrationService.ChannelCount))
        End If

        Return channelNo
    End Function

    Private Function GetCheckedChannelIndexes() As List(Of Integer)
        grid.EndEdit()
        Dim indexes As New List(Of Integer)()
        For i As Integer = 0 To CalibrationService.ChannelCount - 1
            If Convert.ToBoolean(If(grid.Rows(i).Cells("Selected").Value, False)) Then
                indexes.Add(i)
            End If
        Next
        Return indexes
    End Function

    Private Sub SetChannelChecked(channelIndex0 As Integer, checked As Boolean)
        grid.Rows(channelIndex0).Cells("Selected").Value = checked
    End Sub

    Private Sub LoadGrid()
        service.LoadIntoGlobalArray()
        grid.Rows.Clear()
        For i As Integer = 0 To CalibrationService.ChannelCount - 1
            grid.Rows.Add(False,
                          i + 1,
                          service.GetGlobalValue(i).ToString(CultureInfo.InvariantCulture),
                          "",
                          "",
                          "",
                          "",
                          W(&H672A, &H6821, &H51C6))
        Next
        lblStatus.Text = T("{0}: {1}", W(&H5DF2, &H8BFB, &H53D6, &H6821, &H51C6, &H6587, &H4EF6), service.CalibrationFilePath)
        progress.Value = 0
    End Sub

    Private Sub SaveGridToGlobal()
        For i As Integer = 0 To CalibrationService.ChannelCount - 1
            Dim valueText As String = Convert.ToString(grid.Rows(i).Cells("CurrentValue").Value)
            Dim value As Double
            If Not Double.TryParse(valueText, value) Then
                Throw New InvalidOperationException(T("{0}{1}{2}", W(&H7B2C), (i + 1).ToString(), W(&H901A, &H9053, &H6821, &H51C6, &H503C, &H65E0, &H6548)))
            End If
            service.SetGlobalValue(i, value)
        Next
    End Sub

    Private Sub SaveStandardResistanceSetting(standardResistance As Double)
        Initsetting.cailibrateModelValue = CSng(standardResistance)
        SaveInitFile()
    End Sub

    Private Sub SaveCalibrationState(standardResistance As Double)
        SaveGridToGlobal()
        SaveStandardResistanceSetting(standardResistance)
        service.SaveFromGlobalArray()
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        If Not EnsureCalibrationPermission() Then Return
        LoadGrid()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not EnsureCalibrationPermission() Then Return
        Try
            Dim standardResistance As Double = GetStandardResistance()
            SaveCalibrationState(standardResistance)
            Dim allChannels As List(Of Integer) = Enumerable.Range(0, CalibrationService.ChannelCount).ToList()
            BusinessLogService.Write("校准", "成功", "保存校准值", "全部256通道", "已保存当前通道校准补偿值。", BuildCalibrationGridDetails(allChannels, standardResistance), "通道校准")
            lblStatus.Text = T("{0}{1}", W(&H6821, &H51C6, &H503C, &H548C), W(&H6807, &H51C6, &H963B, &H503C, &H5DF2, &H4FDD, &H5B58))
        Catch ex As Exception
            BusinessLogService.Write("校准", "失败", "保存校准值", "全部256通道", "校准值保存失败。", ex.Message, "通道校准")
            MessageBox.Show(ex.Message, W(&H4FDD, &H5B58, &H5931, &H8D25), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        If Not EnsureCalibrationPermission() Then Return
        If MessageBox.Show(T("{0}256{1}?", W(&H786E, &H5B9A, &H6E05, &H96F6), W(&H4E2A, &H901A, &H9053, &H7684, &H6821, &H51C6, &H8865, &H507F, &H503C)),
                           W(&H786E, &H8BA4, &H6E05, &H96F6),
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        For i As Integer = 0 To CalibrationService.ChannelCount - 1
            grid.Rows(i).Cells("CurrentValue").Value = "0"
            grid.Rows(i).Cells("Measured").Value = ""
            grid.Rows(i).Cells("Standard").Value = ""
            grid.Rows(i).Cells("Compensation").Value = ""
            grid.Rows(i).Cells("CalibratedAt").Value = ""
            grid.Rows(i).Cells("Status").Value = W(&H5DF2, &H6E05, &H96F6)
        Next
        BusinessLogService.Write("校准", "成功", "清零校准值", "全部256通道", "已将界面中的校准补偿值清零，待保存后生效。", "清零操作尚未保存至 OCV_DATA。", "通道校准")
        lblStatus.Text = T("{0}{1}", W(&H5DF2, &H6E05, &H96F6), W(&HFF0C, &H70B9, &H51FB, &H4FDD, &H5B58, &H540E, &H751F, &H6548))
    End Sub

    Private Sub btnSelectRange_Click(sender As Object, e As EventArgs) Handles btnSelectRange.Click
        If Not EnsureCalibrationPermission() Then Return
        Try
            Dim startChannel As Integer = GetChannelNumber(txtStartChannel, W(&H8D77, &H59CB, &H901A, &H9053))
            Dim endChannel As Integer = GetChannelNumber(txtEndChannel, W(&H7ED3, &H675F, &H901A, &H9053))
            If startChannel > endChannel Then
                Throw New InvalidOperationException(W(&H8D77, &H59CB, &H901A, &H9053) & " > " & W(&H7ED3, &H675F, &H901A, &H9053))
            End If

            For i As Integer = 0 To CalibrationService.ChannelCount - 1
                SetChannelChecked(i, i >= startChannel - 1 AndAlso i <= endChannel - 1)
            Next
            lblStatus.Text = T("{0}: {1}-{2}", W(&H5DF2, &H9009, &H62E9, &H8303, &H56F4), startChannel, endChannel)
        Catch ex As Exception
            MessageBox.Show(ex.Message, W(&H9009, &H62E9, &H5931, &H8D25), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSelectAll_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        If Not EnsureCalibrationPermission() Then Return
        For i As Integer = 0 To CalibrationService.ChannelCount - 1
            SetChannelChecked(i, True)
        Next
        lblStatus.Text = W(&H5DF2, &H5168, &H9009)
    End Sub

    Private Sub btnClearSelection_Click(sender As Object, e As EventArgs) Handles btnClearSelection.Click
        If Not EnsureCalibrationPermission() Then Return
        For i As Integer = 0 To CalibrationService.ChannelCount - 1
            SetChannelChecked(i, False)
        Next
        lblStatus.Text = W(&H5DF2, &H53D6, &H6D88, &H9009, &H62E9)
    End Sub

    Private Sub btnCalibrateSelected_Click(sender As Object, e As EventArgs) Handles btnCalibrateSelected.Click
        If Not EnsureCalibrationPermission() Then Return
        Dim channelIndexes As List(Of Integer) = Nothing
        Dim standardResistance As Double = 0
        Try
            standardResistance = GetStandardResistance()
            channelIndexes = GetCheckedChannelIndexes()
            If channelIndexes.Count = 0 Then
                If grid.CurrentRow Is Nothing Then
                    Throw New InvalidOperationException(W(&H8BF7, &H9009, &H62E9, &H901A, &H9053))
                End If
                channelIndexes.Add(grid.CurrentRow.Index)
                SetChannelChecked(grid.CurrentRow.Index, True)
            End If

            If channelIndexes.Count = 1 Then
                CalibrateOneRow(channelIndexes(0), standardResistance, False)
                SaveCalibrationState(standardResistance)
                BusinessLogService.Write("校准", "成功", "校准并保存单通道", "通道 " & (channelIndexes(0) + 1).ToString(CultureInfo.InvariantCulture), "单通道校准完成并已保存。", BuildCalibrationGridDetails(channelIndexes, standardResistance), "通道校准")
                lblStatus.Text = T("{0}{1}", W(&H5355, &H901A, &H9053, &H6821, &H51C6, &H5B8C, &H6210), W(&HFF0C, &H5DF2, &H81EA, &H52A8, &H4FDD, &H5B58))
            Else
                RunBatchCalibration(channelIndexes, standardResistance)
            End If
        Catch ex As Exception
            Dim failedTarget As String = If(channelIndexes Is Nothing OrElse channelIndexes.Count = 0, "所选通道", String.Join(", ", channelIndexes.Select(Function(index) (index + 1).ToString(CultureInfo.InvariantCulture))))
            Dim failureDetails As String = ex.Message & If(standardResistance > 0, "；标准电阻=" & standardResistance.ToString("0.000", CultureInfo.InvariantCulture) & " mΩ", "")
            If channelIndexes IsNot Nothing AndAlso channelIndexes.All(Function(index) index >= 0 AndAlso index < grid.Rows.Count) Then
                failureDetails &= Environment.NewLine & BuildCalibrationGridDetails(channelIndexes, standardResistance)
            End If
            BusinessLogService.Write("校准", "失败", "执行校准", failedTarget, "校准操作失败。", failureDetails, "通道校准")
            MessageBox.Show(ex.Message, W(&H6821, &H51C6, &H5931, &H8D25), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        stopRequested = True
        Try
            runner.StopCurrentCalibration()
        Catch ex As Exception
            lblStatus.Text = ex.Message
            Return
        End Try
        lblStatus.Text = T("{0}...", W(&H6B63, &H5728, &H505C, &H6B62, &H6279, &H91CF, &H6821, &H51C6))
    End Sub

    Private Sub RunBatchCalibration(channelIndexes As List(Of Integer), standardResistance As Double)
        If channelIndexes Is Nothing OrElse channelIndexes.Count = 0 Then
            Throw New InvalidOperationException(W(&H8BF7, &H9009, &H62E9, &H901A, &H9053))
        End If

        channelIndexes.Sort()
        If MessageBox.Show(BuildPlanSummary(channelIndexes),
                           W(&H786E, &H8BA4, &H6279, &H91CF, &H6821, &H51C6),
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        stopRequested = False
        SetCalibrationButtonsEnabled(False)
        progress.Maximum = channelIndexes.Count
        progress.Value = 0
        Dim results As List(Of ChannelCalibrationResult) = Nothing

        Try
            For Each channelIndex0 As Integer In channelIndexes
                grid.Rows(channelIndex0).Cells("Status").Value = W(&H7B49, &H5F85, &H6821, &H51C6)
            Next
            Application.DoEvents()

            results = runner.CalibrateChannels(channelIndexes, standardResistance)
            For i As Integer = 0 To results.Count - 1
                If stopRequested Then
                    Exit For
                End If

                ApplyCalibrationResultToRow(results(i))
                progress.Value = i + 1
                Application.DoEvents()
            Next
        Finally
            SetCalibrationButtonsEnabled(True)
            btnStop.Enabled = False
        End Try

        If stopRequested Then
            BusinessLogService.Write("校准", "失败", "停止批量校准", BuildCalibrationTarget(channelIndexes), "批量校准已停止。", BuildCalibrationResultDetails(results, standardResistance), "通道校准")
            lblStatus.Text = W(&H6279, &H91CF, &H6821, &H51C6, &H5DF2, &H505C, &H6B62)
        Else
            SaveCalibrationState(standardResistance)
            Dim succeeded As Integer = results.Where(Function(result) result.Success).Count()
            Dim failed As Integer = results.Count - succeeded
            BusinessLogService.Write("校准", If(failed = 0, "成功", "失败"), "批量校准并保存", BuildCalibrationTarget(channelIndexes), String.Format(CultureInfo.InvariantCulture, "批量校准完成：成功 {0} 通道，失败 {1} 通道。", succeeded, failed), BuildCalibrationResultDetails(results, standardResistance), "通道校准")
            lblStatus.Text = T("{0}{1}", W(&H6279, &H91CF, &H6821, &H51C6, &H5B8C, &H6210), W(&HFF0C, &H5DF2, &H81EA, &H52A8, &H4FDD, &H5B58))
        End If
        progress.Maximum = CalibrationService.ChannelCount
    End Sub

    Private Sub SetCalibrationButtonsEnabled(enabled As Boolean)
        btnReload.Enabled = enabled
        btnSave.Enabled = enabled
        btnClear.Enabled = enabled
        btnSelectRange.Enabled = enabled
        btnSelectAll.Enabled = enabled
        btnClearSelection.Enabled = enabled
        btnCalibrateSelected.Enabled = enabled
        btnStop.Enabled = Not enabled
    End Sub

    Private Sub CalibrateOneRow(channelIndex0 As Integer, standardResistance As Double, keepFixtureClosed As Boolean)
        lblStatus.Text = T("{0}{1}{2}...", W(&H6B63, &H5728, &H6821, &H51C6, &H7B2C), (channelIndex0 + 1).ToString(), W(&H901A, &H9053))
        grid.Rows(channelIndex0).Cells("Status").Value = W(&H6821, &H51C6, &H4E2D)
        grid.CurrentCell = grid.Rows(channelIndex0).Cells("ChannelNo")
        grid.FirstDisplayedScrollingRowIndex = Math.Max(0, channelIndex0 - 5)
        Application.DoEvents()

        Dim result As ChannelCalibrationResult = runner.CalibrateSingleChannel(channelIndex0, standardResistance, keepFixtureClosed)
        grid.Rows(channelIndex0).Cells("Measured").Value = result.MeasuredResistance.ToString("0.000", CultureInfo.InvariantCulture)
        grid.Rows(channelIndex0).Cells("Standard").Value = result.StandardResistance.ToString("0.000", CultureInfo.InvariantCulture)
        grid.Rows(channelIndex0).Cells("Compensation").Value = result.Compensation.ToString("0.000", CultureInfo.InvariantCulture)
        grid.Rows(channelIndex0).Cells("CalibratedAt").Value = result.CalibratedAt.ToString("yyyy/MM/dd HH:mm:ss")
        grid.Rows(channelIndex0).Cells("Status").Value = If(result.Success, W(&H5DF2, &H6821, &H51C6), T("{0}: {1}", W(&H5931, &H8D25), result.Message))

        If result.Success Then
            grid.Rows(channelIndex0).Cells("CurrentValue").Value = result.Compensation.ToString(CultureInfo.InvariantCulture)
        ElseIf Not keepFixtureClosed Then
            Throw New InvalidOperationException(result.Message)
        End If
    End Sub

    Private Sub ApplyCalibrationResultToRow(result As ChannelCalibrationResult)
        Dim channelIndex0 As Integer = result.ChannelIndex0
        grid.Rows(channelIndex0).Cells("Measured").Value = result.MeasuredResistance.ToString("0.000", CultureInfo.InvariantCulture)
        grid.Rows(channelIndex0).Cells("Standard").Value = result.StandardResistance.ToString("0.000", CultureInfo.InvariantCulture)
        grid.Rows(channelIndex0).Cells("Compensation").Value = result.Compensation.ToString("0.000", CultureInfo.InvariantCulture)
        grid.Rows(channelIndex0).Cells("CalibratedAt").Value = result.CalibratedAt.ToString("yyyy/MM/dd HH:mm:ss")
        grid.Rows(channelIndex0).Cells("Status").Value = If(result.Success, W(&H5DF2, &H6821, &H51C6), T("{0}: {1}", W(&H5931, &H8D25), result.Message))

        If result.Success Then
            grid.Rows(channelIndex0).Cells("CurrentValue").Value = result.Compensation.ToString(CultureInfo.InvariantCulture)
        End If
    End Sub

    Private Function BuildCalibrationTarget(channelIndexes As IEnumerable(Of Integer)) As String
        Dim channels As List(Of Integer) = channelIndexes.Select(Function(index) index + 1).OrderBy(Function(channel) channel).ToList()
        If channels.Count = 0 Then Return "无通道"
        If channels.Count = CalibrationService.ChannelCount Then Return "全部256通道"
        Return String.Format(CultureInfo.InvariantCulture, "通道 {0}-{1}（共{2}个）", channels(0), channels(channels.Count - 1), channels.Count)
    End Function

    Private Function BuildCalibrationGridDetails(channelIndexes As IEnumerable(Of Integer), standardResistance As Double) As String
        Dim lines As New StringBuilder()
        lines.AppendLine("标准电阻：" & standardResistance.ToString("0.000", CultureInfo.InvariantCulture) & " mΩ")
        For Each channelIndex0 As Integer In channelIndexes.OrderBy(Function(index) index)
            Dim row As DataGridViewRow = grid.Rows(channelIndex0)
            lines.AppendLine(String.Format(CultureInfo.InvariantCulture,
                                           "通道 {0}: 测量={1}; 标准={2}; 补偿={3}; 状态={4}",
                                           channelIndex0 + 1,
                                           Convert.ToString(row.Cells("Measured").Value, CultureInfo.InvariantCulture),
                                           Convert.ToString(row.Cells("Standard").Value, CultureInfo.InvariantCulture),
                                           Convert.ToString(row.Cells("Compensation").Value, CultureInfo.InvariantCulture),
                                           Convert.ToString(row.Cells("Status").Value, CultureInfo.CurrentCulture)))
        Next
        Return lines.ToString()
    End Function

    Private Function BuildCalibrationResultDetails(results As IEnumerable(Of ChannelCalibrationResult), standardResistance As Double) As String
        Dim lines As New StringBuilder()
        lines.AppendLine("标准电阻：" & standardResistance.ToString("0.000", CultureInfo.InvariantCulture) & " mΩ")
        If results Is Nothing Then Return lines.ToString()
        For Each result As ChannelCalibrationResult In results.OrderBy(Function(item) item.ChannelIndex0)
            lines.AppendLine(String.Format(CultureInfo.InvariantCulture,
                                           "通道 {0}: 测量={1:0.000} mΩ; 标准={2:0.000} mΩ; 补偿={3:0.000} mΩ; 结果={4}{5}",
                                           result.ChannelIndex0 + 1,
                                           result.MeasuredResistance,
                                           result.StandardResistance,
                                           result.Compensation,
                                           If(result.Success, "成功", "失败"),
                                           If(String.IsNullOrWhiteSpace(result.Message), "", "；" & result.Message)))
        Next
        Return lines.ToString()
    End Function

    Private Function BuildPlanSummary(channelIndexes As IList(Of Integer)) As String
        Dim g1 As New List(Of Integer)()
        Dim g2Physical As New List(Of Integer)()
        Dim execution As New List(Of Integer)()

        For Each channelIndex0 As Integer In channelIndexes
            Dim physicalChannel As Integer = channelIndex0 + 1
            execution.Add(physicalChannel)
            If physicalChannel <= 128 Then
                g1.Add(physicalChannel)
            Else
                g2Physical.Add(physicalChannel)
            End If
        Next

        g1.Sort()
        g2Physical.Sort()
        execution.Sort()

        Dim g2Local As New List(Of Integer)()
        For Each physicalChannel As Integer In g2Physical
            g2Local.Add(physicalChannel - 128)
        Next

        Dim sb As New StringBuilder()
        sb.AppendLine(T("Channels selected: {0}", execution.Count))
        sb.AppendLine(T("Mode: {0}", If(g1.Count > 0 AndAlso g2Physical.Count = 0, "Sync G1", "Independent G1/G2")))
        sb.AppendLine(T("G1 physical: {0}", FormatChannels(g1)))
        sb.AppendLine(T("G1 plan: {0}", If(g1.Count = 0, "-", If(IsContinuous(g1), "Range", "List"))))
        sb.AppendLine(T("G2 physical: {0}", FormatChannels(g2Physical)))
        sb.AppendLine(T("G2 local: {0}", FormatChannels(g2Local)))
        sb.AppendLine(T("G2 plan: {0}", If(g2Local.Count = 0, "-", If(IsContinuous(g2Local), "Range", "List"))))
        sb.AppendLine(T("Execution order: {0}", FormatChannels(execution)))
        sb.AppendLine("Calibration values will be saved automatically after completion.")
        Return sb.ToString()
    End Function

    Private Shared Function FormatChannels(channels As IList(Of Integer)) As String
        If channels Is Nothing OrElse channels.Count = 0 Then
            Return "-"
        End If

        Dim sorted As New List(Of Integer)(channels)
        sorted.Sort()
        If IsContinuous(sorted) Then
            If sorted.Count = 1 Then
                Return "CH" & sorted(0).ToString()
            End If
            Return "CH" & sorted(0).ToString() & "-CH" & sorted(sorted.Count - 1).ToString()
        End If

        Dim values As New List(Of String)()
        Dim displayCount As Integer = Math.Min(sorted.Count, 24)
        For i As Integer = 0 To displayCount - 1
            values.Add("CH" & sorted(i).ToString())
        Next
        If sorted.Count > displayCount Then
            values.Add("... total " & sorted.Count.ToString())
        End If
        Return String.Join(", ", values.ToArray())
    End Function

    Private Shared Function IsContinuous(channels As IList(Of Integer)) As Boolean
        If channels Is Nothing OrElse channels.Count <= 1 Then
            Return True
        End If

        For i As Integer = 1 To channels.Count - 1
            If channels(i) <> channels(i - 1) + 1 Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Function W(ParamArray codes As Integer()) As String
        Dim chars(codes.Length - 1) As Char
        For i As Integer = 0 To codes.Length - 1
            chars(i) = ChrW(codes(i))
        Next
        Return New String(chars)
    End Function

    Private Shared Function T(format As String, ParamArray args As Object()) As String
        Return String.Format(CultureInfo.InvariantCulture, format, args)
    End Function
End Class
