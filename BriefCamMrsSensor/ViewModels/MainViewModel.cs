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
        private string _remoteServerAddress;
        private string _sensorIp;
        private int _sensorPort;
        private bool _showStatusReports;
        private Action _sensorAction;
        private Action _briefCamAction;
        private bool _validate = true;
        private readonly ILog _logger = LogManager.GetLogger(typeof(MainViewModel));
        private BreifCamClient _breifCamClient;
        private readonly Sensor _sensor = Sensor.Instance;
        private readonly Dictionary<string, Point> _alertLocations = new Dictionary<string, Point>();
        private readonly BriefcamSimulator _simulator;
        private bool _isSimActive;

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

        public string RemoteServerAddress
        {
            get => _remoteServerAddress;
            set
            {
                _remoteServerAddress = value;
                OnPropertyChanged(nameof(RemoteServerAddress));
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
                _sensor.ValidateMessages = value;
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
            RemoteServerAddress = Settings.Default.BriefCamAddress;
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
                _breifCamClient = new BreifCamClient(new Uri(RemoteServerAddress, UriKind.Absolute));
                _breifCamClient.Opened += BreifCamClient_Opened;
                _breifCamClient.Closed += BreifCamClient_Closed;
                _breifCamClient.AlertReceived += BreifCamClient_AlertReceived;
                _breifCamClient.ImageReceived += BreifCamClient_ImageReceived;
                _breifCamClient.ErrorOccurred += BreifCamClient_ErrorOccurred;
                _breifCamClient.Connect();
                IsThinking = true;
            };
            _briefCamAction.BeginInvoke(BriefCamCallback, null);
        }

        private void StopBriefCamClient()
        {
            _breifCamClient?.Disconnect();
        }

        private void StartSensor()
        {
            _sensorAction = () =>
            {
                IsThinking = true;
                var configuration = MrsBriefCamHelper.CreateDefaultConfig(SensorIP, SensorPort);
                var status = MrsBriefCamHelper.CreateDefaultStatus();

                string path = AppDomain.CurrentDomain.BaseDirectory + "Configuration.xml";
                if (File.Exists(path))
                {
                    try
                    {
                        configuration = MrsMessage.Load<DeviceConfiguration>(File.ReadAllText(path));
                        configuration.NotificationServiceIPAddress = SensorIP;
                        configuration.NotificationServicePort = SensorPort.ToString();
                    }
                    catch (Exception ex)
                    {
                        WriteSensorLog("Error Loading Configuration", ex, true);
                    }
                }

                path = AppDomain.CurrentDomain.BaseDirectory + "StatusReport.xml";
                if (File.Exists(path))
                {
                    try
                    {
                        status = MrsMessage.Load<DeviceStatusReport>(File.ReadAllText(path));
                    }
                    catch (Exception ex)
                    {
                        WriteSensorLog("Error Loading Status Report", ex, true);
                    }
                }
                _sensor.OpenWebService(configuration, status);
            };
            _sensorAction.BeginInvoke(SensorCallback, null);
        }

        private void StopSensor()
        {
            _sensor.CloseWebService();
        }

        private void SaveConfig()
        {
            Settings.Default.BriefCamAddress = RemoteServerAddress;
            Settings.Default.SensorIP = SensorIP;
            Settings.Default.SensorPort = SensorPort;
            Settings.Default.ValidateMessages = ValidateMessages;
            Settings.Default.Save();

            if (_sensor.DeviceConfiguration != null && !File.Exists("Configuration.xml"))
            {
                using (FileStream stream = new FileStream("Configuration.xml", FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DeviceConfiguration));
                    serializer.Serialize(stream, _sensor.DeviceConfiguration);
                }
            }

            if (_sensor.StatusReport != null && !File.Exists("StatusReport.xml"))
            {
                using (FileStream stream = new FileStream("StatusReport.xml", FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DeviceStatusReport));
                    serializer.Serialize(stream, _sensor.StatusReport);
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
                        if (_breifCamClient == null || _breifCamClient.IsConnected == false)
                        {
                            _logger.Error("BriefCam Failed to Connect, Unknown Error");
                            WriteBriefCamLog("BriefCam Failed to Connect");
                        }
                        else
                        {
                            _logger.Info("BriefCam Connected");
                            WriteBriefCamLog("BriefCam Connected");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error Connecting to BriefCam", ex);
                        WriteBriefCamLog("Error Connecting to BriefCam!\nError Details: " + ex.Message);
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
                        if (_sensor.IsOpen)
                        {
                            _logger.Info("Sensor Web Service Opened");
                            WriteSensorLog("Web Service Opened on " + _sensor.ServerAddress);
                        }
                        else
                        {
                            _logger.Error("Failed to open sensor web service, unknown error");
                            WriteSensorLog("Failed to open sensor web service", isAlarm: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error Opening Sensor Web Service", ex);
                        WriteSensorLog("Error Opening Mars Sensor!\nError Details: " + ex.Message);
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

        private bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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

        private void BreifCamClient_ImageReceived(object sender, BriefCamInterface.DataTypes.Image e)
        {
            if (_alertLocations.ContainsKey(e.AlertID))
            {
                var location = _alertLocations[e.AlertID];
                _sensor.SendIndicationReport(MrsBriefCamHelper.ConvertImage(e, location));
                _logger.Info("BriefCam Client: Image Received");
                WriteBriefCamLog("Image Received", e.ToJson());
            }
            else
            {
                _logger.Warn("BriefCam Client: Image ignored, Unknown Image ID");
                WriteBriefCamLog("Image ignored, Unknown Image ID", e.ToJson());
            }
        }

        private void BreifCamClient_AlertReceived(object sender, BriefCamInterface.DataTypes.Alert e)
        {
            _sensor.SendIndicationReport(MrsBriefCamHelper.ConvertAlert(e));
            _alertLocations.Add(e.AlertID, MrsBriefCamHelper.CreateMrsPoint(e.Latitude, e.Longitude));
            _logger.Warn("BriefCam Client: Alert Received");
            WriteBriefCamLog("Alert Received", e.ToJson(), true);
        }

        private void BreifCamClient_Closed(object sender, EventArgs e)
        {
            _logger.Info("BriefCam Client Closed");
            WriteBriefCamLog("Client Closed");
        }

        private void BreifCamClient_Opened(object sender, EventArgs e)
        {
            _logger.Info("BriefCam Client Opened");
            WriteBriefCamLog("Client Opened");
        }

        private void BreifCamClient_ErrorOccurred(object sender, Exception e)
        {
            _logger.Error("BriefCam Client Error", e);
            WriteBriefCamLog("Error Occurred", e, true);
        }

        private void Sensor_ValidationErrorOccured(InvalidMessageException messageException)
        {
            _logger.Error("Mars Sensor Validation Error", messageException);
            WriteSensorLog("Validation Error", messageException, true);
        }

        private void Sensor_MessageSent(MrsMessageTypes messageType, MrsMessage message, string marsName)
        {
            if (messageType != MrsMessageTypes.DeviceStatusReport || ShowStatusReports)
            {
                WriteSensorLog($"{messageType} Sent", message.ToXml(), messageType == MrsMessageTypes.DeviceIndicationReport);
            }
        }

        private void Sensor_MessageReceived(MrsMessageTypes messageType, MrsMessage message, string marsName)
        {
            if (messageType == MrsMessageTypes.CommandMessage && message.ToXml().Contains("KeepAlive") && !ShowStatusReports)
            {
                return;
            }
            WriteSensorLog($"{messageType} Received", message.ToXml());
        }

        private void Simulator_Image(object sender, BriefCamInterface.DataTypes.Image e)
        {
            if (_alertLocations.ContainsKey(e.AlertID))
            {
                var location = _alertLocations[e.AlertID];
                _sensor.SendIndicationReport(MrsBriefCamHelper.ConvertImage(e, location));
                WriteBriefCamLog("Simulator: Image Received", e.ToJson());
            }
            else
            {
                WriteBriefCamLog("Simulator: Image ignored, Unknown Image ID", e.ToJson());
            }
        }

        private void Simulator_Alert(object sender, BriefCamInterface.DataTypes.Alert e)
        {
            _sensor.SendIndicationReport(MrsBriefCamHelper.ConvertAlert(e));
            _alertLocations.Add(e.AlertID, MrsBriefCamHelper.CreateMrsPoint(e.Latitude, e.Longitude));
            WriteBriefCamLog("Simulator: Alert Received", e.ToJson(), true);
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
            StartBriefCamCommand = new Command(StartBriefCamClient, () => ValidateUrl(RemoteServerAddress));
            StopBriefCamCommand = new Command(StopBriefCamClient);
            StartSensorCommand = new Command(StartSensor, () => ValidateIpEndpoint(SensorIP, SensorPort));
            StopSensorCommand = new Command(StopSensor);
            ClearBriefCamLogCommand = new Command(ClearBreifCamLog);
            ClearSensorLogCommand = new Command(ClearSensorLog);
            StartSimCommand = new Command(StartSimulator);
            StopSimCommand = new Command(StopSimulator);

            BriefCamLogItems = new ObservableCollection<ListViewItem>();
            SensorLogItems = new ObservableCollection<ListViewItem>();

            _sensor.MessageReceived += Sensor_MessageReceived;
            _sensor.MessageSent += Sensor_MessageSent;
            _sensor.ValidationErrorOccured += Sensor_ValidationErrorOccured;
            _sensor.AutoStatusReport = true;

            _simulator = new BriefcamSimulator(Settings.Default.SimRate);
            _simulator.Alert += Simulator_Alert;
            _simulator.Image += Simulator_Image;
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
