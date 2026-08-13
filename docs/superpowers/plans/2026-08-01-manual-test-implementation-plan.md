# Manual Test Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a modal manual test page that lets operators select any physical channels from CH1~CH256 and run a non-production sparse-channel test.

**Architecture:** Add a new WinForms form `FrmManualTest` that owns channel selection UI, input parsing, manual result display, and manual-test logging. Reuse `FormalTestRunner.RunSparseMeasurement(channelNumbers)` for the actual middle-machine plan, instrument cache handling, data readback, and `RecieveData` mapping. Add a menu item under `MDIForm`'s test menu to open the form with `ShowDialog(Me)`.

**Tech Stack:** VB.NET WinForms, .NET Framework 4.8, existing `FormalTestRunner`, `TestChannelPlanService`, `RecieveData`, `IsNG`, MSBuild x86 Debug.

---

## File Structure

- Create: `FrmManualTest.vb`
  - One focused form class.
  - Creates its controls in code to avoid hand-maintaining 256 Designer controls.
  - Owns channel matrix, selection parsing, manual test execution, result display, and manual log writing.
- Modify: `MDIForm.Designer.vb`
  - Add `ManualTestToolStripMenuItem` declaration and add it under `EditMenu`.
- Modify: `MDIForm.vb`
  - Add click handler that opens `FrmManualTest` with `ShowDialog(Me)`.
- Modify: `MP-60A-RV-64Mem.vbproj`
  - Include `FrmManualTest.vb` in compilation.

## Task 1: Add Manual Test Form Skeleton

**Files:**
- Create: `FrmManualTest.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: Create `FrmManualTest.vb` with an empty modal-safe form**

Create `FrmManualTest.vb`:

```vb
Imports System.IO
Imports System.Text.RegularExpressions

Public Class FrmManualTest
    Inherits Form

    Private ReadOnly channelButtons As New Dictionary(Of Integer, CheckBox)()
    Private ReadOnly selectedChannels As New HashSet(Of Integer)()

    Private matrixPanel As TableLayoutPanel
    Private resultGrid As DataGridView
    Private lblSummary As Label
    Private lblStatus As Label
    Private txtRangeStart As TextBox
    Private txtRangeEnd As TextBox
    Private txtChannelList As TextBox
    Private btnStart As Button
    Private btnClose As Button
    Private isManualTesting As Boolean

    Public Sub New()
        InitializeManualForm()
    End Sub

    Private Sub InitializeManualForm()
        Me.Text = "手动测试"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(1180, 780)
        Me.MinimumSize = New Size(980, 680)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
    End Sub
End Class
```

- [ ] **Step 2: Add the new file to the project**

In `MP-60A-RV-64Mem.vbproj`, add this compile item near the other form files:

```xml
<Compile Include="FrmManualTest.vb" />
```

- [ ] **Step 3: Build to verify the skeleton compiles**

Run:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'MP-60A-RV-64Mem.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86 /p:OutDir=obj\x86\VerifyBuild\
```

Expected:

```text
已成功生成。
0 个警告
0 个错误
```

## Task 2: Build the Manual Test UI

**Files:**
- Modify: `FrmManualTest.vb`

- [ ] **Step 1: Add layout construction methods**

Replace `InitializeManualForm()` with:

```vb
Private Sub InitializeManualForm()
    Me.Text = "手动测试"
    Me.StartPosition = FormStartPosition.CenterParent
    Me.Size = New Size(1180, 780)
    Me.MinimumSize = New Size(980, 680)
    Me.MaximizeBox = False
    Me.MinimizeBox = False

    Dim root As New TableLayoutPanel()
    root.Dock = DockStyle.Fill
    root.ColumnCount = 1
    root.RowCount = 4
    root.RowStyles.Add(New RowStyle(SizeType.Absolute, 112))
    root.RowStyles.Add(New RowStyle(SizeType.Percent, 58))
    root.RowStyles.Add(New RowStyle(SizeType.Percent, 42))
    root.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
    Me.Controls.Add(root)

    root.Controls.Add(BuildCommandPanel(), 0, 0)
    root.Controls.Add(BuildMatrixPanel(), 0, 1)
    root.Controls.Add(BuildResultGrid(), 0, 2)
    root.Controls.Add(BuildBottomPanel(), 0, 3)

    UpdateSelectionSummary()
End Sub
```

