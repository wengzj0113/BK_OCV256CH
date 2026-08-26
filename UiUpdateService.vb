Imports System
Imports System.Windows.Forms

Public Class UiUpdateService
    Private ReadOnly owner As Control
    Private ReadOnly channelGrid As ChannelGridControl
    Private ReadOnly statisticsControl As StatisticsControl

    Public Sub New(ByVal owner As Control, ByVal channelGrid As ChannelGridControl, ByVal statisticsControl As StatisticsControl)
        Me.owner = owner
        Me.channelGrid = channelGrid
        Me.statisticsControl = statisticsControl
    End Sub

    Public Sub UpdateChannel(ByVal channelNo As Integer, ByVal data As ChannelDisplayData)
        RunOnUiThread(Sub()
                          If channelGrid IsNot Nothing AndAlso Not channelGrid.IsDisposed Then
                              channelGrid.UpdateChannel(channelNo, data)
                          End If
                      End Sub)
    End Sub

    Public Sub UpdateStatistics(ByVal statistics As ChannelStatistics)
        RunOnUiThread(Sub()
                          If statisticsControl IsNot Nothing AndAlso Not statisticsControl.IsDisposed Then
                              statisticsControl.UpdateStatistics(statistics)
                          End If
                      End Sub)
    End Sub

    Private Sub RunOnUiThread(ByVal update As Action)
        If update Is Nothing OrElse owner Is Nothing OrElse owner.IsDisposed Then Return
        If owner.InvokeRequired Then
            owner.BeginInvoke(update)
        Else
            update()
        End If
    End Sub
End Class
