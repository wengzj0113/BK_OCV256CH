Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Owns the responsive layout containers for the test form.
''' It only reparents existing controls; it does not change test behavior.
''' </summary>
Public NotInheritable Class ResponsiveTestLayout
    Private rootLayout As TableLayoutPanel
    Private rightLayout As TableLayoutPanel
    Private leftContentPanel As Panel

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

        If rootLayout Is Nothing Then
            rootLayout = CreateRootLayout()
            rightLayout = CreateRightLayout()

            host.SuspendLayout()
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
                leftPanel.AutoScroll = True
                leftPanel.HorizontalScroll.Enabled = True
                leftPanel.VerticalScroll.Enabled = True
                PrepareScrollableLeftPanel(leftPanel)

                statistics.Dock = DockStyle.Fill
                statistics.Margin = New Padding(0)

                channelGrid.Dock = DockStyle.Fill
                channelGrid.Margin = New Padding(0)

                rightLayout.Controls.Add(statistics, 0, 0)
                rightLayout.Controls.Add(channelGrid, 0, 1)
                rootLayout.Controls.Add(leftPanel, 0, 0)
                rootLayout.Controls.Add(rightLayout, 1, 0)
                host.Controls.Add(rootLayout)
                rootLayout.BringToFront()
            Finally
                host.ResumeLayout(True)
            End Try
        End If

        rootLayout.Dock = DockStyle.Fill
        rootLayout.PerformLayout()
        ResizeLeftContent(leftPanel)
        '打开测试页时左侧滚动区必须从顶部开始,否则首屏会从中段(如启动按钮)开始
        leftPanel.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub ResizeLeftContent(ByVal leftPanel As Panel)
        If leftContentPanel Is Nothing Then Return

        Dim contentWidth As Integer = 0
        For Each child As Control In leftContentPanel.Controls
            contentWidth = Math.Max(contentWidth, child.Right)
        Next

        '让左侧内容至少铺满分栏宽度，同时保留内容自身所需宽度，避免出现右侧大块空白。
        Dim availableWidth As Integer = Math.Max(0, leftPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth)
        leftContentPanel.Width = Math.Max(contentWidth, availableWidth)
        leftPanel.AutoScrollMinSize = New Size(leftContentPanel.Width, leftContentPanel.Height)
    End Sub

    Private Sub PrepareScrollableLeftPanel(ByVal leftPanel As Panel)
        leftContentPanel = New Panel With {
            .Name = "LeftControlContent",
            .Location = New Point(0, 0),
            .AutoScroll = False,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }

        Dim contentWidth As Integer = 0
        Dim contentHeight As Integer = 0
        Dim existingChildren As New List(Of Control)()
        For Each child As Control In leftPanel.Controls
            existingChildren.Add(child)
        Next
        For Each child As Control In existingChildren
            contentWidth = Math.Max(contentWidth, child.Right)
            contentHeight = Math.Max(contentHeight, child.Bottom)
            child.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            leftContentPanel.Controls.Add(child)
        Next
        leftContentPanel.Size = New Size(contentWidth + leftPanel.Padding.Horizontal,
                                         contentHeight + leftPanel.Padding.Vertical)
        leftPanel.Controls.Add(leftContentPanel)
        leftPanel.AutoScrollMinSize = leftContentPanel.Size
    End Sub

    Private Shared Function CreateRootLayout() As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = UiTheme.PageBackground,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 330.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        Return layout
    End Function

    Private Shared Function CreateRightLayout() As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
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
