<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSetCondition
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSetCondition))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.cmboGrade = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtcondition = New System.Windows.Forms.TextBox()
        Me.btnDel = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.CmboCondition = New System.Windows.Forms.ComboBox()
        Me.DGVCondition = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lstcondition = New System.Windows.Forms.CheckedListBox()
        Me.lstNosee = New System.Windows.Forms.CheckedListBox()
        Me.GroupBox1.SuspendLayout()
        CType(Me.DGVCondition, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.GroupBox1.Controls.Add(Me.cmboGrade)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.txtcondition)
        Me.GroupBox1.Controls.Add(Me.btnDel)
        Me.GroupBox1.Controls.Add(Me.btnSave)
        Me.GroupBox1.Controls.Add(Me.btnAdd)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.CmboCondition)
        Me.GroupBox1.Location = New System.Drawing.Point(3, 0)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(933, 72)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        '
        'cmboGrade
        '
        Me.cmboGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmboGrade.FormattingEnabled = True
        Me.cmboGrade.Location = New System.Drawing.Point(86, 45)
        Me.cmboGrade.Name = "cmboGrade"
        Me.cmboGrade.Size = New System.Drawing.Size(208, 20)
        Me.cmboGrade.TabIndex = 10
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(6, 45)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 12)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "有效等级"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(300, 21)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(101, 12)
        Me.Label2.TabIndex = 8
        Me.Label2.Text = "当前的条件名称："
        '
        'txtcondition
        '
        Me.txtcondition.Location = New System.Drawing.Point(407, 16)
        Me.txtcondition.Name = "txtcondition"
        Me.txtcondition.ReadOnly = True
        Me.txtcondition.Size = New System.Drawing.Size(155, 21)
        Me.txtcondition.TabIndex = 7
        '
        'btnDel
        '
        Me.btnDel.Location = New System.Drawing.Point(805, 12)
        Me.btnDel.Name = "btnDel"
        Me.btnDel.Size = New System.Drawing.Size(98, 30)
        Me.btnDel.TabIndex = 6
        Me.btnDel.Text = "删除当前条件"
        Me.btnDel.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(714, 12)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(72, 30)
        Me.btnSave.TabIndex = 5
        Me.btnSave.Text = "保存"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(601, 12)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(77, 30)
        Me.btnAdd.TabIndex = 4
        Me.btnAdd.Text = "新增"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(4, 17)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(77, 12)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "选择分档条件"
        '
        'CmboCondition
        '
        Me.CmboCondition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CmboCondition.FormattingEnabled = True
        Me.CmboCondition.Location = New System.Drawing.Point(86, 17)
        Me.CmboCondition.Name = "CmboCondition"
        Me.CmboCondition.Size = New System.Drawing.Size(208, 20)
        Me.CmboCondition.TabIndex = 2
        '
        'DGVCondition
        '
        Me.DGVCondition.AllowUserToAddRows = False
        Me.DGVCondition.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader
        Me.DGVCondition.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGVCondition.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2})
        Me.DGVCondition.Location = New System.Drawing.Point(231, 81)
        Me.DGVCondition.Name = "DGVCondition"
        Me.DGVCondition.RowTemplate.Height = 23
        Me.DGVCondition.Size = New System.Drawing.Size(705, 425)
        Me.DGVCondition.TabIndex = 3
        '
        'Column1
        '
        Me.Column1.HeaderText = "序号"
        Me.Column1.Name = "Column1"
        Me.Column1.Width = 54
        '
        'Column2
        '
        Me.Column2.HeaderText = "等级"
        Me.Column2.Name = "Column2"
        Me.Column2.Width = 54
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(485, 512)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(72, 30)
        Me.btnClose.TabIndex = 6
        Me.btnClose.Text = "关闭"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'lstcondition
        '
        Me.lstcondition.FormattingEnabled = True
        Me.lstcondition.Location = New System.Drawing.Point(3, 80)
        Me.lstcondition.Name = "lstcondition"
        Me.lstcondition.Size = New System.Drawing.Size(222, 420)
        Me.lstcondition.TabIndex = 7
        '
        'lstNosee
        '
        Me.lstNosee.FormattingEnabled = True
        Me.lstNosee.Location = New System.Drawing.Point(3, 81)
        Me.lstNosee.Name = "lstNosee"
        Me.lstNosee.Size = New System.Drawing.Size(222, 420)
        Me.lstNosee.TabIndex = 8
        Me.lstNosee.Visible = False
        '
        'frmSetCondition
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Silver
        Me.ClientSize = New System.Drawing.Size(940, 550)
        Me.Controls.Add(Me.lstcondition)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.DGVCondition)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.lstNosee)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "frmSetCondition"
        Me.Text = "分档条件"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.DGVCondition, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnAdd As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CmboCondition As System.Windows.Forms.ComboBox
    Friend WithEvents DGVCondition As System.Windows.Forms.DataGridView
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents btnDel As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtcondition As System.Windows.Forms.TextBox
    Friend WithEvents cmboGrade As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lstcondition As System.Windows.Forms.CheckedListBox
    Friend WithEvents lstNosee As System.Windows.Forms.CheckedListBox
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
