Imports Microsoft.VisualBasic
Public Class WthData
    Private _year As UShort
    Private _month As UShort
    Private _day As UShort
    Private _maxTemp As Single
    Private _minTemp As Single
    Private _pcp As Single
    Private _wetDay As UShort
    Private _dd_wd As UShort

    Property Year() As UShort
        Get
            Return _year
        End Get
        Set(ByVal value As UShort)
            _year = value
        End Set
    End Property

    Property Month() As UShort
        Get
            Return _month
        End Get
        Set(ByVal value As UShort)
            _month = value
        End Set
    End Property
    Property Day() As UShort
        Get
            Return _day
        End Get
        Set(ByVal value As UShort)
            _day = value
        End Set
    End Property

    Property MaxTemp() As Single
        Get
            Return _maxTemp
        End Get
        Set(ByVal value As Single)
            _maxTemp = value
        End Set
    End Property

    Property MinTemp() As Single
        Get
            Return _minTemp
        End Get
        Set(ByVal value As Single)
            _minTemp = value
        End Set
    End Property

    Property Pcp() As Single
        Get
            Return _pcp
        End Get
        Set(ByVal value As Single)
            _pcp = value
        End Set
    End Property

    Property WetDay() As UShort
        Get
            Return _wetDay
        End Get
        Set(ByVal value As UShort)
            _wetDay = value
        End Set
    End Property

    Property dd_wd() As UShort
        Get
            Return _dd_wd
        End Get
        Set(ByVal value As UShort)
            _dd_wd = value
        End Set
    End Property
End Class
