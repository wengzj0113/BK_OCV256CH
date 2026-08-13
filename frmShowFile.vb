Public Class frmShowFile

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
    Private Sub btnExport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExport.Click
        '    Call ExpToXls
        If lstData.Items.Count = 0 Then
            MsgBox("无数据可导出。", vbOKOnly, "提示")
            Exit Sub
        End If

        Dim xlObject As Object 'Excel.Application
        Dim xlWB As Object 'Excel.Workbook
        Dim strExcelFile As String
        Dim intLoop As Integer
        Dim Temp() As String = New String() {"", "", ""}
        'On Error GoTo exit_sub
        xlObject = CreateObject("Excel.Application")

        xlWB = xlObject.Workbooks.Add
        With xlWB.activesheet
            .cells(2, 1) = "电池编码"
            .cells(2, 2) = "OCV"
            .cells(2, 3) = "电压"
            .cells(2, 4) = "内阻"
            For intLoop = 0 To lstData.Items.Count - 1
                Temp = Split(lstData.Items.Item(intLoop), ";")
                .cells(intLoop + 3, 1) = Split(Temp(0), "：")(1) '电池编码
                .cells(intLoop + 3, 2) = Split(Temp(1), "：")(1) 'OCV
                .cells(intLoop + 3, 3) = Split(Temp(3), "：")(1) '电压
                .cells(intLoop + 3, 4) = Split(Temp(4), "：")(1) '内阻
            Next
            '托盘号
            .cells(1, 1) = Temp(2)
        End With
        xlObject.Visible = True

        If Dir("C:\ExpData", vbDirectory) = "" Then
            MkDir("C:\ExpData")
        End If
        '文件名
        strExcelFile = "C:\ExpData\DataExp_" & Temp(2) & ".xls"
        xlWB.SaveAs(strExcelFile)
exit_sub:
    End Sub
End Class
