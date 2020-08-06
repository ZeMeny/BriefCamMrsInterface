using BriefCamInterface;
using BriefCamMrsSensor.Models;
using BriefCamMrsSensor.Properties;
using BriefCamMrsSensor.Views;
using log4net;
using MarsSensor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using BriefCamInterface.DataTypes;
using SensorStandard;
using SensorStandard.MrsTypes;
using File = System.IO.File;
using Point = SensorStandard.MrsTypes.Point;

namespace BriefCamMrsSensor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region / / / / /  Private fields  / / / / /

        private bool _isThinking;
        private string _briefCamServerIp;
        private int _briefCamServerPort;
        private string _sensorIp;
        private int _sensorPort;
        private int _cameraSensorPort;
        private bool _showStatusReports;
        private Action _sensorAction;
        private Action _briefCamAction;
        private Action _cameraSensorAction;
        private bool _validate = true;
        private readonly ILog _logger = LogManager.GetLogger(typeof(MainViewModel));
        private BriefCamServer _breifCamServer;
        private Sensor _indicationSensor;
        private readonly Dictionary<string, AlertData> _alertsData = new Dictionary<string, AlertData>();
        private readonly BriefcamSimulator _simulator;
        private bool _isSimActive;
        private readonly string _configPath = AppDomain.CurrentDomain.BaseDirectory + "Configuration.xml";
        private readonly string _statusPath = AppDomain.CurrentDomain.BaseDirectory + "StatusReport.xml";
        private Sensor _cameraSensor;

        #endregion


        #region / / / / /  Properties  / / / / /

        public bool IsThinking
        {
            get => _isThinking;
            set
            {
                _isThinking = value;
                OnPropertyChanged(nameof(IsThinking));
            }
        }

        public string BriefCamServerIP
        {
            get => _briefCamServerIp;
            set
            {
                _briefCamServerIp = value;
                OnPropertyChanged(nameof(BriefCamServerIP));
            }
        }

        public int BriefCamServerPort
        {
            get => _briefCamServerPort;
            set
            {
                _briefCamServerPort = value;
                OnPropertyChanged(nameof(BriefCamServerPort));
            }
        }

        public string SensorIP
        {
            get => _sensorIp;
            set
            {
                _sensorIp = value;
                OnPropertyChanged(nameof(SensorIP));
            }
        }

        public int SensorPort
        {
            get => _sensorPort;
            set
            {
                _sensorPort = value;
                OnPropertyChanged(nameof(SensorPort));
            }
        }

        public int CameraSensorPort
        {
            get => _cameraSensorPort;
            set
            {
                _cameraSensorPort = value;
                OnPropertyChanged(nameof(CameraSensorPort));
            }
        }

        public ObservableCollection<ListViewItem> SensorLogItems { get; set; }

        public ObservableCollection<ListViewItem> BriefCamLogItems { get; set; }

        public bool ShowStatusReports
        {
            get => _showStatusReports;
            set
            {
                _showStatusReports = value;
                OnPropertyChanged(nameof(ShowStatusReports));
            }
        }

        public bool ValidateMessages
        {
            get => _validate;
            set
            {
                _validate = value;
                if (_indicationSensor != null)
                {
                    _indicationSensor.ValidateMessages = value;
                }
                if (_cameraSensor != null)
                {
                    _cameraSensor.ValidateMessages = value;
                }
                OnPropertyChanged(nameof(ValidateMessages));
            }
        }

        public int MaximumLogItems { get; } = Settings.Default.MaxLogItems;

        public bool IsSimActive
        {
            get => _isSimActive;
            set
            {
                _isSimActive = value;
                OnPropertyChanged(nameof(IsSimActive));
            }
        }

        #endregion


        #region / / / / /  Commands  / / / / /

        public ICommand LoadedCommand { get; set; }
        public ICommand ClosedCommand { get; set; }
        public ICommand StartBriefCamCommand { get; set; }
        public ICommand StopBriefCamCommand { get; set; }
        public ICommand StartSensorCommand { get; set; }
        public ICommand StopSensorCommand { get; set; }
        public ICommand ClearBriefCamLogCommand { get; set; }
        public ICommand ClearSensorLogCommand { get; set; }
        public ICommand StartSimCommand { get; set; }
        public ICommand StopSimCommand { get; set; }

        #endregion


        #region / / / / /  Private methdos  / / / / /

        private void LoadApp()
        {
            SensorIP = Settings.Default.SensorIP;
            SensorPort = Settings.Default.SensorPort;
            BriefCamServerIP = Settings.Default.BriefCamAddress;
            BriefCamServerPort = Settings.Default.BriefCamPort;
            CameraSensorPort = Settings.Default.CameraSensorPort;
            ValidateMessages = Settings.Default.ValidateMessages;
        }

        private void CloseApp()
        {
            SaveConfig();
            StopBriefCamClient();
            StopSensor();
            Environment.Exit(0);
        }

        private void StartBriefCamClient()
        {
            _briefCamAction = () =>
            {
                _breifCamServer?.Stop();
                _breifCamServer = new BriefCamServer(BriefCamServerIP, BriefCamServerPort, BriefCamServerLog);
                _breifCamServer.AlertReceived += BreifCamServerOnAlertReceived;
                _breifCamServer.ImageReceived += BreifCamServerOnImageReceived;
                _breifCamServer.CameraReceived += BreifCamServerOnCameraReceived;
                _breifCamServer.Start();
                IsThinking = true;
            };
            _briefCamAction.BeginInvoke(BriefCamCallback, null);
        }

        private void BreifCamServerOnCameraReceived(object sender, Camera[] e)
        {
            _logger.Warn("BriefCam Server: Camera Tree Received");
            WriteBriefCamLog("Camera Tree Received", e.ToJson(), true);

            var config = MrsBriefCamHelper.CreateConfiguration(e);
            var status = MrsBriefCamHelper.CreateStatusReport(e);
            config.NotificationServiceIPAddress = SensorIP;
            config.NotificationServicePort = CameraSensorPort.ToString();

            _cameraSensorAction = () =>
            {
                if (_indicationSensor != null && _indicationSensor.IsOpen)
                {
                    if (_cameraSensor == null)
                    {
                        _cameraSensor = new Sensor(config, status);
                        _cameraSensor.MessageReceived += CameraSensorOnMessageReceived;
                        _cameraSensor.MessageSent += CameraSensorOnMessageSent;
                        _cameraSensor.ValidateMessages = ValidateMessages;
                        _cameraSensor.OpenWebService();
                    }
                    else
                    {
                        _cameraSensor.DeviceConfiguration = config;
                        _cameraSensor.StatusReport = status;
                        // notify clients
                        _cameraSensor.SendFullDeviceStatusReport();
                    } 
                }

                IsThinking = true;
            };
            _cameraSensorAction.BeginInvoke(CameraSensorCallback, null);
        }

        private void CameraSensorCallback(IAsyncResult ar)
        {
            if (ar is AsyncResult res)
            {
                Action caller = res.AsyncDelegate as Action;
                if (caller == _cameraSensorAction)
                {
                    try
                    {
                        string message = ar.AsyncState.ToString();
                        caller?.EndInvoke(res);
                        if (_cameraSensor.IsOpen)
                        {
                            _logger.Info(message);
                            WriteSensorLog(message);
                        }
                        else
                        {
                            _logger.Error("Failed to open camera sensor web service, unknown error");
                            WriteSensorLog("Failed to open camera sensor web service", isAlarm: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error in Camera Sensor Web Service", ex);
                        WriteSensorLog("Error in Camera Sensor Web Service!", ex.Message, true);
                    }
                    finally
                    {
                        IsThinking = false;
                    }
                }
            }
        }

        private void CameraSensorOnMessageSent(MrsMessage message, string marsname)
        {
            if (message.MrsMessageType != MrsMessageTypes.DeviceStatusReport || ShowStatusReports)
            {
                WriteSensorLog($"Camera Tree Sensor: {message.MrsMessageType} Sent", message.ToXml(), message.MrsMessageType == MrsMessageTypes.DeviceIndicationReport);
            }
        }

        private void CameraSensorOnMessageReceived(MrsMessage message, string marsname)
        {
            if (message.MrsMessageType == MrsMessageTypes.CommandMessage && message.ToXml().Contains("KeepAlive") && !ShowStatusReports)
            {
                return;
            }
            WriteSensorLog($"Camera Tree Sensor: {message.MrsMessageType} Received", message.ToXml());
        }

        private void BreifCamServerOnImageReceived(object sender, BriefCamInterface.DataTypes.Image e)
        {
            if (_alertsData.ContainsKey(e.AlertID))
            {
                _indicationSensor?.SendIndicationReport(MrsBriefCamHelper.ConvertImage(e, _alertsData[e.AlertID]));
                _logger.Info("BriefCam Server: Image Received");
                WriteBriefCamLog("Image Received", e.ToJson());
            }
            else
            {
                _logger.Warn("BriefCam Server: Image ignored, Unknown Image ID");
                WriteBriefCamLog("Image ignored, Unknown Image ID", e.ToJson());
            }
        }

        private void BreifCamServerOnAlertReceived(object sender, Alert e)
        {
            _indicationSensor?.SendIndicationReport(MrsBriefCamHelper.ConvertAlert(e));
            if (_alertsData.ContainsKey(e.AlertID))
            {
                _alertsData[e.AlertID] = new AlertData
                {
                    Location = MrsBriefCamHelper.CreateMrsPoint(e.Latitude, e.Longitude),
                    AlertType = e.AlertType,
                    AlertObject = e.AlertObject
                };
            }
            else
            {
                _alertsData.Add(e.AlertID,
                    _alertsData[e.AlertID] = new AlertData
                    {
                        Location = MrsBriefCamHelper.CreateMrsPoint(e.Latitude, e.Longitude),
                        AlertType = e.AlertType,
                        AlertObject = e.AlertObject
                    });
            }
            _logger.Warn("BriefCam Server: Alert Received");
            WriteBriefCamLog("Alert Received", e.ToJson(), true);
        }

        private void BriefCamServerLog(string message)
        {
            _logger.Error($"BriefCam Server Error: {message}");
        }

        private void StopBriefCamClient()
        {
            _breifCamServer?.Stop();
        }

        private void StartSensor()
        {
            _sensorAction = () =>
            {
                IsThinking = true;
                var configuration = MrsBriefCamHelper.CreateDefaultConfig(SensorIP, SensorPort);
                var status = MrsBriefCamHelper.CreateDefaultStatus();

                if (File.Exists(_configPath))
                {
                    try
                    {
                        configuration = MrsMessage.Load<DeviceConfiguration>(File.ReadAllText(_configPath));
                        configuration.NotificationServiceIPAddress = SensorIP;
                        configuration.NotificationServicePort = SensorPort.ToString();
                    }
                    catch (Exception ex)
                    {
                        WriteSensorLog("Error Loading Configuration", ex, true);
                    }
                }

                if (File.Exists(_statusPath))
                {
                    try
                    {
                        status = MrsMessage.Load<DeviceStatusReport>(File.ReadAllText(_statusPath));
                    }
                    catch (Exception ex)
                    {
                        WriteSensorLog("Error Loading Status Report", ex, true);
                    }
                }

                StopSensor();
                _indicationSensor = new Sensor(configuration, status) {AutoStatusReport = true};
                _indicationSensor.MessageReceived += Sensor_MessageReceived;
                _indicationSensor.MessageSent += Sensor_MessageSent;
                _indicationSensor.ValidationErrorOccured += Sensor_ValidationErrorOccured;
                _indicationSensor.OpenWebService();
            };
            _sensorAction.BeginInvoke(SensorCallback, null);
        }

        private void StopSensor()
        {
            _indicationSensor?.CloseWebService();
            _cameraSensor?.CloseWebService();
            _cameraSensor = null;
        }

        private void SaveConfig()
        {
            Settings.Default.BriefCamAddress = BriefCamServerIP;
            Settings.Default.BriefCamPort = BriefCamServerPort;
            Settings.Default.CameraSensorPort = CameraSensorPort;
            Settings.Default.SensorIP = SensorIP;
            Settings.Default.SensorPort = SensorPort;
            Settings.Default.ValidateMessages = ValidateMessages;
            Settings.Default.Save();

            if (_indicationSensor != null)
            {
                if (_indicationSensor.DeviceConfiguration != null && !File.Exists(_configPath))
                {
                    using (FileStream stream = new FileStream(_configPath, FileMode.Create))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(DeviceConfiguration));
                        serializer.Serialize(stream, _indicationSensor.DeviceConfiguration);
                    }
                }

                if (_indicationSensor.StatusReport != null && !File.Exists(_statusPath))
                {
                    using (FileStream stream = new FileStream(_statusPath, FileMode.Create))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(DeviceStatusReport));
                        serializer.Serialize(stream, _indicationSensor.StatusReport);
                    }
                } 
            }
        }

        private void BriefCamCallback(IAsyncResult result)
        {
            if (result is AsyncResult res)
            {
                Action caller = res.AsyncDelegate as Action;
                if (caller == _briefCamAction)
                {
                    try
                    {
                        caller?.EndInvoke(res);
                        if (_breifCamServer == null || _breifCamServer.IsOpen == false)
                        {
                            _logger.Error("BriefCam Failed to Start, Unknown Error");
                            WriteBriefCamLog("BriefCam Failed to Start");
                        }
                        else
                        {
                            _logger.Info("BriefCam Server Started");
                            WriteBriefCamLog("BriefCam Server Started");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error Starting to BriefCam Server", ex);
                        WriteBriefCamLog("Error Starting to BriefCam Server!", ex.Message, true);
                    }
                    finally
                    {
                        IsThinking = false;
                    }
                }
            }
        }

        private void SensorCallback(IAsyncResult result)
        {
            if (result is AsyncResult res)
            {
                Action caller = res.AsyncDelegate as Action;
                if (caller == _sensorAction)
                {
                    try
                    {
                        caller?.EndInvoke(res);
                        if (_indicationSensor.IsOpen)
                        {
                            _logger.Info("Sensor Web Service Opened");
                            WriteSensorLog("Indication Sensor Web Service Opened on " + _indicationSensor.ServerAddress);
                        }
                        else
                        {
                            _logger.Error("Failed to open Indication sensor web service, unknown error");
                            WriteSensorLog("Failed to open Indication sensor web service", isAlarm: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error Opening Indication Sensor Web Service", ex);
                        WriteSensorLog("Error Opening Indication Sensor!", ex.Message, true);
                    }
                    finally
                    {
                        IsThinking = false;
                    }
                }
            }
        }

        private bool ValidateIpEndpoint(string ip, int port)
        {
            string pattern = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            return ip != null && Regex.IsMatch(ip, pattern) && port > 0 && port < 65535;
        }

        private void ClearSensorLog()
        {
            SensorLogItems.Clear();
        }

        private void ClearBreifCamLog()
        {
            BriefCamLogItems.Clear();
        }

        private void WriteSensorLog(string header, object content = null, bool isAlarm = false)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ListViewItem item = new ListViewItem()
                {
                    Content = $"{DateTime.Now.ToLongTimeString()} - {header}",
                };
                item.MouseDoubleClick += (sender, args) =>
                {
                    if (content != null)
                    {
                        LogItemWindow itemWindow = new LogItemWindow(content);
                        itemWindow.Show();
                    }
                    else
                    {
                        LogItemWindow itemWindow = new LogItemWindow(header);
                        itemWindow.Show();
                    }
                };
                if (isAlarm)
                {
                    item.Background = Application.Current.FindResource("alarmColor") as System.Windows.Media.SolidColorBrush;
                }
                SensorLogItems.Insert(0, item);
                // no more than max value
                if (SensorLogItems.Count > MaximumLogItems)
                {
                    // remove last
                    SensorLogItems.RemoveAt(SensorLogItems.Count - 1);
                }
            });
        }

        private void WriteBriefCamLog(string header, object content = null, bool isAlarm = false)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ListViewItem item = new ListViewItem()
                {
                    Content = $"{DateTime.Now.ToLongTimeString()} - {header}",
                };
                item.MouseDoubleClick += (sender, args) =>
                {
                    if (content != null)
                    {
                        LogItemWindow itemWindow = new LogItemWindow(content);
                        itemWindow.Show();
                    }
                    else
                    {
                        LogItemWindow itemWindow = new LogItemWindow(header);
                        itemWindow.Show();
                    }
                };
                if (isAlarm)
                {
                    item.Background = Application.Current.FindResource("alarmColor") as System.Windows.Media.SolidColorBrush;
                }
                BriefCamLogItems.Insert(0, item);
                // no more than max value
                if (BriefCamLogItems.Count > MaximumLogItems)
                {
                    // remove last
                    BriefCamLogItems.RemoveAt(BriefCamLogItems.Count - 1);
                }
            });
        }

        private void Sensor_ValidationErrorOccured(object sender, InvalidMessageException messageException)
        {
            _logger.Error("Mars Sensor Validation Error", messageException);
            WriteSensorLog("Indication Sensor: Validation Error", messageException, true);
        }

        private void Sensor_MessageSent(MrsMessage message, string marsName)
        {
            if (message.MrsMessageType != MrsMessageTypes.DeviceStatusReport || ShowStatusReports)
            {
                WriteSensorLog($"Indication Sensor: {message.MrsMessageType} Sent", message.ToXml(), message.MrsMessageType == MrsMessageTypes.DeviceIndicationReport);
            }
        }

        private void Sensor_MessageReceived(MrsMessage message, string marsName)
        {
            if (message.MrsMessageType == MrsMessageTypes.CommandMessage && message.ToXml().Contains("KeepAlive") && !ShowStatusReports)
            {
                return;
            }
            WriteSensorLog($"Indication Sensor: {message.MrsMessageType} Received", message.ToXml());
        }

        private void Simulator_Image(object sender, BriefCamInterface.DataTypes.Image e)
        {
            if (_alertsData.ContainsKey(e.AlertID))
            {
                _indicationSensor?.SendIndicationReport(MrsBriefCamHelper.ConvertImage(e, _alertsData[e.AlertID]));
                WriteBriefCamLog("Simulator: Image Received", e.ToJson());
            }
            else
            {
                WriteBriefCamLog("Simulator: Image ignored, Unknown Image ID", e.ToJson());
            }
        }

        private void Simulator_Alert(object sender, Alert e)
        {
            _indicationSensor?.SendIndicationReport(MrsBriefCamHelper.ConvertAlert(e));
            if (_alertsData.ContainsKey(e.AlertID))
            {
                _alertsData[e.AlertID] = new AlertData
                {
                    Location = MrsBriefCamHelper.CreateMrsPoint(e.Latitude, e.Longitude),
                    AlertType = e.AlertType,
                    AlertObject = e.AlertObject
                };
            }
            else
            {
                _alertsData.Add(e.AlertID,
                    _alertsData[e.AlertID] = new AlertData
                    {
                        Location = MrsBriefCamHelper.CreateMrsPoint(e.Latitude, e.Longitude),
                        AlertType = e.AlertType,
                        AlertObject = e.AlertObject
                    });
            }

            WriteBriefCamLog("Simulator: Alert Received", e.ToJson(), true);
        }

        private void SimulatorOnCameras(object sender, Camera[] e)
        {
            WriteBriefCamLog("Simulator: Camera Tree Received", e.ToJson(), true);

            var config = MrsBriefCamHelper.CreateConfiguration(e);
            var status = MrsBriefCamHelper.CreateStatusReport(e);
            config.NotificationServiceIPAddress = SensorIP;
            config.NotificationServicePort = CameraSensorPort.ToString();

            string message = "";

            if (_indicationSensor != null && _indicationSensor.IsOpen)
            {
                if (_cameraSensor == null)
                {
                    _cameraSensorAction = () =>
                    {
                        _cameraSensor = new Sensor(config, status);
                        _cameraSensor.MessageReceived += CameraSensorOnMessageReceived;
                        _cameraSensor.MessageSent += CameraSensorOnMessageSent;
                        _cameraSensor.ValidateMessages = ValidateMessages;
                        _cameraSensor.OpenWebService();
                    };
                    message = "Camera Sensor Web Service Opened";
                }
                else
                {
                    _cameraSensorAction = () =>
                    {
                        _cameraSensor.DeviceConfiguration = config;
                        _cameraSensor.StatusReport = status;
                        // notify clients
                        _cameraSensor.SendFullDeviceStatusReport();
                    };
                    message = "Camera Sensor Status Updated";
                }
            }

            IsThinking = true;
            _cameraSensorAction.BeginInvoke(CameraSensorCallback, message);
        }

        private void StopSimulator()
        {
            _simulator.Stop();
            IsSimActive = _simulator.IsActive;
        }

        private void StartSimulator()
        {
            _simulator.Start();
            IsSimActive = _simulator.IsActive;
        }

        #endregion


        #region / / / / /  Public methdos  / / / / /

        public MainViewModel()
        {
            LoadedCommand = new Command(LoadApp);
            ClosedCommand = new Command(CloseApp);
            StartBriefCamCommand = new Command(StartBriefCamClient, () => ValidateIpEndpoint(BriefCamServerIP, BriefCamServerPort));
            StopBriefCamCommand = new Command(StopBriefCamClient);
            StartSensorCommand = new Command(StartSensor, () => ValidateIpEndpoint(SensorIP, SensorPort));
            StopSensorCommand = new Command(StopSensor);
            ClearBriefCamLogCommand = new Command(ClearBreifCamLog);
            ClearSensorLogCommand = new Command(ClearSensorLog);
            StartSimCommand = new Command(StartSimulator);
            StopSimCommand = new Command(StopSimulator);

            BriefCamLogItems = new ObservableCollection<ListViewItem>();
            SensorLogItems = new ObservableCollection<ListViewItem>();

            _simulator = new BriefcamSimulator(Settings.Default.SimRate);
            _simulator.Alert += Simulator_Alert;
            _simulator.Image += Simulator_Image;
            _simulator.Cameras += SimulatorOnCameras;
        }

        #endregion


        #region / / / / /  INotifyPropertyChanged  / / / / /

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
