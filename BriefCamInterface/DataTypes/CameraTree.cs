using Newtonsoft.Json;

namespace BriefCamInterface.DataTypes
{
    public class CameraTree
    {
        [JsonProperty(PropertyName = "cameras")]
        public Camera[] Cameras { get; set; }
    }
}