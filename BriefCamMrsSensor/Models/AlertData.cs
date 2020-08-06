using BriefCamInterface.DataTypes;
using SensorStandard.MrsTypes;

namespace BriefCamMrsSensor.Models
{
    public class AlertData
    {
        public Point Location { get; set; }
        public AlertTypes AlertType { get; set; }
        public AlertObjectTypes AlertObject { get; set; }
    }
}