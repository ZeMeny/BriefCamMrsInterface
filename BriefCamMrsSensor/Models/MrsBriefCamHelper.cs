using System;
using BriefCamInterface.DataTypes;
using SensorStandard.MrsTypes;

namespace BriefCamMrsSensor.Models
{
    public static class MrsBriefCamHelper
    {
        private static readonly DeviceIdentificationType deviceIdentification = new DeviceIdentificationType
        {
            DeviceName = "VideoAnalyticDevice",
            DeviceType = DeviceTypeType.VideoAnalyticSystem
        };

        private static readonly SensorIdentificationType sensorIdentification = new SensorIdentificationType
        {
            SensorName = "VideoAnalyticSensor",
            SensorType = SensorTypeType.VideoFusion // todo: sensor type???
        };

        public static IndicationType ConvertAlert(Alert alert)
        {
            return new IndicationType
            {
                CreationTime = new TimeType
                {
                    Zone = TimezoneType.GMT,
                    Value = DateTime.Now
                },
                ID = alert.AlertID,
                Item = new VideoAnalyticDetectionType
                {
                    PersonIdentification = new PersonIdentification
                    {
                        personID = alert.HumanID,
                        Permits = alert.SuspectPermits,
                        Address = alert.NameAddress,
                        Name = alert.SuspectQuadName,
                        Age = alert.SuspectAge.ToString(),
                        BirthDate = alert.SuspectBirthDate.ToString("O"),
                        GenderSpecified = true,
                        Gender = alert.SuspectSex == GenderTypes.Famale ? GenderType.Female : GenderType.Male,
                        PhoneNumber = alert.PhoneNumber,
                        Prevents = alert.SuspectPrevents,
                        Reliability = new Percent
                        {
                            Units = PercentUnitsType.Percent,
                            Value = alert.SuspectIndication
                        }
                    },
                    Message = alert.Message,
                    AlertIdentification = new AlertIdentification
                    {
                        AlertStatusSpecified = true,
                        AlertStatus = ConvertAlertStatus(alert.AlertStatus),
                        AlertDescription = alert.AlertDescription,
                        AlertReality = alert.AlertReality.ToString(),
                        AlertRelevanceTime = alert.AlertRelevanceTime.ToString(),
                        AlertSubType = alert.AlertSubType,
                        AlertType = ConvertAlertType(alert.AlertType),
                        AlertText = alert.AlertText
                    },
                    ActivityTypeSpecified = alert.WeaponAccessabillity == WeaponAccessabilityTypes.HasWeapon,
                    ActivityType = alert.WeaponAccessabillity == WeaponAccessabilityTypes.HasWeapon ? 
                        ConvertActivityType(alert.WeaponAccessabillity) : DetectionActivityType.Undefined,
                    Location = CreateMrsPoint(alert.Latitude, alert.Longitude),
                    DetectionSpeed = new VelocityType
                    {
                        Item = new Speed
                        {
                            Units = SpeedUnitsType.MetersPerSecond,
                            Value = alert.Speed * 1000 // kmh
                        }
                    },
                    RuleDescription = alert.RuleDescription,
                    SeverityLevelSpecified = true,
                    SeverityLevel = ConverSeverityLevels(alert.AlertSeverity),
                    coordinates = alert.Coordinates,
                    analyticAlertTimeout = alert.AnalyticAlertTimeout.ToString(),
                    distributionTime = new TimeType
                    {
                        Zone = TimezoneType.GMT,
                        Value = alert.DistributionTime
                    },
                    confidence = new Percent
                    {
                        Units = PercentUnitsType.Percent,
                        Value = alert.Confidence
                    },
                    collateID = alert.CollateID,
                    evidenceSquare = alert.EvidenceSquare,
                    sensorGroupID = alert.SensorGroupID,
                    StallTime = alert.LoiteringTime.ToString(),
                    sourceSystem = alert.SourceSystem,
                    sensorGroupName = alert.SensorGroupName,
                    suspectGroup = alert.SuspectGroup,
                    DetectionTypeSpecified = true,
                    DetectionType = ConvertDetectionType(alert.AlertObject),
                    VehicleIdentification = new VehicleIdentification
                    {
                        VehicleMakerName = alert.CarMakerName,
                        LicenseNumber = alert.LicensePlate,
                        IsLicensePlateSpecified = true,
                        IsLicensePlate = string.IsNullOrEmpty(alert.LicensePlate) ? StatusType.No : StatusType.Yes,
                        VehicleTypeSpecified = true,
                        VehicleType = ConvertVehicleType(alert.CarType),
                        LicenseTypeSpecified = true,
                        LicenseType = ConvertLicenseType(alert.LicenseType),
                        VehicleModelName = alert.CarBrandName,
                        // todo: color
                    },
                    SystemIdentification = new SystemIdentification
                    {
                        sensorAlertTime = new TimeType
                        {
                            Zone = TimezoneType.GMT,
                            Value = alert.SensorAlertTime
                        },
                        CameraID = alert.SensorID,
                        CameraName = alert.SensorName,
                        LocationName = alert.SiteName,
                        WatchDirection = alert.WatchDirection,
                        systemAlertTime = new TimeType
                        {
                            Zone = TimezoneType.GMT,
                            Value = alert.SensorAlertTime
                        },
                        SystemType = ConvertSensorType(alert.SensorType)
                    },
                    evidences = new []
                    {
                        new evidences
                        {
                            evidenceId = alert.EvidenceID,
                            evidenceLocalTime = new TimeType
                            {
                                Zone = TimezoneType.GMT,
                                Value = alert.EvidenceLocalTime
                            },
                            evidenceLocation = CreateMrsPoint(alert.EvidenceLocalLat, alert.EvidenceLocalLong),
                            evidenceType = alert.EvidenceType
                        }
                    }
                }
            };
        }