- [ ] **Step 2: Add the command panel**

Add this method:

```vb
Private Function BuildCommandPanel() As Control
    Dim panel As New FlowLayoutPanel()
    panel.Dock = DockStyle.Fill
    panel.Padding = New Padding(10)
    panel.AutoScroll = True

    panel.Controls.Add(CreateButton("全选", AddressOf SelectAllChannels))
    panel.Controls.Add(CreateButton("清空", AddressOf ClearSelection))
    panel.Controls.Add(CreateButton("反选", AddressOf InvertSelection))
    panel.Controls.Add(CreateButton("G1全选", AddressOf SelectG1Channels))
    panel.Controls.Add(CreateButton("G2全选", AddressOf SelectG2Channels))

    panel.Controls.Add(New Label() With {.Text = "起始CH", .AutoSize = True, .Padding = New Padding(10, 8, 0, 0)})
    txtRangeStart = New TextBox() With {.Width = 56}
    panel.Controls.Add(txtRangeStart)
    panel.Controls.Add(New Label() With {.Text = "结束CH", .AutoSize = True, .Padding = New Padding(10, 8, 0, 0)})
    txtRangeEnd = New TextBox() With {.Width = 56}
    panel.Controls.Add(txtRangeEnd)
    panel.Controls.Add(CreateButton("添加范围", AddressOf AddRangeSelection))

    panel.Controls.Add(New Label() With {.Text = "零星通道", .AutoSize = True, .Padding = New Padding(10, 8, 0, 0)})
    txtChannelList = New TextBox() With {.Width = 260}
    panel.Controls.Add(txtChannelList)
    panel.Controls.Add(CreateButton("添加列表", AddressOf AddListSelection))

    lblSummary = New Label() With {.AutoSize = True, .Padding = New Padding(10, 8, 0, 0)}
    panel.Controls.Add(lblSummary)

    Return panel
End Function
```

- [ ] **Step 3: Add button factory**

Add this helper:

```vb
Private Function CreateButton(text As String, handler As EventHandler) As Button
    Dim button As New Button()
    button.Text = text
    button.Width = 82
    button.Height = 30
    AddHandler button.Click, handler
    Return button
End Function
```

- [ ] **Step 4: Add the 16x16 matrix**

Add this method:

```vb
Private Function BuildMatrixPanel() As Control
    matrixPanel = New TableLayoutPanel()
    matrixPanel.Dock = DockStyle.Fill
    matrixPanel.ColumnCount = 16
    matrixPanel.RowCount = 16
    matrixPanel.Padding = New Padding(10)

    For i As Integer = 0 To 15
        matrixPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 6.25F))
        matrixPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 6.25F))
    Next

    For channel As Integer = 1 To 256
        Dim box As New CheckBox()
        box.Appearance = Appearance.Button
        box.Text = "CH" & channel.ToString()
        box.TextAlign = ContentAlignment.MiddleCenter
        box.Dock = DockStyle.Fill
        box.Margin = New Padding(2)
        box.Tag = channel
        box.BackColor = If(channel <= 128, Color.AliceBlue, Color.Honeydew)
        Dim localChannel As Integer = If(channel <= 128, channel, channel - 128)
        box.ToolTipText = ""
        ToolTipForChannel(box, channel, localChannel)
        AddHandler box.CheckedChanged, AddressOf ChannelCheckedChanged
        channelButtons(channel) = box
        matrixPanel.Controls.Add(box, (channel - 1) Mod 16, (channel - 1) \ 16)
    Next

    Return matrixPanel
End Function
```

- [ ] **Step 5: Add tooltip helper**

Add this method:

