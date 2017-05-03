Imports Microsoft.ApplicationBlocks.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Data.OleDb

Public Class Form1
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
    Private service As New NTTBlock.ServiceSoapClient
    Private sw_log As StreamWriter = Nothing
    Private xlApp As Microsoft.Office.Interop.Excel.Application
    Private wb As Microsoft.Office.Interop.Excel.Workbook
    Private ws As Microsoft.Office.Interop.Excel.Worksheet
    Private row_number = 1

    Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        LoadStates()
        LoadControlFiles()
        LoadParmFiles()
        LoadManagementFiles()
    End Sub

    Public Sub LoadStates()
        'Dim dr As SqlDataReader = Nothing
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

    'Public Function GetStates() As SqlDataReader
    '    Dim sSQL As String = String.Empty
    '    Dim dr As SqlDataReader = Nothing

    '    Try
    '        sSQL = "SELECT * FROM State ORDER BY [Name]"
    '        dr = SqlHelper.ExecuteReader(dbConnectString("No"), CommandType.Text, sSQL)
    '        Return dr

    '    Catch ex As Exception
    '        Return dr
    '    End Try
    'End Function

    'Public Function GetCounties(state As String) As SqlDataReader
    '    'Dim sSQL As String = String.Empty
    '    'Dim dr As SqlDataReader = Nothing
    '    Dim dt As DataTable

    '    Try
    '        dt = service.GetCounties(state)
    '        'sSQL = "SELECT * FROM County WHERE StateAbrev like '" & state.Trim & "%' ORDER BY [Name]"
    '        'dr = SqlHelper.ExecuteReader(dbConnectString("No"), CommandType.Text, sSQL)
    '        Return dt

    '    Catch ex As Exception
    '        Return dr
    '    End Try
    'End Function

    'Public Function GetCountie(state As String, county As String) As SqlDataReader
    '    Dim sSQL As String = String.Empty
    '    Dim dr As SqlDataReader = Nothing

    '    Try
    '        sSQL = "SELECT * FROM County_Extended WHERE StateAbrev like '" & state.Trim & "%' AND name like '" & county.Trim & "%' ORDER BY [Name]"
    '        dr = SqlHelper.ExecuteReader(dbConnectString("No"), CommandType.Text, sSQL)
    '        Return dr

    '    Catch ex As Exception
    '        Return dr
    '    End Try
    'End Function

    ReadOnly Property dbConnectString(db As String) As String
        Get
            Dim sConnectString As String = String.Empty
            Const sDBConnectionDefault As String = "Server=DBSERVER;initial catalog=DBCATALOG;persist security info=False"
            Dim sServer As String = "T-NN"
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

        Return msg
    End Function

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
        Dim slope As Single = 0
        Dim name As String = String.Empty
        Dim key As Integer = 0
        Dim errors As Boolean = False
        Dim msg As String = "OK"

        Try
            lblMessage.Text = validate_inputs()
            If lblMessage.Text <> "" Then errors = True : lblMessage.ForeColor = Color.Red : Exit Sub
            'create the excel file in memory before the simulations start.
            create_excel_file("Sheet1")
            'Read all of the counties selected and take code and center coordinates.
            For i = 1 To clBox.Items.Count - 1
                If clBox.GetItemCheckState(i) = CheckState.Checked Then
                    county_info = CoutyInfo("SELECT TOP 1 * FROM County_Extended WHERE StateAbrev like '" & Split(cbStates.SelectedItem, "-")(1).Trim & "%' AND [Name] like '" & clBox.Items(i).Trim & "%' ORDER BY [Name]")
                    weather_info = GetWeatherInfo(county_info.lat, county_info.lon)
                    ssas = service.GetSSA(county_info.code)
                    APEXFolders(cbControl.SelectedItem, cbParm.SelectedItem)
                    'initialize log file
                    sw_log = New StreamWriter(apex_current & "\log.log")
                    create_Weather_file(weather_info.name)
                    For Each ssa In ssas.Rows
                        soils = service.GetSoils(ssa("code"), county_info.code, txtMaxSlope.Text)
                        series_name = soils.Rows(0)("seriesName")
                        For Each soil In soils.Rows
                            lblMessage.Text = "Running County => " & clBox.Items(i) & " - SSA => " & ssa("Code") & " - Soil => " & soil("series")
                            lblMessage.ForeColor = Color.Green
                            If depth > soil("ldep") And series_name <> soil("seriesName") Then
                                layer_number = 0 : If Not swSoil Is Nothing Then
                                    print_layers()
                                    swSoil.Close()
                                    'create subarea
                                    create_subarea_file(slope / 100)
                                    'copy the operation file one by one from the management list and then run the simulation
                                    For Each mgt In clbManagement.CheckedItems
                                        If mgt.ToString.Contains("Select") Then Continue For
                                        copy_management_file(mgt)
                                        msg = run_apex(cbStates.SelectedItem, county_info.code, ssa("code"), name, series_name, key, slope, mgt, cbParm.SelectedItem, cbControl.SelectedItem, 0)
                                        If msg <> "OK" Then
                                            Throw New Global.System.Exception("Error running APEX program")
                                        End If
                                        series_name = soil("SeriesName")
                                        'GoTo controls
                                    Next
                                End If
                            End If
                            If Not (depth = soil("ldep") And series_name = soil("seriesName")) Then
                                If layer_number = 0 Then
                                    slope = (soil("slopel") + soil("slopeh")) / 2
                                    name = soil("series")
                                    key = soil("muid")
                                    layers = New layer_info
                                    swSoil = New StreamWriter(apex_current & "\APEX.sol")
                                End If
                                depth = soil("ldep")
                                layer_number += 1
                                create_soils(soil, layer_number)
                            End If
                        Next
                    Next
                End If
            Next