        public static IndicationType ConvertImage(Image image)
        {
            throw new NotImplementedException();
        }

        public static DeviceConfiguration CreateDefaultConfig(string ip, int port)
        {
            return new DeviceConfiguration
            {
                NotificationServiceIPAddress = ip,
                NotificationServicePort = port.ToString(),
                DeviceIdentification = deviceIdentification,
                MessageTypeSpecified = true,
                MessageType = MessageType.Report,
                ProtocolVersion = ProtocolVersionType.Item26,
                SensorConfiguration = new []
                {
                    new SensorConfiguration
                    {
                        SensorIdentification = sensorIdentification
                        // todo: sensor configuration
                    }
                }
            };
        }

        public static DeviceStatusReport CreateDefaultStatus()
        {
            return new DeviceStatusReport
            {
                MessageType = MessageType.Report,
                DeviceIdentification = deviceIdentification,
                Items = new object[]
                {
                    new DetailedSensorBITType
                    {
                        SensorIdentification = sensorIdentification,
                        FaultCode = new string[0]
                    },
                    new SensorStatusReport
                    {
                        SensorIdentification = sensorIdentification,
                        PowerState = StatusType.Yes,
                        SensorMode = SensorModeType.ON,
                        SensorTechnicalState = BITResultType.OK,
                        CommunicationState = BITResultType.OK,
                        // todo: sensor status
                    }, 
                }
            };
        }

        private static StatusType ConvertAlertStatus(AlertStatusTypes alertStatus)
        {
            switch (alertStatus)
            {
                case AlertStatusTypes.Active:
                    return StatusType.Activated;
                case AlertStatusTypes.Inactive:
                    return StatusType.DeActivated;
                case AlertStatusTypes.InProgress:
                    return StatusType.Undertreatment;
                case AlertStatusTypes.Closed:
                    return StatusType.Done;
                default:
                    return StatusType.Undefined;
            }
        }