```vb
Private Sub ToolTipForChannel(box As CheckBox, channel As Integer, localChannel As Integer)
    Dim tip As New ToolTip()
    If channel <= 128 Then
        tip.SetToolTip(box, "G1 CH" & localChannel.ToString())
    Else
        tip.SetToolTip(box, "G2 本地 CH" & localChannel.ToString())
    End If
End Sub
```

- [ ] **Step 6: Add result grid**

Add this method:

```vb
Private Function BuildResultGrid() As Control
    resultGrid = New DataGridView()
    resultGrid.Dock = DockStyle.Fill
    resultGrid.AllowUserToAddRows = False
    resultGrid.AllowUserToDeleteRows = False
    resultGrid.ReadOnly = True
    resultGrid.RowHeadersVisible = False
    resultGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    resultGrid.Columns.Add("Channel", "通道")
    resultGrid.Columns.Add("Group", "分组")
    resultGrid.Columns.Add("Voltage", "电压 mV")
    resultGrid.Columns.Add("Resistance", "内阻 mΩ")
    resultGrid.Columns.Add("Result", "判定")
    Return resultGrid
End Function
```

- [ ] **Step 7: Add bottom panel**

Add this method:

```vb
Private Function BuildBottomPanel() As Control
    Dim panel As New Panel()
    panel.Dock = DockStyle.Fill

    lblStatus = New Label()
    lblStatus.Text = "空闲"
    lblStatus.AutoSize = True
    lblStatus.Location = New Point(12, 14)
    panel.Controls.Add(lblStatus)

    btnStart = New Button()
    btnStart.Text = "开始测试"
    btnStart.Width = 100
    btnStart.Height = 30
    btnStart.Anchor = AnchorStyles.Right Or AnchorStyles.Top
    btnStart.Location = New Point(Me.Width - 240, 8)
    AddHandler btnStart.Click, AddressOf StartManualTest
    panel.Controls.Add(btnStart)

    btnClose = New Button()
    btnClose.Text = "关闭"
    btnClose.Width = 80
    btnClose.Height = 30
    btnClose.Anchor = AnchorStyles.Right Or AnchorStyles.Top
    btnClose.Location = New Point(Me.Width - 128, 8)
    AddHandler btnClose.Click, Sub(sender, e) Me.Close()
    panel.Controls.Add(btnClose)

    Return panel
End Function
```

- [ ] **Step 8: Build to catch UI compile errors**

Run the MSBuild command from Task 1.

Expected:

```text
已成功生成。
0 个警告
0 个错误
```

## Task 3: Implement Channel Selection and Parsing

**Files:**
- Modify: `FrmManualTest.vb`

- [ ] **Step 1: Add channel checked handler**

Add:

```vb
Private Sub ChannelCheckedChanged(sender As Object, e As EventArgs)
    Dim box As CheckBox = CType(sender, CheckBox)
    Dim channel As Integer = CInt(box.Tag)

    If box.Checked Then
        selectedChannels.Add(channel)
        box.BackColor = Color.LightSkyBlue
    Else
        selectedChannels.Remove(channel)
        box.BackColor = If(channel <= 128, Color.AliceBlue, Color.Honeydew)
    End If

    UpdateSelectionSummary()
End Sub
```

- [ ] **Step 2: Add bulk selection methods**

Add:

```vb
Private Sub SetChannelChecked(channel As Integer, checked As Boolean)
    If channelButtons.ContainsKey(channel) Then
        channelButtons(channel).Checked = checked
    End If
End Sub

Private Sub SelectAllChannels(sender As Object, e As EventArgs)
    For channel As Integer = 1 To 256
        SetChannelChecked(channel, True)
    Next
End Sub

Private Sub ClearSelection(sender As Object, e As EventArgs)
    For channel As Integer = 1 To 256
        SetChannelChecked(channel, False)
    Next
End Sub

Private Sub InvertSelection(sender As Object, e As EventArgs)
    For channel As Integer = 1 To 256
        SetChannelChecked(channel, Not channelButtons(channel).Checked)
    Next
End Sub

Private Sub SelectG1Channels(sender As Object, e As EventArgs)
    For channel As Integer = 1 To 128
        SetChannelChecked(channel, True)
    Next
End Sub

Private Sub SelectG2Channels(sender As Object, e As EventArgs)
    For channel As Integer = 129 To 256
        SetChannelChecked(channel, True)
    Next
End Sub
```

