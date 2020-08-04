using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using BriefCamInterface.DataTypes;
using BriefCamMrsSensor.Properties;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace BriefCamMrsSensor.Models
{
    public class BriefcamSimulator
    {
        #region / / / / /  Private Fields  / / / / /

        private readonly Timer _timer;
        private int _currentId;
        private readonly string _alertJsonPath = AppDomain.CurrentDomain.BaseDirectory + "SimAlert.json";

        #endregion

        #region / / / / /  Properties  / / / / /

        public TimeSpan Rate { get; }

        public bool IsActive => _timer.Enabled;

        #endregion


        #region / / / / /  Private Methods  / / / / /

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Alert?.Invoke(this, CreateAlert());
            Thread.Sleep(1000);
            Image?.Invoke(this, CreateImage());
        }

        private Alert CreateAlert()
        {
            try
            {
                using (StreamReader reader = new StreamReader(_alertJsonPath, Encoding.UTF8))
                {
                    var alert = JsonConvert.DeserializeObject<Alert>(reader.ReadToEnd());
                    // override fields
                    if (Settings.Default.SimOverride)
                    {
                        alert.AlertID = (++_currentId).ToString();
                        alert.SystemAlertTime = DateTime.Now;
                        alert.SensorAlertTime = DateTime.Now;
                    }

                    return alert;
                }
            }
            catch
            {
                return new Alert
                {
                    Latitude = 34.5,
                    Longitude = 32.5,
                    AlertID = (++_currentId).ToString(),
                    SensorAlertTime = DateTime.Now,
                    SuspectSex = GenderTypes.Male,
                    AlertDescription = "Simulation",
                    AlertObject = AlertObjectTypes.Man,
                    AlertReality = AlertRealityTypes.Drill,
                    AlertSeverity = AlertSevirityTypes.Low,
                    HumanID = "123456789",
                    PhoneNumber = "1234567890",
                    WeaponAccessabillity = WeaponAccessabilityTypes.NoWeapon,
                    SuspectBirthDate = new DateTime(1990, 1, 1),
                    SuspectAge = 30,
                    SystemAlertTime = DateTime.Now,
                    NameAddress = "Simulation",
                    AlertType = AlertTypes.FaceDetection,
                    AlertStatus = AlertStatusTypes.Active,
                    SensorType = SensorTypes.VideoCamera
                };
            }
        }

        private Image CreateImage()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Resources.pp.Save(stream, ImageFormat.Jpeg);
                return new Image
                {
                    Image1 = stream.ToArray(),
                    VideoClip = Resources.file_example_MP4_640_3MG,
                    AlertID = _currentId.ToString(),
                    DistributionTime = DateTime.Now
                };
            }
        }

        #endregion


        #region / / / / /  Public Methods  / / / / /

        public BriefcamSimulator(TimeSpan rate)
        {
            Rate = rate;

            _timer = new Timer(Rate.TotalMilliseconds);
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        #endregion


        #region / / / / /  Events  / / / / /

        public event EventHandler<Alert> Alert;
        public event EventHandler<Image> Image;

        #endregion
    }
}