Imports System.Data
Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.Math
Public Class frmInquery
    Private ObjOldWidth As Long  '保存窗体的原始宽度
    Private ObjOldHeight As Long '保存窗体的原始高度
    Private ObjOldFont As Single '保存窗体的原始字体比
    Private Shared objfrmCondition As frmSetCondition
    ' 其他代码
    Private WithEvents timer1 As New Timer()
    Private WithEvents chkHeartbeat As CheckBox
    Private WithEvents chkStateAndStopReason As CheckBox
    Private WithEvents chkAlarmUpload As CheckBox
    Private WithEvents chkEnergyUpload As CheckBox
    Private WithEvents chkResultDataUploadAssembly As CheckBox
    Private WithEvents chkWIPInStationCheck As CheckBox





    Public Sub ResizeInit(ByVal Form1 As Form)
        Dim Obj As Control
        ObjOldWidth = Form1.Width
        ObjOldHeight = Form1.Height
        ObjOldFont = Form1.Font.Size / ObjOldHeight
        On Error Resume Next
        For Each Obj In Form1.Controls
            If Obj.Tag = "" Then
                Obj.Tag = "000"
            End If
            Obj.Tag = Obj.Tag & Obj.Left & " " & Obj.Top & " " & Obj.Width & " " & Obj.Height & " "
        Next Obj
        On Error GoTo 0
    End Sub

    '按比例改变表单内各元件的大小，
    '在调用ReSizeForm前先调用ReSizeInit函数
    Public Sub ResizeForm(ByVal Form1 As Form)
        Dim Pos(4) As Double
        Dim i As Long, TempPos As Long, StartPos As Long
        Dim Obj As Control
        Dim ScaleX As Double, ScaleY As Double

        ScaleX = Form1.Width / ObjOldWidth
        '保存窗体宽度缩放比例
        ScaleY = Form1.Height / ObjOldHeight
        '保存窗体高度缩放比例
        On Error Resume Next

        For Each Obj In Me.Controls
            StartPos = 4
            For i = 0 To 4
                '读取控件的原始位置与大小
                TempPos = InStr(StartPos, Obj.Tag, " ", vbTextCompare)
                If TempPos > 0 Then
                    Pos(i) = Mid(Obj.Tag, StartPos, TempPos - StartPos)
                    StartPos = TempPos + 1
                Else
                    Pos(i) = 0
                End If
                '根据控件的原始位置及窗体改变大
                '小的比例对控件重新定位与改变大小
                SetObjPosition(Obj, Pos(0) * ScaleX, Pos(1) * ScaleY, Pos(2) * ScaleX, Pos(3) * ScaleY)
                Obj.Font = New Font("宋体", ObjOldFont * Me.Height)
            Next i
        Next Obj

        On Error GoTo 0
    End Sub
    Private Sub SetObjPosition(ByVal Obj As Control, ByVal leftX As Integer, ByVal topY As Integer, ByVal widthX As Integer, ByVal heightY As Integer)
        Obj.Left = leftX
        Obj.Top = topY
        Obj.Width = widthX
        Obj.Height = heightY
    End Sub
    Private Sub frmInquery_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim I As Integer

        '***********************************
        '改变控件的位置和大小
        ResizeInit(Me)
        '************************************
        '初始化分选条件 (改用 OcvDataService 新库)
        Try
            Dim schemeService As New OcvDataService()
            Dim schemeTable As DataTable = schemeService.GetGradeSchemes()
            For Each schemeRow As DataRow In schemeTable.Rows
                cmboCondition.Items.Add(Convert.ToString(schemeRow("SchemeName")))
            Next
        Catch ex As Exception
            'OcvDataService 调用失败时优雅降级，不崩溃
        End Try
        If cmboCondition.Items.Count > 0 Then
            cmboCondition.SelectedIndex = 0
        End If
        '************************************
        'init grade (改用 OcvDataService 新库)
        I = 0
        dgv_grade.RowCount = 0
        Try
            Dim gradeService As New OcvDataService()
            Dim gradeTable As DataTable = gradeService.GetGradeCodes(Trim(cmboCondition.Text))
            For Each gradeRow As DataRow In gradeTable.Rows
                I = I + 1
                dgv_grade.Rows.Add(1)
                dgv_grade.Item(0, dgv_grade.RowCount - 1).Value = I
                dgv_grade.Item(1, dgv_grade.RowCount - 1).Value = Convert.ToString(gradeRow("GradeCode"))
            Next
        Catch ex As Exception
            'OcvDataService 调用失败时优雅降级，不崩溃
        End Try
        '填写条件到表格
        freshCondition(Trim(cmboCondition.Text))
        'init Tray NO.
        LoadTrayListFromOcvData(False)
        Call GetExpression(cmboCondition.Text)

    End Sub

    Private Sub frmInquery_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        ResizeForm(Me)
        With lstTray
            .Height = GroupBox1.Height - .Top
            .Width = GroupBox1.Width * 0.95
        End With
    End Sub

    Private Sub txtTray_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtTray.KeyDown
        'R2-15:旧 clsDB 查询已移除,回车刷新由 lstTray_SelectedIndexChanged 处理
    End Sub
    Private Sub lstTray_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstTray.SelectedIndexChanged
        Dim strTray As String
        strTray = lstTray.Text
        Call freshTray(strTray)
    End Sub
    Private Sub cmboCondition_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmboCondition.TextChanged
        Dim strLvName As String
        Dim I As Integer

        '***************************
        '获取条件名称
        strLvName = Trim(cmboCondition.Text)
        '***************************
        '填写条件的等级 (改用 OcvDataService 新库)
        I = 0
        dgv_grade.RowCount = 0
        Try
            Dim gradeService As New OcvDataService()
            Dim gradeTable As DataTable = gradeService.GetGradeCodes(strLvName)
            For Each gradeRow As DataRow In gradeTable.Rows
                I = I + 1
                dgv_grade.Rows.Add(1)
                dgv_grade.Item(0, dgv_grade.RowCount - 1).Value = I
                dgv_grade.Item(1, dgv_grade.RowCount - 1).Value = Convert.ToString(gradeRow("GradeCode"))
            Next
        Catch ex As Exception
            'OcvDataService 调用失败时优雅降级，不崩溃
        End Try
        '***************************
        '取等级判断表达式
        GetExpression(strLvName)
        '***************************
        '刷新条件
        strLvName = Trim(cmboCondition.Text)
        Call freshCondition(strLvName)

    End Sub

    Private Sub btnInquery_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInquery.Click
        Dim strTray As String

        Try
            Me.Cursor = Cursors.WaitCursor
            strTray = Trim(txtTray.Text)
            If strTray = "" Then
                MsgBox("请输入有效的托盘编码！", vbOKOnly, "提示")
            End If
            '**************************************
            '查询托盘内电池数据
            Call freshTray(strTray)
            '*************************************
        Catch
        Finally
            lstTray.ClearSelected()
            Me.Cursor = Cursors.Default
        End Try
    End Sub
    Public Sub freshTray(ByVal strTray As String)

        If currentOCVNum <> 1 AndAlso currentOCVNum <> 2 Then
            MsgBox("当前OCV类型无效，无法查询托盘数据。", vbOKOnly, "提示")
            Exit Sub
        End If

        'R2-15:完全走 OCV_DATA 新库,删除旧 CW_tblData_Info 兜底路径
        If FillTrayGridFromOcvData(strTray) Then
            CalculateGradesFromGrid()
        Else
            DGVbar.DataSource = Nothing
            DGVbar.Rows.Clear()
            DGVbar.Refresh()
            lblBarnumber.Text = "0"
        End If

    End Sub

    ''' <summary>
    ''' 基于 DGVbar 已填入的测试数据,用 GradeRule 的 LowerLimit/UpperLimit 直接判断每颗电池的等级,
    ''' 替代旧的 MSScriptControl 表达式求值方式。
    ''' </summary>
    Private Sub CalculateGradesFromGrid()
        Dim schemeName As String = Trim(cmboCondition.Text)
        If String.IsNullOrEmpty(schemeName) Then Exit Sub

        Dim rules As DataTable = Nothing
        Try
            Using service As New OcvDataService()
                rules = service.GetGradeRules(schemeName)
            End Using
        Catch ex As Exception
            Exit Sub
        End Try

        If rules Is Nothing OrElse rules.Rows.Count = 0 Then Exit Sub

        '按 GradeCode 分组
        Dim gradeRules As New Dictionary(Of String, List(Of DataRow))
        For Each row As DataRow In rules.Rows
            Dim gradeCode As String = Convert.ToString(row("GradeCode"))
            If Not gradeRules.ContainsKey(gradeCode) Then
                gradeRules.Add(gradeCode, New List(Of DataRow))
            End If
            gradeRules(gradeCode).Add(row)
        Next

        '获取当前 OCV 阶段在 DGVbar 中的列偏移
        Dim colOffset As Integer = GetGridStageColumnOffset(GetCurrentOcvStageCode())
        Dim voltageCol As Integer = colOffset
        Dim resistanceCol As Integer = colOffset + 1

        Dim totalBatteries As Integer = DGVbar.Rows.Count
        Dim gradeIndex As Integer = 0

        For Each kvp As KeyValuePair(Of String, List(Of DataRow)) In gradeRules
            Dim count As Integer = 0

            For Each gridRow As DataGridViewRow In DGVbar.Rows
                '从 DGVbar 读电压(mV)和电阻(mOhm)
                Dim voltageMv As Double = 0
                Dim resistanceMOhm As Double = 0
                If gridRow.Cells(voltageCol).Value IsNot Nothing Then
                    Double.TryParse(gridRow.Cells(voltageCol).Value.ToString(), voltageMv)
                End If
                If gridRow.Cells(resistanceCol).Value IsNot Nothing Then
                    Double.TryParse(gridRow.Cells(resistanceCol).Value.ToString(), resistanceMOhm)
                End If
                'DGVbar 电压存的是 V*1000(毫伏),转回 V 用于比较
                Dim voltageV As Double = voltageMv / 1000.0

                '检查该等级的所有规则
                Dim allMatch As Boolean = True
                For Each rule As DataRow In kvp.Value
                    Dim fieldName As String = Convert.ToString(rule("FieldName"))
                    Dim lowerLimit As Double? = If(rule("LowerLimit") IsNot DBNull.Value, Convert.ToDouble(rule("LowerLimit")), CType(Nothing, Double?))
                    Dim upperLimit As Double? = If(rule("UpperLimit") IsNot DBNull.Value, Convert.ToDouble(rule("UpperLimit")), CType(Nothing, Double?))

                    Dim value As Double
                    Select Case fieldName
                        Case "VoltageVolt"
                            value = voltageV
                        Case "FinalResistanceMilliohm"
                            value = resistanceMOhm
                        Case Else
                            Continue For
                    End Select

                    '旧表语义: >= LowerLimit 且 < UpperLimit (闭开区间)
                    If lowerLimit.HasValue AndAlso value < lowerLimit.Value Then
                        allMatch = False
                        Exit For
                    End If
                    If upperLimit.HasValue AndAlso value >= upperLimit.Value Then
                        allMatch = False
                        Exit For
                    End If
                Next

                If allMatch Then count += 1
            Next

            '更新 dgv_grade:列2=数量, 列3=占比
            If gradeIndex < dgv_grade.Rows.Count Then
                dgv_grade.Item(2, gradeIndex).Value = count
                dgv_grade.Item(3, gradeIndex).Value = If(totalBatteries > 0, Math.Round(count / totalBatteries, 4), 0)
            End If
            gradeIndex += 1
        Next
    End Sub

    Private Function FillTrayGridFromOcvData(ByVal trayCode As String) As Boolean
        trayCode = Trim(trayCode)
        If trayCode = "" Then
            Return False
        End If

        Try
            Dim service As New OcvDataService()
            Dim table As System.Data.DataTable = service.GetTrayLatestStageResults(trayCode, GetCurrentOcvStageCode())
            If table Is Nothing OrElse table.Rows.Count = 0 Then
                Return False
            End If

            DGVbar.DataSource = Nothing
            DGVbar.Rows.Clear()

            For Each row As System.Data.DataRow In table.Rows
                Dim gridIndex As Integer = DGVbar.Rows.Add()
                Dim stageColumnOffset As Integer = GetGridStageColumnOffset(Convert.ToString(row("OcvStage")))
                DGVbar.Item(0, gridIndex).Value = Convert.ToString(row("BatteryCode"))
                If Not IsDBNull(row("VoltageVolt")) Then
                    DGVbar.Item(stageColumnOffset, gridIndex).Value = Round(Convert.ToDouble(row("VoltageVolt")) * 1000, 4).ToString()
                End If
                If Not IsDBNull(row("FinalResistanceMilliohm")) Then
                    DGVbar.Item(stageColumnOffset + 1, gridIndex).Value = Round(Convert.ToDouble(row("FinalResistanceMilliohm")), 4).ToString()
                End If
                If Not IsDBNull(row("ResultCreatedAt")) Then
                    DGVbar.Item(stageColumnOffset + 2, gridIndex).Value = row("ResultCreatedAt")
                End If
            Next

            lblBarnumber.Text = table.Rows.Count.ToString()
            DGVbar.Refresh()
            Return True
        Catch ex As Exception
            MsgBox("查询 OCV_DATA 托盘结果失败：" & ex.Message, vbOKOnly, "错误")
            Return False
        End Try
    End Function

    Private Sub LoadTrayListFromOcvData(ByVal showError As Boolean)
        Try
            Dim startTime As DateTime = DateTimePicker1.Value.Date
            Dim endTime As DateTime = startTime.AddDays(1)
            Dim service As New OcvDataService()
            Dim table As System.Data.DataTable = service.GetTrayListByDate(startTime, endTime, GetCurrentOcvStageCode())

            lstTray.Items.Clear()
            For Each row As System.Data.DataRow In table.Rows
                If Not IsDBNull(row("TrayCode")) AndAlso Convert.ToString(row("TrayCode")).Trim() <> "" Then
                    lstTray.Items.Add(Convert.ToString(row("TrayCode")))
                End If
            Next
        Catch ex As Exception
            If showError Then
                MsgBox("查询 OCV_DATA 托盘列表失败：" & ex.Message, vbOKOnly, "错误")
            End If
        End Try
    End Sub

    Private Function GetCurrentOcvStageCode() As String
        Select Case currentOCVNum
            Case 1
                Return "OCV0"
            Case 2
                Return "OCV1"
            Case 3
                Return "OCV2"
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function GetGridStageColumnOffset(ByVal stageCode As String) As Integer
        Select Case stageCode
            Case "OCV0"
                Return 1
            Case "OCV1"
                Return 4
            Case "OCV2"
                Return 7
            Case Else
                Return 10
        End Select
    End Function

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Dispose()
    End Sub
    Private Sub freshCondition(ByVal strLvName As String)
        Dim I, J As Int16

        '清除表格
        With dgv_Condition
            .RowCount = 0
            .ColumnCount = 1
            .Columns(0).HeaderText = "序号"
        End With
        '********************************************
        '有效等级 (改用 OcvDataService 新库)
        '添加一列：等级
        dgv_Condition.ColumnCount = dgv_Condition.ColumnCount + 1
        dgv_Condition.Columns(1).HeaderText = "等级"
        '有几个等级就添加几行
        Try
            Using service As New OcvDataService()
                Dim gradeTable As DataTable = service.GetGradeCodes(strLvName)
                I = 0
                For Each gradeRow As DataRow In gradeTable.Rows
                    I = I + 1
                    dgv_Condition.Rows.Add(1)
                    dgv_Condition.Item(0, I - 1).Value = I
                    dgv_Condition.Item(dgv_Condition.ColumnCount - 1, dgv_Condition.RowCount - 1).Value = Convert.ToString(gradeRow("GradeCode"))
                Next
            End Using
        Catch ex As Exception
            'OcvDataService 调用失败时优雅降级，不崩溃
        End Try
        '*************************************
        'DGVCondition data (改用 OcvDataService 新库)
        Try
            Using service As New OcvDataService()
                '获取 FieldName -> DisplayName 映射
                Dim fieldMap As New Dictionary(Of String, String)
                Dim selTable As DataTable = service.GetGradeSelectableFields()
                For Each selRow As DataRow In selTable.Rows
                    fieldMap.Add(Convert.ToString(selRow("FieldName")), Convert.ToString(selRow("DisplayName")))
                Next
                '获取分档规则
                Dim ruleTable As DataTable = service.GetGradeRules(strLvName)
                For Each ruleRow As DataRow In ruleTable.Rows
                    Dim gradeCode As String = Convert.ToString(ruleRow("GradeCode"))
                    Dim fieldName As String = Convert.ToString(ruleRow("FieldName"))
                    '获取字段中文名
                    Dim displayName As String = fieldName
                    If fieldMap.ContainsKey(fieldName) Then
                        displayName = fieldMap(fieldName)
                    End If
                    '找到对应的行
                    Dim foundCol As Boolean = False
                    For I = 0 To dgv_Condition.RowCount - 1
                        If dgv_Condition.Item(1, I).Value IsNot Nothing AndAlso
                            dgv_Condition.Item(1, I).Value.ToString() = gradeCode Then
                            '处理 LowerLimit
                            If ruleRow("LowerLimit") IsNot DBNull.Value Then
                                Dim lowerHeader As String = displayName & ">="
                                foundCol = False
                                For J = 2 To dgv_Condition.ColumnCount - 1
                                    If dgv_Condition.Columns(J).HeaderText = lowerHeader Then
                                        dgv_Condition.Item(J, I).Value = Convert.ToString(ruleRow("LowerLimit"))
                                        foundCol = True
                                        Exit For
                                    End If
                                Next J
                                If Not foundCol Then
                                    dgv_Condition.ColumnCount = dgv_Condition.ColumnCount + 1
                                    dgv_Condition.Columns(dgv_Condition.ColumnCount - 1).HeaderText = lowerHeader
                                    dgv_Condition.Item(dgv_Condition.ColumnCount - 1, I).Value = Convert.ToString(ruleRow("LowerLimit"))
                                End If
                            End If
                            '处理 UpperLimit
                            If ruleRow("UpperLimit") IsNot DBNull.Value Then
                                Dim upperHeader As String = displayName & "<"
                                foundCol = False
                                For J = 2 To dgv_Condition.ColumnCount - 1
                                    If dgv_Condition.Columns(J).HeaderText = upperHeader Then
                                        dgv_Condition.Item(J, I).Value = Convert.ToString(ruleRow("UpperLimit"))
                                        foundCol = True
                                        Exit For
                                    End If
                                Next J
                                If Not foundCol Then
                                    dgv_Condition.ColumnCount = dgv_Condition.ColumnCount + 1
                                    dgv_Condition.Columns(dgv_Condition.ColumnCount - 1).HeaderText = upperHeader
                                    dgv_Condition.Item(dgv_Condition.ColumnCount - 1, I).Value = Convert.ToString(ruleRow("UpperLimit"))
                                End If
                            End If
                            Exit For
                        End If
                    Next I
                Next
            End Using
        Catch ex As Exception
            'OcvDataService 调用失败时优雅降级，不崩溃
        End Try
    End Sub

    Private Sub btnSetCon_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetCon.Click
        Me.Cursor = Cursors.WaitCursor
        If objfrmCondition Is Nothing Then
            objfrmCondition = New frmSetCondition
        Else
            objfrmCondition.Dispose()
            objfrmCondition = Nothing
            objfrmCondition = New frmSetCondition
        End If
        objfrmCondition.mPara1 = cmboCondition.Text
        objfrmCondition.ShowDialog()

        '*****************************
        '刷新查询界面的条件名称列表 (改用 OcvDataService 新库)
        cmboCondition.Items.Clear()
        Try
            Using service As New OcvDataService()
                Dim schemeTable As DataTable = service.GetGradeSchemes()
                For Each schemeRow As DataRow In schemeTable.Rows
                    cmboCondition.Items.Add(Convert.ToString(schemeRow("SchemeName")))
                Next
            End Using
        Catch ex As Exception
            'OcvDataService 调用失败时优雅降级，不崩溃
        End Try
        Dim i As Long
        For i = 0 To cmboCondition.Items.Count - 1
            If cmboCondition.Items(i).ToString = objfrmCondition.mPara1 Then
                cmboCondition.SelectedIndex = i
                Exit For
            End If
        Next
        If i = cmboCondition.Items.Count And cmboCondition.Items.Count > 0 Then
            cmboCondition.SelectedIndex = 0
        End If
        objfrmCondition.mPara1 = ""
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub btnExport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExport.Click
        Dim I, J As Integer
        Dim ExcelApp As New Microsoft.Office.Interop.Excel.Application
        Dim ExcelBook As Microsoft.Office.Interop.Excel.Workbook
        Dim ExcelSheet As Microsoft.Office.Interop.Excel.Worksheet
        'Dim excelApp As New Microsoft.Office.

        Try
            ExcelBook = ExcelApp.Workbooks.Add
            ExcelSheet = ExcelBook.Sheets(1)
            '********************************
            '初始化Excel表的表头
            For I = 0 To DGVbar.ColumnCount - 1
                ExcelSheet.Cells(1, I + 1).value = DGVbar.Columns(I).HeaderText
            Next
            '数据填入Excel表,从第二行起
            For I = 0 To DGVbar.RowCount - 1
                For J = 0 To DGVbar.ColumnCount - 1
                    ExcelSheet.Cells(I + 2, J + 1).value = DGVbar.Item(J, I).Value
                Next J
            Next I
            ExcelApp.Visible = True
            ExcelSheet.Visible = True
            ExcelSheet.Activate()
            ExcelApp.DisplayAlerts = False
        Catch
        End Try
    End Sub

    Private Sub txtTray_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTray.TextChanged

    End Sub

    Private Sub DateTimePicker1_ValueChanged(sender As Object, e As EventArgs) Handles DateTimePicker1.ValueChanged
        LoadTrayListFromOcvData(True)
    End Sub
End Class
