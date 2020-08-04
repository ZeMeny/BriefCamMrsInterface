using System;
using BriefCamInterface.DataTypes;
using Newtonsoft.Json;
using WebSocket4Net;

namespace BriefCamInterface
{
    public class BriefCamClient
    {
        #region / / / / /  Private fields  / / / / /

        private readonly WebSocket _socket;

        #endregion


        #region / / / / /  Properties  / / / / /

        public bool IsConnected => _socket.State == WebSocketState.Open;

        #endregion


        #region / / / / /  Private methods  / / / / /

        private void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                Alert alert = JsonConvert.DeserializeObject<Alert>(e.Message);
                AlertReceived?.Invoke(this, alert);
            }
            catch (JsonException)
            {
                Image image = JsonConvert.DeserializeObject<Image>(e.Message);
                ImageReceived?.Invoke(this, image);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #endregion


        #region / / / / /  Public methods  / / / / /

        public BriefCamClient(Uri address)
        {
            _socket = new WebSocket(address.OriginalString);
            _socket.MessageReceived += Socket_MessageReceived;
            _socket.Error += (sender, args) => ErrorOccurred?.Invoke(this, args.Exception);
        }

        public void Connect()
        {
            _socket.Open(); 
        }

        public void Disconnect()
        {
            _socket?.Close();
        }

        #endregion


        #region / / / / /  Events  / / / / /

        public event EventHandler Opened
        {
            add => _socket.Opened += value;
            remove => _socket.Opened -= value;
        }
        public event EventHandler Closed
        {
            add => _socket.Closed += value;
            remove => _socket.Closed -= value;
        }

        public event EventHandler<Exception> ErrorOccurred;
        public event EventHandler<Alert> AlertReceived;
        public event EventHandler<Image> ImageReceived;

        #endregion
    }
}
