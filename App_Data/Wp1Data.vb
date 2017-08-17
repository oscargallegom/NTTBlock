Imports Microsoft.VisualBasic

Public Class Wp1Data
    Private _days_obmx As UShort
    Private _obmx As Single
    Private _days_obmn As UShort
    Private _obmn As Single
    Private _sdtmx As Single
    Private _sdtmn As Single
    Private _days_rmo As UShort
    Private _rmo As Single
    Private _rmosd As Single
    Private _rst2 As Single
    Private _rst3 As Single
    Private _prw1 As Single
    Private _prw2 As Single
    Private _uavm As Single
    Private _wi As Single
    Private _days_obsl As UShort
    Private _obsl As Single
    Private _days_rh As UShort
    Private _rh As Single
    Private _days_uav0 As UShort
    Private _uav0 As Single

    Property Obmx() As Single
        Get
            Return _obmx
        End Get
        Set(ByVal value As Single)
            _obmx = value
        End Set
    End Property

    Property Obmn() As Single
        Get
            Return _obmn
        End Get
        Set(ByVal value As Single)
            _obmn = value
        End Set
    End Property

    Property Sdtmx() As Single
        Get
            Return _sdtmx
        End Get
        Set(ByVal value As Single)
            _sdtmx = value
        End Set
    End Property

    Property Sdtmn() As Single
        Get
            Return _sdtmn
        End Get
        Set(ByVal value As Single)
            _sdtmn = value
        End Set
    End Property

    Property Rmo() As Single
        Get
            Return _rmo
        End Get
        Set(ByVal value As Single)
            _rmo = value
        End Set
    End Property

    Property Rmosd() As Single
        Get
            Return _rmosd
        End Get
        Set(ByVal value As Single)
            _rmosd = value
        End Set
    End Property

    Property Rst2() As Single
        Get
            Return _rst2
        End Get
        Set(ByVal value As Single)
            _rst2 = value
        End Set
    End Property

    Property Rst3() As Single
        Get
            Return _rst3
        End Get
        Set(ByVal value As Single)
            _rst3 = value
        End Set
    End Property

    Property Prw1() As Single
        Get
            Return _prw1
        End Get
        Set(ByVal value As Single)
            _prw1 = value
        End Set
    End Property

    Property Prw2() As Single
        Get
            Return _prw2
        End Get
        Set(ByVal value As Single)
            _prw2 = value
        End Set
    End Property

    Property Uavm() As Single
        Get
            Return _uavm
        End Get
        Set(ByVal value As Single)
            _uavm = value
        End Set
    End Property

    Property Wi() As Single
        Get
            Return _wi
        End Get
        Set(ByVal value As Single)
            _wi = value
        End Set
    End Property

    Property Obsl() As Single
        Get
            Return _obsl
        End Get
        Set(ByVal value As Single)
            _obsl = value
        End Set
    End Property
    Property Rh() As Single
        Get
            Return _rh
        End Get
        Set(ByVal value As Single)
            _rh = value
        End Set
    End Property
    Property Uav0() As Single
        Get
            Return _uav0
        End Get
        Set(ByVal value As Single)
            _uav0 = value
        End Set
    End Property
    Property Days_obsl() As UShort
        Get
            Return _days_obsl
        End Get
        Set(ByVal value As UShort)
            _days_obsl = value
        End Set
    End Property
    Property Days_obmx() As UShort
        Get
            Return _days_obmx
        End Get
        Set(ByVal value As UShort)
            _days_obmx = value
        End Set
    End Property
    Property Days_obmn() As UShort
        Get
            Return _days_obmn
        End Get
        Set(ByVal value As UShort)
            _days_obmn = value
        End Set
    End Property
    Property Days_rmo() As UShort
        Get
            Return _days_rmo
        End Get
        Set(ByVal value As UShort)
            _days_rmo = value
        End Set
    End Property
    Property Days_rh() As UShort
        Get
            Return _days_rh
        End Get
        Set(ByVal value As UShort)
            _days_rh = value
        End Set
    End Property
    Property Days_Uav0() As UShort
        Get
            Return _days_uav0
        End Get
        Set(ByVal value As UShort)
            _days_uav0 = value
        End Set
    End Property

End Class
