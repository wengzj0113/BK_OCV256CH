Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

''' <summary>
''' Owns the responsive layout containers for the test form.
''' It only reparents existing controls; it does not change test behavior.
''' </summary>
Public NotInheritable Class ResponsiveTestLayout
    '左栏中的历史控件仍使用固定的内部坐标,需要为输入框和四个通道按钮
    '预留滚动条、内容边距以及边界余量,否则窄栏会把右侧控件裁掉。
    Private Const LeftColumnWidth As Single = 350.0F
    Private rootLayout As TableLayoutPanel
    Private rightLayout As TableLayoutPanel
    Private leftContentPanel As TableLayoutPanel

    Public ReadOnly Property Root As TableLayoutPanel
        Get
            Return rootLayout
        End Get
    End Property

    Public Sub Apply(ByVal host As Control,
                     ByVal leftPanel As Panel,
                     ByVal statistics As Control,
                     ByVal channelGrid As Control,
                     ByVal legacyPicture As Control)
        If host Is Nothing OrElse leftPanel Is Nothing OrElse statistics Is Nothing OrElse channelGrid Is Nothing Then Return

        Dim firstApply As Boolean = (rootLayout Is Nothing)
        If firstApply Then
            rootLayout = CreateRootLayout()
            rightLayout = CreateRightLayout()

            host.SuspendLayout()
            rootLayout.SuspendLayout()
            Try
                If legacyPicture IsNot Nothing Then
                    legacyPicture.Visible = False
                End If

                host.Controls.Remove(leftPanel)
                host.Controls.Remove(statistics)
                host.Controls.Remove(channelGrid)

                '注意:不得在 Dock=Fill 之后再设置 Anchor——Dock 与 Anchor 互斥,
                '设 Anchor 会把 Dock 重置为 None,导致首开布局按初始尺寸计算、错位,
                '重新最大化触发重排后才恢复。保持 Dock=Fill 即可让控件始终跟随单元格。
                leftPanel.Dock = DockStyle.Fill
                leftPanel.AutoSize = False
                leftPanel.MinimumSize = New Size(0, 0)
                leftPanel.AutoScroll = True
                leftPanel.HorizontalScroll.Enabled = True
                leftPanel.VerticalScroll.Enabled = True
                PrepareScrollableLeftPanel(leftPanel)

                statistics.Dock = DockStyle.Fill
                statistics.AutoSize = False
                statistics.MinimumSize = New Size(0, 0)
                statistics.Margin = New Padding(0)

                channelGrid.Dock = DockStyle.Fill
                channelGrid.AutoSize = False
                channelGrid.MinimumSize = New Size(0, 0)
                channelGrid.Margin = New Padding(0)

                rightLayout.Controls.Add(statistics, 0, 0)
                rightLayout.Controls.Add(channelGrid, 0, 1)
                rootLayout.Controls.Add(leftPanel, 0, 0)
                rootLayout.Controls.Add(rightLayout, 1, 0)
                host.Controls.Add(rootLayout)
                rootLayout.BringToFront()
            Finally
                rootLayout.ResumeLayout(False)
                host.ResumeLayout(False)
            End Try
        End If

        If firstApply Then
            rootLayout.Dock = DockStyle.Fill
            rootLayout.AutoSize = False
            rootLayout.MinimumSize = New Size(0, 0)
            rightLayout.AutoSize = False
            rightLayout.MinimumSize = New Size(0, 0)
            rootLayout.PerformLayout()
            ResizeLeftContent(leftPanel)
        End If
    End Sub

    Public Sub RefreshLeftContent(ByVal leftPanel As Panel)
        If leftPanel Is Nothing OrElse leftContentPanel Is Nothing Then Return
        leftContentPanel.PerformLayout()
        ResizeLeftContent(leftPanel)
        leftPanel.PerformLayout()
    End Sub

    Private Sub ResizeLeftContent(ByVal leftPanel As Panel)
        If leftContentPanel Is Nothing Then Return

        '先按完整客户区排版；只有内容确实需要纵向滚动时，才为滚动条预留宽度。
        '这样正常生产分辨率不会永久浪费一条滚动条宽度，小分辨率也不会出现横向滚动。
        SetLeftContentWidth(leftPanel.ClientSize.Width)
        Dim contentHeight As Integer = CalculateLeftContentHeight()
        If contentHeight > leftPanel.ClientSize.Height Then
            SetLeftContentWidth(Math.Max(0, leftPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth))
            contentHeight = CalculateLeftContentHeight()
        End If
        leftContentPanel.Height = contentHeight
    End Sub

    Private Sub SetLeftContentWidth(ByVal width As Integer)
        leftContentPanel.Width = Math.Max(0, width)
        Dim childWidth As Integer = Math.Max(0, leftContentPanel.ClientSize.Width - leftContentPanel.Padding.Horizontal)
        For Each child As Control In leftContentPanel.Controls
            child.Width = childWidth
        Next
        leftContentPanel.PerformLayout()
    End Sub

    Private Function CalculateLeftContentHeight() As Integer
        Dim contentHeight As Integer = leftContentPanel.Padding.Vertical
        For Each child As Control In leftContentPanel.Controls
            contentHeight += child.Height + child.Margin.Vertical
        Next
        Return contentHeight
    End Function

    Private Sub PrepareScrollableLeftPanel(ByVal leftPanel As Panel)
        leftContentPanel = New TableLayoutPanel With {
            .Name = "LeftControlContent",
            .Location = New Point(0, 0),
            .AutoSize = False,
            .AutoSizeMode = AutoSizeMode.GrowOnly,
            .ColumnCount = 1,
            .RowCount = 0,
            .BackColor = leftPanel.BackColor,
            .Padding = New Padding(6),
            .Margin = New Padding(0),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        leftPanel.SuspendLayout()
        leftContentPanel.SuspendLayout()

        Try
            Dim existingChildren As New List(Of Control)()
            For Each child As Control In leftPanel.Controls
                existingChildren.Add(child)
            Next
            Dim orderedChildren As New List(Of Control)()
            AddIfPresent(existingChildren, orderedChildren, "grpTestMode")
            AddIfPresent(existingChildren, orderedChildren, "GroupBox2")
            AddIfPresent(existingChildren, orderedChildren, "GroupBox1")
            AddIfPresent(existingChildren, orderedChildren, "grpChannelSelection")
            AddIfPresent(existingChildren, orderedChildren, "GroupBox3")
            AddIfPresent(existingChildren, orderedChildren, "GroupBox5")
            AddIfPresent(existingChildren, orderedChildren, "GroupBox8")
            AddIfPresent(existingChildren, orderedChildren, "GroupBox7")
            AddIfPresent(existingChildren, orderedChildren, "Button2")

            For Each child As Control In orderedChildren
                Dim contentItem As Control = child
                If String.Equals(child.Name, "Button2", StringComparison.OrdinalIgnoreCase) Then
                    contentItem = CreateCenteredResetLayout(child)
                End If
                contentItem.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
                contentItem.Dock = DockStyle.Top
                contentItem.Margin = New Padding(0, 0, 0, UiTheme.LeftSectionGap)
                leftContentPanel.RowStyles.Add(New RowStyle(SizeType.AutoSize))
                leftContentPanel.Controls.Add(contentItem, 0, leftContentPanel.RowCount)
                leftContentPanel.RowCount += 1
            Next
            leftPanel.Controls.Add(leftContentPanel)
            leftContentPanel.BringToFront()
            BuildStructuredSectionLayouts()
        Finally
            leftContentPanel.ResumeLayout(False)
            leftPanel.ResumeLayout(False)
        End Try
    End Sub

    Private Shared Function CreateCenteredResetLayout(ByVal resetButton As Control) As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Name = "ResetAlarmLayout",
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .ColumnCount = 1,
            .RowCount = 1,
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .BackColor = resetButton.Parent.BackColor
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        resetButton.Dock = DockStyle.None
        resetButton.Anchor = AnchorStyles.None
        resetButton.Margin = New Padding(0)
        layout.Controls.Add(resetButton, 0, 0)
        return layout
    End Function

    Private Shared Sub AddIfPresent(ByVal source As List(Of Control),
                                    ByVal target As List(Of Control),
                                    ByVal controlName As String)
        Dim match As Control = source.FirstOrDefault(Function(control) String.Equals(control.Name, controlName, StringComparison.OrdinalIgnoreCase))
        If match IsNot Nothing Then target.Add(match)
    End Sub

    Private Sub BuildStructuredSectionLayouts()
        If leftContentPanel Is Nothing Then Return

        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "grpTestMode"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "GroupBox2"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "GroupBox1"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "grpChannelSelection"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "GroupBox3"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "GroupBox5"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "GroupBox8"))
        ConfigureGroupLayout(FindDirectChild(leftContentPanel, "GroupBox7"))

        BuildTestModeLayout(FindDirectChild(leftContentPanel, "grpTestMode"))
        BuildStationLayout(FindDirectChild(leftContentPanel, "GroupBox2"))
        BuildControlBoardLayout(FindDirectChild(leftContentPanel, "GroupBox1"))
        BuildChannelSelectionLayout(FindDirectChild(leftContentPanel, "grpChannelSelection"))
        BuildRangeLayout(FindDirectChild(leftContentPanel, "GroupBox3"))
        BuildAlarmLayout(FindDirectChild(leftContentPanel, "GroupBox5"))
        BuildProbeUsageLayout(FindDirectChild(leftContentPanel, "GroupBox8"))
        BuildEventLogLayout(FindDirectChild(leftContentPanel, "GroupBox7"))
    End Sub

    Private Shared Sub ConfigureGroupLayout(ByVal group As Control)
        If group Is Nothing Then Return

        group.Dock = DockStyle.Top
        group.AutoSize = True
        Dim groupBox As GroupBox = TryCast(group, GroupBox)
        If groupBox IsNot Nothing Then groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink
        group.MinimumSize = New Size(0, 0)
    End Sub

    Private Shared Function FindDirectChild(ByVal parent As Control, ByVal name As String) As Control
        If parent Is Nothing Then Return Nothing
        Return parent.Controls.Cast(Of Control)().FirstOrDefault(Function(control) String.Equals(control.Name, name, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function CreateTableLayout(ByVal parent As Control,
                                               ByVal name As String,
                                               ByVal columns As Integer,
                                               ByVal rows As Integer) As TableLayoutPanel
        Dim existing As TableLayoutPanel = TryCast(FindDirectChild(parent, name), TableLayoutPanel)
        If existing IsNot Nothing Then Return existing

        Dim layout As New TableLayoutPanel With {
            .Name = name,
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .ColumnCount = columns,
            .RowCount = rows,
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .BackColor = parent.BackColor
        }
        For index As Integer = 0 To columns - 1
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / columns))
        Next
        For index As Integer = 0 To rows - 1
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next
        parent.Controls.Add(layout)
        layout.BringToFront()
        Return layout
    End Function

    Private Shared Sub MoveControlToTable(ByVal parent As Control,
                                           ByVal layout As TableLayoutPanel,
                                           ByVal name As String,
                                           ByVal column As Integer,
                                           ByVal row As Integer,
                                           Optional ByVal columnSpan As Integer = 1)
        Dim control As Control = FindDirectChild(parent, name)
        If control Is Nothing Then Return

        parent.Controls.Remove(control)
        control.Margin = New Padding(2)
        layout.Controls.Add(control, column, row)
        If columnSpan > 1 Then layout.SetColumnSpan(control, columnSpan)
    End Sub

    Private Shared Sub ConfigureFillControl(ByVal control As Control)
        If control Is Nothing Then Return
        control.Dock = DockStyle.Fill
    End Sub

    Private Shared Sub ConfigureLabel(ByVal control As Control)
        If control Is Nothing Then Return
        ConfigureFillControl(control)
        Dim label As Label = TryCast(control, Label)
        If label IsNot Nothing Then label.TextAlign = ContentAlignment.MiddleRight
    End Sub

    Private Shared Sub ConfigureCenteredLabel(ByVal control As Control)
        If control Is Nothing Then Return
        ConfigureFillControl(control)
        Dim label As Label = TryCast(control, Label)
        If label IsNot Nothing Then label.TextAlign = ContentAlignment.MiddleCenter
    End Sub

    Private Shared Sub BuildTestModeLayout(ByVal group As Control)
        If group Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "TestModeLayout", 2, 1)
        MoveControlToTable(group, layout, "chkAutoTest", 0, 0)
        MoveControlToTable(group, layout, "chkManualTest", 1, 0)
        ConfigureFillControl(FindDirectChild(layout, "chkAutoTest"))
        ConfigureFillControl(FindDirectChild(layout, "chkManualTest"))
    End Sub

    Private Shared Sub BuildStationLayout(ByVal group As Control)
        If group Is Nothing Then Return
        For Each legacyName As String In {"cmbShow", "CmdBatInput"}
            Dim legacyControl As Control = FindDirectChild(group, legacyName)
            If legacyControl IsNot Nothing Then
                legacyControl.Visible = False
                legacyControl.Enabled = False
                legacyControl.TabStop = False
            End If
        Next
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "StationOptionsLayout", 3, 1)
        MoveControlToTable(group, layout, "Rbtn1", 0, 0)
        MoveControlToTable(group, layout, "Rbtn2", 1, 0)
        MoveControlToTable(group, layout, "Rbtn3", 2, 0)
        For Each name As String In {"Rbtn1", "Rbtn2", "Rbtn3"}
            ConfigureFillControl(FindDirectChild(layout, name))
        Next
    End Sub

    Private Shared Sub BuildControlBoardLayout(ByVal group As Control)
        If group Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "ControlBoardLayout", 3, 4)
        MoveControlToTable(group, layout, "Label2", 0, 0)
        MoveControlToTable(group, layout, "txtTray", 1, 0, 2)
        MoveControlToTable(group, layout, "Label5", 0, 1)
        MoveControlToTable(group, layout, "lblBarNum", 1, 1)
        MoveControlToTable(group, layout, "CmdStart", 0, 2)
        MoveControlToTable(group, layout, "btnStop", 1, 2)
        MoveControlToTable(group, layout, "btnNGTest", 2, 2)
        MoveControlToTable(group, layout, "btnTrayState", 0, 3)
        MoveControlToTable(group, layout, "btnOpen", 1, 3)
        MoveControlToTable(group, layout, "btnClose", 2, 3)

        For Each name As String In {"Label2", "Label5"}
            ConfigureLabel(FindDirectChild(layout, name))
        Next
        ConfigureFillControl(FindDirectChild(layout, "txtTray"))
        For Each name As String In {"CmdStart", "btnStop", "btnNGTest", "btnTrayState", "btnOpen", "btnClose"}
            ConfigureFillControl(FindDirectChild(layout, name))
        Next
    End Sub

    Private Shared Sub BuildChannelSelectionLayout(ByVal group As Control)
        If group Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "ChannelSelectionLayout", 4, 1)
        For index As Integer = 0 To 3
            Dim names() As String = {"btnChannelSelectAll", "btnChannelClear", "btnChannelRectangle", "btnChannelSelectSingle"}
            MoveControlToTable(group, layout, names(index), index, 0)
            ConfigureFillControl(FindDirectChild(layout, names(index)))
        Next
    End Sub

    Private Shared Sub BuildRangeLayout(ByVal group As Control)
        If group Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "RangeSettingsLayout", 3, 3)
        layout.ColumnStyles.Clear()
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        MoveControlToTable(group, layout, "Label9", 1, 0)
        MoveControlToTable(group, layout, "Label8", 2, 0)
        MoveControlToTable(group, layout, "Label7", 0, 1)
        MoveControlToTable(group, layout, "txtMinV", 1, 1)
        MoveControlToTable(group, layout, "txtMaxV", 2, 1)
        MoveControlToTable(group, layout, "Label12", 0, 2)
        MoveControlToTable(group, layout, "txtMinR", 1, 2)
        MoveControlToTable(group, layout, "txtMaxR", 2, 2)
        For Each name As String In {"Label7", "Label12"}
            ConfigureLabel(FindDirectChild(layout, name))
        Next
        ConfigureCenteredLabel(FindDirectChild(layout, "Label9"))
        ConfigureCenteredLabel(FindDirectChild(layout, "Label8"))
        For Each name As String In {"txtMinV", "txtMaxV", "txtMinR", "txtMaxR"}
            ConfigureFillControl(FindDirectChild(layout, name))
        Next
    End Sub

    Private Shared Sub BuildAlarmLayout(ByVal group As Control)
        If group Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "AlarmSectionLayout", 1, 2)
        MoveControlToTable(group, layout, "Panel2", 0, 0)
        MoveControlToTable(group, layout, "Panel3", 0, 1)
        Dim temperaturePanel As Control = FindDirectChild(layout, "Panel2")
        Dim statusPanel As Control = FindDirectChild(layout, "Panel3")
        Dim temperature As Panel = TryCast(temperaturePanel, Panel)
        If temperature IsNot Nothing Then
            temperature.BorderStyle = BorderStyle.None
            temperature.AutoSize = True
            temperature.AutoSizeMode = AutoSizeMode.GrowAndShrink
            temperature.MinimumSize = New Size(0, 0)
            temperature.Margin = New Padding(0)
            temperature.Padding = New Padding(0)
        End If
        Dim status As Panel = TryCast(statusPanel, Panel)
        If status IsNot Nothing Then
            status.BorderStyle = BorderStyle.None
            status.AutoSize = True
            status.AutoSizeMode = AutoSizeMode.GrowAndShrink
            status.MinimumSize = New Size(0, 0)
            status.Margin = New Padding(0, 4, 0, 0)
            status.Padding = New Padding(0)
        End If
        ConfigureFillControl(temperaturePanel)
        ConfigureFillControl(statusPanel)
        BuildTemperatureLayout(temperaturePanel)
        BuildAlarmStatusLayout(statusPanel)
    End Sub

    Private Shared Sub BuildTemperatureLayout(ByVal panel As Control)
        If panel Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(panel, "AlarmTemperatureLayout", 4, 2)
        MoveControlToTable(panel, layout, "Label1", 0, 0)
        MoveControlToTable(panel, layout, "t1", 1, 0)
        MoveControlToTable(panel, layout, "Label14", 2, 0)
        MoveControlToTable(panel, layout, "t2", 3, 0)
        MoveControlToTable(panel, layout, "Label15", 0, 1)
        MoveControlToTable(panel, layout, "t3", 1, 1)
        MoveControlToTable(panel, layout, "Label16", 2, 1)
        MoveControlToTable(panel, layout, "t4", 3, 1)
        For Each name As String In {"Label1", "Label14", "Label15", "Label16"}
            ConfigureLabel(FindDirectChild(layout, name))
        Next
        For Each name As String In {"t1", "t2", "t3", "t4"}
            ConfigureFillControl(FindDirectChild(layout, name))
        Next
    End Sub

    Private Shared Sub BuildAlarmStatusLayout(ByVal panel As Control)
        If panel Is Nothing Then Return

        '先保存原始控件引用。控件从 Panel3 移走后，不能再从 Panel3 递归查找，
        '否则烟感和设备状态会被创建出来却没有内容。
        Dim label19 As Control = FindDirectChild(panel, "Label19")
        Dim smoke1 As Control = FindDirectChild(panel, "smokeAlarm1")
        Dim label17 As Control = FindDirectChild(panel, "Label17")
        Dim smoke2 As Control = FindDirectChild(panel, "smokeAlarm2")
        Dim label28 As Control = FindDirectChild(panel, "Label28")
        Dim labelRun As Control = FindDirectChild(panel, "LabelRun")
        Dim labelAlarm As Control = FindDirectChild(panel, "LabelAlarm")

        Dim layout As TableLayoutPanel = CreateTableLayout(panel, "AlarmStatusLayout", 3, 1)
        layout.ColumnStyles.Clear()
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52.0F))
        For Each name As String In {"Label19", "smokeAlarm1", "Label17", "smokeAlarm2", "Label28", "LabelRun", "LabelAlarm"}
            Dim control As Control = FindDirectChild(panel, name)
            If control IsNot Nothing Then panel.Controls.Remove(control)
        Next
        Dim smokeOne As New FlowLayoutPanel With {.Name = "SmokeOneLayout", .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .WrapContents = False, .Margin = New Padding(2), .Padding = New Padding(0), .Dock = DockStyle.Fill}
        Dim smokeTwo As New FlowLayoutPanel With {.Name = "SmokeTwoLayout", .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .WrapContents = False, .Margin = New Padding(2), .Padding = New Padding(0), .Dock = DockStyle.Fill}
        Dim deviceStatus As New FlowLayoutPanel With {.Name = "DeviceStatusLayout", .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .WrapContents = False, .Margin = New Padding(2), .Padding = New Padding(0), .Dock = DockStyle.Fill}
        AddToFlow(smokeOne, label19)
        AddToFlow(smokeOne, smoke1)
        AddToFlow(smokeTwo, label17)
        AddToFlow(smokeTwo, smoke2)
        AddToFlow(deviceStatus, label28)
        AddToFlow(deviceStatus, labelRun)
        AddToFlow(deviceStatus, labelAlarm)
        layout.Controls.Add(smokeOne, 0, 0)
        layout.Controls.Add(smokeTwo, 1, 0)
        layout.Controls.Add(deviceStatus, 2, 0)
    End Sub

    Private Shared Sub AddToFlow(ByVal flow As FlowLayoutPanel, ByVal control As Control)
        If control Is Nothing Then Return
        control.Margin = New Padding(2)
        flow.Controls.Add(control)
    End Sub

    Private Shared Sub BuildProbeUsageLayout(ByVal group As Control)
        If group Is Nothing Then Return
        Dim layout As TableLayoutPanel = CreateTableLayout(group, "ProbeUsageLayout", 3, 1)
        layout.ColumnStyles.Clear()
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        MoveControlToTable(group, layout, "Label26", 0, 0)
        MoveControlToTable(group, layout, "txtRemain", 1, 0)
        MoveControlToTable(group, layout, "Label29", 2, 0)
        ConfigureLabel(FindDirectChild(layout, "Label26"))
        ConfigureFillControl(FindDirectChild(layout, "txtRemain"))
        Dim usageUnit As Label = TryCast(FindDirectChild(layout, "Label29"), Label)
        If usageUnit IsNot Nothing Then
            ConfigureFillControl(usageUnit)
            usageUnit.TextAlign = ContentAlignment.MiddleLeft
        End If
    End Sub

    Private Shared Sub BuildEventLogLayout(ByVal group As Control)
        If group Is Nothing Then Return
        group.AutoSize = False
        group.MinimumSize = New Size(0, 82)
        group.Height = 82
        Dim alarmText As Control = FindDirectChild(group, "txtAlarm")
        If alarmText IsNot Nothing Then
            alarmText.Dock = DockStyle.Fill
            alarmText.Margin = New Padding(0)
        End If
    End Sub

    Private Shared Function CreateRootLayout() As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .AutoSize = False,
            .MinimumSize = New Size(0, 0),
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = UiTheme.PageBackground,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, LeftColumnWidth))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        Return layout
    End Function

    Private Shared Function CreateRightLayout() As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .AutoSize = False,
            .MinimumSize = New Size(0, 0),
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = UiTheme.PageBackground,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 14.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 86.0F))
        Return layout
    End Function
End Class
