using BriefCamInterface.DataTypes;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BriefCamInterface
{
    public class BriefCamServer2 : IBriefCamServer
    {
		#region / / / / /  Private fields  / / / / /

		private readonly HttpListener _server;
		private readonly Action<string> _logAction;
		private Thread _listenerThread;
		private readonly string[] _endpoints = {"alerts", "images", "cameras"};

		#endregion


		#region / / / / /  Properties  / / / / /

		public string IP { get; }
		public int Port { get; }
		public bool IsOpen { get; private set; }

		#endregion


		#region / / / / /  Private methods  / / / / /

		private async void Listen()
		{
			while (IsOpen)
			{
				HttpListenerContext context = null;
				var responseBytes = new byte[0];
				try
				{
					context = await _server.GetContextAsync();
					var status = HandleRequest(context.Request, out var response);

					responseBytes = Encoding.UTF8.GetBytes(response);
					context.Response.StatusCode = (int) status;
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex)
				{
					_logAction?.BeginInvoke(ex.Message, null, null);

					if (context != null)
					{
						responseBytes = Encoding.UTF8.GetBytes(ex.Message);
						context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
					}
				}
				finally
				{
					if (context != null)
					{
						context.Response.ContentType = "text/html";
						context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
						context.Response.OutputStream.Flush();
						context.Response.OutputStream.Close();
					}
				}
			}
		}

		private HttpStatusCode HandleRequest(HttpListenerRequest request, out string responseMessage)
		{
			responseMessage = string.Empty;
			var status = HttpStatusCode.OK;

			var contentBytes = new byte[request.ContentLength64];
			request.InputStream.Read(contentBytes, 0, contentBytes.Length);
			string content = Encoding.UTF8.GetString(contentBytes);

			string resource = request.Url.Segments[1].Replace("/", "");

			if (resource == _endpoints[0]) // alerts
			{
				if (request.HttpMethod.ToUpper() != "POST")
				{
					status = HttpStatusCode.MethodNotAllowed;
				}
				else
				{
					Alert alert = JsonConvert.DeserializeObject<Alert>(content);
					AlertReceived?.BeginInvoke(this, alert, null, null);
				}
			}
			else if (resource == _endpoints[1]) // images
			{
				if (request.HttpMethod.ToUpper() != "POST")
				{
					status = HttpStatusCode.MethodNotAllowed;
				}
				else
				{
					Image image = JsonConvert.DeserializeObject<Image>(content);
					ImageReceived?.BeginInvoke(this, image, null, null);
				}
			}
			else if (resource == _endpoints[2]) // cameras
			{
				if (request.HttpMethod.ToUpper() != "POST")
				{
					status = HttpStatusCode.MethodNotAllowed;
				}
				else
				{
					CameraTree cameras = JsonConvert.DeserializeObject<CameraTree>(content);
					CameraReceived?.BeginInvoke(this, cameras, null, null);
				}
			}
			
			return status;
		}

		#endregion


		#region / / / / /  Public methods  / / / / /

		public BriefCamServer2(string ip, int port, Action<string> logAction = null)
		{
			string pattern =
				@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
			if (string.IsNullOrEmpty(ip) || Regex.IsMatch(ip, pattern) == false)
			{
				throw new ArgumentException("Invalid ip address", nameof(ip));
			}

			if (port <= 0 || port > 65535)
			{
				throw new ArgumentNullException(nameof(port), "port must be larger than 0 and smaller than 65535");
			}

			IP = ip;
			Port = port;

			_server = new HttpListener();
			foreach (var endpoint in _endpoints)
			{
				_server.Prefixes.Add($"http://{IP}:{Port}/{endpoint}/");
			}
			_logAction = logAction;
		}

		public void Start()
		{
			_server.Start();
			IsOpen = true;
			_listenerThread = new Thread(Listen)
			{
				Name = "HttpListenerThread"
			};
			_listenerThread.Start();
		}

		public void Stop()
		{
			IsOpen = false;
			_server.Abort();
			if (_listenerThread != null && _listenerThread.IsAlive)
			{
				_listenerThread.Abort();
			}
		}

		#endregion


		#region / / / / /  Events  / / / / /

		public event EventHandler<Alert> AlertReceived;
		public event EventHandler<Image> ImageReceived;
		public event EventHandler<CameraTree> CameraReceived;

		#endregion
	}
}