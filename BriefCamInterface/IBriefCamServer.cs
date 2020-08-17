using System;
using BriefCamInterface.DataTypes;

namespace BriefCamInterface
{
    public interface IBriefCamServer
    {
        string IP { get; }
        int Port { get; }
        bool IsOpen { get; }

        void Start();
        void Stop();

        event EventHandler<Alert> AlertReceived;
        event EventHandler<Image> ImageReceived;
        event EventHandler<Camera[]> CameraReceived;
    }
}