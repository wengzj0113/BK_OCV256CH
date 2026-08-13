Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Centralized visual tokens and styling helpers for the UI-only refresh.
''' This class must not read or modify test, device, database, or MES state.
''' </summary>
Public NotInheritable Class UiTheme
    Private Sub New()
    End Sub

    Public Enum ButtonRole
        Primary
        Danger
        Warning
        Neutral
    End Enum

    Public Shared ReadOnly PageBackground As Color = ColorTranslator.FromHtml("#F5F7FA")
    Public Shared ReadOnly CardBackground As Color = Color.White
    Public Shared ReadOnly CardBorder As Color = ColorTranslator.FromHtml("#E5E7EB")
    Public Shared ReadOnly HeaderBackground As Color = ColorTranslator.FromHtml("#0B2345")
    Public Shared ReadOnly Primary As Color = ColorTranslator.FromHtml("#1677FF")
    Public Shared ReadOnly Success As Color = ColorTranslator.FromHtml("#22C55E")
    Public Shared ReadOnly Warning As Color = ColorTranslator.FromHtml("#F59E0B")
    Public Shared ReadOnly Danger As Color = ColorTranslator.FromHtml("#EF4444")
    Public Shared ReadOnly TextPrimary As Color = ColorTranslator.FromHtml("#1F2937")
    Public Shared ReadOnly TextSecondary As Color = ColorTranslator.FromHtml("#6B7280")

    Public Shared ReadOnly UiFont As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Regular)
    Public Shared ReadOnly UiFontBold As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Bold)
    Public Shared ReadOnly UiFontTitle As New Font("Microsoft YaHei UI", 14.0F, FontStyle.Bold)

    Public Shared Sub ApplyCardStyle(ByVal control As Control)
        If control Is Nothing Then Return

        control.BackColor = CardBackground
        control.ForeColor = TextPrimary
        control.Font = UiFont

        Dim groupBox As GroupBox = TryCast(control, GroupBox)
        If groupBox IsNot Nothing Then
            groupBox.FlatStyle = FlatStyle.Flat
            groupBox.ForeColor = TextPrimary
        End If

        Dim panel As Panel = TryCast(control, Panel)
        If panel IsNot Nothing Then
            panel.BorderStyle = BorderStyle.FixedSingle
        End If
    End Sub

    Public Shared Sub ApplyTextBoxStyle(ByVal textBox As TextBox)
        If textBox Is Nothing Then Return

        textBox.BackColor = CardBackground
        textBox.ForeColor = TextPrimary
        textBox.Font = UiFont
        textBox.BorderStyle = BorderStyle.FixedSingle
    End Sub

    Public Shared Sub ApplyButtonStyle(ByVal button As Button, ByVal role As ButtonRole)
        If button Is Nothing Then Return

        button.Font = UiFontBold
        button.FlatStyle = FlatStyle.Flat
        button.FlatAppearance.BorderSize = 1
        button.UseVisualStyleBackColor = False

        Select Case role
            Case ButtonRole.Primary
                button.BackColor = Primary
                button.ForeColor = Color.White
                button.FlatAppearance.BorderColor = Primary
            Case ButtonRole.Danger
                button.BackColor = Color.White
                button.ForeColor = Danger
                button.FlatAppearance.BorderColor = Danger
            Case ButtonRole.Warning
                button.BackColor = Warning
                button.ForeColor = Color.White
                button.FlatAppearance.BorderColor = Warning
            Case Else
                button.BackColor = Color.White
                button.ForeColor = TextPrimary
                button.FlatAppearance.BorderColor = CardBorder
        End Select
    End Sub
End Class
