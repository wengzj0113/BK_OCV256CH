Imports System.Data
Imports System.Data.SqlClient
''' <summary>
''' 旧库 CWSQL 数据访问层,已被 OcvDataService (OCV_DATA) 替代。
''' R2-15 起:frmTest 不再调用 Ins4BarCode/Ups4BarCode/Query4BarCode;
''' Module1 不再调用 DoQuery 查 CW_tblCondition/CW_tblSelFld;
''' frmSetCondition/frmInquery 分档条件已切新库。
''' 仅保留 MDIForm 启动连接检查和 frmSetCondition 的 IsExists(查 cw_trayMain),
''' 后续版本将完全废弃。
''' </summary>
<Obsolete("clsDB 已迁移到 OCV_DATA.OcvDataService,新增代码请勿使用。")>
Public Class clsDB
    Public adoConn As SqlConnection
    Public adoComm As SqlCommand
    Public DataSource As String '数据源
    Public DataServer As String '数据服务器
    Public UserID As String '用户名
    Public Userpwd As String '用户密码
    Public DataBaseStation As Boolean '数据库状态
    ''' <summary>
    ''' 初始化一个数据库
    ''' </summary>
    Sub New()
        Dim connectstring As String
        DataSource = "CWSQL"
        DataServer = Initsetting.strDatabase  '初始化设置中的IP地址
        UserID = Initsetting.strUserID '初始化中的用户ID
        Userpwd = Initsetting.strpwd '初始化中的用户密码
        ' Userpwd = "123"
        '组合数据库链接字符串：sever、database、UserID、Password
        connectstring = "server=" & DataServer & ";" & "DataBase=" & DataSource & ";" & "User ID=" & UserID & ";" & "Password=" & Userpwd & ""
        pubConnectionString = connectstring
        '仅配置连接字符串,不再强行 Close/Open 全局 myConnection,避免与 SqlServerHelper 并发时互相冲掉 (R2-04)
        SyncLock DBOpenLock
            If myConnection.State = ConnectionState.Closed Then
                myConnection.ConnectionString = connectstring
            ElseIf myConnection.ConnectionString <> connectstring Then
                '其他实例已打开且连接串不同,保留现有连接不动,避免打断正在执行的查询
            End If
            DataBaseStation = (myConnection.State = ConnectionState.Open)
        End SyncLock

    End Sub
    '用于序列化全局 myConnection 的 Open/Close 操作 (R2-04)
    Private Shared ReadOnly DBOpenLock As New Object()
    ''' <summary>
    ''' 关闭数据库，并改变数据库链接状态
    ''' </summary>
    Public Sub Dispose()
        SyncLock DBOpenLock
            If DataBaseStation = True Then
                DataBaseStation = False
                Try
                    If myConnection.State <> ConnectionState.Closed Then
                        myConnection.Close()
                    End If
                Catch
                End Try
            End If
        End SyncLock
    End Sub
    ''' <summary>
    ''' 打开数据库，并返回打开结果
    ''' </summary>
    ''' <param name="DataBaseStation"></param>
    ''' <returns></returns>
    Public Function open(ByVal DataBaseStation As Boolean) As Boolean
        SyncLock DBOpenLock
            Try
                If myConnection.State = ConnectionState.Closed Then
                    If String.IsNullOrEmpty(myConnection.ConnectionString) Then
                        myConnection.ConnectionString = pubConnectionString
                    End If
                    myConnection.Open()
                End If
                open = (myConnection.State = ConnectionState.Open)
            Catch Err As Exception
                open = False
            End Try
        End SyncLock
    End Function
    Public Function GetGrade4LVname(ByVal objLV As String) As SqlDataReader
        Dim adoComm As SqlCommand
        Dim strsql As String

        strsql = "SELECT   CondName FROM CW_tblCondition where LVName=" & "'" & objLV & "' group by CondName "
        adoComm = New SqlCommand(strsql, myConnection)
        GetGrade4LVname = adoComm.ExecuteReader
    End Function
    ''' <summary>
    ''' 执行查询操作
    ''' </summary>
    ''' <param name="strSQL"></param>
    ''' <returns></returns>
    Public Function DoQuery(ByVal strSQL As String) As SqlDataReader
        Dim adoComm As SqlCommand
        adoComm = New SqlCommand
        With adoComm
            .CommandType = CommandType.Text
            .Connection = myConnection
            .CommandText = strSQL
            DoQuery = .ExecuteReader
        End With
    End Function
    ''' <summary>
    ''' 插入记录
    ''' </summary>
    ''' <param name="strTray"></param>
    ''' <param name="OCV"></param>
    ''' <param name="RecieveData"></param>
    Public Sub Ins4BarCode(ByVal strTray As String, ByVal OCV As Integer, ByVal RecieveData As Object)
        Dim intRecord As Integer
        Dim SQL As String
        If OCV = 1 Then
            SQL = "insert CW_tblData_Info(Fld_Tray,Fld_SmCode, Fld_VolA,Fld_ResiA,Fld_TimeA) values(@Param1,@Param2,@Param3,@Param4,@Param5)"
        ElseIf OCV = 2 Then
            SQL = "insert CW_tblData_Info(Fld_tray,Fld_SmCode, Fld_VolB,Fld_ResiB,Fld_TimeB) values(@Param1,@Param2,@Param3,@Param4,@Param5)"
        ElseIf OCV = 3 Then
            SQL = "insert CW_tblData_Info(Fld_tray,Fld_SmCode, Fld_VolC,Fld_ResiC,Fld_TimeC) values(@Param1,@Param2,@Param3,@Param4,@Param5)"
        Else
            SQL = "insert CW_tblData_Info(Fld_Tray,Fld_SmCode, Fld_VolD,Fld_ResiD,Fld_TimeD) values(@Param1,@Param2,@Param3,@Param4,@Param5)"
        End If
        adoComm = New SqlCommand
        With adoComm
            .Parameters.Clear()
            .Parameters.AddWithValue("@Param1", strTray)
            .Parameters.AddWithValue("@Param2", CType(RecieveData, Bat_info).BatCode)
            .Parameters.AddWithValue("@Param3", CType(RecieveData, Bat_info).Voltage)
            .Parameters.AddWithValue("@Param4", CType(RecieveData, Bat_info).Resistance)
            .Parameters.AddWithValue("@Param5", CType(RecieveData, Bat_info).TestTime)
            .Connection = myConnection
            .CommandText = SQL
            intRecord = adoComm.ExecuteNonQuery()
        End With
        'FreeDB()
    End Sub
    ''' <summary>
    ''' 更新记录
    ''' </summary>
    ''' <param name="strTray"></param>
    ''' <param name="OCV"></param>
    ''' <param name="ReciveData"></param>
    Public Sub Ups4BarCode(ByVal strTray As String, ByVal OCV As Int16, ByRef ReciveData As Object)
        Dim intRecord As Integer
        Dim SQL As String
        Dim adoComm As SqlCommand
        If OCV = 1 Then
            SQL = "UPDATE  dbo.CW_tblData_Info set  Fld_Tray='" & strTray & "'" & ", Fld_VolA='" & CType(ReciveData, Bat_info).Voltage & "'" & ", Fld_ResiA=' " & CType(ReciveData, Bat_info).Resistance & "'" & ", Fld_TimeA=' " & CType(ReciveData, Bat_info).TestTime & "'" &
                " where Fld_SmCode =" & "'" & CType(ReciveData, Bat_info).BatCode & "' "
        ElseIf OCV = 2 Then
            SQL = "UPDATE  dbo.CW_tblData_Info set  Fld_Tray=" & "'" & strTray & "'" & ",Fld_VolB=" & CType(ReciveData, Bat_info).Voltage & ",Fld_ResiB= " & CType(ReciveData, Bat_info).Resistance & ", Fld_TimeB=' " & CType(ReciveData, Bat_info).TestTime & "'" &
                "   where Fld_SmCode =" & "'" & CType(ReciveData, Bat_info).BatCode & "'"
        ElseIf OCV = 3 Then
            SQL = "UPDATE  dbo.CW_tblData_Info set Fld_Tray=" & "'" & strTray & "'" & ", Fld_VolC=" & CType(ReciveData, Bat_info).Voltage & ",Fld_ResiC= " & CType(ReciveData, Bat_info).Resistance & ", Fld_TimeC=' " & CType(ReciveData, Bat_info).TestTime & "'" &
                " where Fld_SmCode =" & "'" & CType(ReciveData, Bat_info).BatCode & "'"
        Else
            SQL = "UPDATE  dbo.CW_tblData_Info set Fld_Tray=" & "'" & strTray & "'" & ", Fld_VolD=" & CType(ReciveData, Bat_info).Voltage & ",Fld_ResiD= " & CType(ReciveData, Bat_info).Resistance & ", Fld_TimeD=' " & CType(ReciveData, Bat_info).TestTime & "'" &
                " where Fld_SmCode =" & "'" & CType(ReciveData, Bat_info).BatCode & "'"
        End If
        adoComm = New SqlCommand
        adoComm.Connection = myConnection
        adoComm.CommandText = SQL
        intRecord = adoComm.ExecuteNonQuery()
        'FreeDB()
    End Sub
    ''' <summary>
    ''' 根据托盘号查询
    ''' </summary>
    ''' <param name="BarCode"></param>
    ''' <param name="strTray"></param>
    ''' <returns></returns>
    Public Function Query4BarCode(Optional ByVal BarCode As String = "", Optional ByVal strTray As String = "") As SqlDataReader
        '查询是否有相同的条码
        Dim SQL As String = ""
        Dim adoComm As SqlCommand

        SQL = "select Fld_Tray,fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info where fld_Smcode=" & "'" & BarCode & "'"
        adoComm = New SqlCommand
        With adoComm
            .CommandType = CommandType.Text
            .CommandText = SQL
            .Connection = myConnection
            Query4BarCode = .ExecuteReader
        End With

    End Function
    ''' <summary>
    ''' 查询记录
    ''' </summary>
    ''' <param name="myDataReader"></param>
    ''' <param name="BarCode"></param>
    ''' <param name="strTray"></param>
    Public Sub BatchQueryData(ByVal myDataReader As SqlDataReader, Optional ByVal BarCode As String = "", Optional ByVal strTray As String = "")
        Dim SQL As String = ""
        Dim sqlComm As SqlCommand

        If String.IsNullOrEmpty(BarCode) = False And String.IsNullOrEmpty(strTray) = False Then
            SQL = "select Fld_Tray,fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info where fld_Smcode like BarCode " & " And fld_Tray like  " & " '" & strTray & "'"
        ElseIf String.IsNullOrEmpty(BarCode) = False And String.IsNullOrEmpty(strTray) = True Then
            SQL = "select Fld_Tray, fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info where fld_Smcode like  " & "'" & BarCode & "'"
        ElseIf String.IsNullOrEmpty(BarCode) = True And String.IsNullOrEmpty(strTray) = False Then
            SQL = "select Fld_Tray,fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info where  fld_Tray like  " & " '" & strTray & "'"
        End If
        sqlComm = New SqlCommand
        With sqlComm
            .Connection = myConnection
            .CommandType = CommandType.Text
            .CommandText = SQL
            myDataReader = .ExecuteReader
        End With
    End Sub
    ''' <summary>
    ''' 初始化选择，选择所有记录
    ''' </summary>
    ''' <param name="myDataReader"></param>
    Public Sub InitSelect(ByRef myDataReader As SqlDataReader)
        Dim SQL As String
        Dim sqlComm As SqlCommand

        SQL = "select Fld_Tray,fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info "
        sqlComm = New SqlCommand
        With sqlComm
            .Connection = myConnection
            .CommandType = CommandType.Text
            .CommandText = SQL
            myDataReader = .ExecuteReader
        End With
    End Sub
    ''' <summary>
    ''' 托盘选择
    ''' </summary>
    ''' <param name="myDataReader"></param>
    ''' <param name="strTray"></param>
    Public Sub TraySelect(ByRef myDataReader As SqlDataReader, ByVal strTray As String)
        Dim SQL As String
        Dim sqlComm As SqlCommand

        SQL = "select Fld_Tray,fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info  where Fld_Tray=" & "'" & strTray & "'"
        sqlComm = New SqlCommand
        With sqlComm
            .Connection = myConnection
            .CommandType = CommandType.Text
            .CommandText = SQL
            myDataReader = .ExecuteReader
        End With
    End Sub
    Public Sub NocSelect(ByRef myDataReader As SqlDataReader)
        Dim SQL As String
        Dim sqlComm As SqlCommand

        SQL = "select Fld_Tray,fld_Smcode,Fld_VolA,Fld_ResiA,Fld_VolB,Fld_ResiB,Fld_VolC,Fld_ResiC,Fld_VolD,Fld_ResiD from CW_tblData_Info "
        sqlComm = New SqlCommand
        With sqlComm
            .Connection = myConnection
            .CommandType = CommandType.Text
            .CommandText = SQL
            myDataReader = .ExecuteReader
        End With
    End Sub
    Public Sub ExitRecord(ByRef myDataReader As SqlDataReader, ByVal strTraynum As String, ByVal strBarCode As String)
        Dim SQL As String
        Dim sqlComm As SqlCommand

        SQL = "select * from CW_tblData_Info where Fld_Tray=" & "'" & strTraynum & "'" & " and " & "Fld_SmCode=" & "'" & strBarCode & "'"
        sqlComm = New SqlCommand
        With sqlComm
            .Connection = myConnection
            .CommandType = CommandType.Text
            .CommandText = SQL
            myDataReader = .ExecuteReader
        End With
    End Sub
    Public Sub InsertFldCond(ByRef strVal1 As String, ByRef strVal2 As String, ByRef strVal3 As String, ByRef strVal4 As String, ByRef strVal5 As String, ByRef strVal6 As String)
        Dim adoComm As SqlCommand
        Dim objClsDB As New clsDB
        Dim strFldInsSQL As String

        strFldInsSQL = "INSERT INTO CW_tblCondition(CondName,FldName,FldCond,FldValue,FldName_E, LvName) VALUES(@CondName,@FldName,@FldCond,@FldValue,@FldName_E,@LvName)"
        adoComm = New SqlCommand(strFldInsSQL, myConnection)
        adoComm.CommandType = CommandType.Text
        adoComm.CommandText = strFldInsSQL
        'adoComm.Connection.Open()
        adoComm.Parameters.Clear()
        adoComm.Parameters.AddWithValue("@CondName", strVal1)
        adoComm.Parameters.AddWithValue("@FldName", strVal2)
        adoComm.Parameters.AddWithValue("@FldCond", strVal3)
        adoComm.Parameters.AddWithValue("@FldValue", strVal4)
        adoComm.Parameters.AddWithValue("@FldName_E", strVal5)
        adoComm.Parameters.AddWithValue("@LvName", strVal6)
        adoComm.ExecuteNonQuery()
    End Sub
    Public Function FindFldCond(ByRef strVal1 As String, ByRef strVal2 As String, ByRef strVal3 As String) As Boolean
        Dim adoComm As SqlCommand
        Dim adoRst As SqlDataReader
        Dim strQrySQL As String
        Dim objClsDB As New clsDB

        FindFldCond = False
        strQrySQL = "SELECT 1 FROM CW_tblCondition WHERE FldName = @FldName AND CondName = @CondName AND LvName = @LvName"
        adoComm = New SqlCommand(strQrySQL, myConnection)
        adoComm.CommandType = CommandType.Text
        adoComm.CommandText = strQrySQL
        'adoComm.Connection.Open()
        adoComm.Parameters.Clear()
        adoComm.Parameters.AddWithValue("@FldName", strVal1)
        adoComm.Parameters.AddWithValue("@CondName", strVal2)
        adoComm.Parameters.AddWithValue("@LvName", strVal3)
        adoRst = adoComm.ExecuteReader
        If adoRst.HasRows Then
            FindFldCond = True
        End If
    End Function
    Public Sub UpdateFldCond(ByRef strVal1 As String, ByRef strVal2 As String, ByRef strVal3 As String, ByRef strVal4 As String, ByRef strVal5 As String)
        Dim adoComm As SqlCommand
        'Dim intLoop As Short
        Try
            adoComm = New SqlCommand("strFldUpdSQL", myConnection)
            adoComm.CommandType = CommandType.StoredProcedure
            'adoComm.Connection.Open()
            adoComm.Parameters.Clear()
            'adoComm.Parameters.Add(strVal1, SqlDbType.VarChar)
            adoComm.Parameters.AddWithValue("FldValue", strVal1)
            adoComm.Parameters.AddWithValue("FldName_E", strVal2)
            adoComm.Parameters.AddWithValue("CondName", strVal3)
            adoComm.Parameters.AddWithValue("LvName", strVal4)
            adoComm.Parameters.AddWithValue("FldCond", strVal5)
            adoComm.ExecuteNonQuery()
        Catch
        End Try
    End Sub
    Public Function IsExists(ByVal code As String) As Boolean
        Dim adoComm As SqlCommand

        adoComm = New SqlCommand
        With adoComm
            .Connection = myConnection
            .CommandType = CommandType.Text
            .CommandText = "select * from cw_trayMain where cTrayCode='" & code & "'"
            .Parameters.Clear()
            Dim adoRtnRst = .ExecuteReader
            If adoRtnRst.HasRows Then
                IsExists = True
            Else
                IsExists = False
            End If
            adoRtnRst.Close()
        End With
    End Function
    ''' <summary>
    ''' 给托盘表插入托盘号
    ''' </summary>
    ''' <param name="strTray"></param>
    ''' <returns></returns>
    Public Function InsertTrayNum(ByVal strTray As String) As Boolean
        Dim intRecord As Integer
        Dim SQL As String
        Dim adocomm As SqlCommand
        SQL = "insert cw_trayMain(cTrayCode) values(@Param1)"

        adoComm = New SqlCommand
        Try
            With adoComm
                .Parameters.Clear()
                .Parameters.AddWithValue("@Param1", strTray)

                .Connection = myConnection
                .CommandText = SQL
                intRecord = adoComm.ExecuteNonQuery()
            End With
            Return True
        Catch ex As Exception
            Return False
        End Try


    End Function
End Class