- [ ] **Step 3: Add summary calculation**

Add:

```vb
Private Sub UpdateSelectionSummary()
    Dim total As Integer = selectedChannels.Count
    Dim g1 As Integer = selectedChannels.Count(Function(channel) channel <= 128)
    Dim g2 As Integer = selectedChannels.Count(Function(channel) channel > 128)
    Dim expectedHioki As Integer = g1 + g2
    Dim expectedDmm As Integer = g2 + g1

    If lblSummary IsNot Nothing Then
        lblSummary.Text = String.Format("已选 {0}/256；G1 {1}；G2 {2}；3562 {3}；34461 {4}", total, g1, g2, expectedHioki, expectedDmm)
    End If
End Sub
```

- [ ] **Step 4: Add range selection**

Add:

```vb
Private Sub AddRangeSelection(sender As Object, e As EventArgs)
    Dim startChannel As Integer
    Dim endChannel As Integer

    If Not Integer.TryParse(txtRangeStart.Text.Trim(), startChannel) OrElse
       Not Integer.TryParse(txtRangeEnd.Text.Trim(), endChannel) Then
        MsgBox("起始通道和结束通道必须是整数。", vbOKOnly, "提示")
        Return
    End If

    If startChannel < 1 OrElse startChannel > 256 OrElse endChannel < 1 OrElse endChannel > 256 Then
        MsgBox("通道范围必须在 1 到 256 之间。", vbOKOnly, "提示")
        Return
    End If

    If startChannel > endChannel Then
        MsgBox("起始通道不能大于结束通道。", vbOKOnly, "提示")
        Return
    End If

    For channel As Integer = startChannel To endChannel
        SetChannelChecked(channel, True)
    Next
End Sub
```

- [ ] **Step 5: Add list parsing**

Add:

```vb
Private Function ParseChannelList(input As String, ByRef invalidItems As List(Of String)) As List(Of Integer)
    invalidItems = New List(Of String)()
    Dim channels As New List(Of Integer)()
    Dim normalized As String = input.ToUpperInvariant().
        Replace("CH", "").
        Replace("，", ",").
        Replace("、", ",").
        Replace(";", ",").
        Replace("；", ",")

    Dim parts As String() = Regex.Split(normalized, "[,\s]+")
    For Each raw As String In parts
        Dim token As String = raw.Trim()
        If token = "" Then
            Continue For
        End If

        Dim channel As Integer
        If Not Integer.TryParse(token, channel) OrElse channel < 1 OrElse channel > 256 Then
            invalidItems.Add(raw)
        Else
            channels.Add(channel)
        End If
    Next

    Return channels.Distinct().OrderBy(Function(channel) channel).ToList()
End Function
```

- [ ] **Step 6: Add list selection handler**

Add:

```vb
Private Sub AddListSelection(sender As Object, e As EventArgs)
    Dim invalidItems As List(Of String) = Nothing
    Dim channels As List(Of Integer) = ParseChannelList(txtChannelList.Text, invalidItems)

    If invalidItems.Count > 0 Then
        MsgBox("以下通道无效：" & String.Join(", ", invalidItems), vbOKOnly, "提示")
        Return
    End If

    If channels.Count = 0 Then
        MsgBox("请输入至少一个通道。", vbOKOnly, "提示")
        Return
    End If

    For Each channel As Integer In channels
        SetChannelChecked(channel, True)
    Next
End Sub
```

- [ ] **Step 7: Add selected channel getter**

Add:

```vb
Private Function GetSelectedChannels() As List(Of Integer)
    Return selectedChannels.OrderBy(Function(channel) channel).ToList()
End Function
```

