<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.btnInitialRun = New System.Windows.Forms.Button()
        Me.btnRuns = New System.Windows.Forms.Button()
        Me.gbInitialRun = New System.Windows.Forms.GroupBox()
        Me.txtGrazing = New System.Windows.Forms.TextBox()
        Me.lblGrazing = New System.Windows.Forms.Label()
        Me.chkGrazing = New System.Windows.Forms.CheckBox()
        Me.txtSoilP = New System.Windows.Forms.TextBox()
        Me.lblSoilP = New System.Windows.Forms.Label()
        Me.txtMaxSlope = New System.Windows.Forms.TextBox()
        Me.lblMaxSlope = New System.Windows.Forms.Label()
        Me.lblManagement = New System.Windows.Forms.Label()
        Me.clbManagement = New System.Windows.Forms.CheckedListBox()
        Me.cbParm = New System.Windows.Forms.ComboBox()
        Me.cbControl = New System.Windows.Forms.ComboBox()
        Me.lblParm = New System.Windows.Forms.Label()
        Me.lblControl = New System.Windows.Forms.Label()
        Me.lblStates = New System.Windows.Forms.Label()
        Me.btnSimulation = New System.Windows.Forms.Button()
        Me.clBox = New System.Windows.Forms.CheckedListBox()
        Me.cbStates = New System.Windows.Forms.ComboBox()
        Me.gbRuns = New System.Windows.Forms.GroupBox()
        Me.btnSimulation1 = New System.Windows.Forms.Button()
        Me.lblRuns = New System.Windows.Forms.Label()
        Me.clbRuns = New System.Windows.Forms.CheckedListBox()
        Me.lblSoilPercentage = New System.Windows.Forms.Label()
        Me.txtSoilPercentage = New System.Windows.Forms.TextBox()
        Me.gbInitialRun.SuspendLayout()
        Me.gbRuns.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblMessage
        '
        Me.lblMessage.AutoSize = True
        Me.lblMessage.Location = New System.Drawing.Point(248, 10)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New System.Drawing.Size(0, 13)
        Me.lblMessage.TabIndex = 14
        '
        'btnInitialRun
        '
        Me.btnInitialRun.Location = New System.Drawing.Point(12, 7)
        Me.btnInitialRun.Name = "btnInitialRun"
        Me.btnInitialRun.Size = New System.Drawing.Size(75, 23)
        Me.btnInitialRun.TabIndex = 15
        Me.btnInitialRun.Text = "Initial Runs"
        Me.btnInitialRun.UseVisualStyleBackColor = True
        '
        'btnRuns
        '
        Me.btnRuns.Location = New System.Drawing.Point(115, 5)
        Me.btnRuns.Name = "btnRuns"
        Me.btnRuns.Size = New System.Drawing.Size(75, 23)
        Me.btnRuns.TabIndex = 16
        Me.btnRuns.Text = "List of Runs"
        Me.btnRuns.UseVisualStyleBackColor = True
        '
        'gbInitialRun
        '
        Me.gbInitialRun.Controls.Add(Me.txtSoilPercentage)
        Me.gbInitialRun.Controls.Add(Me.lblSoilPercentage)
        Me.gbInitialRun.Controls.Add(Me.txtGrazing)
        Me.gbInitialRun.Controls.Add(Me.lblGrazing)
        Me.gbInitialRun.Controls.Add(Me.chkGrazing)
        Me.gbInitialRun.Controls.Add(Me.txtSoilP)
        Me.gbInitialRun.Controls.Add(Me.lblSoilP)
        Me.gbInitialRun.Controls.Add(Me.txtMaxSlope)
        Me.gbInitialRun.Controls.Add(Me.lblMaxSlope)
        Me.gbInitialRun.Controls.Add(Me.lblManagement)
        Me.gbInitialRun.Controls.Add(Me.clbManagement)
        Me.gbInitialRun.Controls.Add(Me.cbParm)
        Me.gbInitialRun.Controls.Add(Me.cbControl)
        Me.gbInitialRun.Controls.Add(Me.lblParm)
        Me.gbInitialRun.Controls.Add(Me.lblControl)
        Me.gbInitialRun.Controls.Add(Me.lblStates)
        Me.gbInitialRun.Controls.Add(Me.btnSimulation)
        Me.gbInitialRun.Controls.Add(Me.clBox)
        Me.gbInitialRun.Controls.Add(Me.cbStates)
        Me.gbInitialRun.Location = New System.Drawing.Point(15, 34)
        Me.gbInitialRun.Name = "gbInitialRun"
        Me.gbInitialRun.Size = New System.Drawing.Size(547, 534)
        Me.gbInitialRun.TabIndex = 17
        Me.gbInitialRun.TabStop = False
        Me.gbInitialRun.Text = "Initial Runs"
        '
        'txtGrazing
        '
        Me.txtGrazing.Location = New System.Drawing.Point(321, 198)
        Me.txtGrazing.Name = "txtGrazing"
        Me.txtGrazing.Size = New System.Drawing.Size(68, 20)
        Me.txtGrazing.TabIndex = 30
        Me.txtGrazing.Text = "0.01"
        Me.txtGrazing.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.txtGrazing.Visible = False
        '
        'lblGrazing
        '
        Me.lblGrazing.AutoSize = True
        Me.lblGrazing.Location = New System.Drawing.Point(225, 202)
        Me.lblGrazing.Name = "lblGrazing"
        Me.lblGrazing.Size = New System.Drawing.Size(93, 13)
        Me.lblGrazing.TabIndex = 29
        Me.lblGrazing.Text = "Grazing Limit(t/ha)"
        Me.lblGrazing.Visible = False
        '
        'chkGrazing
        '
        Me.chkGrazing.AutoSize = True
        Me.chkGrazing.Location = New System.Drawing.Point(155, 201)
        Me.chkGrazing.Name = "chkGrazing"
        Me.chkGrazing.Size = New System.Drawing.Size(68, 17)
        Me.chkGrazing.TabIndex = 28
        Me.chkGrazing.Text = "Grazing?"
        Me.chkGrazing.UseVisualStyleBackColor = True
        '
        'txtSoilP
        '
        Me.txtSoilP.Location = New System.Drawing.Point(323, 164)
        Me.txtSoilP.Name = "txtSoilP"
        Me.txtSoilP.Size = New System.Drawing.Size(68, 20)
        Me.txtSoilP.TabIndex = 27
        Me.txtSoilP.Text = "3"
        Me.txtSoilP.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblSoilP
        '
        Me.lblSoilP.AutoSize = True
        Me.lblSoilP.Location = New System.Drawing.Point(152, 167)
        Me.lblSoilP.Name = "lblSoilP"
        Me.lblSoilP.Size = New System.Drawing.Size(62, 13)
        Me.lblSoilP.TabIndex = 26
        Me.lblSoilP.Text = "Enter Soil P"
        '
        'txtMaxSlope
        '
        Me.txtMaxSlope.Location = New System.Drawing.Point(323, 135)
        Me.txtMaxSlope.Name = "txtMaxSlope"
        Me.txtMaxSlope.Size = New System.Drawing.Size(68, 20)
        Me.txtMaxSlope.TabIndex = 25
        Me.txtMaxSlope.Text = "50"
        Me.txtMaxSlope.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblMaxSlope
        '
        Me.lblMaxSlope.AutoSize = True
        Me.lblMaxSlope.Location = New System.Drawing.Point(152, 135)
        Me.lblMaxSlope.Name = "lblMaxSlope"
        Me.lblMaxSlope.Size = New System.Drawing.Size(108, 13)
        Me.lblMaxSlope.TabIndex = 24
        Me.lblMaxSlope.Text = "Enter Max. Slope (in.)"
        '
        'lblManagement
        '
        Me.lblManagement.AutoSize = True
        Me.lblManagement.Location = New System.Drawing.Point(394, 49)
        Me.lblManagement.Name = "lblManagement"
        Me.lblManagement.Size = New System.Drawing.Size(126, 13)
        Me.lblManagement.TabIndex = 23
        Me.lblManagement.Text = "Select Management Files"
        '
        'clbManagement
        '
        Me.clbManagement.CheckOnClick = True
        Me.clbManagement.FormattingEnabled = True
        Me.clbManagement.Location = New System.Drawing.Point(397, 68)
        Me.clbManagement.Name = "clbManagement"
        Me.clbManagement.Size = New System.Drawing.Size(144, 454)
        Me.clbManagement.TabIndex = 22
        '
        'cbParm
        '
        Me.cbParm.FormattingEnabled = True
        Me.cbParm.Location = New System.Drawing.Point(212, 105)
        Me.cbParm.Name = "cbParm"
        Me.cbParm.Size = New System.Drawing.Size(179, 21)
        Me.cbParm.TabIndex = 21
        '
        'cbControl
        '
        Me.cbControl.FormattingEnabled = True
        Me.cbControl.Location = New System.Drawing.Point(212, 66)
        Me.cbControl.Name = "cbControl"
        Me.cbControl.Size = New System.Drawing.Size(179, 21)
        Me.cbControl.TabIndex = 20
        '
        'lblParm
        '
        Me.lblParm.AutoSize = True
        Me.lblParm.Location = New System.Drawing.Point(152, 89)
        Me.lblParm.Name = "lblParm"
        Me.lblParm.Size = New System.Drawing.Size(83, 13)
        Me.lblParm.TabIndex = 19
        Me.lblParm.Text = "Select Parm File"
        '
        'lblControl
        '
        Me.lblControl.AutoSize = True
        Me.lblControl.Location = New System.Drawing.Point(152, 50)
        Me.lblControl.Name = "lblControl"
        Me.lblControl.Size = New System.Drawing.Size(92, 13)
        Me.lblControl.TabIndex = 18
        Me.lblControl.Text = "Select Control File"
        '
        'lblStates
        '
        Me.lblStates.AutoSize = True
        Me.lblStates.Location = New System.Drawing.Point(16, 50)
        Me.lblStates.Name = "lblStates"
        Me.lblStates.Size = New System.Drawing.Size(65, 13)
        Me.lblStates.TabIndex = 17
        Me.lblStates.Text = "Select State"
        '
        'btnSimulation
        '
        Me.btnSimulation.Location = New System.Drawing.Point(16, 19)
        Me.btnSimulation.Name = "btnSimulation"
        Me.btnSimulation.Size = New System.Drawing.Size(121, 23)
        Me.btnSimulation.TabIndex = 16
        Me.btnSimulation.Text = "Run simulations"
        Me.btnSimulation.UseVisualStyleBackColor = True
        '
        'clBox
        '
        Me.clBox.CheckOnClick = True
        Me.clBox.FormattingEnabled = True
        Me.clBox.Location = New System.Drawing.Point(16, 96)
        Me.clBox.Name = "clBox"
        Me.clBox.Size = New System.Drawing.Size(120, 424)
        Me.clBox.TabIndex = 15
        '
        'cbStates
        '
        Me.cbStates.FormattingEnabled = True
        Me.cbStates.Location = New System.Drawing.Point(16, 69)
        Me.cbStates.Name = "cbStates"
        Me.cbStates.Size = New System.Drawing.Size(121, 21)
        Me.cbStates.TabIndex = 14
        '
        'gbRuns
        '
        Me.gbRuns.Controls.Add(Me.btnSimulation1)
        Me.gbRuns.Controls.Add(Me.lblRuns)
        Me.gbRuns.Controls.Add(Me.clbRuns)
        Me.gbRuns.Location = New System.Drawing.Point(15, 36)
        Me.gbRuns.Name = "gbRuns"
        Me.gbRuns.Size = New System.Drawing.Size(222, 534)
        Me.gbRuns.TabIndex = 18
        Me.gbRuns.TabStop = False
        Me.gbRuns.Text = "List of Runs"
        Me.gbRuns.Visible = False
        '
        'btnSimulation1
        '
        Me.btnSimulation1.Location = New System.Drawing.Point(13, 19)
        Me.btnSimulation1.Name = "btnSimulation1"
        Me.btnSimulation1.Size = New System.Drawing.Size(121, 23)
        Me.btnSimulation1.TabIndex = 26
        Me.btnSimulation1.Text = "Run simulations"
        Me.btnSimulation1.UseVisualStyleBackColor = True
        '
        'lblRuns
        '
        Me.lblRuns.AutoSize = True
        Me.lblRuns.Location = New System.Drawing.Point(10, 52)
        Me.lblRuns.Name = "lblRuns"
        Me.lblRuns.Size = New System.Drawing.Size(115, 13)
        Me.lblRuns.TabIndex = 25
        Me.lblRuns.Text = "Select Run to Simulate"
        '
        'clbRuns
        '
        Me.clbRuns.CheckOnClick = True
        Me.clbRuns.FormattingEnabled = True
        Me.clbRuns.Location = New System.Drawing.Point(13, 71)
        Me.clbRuns.Name = "clbRuns"
        Me.clbRuns.Size = New System.Drawing.Size(144, 454)
        Me.clbRuns.TabIndex = 24
        '
        'lblSoilPercentage
        '
        Me.lblSoilPercentage.AutoSize = True
        Me.lblSoilPercentage.Location = New System.Drawing.Point(152, 235)
        Me.lblSoilPercentage.Name = "lblSoilPercentage"
        Me.lblSoilPercentage.Size = New System.Drawing.Size(131, 13)
        Me.lblSoilPercentage.TabIndex = 31
        Me.lblSoilPercentage.Text = "Soil Percentage to Upload"
        '
        'txtSoilPercentage
        '
        Me.txtSoilPercentage.Location = New System.Drawing.Point(321, 235)
        Me.txtSoilPercentage.Name = "txtSoilPercentage"
        Me.txtSoilPercentage.Size = New System.Drawing.Size(68, 20)
        Me.txtSoilPercentage.TabIndex = 32
        Me.txtSoilPercentage.Text = "100"
        Me.txtSoilPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(985, 580)
        Me.Controls.Add(Me.gbInitialRun)
        Me.Controls.Add(Me.btnRuns)
        Me.Controls.Add(Me.btnInitialRun)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.gbRuns)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.gbInitialRun.ResumeLayout(False)
        Me.gbInitialRun.PerformLayout()
        Me.gbRuns.ResumeLayout(False)
        Me.gbRuns.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents btnInitialRun As System.Windows.Forms.Button
    Friend WithEvents btnRuns As System.Windows.Forms.Button
    Friend WithEvents gbInitialRun As System.Windows.Forms.GroupBox
    Friend WithEvents txtSoilP As System.Windows.Forms.TextBox
    Friend WithEvents lblSoilP As System.Windows.Forms.Label
    Friend WithEvents txtMaxSlope As System.Windows.Forms.TextBox
    Friend WithEvents lblMaxSlope As System.Windows.Forms.Label
    Friend WithEvents lblManagement As System.Windows.Forms.Label
    Friend WithEvents clbManagement As System.Windows.Forms.CheckedListBox
    Friend WithEvents cbParm As System.Windows.Forms.ComboBox
    Friend WithEvents cbControl As System.Windows.Forms.ComboBox
    Friend WithEvents lblParm As System.Windows.Forms.Label
    Friend WithEvents lblControl As System.Windows.Forms.Label
    Friend WithEvents lblStates As System.Windows.Forms.Label
    Friend WithEvents btnSimulation As System.Windows.Forms.Button
    Friend WithEvents clBox As System.Windows.Forms.CheckedListBox
    Friend WithEvents cbStates As System.Windows.Forms.ComboBox
    Friend WithEvents gbRuns As System.Windows.Forms.GroupBox
    Friend WithEvents btnSimulation1 As System.Windows.Forms.Button
    Friend WithEvents lblRuns As System.Windows.Forms.Label
    Friend WithEvents clbRuns As System.Windows.Forms.CheckedListBox
    Friend WithEvents txtGrazing As System.Windows.Forms.TextBox
    Friend WithEvents lblGrazing As System.Windows.Forms.Label
    Friend WithEvents chkGrazing As System.Windows.Forms.CheckBox
    Friend WithEvents txtSoilPercentage As System.Windows.Forms.TextBox
    Friend WithEvents lblSoilPercentage As System.Windows.Forms.Label

End Class
