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

    Public Shared ReadOnly PageBackground As Color = ColorTranslator.FromHtml("#F3F6FA")
    Public Shared ReadOnly CardBackground As Color = Color.White
    Public Shared ReadOnly CardBorder As Color = ColorTranslator.FromHtml("#E7EBF0")
    Public Shared ReadOnly NeutralButtonBackground As Color = ColorTranslator.FromHtml("#E7E7E5")
    Public Shared ReadOnly NeutralButtonBorder As Color = ColorTranslator.FromHtml("#A7ADB5")
    Public Shared ReadOnly NeutralButtonHoverBackground As Color = ColorTranslator.FromHtml("#DEDFDD")
    Public Shared ReadOnly NeutralButtonPressedBackground As Color = ColorTranslator.FromHtml("#D4D6D3")
    Public Shared ReadOnly HeaderBackground As Color = ColorTranslator.FromHtml("#061D3B")
    Public Shared ReadOnly HeaderHighlight As Color = ColorTranslator.FromHtml("#0B2E5B")
    Public Shared ReadOnly Primary As Color = ColorTranslator.FromHtml("#1769E8")
    Public Shared ReadOnly Success As Color = ColorTranslator.FromHtml("#1FA463")
    Public Shared ReadOnly SuccessBackground As Color = ColorTranslator.FromHtml("#D9F7C8")
    Public Shared ReadOnly Warning As Color = ColorTranslator.FromHtml("#F3A400")
    Public Shared ReadOnly Danger As Color = ColorTranslator.FromHtml("#EF3E3E")
    Public Shared ReadOnly DangerBackground As Color = ColorTranslator.FromHtml("#FFE0DE")
    Public Shared ReadOnly TestingBackground As Color = ColorTranslator.FromHtml("#78A9F4")
    Public Shared ReadOnly IdleBackground As Color = ColorTranslator.FromHtml("#EEF0F3")
    Public Shared ReadOnly TextPrimary As Color = ColorTranslator.FromHtml("#243142")
    Public Shared ReadOnly TextSecondary As Color = ColorTranslator.FromHtml("#637084")

    Public Shared ReadOnly UiFont As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Regular)
    Public Shared ReadOnly UiFontBold As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Bold)
    Public Shared ReadOnly UiFontTitle As New Font("Microsoft YaHei UI", 14.0F, FontStyle.Bold)
    Public Shared ReadOnly UiFontMetric As New Font("Microsoft YaHei UI", 20.0F, FontStyle.Regular)
    Public Const LeftSectionGap As Integer = 5
    Public Const LeftContentPadding As Integer = 6
    Public Const LeftButtonHeight As Integer = 28
    Public Const LeftStationButtonHeight As Integer = 56

    Public Shared Sub ApplyCardStyle(ByVal control As Control)
        If control Is Nothing Then Return

        control.BackColor = CardBackground
        control.ForeColor = TextPrimary
        control.Font = UiFont

        Dim groupBox As GroupBox = TryCast(control, GroupBox)
        If groupBox IsNot Nothing Then
            groupBox.FlatStyle = FlatStyle.Flat
            groupBox.ForeColor = Primary
            groupBox.Font = UiFontBold
            groupBox.Padding = New Padding(6, 15, 6, 6)
        End If

        Dim panel As Panel = TryCast(control, Panel)
        If panel IsNot Nothing Then
            panel.BorderStyle = BorderStyle.FixedSingle
        End If
    End Sub

    Public Shared Sub ApplyCheckBoxStyle(ByVal checkBox As CheckBox)
        If checkBox Is Nothing Then Return

        checkBox.AutoSize = False
        checkBox.Height = 28
        checkBox.Font = UiFontBold
        checkBox.BackColor = CardBackground
        checkBox.ForeColor = TextPrimary
        checkBox.TextAlign = ContentAlignment.MiddleCenter
        checkBox.Appearance = Appearance.Button
        checkBox.FlatStyle = FlatStyle.Flat
        checkBox.UseVisualStyleBackColor = False
        checkBox.FlatAppearance.BorderSize = 1
        ApplyToggleStateStyle(checkBox, checkBox.Checked)
    End Sub

    Public Shared Sub ApplyRadioButtonStyle(ByVal radioButton As RadioButton)
        If radioButton Is Nothing Then Return

        radioButton.AutoSize = False
        radioButton.Height = 28
        radioButton.Font = UiFontBold
        radioButton.BackColor = CardBackground
        radioButton.ForeColor = TextPrimary
        radioButton.TextAlign = ContentAlignment.MiddleCenter
        radioButton.Appearance = Appearance.Button
        radioButton.FlatStyle = FlatStyle.Flat
        radioButton.UseVisualStyleBackColor = False
        radioButton.FlatAppearance.BorderSize = 1
        ApplyToggleStateStyle(radioButton, radioButton.Checked)
    End Sub

    Public Shared Sub ApplyStationButtonStyle(ByVal radioButton As RadioButton)
        If radioButton Is Nothing Then Return

        radioButton.AutoSize = False
        radioButton.Height = LeftStationButtonHeight
        radioButton.Font = UiFontTitle
        radioButton.TextAlign = ContentAlignment.MiddleCenter
        radioButton.Appearance = Appearance.Button
        radioButton.FlatStyle = FlatStyle.Flat
        radioButton.UseVisualStyleBackColor = False
        radioButton.FlatAppearance.BorderSize = 1
        ApplyToggleStateStyle(radioButton, True)
    End Sub

    Public Shared Sub ApplyToggleStateStyle(ByVal control As Control, ByVal selected As Boolean)
        If control Is Nothing Then Return

        If selected Then
            control.BackColor = Primary
            control.ForeColor = Color.White
            Dim checkBox As CheckBox = TryCast(control, CheckBox)
            If checkBox IsNot Nothing Then
                checkBox.FlatAppearance.BorderColor = Primary
                checkBox.FlatAppearance.MouseOverBackColor = Primary
                checkBox.FlatAppearance.MouseDownBackColor = Primary
            End If
            Dim radioButton As RadioButton = TryCast(control, RadioButton)
            If radioButton IsNot Nothing Then
                radioButton.FlatAppearance.BorderColor = Primary
                radioButton.FlatAppearance.MouseOverBackColor = Primary
                radioButton.FlatAppearance.MouseDownBackColor = Primary
            End If
        Else
            control.BackColor = NeutralButtonBackground
            control.ForeColor = TextPrimary
            Dim checkBox As CheckBox = TryCast(control, CheckBox)
            If checkBox IsNot Nothing Then
                checkBox.FlatAppearance.BorderColor = NeutralButtonBorder
                checkBox.FlatAppearance.MouseOverBackColor = NeutralButtonHoverBackground
                checkBox.FlatAppearance.MouseDownBackColor = NeutralButtonPressedBackground
            End If
            Dim radioButton As RadioButton = TryCast(control, RadioButton)
            If radioButton IsNot Nothing Then
                radioButton.FlatAppearance.BorderColor = NeutralButtonBorder
                radioButton.FlatAppearance.MouseOverBackColor = NeutralButtonHoverBackground
                radioButton.FlatAppearance.MouseDownBackColor = NeutralButtonPressedBackground
            End If
        End If
    End Sub

    Public Shared Sub ApplyTextBoxStyle(ByVal textBox As TextBox)
        If textBox Is Nothing Then Return

        textBox.BackColor = CardBackground
        textBox.ForeColor = TextPrimary
        textBox.Font = UiFont
        textBox.BorderStyle = BorderStyle.FixedSingle
    End Sub

    Public Shared Sub ApplyFieldLabelStyle(ByVal label As Label)
        If label Is Nothing Then Return

        label.Font = UiFont
        label.ForeColor = TextPrimary
        label.BackColor = CardBackground
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
                button.BackColor = DangerBackground
                button.ForeColor = Danger
                button.FlatAppearance.BorderColor = Danger
            Case ButtonRole.Warning
                button.BackColor = Warning
                button.ForeColor = Color.White
                button.FlatAppearance.BorderColor = Warning
            Case Else
                button.BackColor = NeutralButtonBackground
                button.ForeColor = TextPrimary
                button.FlatAppearance.BorderColor = NeutralButtonBorder
                button.FlatAppearance.MouseOverBackColor = NeutralButtonHoverBackground
                button.FlatAppearance.MouseDownBackColor = NeutralButtonPressedBackground
        End Select
    End Sub
End Class
