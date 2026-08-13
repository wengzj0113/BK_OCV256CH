Imports System.Data.SqlClient
Public Class frmSetCondition
    Dim strLVName As String
    Public mPara1 As String
    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        mPara1 = txtcondition.Text
        Me.Close()
    End Sub

    Private Sub frmSetCondition_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        mPara1 = txtcondition.Text
    End Sub

    Private Sub frmSetCondition_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        mPara1 = txtcondition.Text
    End Sub
    Private Sub frmSetCondition_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim I As Integer
        '*********************
        '初始化有效等级列表，最大等级为100
        With cmboGrade
            For I = 0 To 99
                .Items.Add(I + 1)
            Next I
            .SelectedIndex = 0
        End With
        '判断数据库的连接状态
        '分选条件可选项目*************************
        Try
            Using service As New OcvDataService()
                Dim dt As DataTable = service.GetGradeSelectableFields()
                For Each dr As DataRow In dt.Rows
                    lstcondition.Items.Add(dr("DisplayName").ToString())
                    lstNosee.Items.Add(dr("FieldName").ToString())
                Next
            End Using
        Catch ex As Exception
            MsgBox("加载可选字段失败: " & ex.Message)
        End Try
        '******************************************
        '初始化条件列表
        Call freshLvName()
        '*****************************************
        '刷新条件
        Call freshCondition(strLVName)
        For I = 0 To CmboCondition.Items.Count - 1
            If CmboCondition.Items(I).ToString = mPara1 Then
                CmboCondition.SelectedIndex = I
            End If
        Next
    End Sub
    ''' <summary>
    ''' 更新分选名字
    ''' </summary>
    Private Sub freshLvName()
        '初始化条件列表
        CmboCondition.Items.Clear()
        Try
            Using service As New OcvDataService()
                Dim dt As DataTable = service.GetGradeSchemes()
                For Each dr As DataRow In dt.Rows
                    CmboCondition.Items.Add(dr("SchemeName").ToString())
                Next
            End Using
        Catch ex As Exception
            MsgBox("加载分档方案失败: " & ex.Message)
        End Try
        '************************
        '刷新显示
        If CmboCondition.Items.Count > 0 Then
            CmboCondition.SelectedIndex = 0
        End If
        strLVName = Trim(CmboCondition.Text)
        txtcondition.Text = strLVName

    End Sub
    Private Sub freshCondition(ByVal strLvName As String)
        Dim I As Int16
        Dim row As Integer
        Dim oldConName As String = ""
        txtcondition.Text = strLvName
        ''********************************************
        ''有效等级
        'objClsDB = New clsDB
        'strSQL = "SELECT DISTINCT CondName FROM CW_tblCondition  where LvName=" & "'" & strLVName & "'"
        'myDataReader = objClsDB.DoQuery(strSQL)
        'I = 0
        'DGVCondition.RowCount = 0
        'Do While myDataReader.Read
        '    I = I + 1
        '    DGVCondition.Rows.Add(1)
        'Loop
        'cmboGrade.SelectedIndex = I - 1
        'myDataReader.Close()
        'objClsDB.Dispose()
        ''**********************************************
        'update DGVCondition
        Dim list As New List(Of Int16)
        With DGVCondition
            'DGVCondition 的表头
            While DGVCondition.Columns.Count > 0
                DGVCondition.Columns.RemoveAt(0)
            End While
            DGVCondition.Columns.Add("", "序号")
            DGVCondition.Columns.Add("", "等级")

            Try
                Using service As New OcvDataService()
                    ' 构建 FieldName -> DisplayName 字典
                    Dim fieldMap As New Dictionary(Of String, String)()
                    Dim fieldDt As DataTable = service.GetGradeSelectableFields()
                    For Each fr As DataRow In fieldDt.Rows
                        fieldMap(fr("FieldName").ToString()) = fr("DisplayName").ToString()
                    Next

                    ' 获取分档规则
                    Dim ruleDt As DataTable = service.GetGradeRules(strLvName)

                    ' 第一部分：根据规则中的 FieldName 反查 lstNosee 索引，勾选 lstcondition
                    For I = 0 To lstcondition.Items.Count - 1
                        lstcondition.SetItemChecked(I, False)
                    Next
                    For Each dr As DataRow In ruleDt.Rows
                        Dim fieldName As String = dr("FieldName").ToString()
                        For I = 0 To lstNosee.Items.Count - 1
                            If fieldName = lstNosee.Items.Item(I).ToString() Then
                                list.Add(I)
                                Exit For
                            End If
                        Next
                    Next
                    List.Sort()
                    For J = 0 To List.Count - 1
                        I = List(J)
                        If lstcondition.GetItemCheckState(I) = CheckState.Unchecked Then
                            lstcondition.Text = lstcondition.Items.Item(I)
                            Dim ee = New System.Windows.Forms.ItemCheckEventArgs(I, CheckState.Unchecked, CheckState.Checked)
                            lstcondition_ItemCheck(Nothing, ee)
                            lstcondition.SetItemChecked(I, True)
                        End If
                    Next

                    ' 第二部分：填充 DGV 数据
                    row = -1
                    oldConName = ""

                    For Each dr As DataRow In ruleDt.Rows
                        Dim gradeCode As String = dr("GradeCode").ToString()
                        If gradeCode = "" Then Continue For
                        If gradeCode <> oldConName Then
                            oldConName = gradeCode
                            row = row + 1
                            If DGVCondition.RowCount < row + 1 Then
                                DGVCondition.Rows.Add()
                            End If
                            DGVCondition.Item(0, row).Value = row + 1
                        End If
                        DGVCondition.Item(1, row).Value = gradeCode

                        Dim fieldName As String = dr("FieldName").ToString()
                        Dim displayName As String = ""
                        If fieldMap.ContainsKey(fieldName) Then
                            displayName = fieldMap(fieldName)
                        End If

                        Dim condHeader As String = ""
                        Dim condValue As String = ""
                        If Not IsDBNull(dr("LowerLimit")) Then
                            condHeader = displayName & ">="
                            condValue = dr("LowerLimit").ToString()
                        ElseIf Not IsDBNull(dr("UpperLimit")) Then
                            condHeader = displayName & "<"
                            condValue = dr("UpperLimit").ToString()
                        End If

                        If condHeader <> "" Then
                            Dim colIdx As Integer
                            For colIdx = 0 To DGVCondition.Columns.Count - 1
                                If InStr(DGVCondition.Columns(colIdx).HeaderText, condHeader) > 0 Then
                                    Exit For
                                End If
                            Next
                            If colIdx = DGVCondition.Columns.Count Then
                                Dim col = New DataGridViewTextBoxColumn()
                                col.HeaderText = condHeader
                                DGVCondition.Columns.Add(col)
                                colIdx = DGVCondition.Columns.Count - 1
                            End If
                            DGVCondition.Item(colIdx, row).Value = condValue
                        End If
                    Next
                End Using
            Catch ex As Exception
                MsgBox("加载分档规则失败: " & ex.Message)
            End Try
            cmboGrade.SelectedIndex = row
        End With
    End Sub
    Private Sub CmboCondition_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmboCondition.SelectedIndexChanged
        strLVName = Trim(CmboCondition.Text)
        Call freshCondition(strLVName)
    End Sub
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim I As Integer
        Dim intLoop As Short
        Dim intCounter As Short
        Dim strFldEn As String = ""
        Dim strFldCn As String
        Dim strCondName As String
        Dim strLvName As String

        strLvName = Trim(txtcondition.Text)
        If strLvName = "" Then
            MsgBox("请输入分容代码")
            Exit Sub
        Else
            With DGVCondition
                If .ColumnCount < 3 Then
                    MsgBox("至少选择一个条件")
                    Exit Sub
                End If
                If .Rows.Count = 0 Then
                    MsgBox("至少要有一个等级")
                    Exit Sub
                End If
                For intLoop = 0 To .Rows.Count - 1
                    strCondName = Trim(.Item(1, intLoop).Value)
                    If strCondName = "" Then
                        MsgBox("等级名称不能为空", , "提示")
                        Exit Sub
                    End If
                    For intCounter = 2 To .Columns.Count - 1 Step 1
                        If .Item(intCounter, intLoop).Value Is Nothing OrElse
                           IsDBNull(.Item(intCounter, intLoop).Value) = True OrElse
                           IsNumeric(.Item(intCounter, intLoop).Value) = False Then
                            MsgBox("第 " & intLoop + 1 & " 行，第 " & intCounter + 1 & "列的值不对 ", , "提示")
                            Exit Sub
                        End If
                    Next
                Next
            End With
        End If
        With DGVCondition
            strLvName = CmboCondition.Text

            Dim code = CmboCondition.SelectedItem.GetHashCode
            If AddList.ContainsKey(code) Then
                Dim schemeService As New OcvDataService()
                If schemeService.CheckGradeSchemeExists(strLvName, isActive:=True) Then
                    MsgBox("该名称已经存在，请换另一个名称")
                    Exit Sub
                End If
            End If

            Try
                Using service As New OcvDataService()
                    service.DeleteGradeRules(strLvName)
                    For intLoop = 0 To .Rows.Count - 1
                        strCondName = Trim(.Item(1, intLoop).Value)
                        For intCounter = 2 To .Columns.Count - 1 Step 2
                            If strCondName <> "" Then
                                strFldCn = Replace(Replace(.Columns(intCounter).HeaderText, ">=", ""), "<", "")
                                strFldEn = ""
                                For I = 0 To lstcondition.Items.Count - 1
                                    If strFldCn = lstcondition.Items.Item(I) Then
                                        strFldEn = lstNosee.Items.Item(I)
                                        Exit For
                                    End If
                                Next
                                Dim sortOrder As Integer = (intCounter - 2) \ 2
                                If .Item(intCounter, intLoop).Value IsNot Nothing AndAlso
                                   Not IsDBNull(.Item(intCounter, intLoop).Value) Then
                                    service.SaveGradeRule(strLvName, strCondName, strFldEn,
                                                          CDec(.Item(intCounter, intLoop).Value), Nothing, sortOrder)
                                End If
                                If .Item(intCounter + 1, intLoop).Value IsNot Nothing AndAlso
                                   Not IsDBNull(.Item(intCounter + 1, intLoop).Value) Then
                                    service.SaveGradeRule(strLvName, strCondName, strFldEn,
                                                          Nothing, CDec(.Item(intCounter + 1, intLoop).Value), sortOrder)
                                End If
                            End If
                        Next intCounter
                    Next intLoop
                End Using
            Catch ex As Exception
                MsgBox("保存分档规则失败: " & ex.Message)
                Exit Sub
            End Try
            AddList.Remove(code)
        End With

        MsgBox("新增条件保存成功!", vbOKOnly)
        '***********************************
        '刷新条件列表
        Call freshLvName()
        For I = 0 To CmboCondition.Items.Count - 1
            If CmboCondition.Items(I).ToString = strLvName Then
                CmboCondition.SelectedIndex = I
            End If
        Next
    End Sub
    Dim AddList As New Dictionary(Of Integer, Integer)
    ''' <summary>
    ''' 增加分选条件
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        '清掉列表的项目选择
        Dim I As Integer
        lstcondition.ClearSelected()
        '显示窗口录入条件名
        frmName.ShowDialog()
        If frmName.mc <> "" Then
            txtcondition.Text = frmName.mc
            '遍历所有条件名，如果有重名的，直接退出
            For I = 0 To CmboCondition.Items.Count - 1
                If CmboCondition.Items(I).ToString = txtcondition.Text Then
                    Exit For
                End If
            Next
            '到最后都没有发现重名的，直接添加到列表中
            If I = CmboCondition.Items.Count Then
                CmboCondition.Items.Add(frmName.mc)
            End If
            '再次遍历所有条件名，选中该条件
            For I = 0 To CmboCondition.Items.Count - 1
                If CmboCondition.Items(I).ToString = txtcondition.Text Then
                    CmboCondition.SelectedIndex = I
                    '获得该条件对应的哈希代码
                    Dim code = CmboCondition.Items(I).GetHashCode()
                    '如果AddList中的键值不包含该哈希代码，把该条件添加到字典中
                    If AddList.ContainsKey(code) = True Then
                    Else
                        AddList.Add(code, code)
                    End If
                End If
            Next
        End If

    End Sub
    Private Sub txtcondition_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtcondition.KeyUp
        If e.KeyCode = Keys.Enter Then
            CmboCondition.Items.Add(Trim(txtcondition.Text))
            CmboCondition.SelectedIndex = CmboCondition.Items.Count - 1
        End If
    End Sub
    Private Sub cmboGrade_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmboGrade.SelectedIndexChanged
        Dim intGrade As Integer
        Dim I As Integer

        intGrade = Val(cmboGrade.Text)
        With DGVCondition
            .RowCount = intGrade
            For I = 0 To intGrade - 1
                .Rows(I).Cells(0).Value = I + 1
            Next
        End With
    End Sub

    Private Sub lstcondition_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lstcondition.ItemCheck
        Dim intColPos As Short
        Dim blnFindCol As Boolean

        blnFindCol = FindColumnInGrd(lstcondition.Text & ">=", intColPos)
        If lstcondition.GetItemChecked(e.Index) = False Then
            If blnFindCol Then
                DGVCondition.Columns(intColPos).Width = 2000
                DGVCondition.Columns(intColPos + 1).Width = 2000
            Else
                intColPos = DGVCondition.Columns.Count
                DGVCondition.Columns.Add(1, lstcondition.Text & ">=")
                DGVCondition.Columns.Add(1, lstcondition.Text & "<")
                DGVCondition.Columns(intColPos).Width = 2000
                DGVCondition.Columns(intColPos + 1).Width = 2000
            End If
        Else
            'DGVCondition.Columns(intColPos).Width = 0
            'DGVCondition.Columns(intColPos + 1).Width = 0
            If blnFindCol = True Then
                DGVCondition.Columns.RemoveAt(intColPos) '>=
                DGVCondition.Columns.RemoveAt(intColPos) '<
            End If
        End If
    End Sub
    Private Function FindColumnInGrd(ByRef strDesi As String, ByRef intColPos As Short) As Boolean
        Dim intLoop As Short

        For intLoop = 0 To DGVCondition.Columns.Count - 1
            If InStr(DGVCondition.Columns(intLoop).HeaderText, strDesi) > 0 Then
                intColPos = intLoop
                FindColumnInGrd = True
                Exit Function
            End If
        Next intLoop

        FindColumnInGrd = False
    End Function

    Private Sub btnDel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDel.Click
        Dim strLvName As String
        '*********************************
        '删除当前条件
        strLvName = CmboCondition.Text
        Try
            Using service As New OcvDataService()
                service.DeleteGradeRules(strLvName)
            End Using
        Catch ex As Exception
            MsgBox("删除分档规则失败: " & ex.Message)
        End Try
        '*************************************
        '刷新条件列表
        Call freshLvName()
        '************************************
        '刷新当前界面
        If CmboCondition.Items.Count <= 0 Then
            Exit Sub
        End If
        CmboCondition.SelectedIndex = 0
        strLvName = CmboCondition.Text
        txtcondition.Text = strLvName
        Call freshCondition(strLvName)
    End Sub

    Private Sub GroupBox1_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GroupBox1.Enter

    End Sub
End Class