- [ ] **Step 8: Build**

Run the MSBuild command from Task 1.

Expected:

```text
已成功生成。
0 个警告
0 个错误
```

## Task 4: Implement Manual Test Execution and Result Display

**Files:**
- Modify: `FrmManualTest.vb`

- [ ] **Step 1: Add start handler**

Add:

```vb
Private Sub StartManualTest(sender As Object, e As EventArgs)
    Dim channels As List(Of Integer) = GetSelectedChannels()
    If channels.Count = 0 Then
        MsgBox("请选择至少一个通道。", vbOKOnly, "提示")
        Return
    End If

    If isRuning OrElse isManualTesting Then
        MsgBox("系统正在测试中，请稍后。", vbOKOnly, "提示")
        Return
    End If

    If CheckCom_SCM() <> 1 Then
        MsgBox("中位机串口通讯失败，无法启动手动测试。", vbOKOnly, "错误")
        Return
    End If

    Try
        SetTestingState(True, "测试中...")
        WriteManualLog("开始手动测试：" & String.Join(",", channels))

        Dim formalRunner As New FormalTestRunner()
        formalRunner.RunSparseMeasurement(channels)

        DisplayManualTestResult(channels)
        WriteManualLog("手动测试完成。")
        lblStatus.Text = "测试完成"
    Catch ex As Exception
        lblStatus.Text = "测试失败"
        WriteManualLog("手动测试失败：" & ex.ToString())
        MsgBox("手动测试失败：" & ex.Message, vbOKOnly, "错误")
    Finally
        SetTestingState(False, lblStatus.Text)
    End Try
End Sub
```

- [ ] **Step 2: Add state setter**

Add:

```vb
Private Sub SetTestingState(testing As Boolean, statusText As String)
    isManualTesting = testing
    isRuning = testing
    lblStatus.Text = statusText

    btnStart.Enabled = Not testing
    btnClose.Enabled = Not testing
    For Each control As Control In matrixPanel.Controls
        control.Enabled = Not testing
    Next
End Sub
```

- [ ] **Step 3: Add result display**

Add:

```vb
Private Sub DisplayManualTestResult(channels As IEnumerable(Of Integer))
    resultGrid.Rows.Clear()

    For Each channel As Integer In channels.OrderBy(Function(ch) ch)
        Dim index0 As Integer = channel - 1
        Dim voltageMv As Single = RecieveData(index0).Voltage * 1000
        Dim resistance As Single = RecieveData(index0).Resistance
        Dim resultText As String = If(IsNG(RecieveData(index0).Voltage, resistance) = 0, "OK", "NG")
        Dim groupText As String = If(channel <= 128, "G1", "G2 本地CH" & (channel - 128).ToString())

        resultGrid.Rows.Add("CH" & channel.ToString(),
                            groupText,
                            voltageMv.ToString("0.0000"),
                            resistance.ToString("0.0000"),
                            resultText)
    Next
End Sub
```

- [ ] **Step 4: Add log writer**

Add:

```vb
Private Sub WriteManualLog(message As String)
    Try
        Dim logDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logFile")
        If Not Directory.Exists(logDirectory) Then
            Directory.CreateDirectory(logDirectory)
        End If

        Dim logFile As String = Path.Combine(logDirectory, "ManualTest_" & DateTime.Now.ToString("yyyyMMdd") & ".log")
        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & " " & message & Environment.NewLine)
    Catch
    End Try
End Sub
```

- [ ] **Step 5: Prevent closing during test**

Add:

```vb
Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
    If isManualTesting Then
        MsgBox("手动测试正在进行，请等待测试完成。", vbOKOnly, "提示")
        e.Cancel = True
        Return
    End If

    MyBase.OnFormClosing(e)
End Sub
```

- [ ] **Step 6: Build**

Run the MSBuild command from Task 1.

Expected:

```text
已成功生成。
0 个警告
0 个错误
```

## Task 5: Add MDI Menu Entry

