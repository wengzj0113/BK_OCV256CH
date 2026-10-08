<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmInquery
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmInquery))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.DateTimePicker1 = New System.Windows.Forms.DateTimePicker()
        Me.chkDebugResults = New System.Windows.Forms.CheckBox()
        Me.btnInquery = New System.Windows.Forms.Button()
        Me.lstTray = New System.Windows.Forms.ListBox()
        Me.txtTray = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.cmboItem = New System.Windows.Forms.ComboBox()
        Me.chkocv4 = New System.Windows.Forms.CheckBox()
        Me.chkocv3 = New System.Windows.Forms.CheckBox()
        Me.chkocv2 = New System.Windows.Forms.CheckBox()
        Me.chkocv1 = New System.Windows.Forms.CheckBox()
        Me.DGVbar = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column11 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column9 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column13 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.lblBarnumber = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmboCondition = New System.Windows.Forms.ComboBox()
        Me.dgv_grade = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column15 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.dgv_Condition = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnSetCon = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.DGVbar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox5.SuspendLayout()
        CType(Me.dgv_grade, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgv_Condition, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.GroupBox1.Controls.Add(Me.DateTimePicker1)
        Me.GroupBox1.Controls.Add(Me.chkDebugResults)
        Me.GroupBox1.Controls.Add(Me.btnInquery)
        Me.GroupBox1.Controls.Add(Me.lstTray)
        Me.GroupBox1.Controls.Add(Me.txtTray)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(2, 2)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(277, 531)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "托盘编码"
        '
        'DateTimePicker1
        '
        Me.DateTimePicker1.Location = New System.Drawing.Point(57, 50)
        Me.DateTimePicker1.Name = "DateTimePicker1"
        Me.DateTimePicker1.Size = New System.Drawing.Size(136, 21)
        Me.DateTimePicker1.TabIndex = 5
        '
        'chkDebugResults
        '
        Me.chkDebugResults.AutoSize = True
        Me.chkDebugResults.Location = New System.Drawing.Point(198, 53)
        Me.chkDebugResults.Name = "chkDebugResults"
        Me.chkDebugResults.Size = New System.Drawing.Size(72, 16)
        Me.chkDebugResults.TabIndex = 6
        Me.chkDebugResults.Text = "调试结果"
        Me.chkDebugResults.UseVisualStyleBackColor = True
        '
        'btnInquery
        '
        Me.btnInquery.Location = New System.Drawing.Point(220, 20)
        Me.btnInquery.Name = "btnInquery"
        Me.btnInquery.Size = New System.Drawing.Size(51, 24)
        Me.btnInquery.TabIndex = 3
        Me.btnInquery.Text = "查询"
        Me.btnInquery.UseVisualStyleBackColor = True
        '
        'lstTray
        '
        Me.lstTray.FormattingEnabled = True
        Me.lstTray.ItemHeight = 12
        Me.lstTray.Location = New System.Drawing.Point(8, 80)
        Me.lstTray.Name = "lstTray"
        Me.lstTray.Size = New System.Drawing.Size(263, 448)
        Me.lstTray.TabIndex = 4
        '
        'txtTray
        '
        Me.txtTray.Location = New System.Drawing.Point(55, 23)
        Me.txtTray.Name = "txtTray"
        Me.txtTray.Size = New System.Drawing.Size(164, 21)
        Me.txtTray.TabIndex = 1
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(5, 54)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(29, 12)
        Me.Label4.TabIndex = 0
        Me.Label4.Text = "日期"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(1, 26)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(47, 12)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "托盘NO."
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.cmboItem)
        Me.GroupBox3.Controls.Add(Me.chkocv4)
        Me.GroupBox3.Controls.Add(Me.chkocv3)
        Me.GroupBox3.Controls.Add(Me.chkocv2)
        Me.GroupBox3.Controls.Add(Me.chkocv1)
        Me.GroupBox3.Location = New System.Drawing.Point(2, 2)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(277, 100)
        Me.GroupBox3.TabIndex = 7
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "显示项目"
        Me.GroupBox3.Visible = False
        '
        'cmboItem
        '
        Me.cmboItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmboItem.FormattingEnabled = True
        Me.cmboItem.Location = New System.Drawing.Point(143, 20)
        Me.cmboItem.Name = "cmboItem"
        Me.cmboItem.Size = New System.Drawing.Size(122, 20)
        Me.cmboItem.TabIndex = 9
        Me.cmboItem.Visible = False
        '
        'chkocv4
        '
        Me.chkocv4.AutoSize = True
        Me.chkocv4.Location = New System.Drawing.Point(10, 80)
        Me.chkocv4.Name = "chkocv4"
        Me.chkocv4.Size = New System.Drawing.Size(108, 16)
        Me.chkocv4.TabIndex = 8
        Me.chkocv4.Text = "ocV4/ocR4/oCT4"
        Me.chkocv4.UseVisualStyleBackColor = True
        '
        'chkocv3
        '
        Me.chkocv3.AutoSize = True
        Me.chkocv3.Location = New System.Drawing.Point(10, 60)
        Me.chkocv3.Name = "chkocv3"
        Me.chkocv3.Size = New System.Drawing.Size(108, 16)
        Me.chkocv3.TabIndex = 7
        Me.chkocv3.Text = "ocV3/ocR3/oCT3"
        Me.chkocv3.UseVisualStyleBackColor = True
        '
        'chkocv2
        '
        Me.chkocv2.AutoSize = True
        Me.chkocv2.Location = New System.Drawing.Point(10, 40)
        Me.chkocv2.Name = "chkocv2"
        Me.chkocv2.Size = New System.Drawing.Size(108, 16)
        Me.chkocv2.TabIndex = 6
        Me.chkocv2.Text = "ocV2/ocR2/oCT2"
        Me.chkocv2.UseVisualStyleBackColor = True
        '
        'chkocv1
        '
        Me.chkocv1.AutoSize = True
        Me.chkocv1.Location = New System.Drawing.Point(10, 20)
        Me.chkocv1.Name = "chkocv1"
        Me.chkocv1.Size = New System.Drawing.Size(108, 16)
        Me.chkocv1.TabIndex = 5
        Me.chkocv1.Text = "ocV1/ocR1/oCT1"
        Me.chkocv1.UseVisualStyleBackColor = True
        '
        'DGVbar
        '
        Me.DGVbar.AllowUserToAddRows = False
        Me.DGVbar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGVbar.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column6, Me.Column10, Me.Column3, Me.Column7, Me.Column11, Me.Column4, Me.Column8, Me.Column12, Me.Column5, Me.Column9, Me.Column13})
        Me.DGVbar.Location = New System.Drawing.Point(283, 42)
        Me.DGVbar.Name = "DGVbar"
        Me.DGVbar.RowHeadersWidth = 51
        Me.DGVbar.RowTemplate.Height = 23
        Me.DGVbar.Size = New System.Drawing.Size(1132, 488)
        Me.DGVbar.TabIndex = 32
        '
        'Column1
        '
        Me.Column1.HeaderText = "电池编码"
        Me.Column1.MinimumWidth = 6
        Me.Column1.Name = "Column1"
        Me.Column1.Width = 125
        '
        'Column2
        '
        Me.Column2.HeaderText = "OCV1(mV)"
        Me.Column2.MinimumWidth = 6
        Me.Column2.Name = "Column2"
        Me.Column2.Width = 125
        '
        'Column6
        '
        Me.Column6.HeaderText = "OCR1(mΩ）"
        Me.Column6.MinimumWidth = 6
        Me.Column6.Name = "Column6"
        Me.Column6.Width = 125
        '
        'Column10
        '
        Me.Column10.HeaderText = "OCT1"
        Me.Column10.MinimumWidth = 6
        Me.Column10.Name = "Column10"
        Me.Column10.Width = 125
        '
        'Column3
        '
        Me.Column3.HeaderText = "OCV2(mV)"
        Me.Column3.MinimumWidth = 6
        Me.Column3.Name = "Column3"
        Me.Column3.Width = 125
        '
        'Column7
        '
        Me.Column7.HeaderText = "OCR2(mΩ）"
        Me.Column7.MinimumWidth = 6
        Me.Column7.Name = "Column7"
        Me.Column7.Width = 125
        '
        'Column11
        '
        Me.Column11.HeaderText = "OCT2"
        Me.Column11.MinimumWidth = 6
        Me.Column11.Name = "Column11"
        Me.Column11.Width = 125
        '
        'Column4
        '
        Me.Column4.HeaderText = "OCV3(mV)"
        Me.Column4.MinimumWidth = 6
        Me.Column4.Name = "Column4"
        Me.Column4.Width = 125
        '
        'Column8
        '
        Me.Column8.HeaderText = "OCR3(mΩ）"
        Me.Column8.MinimumWidth = 6
        Me.Column8.Name = "Column8"
        Me.Column8.Width = 125
        '
        'Column12
        '
        Me.Column12.HeaderText = "OCT3"
        Me.Column12.MinimumWidth = 6
        Me.Column12.Name = "Column12"
        Me.Column12.Width = 125
        '
        'Column5
        '
        Me.Column5.HeaderText = "OCV4(mV)"
        Me.Column5.MinimumWidth = 6
        Me.Column5.Name = "Column5"
        Me.Column5.Width = 125
        '
        'Column9
        '
        Me.Column9.HeaderText = "OCR4(mΩ）"
        Me.Column9.MinimumWidth = 6
        Me.Column9.Name = "Column9"
        Me.Column9.Width = 125
        '
        'Column13
        '
        Me.Column13.HeaderText = "OCT4"
        Me.Column13.MinimumWidth = 6
        Me.Column13.Name = "Column13"
        Me.Column13.Width = 125
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(417, 15)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 12)
        Me.Label3.TabIndex = 31
        Me.Label3.Text = "选择条件"
        Me.Label3.Visible = False
        '
        'GroupBox5
        '
        Me.GroupBox5.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.GroupBox5.Controls.Add(Me.lblBarnumber)
        Me.GroupBox5.Controls.Add(Me.Label2)
        Me.GroupBox5.Location = New System.Drawing.Point(759, 6)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(320, 31)
        Me.GroupBox5.TabIndex = 30
        Me.GroupBox5.TabStop = False
        '
        'lblBarnumber
        '
        Me.lblBarnumber.AutoSize = True
        Me.lblBarnumber.Location = New System.Drawing.Point(145, 15)
        Me.lblBarnumber.Name = "lblBarnumber"
        Me.lblBarnumber.Size = New System.Drawing.Size(23, 12)
        Me.lblBarnumber.TabIndex = 26
        Me.lblBarnumber.Text = "***"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(33, 14)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 12)
        Me.Label2.TabIndex = 25
        Me.Label2.Text = "电池总数"
        '
        'cmboCondition
        '
        Me.cmboCondition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmboCondition.FormattingEnabled = True
        Me.cmboCondition.Location = New System.Drawing.Point(494, 11)
        Me.cmboCondition.Name = "cmboCondition"
        Me.cmboCondition.Size = New System.Drawing.Size(188, 20)
        Me.cmboCondition.TabIndex = 29
        Me.cmboCondition.Visible = False
        '
        'dgv_grade
        '
        Me.dgv_grade.AllowUserToAddRows = False
        Me.dgv_grade.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader
        Me.dgv_grade.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgv_grade.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2, Me.Column14, Me.Column15})
        Me.dgv_grade.Location = New System.Drawing.Point(285, 42)
        Me.dgv_grade.Name = "dgv_grade"
        Me.dgv_grade.RowHeadersWidth = 51
        Me.dgv_grade.RowTemplate.Height = 23
        Me.dgv_grade.Size = New System.Drawing.Size(675, 139)
        Me.dgv_grade.TabIndex = 28
        Me.dgv_grade.Visible = False
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.HeaderText = "序号"
        Me.DataGridViewTextBoxColumn1.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.Width = 54
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.HeaderText = "等级名称"
        Me.DataGridViewTextBoxColumn2.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.Width = 78
        '
        'Column14
        '
        Me.Column14.HeaderText = "电池个数"
        Me.Column14.MinimumWidth = 6
        Me.Column14.Name = "Column14"
        Me.Column14.Width = 78
        '
        'Column15
        '
        Me.Column15.HeaderText = "合格率"
        Me.Column15.MinimumWidth = 6
        Me.Column15.Name = "Column15"
        Me.Column15.Width = 66
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(738, 536)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(107, 26)
        Me.btnClose.TabIndex = 34
        Me.btnClose.Text = "关闭"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'dgv_Condition
        '
        Me.dgv_Condition.AllowUserToAddRows = False
        Me.dgv_Condition.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader
        Me.dgv_Condition.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgv_Condition.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn3, Me.DataGridViewTextBoxColumn4})
        Me.dgv_Condition.Location = New System.Drawing.Point(283, 42)
        Me.dgv_Condition.Name = "dgv_Condition"
        Me.dgv_Condition.RowHeadersWidth = 51
        Me.dgv_Condition.RowTemplate.Height = 23
        Me.dgv_Condition.Size = New System.Drawing.Size(349, 139)
        Me.dgv_Condition.TabIndex = 35
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.HeaderText = "序号"
        Me.DataGridViewTextBoxColumn3.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.Width = 54
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.HeaderText = "等级名称"
        Me.DataGridViewTextBoxColumn4.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.Width = 78
        '
        'btnExport
        '
        Me.btnExport.Location = New System.Drawing.Point(1246, 8)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(160, 26)
        Me.btnExport.TabIndex = 36
        Me.btnExport.Text = "托盘电池信息导出Excel"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnSetCon
        '
        Me.btnSetCon.Location = New System.Drawing.Point(289, 8)
        Me.btnSetCon.Name = "btnSetCon"
        Me.btnSetCon.Size = New System.Drawing.Size(101, 26)
        Me.btnSetCon.TabIndex = 37
        Me.btnSetCon.Text = "设置分选条件"
        Me.btnSetCon.UseVisualStyleBackColor = True
        Me.btnSetCon.Visible = False
        '
        'frmInquery
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1427, 568)
        Me.Controls.Add(Me.DGVbar)
        Me.Controls.Add(Me.dgv_grade)
        Me.Controls.Add(Me.btnSetCon)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.dgv_Condition)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox5)
        Me.Controls.Add(Me.cmboCondition)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmInquery"
        Me.Text = "查询统计"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.DGVbar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        CType(Me.dgv_grade, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgv_Condition, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lstTray As System.Windows.Forms.ListBox
    Friend WithEvents txtTray As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents cmboItem As System.Windows.Forms.ComboBox
    Friend WithEvents chkocv4 As System.Windows.Forms.CheckBox
    Friend WithEvents chkocv3 As System.Windows.Forms.CheckBox
    Friend WithEvents chkocv2 As System.Windows.Forms.CheckBox
    Friend WithEvents chkocv1 As System.Windows.Forms.CheckBox
    Friend WithEvents btnInquery As System.Windows.Forms.Button
    Friend WithEvents DGVbar As System.Windows.Forms.DataGridView
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents lblBarnumber As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents cmboCondition As System.Windows.Forms.ComboBox
    Friend WithEvents dgv_grade As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column14 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column15 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents dgv_Condition As System.Windows.Forms.DataGridView
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnSetCon As System.Windows.Forms.Button
    Friend WithEvents Column1 As DataGridViewTextBoxColumn
    Friend WithEvents Column2 As DataGridViewTextBoxColumn
    Friend WithEvents Column6 As DataGridViewTextBoxColumn
    Friend WithEvents Column10 As DataGridViewTextBoxColumn
    Friend WithEvents Column3 As DataGridViewTextBoxColumn
    Friend WithEvents Column7 As DataGridViewTextBoxColumn
    Friend WithEvents Column11 As DataGridViewTextBoxColumn
    Friend WithEvents Column4 As DataGridViewTextBoxColumn
    Friend WithEvents Column8 As DataGridViewTextBoxColumn
    Friend WithEvents Column12 As DataGridViewTextBoxColumn
    Friend WithEvents Column5 As DataGridViewTextBoxColumn
    Friend WithEvents Column9 As DataGridViewTextBoxColumn
    Friend WithEvents Column13 As DataGridViewTextBoxColumn
    Friend WithEvents DateTimePicker1 As DateTimePicker
    Friend WithEvents chkDebugResults As CheckBox
    Friend WithEvents Label4 As Label
End Class