Controls:
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

            If Not sw_log Is Nothing Then
                sw_log.Close()
                sw_log.Dispose()
                sw_log = Nothing
            End If
            If errors = False Then SaveFile(Directory.GetCurrentDirectory(), "Results.xls")
        End Try
    End Sub

    Public Function CoutyInfo(sSql As String) As county_info
        Dim c_i As New county_info
        'Dim dr As SqlDataReader = Nothing
        Dim dr As DataTable
        dr = service.GetRecord(sSql)
        c_i.lat = dr.Rows(0).Item("lat")
        c_i.lon = dr.Rows(0).Item("long")
        c_i.code = dr.Rows(0).Item("code")
        c_i.wind_code = dr.Rows(0).Item("windName").ToString.Split(",")(0)
        c_i.wind_name = dr.Rows(0).Item("windName").ToString.Split(",")(1)
        c_i.wp1_code = dr.Rows(0).Item("wp1Name").ToString.Split(",")(0)
        c_i.wp1_name = dr.Rows(0).Item("wp1Name").ToString.Split(",")(1)

        Return c_i
    End Function

    Private Sub clBox_Click(sender As Object, e As System.EventArgs) Handles clBox.SelectedIndexChanged
        'Dim checked As CheckState = CheckState.Checked

        If sender.selectedIndex = 0 Then
            'If sender.GetItemCheckState(0) = CheckState.Checked Then
            '    checked = CheckState.Checked
            'End If
            For i = 1 To clBox.Items.Count - 1
                clBox.SetItemChecked(i, sender.GetItemCheckState(0))
            Next
        End If
        'If sender.GetItemCheckState(sender.selectedIndex) = CheckState.Checked Then
        '    checked = CheckState.Unchecked
        'End If
        'clBox.SetItemChecked(sender.selectedIndex, checked)
    End Sub

    'Public Function GetWeatherCoor(ByVal nlat As Single, ByVal nlon As Single, ByVal latLess As Single, ByVal latPlus As Single, ByVal lonLess As Single, ByVal lonPlus As Single) As SqlDataReader
    '    Dim sSQL As String = String.Empty
    '    Dim dr As SqlDataReader = Nothing

    '    Try
    '        sSQL = "SELECT TOP 1 lat, lon, FileName, initialYear, finalYear, (lat - " & nlat & ") + (lon + " & nlon & ") as distance FROM weatherCoor " & _
    '               "WHERE lat > " & latLess & " and lat < " & latPlus & " and lon > " & lonLess & " and lon < " & lonPlus & " ORDER BY distance"
    '        dr = SqlHelper.ExecuteReader(dbConnectString("No"), CommandType.Text, sSQL)
    '        Return dr

    '    Catch ex As Exception
    '        Return dr
    '    End Try
    'End Function

    Public Function GetWeatherInfo(nlat As Single, nlon As Single) As weather_info
        Dim w_i As New weather_info
        Const latDif As Single = 0.04
        Const lonDif As Single = 0.09
        Dim latLess, latPlus, lonLess, lonPlus As Double
        Dim weatherPrismFiles As String = "E:\Weather\weatherFiles\US"

        latLess = nlat - latDif : latPlus = nlat + latDif
        lonLess = nlon - lonDif : lonPlus = nlon + lonDif
        Dim weatherFileQuery As DataTable = service.GetWeatherfileName(nlat, nlon, latLess, latPlus, lonLess, lonPlus)

        w_i.name = weatherPrismFiles & "\" & weatherFileQuery.Rows(0).Item("fileName")
        w_i.finalYear = weatherFileQuery.Rows(0).Item("finalYear")
        w_i.initialYear = weatherFileQuery.Rows(0).Item("initialYear")

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
            Directory.Delete(apex_current, True)
        End If
        Do
            If Not Directory.Exists(apex_current) Then Exit Do
        Loop
        Directory.CreateDirectory(apex_current)
        directoryFiles = Directory.GetFiles(apex_default)
        For Each textFile In directoryFiles
            currentFile = Path.GetFileName(textFile)
            If Not (currentFile.ToLower = "apexcont.dat" Or currentFile.ToLower = "parm.dat") Then
                Select Case True
                    Case currentFile = control_file
                        File.Copy(textFile, apex_current & "\apexcont.dat", True)
                    Case currentFile = parm_file
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
                    nStartYear = item.Substring(0, 4)
                    first = False
                End If
                sw.WriteLine(item)
            Next

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
        If soil("ldep") Is Nothing Or soil("ldep") <= 0 Then Exit Sub
        ReDim Preserve layers.z(layer_number - 1)
        layers.z(layer_number - 1) = soil("ldep") * IN_TO_CM

        ReDim Preserve layers.bd(layer_number - 1)
        If Not IsDBNull(soil("bd")) AndAlso Not soil("bd") <= 0 Then
            layers.bd(layer_number - 1) = soil("bd")
        Else
            If layer_number - 1 > 1 Then
                layers.bd(layer_number - 1) = layers.bd(layer_number - 1 - 2)
            Else
                layers.bd(layer_number - 1) = BD_MIN
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
            End If
        End If

        ReDim Preserve layers.sil(layer_number - 1)
        If Not IsDBNull(soil("silt")) AndAlso Not soil("silt") <= 0 Then
            layers.sil(layer_number - 1) = soil("silt")
        Else
            If layer_number - 1 > 1 Then
                layers.sil(layer_number - 1) = layers.sil(layer_number - 1 - 2)
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
            End If
        End If

    End Sub

    Private Sub print_layers()
        Dim twice As Boolean = False
        Try
            sw_log.WriteLine("start Printing Layers")
            'if there are layers those are printed for the current soil before it goes to the nex soil
            If layers.z.Length > 0 Then
                If layers.z(0) > 10 Then  'if first layer is > than 10 cm then and new layer is added at the begining
                    twice = True
                End If
                If twice = True Then
                    swSoil.Write("{0,8:N2}", 10 / 100)
                    sw_log.Write("{0,8:N2}", 10 / 100)
                End If
                For Each z In layers.z  'depth
                    swSoil.Write("{0,8:N2}", z / 100)
                    sw_log.Write("{0,8:N2}", 10 / 100)
                Next
                swSoil.WriteLine()
                sw_log.WriteLine("{0,8:N2}", 10 / 100)

                If twice = True Then
                    swSoil.Write("{0,8:N2}", layers.bd(0) / 100)
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
                sw_log.WriteLine("end printing layers")

            End If

        Catch ex As Exception
            sw_log.WriteLine("Error printing layers => " & ex.Message)
        End Try

    End Sub

    Private Sub create_subarea_file(slope As Single)
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
            swSubarea.WriteLine("   0.000    0.00    0.00    0.00    0.00    0.00")
            swSubarea.WriteLine("  00   0   0   0   0   0   0   0   0   0   0")
            swSubarea.WriteLine("    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            swSubarea.WriteLine("    1.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00    0.00")
            swSubarea.WriteLine("   0   0   0   0")
            swSubarea.WriteLine("    0.00    0.00    0.00    0.00")

        Catch ex As Exception
        Finally
            If Not swSubarea Is Nothing Then
                swSubarea.Close()
                swSubarea.Dispose()
                swSubarea = Nothing
            End If
        End Try
    End Sub

    Private Function run_apex(state As String, county As String, ssa As String, soil_name As String, soil_component As String, soil_key As Integer, slope As Single, mgt As String, parm_file As String, control_file As String, i As UShort) As String
        Dim APEXResults1 As ScenariosData.APEXResults
        Dim msg = "OK"
        Try
            sw_log.WriteLine("Start running process")
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

            'Run APEX0604 Baseline
            _result.message = doAPEXProcess(apex_current & "\RunAPEX.bat")
            If _result.message <> "OK" Then
                sw_log.WriteLine("End runing process with error => " & _result.message)
                Throw New Global.System.Exception("Error running APEX program - " & _result.message)
            Else
                sw_log.WriteLine("End runing process")
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
            sw_log.WriteLine("End results totaly")
            Save_results(APEXResults1, i)
            Return msg
        Catch ex As Exception
            msg = ex.Message
            Return msg
        End Try

    End Function

    Private Sub copy_management_file(mgt As String)
        Try
            lblMessage.Text &= " - Management => " & mgt
            lblMessage.ForeColor = Color.Green
            sw_log.WriteLine(lblMessage.Text)
            File.Copy(apex_current & "\" & mgt, apex_current & "\APEX.opc", True)
        Catch ex As Exception
        Finally
        End Try
    End Sub

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
        Dim i, Sub1, k, m(0), n, APEXStartYear, l, index As Integer
        Dim APEXResults1 As New ScenariosData.APEXResults
        Dim found As Boolean
        Dim totalArea As Single
        Dim resultFS(11) As Single
        Dim srfile As StreamReader
        Dim ddCYR1 As Integer = 0
        Dim ddNBYR1 As Integer = 0
        Dim crop1() As ScenariosData.Crops
        Dim dr As SqlDataReader = Nothing
        Const ha_ac As Single = 2.471053814672
        'Dim dsResults As New DataSet

        Try
            sw_log.WriteLine("Estar reading results")

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

                        If Sub1 <> 0 Then
                            APEXResults1.SoilResults(Sub1).deepPerFlow = APEXResults1.SoilResults(Sub1).deepPerFlow + Val(Mid(tempa, 136, 9)) * mm_in
                            APEXResults1.SoilResults(Sub1).LeachedP = APEXResults1.SoilResults(Sub1).LeachedP + (Val(Mid(tempa, 110, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(Sub1).LeachedN = APEXResults1.SoilResults(Sub1).LeachedN + (Val(Mid(tempa, 14, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(Sub1).volatizationN = APEXResults1.SoilResults(Sub1).volatizationN + (Val(Mid(tempa, 23, 9)) * kg_lbs / ha_ac)
                            'APEXResults1.SoilResults(Sub1).co2 = APEXResults1.SoilResults(Sub1).co2 + Val(Mid(tempa, 163, 9)) * kg_lbs / ha_ac
                            APEXResults1.SoilResults(Sub1).n2o = APEXResults1.SoilResults(Sub1).n2o + Val(Mid(tempa, 154, 9)) * kg_lbs / ha_ac

                            'APEXResults1.SoilResults(0).tileDrainFlow = APEXResults1.SoilResults(0).tileDrainFlow + Val(Mid(tempa, 127, 9)) * mm_in
                            APEXResults1.SoilResults(0).deepPerFlow = APEXResults1.SoilResults(0).deepPerFlow + Val(Mid(tempa, 136, 9)) * mm_in
                            APEXResults1.SoilResults(0).LeachedP = APEXResults1.SoilResults(0).LeachedP + (Val(Mid(tempa, 110, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).LeachedN = APEXResults1.SoilResults(0).LeachedN + (Val(Mid(tempa, 14, 9)) * kg_lbs / ha_ac)
                            'APEXResults1.SoilResults(0).tileDrainN = APEXResults1.SoilResults(0).tileDrainN + (Val(Mid(tempa, 145, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).volatizationN = APEXResults1.SoilResults(0).volatizationN + (Val(Mid(tempa, 23, 9)) * kg_lbs / ha_ac)
                            APEXResults1.SoilResults(0).n2o = APEXResults1.SoilResults(0).n2o + Val(Mid(tempa, 154, 9)) * kg_lbs / ha_ac
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
                            If APEXResults1.SoilResults(0).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).Yields(i)
                            APEXResults1.SoilResults(0).Yields(i) = APEXResults1.SoilResults(0).Yields(i) + ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
                            If APEXResults1.SoilResults(Sub1).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(Sub1).Yields(i)
                            APEXResults1.SoilResults(Sub1).Yields(i) = APEXResults1.SoilResults(Sub1).Yields(i) + ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
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
                        If APEXResults1.SoilResults(0).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).Yields(i)
                        APEXResults1.SoilResults(0).Yields(i) = ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
                        If APEXResults1.SoilResults(0).CountCrops.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(0).CountCrops(i)
                        APEXResults1.SoilResults(0).CountCrops(i) = APEXResults1.SoilResults(0).CountCrops(i) + 1
                        If APEXResults1.SoilResults(Sub1).Yields.Length - 1 < i Then ReDim Preserve APEXResults1.SoilResults(Sub1).Yields(i)
                        APEXResults1.SoilResults(Sub1).Yields(i) = ((Val(Mid(tempa, 101, 9)) + Val(Mid(tempa, 92, 9))))
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
                APEXResults1.SoilResults(n).LeachedN = APEXResults1.SoilResults(n).LeachedN / (m(n) * index)
                APEXResults1.SoilResults(n).tileDrainN = APEXResults1.SoilResults(n).tileDrainN / m(n)
                APEXResults1.SoilResults(n).tileDrainP = APEXResults1.SoilResults(n).tileDrainP / m(n)
                APEXResults1.SoilResults(n).volatizationN = APEXResults1.SoilResults(n).volatizationN / (m(n) * index)
                APEXResults1.SoilResults(n).n2o = APEXResults1.SoilResults(n).n2o / (m(n) * index)
                APEXResults1.SoilResults(n).co2 = APEXResults1.SoilResults(n).co2 / (m(n))  'do not use index because the total is comming from field 0 (total for the field)

                'CALCULATE tile drain P
                If APEXResults1.SoilResults(n).tileDrainN > 0 Then
                    If APEXResults1.SoilResults(n).LeachedN > 0 Then
                        APEXResults1.SoilResults(n).tileDrainP = APEXResults1.SoilResults(n).tileDrainN * 0.07

                        'sTemp1 = APEXResults1.SoilResults(n).LeachedN / APEXResults1.SoilResults(n).tileDrainN * 100
                        'sTemp2 = 100 - sTemp1
                        'sTemp1 = APEXResults1.SoilResults(n).LeachedP * sTemp2 / sTemp1
                        'APEXResults1.SoilResults(n).tileDrainP = sTemp1 - APEXResults1.SoilResults(n).LeachedP
                    Else
                        APEXResults1.SoilResults(n).tileDrainP = APEXResults1.SoilResults(n).tileDrainN * 0.07
                    End If
                Else
                    APEXResults1.SoilResults(n).tileDrainP = 0
                End If
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
            For k = 0 To APEXResults1.Crops.GetUpperBound(0)
                ReDim Preserve crop1(k)
                Dim foundSilage As Boolean = False
                crop1(k).cropYieldBase = 0
                crop1(k).cropYieldAlt = 0
                crop1(k).cropDryMatter = 0
                crop1(k).cropName = APEXResults1.Crops(k)
                '//Take new unit and the convertion factor
                'If sState = "MD" Then
                '    dr = GetRecords("SELECT * FROM " & sState & "APEXCrops WHERE CropCode LIKE" & "'" & crop1(k).cropName.ToString.Trim & "%'")
                'Else
                '    dr = GetRecords("SELECT * FROM APEXCrops WHERE CropCode LIKE" & "'" & crop1(k).cropName.ToString.Trim & "%'")
                'End If
                If dr.HasRows = True Then
                    dr.Read()
                    'Dim cropx As Short
                    'For Each cropx In cropSillage
                    '    If cropx = dr.Item("cropNumber") Then
                    '        foundSilage = True
                    '    End If
                    'Next
                    crop1(k).cropCode = dr.Item("cropNumber")
                    If foundSilage = False Then
                        crop1(k).cropYieldUnit = dr.Item("yieldUnit")
                        crop1(k).cropYieldFactor = dr.Item("conversionFactor") / ha_ac
                        crop1(k).cropDryMatter = dr.Item("DryMatter")
                    Else
                        crop1(k).cropYieldUnit = "t"
                        crop1(k).cropYieldFactor = 1 / ha_ac
                        crop1(k).cropDryMatter = 33
                    End If
                Else
                    crop1(k).cropYieldUnit = "t"
                    crop1(k).cropYieldFactor = 1 / ha_ac
                    crop1(k).cropDryMatter = 100
                End If
                If crop1(k).cropYieldUnit.Trim = "t" Then
                    crop1(k).cropYieldBase = Math.Round(APEXResults1.SoilResults(0).Yields(k) * crop1(k).cropYieldFactor / (crop1(k).cropDryMatter / 100), 2)
                Else
                    crop1(k).cropYieldBase = Math.Round(APEXResults1.SoilResults(0).Yields(k) * crop1(k).cropYieldFactor / (crop1(k).cropDryMatter / 100), 0)
                End If

                If k <> 0 Then
                    'rowlinkNew = dsResults.Tables("Crops").NewRow
                    'rowlinkNew("CropCode") = crop1(k).cropCode
                    'rowlinkNew("Crop") = crop1(k).cropName
                    'rowlinkNew("Yield") = crop1(k).cropYieldBase
                    'rowlinkNew("Unit") = crop1(k).cropYieldUnit.Trim
                    'dsResults.Tables("Crops").Rows.Add(rowlinkNew)
                    'If foundSilage = True Then rowlinkNew("Crop") &= " (Silage)"
                Else
                    'rowLink("CropCode") = crop1(k).cropCode
                    'rowLink("Crop") = crop1(k).cropName
                    'rowLink("Yield") = crop1(k).cropYieldBase
                    'rowLink("Unit") = crop1(k).cropYieldUnit.Trim
                    'If foundSilage = True Then rowLink("Crop") &= " (Silage)"
                End If

            Next

            'dsResults.WriteXml(configurationAppSettings.Get("Results").ToString.Trim & session & ".xml")
            APEXResults1.message = "OK"
            Return APEXResults1
            sw_log.WriteLine("End reading results")

        Catch ex As Exception
            APEXResults1.message = "Error 405 - " & ex.Message & " Read results process.'"
            sw_log.WriteLine("Error reading results")
            Return APEXResults1
        End Try

    End Function

    Private Sub create_excel_file(sheet As String)
        xlApp = CreateObject("Excel.Application")
        wb = xlApp.Workbooks.Add
        ws = wb.Worksheets(sheet) 'Specify your worksheet name
        row_number = 1
        Dim i As UShort = row_number
        Dim j As UShort = 1

        ws.Cells._Default(i, j) = "ID"
        j += 1
        ws.Cells._Default(i, j) = "State"
        j += 1
        ws.Cells._Default(i, j) = "County"
        j += 1
        ws.Cells._Default(i, j) = "SSA"
        j += 1
        ws.Cells._Default(i, j) = "Soil Name"
        j += 1
        ws.Cells._Default(i, j) = "Soil Key"
        j += 1
        ws.Cells._Default(i, j) = "Soil Component"
        j += 1
        ws.Cells._Default(i, j) = "Slope"
        j += 1
        ws.Cells._Default(i, j) = "Management"
        j += 1
        ws.Cells._Default(i, j) = "Parm File"
        j += 1
        ws.Cells._Default(i, j) = "Control File"
        j += 1
        ws.Cells._Default(i, j) = "Org N"
        j += 1
        ws.Cells._Default(i, j) = "NO3_N"
        j += 1
        ws.Cells._Default(i, j) = "Tile Drain N"
        j += 1
        ws.Cells._Default(i, j) = "Org P"
        j += 1
        ws.Cells._Default(i, j) = "PO4_P"
        j += 1
        ws.Cells._Default(i, j) = "Tile Drain P"
        j += 1
        ws.Cells._Default(i, j) = "Flow"
        j += 1
        ws.Cells._Default(i, j) = "Tile Drain Flow"
        j += 1
        ws.Cells._Default(i, j) = "Sediment"
        j += 1
        ws.Cells._Default(i, j) = "Manure Erosion"
        j += 1
        ws.Cells._Default(i, j) = "Deep Percolation"
        j += 1
        ws.Cells._Default(i, j) = "N2O"
        j += 1
        ws.Cells._Default(i, j) = "CO2"
        j += 1
        ws.Cells._Default(i, j) = "Crop1"
        j += 1
        ws.Cells._Default(i, j) = "Crop Yield1"
        j += 1
        ws.Cells._Default(i, j) = "Crop2"
        j += 1
        ws.Cells._Default(i, j) = "Crop Yield2"
        j += 1
        ws.Cells._Default(i, j) = "Crop3"
        j += 1
        ws.Cells._Default(i, j) = "Crop Yield3"
        j += 1
        ws.Cells._Default(i, j) = "Crop4"
        j += 1
        ws.Cells._Default(i, j) = "Crop Yield4"
    End Sub

    Public Sub Save_results(results As ScenariosData.APEXResults, id As UShort)
        Dim i As UShort
        Dim j As UShort = 1
        row_number += 1
        Try
            sw_log.WriteLine("Start Saving REsults")
            i = row_number
            If id > 0 Then
                ws.Cells._Default(i, j) = id
            Else
                ws.Cells._Default(i, j) = i - 1
            End If
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.state
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.county
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.ssa
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.soil_name
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.soil_key
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.soil_component
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.soil_slope
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.management
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.param_file
            j += 1
            ws.Cells._Default(i, j) = results.OtherInfo.control_file
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).OrgN
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).NO3
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).tileDrainN
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).OrgP
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).PO4
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).tileDrainP
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).flow
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).tileDrainFlow
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).Sediment
            j += 1
            ws.Cells._Default(i, j) = 0  'no set for now
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).deepPerFlow
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).n2o
            j += 1
            ws.Cells._Default(i, j) = results.SoilResults(0).co2
            j += 1
            'todo check what happen with crops
            For k = 0 To results.Crops.Count - 1
                ws.Cells._Default(i, j) = results.Crops(k)
                j += 1
                ws.Cells._Default(i, j) = results.SoilResults(0).Yields(k)
                j += 1
            Next
            sw_log.WriteLine("End Saving REsults")

        Catch ex As Exception
            sw_log.WriteLine("Problem Saving Results")
        Finally

        End Try
    End Sub

    Private Sub SaveFile(filePath As String, fileName As String)
        Dim fileToSave As String = Path.Combine(filePath, fileName)
        Try
            If Dir(fileToSave) <> "" Then
                File.Delete(fileToSave)
            End If
            wb.SaveAs(fileToSave)
            MsgBox("Your " & fileName.Replace(".xls", "") & " File has been Saved as " & fileToSave, MsgBoxStyle.OkOnly, Me.Name & fileName.Replace(".xls", "") & "_Click")
        Catch ex As Exception
            MsgBox(ex.Message & " - " & fileToSave, MsgBoxStyle.OkOnly, Me.Name & fileName & " _Click")
        Finally
            releaseObject(xlApp, 0)
            releaseObject(wb, 1)
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
        Dim results_file As String = Path.Combine(Directory.GetCurrentDirectory(), "Results.xls")
        Try
            gbRuns.Visible = True
            gbInitialRun.Visible = False

            xlApp = New Microsoft.Office.Interop.Excel.Application
            wb = xlApp.Workbooks.Open(results_file)
            ws = wb.Worksheets("Sheet1") 'Specify your worksheet name
            Dim i As UShort = 2
            If results_file Is Nothing Or results_file = "" Or results_file = String.Empty Then Exit Sub
            clbRuns.Items.Clear()
            With ws
                Do While .Cells(i, 1).value <> 0 And Not (.Cells(i, 1).value Is Nothing)
                    clbRuns.Items.Add(.Cells(i, 1).value)
                    i += 1
                Loop
            End With

        Catch ex As Exception
        Finally
            releaseObject(xlApp, 0)
            releaseObject(wb, 1)
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
        Dim results_file As String = Path.Combine(Directory.GetCurrentDirectory(), "Results.xls")
        Dim i As UShort = 2
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

        Try
            If clbRuns.CheckedItems.Count <= 0 Then lblMessage.Text = "Please check one id from the runs list" : lblMessage.ForeColor = Color.Red : Exit Sub
            If results_file Is Nothing Or results_file = "" Or results_file = String.Empty Then Exit Sub
            xlApp = New Microsoft.Office.Interop.Excel.Application
            wb = xlApp.Workbooks.Open(results_file)
            ws = wb.Worksheets("Sheet1") 'Specify your worksheet name

            With ws
                Do While .Cells(i, 1).value <> 0 And Not (.Cells(i, 1).value Is Nothing)
                    If .Cells(i, 1).value = clbRuns.SelectedItem Then
                        'create the excel file in memory before the simulations start.
                        county_code = .Cells(i, 3).value
                        'Read all of the counties selected and take code and center coordinates.
                        Dim sSql As String = "SELECT TOP 1 * FROM County_Extended WHERE StateAbrev like '" & Split(.Cells(i, 2).value, "-")(1).Trim & "%' AND [Code] like '" & county_code.Trim & "%' ORDER BY [Name]"

                        county_info = CoutyInfo(sSql)
                        weather_info = GetWeatherInfo(county_info.lat, county_info.lon)
                        ssa_code = .Cells(i, 4).value
                        APEXFolders(.Cells(i, 11).value, .Cells(i, 10).value)
                        'initialize log file
                        sw_log = New StreamWriter(apex_current & "\log.log")
                        create_Weather_file(weather_info.name)
                        lblMessage.Text = "Running County => " & county_code & " - SSA => " & ssa_code & " - Soil => " & .Cells(i, 5).value
                        lblMessage.ForeColor = Color.Green
                        mgt = .Cells(i, 9).value
                        params = .Cells(i, 10).value
                        control = .Cells(i, 11).value
                        name = .Cells(i, 5).value
                        component = .Cells(i, 7).value
                        muid = .Cells(i, 6).value
                        slope = .Cells(i, 8).value
                        i = .Cells(i, 1).value  'take the row number from the simulations to put in the individual run.
                        state = .Cells(i, 2).value
                        releaseObject(xlApp, 0)
                        releaseObject(wb, 1)
                        create_excel_file("Sheet1")
                        sSql = "SELECT * FROM " & county_code.Substring(0, 2) & "SOILS WHERE TSSSACode = '" & ssa_code & "' AND TSCountyCode = '" & county_code & "' AND muid = " & muid & " AND seriesname = '" & component & "' ORDER BY [ldep]"
                        soils = service.GetSoilRecord(sSql)
                        For Each soil In soils.Rows
                            If depth > soil("ldep") Then
                                layer_number = 0
                                layers = New layer_info
                                swSoil = New StreamWriter(apex_current & "\APEX.sol")
                                'create subarea
                                create_subarea_file((soil("slopel") + soil("slopeh")) / 2 / 100)
                                'copy the operation file one by one from the management list and then run the simulation
                                copy_management_file(mgt)
                            End If
                            If Not (depth = soil("ldep")) Then
                                depth = soil("ldep")
                                layer_number += 1
                                create_soils(soil, layer_number)
                            End If
                        Next
                        print_layers()
                        swSoil.Close()
                        'If Not swSoil Is Nothing Then
                        msg = run_apex(state, county_info.code, ssa_code, name, component, muid, slope, mgt, params, control, i)
                        'End If
                        lblMessage.Text = "Simulations finished succesfully"
                        lblMessage.ForeColor = Color.Green
                        Exit Do
                    End If
                    i += 1
                Loop
            End With

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

            If Not sw_log Is Nothing Then
                sw_log.Close()
                sw_log.Dispose()
                sw_log = Nothing
            End If
            If clbRuns.CheckedItems.Count > 0 Then SaveFile(Directory.GetCurrentDirectory(), "Results_individual.xls")
        End Try
    End Sub
End Class
