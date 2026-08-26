<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmLogQuery
    Inherits Form

    Private components As System.ComponentModel.IContainer
    Friend WithEvents topPanel As Panel
    Friend WithEvents lblFilter As Label
    Friend WithEvents txtFilter As TextBox
    Friend WithEvents btnRefresh As Button
    Friend WithEvents lblDirectory As Label
    Friend WithEvents split As SplitContainer
    Friend WithEvents lstFiles As ListBox
    Friend WithEvents txtContent As TextBox

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then components.Dispose()
        MyBase.Dispose(disposing)
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.topPanel = New Panel()
        Me.lblFilter = New Label()
        Me.txtFilter = New TextBox()
        Me.btnRefresh = New Button()
        Me.lblDirectory = New Label()
        Me.split = New SplitContainer()
        Me.lstFiles = New ListBox()
        Me.txtContent = New TextBox()
        Me.topPanel.SuspendLayout()
        CType(Me.split, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.split.Panel1.SuspendLayout()
        Me.split.Panel2.SuspendLayout()
        Me.split.SuspendLayout()
        Me.SuspendLayout()

        Me.Text = "日志查询"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(1100, 700)
        Me.MinimumSize = New Size(800, 500)

        Me.topPanel.Dock = DockStyle.Top
        Me.topPanel.Height = 66
        Me.topPanel.Padding = New Padding(10, 8, 10, 8)
        Me.lblFilter.AutoSize = True
        Me.lblFilter.Location = New Point(10, 14)
        Me.lblFilter.Text = "文件筛选："
        Me.txtFilter.Location = New Point(82, 10)
        Me.txtFilter.Size = New Size(220, 23)
        Me.btnRefresh.Location = New Point(315, 9)
        Me.btnRefresh.Size = New Size(80, 26)
        Me.btnRefresh.Text = "刷新"
        Me.btnRefresh.UseVisualStyleBackColor = True
        Me.lblDirectory.AutoEllipsis = True
        Me.lblDirectory.AutoSize = False
        Me.lblDirectory.Location = New Point(410, 13)
        Me.lblDirectory.Size = New Size(650, 24)
        Me.lblDirectory.TextAlign = ContentAlignment.MiddleLeft
        Me.topPanel.Controls.Add(Me.lblFilter)
        Me.topPanel.Controls.Add(Me.txtFilter)
        Me.topPanel.Controls.Add(Me.btnRefresh)
        Me.topPanel.Controls.Add(Me.lblDirectory)

        Me.split.Dock = DockStyle.Fill
        Me.split.SplitterDistance = 50
        Me.split.Panel1MinSize = 0
        Me.split.Panel2MinSize = 0
        Me.split.SplitterWidth = 6
        Me.lstFiles.Dock = DockStyle.Fill
        Me.lstFiles.IntegralHeight = False
        Me.txtContent.Dock = DockStyle.Fill
        Me.txtContent.Multiline = True
        Me.txtContent.ReadOnly = True
        Me.txtContent.ScrollBars = ScrollBars.Both
        Me.txtContent.WordWrap = False
        Me.split.Panel1.Controls.Add(Me.lstFiles)
        Me.split.Panel2.Controls.Add(Me.txtContent)

        Me.Controls.Add(Me.split)
        Me.Controls.Add(Me.topPanel)
        Me.topPanel.ResumeLayout(False)
        CType(Me.split, System.ComponentModel.ISupportInitialize).EndInit()
        Me.split.Panel1.ResumeLayout(False)
        Me.split.Panel2.ResumeLayout(False)
        Me.split.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub
End Class
