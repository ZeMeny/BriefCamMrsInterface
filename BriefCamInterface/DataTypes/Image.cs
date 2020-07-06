using System;

namespace BriefCamInterface.DataTypes
{
    public class Image
    {
        public string AlertID { get; set; }

        public string Image1 { get; set; } // base64

        public string Image2 { get; set; } // base64
        
        public string Image3 { get; set; } // base64

        public string VideoClip { get; set; } // base64

        public DateTime DistributionTime { get; set; }

        public string[] GroupIDs { get; set; }


    }
}