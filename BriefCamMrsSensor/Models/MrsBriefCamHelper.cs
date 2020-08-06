using System;
using System.Collections.Generic;
using System.Linq;
using BriefCamInterface.DataTypes;
using BriefCamMrsSensor.ViewModels;
using Newtonsoft.Json;
using SensorStandard.MrsTypes;

namespace BriefCamMrsSensor.Models
{
    public static class MrsBriefCamHelper
    {

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static Point CreateMrsPoint(double lat, double lon)
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


        private static readonly DeviceIdentificationType deviceIdentification = new DeviceIdentificationType
        {
            DeviceName = "VideoAnalyticDevice",
            DeviceType = DeviceTypeType.VideoAnalyticSystem
        };

        private static readonly SensorIdentificationType sensorIdentification = new SensorIdentificationType
        {
            SensorName = "VideoAnalyticCamera",
            SensorType = SensorTypeType.VideoAnalyticCamera
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
                        BirthDate = new TimeType
                        {
                            Zone = TimezoneType.GMT,
                            Value = alert.SuspectBirthDate
                        },
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
                    AnalyticAlertTimeout = alert.AnalyticAlertTimeout.ToString(),
                    DistributionTime = new TimeType
                    {
                        Zone = TimezoneType.GMT,
                        Value = alert.DistributionTime
                    },
                    Confidence = new Percent
                    {
                        Units = PercentUnitsType.Percent,
                        Value = alert.Confidence
                    },
                    CollateID = alert.CollateID,
                    EvidenceSquare = alert.EvidenceSquare,
                    SiteID = alert.SensorGroupID,
                    StallTime = alert.LoiteringTime.ToString(),
                    SourceSystem = alert.SourceSystem,
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
                        LicensePlateTypeSpecified = true,
                        LicensePlateType = ConvertLicenseType(alert.LicenseType),
                        VehicleModelName = alert.CarBrandName,
                        Color = alert.CarColorName
                    },
                    SystemIdentification = new SystemIdentification
                    {
                        SensorAlertTime = new TimeType
                        {
                            Zone = TimezoneType.GMT,
                            Value = alert.SensorAlertTime
                        },
                        CameraID = alert.SensorID,
                        CameraName = alert.SensorName,
                        LocationName = alert.SiteName,
                        FacingDirection = alert.WatchDirection,
                        SystemAlertTime = new TimeType
                        {
                            Zone = TimezoneType.GMT,
                            Value = alert.SensorAlertTime
                        },
                        SystemType = ConvertSensorType(alert.SensorType)
                    },
                    Evidences = new []
                    {
                        new Evidences
                        {
                            EvidenceId = alert.EvidenceID,
                            EvidenceLocalTime = new TimeType
                            {
                                Zone = TimezoneType.GMT,
                                Value = alert.EvidenceLocalTime
                            },
                            EvidenceLocation = CreateMrsPoint(alert.EvidenceLocalLat, alert.EvidenceLocalLong),
                            EvidenceType = alert.EvidenceType
                        }
                    }
                }
            };
        }

        public static IndicationType ConvertImage(Image image, AlertData alertData)
        {
            List<File> imageFiles = new List<File>();
            if (image.Image1 != null)
            {
                imageFiles.Add(new File
                {
                    File1 = image.Image1,
                    ItemElementName = ItemChoiceType3.NameJPEG,
                    Item = "image1.jpg"
                });
            }
            if (image.Image2 != null)
            {
                imageFiles.Add(new File
                {
                    File1 = image.Image2,
                    ItemElementName = ItemChoiceType3.NameJPEG,
                    Item = "image2.jpg"
                });
            }
            if (image.Image3 != null)
            {
                imageFiles.Add(new File
                {
                    File1 = image.Image3,
                    ItemElementName = ItemChoiceType3.NameJPEG,
                    Item = "image3.jpg"
                });
            }

            List<File> videoFiles = new List<File>();
            if (image.VideoClip != null)
            {
                videoFiles.Add(new File
                {
                    File1 = image.VideoClip,
                    ItemElementName = ItemChoiceType3.NameMP4,
                    Item = "videoClip.mp4"
                });
            }
            var detection = new VideoAnalyticDetectionType
            {
                Location = alertData.Location,
                Picture = imageFiles.Count > 0 ?  imageFiles.ToArray() : null,
                Video = videoFiles.Count > 0 ? videoFiles.ToArray() : null,
                DetectionTypeSpecified = true,
                DetectionType = ConvertDetectionType(alertData.AlertObject),
                AlertIdentification = new AlertIdentification
                {
                    AlertType = ConvertAlertType(alertData.AlertType)
                }
            };
            var indication = new IndicationType
            {
                CreationTime = new TimeType
                {
                    Zone = TimezoneType.GMT,
                    Value = DateTime.Now
                },
                ID = image.AlertID,
                Item = detection
            };
            return indication;
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
                    //new DetailedSensorBITType
                    //{
                    //    SensorIdentification = sensorIdentification,
                    //    FaultCode = new string[0]
                    //},
                    new SensorStatusReport
                    {
                        SensorIdentification = sensorIdentification,
                        PowerState = StatusType.Yes,
                        SensorMode = SensorModeType.ON,
                        SensorTechnicalState = BITResultType.OK,
                        CommunicationState = BITResultType.OK
                    }, 
                }
            };
        }

        public static DeviceConfiguration CreateConfiguration(Camera[] cameras)
        {
            string[] siteNames = cameras.Select(c => c.SiteName).Distinct().ToArray();
            Dictionary<string, Camera[]> camerasBySite = new Dictionary<string, Camera[]>();
            foreach (var site in siteNames)
            {
                camerasBySite.Add(site, cameras.Where(c=>c.SiteName == site).ToArray());
            }

            List<DeviceConfiguration> subConfigs = new List<DeviceConfiguration>();
            foreach (var cameraSite in camerasBySite)
            {
                var subConfig = new DeviceConfiguration
                {
                    DeviceIdentification = new DeviceIdentificationType
                    {
                        DeviceName = cameraSite.Key,
                        DeviceType = DeviceTypeType.CameraNetworkSystem
                    }
                };
                List<SensorConfiguration> sensors = new List<SensorConfiguration>();
                foreach (var camera in cameraSite.Value)
                {
                    sensors.Add(new SensorConfiguration
                    {
                        SensorIdentification = new SensorIdentificationType
                        {
                            SensorType = SensorTypeType.StaringCamera,
                            SensorName = camera.CameraName
                        },
                        LocationType = CreateMarsLocation(camera.Latitude, camera.Longtitude)
                    });
                }

                subConfig.SensorConfiguration = sensors.ToArray();
                subConfigs.Add(subConfig);
            }

            return new DeviceConfiguration
            {
                DeviceIdentification = new DeviceIdentificationType
                {
                    DeviceName = "CameraHub",
                    DeviceType = DeviceTypeType.DeviceHub
                },
                DeviceConfiguration1 = subConfigs.ToArray()
            };
        }

        public static DeviceStatusReport CreateStatusReport(Camera[] cameras)
        {
            string[] siteNames = cameras.Select(c => c.SiteName).Distinct().ToArray();
            var camerasBySite = new Dictionary<string, Camera[]>();
            foreach (var site in siteNames)
            {
                camerasBySite.Add(site, cameras.Where(c => c.SiteName == site).ToArray());
            }

            var subStatusReports = new List<object>();
            foreach (var cameraSite in camerasBySite)
            {
                var deviceId = new DeviceIdentificationType
                {
                    DeviceName = cameraSite.Key,
                    DeviceType = DeviceTypeType.CameraNetworkSystem
                };
                var deviceStatus = new DeviceStatusReport
                {
                    DeviceIdentification = deviceId
                };
                var sensors = new List<object>();
                foreach (var camera in cameraSite.Value)
                {
                    var sensorId = new SensorIdentificationType
                    {
                        SensorType = SensorTypeType.StaringCamera,
                        SensorName = camera.CameraName
                    };
                    sensors.Add(new SensorStatusReport
                    {
                        SensorIdentification = sensorId,
                        CommunicationState = ConvertCameraStatus(camera.CameraStatus),
                        PowerState = camera.CameraStatus == CameraStatus.Ok ? StatusType.Yes : StatusType.No,
                        SensorTechnicalState = ConvertCameraStatus(camera.CameraStatus),
                        SensorMode = SensorModeType.Normal
                    });
                    sensors.Add(new DetailedSensorBITType
                    {
                        SensorIdentification = sensorId,
                        FaultCode = new string[0]
                    });
                }

                var deviceBit = new DetailedDeviceBIT
                {
                    DeviceIdentification = deviceId
                };

                deviceStatus.Items = sensors.ToArray();
                subStatusReports.Add(deviceStatus);
                subStatusReports.Add(deviceBit);
            }

            return new DeviceStatusReport
            {
                DeviceIdentification = new DeviceIdentificationType
                {
                    DeviceName = "CameraHub",
                    DeviceType = DeviceTypeType.DeviceHub
                },
                Items = subStatusReports.ToArray()
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
                case CarTypes.Bus:
                    return VehicleType.Bus;
                default:
                    return VehicleType.Undefined;
            }
        }

        private static LicensePlateType ConvertLicenseType(LicenseTypes licenseType)
        {
            switch (licenseType)
            {
                case LicenseTypes.Israeli:
                    return LicensePlateType.Israeli;
                case LicenseTypes.Palestinian:
                    return LicensePlateType.Palestinian;
                case LicenseTypes.Diplomat:
                    return LicensePlateType.Diplomat;
                case LicenseTypes.Army:
                    return LicensePlateType.Military;
                case LicenseTypes.Police:
                    return LicensePlateType.Police;
                default:
                    return LicensePlateType.Undefined;
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

        private static BITResultType ConvertCameraStatus(CameraStatus cameraStatus)
        {
            switch (cameraStatus)
            {
                case CameraStatus.Ok:
                    return BITResultType.OK;
                case CameraStatus.Fault:
                    return BITResultType.Fault;
                case CameraStatus.Other:
                    return BITResultType.Undefined;
                default:
                    return BITResultType.Undefined;
            }
        }

        private static LocationType CreateMarsLocation(double lat, double lon)
        {
            return new LocationType
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
            };
        }
    }
}