Imports Microsoft.ApplicationBlocks.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Data.OleDb
Imports System.Data.Common

Public Class Form1
    Private Declare Function FatalAppExit Lib "Kernel32" (ByVal u As Short, ByVal p As String) As String
    Private Declare Function WaitForSingleObject Lib "Kernel32" (ByVal hHandle As Integer, ByVal dwMilliseconds As Integer) As Integer
    Private Declare Function GetLastError Lib "Kernel32" () As Object
    Private Declare Function CreateProcessA Lib "Kernel32" (ByVal lpApplicationName As String, ByVal lpCommandLine As String, ByVal lpProcessAttributes As Integer, ByVal lpThreadAttributes As Integer, ByVal bInheritHandles As Integer, ByVal dwCreationFlags As Integer, ByVal lpEnvironment As Integer, ByVal lpCurrentDirectory As String, ByRef lpStartupInfo As STARTUPINFO, ByRef lpProcessInformation As PROCESS_INFORMATION) As Integer
    Private Declare Function CloseHandle Lib "Kernel32" (ByVal hObject As Integer) As Integer
    Private Declare Function GetExitCodeProcess Lib "Kernel32" (ByVal hProcess As Integer, ByRef lpExitCode As Integer) As Integer
    Private Const NORMAL_PRIORITY_CLASS As Integer = &H20
    Private Const INFINITE As Short = -1

    Dim same As Boolean
    Dim currentDir As String

    Private Structure PROCESS_INFORMATION
        Dim hProcess As Integer
        Dim hThread As Integer
        Dim dwProcessID As Integer
        Dim dwThreadID As Integer
    End Structure

    Private Structure STARTUPINFO
        Dim cb As Integer
        Dim lpReserved As String
        Dim lpDesktop As String
        Dim lpTitle As String
        Dim dwX As Integer
        Dim dwY As Integer
        Dim dwXSize As Integer
        Dim dwYSize As Integer
        Dim dwXCountChars As Integer
        Dim dwYCountChars As Integer
        Dim dwFillAttribute As Integer
        Dim dwFlags As Integer
        Dim wShowWindow As Short
        Dim cbReserved2 As Short
        Dim lpReserved2 As Integer
        Dim hStdInput As Integer
        Dim hStdOutput As Integer
        Dim hStdError As Integer
    End Structure

    Public Structure layer_info
        Dim z() As Single  'line 4
        Dim bd() As Single    'line 5
        Dim san() As Single   'line 8
        Dim sil() As Single   'line 9
        Dim ph() As Single    'line 11
        Dim woc() As Single   'line 13
        Dim cec() As Single   'line 15
        Dim ssf() As Single   'line 18
        Dim bdd() As Single   'line 20
        Dim satc() As Single  'line 22
    End Structure

    Public Structure county_info
        Dim lat As Double
        Dim lon As Double
        Dim code As String
        Dim wind_code As Integer
        Dim wp1_code As Integer
        Dim wind_name As String
        Dim wp1_name As String
    End Structure

    Public Structure weather_info
        Dim name As String
        Dim initialYear As UShort
        Dim finalYear As UShort
    End Structure

    Private apex_default As String = Directory.GetCurrentDirectory() & "\APEX1"
    Private apex_current As String = Directory.GetCurrentDirectory() & "\APEX1_Current"
    Private swSoil As StreamWriter
    Private layers As layer_info
    Const IN_TO_CM = 2.54
    Const BD_MIN = 1.1 : Const BD_MAX = 1.79
    Const PH_MIN = 3.5 : Const PH_MAX = 9.0 : Const PH_DEFAULT = 7.0
    Const OM_MIN = 0.0 : Const OM_MAX = 4.3
    Const SSF_DEFAULT = 3
    Private bProcessAPEXExited As Boolean = False
    Private _result As ScenariosData.APEXResults
    Private nYearRotation As UShort = 1
    Private nStartYear As UShort
    Private Const ha_ac As Single = 2.471053814672
    Private Const ft_to_km As Single = 0.0003048
    Private Const ac_to_km As Single = 0.00404685643
    Private Const ha_to_m2 As Single = 10000
    Private Const ac_to_m2 As Single = 4046.8564224
    Private Const kg_lbs As Single = 2.204622621849
    Private Const mm_in As Single = 0.03937007874
    Private Const tha_tac As Single = 0.446   'http://bioenergy.ornl.gov/papers/misc/energy_conv.html
    Private Const in_to_mm As Single = 25.4
    Private Const lbs_to_kg As Single = 0.453592
    Private Const ac_to_ha As Single = 0.404686
    Private service As New NTTBlock.ServiceSoapClient  'Source on t-nn\E:\borrar\NTTBlock_WebService
    Private sw_log As StreamWriter = Nothing
    'Private xlApp, xlApp1, xlApp2 As Microsoft.Office.Interop.Excel.Application
    'Private wb, wb1, wb2 As Microsoft.Office.Interop.Excel.Workbook
    'Private ws, ws1, ws2 As Microsoft.Office.Interop.Excel.Worksheet
    Private row_number = 1
    Private sw_results As StreamWriter = Nothing

    Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        LoadStates()
        LoadControlFiles()
        LoadParmFiles()
        LoadManagementFiles()
    End Sub

    Public Sub LoadStates()
        Dim dt As DataTable

        Try
            dt = service.GetStates()
            cbStates.Items.Add("Select One")
            For Each state In dt.Rows
                cbStates.Items.Add(state("name").trim & " - " & state("StateAbrev"))
            Next
            cbStates.SelectedIndex = 0

        Catch ex As Exception
        End Try
    End Sub

    ReadOnly Property dbConnectString(db As String) As String
        Get
            Dim sConnectString As String = String.Empty
            Const sDBConnectionDefault As String = "Server=DBSERVER;initial catalog=DBCATALOG;persist security info=False"
            Dim sServer As String = "T-NN1"
            Dim sInstance As String = "SQLEXPRESS"
            Dim sCatalog As String = "NTTDB"
            Dim sUN As String = "sa"
            Dim sPW As String = "pass$word"
            If db = "Soil" Then
                sCatalog = "SSURGOSOILDB2014" : sServer = sServer & "1"
            End If
            sServer = sServer + "\" + sInstance
            sConnectString = sDBConnectionDefault.Replace("DBSERVER", sServer)
            sConnectString = sConnectString.Replace("DBCATALOG", sCatalog)
            sConnectString = sConnectString.ToString & ";user id=" & sUN & "; password=" & sPW
            sConnectString = sConnectString & ";Pooling=false"
            Return sConnectString
        End Get
    End Property

    Private Sub cbStates_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cbStates.SelectedIndexChanged
        Dim dt As DataTable

        dt = service.GetCounties(Split(cbStates.SelectedItem, "-")(1))
        clBox.Items.Clear()
        clBox.Items.Add("Select All")
        For Each county In dt.Rows
            clBox.Items.Add(county("name").trim)
        Next
    End Sub

    Private Function validate_inputs()
        Dim msg As String = ""
        If cbStates.SelectedIndex = 0 Then msg = "- Please select state "

        If clBox.CheckedItems.Count = 0 Then msg &= "- Please select county "

        If clBox.CheckedItems.Count = 1 And clBox.GetItemChecked(0) Then msg &= "- Please select county "

        If cbControl.SelectedItem Is Nothing Then msg &= "- Please select control file "

        If cbParm.SelectedItem Is Nothing Then msg &= "- Please select parm file "

        If clbManagement.CheckedItems.Count = 0 Then msg &= "- Please management file "

        If clbManagement.CheckedItems.Count = 1 And clbManagement.GetItemChecked(0) Then msg &= "- Please management file "

        If txtMaxSlope.Text <= "0" Then msg &= "- Please enter max. slope "

        If txtSoilP.Text <= "0" Then msg &= "- Please enter soil P "

        If chk_autoirrigation.Checked Then
            If cb_irrigation_type.SelectedIndex < 0 Then msg = "- Please select type of irrigation "
            If txtEfficiency.Text <= 0.0 Then msg = "- Please enter irrigation effiency "
            If txtInterval.Text <= 0.0 Then msg = "- Please enter irrigation interval in days "
            If txtStress.Text <= 0.0 Then msg = "- Please enter irrigation stress level to trigger irrigation "
            If txtApplication.Text <= 0.0 Then msg = "- Please Max single application in in. "
        End If

        Return msg
    End Function

    Private Sub btnSimulation_Click1(sender As System.Object, e As System.EventArgs)
        Dim county_code As String = String.Empty
        Dim county_info As county_info
        Dim weather_info As weather_info
        Dim weaterFileName As SqlDataReader = Nothing
        'Dim ssas As SqlDataReader = Nothing
        Dim ssas As DataTable = Nothing
        Dim soils As DataTable = Nothing
        Dim layer_number As UShort = 0
        Dim depth As Single = 999
        Dim series_name As String = String.Empty
        Dim soil_name As String = String.Empty
        Dim new_layer As Boolean = False
        Dim slope As Single = 0
        Dim name As String = String.Empty
        Dim key As Integer = 0
        Dim errors As Boolean = False
        Dim msg As String = "OK"
        Dim total_soils As Integer = 0
        Dim soil_number As Integer = 0
        Dim first_random As Random = New Random
        Dim next_soil As Single = 0
        Dim next_selected As Single = 0.0

        Try
            lblMessage.Text = validate_inputs()
            If lblMessage.Text <> "" Then errors = True : lblMessage.ForeColor = Color.Red : Exit Sub
            'create the excel file in memory before the simulations start.
            'xlApp = CreateObject("Excel.Application")
            'wb = xlApp.Workbooks.Add
            'ws = wb.Worksheets("Sheet1") 'Specify your worksheet name
            'create_excel_file()
            'Read all of the counties selected and take code and center coordinates.
            For i = 1 To clBox.Items.Count - 1
                If clBox.GetItemCheckState(i) = CheckState.Checked Then
                    county_info = CoutyInfo("SELECT TOP 1 * FROM County_Extended WHERE StateAbrev like '" & Split(cbStates.SelectedItem, "-")(1).Trim & "%' AND [Name] like '" & clBox.Items(i).Trim & "%' ORDER BY [Name]")
                    If county_info.lon = 0 And county_info.lat = 0 Then Continue For
                    weather_info = GetWeatherInfo(county_info.lat, county_info.lon)
                    If weather_info.name = "Error" Then Continue For
                    ssas = service.GetSSA(county_info.code)
                    APEXFolders(cbControl.SelectedItem, cbParm.SelectedItem)
                    'initialize log file
                    'sw_log = New StreamWriter(apex_current & "\log.log")
                    create_Weather_file(weather_info.name)
                    For Each ssa In ssas.Rows
                        If Not swSoil Is Nothing Then swSoil.Close()
                        layer_number = 0
                        depth = 999
                        series_name = String.Empty
                        soil_name = String.Empty
                        soil_number = 1
                        total_soils = service.GetTotalSoils(ssa("code"), county_info.code, txtMaxSlope.Text)
                        next_soil = total_soils / (total_soils * txtSoilPercentage.Text / 100)
                        next_selected = first_random.Next(1, next_soil)
                        soils = service.GetSoils(ssa("code"), county_info.code, txtMaxSlope.Text)
                        If soils.Rows.Count = 0 Then Continue For
                        series_name = soils.Rows(0)("seriesName")
                        soil_name = soils.Rows(0)("MuName")
                        'For Each soil In soils.Rows
                        For j = 0 To soils.Rows.Count - 1
                            If next_selected <= soil_number Then
                                new_layer = False
                                lblMessage.Text = "Running County => " & clBox.Items(i) & " - SSA => " & ssa("Code") & " - Soil => " & soils.Rows(j).Item("series")
                                lblMessage.ForeColor = Color.Green
                                If IsDBNull(soils.Rows(j).Item("ldep")) Then Continue For
                                If depth >= soils.Rows(j).Item("ldep") Then
                                    If series_name <> soils.Rows(j).Item("seriesName") Or soil_name <> soils.Rows(j).Item("MuName") Then
                                        next_selected += next_soil
                                        soil_number += 1
                                        layer_number = 0
                                        If Not swSoil Is Nothing Then
                                            print_layers()
                                            swSoil.Close()
                                            'create subarea
                                            create_subarea_file(slope / 100, soils.Rows(j).Item("horizgen"))
                                            'copy the operation file one by one from the management list and then run the simulation
                                            For Each mgt In clbManagement.CheckedItems
                                                If mgt.ToString.Contains("Select") Then Continue For
                                                copy_management_file(mgt)
                                                If layers.z.Length > 0 Then msg = run_apex(cbStates.SelectedItem, county_info.code, ssa("code"), name, series_name, key, slope, mgt, cbParm.SelectedItem, cbControl.SelectedItem, 0, soils.Rows(j).Item("horizgen"))
                                            Next
                                            series_name = soils.Rows(j).Item("SeriesName")
                                            soil_name = soils.Rows(j).Item("MuName")
                                        End If
                                    End If
                                Else
                                    new_layer = True
                                End If
                                'If Not (depth = soils.Rows(j).Item("ldep") And series_name = soils.Rows(j).Item("seriesName")) Then
                                If layer_number = 0 Then
                                    slope = (soils.Rows(j).Item("slopel") + soils.Rows(j).Item("slopeh")) / 2
                                    name = soils.Rows(j).Item("series")
                                    key = soils.Rows(j).Item("muid")
                                    layers = New layer_info
                                    swSoil = New StreamWriter(apex_current & "\APEX.sol")
                                End If
                                depth = soils.Rows(j).Item("ldep")
                                If new_layer Or layer_number = 0 Then
                                    layer_number += 1
                                    create_soils(soils.Rows(j), layer_number)
                                End If
                            End If
                            If series_name <> soils.Rows(j).Item("seriesName") Or soil_name <> soils.Rows(j).Item("MuName") Then
                                series_name = soils.Rows(j).Item("SeriesName")
                                soil_name = soils.Rows(j).Item("MuName")
                                soil_number += 1
                            End If
                        Next
                        swSoil.Close()
                        'sw_log.Close()
                    Next
                End If
            Next

            lblMessage.Text = "Simulations finished succesfully"
            lblMessage.ForeColor = Color.Green

        Catch ex As Exception
            'if any error send a message.
            lblMessage.Text = "Error running Simulations " & ex.Message
            lblMessage.ForeColor = Color.Red
        Finally
            If Not swSoil Is Nothing Then
                swSoil.Close()
                swSoil.Dispose()
                swSoil = Nothing
            End If

            'If Not sw_log Is Nothing Then
            '    sw_log.Close()
            '    sw_log.Dispose()
            '    sw_log = Nothing
            'End If
            If errors = False Then SaveFile(Directory.GetCurrentDirectory(), "Results.xlsx")
        End Try
    End Sub

    Private Sub btnSimulation_Click(sender As System.Object, e As System.EventArgs) Handles btnSimulation.Click
        Dim county_code As String = String.Empty
        Dim county_info As county_info
        Dim weather_info As weather_info
        Dim weaterFileName As SqlDataReader = Nothing
        'Dim ssas As SqlDataReader = Nothing
        Dim ssas As DataTable = Nothing
        Dim soils As DataTable = Nothing
        Dim layer_number As UShort = 0
        Dim depth As Single = 999
        Dim series_name As String = String.Empty
        Dim soil_name As String = String.Empty
        Dim new_layer As Boolean = False
        Dim slope As Single = 0
        Dim name As String = String.Empty
        Dim key As Integer = 0
        Dim errors As Boolean = False
        Dim msg As String = "OK"
        Dim total_soils As Integer = 0
        Dim soil_number As Integer = 0
        Dim first_random As Random = New Random
        Dim next_soil As Single = 0
        Dim next_selected As Single = 0.0
        Dim total_layers As Integer = 0
        Dim found As Boolean = False
        Dim horizgen As String = String.Empty

        Try
            sw_results = New StreamWriter(Directory.GetCurrentDirectory() & "\Results.txt")
            sw_results.AutoFlush = True
            lblMessage.Text = validate_inputs()
            If lblMessage.Text <> "" Then errors = True : lblMessage.ForeColor = Color.Red : Exit Sub
            'create the excel file in memory before the simulations start.
            create_titles_file()
            'Read all of the counties selected and take code and center coordinates.
            For Each county In clBox.CheckedItems
                If county.ToString.Contains("'s") Then county = county.ToString.Replace("'s", "")
                county_info = CoutyInfo("SELECT code, lon, lat, wcode, wname FROM County, CountyCoor, CountyWp1 WHERE StateAbrev like '" & Split(cbStates.SelectedItem, "-")(1).Trim & "%' AND [Name] like '" & county.Trim & "%' AND County.code = CountyCoor.County and County.code = CountyWp1.County")
                If county_info.lon = 0 And county_info.lat = 0 Then Continue For
                weather_info = GetWeatherInfo(county_info.lat, county_info.lon)
                If weather_info.name = "Error" Then
                    Throw New Global.System.Exception("No weather file found for this county")
                End If
                ssas = service.GetSSA(county_info.code)
                'create APEX folder
                APEXFolders(cbControl.SelectedItem, cbParm.SelectedItem)
                'Create weather file
                create_Weather_file(weather_info.name)
                'read all of the soil survey areas
                For Each ssa In ssas.Rows
                    soil_number = 1
                    'Take total number of soils for that soils survey area
                    total_soils = service.GetTotalSoils(ssa("code"), county_info.code, txtMaxSlope.Text)
                    If total_soils = 0 Then Continue For 'if there is not soils
                    next_soil = total_soils / (total_soils * txtSoilPercentage.Text / 100)
                    next_selected = first_random.Next(1, next_soil)
                    'Get the soils information for that soil survey area
                    soils = service.GetSoils(ssa("code"), county_info.code, txtMaxSlope.Text)
                    If soils.Rows.Count <= 0 Then Continue For
                    soil_name = soils.Rows(0).Item("MuName")
                    series_name = soils.Rows(0).Item("seriesName")
                    total_layers = soils.Rows.Count - 1
                    'Go through all of the layers for each soil. Controling when the soil change either soil name or soil series.
                    For j = 0 To total_layers
                        If soils.Rows(j).Item("MuName").ToString.Contains("Wintergreen") Then
                            Dim a As String
                            a = ""
                        End If
                        depth = 0
                        If next_selected <= soil_number Then
                            lblMessage.Text = "Running County => " & county & " - SSA => " & ssa("Code") & " - Soil => " & soils.Rows(j).Item("series")
                            lblMessage.ForeColor = Color.Green
                            layer_number = 0
                            slope = (soils.Rows(j).Item("slopel") + soils.Rows(j).Item("slopeh")) / 2
                            key = soils.Rows(j).Item("Muid")
                            name = soils.Rows(j).Item("series")
                            horizgen = soils.Rows(j).Item("horizgen")
                            layers = New layer_info
                            swSoil = New StreamWriter(apex_current & "\APEX.sol")
                            found = False
                            Do While soil_name = soils.Rows(j).Item("MuName") And series_name = soils.Rows(j).Item("seriesName")
                                If Not (soils.Rows(j).Item("ldep") Is Nothing Or IsDBNull(soils.Rows(j).Item("ldep"))) Then  'controls if the the same depth is for the current layer to not use it.
                                    If depth < soils.Rows(j).Item("ldep") Then
                                        layer_number += 1
                                        If layer_number <= 10 Then create_soils(soils.Rows(j), layer_number)
                                        depth = soils.Rows(j).Item("ldep")
                                    End If
                                End If
                                j += 1
                                found = True
                                If j > total_layers Then Exit Do
                            Loop
                            print_layers()  'print layers for the current sooi in the APEX soil file
                            swSoil.Close()
                            swSoil.Dispose()
                            swSoil = Nothing
                            create_subarea_file(slope / 100, horizgen) 'create subarea file.
                            'create operation files for each management selected
                            For Each mgt In clbManagement.CheckedItems
                                If mgt.ToString.Contains("Select") Then Continue For
                                copy_management_file(mgt)
                                If Not (name.ToLower.Contains("urban") Or series_name.ToLower.Contains("urban")) AndAlso layer_number > 0 AndAlso layers.z.Length > 0 Then msg = run_apex(cbStates.SelectedItem, county_info.code, ssa("code"), name, series_name, key, slope, mgt, cbParm.SelectedItem, cbControl.SelectedItem, 0, horizgen)
                            Next
                            next_selected += next_soil
                        End If
                        If j > total_layers Then Exit For
                        If soil_name <> soils.Rows(j).Item("MuName") Or series_name <> soils.Rows(j).Item("seriesName") Then
                            soil_name = soils.Rows(j).Item("MuName")
                            series_name = soils.Rows(j).Item("seriesName")
                            soil_number += 1
                            If found Then j -= 1 'reduce j because the for will increase it again
                        End If
                    Next
                Next
            Next
            lblMessage.Text = "Simulations finished succesfully"
            lblMessage.ForeColor = Color.Green

        Catch ex As Exception
            'if any error send a message.
            lblMessage.Text = "Error running Simulations " & ex.Message & " - Soil Name: " & soil_name & " - Soil Series: " & series_name
            lblMessage.ForeColor = Color.Red
        Finally
            If Not swSoil Is Nothing Then
                swSoil.Close()
                swSoil.Dispose()
                swSoil = Nothing
            End If

            If Not sw_results Is Nothing Then
                sw_results.Close()
                sw_results.Dispose()
                sw_results = Nothing
            End If
            'If errors = False Then SaveFile(Directory.GetCurrentDirectory(), "Results.xlsx")
        End Try
    End Sub

    Public Function CoutyInfo(sSql As String) As county_info
        Dim c_i As New county_info
        Dim dr As DataTable
        dr = service.GetRecord(sSql)
        If dr.Rows.Count = 0 Then
            c_i.lat = 0 : c_i.lon = 0
        Else
            c_i.lat = dr.Rows(0).Item("lat")
            c_i.lon = dr.Rows(0).Item("lon")
            c_i.code = dr.Rows(0).Item("code")
            'c_i.wind_code = dr.Rows(0).Item("windName").ToString.Split(",")(0)
            'c_i.wind_name = dr.Rows(0).Item("windName").ToString.Split(",")(1)
            'c_i.wp1_code = dr.Rows(0).Item("wp1Name").ToString.Split(",")(0)
            'c_i.wp1_name = dr.Rows(0).Item("wp1Name").ToString.Split(",")(1)
            c_i.wind_code = dr.Rows(0).Item("wcode")
            c_i.wind_name = dr.Rows(0).Item("wName").ToString.Trim
            c_i.wp1_code = dr.Rows(0).Item("wcode")
            c_i.wp1_name = dr.Rows(0).Item("wName").ToString.Trim
        End If
        Return c_i
    End Function

    Private Sub clBox_Click(sender As Object, e As System.EventArgs) Handles clBox.SelectedIndexChanged
        If sender.selectedIndex = 0 Then
            For i = 1 To clBox.Items.Count - 1
                clBox.SetItemChecked(i, sender.GetItemCheckState(0))
            Next
        End If
    End Sub

    Public Function GetWeatherInfo(nlat As Single, nlon As Single) As weather_info
        Dim w_i As New weather_info
        Const latDif As Single = 0.04
        Const lonDif As Single = 0.09
        Dim latLess, latPlus, lonLess, lonPlus As Double
        Dim weather2013 As String = "E:\Weather\weatherFiles\US"
        Dim weather2015 As String = "C:\Weather\weatherFiles\1981-2015"

        latLess = nlat - latDif : latPlus = nlat + latDif
        lonLess = nlon - lonDif : lonPlus = nlon + lonDif
        Dim weatherFileQuery As DataTable = service.GetWeatherfileName(nlat, nlon, latLess, latPlus, lonLess, lonPlus)
        If weatherFileQuery.Rows.Count > 0 Then
            w_i.finalYear = weatherFileQuery.Rows(0).Item("finalYear")
            w_i.initialYear = weatherFileQuery.Rows(0).Item("initialYear")
            If w_i.finalYear < 2015 Then
                w_i.name = weather2013 & "\" & weatherFileQuery.Rows(0).Item("fileName")
            Else
                w_i.name = weather2015 & "\" & weatherFileQuery.Rows(0).Item("fileName")
            End If
        Else
            w_i.name = "Error"
            w_i.finalYear = 0
            w_i.initialYear = 0
        End If
        Return w_i
    End Function

    Public Sub LoadControlFiles()
        Dim control_files() As String = Directory.GetFiles(Directory.GetCurrentDirectory() & "\" & "APEX1", "ApexCont_*.dat")
        For Each file In control_files
            cbControl.Items.Add(Path.GetFileName(file))
        Next
    End Sub

    Public Sub LoadParmFiles()
        Dim parm_files() As String = Directory.GetFiles(Directory.GetCurrentDirectory() & "\" & "APEX1", "Parm_*.dat")
        For Each file In parm_files
            cbParm.Items.Add(Path.GetFileName(file))
        Next
    End Sub

    Public Sub LoadManagementFiles()
        Dim mgt_files() As String = Directory.GetFiles(Directory.GetCurrentDirectory() & "\" & "APEX1", "*.opc")
        clbManagement.Items.Clear()
        clbManagement.Items.Add("Select All")
        For Each file In mgt_files
            clbManagement.Items.Add(Path.GetFileName(file))
        Next
    End Sub

    Private Sub clbManagement_Click(sender As Object, e As System.EventArgs) Handles clbManagement.SelectedIndexChanged
        'Dim checked As CheckState = CheckState.Checked

        If sender.selectedIndex = 0 Then
            'If sender.GetItemCheckState(0) = CheckState.Checked Then
            '    checked = CheckState.Unchecked
            'End If
            For i = 1 To clbManagement.Items.Count - 1
                clbManagement.SetItemChecked(i, sender.GetItemCheckState(0))
            Next
        End If
        'If sender.GetItemCheckState(sender.selectedIndex) = CheckState.Checked Then
        '    checked = CheckState.Unchecked
        'End If
        'clbManagement.SetItemChecked(sender.selectedIndex, checked)

    End Sub

    Private Sub APEXFolders(control_file As String, parm_file As String)
        Dim directoryFiles(), currentFile As String
        Dim textFile As String
        'Create APEX folder to run the current simulation
        If Directory.Exists(apex_current) = True Then
            directoryFiles = Directory.GetFiles(apex_current)
            For Each textFile In directoryFiles
                File.Delete(textFile)
            Next
            'Directory.Delete(apex_current, True)
        Else
            Directory.CreateDirectory(apex_current)
        End If
        'Do
        '    If Not Directory.Exists(apex_current) Then Exit Do
        'Loop
        directoryFiles = Directory.GetFiles(apex_default)
        For Each textFile In directoryFiles
            currentFile = Path.GetFileName(textFile)
            If Not (currentFile.ToLower = "apexcont.dat" Or currentFile.ToLower = "parm.dat") Then
                Select Case True
                    Case currentFile.ToLower = control_file.ToLower
                        File.Copy(textFile, apex_current & "\apexcont.dat", True)
                    Case currentFile.ToLower = parm_file.ToLower
                        File.Copy(textFile, apex_current & "\parm.dat", True)
                    Case Else
                        File.Copy(textFile, apex_current & "\" & currentFile, True)
                End Select
            End If
        Next
    End Sub

    Private Sub create_Weather_file(name As String)
        Dim weather_file() As String
        Dim sw As New StreamWriter(apex_current & "\APEX.wth", False)
        Dim first As Boolean = True

        Try
            weather_file = service.GetWeather(name)
            For Each item In weather_file
                If first = True Then
                    nStartYear = item.Substring(0, 6)
                    first = False
                End If
                sw.WriteLine(item)
            Next

            sw.Close()
            sw.Dispose()
            sw = Nothing

            Create_wp1_from_weather(apex_current, "CHINAG", "APEX")
        Catch ex As Exception
        Finally
            If Not sw Is Nothing Then
                sw.Close()
                sw.Dispose()
                sw = Nothing
            End If
        End Try
    End Sub

    'Private Function LoadSSA(county_code As String) As SqlDataReader
    '    Dim sSQL As String = String.Empty
    '    Dim dr As SqlDataReader = Nothing

    '    Try
    '        sSQL = "SELECT Code FROM SSArea WHERE CountyCode = '" & county_code & "' ORDER BY [Code]"
    '        dr = SqlHelper.ExecuteReader(dbConnectString("No"), CommandType.Text, sSQL)
    '        Return dr

    '    Catch ex As Exception
    '        Return dr
    '    End Try
    'End Function

    'Private Function LoadSoils(ssa As String, county_code As String) As SqlDataReader
    '    Dim sSQL As String = String.Empty
    '    Dim dr As SqlDataReader = Nothing

    '    Try
    '        sSQL = "SELECT * FROM " & county_code.Substring(0, 2) & "SOILS WHERE TSSSACode = '" & ssa & "' AND ldep <= " & txtMaxSlope.Text & "  ORDER BY [TSSSACode]"
    '        dr = SqlHelper.ExecuteReader(dbConnectString("Soil"), CommandType.Text, sSQL)
    '        Return dr

    '    Catch ex As Exception
    '        Return dr
    '    End Try
    'End Function

    Private Sub create_soils(soil As DataRow, layer_number As UShort)
        'added to control when information is not available
        Dim texture() As String = {"sandy clay loam", "silty clay loam", "loamy sand", "sandy loam", "sandy clay", "silt loam", "clay loam", "silty clay", "sand", "loam", "silt", "clay"}
        Dim sands() As Single = {53.2, 8.9, 80.2, 63.4, 52, 15, 29.1, 7.7, 84.6, 41.2, 4.9, 12.7}
        Dim silts() As Single = {20.6, 58.9, 14.6, 26.3, 6, 67, 39.3, 45.8, 11.1, 40.2, 85, 32.7}
        Dim satcs() As Single = {9.24, 11.4, 94.66, 48.01, 0.8, 15.55, 7.74, 5.29, 107.83, 19.98, 10.64, 2.1}
        Dim bds() As Single = {1.49, 1.2, 1.44, 1.46, 1.49, 1.31, 1.33, 1.21, 1.45, 1.4, 1.42, 1.24}
        Dim j As UShort
        'determine values for sand, silt, and bd depending on the texture just in case they are needed due to lack of information from soil database
        For j = 0 To 11
            If soil.Item("Textr").ToLower.Contains(texture(j)) Then Exit For
        Next

        If layer_number > 10 Then Exit Sub
        If layer_number = 1 Then 'create the first three lines of the soil file
            swSoil.WriteLine(" .sol file Soil:APEX.sol  Date:" & Date.Now.ToString & "  Soil Name: " & soil("Muname"))
            swSoil.Write("{0,8:N2}", soil.Item("Albedo"))
            ' find the corresponding soil group
            Select Case soil("Horizdesc1").ToString.Substring(0, 1)
                Case "A"
                    swSoil.Write("{0,8:N2}", 1)
                Case "B"
                    swSoil.Write("{0,8:N2}", 2)
                Case "C"
                    swSoil.Write("{0,8:N2}", 3)
                Case "D"
                    swSoil.Write("{0,8:N2}", 4)
                Case Else
                    swSoil.Write("{0,8:N2}", 2)
            End Select
            swSoil.WriteLine("    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            swSoil.WriteLine("   10.00    1.00    0.00    0.00    0.00    0.00    0.00")
        End If
        'Prepare layers
        If soil("ldep") Is Nothing Or soil("ldep") <= 0 Or IsDBNull(soil("ldep")) Then Exit Sub
        ReDim Preserve layers.z(layer_number - 1)
        layers.z(layer_number - 1) = soil("ldep") * IN_TO_CM

        ReDim Preserve layers.bd(layer_number - 1)
        If Not IsDBNull(soil("bd")) AndAlso Not soil("bd") <= 0 Then
            layers.bd(layer_number - 1) = soil("bd")
        Else
            If layer_number - 1 > 1 Then
                layers.bd(layer_number - 1) = layers.bd(layer_number - 1 - 2)
            Else
                If j <= 11 Then
                    layers.bd(layer_number - 1) = bds(j)
                Else
                    layers.bd(layer_number - 1) = BD_MIN
                End If
            End If
        End If
        If layers.bd(layer_number - 1) < BD_MIN Then layers.bd(layer_number - 1) = BD_MIN
        If layers.bd(layer_number - 1) > BD_MAX Then layers.bd(layer_number - 1) = BD_MAX

        ReDim Preserve layers.san(layer_number - 1)
        If Not IsDBNull(soil("sand")) AndAlso Not soil("sand") <= 0 Then
            layers.san(layer_number - 1) = soil("sand")
        Else
            If layer_number - 1 > 1 Then
                layers.san(layer_number - 1) = layers.san(layer_number - 1 - 2)
            Else
                If j <= 11 Then
                    layers.san(layer_number - 1) = sands(j)
                Else
                    layers.san(layer_number - 1) = 0
                End If
            End If
        End If

        ReDim Preserve layers.sil(layer_number - 1)
        If Not IsDBNull(soil("silt")) AndAlso Not soil("silt") <= 0 Then
            layers.sil(layer_number - 1) = soil("silt")
        Else
            If layer_number - 1 > 1 Then
                layers.sil(layer_number - 1) = layers.sil(layer_number - 1 - 2)
            Else
                If j <= 11 Then
                    layers.sil(layer_number - 1) = silts(j)
                Else
                    layers.sil(layer_number - 1) = 0
                End If
            End If
        End If

        ReDim Preserve layers.ph(layer_number - 1)
        If Not IsDBNull(soil("ph")) AndAlso Not soil("ph") <= 0 Then
            layers.ph(layer_number - 1) = soil("ph")
        Else
            If layer_number - 1 > 1 Then
                layers.ph(layer_number - 1) = layers.ph(layer_number - 1 - 2)
            Else
                layers.ph(layer_number - 1) = PH_DEFAULT
            End If
        End If
        If layers.ph(layer_number - 1) < PH_MIN Then layers.ph(layer_number - 1) = PH_MIN
        If layers.ph(layer_number - 1) > PH_MAX Then layers.ph(layer_number - 1) = PH_MAX

        ReDim Preserve layers.woc(layer_number - 1)
        If Not IsDBNull(soil("om")) AndAlso Not soil("om") <= 0 Then
            layers.woc(layer_number - 1) = soil("om")
        Else
            If layer_number - 1 > 1 Then
                layers.woc(layer_number - 1) = layers.woc(layer_number - 1 - 2)
            Else
                layers.woc(layer_number - 1) = OM_MIN
            End If
        End If
        If layers.woc(layer_number - 1) < OM_MIN Then layers.woc(layer_number - 1) = OM_MIN
        If layers.woc(layer_number - 1) > OM_MAX Then layers.woc(layer_number - 1) = OM_MAX

        'ReDim Preserve layers.cec(layer_number - 1)
        'If Not IsDBNull(soil("cec") AndAlso Not soil("cec") <= 0) Then
        '    layers.cec(layer_number - 1) = soil("cec")
        'Else
        '    If layer_number - 1 > 1 Then
        '        layers.cec(layer_number - 1) = layers.cec(layer_number - 1 - 2)
        '    End If
        'End If

        ReDim Preserve layers.ssf(layer_number - 1)
        If layer_number - 1 = 1 Then
            layers.ssf(layer_number - 1) = txtSoilP.Text
        Else
            layers.ssf(layer_number - 1) = SSF_DEFAULT
        End If

        ReDim Preserve layers.bdd(layer_number - 1)
        layers.bdd(layer_number - 1) = layers.bd(layer_number - 1)

        ReDim Preserve layers.satc(layer_number - 1)
        If Not IsDBNull(soil("ksat")) AndAlso Not soil("ksat") <= 0 Then
            layers.satc(layer_number - 1) = soil("ksat")
        Else
            If layer_number - 1 > 1 Then
                layers.satc(layer_number - 1) = layers.satc(layer_number - 1 - 2)
            Else
                If j <= 11 Then
                    layers.satc(layer_number - 1) = satcs(j)
                Else
                    layers.satc(layer_number - 1) = 0
                End If
            End If
        End If

    End Sub

    Private Sub print_layers()
        Dim twice As Boolean = False
        Try
            'sw_log.WriteLine("start Printing Layers")
            'if there are layers those are printed for the current soil before it goes to the nex soil
            If layers.z.Length > 0 Then
                If Math.Round((layers.z(0) / 100), 2) > 10 / 100 Then  'if first layer is > than 10 cm then and new layer is added at the begining
                    twice = True
                End If
                If twice = True Then
                    swSoil.Write("{0,8:N2}", 10 / 100)
                    'sw_log.Write("{0,8:N2}", 10 / 100)
                End If
                For Each z In layers.z  'depth
                    swSoil.Write("{0,8:N2}", z / 100)
                    'sw_log.Write("{0,8:N2}", 10 / 100)
                Next
                swSoil.WriteLine()
                'sw_log.WriteLine("{0,8:N2}", 10 / 100)

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.bd(0))
                End If
                For Each item In layers.bd    'db
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'uw
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'fc
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.san(0))
                End If
                For Each item In layers.san 'sand
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.sil(0))
                End If
                For Each item In layers.sil 'silt
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'wn
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.ph(0))
                End If
                For Each item In layers.ph  'ph
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'smb
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.woc(0))
                End If
                For Each item In layers.woc  'woc
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'cac
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'cec
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'rok
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'cnds
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.ssf(0))
                End If
                For Each item In layers.ssf  'ssf
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'rsd
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.bdd(0))
                End If
                For Each item In layers.bdd  'bdd
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'psp
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.satc(0))
                End If
                For Each item In layers.satc  'satc
                    swSoil.Write("{0,8:N2}", item)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'hcl
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()

                If twice = True Then
                    swSoil.Write("{0,8:N2}", 0)
                End If
                For Each item In layers.z  'hcl
                    swSoil.Write("{0,8:N2}", 0)
                Next
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                swSoil.WriteLine()
                'sw_log.WriteLine("end printing layers")
            End If
        Catch ex As Exception
            'sw_log.WriteLine("Error printing layers => " & ex.Message)
        End Try

    End Sub

    Private Sub create_subarea_file(slope As Single, horizgen As String)
        Dim swSubarea As New StreamWriter(apex_current & "\APEX.sub", False)
        Dim slope_length As Single = calcSlopeLength(slope * 100)
        Try
            swSubarea.WriteLine("       10000000000000000  .sub file Subbasin:1  Date: " & Date.Now)
            swSubarea.WriteLine("   1   1   1   0   0   0   0   1   0   0   0   0")
            swSubarea.WriteLine("    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            swSubarea.Write("   40.00  0.6361    0.00    0.00    0.00")
            swSubarea.Write("{0,8:N4}{1,8:N4}", slope, slope_length)
            swSubarea.WriteLine("    0.00    0.00    0.00")
            swSubarea.WriteLine("  0.6361    0.00    0.00    0.00    0.00    0.00  0.2000  0.2000       0  0.0000")
            swSubarea.WriteLine("    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            swSubarea.WriteLine("   0.000    0.00    0.00    0.00    0.00    0.00   0.000    0.00    0.00    0.00    0.00    0.00    0.00")
            'changed on 3/20/18 to use autoirrigation just when the check box chk_autoirrigation is checked.
            If chk_autoirrigation.Checked Then
                Select Case cb_irrigation_type.SelectedIndex
                    Case 0  'Sprinkler
                        swSubarea.Write("  01" & txtInterval.Text.PadLeft(4)) 'And 500 in operation file
                    Case 1  'Furrow/Flood
                        swSubarea.Write("  02" & txtInterval.Text.PadLeft(4)) 'And 502 in operation file
                    Case 2  'Drip
                        swSubarea.Write("  05" & txtInterval.Text.PadLeft(4))  'And 530 in operation file
                    Case 3  'Furrow Diking
                        swSubarea.Write("  02" & txtInterval.Text.PadLeft(4)) 'And 502 in operation file. The difference with Furrow/Flood is the efficiency
                End Select
            Else
                swSubarea.Write("  00   0")
            End If
            'changed on 4/9/18 to use tile drain when the check box chk_tile_drain is checked and when drainage type is poorly only. If not poorly never tile drain is set up or if check box not checked.
            If chk_tile_drain.Checked And horizgen.ToLower.Contains("poorly") Then
                swSubarea.WriteLine("   0   0   0" & txt_tile_drain.Text.PadLeft(4) & "   0   0   0   0   0")
            Else
                swSubarea.WriteLine("   0   0   0   0   0   0   0   0   0")
            End If
            'changed on 3/20/18 to use autoirrigation just when the check box chk_autoirrigation is checked.
            If chk_autoirrigation.Checked Then
                swSubarea.Write(((100 - txtStress.Text) / 100).ToString("####0.00").PadLeft(8))
                swSubarea.Write(((100 - txtEfficiency.Text) / 100).ToString("####0.00").PadLeft(8))
                swSubarea.Write(" 5000.00")
                swSubarea.Write("    0.00" & (txtApplication.Text * in_to_mm).ToString("####0.00").PadLeft(8))
                swSubarea.WriteLine("    0.00    0.00    0.00    0.00    0.00")
            Else
                swSubarea.WriteLine("    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            End If
            swSubarea.WriteLine("    1.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            If chkGrazing.Checked Then
                swSubarea.WriteLine("   1   0   0   0")
                swSubarea.WriteLine(txtGrazing.Text.PadLeft(8) & "    0.00    0.00    0.00")
            Else
                swSubarea.WriteLine("   0   0   0   0")
                swSubarea.WriteLine("    0.00    0.00    0.00    0.00")
            End If

        Catch ex As Exception
        Finally
            If Not swSubarea Is Nothing Then
                swSubarea.Close()
                swSubarea.Dispose()
                swSubarea = Nothing
            End If
        End Try
    End Sub

    Private Function run_apex(state As String, county As String, ssa As String, soil_name As String, soil_component As String, soil_key As Integer, slope As Single, mgt As String, parm_file As String, control_file As String, i As UShort, drainType As String) As String
        Dim APEXResults1 As ScenariosData.APEXResults
        Dim msg = "OK"
        Try
            'run the current simualtion
            'Create bat file to run apex for Baseline
            Dim swfile As New StreamWriter(File.OpenWrite(apex_current & "\RunAPEX.bat"))
            swfile.WriteLine(apex_current.Substring(0, 2))  'take drive
            swfile.WriteLine("cd\")
            swfile.WriteLine("Cd " & apex_current)
            swfile.WriteLine("APEX0806.exe")
            swfile.WriteLine(" ")
            swfile.Close()
            swfile.Dispose()
            swfile = Nothing
            'delete previous runs
            If File.Exists(apex_current & "\APEX001.ACY") Then File.Delete(apex_current & "\APEX001.ACY")
            If File.Exists(apex_current & "\APEX001.ASA") Then File.Delete(apex_current & "\APEX001.ASA")
            If File.Exists(apex_current & "\APEX001.AWS") Then File.Delete(apex_current & "\APEX001.AWS")
            If File.Exists(apex_current & "\APEX001.MSW") Then File.Delete(apex_current & "\APEX001.MSW")
            If File.Exists(apex_current & "\APEX001.NTT") Then File.Delete(apex_current & "\APEX001.NTT")

            _result.message = ExecCmd(apex_current & "\RunAPEX.bat", apex_current)
            If _result.message <> "OK" Then
                Throw New Global.System.Exception("Error running APEX program - " & _result.message)
            Else
                Dim swOutpPutFile As StreamReader = New StreamReader(File.OpenRead(apex_current & "\APEX001.OUT"))
                Dim outPutFile As String = swOutpPutFile.ReadToEnd
                If Not outPutFile.Contains("TOTAL RUN TIME") Then
                    swOutpPutFile.Close()
                    swOutpPutFile.Dispose()
                    swOutpPutFile = Nothing
                    _result.message = "Error running APEX program"
                    Throw New Global.System.Exception("Error running APEX program")
                End If
                swOutpPutFile.Close()
                swOutpPutFile.Dispose()
                swOutpPutFile = Nothing
            End If

            APEXResults1 = New ScenariosData.APEXResults
            APEXResults1 = ReadAPEXResults()
            APEXResults1.OtherInfo.state = state
            APEXResults1.OtherInfo.county = county
            APEXResults1.OtherInfo.ssa = ssa
            APEXResults1.OtherInfo.soil_name = soil_name
            APEXResults1.OtherInfo.soil_key = soil_key
            APEXResults1.OtherInfo.soil_component = soil_component
            APEXResults1.OtherInfo.soil_slope = slope
            APEXResults1.OtherInfo.management = mgt
            APEXResults1.OtherInfo.param_file = parm_file
            APEXResults1.OtherInfo.control_file = control_file
            APEXResults1.OtherInfo.drainType = drainType
            Save_results_text(APEXResults1, i, "No")
            Return msg
        Catch ex As Exception
            APEXResults1 = New ScenariosData.APEXResults
            APEXResults1.OtherInfo.state = state
            APEXResults1.OtherInfo.county = county
            APEXResults1.OtherInfo.ssa = ssa
            APEXResults1.OtherInfo.soil_name = soil_name
            APEXResults1.OtherInfo.soil_key = soil_key
            APEXResults1.OtherInfo.soil_component = soil_component
            APEXResults1.OtherInfo.soil_slope = slope
            APEXResults1.OtherInfo.management = mgt
            APEXResults1.OtherInfo.param_file = parm_file
            APEXResults1.OtherInfo.control_file = control_file
            APEXResults1.OtherInfo.drainType = drainType
            'sw_log.WriteLine("End results totaly")
            Save_results_text(APEXResults1, i, ex.Message)
            Return ex.Message
        End Try

    End Function

    Private Sub copy_management_file(mgt As String)
        Try
            lblMessage.Text &= " - Management => " & mgt
            lblMessage.ForeColor = Color.Green
            System.IO.File.Copy(apex_current & "\" & mgt, apex_current & "\APEX.opc", True)
            'if autoirrigation the irrigation type shuold be added to manaagement files.
            If chk_autoirrigation.Checked Then
                Dim srFile As StreamReader = New StreamReader(apex_current & "\APEX.opc")
                Dim swFile As StreamWriter = New StreamWriter(apex_current & "\APEX.tmp")

                swFile.WriteLine(srFile.ReadLine)
                Dim temp As String = srFile.ReadLine
                swFile.Write(temp.Substring(0, 4))
                Select Case cb_irrigation_type.SelectedIndex
                    Case 0  'Sprinkler
                        swFile.Write(" 500")
                    Case 1  'Furrow/Flood
                        swFile.Write(" 502")
                    Case 2  'Drip
                        swFile.Write(" 530")
                    Case 3  'Furrow Diking
                        swFile.Write(" 502")
                End Select
                swFile.WriteLine("")
                Do While srFile.EndOfStream <> True
                    swFile.WriteLine(srFile.ReadLine)
                Loop
                srFile.Close()
                srFile.Dispose()
                srFile = Nothing
                swFile.Close()
                swFile.Dispose()
                swFile = Nothing
                System.IO.File.Copy(apex_current & "\APEX.tmp", apex_current & "\APEX.opc", True)
            End If
        Catch ex As Exception
        Finally
        End Try
    End Sub

    Public Function ExecCmd(ByRef cmdline As String, ByRef direxe As String) As String
        Dim ret As Integer
        Dim CREATE_DEFAULT_ERROR_MODE As Integer
        Dim curdrive As String

        Dim proc As PROCESS_INFORMATION
        proc = New PROCESS_INFORMATION
        Dim start As STARTUPINFO
        start = New STARTUPINFO
        Dim exitCode As Integer

        Try
            ExecCmd = 0
            currentDir = CurDir()
            curdrive = Strings.Left(direxe, 2)
            ChDrive(CStr(curdrive))
            ChDir(direxe) 'define the current directory.
            start.cb = Len(start) ' Initialize the STARTUPINFO structure:
            start.dwFlags = 1
            start.wShowWindow = 0
            ret = CreateProcessA(cmdline, vbNullString, 0, 0, 1, CREATE_DEFAULT_ERROR_MODE, 0, vbNullString, start, proc)
            ' Wait for the shelled application to finish:
            ret = WaitForSingleObject(proc.hProcess, INFINITE)

            exitCode = GetExitCodeProcess(proc.hProcess, ExecCmd)
            Call CloseHandle(proc.hThread)
            Call CloseHandle(proc.hProcess)

            'curdrive = Strings.Left(currentDir, 2)
            'ChDrive(CStr(curdrive))
            'ChDir(currentDir) 'define the current directory.
            If ExecCmd <> 0 Then
                MsgBox("MS-DOS " & cmdline & " process did not finish properly, please check it out", , "Error Message")
            End If
            Return "OK"
        Catch ex As Exception
            MsgBox(Err.Description & " " & cmdline)
            Return ex.Message
        Finally
            curdrive = Strings.Left(currentDir, 2)
            ChDrive(CStr(curdrive))
            ChDir(currentDir) 'define the current directory.
        End Try
    End Function

    Private Function doAPEXProcess(ByVal sRunBat As String) As String
        Dim myProcess As Process = New Process
        Dim i As Integer
        Dim sReturn As String = ""

        Try
            ' set the file name and the command line args
            myProcess.StartInfo.FileName = "cmd.exe"
            myProcess.StartInfo.Arguments = "/C " & sRunBat & " " & Chr(34) & " && exit"

            ' start the process in a hidden window
            'myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            myProcess.StartInfo.CreateNoWindow = True

            ' allow the process to raise events
            myProcess.EnableRaisingEvents = True
            ' add an Exited event handler
            AddHandler myProcess.Exited, AddressOf processAPEXExited

            myProcess.Start()
            For i = 0 To 10000000
                If myProcess.HasExited Then
                    Exit For
                End If
            Next i

            If myProcess.ExitCode = 0 Then
                sReturn = "OK"
            Else
                sReturn = "Erro 404 - "
            End If
            myProcess.Close()
            myProcess.Dispose()
            myProcess = Nothing

            Return sReturn

        Catch ex As Exception
            myProcess.Close()
            myProcess.Dispose()
            myProcess = Nothing
            Return ("Error 404 - " & ex.Message & " - ")
        End Try
    End Function

    Private Sub processAPEXExited(ByVal sender As Object, ByVal e As System.EventArgs)
        Try
            bProcessAPEXExited = True
        Catch ex As System.Exception
        End Try
    End Sub

    Public Function calcSlopeLength(ByVal soilSlope As Single)
        Dim slopeLength As Single

        Const ft_to_m = 0.3048
        Select Case Math.Round(soilSlope, 4)
            Case 0 To 0.5, 11.0001 To 12
                slopeLength = 100 * ft_to_m
            Case 0.5001 To 1, 2.0001 To 3
                slopeLength = 200 * ft_to_m
            Case 1.0001 To 2
                slopeLength = 300 * ft_to_m
            Case 3.0001 To 4
                slopeLength = 180 * ft_to_m
            Case 4.0001 To 5
                slopeLength = 160 * ft_to_m
            Case 5.0001 To 6
                slopeLength = 150 * ft_to_m
            Case 6.0001 To 7
                slopeLength = 140 * ft_to_m
            Case 7.0001 To 8
                slopeLength = 130 * ft_to_m
            Case 8.0001 To 9
                slopeLength = 125 * ft_to_m
            Case 9.0001 To 10
                slopeLength = 120 * ft_to_m
            Case 10.0001 To 11
                slopeLength = 110 * ft_to_m
            Case 12.0001 To 13
                slopeLength = 90 * ft_to_m
            Case 13.0001 To 14
                slopeLength = 80 * ft_to_m
            Case 14.0001 To 15
                slopeLength = 70 * ft_to_m
            Case 15.0001 To 17
                slopeLength = 60 * ft_to_m
            Case Is > 17
                slopeLength = 50 * ft_to_m
        End Select

        Return slopeLength
    End Function

    Public Function ReadAPEXResults() As ScenariosData.APEXResults
        Dim tempa, YearAnt() As String
        Dim i, Sub1, m(0), n, APEXStartYear, l, index As Integer
        Dim APEXResults1 As New ScenariosData.APEXResults
        Dim found As Boolean
        Dim totalArea As Single
        Dim resultFS(11) As Single
        Dim srfile As StreamReader = Nothing
        Dim ddCYR1 As Integer = 0
        Dim ddNBYR1 As Integer = 0
        'Dim crop1() As ScenariosData.Crops
        Dim dr As SqlDataReader = Nothing
        Const ha_ac As Single = 2.471053814672
        'Dim dsResults As New DataSet

        Try
            'sw_log.WriteLine("Estar reading results")

            srfile = New StreamReader(File.OpenRead(apex_current & "\Apexcont.dat"))
            tempa = srfile.ReadLine()
            srfile.Close()
            ddNBYR1 = Val(Mid(tempa, 5, 4))
            ddCYR1 = Val(Mid(tempa, 81, 4))
            srfile = New StreamReader(File.OpenRead(apex_current + "\APEX001.ntt"))

            For i = 1 To 3
                tempa = srfile.ReadLine
            Next

            '// get config values
            'nAPEXYears = configurationAppSettings.Get("RunYears")
            ReDim APEXResults1.SoilResults(0)
            ReDim APEXResults1.Crops(0)
            ReDim APEXResults1.SoilResults(0).CountCrops(10)
            ReDim APEXResults1.SoilResults(0).Yields(10)
            ReDim APEXResults1.SoilResults(0).bioms(10)
            ReDim APEXResults1.SoilResults(0).stl(10)
            ReDim YearAnt(0)
            index = 0

            If ddNBYR1 > 0 Then
                APEXStartYear = ddNBYR1 + nYearRotation
            Else
                APEXStartYear = nStartYear + nYearRotation
            End If

            totalArea = 0
            Sub1 = 0
            Do While srfile.EndOfStream <> True
                tempa = srfile.ReadLine
                Sub1 = Val(Mid(tempa, 1, 5))
                If Sub1 = 0 Then
                    l = 0
                Else
                    l = l + 1
                End If

                If Sub1 > index Then
                    ReDim YearAnt(Sub1)
                    ReDim Preserve APEXResults1.SoilResults(Sub1)
                    ReDim APEXResults1.SoilResults(Sub1).Yields(10)
                    ReDim APEXResults1.SoilResults(Sub1).bioms(10)
                    ReDim APEXResults1.SoilResults(Sub1).stl(10)
                    ReDim APEXResults1.SoilResults(Sub1).CountCrops(10)
                    ReDim m(Sub1)
                    index = Sub1
                End If
                If Val(Mid(tempa, 8, 4)) >= APEXStartYear Then
                    '// Acummulate values converting to units used in NTT
                    If YearAnt(Sub1) <> Mid(tempa, 8, 4) Then
                        YearAnt(Sub1) = Val(Mid(tempa, 8, 4))
                        'change on 4/14/2014 to add flow from dranage system
                        APEXResults1.SoilResults(Sub1).flow = APEXResults1.SoilResults(Sub1).flow + (Val(Mid(tempa, 32, 9)) + Val(Mid(tempa, 127, 9))) * mm_in
                        'APEXResults1.SoilResults(Sub1).flow = APEXResults1.SoilResults(Sub1).flow + Val(Mid(tempa, 32, 9)) * mm_in
                        'change on 4/14/2014 to add sediment from manure application
                        APEXResults1.SoilResults(Sub1).Sediment = APEXResults1.SoilResults(Sub1).Sediment + (Val(Mid(tempa, 41, 9)) + Val(Mid(tempa, 181, 9))) * tha_tac
                        'APEXResults1.SoilResults(Sub1).Sediment = APEXResults1.SoilResults(Sub1).Sediment + Val(Mid(tempa, 41, 9)) * tha_tac
                        APEXResults1.SoilResults(Sub1).OrgP = APEXResults1.SoilResults(Sub1).OrgP + (Val(Mid(tempa, 59, 9)) * kg_lbs / ha_ac)
                        APEXResults1.SoilResults(Sub1).PO4 = APEXResults1.SoilResults(Sub1).PO4 + (Val(Mid(tempa, 77, 9)) * kg_lbs / ha_ac)
                        APEXResults1.SoilResults(Sub1).OrgN = APEXResults1.SoilResults(Sub1).OrgN + (Val(Mid(tempa, 50, 9)) * kg_lbs / ha_ac)
                        APEXResults1.SoilResults(Sub1).NO3 = APEXResults1.SoilResults(Sub1).NO3 + (Val(Mid(tempa, 68, 9)) * kg_lbs / ha_ac)
                        APEXResults1.SoilResults(Sub1).tileDrainFlow = APEXResults1.SoilResults(Sub1).tileDrainFlow + Val(Mid(tempa, 127, 9)) * mm_in
                        APEXResults1.SoilResults(Sub1).tileDrainN = APEXResults1.SoilResults(Sub1).tileDrainN + (Val(Mid(tempa, 145, 9)) * kg_lbs / ha_ac)
                        APEXResults1.SoilResults(Sub1).tileDrainP = APEXResults1.SoilResults(Sub1).tileDrainP + (Val(Mid(tempa, 264, 9)) * kg_lbs / ha_ac)

                        If Sub1 <> 0 Then
                            APEXResults1.SoilResults(Sub1).avol = APEXResults1.SoilResults(Sub1).avol + (Val(Mid(tempa, 23, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(Sub1).deepPerFlow = APEXResults1.SoilResults(Sub1).deepPerFlow + Val(Mid(tempa, 136, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).LeachedP = APEXResults1.SoilResults(Sub1).LeachedP + (Val(Mid(tempa, 110, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(Sub1).LeachedN = APEXResults1.SoilResults(Sub1).LeachedN + (Val(Mid(tempa, 14, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(Sub1).volatizationN = APEXResults1.SoilResults(Sub1).volatizationN + (Val(Mid(tempa, 23, 9)) * kg_lbs / ha_ac)
                            'APEXResults1.SoilResults(Sub1).co2 = APEXResults1.SoilResults(Sub1).co2 + Val(Mid(tempa, 163, 9)) * kg_lbs / ha_ac
                            APEXResults1.SoilResults(Sub1).n2o = APEXResults1.SoilResults(Sub1).n2o + Val(Mid(tempa, 154, 9)) * kg_lbs / ha_ac
                            'These are added just for NTTBlock so far
                            APEXResults1.SoilResults(Sub1).percolation = APEXResults1.SoilResults(Sub1).percolation + Val(Mid(tempa, 273, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).water_yield = APEXResults1.SoilResults(Sub1).water_yield + Val(Mid(tempa, 283, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).pet = APEXResults1.SoilResults(Sub1).pet + Val(Mid(tempa, 293, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).et = APEXResults1.SoilResults(Sub1).et + Val(Mid(tempa, 303, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).soil_water = APEXResults1.SoilResults(Sub1).soil_water + Val(Mid(tempa, 313, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).pcp = APEXResults1.SoilResults(Sub1).pcp + Val(Mid(tempa, 230, 9)) * mm_in

                            'APEXResults1.SoilResults(0).tileDrainFlow = APEXResults1.SoilResults(0).tileDrainFlow + Val(Mid(tempa, 127, 9)) * mm_in
                            APEXResults1.SoilResults(0).avol = APEXResults1.SoilResults(0).avol + (Val(Mid(tempa, 23, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).deepPerFlow = APEXResults1.SoilResults(0).deepPerFlow + Val(Mid(tempa, 136, 9)) * mm_in
                            APEXResults1.SoilResults(0).LeachedP = APEXResults1.SoilResults(0).LeachedP + (Val(Mid(tempa, 110, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).LeachedN = APEXResults1.SoilResults(0).LeachedN + (Val(Mid(tempa, 14, 9)) * kg_lbs / ha_ac)
                            'APEXResults1.SoilResults(0).tileDrainN = APEXResults1.SoilResults(0).tileDrainN + (Val(Mid(tempa, 145, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).volatizationN = APEXResults1.SoilResults(0).volatizationN + (Val(Mid(tempa, 23, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).n2o = APEXResults1.SoilResults(0).n2o + Val(Mid(tempa, 154, 9)) * kg_lbs / ha_ac
                            'These are added just for NTTBlock so far
                            APEXResults1.SoilResults(0).percolation = APEXResults1.SoilResults(0).percolation + Val(Mid(tempa, 273, 9)) * mm_in
                            APEXResults1.SoilResults(0).water_yield = APEXResults1.SoilResults(0).water_yield + Val(Mid(tempa, 283, 9)) * mm_in
                            APEXResults1.SoilResults(0).pet = APEXResults1.SoilResults(0).pet + Val(Mid(tempa, 293, 9)) * mm_in
                            APEXResults1.SoilResults(0).et = APEXResults1.SoilResults(0).et + Val(Mid(tempa, 303, 9)) * mm_in
                            APEXResults1.SoilResults(0).soil_water = APEXResults1.SoilResults(0).soil_water + Val(Mid(tempa, 313, 9)) * mm_in
                            APEXResults1.SoilResults(0).pcp = APEXResults1.SoilResults(0).pcp + Val(Mid(tempa, 230, 9)) * mm_in
                        Else
                            APEXResults1.SoilResults(0).co2 = APEXResults1.SoilResults(0).co2 + Val(Mid(tempa, 163, 9)) * kg_lbs / ha_ac
                        End If
                        'If APEXResults1.SoilResults(Sub1).tileDrainN > 0 Then
                        '    If APEXResults1.SoilResults(Sub1).LeachedN > 0 Then
                        '        sTemp1 = APEXResults1.SoilResults(Sub1).LeachedN / APEXResults1.SoilResults(Sub1).tileDrainN * 100
                        '        sTemp2 = 100 - sTemp1
                        '        sTemp1 = APEXResults1.SoilResults(Sub1).LeachedP * sTemp2 / sTemp1
                        '        APEXResults1.SoilResults(Sub1).tileDrainP = sTemp1 - APEXResults1.SoilResults(Sub1).LeachedP
                        '    Else
                        '        APEXResults1.SoilResults(Sub1).tileDrainP = APEXResults1.SoilResults(Sub1).tileDrainN * 0.07
                        '    End If
                        'Else
                        '    APEXResults1.SoilResults(Sub1).tileDrainP = 0
                        'End If
                        m(Sub1) = m(Sub1) + 1
                    End If

                    found = False
                    If Sub1 = 0 Then Continue Do
                    For i = 0 To APEXResults1.Crops.GetUpperBound(0)
                        If APEXResults1.Crops(i) = "" Then Exit For
                        If APEXResults1.Crops(i) = Mid(tempa, 88, 4) Then
                            found = True
                            If Val(Mid(tempa, 101, 9)) <= 0 And Val(Mid(tempa, 92, 9)) <= 0 Then Continue For
                            If APEXResults1.SoilResults(0).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).Yields(i) : ReDim Preserve APEXResults1.SoilResults(0).bioms(i) : ReDim Preserve APEXResults1.SoilResults(0).stl(i)
                            APEXResults1.SoilResults(0).Yields(i) = APEXResults1.SoilResults(0).Yields(i) + ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
                            APEXResults1.SoilResults(0).bioms(i) = APEXResults1.SoilResults(0).bioms(i) + (Val(Mid(tempa, 323, 8)) * tha_tac)
                            APEXResults1.SoilResults(0).stl(i) = APEXResults1.SoilResults(0).stl(i) + (Val(Mid(tempa, 333, 8)) * tha_tac)
                            If APEXResults1.SoilResults(Sub1).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(Sub1).Yields(i) : ReDim Preserve APEXResults1.SoilResults(Sub1).bioms(i) : ReDim Preserve APEXResults1.SoilResults(Sub1).stl(i)
                            APEXResults1.SoilResults(Sub1).Yields(i) = APEXResults1.SoilResults(Sub1).Yields(i) + ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
                            APEXResults1.SoilResults(Sub1).bioms(i) = APEXResults1.SoilResults(Sub1).bioms(i) + (Val(Mid(tempa, 323, 8)) * tha_tac)
                            APEXResults1.SoilResults(Sub1).stl(i) = APEXResults1.SoilResults(Sub1).stl(i) + (Val(Mid(tempa, 333, 8)) * tha_tac)
                            If APEXResults1.SoilResults(Sub1).CountCrops.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(Sub1).CountCrops(i)
                            APEXResults1.SoilResults(Sub1).CountCrops(i) = APEXResults1.SoilResults(Sub1).CountCrops(i) + 1
                            If APEXResults1.SoilResults(0).CountCrops.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).CountCrops(i)
                            APEXResults1.SoilResults(0).CountCrops(i) = APEXResults1.SoilResults(0).CountCrops(i) + 1
                            Exit For
                        End If
                    Next

                    If found = False Then
                        If APEXResults1.Crops(0) = "" Then i = 0
                        ReDim Preserve APEXResults1.Crops(i)
                        If APEXResults1.SoilResults(0).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).Yields(i) : ReDim Preserve APEXResults1.SoilResults(0).bioms(i) : ReDim Preserve APEXResults1.SoilResults(0).stl(i)
                        APEXResults1.SoilResults(0).Yields(i) = ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
                        APEXResults1.SoilResults(0).bioms(i) = Val(Mid(tempa, 323, 8)) * tha_tac
                        APEXResults1.SoilResults(0).stl(i) = Val(Mid(tempa, 333, 8)) * tha_tac
                        If APEXResults1.SoilResults(0).CountCrops.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).CountCrops(i)
                        APEXResults1.SoilResults(0).CountCrops(i) = APEXResults1.SoilResults(0).CountCrops(i) + 1
                        If APEXResults1.SoilResults(Sub1).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(Sub1).Yields(i) : ReDim Preserve APEXResults1.SoilResults(Sub1).bioms(i) : ReDim Preserve APEXResults1.SoilResults(Sub1).stl(i)
                        APEXResults1.SoilResults(Sub1).Yields(i) = ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
                        APEXResults1.SoilResults(Sub1).bioms(i) = (Val(Mid(tempa, 323, 9)) * tha_tac)
                        APEXResults1.SoilResults(Sub1).stl(i) = (Val(Mid(tempa, 333, 9)) * tha_tac)
                        If APEXResults1.SoilResults(Sub1).CountCrops.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(Sub1).CountCrops(i)
                        APEXResults1.SoilResults(Sub1).CountCrops(i) = APEXResults1.SoilResults(Sub1).CountCrops(i) + 1

                        APEXResults1.Crops(i) = Mid(tempa, 88, 4)
                        found = True
                    End If
                End If
            Loop

            ReDim Preserve APEXResults1.SoilResults(index + 1)

            m(0) = m(1)
            For n = 0 To index
                'ReDim Preserve APEXResults1.SoilResults(index + 1).Yields(5)
                For i = 0 To APEXResults1.SoilResults(n).CountCrops.GetUpperBound(0)
                    APEXResults1.SoilResults(n).Yields(i) = APEXResults1.SoilResults(n).Yields(i) / APEXResults1.SoilResults(n).CountCrops(i)
                    APEXResults1.SoilResults(n).bioms(i) = APEXResults1.SoilResults(n).bioms(i) / APEXResults1.SoilResults(n).CountCrops(i)
                    APEXResults1.SoilResults(n).stl(i) = APEXResults1.SoilResults(n).stl(i) / APEXResults1.SoilResults(n).CountCrops(i)
                Next
                '// Get averages for each value
                APEXResults1.SoilResults(n).flow = APEXResults1.SoilResults(n).flow / m(n)
                APEXResults1.SoilResults(n).tileDrainFlow = APEXResults1.SoilResults(n).tileDrainFlow / m(n)
                APEXResults1.SoilResults(n).deepPerFlow = APEXResults1.SoilResults(n).deepPerFlow / (m(n) * index)
                APEXResults1.SoilResults(n).Sediment = APEXResults1.SoilResults(n).Sediment / m(n)
                APEXResults1.SoilResults(n).OrgP = APEXResults1.SoilResults(n).OrgP / m(n)
                APEXResults1.SoilResults(n).PO4 = APEXResults1.SoilResults(n).PO4 / m(n)
                APEXResults1.SoilResults(n).LeachedP = APEXResults1.SoilResults(n).LeachedP / (m(n) * index)
                APEXResults1.SoilResults(n).OrgN = APEXResults1.SoilResults(n).OrgN / m(n)
                APEXResults1.SoilResults(n).NO3 = APEXResults1.SoilResults(n).NO3 / m(n)
                APEXResults1.SoilResults(n).avol = APEXResults1.SoilResults(n).avol / (m(n) * index)
                APEXResults1.SoilResults(n).LeachedN = APEXResults1.SoilResults(n).LeachedN / (m(n) * index)
                APEXResults1.SoilResults(n).tileDrainN = APEXResults1.SoilResults(n).tileDrainN / m(n)
                APEXResults1.SoilResults(n).tileDrainP = APEXResults1.SoilResults(n).tileDrainP / m(n)
                APEXResults1.SoilResults(n).volatizationN = APEXResults1.SoilResults(n).volatizationN / (m(n) * index)
                APEXResults1.SoilResults(n).n2o = APEXResults1.SoilResults(n).n2o / (m(n) * index)
                APEXResults1.SoilResults(n).co2 = APEXResults1.SoilResults(n).co2 / (m(n))  'do not use index because the total is comming from field 0 (total for the field)
                APEXResults1.SoilResults(n).percolation = APEXResults1.SoilResults(n).percolation / (m(n) * index)
                APEXResults1.SoilResults(n).water_yield = APEXResults1.SoilResults(n).water_yield / (m(n) * index)
                APEXResults1.SoilResults(n).pet = APEXResults1.SoilResults(n).pet / (m(n) * index)
                APEXResults1.SoilResults(n).et = APEXResults1.SoilResults(n).et / (m(n) * index)
                APEXResults1.SoilResults(n).pcp = APEXResults1.SoilResults(n).pcp / (m(n) * index)
                APEXResults1.SoilResults(n).soil_water = APEXResults1.SoilResults(n).soil_water / (m(n) * index)
                'CALCULATE tile drain P. No anymore tile drain P is taking from NTT file
                'If APEXResults1.SoilResults(n).tileDrainN > 0 Then
                '    If APEXResults1.SoilResults(n).LeachedN > 0 Then
                '        APEXResults1.SoilResults(n).tileDrainP = APEXResults1.SoilResults(n).tileDrainN * 0.07

                '        'sTemp1 = APEXResults1.SoilResults(n).LeachedN / APEXResults1.SoilResults(n).tileDrainN * 100
                '        'sTemp2 = 100 - sTemp1
                '        'sTemp1 = APEXResults1.SoilResults(n).LeachedP * sTemp2 / sTemp1
                '        'APEXResults1.SoilResults(n).tileDrainP = sTemp1 - APEXResults1.SoilResults(n).LeachedP
                '    Else
                '        APEXResults1.SoilResults(n).tileDrainP = APEXResults1.SoilResults(n).tileDrainN * 0.07
                '    End If
                'Else
                '    APEXResults1.SoilResults(n).tileDrainP = 0
                'End If
            Next

            srfile.Close()
            srfile.Dispose()
            srfile = Nothing
            APEXResults1.i = APEXResults1.Crops.GetUpperBound(0)

            'getLocalDataTable("DELETE * FROM results", apex_default.Replace("APEX1", ""))
            'rowLink = dsResults.Tables("Results").Rows(0)
            'rowLink("ErrorCode") = 0
            'rowLink("ErrorDes") = "No Error"
            'rowLink("OrganicN") = APEXResults1.SoilResults(0).OrgN
            'rowLink("NO3") = APEXResults1.SoilResults(0).NO3
            'rowLink("LeachedN") = APEXResults1.SoilResults(0).LeachedN
            'rowLink("VolatilizedN") = APEXResults1.SoilResults(0).volatizationN
            'rowLink("NitrousOxide") = APEXResults1.SoilResults(0).n2o
            'rowLink("TileDrainN") = APEXResults1.SoilResults(0).tileDrainN
            'rowLink("OrganicP") = APEXResults1.SoilResults(0).OrgP
            'rowLink("SolubleP") = APEXResults1.SoilResults(0).PO4
            'rowLink("LeachedP") = APEXResults1.SoilResults(0).LeachedP
            'rowLink("TileDrainP") = APEXResults1.SoilResults(0).tileDrainP
            'rowLink("Flow") = APEXResults1.SoilResults(0).flow
            'rowLink("Sediment") = APEXResults1.SoilResults(0).Sediment
            'rowLink("Carbon") = APEXResults1.SoilResults(0).co2
            'rowLink("Customer") = Customer
            'rowLink("ID") = session

            'rowLink = dsResults.Tables("Crops").Rows(0)
            'For k = 0 To APEXResults1.Crops.GetUpperBound(0)
                'ReDim Preserve crop1(k)
                'Dim foundSilage As Boolean = False
                'crop1(k).cropYieldBase = 0
                'crop1(k).cropYieldAlt = 0
                'crop1(k).cropDryMatter = 0
                'crop1(k).cropName = APEXResults1.Crops(k)
                '//Take new unit and the convertion factor
                'If sState = "MD" Then
                'dr = GetRecords("SELECT * FROM " & sState & "APEXCrops WHERE CropCode LIKE" & "'" & crop1(k).cropName.ToString.Trim & "%'")
                'Else
                'dr = GetRecords("SELECT * FROM APEXCrops WHERE CropCode LIKE" & "'" & crop1(k).cropName.ToString.Trim & "%'")
                'End If
                'If dr.HasRows = True Then
                    'dr.Read()
                    'crop1(k).cropCode = dr.Item("cropNumber")
                    'If foundSilage = False Then
                        'crop1(k).cropYieldUnit = dr.Item("yieldUnit")
                        'crop1(k).cropYieldFactor = dr.Item("conversionFactor") / ha_ac
                        'crop1(k).cropDryMatter = dr.Item("DryMatter")
                    'Else
                        'crop1(k).cropYieldUnit = "t"
                        'crop1(k).cropYieldFactor = 1 / ha_ac
                        'crop1(k).cropDryMatter = 33
                    'End If
                'Else
                    'crop1(k).cropYieldUnit = "t"
                    'crop1(k).cropYieldFactor = 1 / ha_ac
                    'crop1(k).cropDryMatter = 100
                'End If
                'If crop1(k).cropYieldUnit.Trim = "t" Then
                '    crop1(k).cropYieldBase = Math.Round(APEXResults1.SoilResults(0).Yields(k) * crop1(k).cropYieldFactor / (crop1(k).cropDryMatter / 100), 2)
                'Else
                '    crop1(k).cropYieldBase = Math.Round(APEXResults1.SoilResults(0).Yields(k) * crop1(k).cropYieldFactor / (crop1(k).cropDryMatter / 100), 0)
                'End If
            'Next

            'dsResults.WriteXml(configurationAppSettings.Get("Results").ToString.Trim & session & ".xml")
            APEXResults1.message = "OK"
            Return APEXResults1
            'sw_log.WriteLine("End reading results")

        Catch ex As Exception
            APEXResults1.message = "Error 405 - " & ex.Message & " Read results process.'"
            'sw_log.WriteLine("Error reading results")
            Return APEXResults1
        Finally
            If Not srfile Is Nothing Then
                srfile.Close()
                srfile.Dispose()
                srfile = Nothing
            End If
        End Try

    End Function

    'Private Sub create_excel_file()
    '    row_number = 1
    '    Dim i As UShort = row_number
    '    Dim j As UShort = 1

    '    ws.Cells._Default(i, j) = "ID"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Observation"
    '    j += 1
    '    ws.Cells._Default(i, j) = "State"
    '    j += 1
    '    ws.Cells._Default(i, j) = "County"
    '    j += 1
    '    ws.Cells._Default(i, j) = "SSA"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Soil Name"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Soil Key"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Soil Component"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Slope"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Management"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Parm File"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Control File"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Drain Type"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Org N"
    '    j += 1
    '    ws.Cells._Default(i, j) = "NO3_N"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Tile Drain N"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Org P"
    '    j += 1
    '    ws.Cells._Default(i, j) = "PO4_P"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Tile Drain P"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Flow"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Tile Drain Flow"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Sediment"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Manure Erosion"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Deep Percolation"
    '    j += 1
    '    ws.Cells._Default(i, j) = "N2O"
    '    j += 1
    '    ws.Cells._Default(i, j) = "CO2"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Percolation(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Deep Per(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Water Yield(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "PET(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "ET(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Precipitation(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Soil Water(in)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop1"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop Yield1"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Biomas(t/ac)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop2"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop Yield2"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Biomas(t/ac)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop3"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop Yield3"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Biomas(t/ac)"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop4"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Crop Yield4"
    '    j += 1
    '    ws.Cells._Default(i, j) = "Biomas(t/ac)"
    'End Sub

    Private Sub create_titles_file()
        Dim row As New System.Text.StringBuilder
        row_number = 1
        Dim i As UShort = row_number

        row.Append("ID")
        row.Append("|")
        row.Append("Observation")
        row.Append("|")
        row.Append("State")
        row.Append("|")
        row.Append("County")
        row.Append("|")
        row.Append("SSA")
        row.Append("|")
        row.Append("Soil Name")
        row.Append("|")
        row.Append("Soil Key")
        row.Append("|")
        row.Append("Soil Component")
        row.Append("|")
        row.Append("Slope")
        row.Append("|")
        row.Append("Management")
        row.Append("|")
        row.Append("Parm File")
        row.Append("|")
        row.Append("Control File")
        row.Append("|")
        row.Append("Drain Type")
        row.Append("|")
        row.Append("Org N")
        row.Append("|")
        row.Append("NO3_N")
        row.Append("|")
        row.Append("Tile Drain N")
        row.Append("|")
        row.Append("Org P")
        row.Append("|")
        row.Append("PO4_P")
        row.Append("|")
        row.Append("Tile Drain P")
        row.Append("|")
        row.Append("Flow")
        row.Append("|")
        row.Append("Tile Drain Flow")
        row.Append("|")
        row.Append("Sediment")
        row.Append("|")
        row.Append("Manure Erosion")
        row.Append("|")
        row.Append("Deep Percolation")
        row.Append("|")
        row.Append("N2O")
        row.Append("|")
        row.Append("CO2")
        row.Append("|")
        row.Append("Percolation(in)")
        row.Append("|")
        row.Append("Deep Per(in)")
        row.Append("|")
        row.Append("Water Yield(in)")
        row.Append("|")
        row.Append("PET(in)")
        row.Append("|")
        row.Append("ET(in)")
        row.Append("|")
        row.Append("Precipitation(in)")
        row.Append("|")
        row.Append("Soil Water(in)")
        row.Append("|")
        row.Append("Nitrogen Volatilitization")
        row.Append("|")
        row.Append("Crop1")
        row.Append("|")
        row.Append("Crop Yield1")
        row.Append("|")
        row.Append("Biomas(t/ac)")
        row.Append("|")
        row.Append("Max STL(t/ac)")
        row.Append("|")
        row.Append("Crop2")
        row.Append("|")
        row.Append("Crop Yield2")
        row.Append("|")
        row.Append("Biomas(t/ac)")
        row.Append("|")
        row.Append("Max STL(t/ac)")
        row.Append("|")
        row.Append("Crop3")
        row.Append("|")
        row.Append("Crop Yield3")
        row.Append("|")
        row.Append("Biomas(t/ac)")
        row.Append("|")
        row.Append("Max STL(t/ac)")
        row.Append("|")
        row.Append("Crop4")
        row.Append("|")
        row.Append("Crop Yield4")
        row.Append("|")
        row.Append("Biomas(t/ac)")
        row.Append("|")
        row.Append("Max STL(t/ac)")
        sw_results.WriteLine(row)
    End Sub

    Public Sub Save_results_text(results As ScenariosData.APEXResults, id As UShort, err As String)
        Dim row As New System.Text.StringBuilder
        Dim i As UInteger
        row_number += 1

        Try
            row.Length = 0
            i = row_number
            If id > 0 Then
                row.Append(id)
            Else
                row.Append(i - 1)
            End If
            row.Append("|")
            row.Append(err)
            row.Append("|")
            row.Append(results.OtherInfo.state)
            row.Append("|")
            row.Append(results.OtherInfo.county)
            row.Append("|")
            row.Append(results.OtherInfo.ssa)
            row.Append("|")
            row.Append(results.OtherInfo.soil_name)
            row.Append("|")
            row.Append(results.OtherInfo.soil_key)
            row.Append("|")
            row.Append(results.OtherInfo.soil_component)
            row.Append("|")
            row.Append(results.OtherInfo.soil_slope)
            row.Append("|")
            row.Append(results.OtherInfo.management)
            row.Append("|")
            row.Append(results.OtherInfo.param_file)
            row.Append("|")
            row.Append(results.OtherInfo.control_file)
            row.Append("|")
            row.Append(results.OtherInfo.drainType)
            row.Append("|")
            If err = "No" Then
                row.Append(results.SoilResults(0).OrgN)
                row.Append("|")
                row.Append(results.SoilResults(0).NO3)
                row.Append("|")
                row.Append(results.SoilResults(0).tileDrainN)
                row.Append("|")
                row.Append(results.SoilResults(0).OrgP)
                row.Append("|")
                row.Append(results.SoilResults(0).PO4)
                row.Append("|")
                row.Append(results.SoilResults(0).tileDrainP)
                row.Append("|")
                row.Append(results.SoilResults(0).flow)
                row.Append("|")
                row.Append(results.SoilResults(0).tileDrainFlow)
                row.Append("|")
                row.Append(results.SoilResults(0).Sediment)
                row.Append("|")
                row.Append(0)  'no set for now
                row.Append("|")
                row.Append(results.SoilResults(0).deepPerFlow)
                row.Append("|")
                row.Append(results.SoilResults(0).n2o)
                row.Append("|")
                row.Append(results.SoilResults(0).co2)
                row.Append("|")
                row.Append(results.SoilResults(0).percolation)
                row.Append("|")
                row.Append(results.SoilResults(0).deepPerFlow)
                row.Append("|")
                row.Append(results.SoilResults(0).water_yield)
                row.Append("|")
                row.Append(results.SoilResults(0).pet)
                row.Append("|")
                row.Append(results.SoilResults(0).et)
                row.Append("|")
                row.Append(results.SoilResults(0).pcp)
                row.Append("|")
                row.Append(results.SoilResults(0).soil_water)
                row.Append("|")
                row.Append(results.SoilResults(0).avol)
                row.Append("|")
                For k = 0 To results.Crops.Count - 1
                    row.Append(results.Crops(k))
                    row.Append("|")
                    row.Append(results.SoilResults(0).Yields(k))
                    row.Append("|")
                    row.Append(results.SoilResults(0).bioms(k))
                    row.Append("|")
                    row.Append(results.SoilResults(0).stl(k))
                    row.Append("|")
                Next
            End If
            sw_results.WriteLine(row)
        Catch ex As Exception
        Finally
        End Try

    End Sub

    'Public Sub Save_results(results As ScenariosData.APEXResults, id As UShort, err As String)
    '    Dim i As UShort
    '    Dim j As UShort = 1
    '    row_number += 1
    '    Try
    '        'sw_log.WriteLine("Start Saving Results")
    '        i = row_number
    '        If id > 0 Then
    '            ws.Cells._Default(i, j) = id
    '        Else
    '            ws.Cells._Default(i, j) = i - 1
    '        End If
    '        j += 1
    '        ws.Cells._Default(i, j) = err
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.state
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.county
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.ssa
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.soil_name
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.soil_key
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.soil_component
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.soil_slope
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.management
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.param_file
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.control_file
    '        j += 1
    '        ws.Cells._Default(i, j) = results.OtherInfo.drainType
    '        j += 1
    '        If err = "No" Then
    '            ws.Cells._Default(i, j) = results.SoilResults(0).OrgN
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).NO3
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).tileDrainN
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).OrgP
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).PO4
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).tileDrainP
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).flow
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).tileDrainFlow
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).Sediment
    '            j += 1
    '            ws.Cells._Default(i, j) = 0  'no set for now
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).deepPerFlow
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).n2o
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).co2
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).percolation
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).deepPerFlow
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).water_yield
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).pet
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).et
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).pcp
    '            j += 1
    '            ws.Cells._Default(i, j) = results.SoilResults(0).soil_water
    '            j += 1
    '            For k = 0 To results.Crops.Count - 1
    '                ws.Cells._Default(i, j) = results.Crops(k)
    '                j += 1
    '                ws.Cells._Default(i, j) = results.SoilResults(0).Yields(k)
    '                j += 1
    '                ws.Cells._Default(i, j) = results.SoilResults(0).bioms(k)
    '                j += 1
    '            Next
    '        Else
    '            'sw_log.WriteLine(err)
    '        End If

    '        'sw_log.WriteLine("End Saving Results")

    '    Catch ex As Exception
    '        'sw_log.WriteLine("Problem Saving Results")
    '    Finally

    '    End Try
    'End Sub

    Private Sub SaveFile(filePath As String, fileName As String)
        Dim fileToSave As String = Path.Combine(filePath, fileName)
        Try
            If Dir(fileToSave) <> "" Then
                File.Delete(fileToSave)
            End If
            'wb.SaveAs(fileToSave)
            MsgBox("Your " & fileName.Replace(".xlsx", "") & " File has been Saved as " & fileToSave, MsgBoxStyle.OkOnly, Me.Name & fileName.Replace(".xlsx", "") & "_Click")
        Catch ex As Exception
            MsgBox(ex.Message & " - " & fileToSave, MsgBoxStyle.OkOnly, Me.Name & fileName & " _Click")
        Finally
            'releaseObject(wb, 1)
            'releaseObject(xlApp, 0)
        End Try
    End Sub

    'Public Function getLocalDataTable(ByRef query As String, ByVal path As String) As Data.DataTable
    '    Dim dsBas As New Data.DataSet()
    '    Dim myConnection As New OleDb.OleDbConnection
    '    Dim myConnection1 As New OleDb.OleDbConnection
    '    Dim myConnection2 As New OleDb.OleDbConnection()
    '    Dim myConnection3 As New OleDb.OleDbConnection()
    '    Dim myConnection4 As New OleDb.OleDbConnection()
    '    Dim myConnection5 As New OleDb.OleDbConnection()
    '    Dim myConnection6 As New OleDb.OleDbConnection()
    '    Dim myConnection7 As New OleDb.OleDbConnection()
    '    'Dim ad As New OleDb.OleDbDataAdapter

    '    Dim dbConnectString As String = "PROVIDER=Microsoft.Jet.OLEDB.4.0;Data Source= " & path & "\Results.accdb;"

    '    Try
    '        myConnection.ConnectionString = dbConnectString
    '        If myConnection.State = Data.ConnectionState.Closed Then
    '            myConnection.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection1.ConnectionString = dbConnectString
    '        If myConnection1.State = Data.ConnectionState.Closed Then
    '            myConnection1.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection1)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection2.ConnectionString = dbConnectString
    '        If myConnection2.State = Data.ConnectionState.Closed Then
    '            myConnection2.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection2)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection3.ConnectionString = dbConnectString
    '        If myConnection3.State = Data.ConnectionState.Closed Then
    '            myConnection3.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection3)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection4.ConnectionString = dbConnectString
    '        If myConnection4.State = Data.ConnectionState.Closed Then
    '            myConnection4.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection4)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection5.ConnectionString = dbConnectString
    '        If myConnection5.State = Data.ConnectionState.Closed Then
    '            myConnection5.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection5)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection6.ConnectionString = dbConnectString
    '        If myConnection6.State = Data.ConnectionState.Closed Then
    '            myConnection6.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection6)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If

    '        myConnection7.ConnectionString = dbConnectString
    '        If myConnection7.State = Data.ConnectionState.Closed Then
    '            myConnection7.Open()
    '            Dim ad As New OleDb.OleDbDataAdapter(query, myConnection7)
    '            ad.Fill(dsBas)
    '            Exit Try
    '        End If
    '    Catch ex As Exception
    '    End Try

    '    If myConnection1.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection2.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection3.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection4.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection5.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection6.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection7.State = Data.ConnectionState.Open Then myConnection.Close()
    '    If myConnection.State = Data.ConnectionState.Open Then myConnection.Close()

    '    myConnection.Dispose() : myConnection1.Dispose() : myConnection2.Dispose() : myConnection3.Dispose()
    '    myConnection4.Dispose() : myConnection5.Dispose() : myConnection6.Dispose() : myConnection7.Dispose()

    '    myConnection = Nothing : myConnection1 = Nothing : myConnection2 = Nothing : myConnection3 = Nothing
    '    myConnection4 = Nothing : myConnection5 = Nothing : myConnection6 = Nothing : myConnection7 = Nothing

    '    If dsBas.Tables.Count > 0 Then
    '        Return dsBas.Tables(0)
    '    Else
    '        Return Nothing
    '    End If
    'End Function

    Private Sub btnInitialRun_Click(sender As System.Object, e As System.EventArgs) Handles btnInitialRun.Click
        gbInitialRun.Visible = True
        gbRuns.Visible = False
    End Sub

    Private Sub btnRuns_Click(sender As System.Object, e As System.EventArgs) Handles btnRuns.Click
        Dim sr_results As StreamReader = New StreamReader(Directory.GetCurrentDirectory() & "\Results.txt")
        'Dim results_file As String = Path.Combine(Directory.GetCurrentDirectory(), "Results.xlsx")

        Try
            gbRuns.Visible = True
            gbInitialRun.Visible = False

            'xlApp2 = New Microsoft.Office.Interop.Excel.Application
            'wb2 = xlApp2.Workbooks.Open(results_file)
            'ws2 = wb2.Worksheets("Sheet1") 'Specify your worksheet name
            Dim i As UShort = 2
            'If results_file Is Nothing Or results_file = "" Or results_file = String.Empty Then Exit Sub
            If sr_results Is Nothing Then Exit Sub
            clbRuns.Items.Clear()
            'With ws2
            Dim temp As String = String.Empty
            'Do While .Cells(i, 1).value <> 0 And Not (.Cells(i, 1).value Is Nothing)
            '    clbRuns.Items.Add(.Cells(i, 1).value)
            '    i += 1
            'Loop
            Do While sr_results.EndOfStream <> True
                temp = sr_results.ReadLine
                clbRuns.Items.Add(temp.Split("|")(0))
            Loop
            'End With
        Catch ex As Exception
        Finally
            'ws2 = Nothing
            'wb2.Close()
            'System.Runtime.InteropServices.Marshal.ReleaseComObject(wb2)
            'wb2 = Nothing
            'releaseObject(wb2, 1)
            'releaseObject(xlApp2, 0)
            'xlApp2.Quit()
            'System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp2)
            'xlApp2 = Nothing
        End Try

    End Sub

    Private Sub releaseObject(ByVal obj As Object, type As UShort)
        Try
            If type = 0 Then
                obj.Quit()
            Else
                obj.Close()
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
            obj = Nothing
        Catch ex As Exception
            obj = Nothing
        Finally
            GC.Collect()
        End Try
    End Sub

    Private Sub btnSimulation1_Click(sender As System.Object, e As System.EventArgs) Handles btnSimulation1.Click
        Dim county_code As String = String.Empty
        Dim county_info As county_info
        Dim weather_info As weather_info
        Dim weaterFileName As SqlDataReader = Nothing
        'Dim ssas As SqlDataReader = Nothing
        Dim soils As DataTable = Nothing
        Dim layer_number As UShort = 0
        Dim depth As Single = 999
        Dim name As String = String.Empty
        Dim results_file As String = Path.Combine(Directory.GetCurrentDirectory(), "Results.xlsx")
        Dim i As UShort = 0
        Dim ssa_code As String
        Dim slope As Single = 0
        Dim mgt As String = String.Empty
        Dim control As String = String.Empty
        Dim params As String = String.Empty
        Dim component As String = String.Empty
        Dim series_name As String = String.Empty
        Dim muid As Integer = 0
        Dim state As String = String.Empty
        Dim msg As String = "OK"
        Dim horizgen As String = String.Empty
        Dim sr_results As StreamReader = New StreamReader(Directory.GetCurrentDirectory() & "\Results.txt")

        Try
            sw_results = New StreamWriter(Directory.GetCurrentDirectory() & "\Results_individual.txt")
            If clbRuns.CheckedItems.Count <= 0 Then lblMessage.Text = "Please check one id from the runs list" : lblMessage.ForeColor = Color.Red : Exit Sub
            If results_file Is Nothing Or results_file = "" Or results_file = String.Empty Then Exit Sub
            create_titles_file()
            sr_results.ReadLine()  'read titles
            Dim temp() As String = Nothing
            For j = 0 To clbRuns.CheckedItems.Count - 1
                temp = sr_results.ReadLine.Split("|")
                i = clbRuns.CheckedItems(j) + 1
                'create the excel file in memory before the simulations start.
                county_code = temp(3)
                'Read all of the counties selected and take code and center coordinates.
                'Dim sSql As String = "SELECT TOP 1 * FROM County_Extended WHERE StateAbrev like '" & Split(temp(2), "-")(1).Trim & "%' AND [Code] like '" & county_code.Trim & "%' ORDER BY [Name]"
                Dim sSql As String = "SELECT code, lon, lat, wcode, wname FROM County, CountyCoor, CountyWp1 WHERE StateAbrev like '" & Split(cbStates.SelectedItem, "-")(1).Trim & "%' AND [Code] like '" & county_code.Trim & "%' AND County.code = CountyCoor.County and County.code = CountyWp1.County"
                county_info = CoutyInfo(sSql)

                'county_info = CoutyInfo(sSql)
                If county_info.lon = 0 And county_info.lat = 0 Then Continue For
                weather_info = GetWeatherInfo(county_info.lat, county_info.lon)
                ssa_code = temp(4)
                APEXFolders(temp(11), temp(10))
                create_Weather_file(weather_info.name)
                lblMessage.Text = "Running County => " & county_code & " - SSA => " & ssa_code & " - Soil => " & temp(5)
                lblMessage.ForeColor = Color.Green
                mgt = temp(9)
                params = temp(10)
                control = temp(11)
                name = temp(5)
                component = temp(7)
                muid = temp(6)
                slope = temp(8)
                i = temp(0)  'take the row number from the simulations to put in the individual run.
                state = temp(2)


                sSql = "SELECT * FROM " & county_code.Substring(0, 2) & "SOILS WHERE TSSSACode = '" & ssa_code & "' AND TSCountyCode = '" & county_code & "' AND muid = " & muid & " AND seriesname = '" & component & "' ORDER BY [Series], [SeriesName], [ldep]"
                soils = service.GetSoilRecord(sSql)
                For Each soil In soils.Rows
                    If Not (soils.Rows(j).Item("ldep") Is Nothing Or IsDBNull(soils.Rows(j).Item("ldep"))) Then
                        If depth > soil("ldep") Then
                            layer_number = 0
                            layers = New layer_info
                            swSoil = New StreamWriter(apex_current & "\APEX.sol")
                        End If
                        If Not (depth = soil("ldep")) Then
                            depth = soil("ldep")
                            layer_number += 1
                            If layer_number <= 10 Then create_soils(soil, layer_number)
                        End If
                    End If
                    horizgen = soil("horizgen")
                Next
                print_layers()
                swSoil.Close()
                'create subarea
                create_subarea_file(slope / 100, horizgen)
                'copy the operation file one by one from the management list and then run the simulation
                copy_management_file(mgt)
                msg = run_apex(state, county_info.code, ssa_code, name, component, muid, slope, mgt, params, control, i, horizgen)
                lblMessage.Text = "Simulations finished succesfully"
                lblMessage.ForeColor = Color.Green
                'temp = sr_results.ReadLine.Split("|")
            Next

        Catch ex As Exception
            'if any error send a message.
            lblMessage.Text = "Error running Simulations " & ex.Message
            lblMessage.ForeColor = Color.Red
        Finally
            If Not swSoil Is Nothing Then
                swSoil.Close()
                swSoil.Dispose()
                swSoil = Nothing
            End If
            If Not sw_results Is Nothing Then
                sw_results.Close()
                sw_results.Dispose()
                sw_results = Nothing
            End If
        End Try
    End Sub

    Public Sub Create_wp1_from_weather(loc As String, wp1name As String, pgm As String)
        Dim sr As StreamReader = New StreamReader(loc & "\APEX.wth")
        Dim sw As StreamWriter = New StreamWriter(loc & "\" & wp1name.Trim & ".tmp")
        'File.Copy(loc & "\" & wp1name.Trim & ".wp1", loc & "\" & wp1name.Trim & ".wp1", True)
        Dim sr1 As StreamReader = New StreamReader(loc & "\" & wp1name.Trim & ".wp1")
        Dim wthData As WthData
        Dim wthDatas As New List(Of WthData)
        Dim wthMonthData As New WthData
        Dim wthMonthDatas As New List(Of WthData)
        Dim temp As String = String.Empty
        Dim maxTemp As Single = 0
        Dim minTemp As Single = 0
        Dim pcp As Single = 0
        Dim solarR As Single = 0
        Dim relativeH As Single = 0
        Dim windS As Single = 0
        Dim month As UShort
        Dim year As UShort
        Dim day As UShort
        Dim wp1Data As New Wp1Data
        Dim wp1Datas As New List(Of Wp1Data)
        Dim wp1MonSD As New Wp1Data
        Dim wp1MonSDs As New List(Of Wp1Data)
        Dim monthAnt As UShort = 0
        Dim firstYear As UShort = 0
        Dim lastYear As UShort = 0
        Dim newMonth As Boolean = False
        Dim dry_day_ant As Boolean = False
        Dim lines(15) As String

        Try
            Do While sr.EndOfStream <> True
                temp = sr.ReadLine
                If temp.Trim = "" Then
                    Exit Do
                End If
                UShort.TryParse(temp.Substring(2, 4), year)
                UShort.TryParse(temp.Substring(6, 4), month)
                UShort.TryParse(temp.Substring(10, 4), day)
                Single.TryParse(temp.Substring(14, 6), solarR)
                Single.TryParse(temp.Substring(20, 6), maxTemp)
                Single.TryParse(temp.Substring(26, 6), minTemp)
                If temp.Length < 39 Then
                    Single.TryParse(temp.Substring(32, 6), pcp)
                Else
                    Single.TryParse(temp.Substring(32, 7), pcp)
                End If
                If temp.Length >= 49 Then
                    Single.TryParse(temp.Substring(44, 6), windS)
                End If
                If temp.Length >= 43 Then
                    Single.TryParse(temp.Substring(38, 6), relativeH)
                End If
                wthData = New WthData
                wthData.Day = day
                wthData.Month = month
                wthData.Year = year
                If wp1Datas.Count < month Then
                    wp1Data = New Wp1Data
                    newMonth = True
                    wp1Datas.Add(wp1Data)
                End If
                If solarR > -900 And solarR < 900 Then wp1Datas(month - 1).Obsl += solarR : wp1Datas(month - 1).Days_obsl += 1
                wthData.MaxTemp = maxTemp
                If maxTemp > -900 And maxTemp < 900 Then wp1Datas(month - 1).Obmx += maxTemp : wp1Datas(month - 1).Days_obmx += 1
                wthData.MinTemp = minTemp
                If minTemp > -900 And minTemp < 900 Then wp1Datas(month - 1).Obmn += minTemp : wp1Datas(month - 1).Days_obmn += 1
                wthData.Pcp = pcp
                If pcp > -900 And pcp < 900 Then wp1Datas(month - 1).Rmo += pcp : wp1Datas(month - 1).Days_rmo += 1
                wp1Datas(month - 1).Rmosd += pcp
                wp1Datas(month - 1).Rh += relativeH
                wp1Datas(month - 1).Uav0 += windS
                wthDatas.Add(wthData)
                If monthAnt <> month Then
                    If monthAnt <> 0 Then
                        wthMonthData.MaxTemp /= wthMonthData.Day
                        wthMonthData.MinTemp /= wthMonthData.Day
                        wthMonthData.Pcp /= wthMonthData.Day
                        wthMonthDatas.Add(wthMonthData)
                        'calculate SD for each month and add to wp1datas
                        wthMonthData.MaxTemp = Math.Sqrt(wp1MonSDs.Where(Function(x) x.Obmx < 999).Sum(Function(x) (wthMonthData.MaxTemp - x.Obmx) ^ 2) / wp1MonSDs.Where(Function(x) x.Obmx < 999).Count)
                        wthMonthData.MinTemp = Math.Sqrt(wp1MonSDs.Where(Function(x) x.Obmn < 999).Sum(Function(x) (wthMonthData.MinTemp - x.Obmn) ^ 2) / wp1MonSDs.Where(Function(x) x.Obmn < 999).Count)
                        wp1MonSDs.Clear()
                    Else
                        firstYear = year
                    End If
                    monthAnt = month
                    wthMonthData = New WthData
                    wthMonthData.Year = year
                    wthMonthData.Month = month
                End If
                wp1MonSD = New Wp1Data
                wthMonthData.Day += 1
                wthMonthData.MaxTemp += maxTemp
                wp1MonSD.Obmx = maxTemp
                wthMonthData.MinTemp += minTemp
                wp1MonSD.Obmn = minTemp
                wthMonthData.Pcp += pcp
                wp1MonSD.Rmo = pcp
                wp1MonSDs.Add(wp1MonSD)
                If pcp > 0 Then
                    wthMonthData.WetDay += 1
                    If dry_day_ant = True Then wthMonthData.dd_wd += 1
                    dry_day_ant = False
                Else
                    dry_day_ant = True
                End If
            Loop
            'calculate the number of years
            Dim years = year - firstYear + 1
            'calculate averages for each month
            For Each mon In wp1Datas
                mon.Obmx /= mon.Days_obmx
                mon.Obmn /= mon.Days_obmn
                mon.Rmo /= years
                mon.Rmosd /= mon.Days_rmo
                mon.Obsl /= mon.Days_obsl
                If mon.Days_rh = 0 Then mon.Rh = 0 Else mon.Rh /= mon.Days_rh
                If mon.Days_Uav0 = 0 Then mon.Uav0 = 0 Else mon.Uav0 /= mon.Days_Uav0
                mon.Wi = 0
            Next
            '************************add the last month of the last year*********************
            wthMonthData.MaxTemp /= wthMonthData.Day
            wthMonthData.MinTemp /= wthMonthData.Day
            wthMonthData.Pcp /= wthMonthData.Day
            If pcp > 0 Then wthMonthData.WetDay += 1
            wthMonthDatas.Add(wthMonthData)
            'calculate SD for each month and add to wp1datas
            wthMonthData.MaxTemp = Math.Sqrt(wp1MonSDs.Where(Function(x) x.Obmx < 999).Sum(Function(x) (wthMonthData.MaxTemp - x.Obmx) ^ 2) / wp1MonSDs.Where(Function(x) x.Obmx < 999).Count)
            wthMonthData.MinTemp = Math.Sqrt(wp1MonSDs.Where(Function(x) x.Obmn < 999).Sum(Function(x) (wthMonthData.MinTemp - x.Obmn) ^ 2) / wp1MonSDs.Where(Function(x) x.Obmn < 999).Count)
            wp1MonSDs.Clear()
            '********************************************************************************
            'calculate total days per month in the whole period
            Dim day_30 As UShort = years * 30
            Dim day_31 As UShort = years * 31
            Dim day_Feb As UShort = years * 28 + years \ 4
            Dim pwd As Single = 0 'probability of wet day
            Dim b1 = 0.75
            Dim numerator, denominator As Single

            For i = 1 To 12
                month = i
                'calculate b1 
                b1 = wthMonthDatas.Where(Function(x) x.Month = month).Sum(Function(x) x.dd_wd) / wthMonthDatas.Where(Function(x) x.Month = month).Sum(Function(x) x.WetDay)
                wp1Datas(i - 1).Sdtmx = wthMonthDatas.Where(Function(x) x.Month = month).Average(Function(x) x.MaxTemp)
                wp1Datas(i - 1).Sdtmn = wthMonthDatas.Where(Function(x) x.Month = month).Average(Function(x) x.MinTemp)
                wp1Datas(i - 1).Rst2 = Math.Sqrt(wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999).Sum(Function(x) (wp1Datas(month - 1).Rmosd - x.Pcp) ^ 2) / wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999).Count)
                numerator = wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999).Sum(Function(x) (x.Pcp - wp1Datas(month - 1).Rmosd) ^ 3) / wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999).Count
                denominator = (wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999).Sum(Function(x) (x.Pcp - wp1Datas(month - 1).Rmosd) ^ 2) / (wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999).Count - 1)) ^ (3 / 2)
                wp1Datas(i - 1).Rst3 = numerator / denominator
                wp1Datas(i - 1).Uavm = wthMonthDatas.Where(Function(x) x.Month = month).Average(Function(x) x.WetDay)
                Select Case month
                    Case 1, 3, 5, 7, 8, 10, 12
                        pwd = wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999 And x.Pcp > 0).Count / day_31
                    Case 2
                        pwd = wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999 And x.Pcp > 0).Count / day_Feb
                    Case 4, 6, 9, 11
                        pwd = wthDatas.Where(Function(x) x.Month = month And x.Pcp < 999 And x.Pcp > 0).Count / day_30
                End Select
                wp1Datas(i - 1).Prw1 = b1 * pwd   'taking from http://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/nrcs143_013182.pdf page 5
                wp1Datas(i - 1).Prw2 = 1.0 - b1 + wp1Data.Prw1   'taking from http://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/nrcs143_013182.pdf page 5
            Next

            'titles
            Dim j As Short = 0
            Do While sr1.EndOfStream = False Or j < 16
                lines(j) = sr1.ReadLine
                j += 1
            Loop
            j = 0
            Dim no_rh As Boolean = False
            For Each wp1 In wp1Datas
                If j = 0 Then
                    lines(2) = Math.Round(wp1.Obmx, 2).ToString("N2").PadLeft(6)
                    lines(3) = Math.Round(wp1.Obmn, 2).ToString("N2").PadLeft(6)
                    lines(4) = Math.Round(wp1.Sdtmx, 2).ToString("N2").PadLeft(6)
                    lines(5) = Math.Round(wp1.Sdtmn, 2).ToString("N2").PadLeft(6)
                    lines(6) = Math.Round(wp1.Rmo, 1).ToString("N1").PadLeft(6)
                    lines(7) = Math.Round(wp1.Rst2, 1).ToString("N1").PadLeft(6)
                    lines(8) = Math.Round(wp1.Rst3, 2).ToString("N2").PadLeft(6)
                    lines(9) = Math.Round(wp1.Prw1, 3).ToString("N3").PadLeft(6)
                    lines(10) = Math.Round(wp1.Prw2, 3).ToString("N3").PadLeft(6)
                    lines(11) = Math.Round(wp1.Uavm, 2).ToString("N2").PadLeft(6)
                    lines(12) = Math.Round(0, 2).ToString("N2").PadLeft(6)
                    lines(13) = Math.Round(wp1.Obsl, 2).ToString("N2").PadLeft(6)
                    If pgm = "APEX" Then
                        lines(14) = Math.Round(wp1.Rh, 2).ToString("N2").PadLeft(6) : no_rh = True
                    Else
                        If wp1.Rh > 0 Then lines(14) = Math.Round(wp1.Rh, 2).ToString("N2").PadLeft(6) : no_rh = True
                    End If
                    lines(15) = Math.Round(wp1.Uav0, 2).ToString("N2").PadLeft(6)
                    j = 1
                Else
                    lines(2) &= Math.Round(wp1.Obmx, 2).ToString("N2").PadLeft(6)
                    lines(3) &= Math.Round(wp1.Obmn, 2).ToString("N2").PadLeft(6)
                    lines(4) &= Math.Round(wp1.Sdtmx, 2).ToString("N2").PadLeft(6)
                    lines(5) &= Math.Round(wp1.Sdtmn, 2).ToString("N2").PadLeft(6)
                    lines(6) &= Math.Round(wp1.Rmo, 1).ToString("N1").PadLeft(6)
                    lines(7) &= Math.Round(wp1.Rst2, 1).ToString("N1").PadLeft(6)
                    lines(8) &= Math.Round(wp1.Rst3, 2).ToString("N2").PadLeft(6)
                    lines(9) &= Math.Round(wp1.Prw1, 3).ToString("N3").PadLeft(6)
                    lines(10) &= Math.Round(wp1.Prw2, 3).ToString("N3").PadLeft(6)
                    lines(11) &= Math.Round(wp1.Uavm, 2).ToString("N2").PadLeft(6)
                    lines(12) &= Math.Round(0, 2).ToString("N2").PadLeft(6)
                    lines(13) &= Math.Round(wp1.Obsl, 2).ToString("N2").PadLeft(6)
                    If no_rh Then lines(14) &= Math.Round(wp1.Rh, 2).ToString("N2").PadLeft(6)
                    lines(15) &= Math.Round(wp1.Uav0, 2).ToString("N2").PadLeft(6)
                End If
            Next
            For line = 0 To 15
                'For Each line In lines. If lines 14-16 has information in wth file it will be calculated if not it will be zeros.
                'SR, RH, and Wind Speed. Line 13 is always zeros.
                sw.WriteLine(lines(line))
            Next
            If Not sr Is Nothing Then
                sr.Close()
                sr.Dispose()
                sr = Nothing
            End If
            If Not sr1 Is Nothing Then
                sr1.Close()
                sr1.Dispose()
                sr1 = Nothing
            End If
            If Not sw Is Nothing Then
                sw.Close()
                sw.Dispose()
                sw = Nothing
            End If

            'Return lines

            File.Copy(loc & "\" & wp1name.Trim & ".wp1", loc & "\" & wp1name.Trim & ".org", True)
            File.Copy(loc & "\" & wp1name.Trim & ".tmp", loc & "\" & wp1name.Trim & ".wp1", True)
        Catch ex As Exception
            Dim msg As String = ex.Message

        Finally
            If Not sr Is Nothing Then
                sr.Close()
                sr.Dispose()
                sr = Nothing
            End If
            If Not sr1 Is Nothing Then
                sr1.Close()
                sr1.Dispose()
                sr1 = Nothing
            End If
            If Not sw Is Nothing Then
                sw.Close()
                sw.Dispose()
                sw = Nothing
            End If
        End Try
    End Sub

    Private Sub chkGrazing_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkGrazing.CheckedChanged
        If sender.checked Then
            txtGrazing.Visible = True
            lblGrazing.Visible = True
        Else
            txtGrazing.Visible = False
            lblGrazing.Visible = False
        End If
    End Sub

    Private Sub chk_tile_drain_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chk_tile_drain.CheckedChanged
        If sender.checked Then
            txt_tile_drain.Visible = True
            lbl_tile_drain.Visible = True
        Else
            txt_tile_drain.Visible = False
            lbl_tile_drain.Visible = False
        End If
    End Sub

    Private Sub chk_autoirrigation_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chk_autoirrigation.CheckedChanged
        If sender.checked Then
            gb_autoirrigation.Visible = True
        Else
            gb_autoirrigation.Visible = False
        End If
    End Sub

    Private Sub cb_irrigation_type_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cb_irrigation_type.SelectedIndexChanged
        Select Case sender.selectedIndex
            Case 0  'Sprinkler
                txtEfficiency.Text = 70
            Case 1  'Furrow/Flood
                txtEfficiency.Text = 65
            Case 2  'Drip
                txtEfficiency.Text = 85
            Case 3  'Furrow Diking
                txtEfficiency.Text = 90
        End Select
    End Sub
End Class
