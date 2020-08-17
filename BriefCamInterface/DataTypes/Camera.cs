using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;

namespace BriefCamInterface.DataTypes
{
    public class Camera
    {
        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }

        [JsonProperty(PropertyName = "cameraType")]
        public CameraTypes CameraType { get; set; }

        [JsonProperty(PropertyName = "lprSource")]
        public string LprSource { get; set; }

        [JsonProperty(PropertyName = "lprCameraId")]
        public string LprCameraID { get; set; }

        [JsonProperty(PropertyName = "cameraName")]
        public string CameraName { get; set; }

        [JsonProperty(PropertyName = "cameraStatus")]
        public CameraStatus CameraStatus { get; set; }

        [JsonProperty(PropertyName = "cameraIP")]
        public string CameraIP { get; set; }

        [JsonProperty(PropertyName = "longtitude")]
        public double Longtitude { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "siteID")]
        public int SiteID { get; set; }

        [JsonProperty(PropertyName = "siteName")]
        public string SiteName { get; set; }

        /// <summary>
        /// WKT Polygon
        /// </summary>
        [JsonProperty(PropertyName = "coverage")]
        public string Coverage { get; set; }

        [JsonProperty(PropertyName = "angle")]
        public double Angle { get; set; }

        [JsonProperty(PropertyName = "heightM")]
        public double HeightM { get; set; }

        [JsonProperty(PropertyName = "detectionCount")]
        public int DetectionCount { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty(PropertyName = "sendTime")]
        public DateTime SendTime { get; set; }

        [JsonProperty(PropertyName = "hativa")]
        public string Hativa { get; set; }

        [JsonIgnore] public Point[] CoveragePolygon => GetPoints(Coverage);

        private Point[] GetPoints(string polygon)
        {
            if (string.IsNullOrEmpty(polygon) || polygon.Contains("POLYGON") == false)
            {
                Console.WriteLine("Invalid polygon format");
                return null;
            }

            // remove start and end
            polygon = polygon.Replace("POLYGON ((", "");
            polygon = polygon.Replace("))", "");

            string[] points = polygon.Split(new[] { ", " }, StringSplitOptions.None);

            var pointsList = new List<Point>();
            foreach (string point in points)
            {
                string[] coor = point.Split(' ');
                if (double.TryParse(coor[0], out double x) && double.TryParse(coor[1], out double y))
                {
                    pointsList.Add(new Point(x, y));
                }
                else
                {
                    Console.WriteLine("Error parsing polygon point");
                }
            }

            return pointsList.ToArray();
        }
    }
}