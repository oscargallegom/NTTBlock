Public Class ScenariosData
    Public Structure Crops
        Public cropCode As Short
        Public cropName As String '//REV# 007
        Public cropYieldBase As Single
        Public cropYieldAlt As Single
        Public cropYieldUnit As String
        Public cropYieldFactor As Single
        Public cropDryMatter As Single
        'Public biomas As Single
    End Structure

    Public Structure APEXResults
        Public area As Single
        Public FIBFertilizer As String
        Public i As Integer
        Public FIAFertilizer As String
        Public Crops() As String
        Public SoilResults() As APEXResultsAll
        Public OtherInfo As OtherInfo
        Public message As String
    End Structure

    Public Structure APEXResultsAll
        Public LeachedP As Single
        Public OrgP As Single
        Public PO4 As Single
        Public Sediment As Single
        Public Yields() As Single
        Public CountCrops() As Integer
        Public OrgN As Single
        Public NO3 As Single
        Public LeachedN As Single
        Public volatizationN As Single
        Public flow As Single
        Public n2o As Single
        Public tileDrainN As Single
        Public tileDrainP As Single
        Public tileDrainFlow As Single
        Public deepPerFlow As Single
        Public co2 As Single
        Public percolation As Single
        Public water_yield As Single
        Public pet As Single
        Public et As Single
        Public soil_water As Single
        Public pcp As Single
        Public bioms() As Single
    End Structure

    Public Structure OtherInfo
        Public state
        Public county
        Public ssa
        Public soil_name
        Public soil_key
        Public soil_component
        Public soil_slope
        Public management
        Public param_file
        Public control_file
    End Structure
End Class
