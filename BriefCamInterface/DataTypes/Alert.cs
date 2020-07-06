using System;

namespace BriefCamInterface.DataTypes
{
    public class Alert
    {
        public string AlertID { get; set; }
        public string HumanID { get; set; }
        public string LicensePlate { get; set; }
        public string PhoneNumber { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string SiteName { get; set; }
        public string WatchDirection { get; set; }
        public string Reason { get; set; }
        public WeaponAccessabilityTypes WeaponAccessabillity { get; set; }
        public AlertStatusTypes AlertStatus { get; set; }
        public string SuspectQuadName { get; set; }
        public string NameAddress { get; set; }
        public GenderTypes SuspectSex { get; set; }
        public DateTime SuspectBirthDate { get; set; }
        public int SuspectAge { get; set; }
        public int SuspectIndication { get; set; }
        public string[] SuspectPermits { get; set; }
        public string[] SuspectPrevents { get; set; }
        public string Message { get; set; }
        public AlertTypes AlertType { get; set; }
        public string AlertSubType { get; set; }
        public string AlertText { get; set; }
        public AlertRealityTypes AlertReality { get; set; }
        public int AlertRelevanceTime { get; set; }
        public AlertObjectTypes AlertObject { get; set; }
        public AlertSevirityTypes AlertSeverity { get; set; }
        public DateTime SystemAlertTime { get; set; }
        public DateTime SensorAlertTime { get; set; }
        public string SensorID { get; set; }
        public string SensorName { get; set; }
        public SensorTypes SensorType { get; set; }
        public LicenseTypes LicenseType { get; set; }
        public CarTypes CarType { get; set; }
        public string CarMakerName { get; set; }
        public string CarBrandName { get; set; }
        public string CarColorName { get; set; }
        public string EvidenceSquare { get; set; }
        public int LoiteringTime { get; set; }
        public string RuleDescription { get; set; }
        public int Confidence { get; set; }
        public int Speed { get; set; }
        public string AlertDescription { get; set; }
        public object Evidences { get; set; }
        public string EvidenceID { get; set; }
        public string EvidenceType { get; set; }
        public DateTime EvidenceLocalTime { get; set; }
        public double EvidenceLocalLong { get; set; }
        public double EvidenceLocalLat { get; set; }
        public DateTime DistributionTime { get; set; }
        public string SourceSystem { get; set; }
        public int AnalyticAlertTimeout { get; set; }
        public string CollateID { get; set; }
        public string SensorGroupID { get; set; }
        public string SensorGroupName { get; set; }
        public string SuspectGroup { get; set; }
        public string Coordinates { get; set; }
    }
}