        private static AnalyticAlertType ConvertAlertType(AlertTypes alertType)
        {
            switch (alertType)
            {
                case AlertTypes.BlackList:
                    return AnalyticAlertType.Blacklist;
                case AlertTypes.Rule:
                    return AnalyticAlertType.Rule;
                case AlertTypes.VideoAnalytic:
                    return AnalyticAlertType.VideoAnalytic;
                case AlertTypes.FaceDetection:
                    return AnalyticAlertType.FaceRecognition;
                default:
                    return AnalyticAlertType.Undefined;
            }
        }

        private static DetectionActivityType ConvertActivityType(WeaponAccessabilityTypes weaponAccessabilityType)
        {
            switch (weaponAccessabilityType)
            {
                case WeaponAccessabilityTypes.HasWeapon:
                    return DetectionActivityType.WeaponCarrying;
                default:
                    return DetectionActivityType.Undefined;
            }
        }

        private static Point CreateMrsPoint(double lat, double lon)
        {
            return new Point
            {
                Item = new LocationType
                {
                    Item = new GeodeticLocation
                    {
                        Latitude = new Latitude
                        {
                            Units = LatLonUnitsType.DecimalDegrees,
                            Value = lat
                        },
                        Longitude = new Longitude
                        {
                            Units = LatLonUnitsType.DecimalDegrees,
                            Value = lon
                        }
                    }
                }
            };
        }

        private static SeverityLevelsType ConverSeverityLevels(AlertSevirityTypes alertSevirity)
        {
            switch (alertSevirity)
            {
                case AlertSevirityTypes.Critical:
                    return SeverityLevelsType.Critical;
                case AlertSevirityTypes.High:
                    return SeverityLevelsType.High;
                case AlertSevirityTypes.Medium:
                    return SeverityLevelsType.Medium;
                case AlertSevirityTypes.Low:
                    return SeverityLevelsType.Low;
                default:
                    return SeverityLevelsType.Undefined;
            }
        }

        private static VehicleType ConvertVehicleType(CarTypes carType)
        {
            switch (carType)
            {
                case CarTypes.Car:
                    return VehicleType.Car;
                case CarTypes.Taxi:
                    return VehicleType.Taxi;
                case CarTypes.Truck:
                    return VehicleType.Truck;
                case CarTypes.Motorcycle:
                    return VehicleType.Bike;
                case CarTypes.Bicycle:
                    return VehicleType.Bicycle;
                case CarTypes.PickupTruck:
                    return VehicleType.Van;
                default:
                    return VehicleType.Undefined;
            }
        }

        private static LicenseType ConvertLicenseType(LicenseTypes licenseType)
        {
            switch (licenseType)
            {
                case LicenseTypes.Israeli:
                    return LicenseType.Israeli;
                case LicenseTypes.Palestinian:
                    return LicenseType.Palestinian;
                case LicenseTypes.Diplomat:
                    return LicenseType.Diplomat;
                case LicenseTypes.Army:
                    return LicenseType.Military;
                case LicenseTypes.Police:
                    return LicenseType.Police;
                default:
                    return LicenseType.Undefined;
            }
        }

        private static SensorTypeType ConvertSensorType(SensorTypes sensorType)
        {
            switch (sensorType)
            {
                case SensorTypes.VideoCamera:
                    return SensorTypeType.StaringCamera;
                case SensorTypes.LPRCameraRadar:
                    return SensorTypeType.LPR;
                case SensorTypes.Radar:
                    return SensorTypeType.Radar;
                case SensorTypes.May:
                    return SensorTypeType.AcousticRadar;
                default:
                    return SensorTypeType.Undefined;
            }
        }

        private static DetectionType ConvertDetectionType(AlertObjectTypes alertObjectType)
        {
            switch (alertObjectType)
            {
                case AlertObjectTypes.Man:
                    return DetectionType.Person;
                case AlertObjectTypes.Vehicle:
                    return DetectionType.Vehicle;
                default:
                    return DetectionType.Undefined;
            }
        }
    }
}