**Files:**
- Modify: `MDIForm.Designer.vb`
- Modify: `MDIForm.vb`

- [ ] **Step 1: Add designer field**

In `MDIForm.Designer.vb`, add near the other menu item declarations:

```vb
Friend WithEvents ManualTestToolStripMenuItem As ToolStripMenuItem
```

- [ ] **Step 2: Instantiate the menu item**

In `InitializeComponent`, add near the other `ToolStripMenuItem` instantiations:

```vb
Me.ManualTestToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
```

- [ ] **Step 3: Add it under the test menu**

Change `EditMenu.DropDownItems.AddRange(...)` so it includes:

```vb
Me.ManualTestToolStripMenuItem
```

The final item list should keep existing items and add manual test after online test:

```vb
Me.EditMenu.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TStripMenuCode, Me.OnLineTest, Me.ManualTestToolStripMenuItem})
```

- [ ] **Step 4: Set menu item properties**

Add in the designer property section:

```vb
'
'ManualTestToolStripMenuItem
'
Me.ManualTestToolStripMenuItem.Name = "ManualTestToolStripMenuItem"
Me.ManualTestToolStripMenuItem.Size = New System.Drawing.Size(245, 26)
Me.ManualTestToolStripMenuItem.Text = "手动测试"
```

- [ ] **Step 5: Add click handler**

In `MDIForm.vb`, add:

```vb
Private Sub ManualTestToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ManualTestToolStripMenuItem.Click
    Using manualForm As New FrmManualTest()
        manualForm.ShowDialog(Me)
    End Using
End Sub
```

- [ ] **Step 6: Build**

Run the MSBuild command from Task 1.

Expected:

```text
已成功生成。
0 个警告
0 个错误
```

## Task 6: Manual Verification Checklist

**Files:**
- No code changes unless verification exposes a defect.

- [ ] **Step 1: Open the app from Visual Studio or the built output**

Expected:

```text
Main MDI window opens.
```

- [ ] **Step 2: Open the manual test page**

Click:

```text
测试(&T) -> 手动测试
```

Expected:

```text
FrmManualTest opens centered over MDIForm and blocks the main window.
```

- [ ] **Step 3: Verify channel selection**

Actions:

```text
Click CH1, CH8, CH129.
Click G1全选.
Click 清空.
Enter start=3 end=8, click 添加范围.
Enter CH25、CH129, click 添加列表.
```

Expected:

```text
Selected count updates.
G1/G2 counts update.
CH3~CH8, CH25, CH129 are selected after the final action.
```

- [ ] **Step 4: Verify invalid inputs**

Actions:

```text
Enter start=8 end=3, click 添加范围.
Enter 0,257,abc, click 添加列表.
```

Expected:

```text
Clear validation messages appear.
No invalid channel is selected.
```

- [ ] **Step 5: Verify empty start protection**

Click `清空`, then click `开始测试`.

Expected:

```text
Message: 请选择至少一个通道。
No test starts.
```

- [ ] **Step 6: Verify execution path with hardware connected**

Select a small safe set such as:

```text
CH3, CH8
```

Click `开始测试`.

Expected:

```text
Buttons disabled while testing.
FormalTestRunner runs sparse measurement.
Results appear in the result grid.
logFile/ManualTest_yyyyMMdd.log contains the test entry.
```

- [ ] **Step 7: Final build**

Run:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'MP-60A-RV-64Mem.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86 /p:OutDir=obj\x86\VerifyBuild\
```

Expected:

```text
已成功生成。
0 个警告
0 个错误
```

## Self-Review

- Spec coverage: The plan covers menu entry, modal form, 256-channel matrix, selection helpers, range/list parsing, sparse manual execution, result display, manual log, and close protection.
- Placeholder scan: No TBD/TODO placeholders remain.
- Type consistency: The plan consistently uses `FrmManualTest`, `GetSelectedChannels`, `ParseChannelList`, `FormalTestRunner.RunSparseMeasurement`, `RecieveData`, and `IsNG`.
