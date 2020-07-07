using System;

namespace BriefCamInterface.DataTypes
{
    public class Image
    {
        public string AlertID { get; set; }

        public byte[] Image1 { get; set; } // base64

        public byte[] Image2 { get; set; } // base64
        
        public byte[] Image3 { get; set; } // base64

        public byte[] VideoClip { get; set; } // base64

        public DateTime DistributionTime { get; set; }
    }
}