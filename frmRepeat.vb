Public Class frmRepeat
    Public Test_Num As Short
    Public Test_Point() As Short
    Public Point_Num As Short
    Private ObjOldWidth As Long  '保存窗体的原始宽度
    Private ObjOldHeight As Long '保存窗体的原始高度
    Private ObjOldFont As Single '保存窗体的原始字体比
    Private Sub frmRepeat_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ResizeInit(Me)
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

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


    Private Sub frmRepeat_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        ResizeForm(Me)
    End Sub
End Class