Imports System.Collections.Generic

Public Enum PlcAutomaticTestState
    Idle
    ReadingTrayCode
    WaitingManualTrayCode
    WaitingTrayScanRetry
    WaitingR150Reset
    ArmingR151
    WaitingFixturePressed
    PreparingTest
    InitialTesting
    AutoRetesting
    Finalizing
    WaitingR152
    WaitingR161Ack
    WaitingR162Ack
    WaitingCycleReset
    Fault
End Enum

Public Enum PlcR152TestState
    NoTestStarted
    TestInProgress
    TestCompleted
    TestFaulted
End Enum

Public Class AutomaticTestCycle
    Public Property TrayCode As String
    Public Property NeedNgRetest As Boolean
    Public Property RequiresNgSorting As Boolean
    Public Property MaxRetestRounds As Integer
    Public Property CurrentRetestRound As Integer
    Public Property CurrentNgChannels As New HashSet(Of Integer)()
    Public Property NgGradesWrittenChannels As New HashSet(Of Integer)()
    Public Property FinalResultPublished As Boolean
    Public Property NgGradesWritten As Boolean
    Public Property R161Written As Boolean
    Public Property TrayConfirmed As Boolean
    Public Property TestStarted As Boolean

    Public Sub Reset()
        TrayCode = String.Empty
        NeedNgRetest = False
        RequiresNgSorting = False
        MaxRetestRounds = 0
        CurrentRetestRound = 0
        CurrentNgChannels.Clear()
        NgGradesWrittenChannels.Clear()
        FinalResultPublished = False
        NgGradesWritten = False
        R161Written = False
        TrayConfirmed = False
        TestStarted = False
    End Sub
End Class

''' <summary>A single completed OCV0 result waiting for the PLC sorting request.</summary>
Public Class PendingNgSelection
    Public Property TrayCode As String
    Public Property Channels As New HashSet(Of Integer)()
    Public Property WrittenChannels As New HashSet(Of Integer)()
    Public Property GradesWritten As Boolean
End Class

Public Class ChannelTestResult
    Public Property ChannelNumber As Integer
    Public Property Voltage As Single
    Public Property Resistance As Single
    Public Property IsNg As Boolean
End Class

Public Class TestPassResult
    Public Property Completed As Boolean
    Public Property Results As New List(Of ChannelTestResult)()
    Public Property ErrorMessage As String
End Class
