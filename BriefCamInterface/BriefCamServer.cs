using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BriefCamInterface.DataTypes;
using Newtonsoft.Json;
using SimpleHttpServer;
using SimpleHttpServer.Models;

namespace BriefCamInterface
{
	public class BriefCamServer
	{
		#region / / / / /  Private fields  / / / / /

		private readonly HttpServer _server;
		private readonly Action<string> _logAction;

		#endregion


		#region / / / / /  Properties  / / / / /

		public string IP => _server.IP.ToString();
		public int Port => _server.Port;
		public bool IsOpen => _server.IsActive;

		#endregion


		#region / / / / /  Private methods  / / / / /

		private HttpResponse CameraHandler(HttpRequest request)
		{
			try
			{
				Camera camera = JsonConvert.DeserializeObject<Camera>(request.Content);
				CameraReceived?.Invoke(this, camera);

				return new HttpResponse
				{
					StatusCode = HttpStatusCode.Ok
				};
			}
			catch (Exception e)
			{
				_logAction?.Invoke(e.Message);
				return new HttpResponse
				{
					StatusCode = HttpStatusCode.InternalServerError
				};
			}
		}

		private HttpResponse ImageHandler(HttpRequest request)
		{
			try
			{
				Image image = JsonConvert.DeserializeObject<Image>(request.Content);
				ImageReceived?.Invoke(this, image);

				return new HttpResponse
				{
					StatusCode = HttpStatusCode.Ok
				};
			}
			catch (Exception e)
			{
				_logAction?.Invoke(e.Message);
				return new HttpResponse
				{
					StatusCode = HttpStatusCode.InternalServerError
				};
			}
		}

		private HttpResponse AlertHandler(HttpRequest request)
		{
			try
			{
				Alert alert = JsonConvert.DeserializeObject<Alert>(request.Content);
				AlertReceived?.Invoke(this, alert);

				return new HttpResponse
				{
					StatusCode = HttpStatusCode.Ok
				};
			}
			catch (Exception e)
			{
				_logAction?.Invoke(e.Message);
				return new HttpResponse
				{
					StatusCode = HttpStatusCode.InternalServerError
				};
			}
		}

		#endregion


		#region / / / / /  Public methods  / / / / /

		public BriefCamServer(string ip, int port, Action<string> logAction = null)
		{
			string pattern =
				@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
			if (string.IsNullOrEmpty(ip) || Regex.IsMatch(ip, pattern) == false)
			{
				throw new ArgumentException("Invalid ip address", nameof(ip));
			}

			if (port > 0 && port < 65535)
			{
				throw new ArgumentNullException(nameof(port), "port must be larger than 0 and smaller than 65535");
			}

			var routeConfig = new List<Route>
			{
				new Route
				{
					Callable = AlertHandler,
					Name = "Alert Hanlder",
					Method = "POST",
					UrlRegex = @"^/alerts$"
				},
				new Route
				{
					Callable = ImageHandler,
					Name = "Image Hanlder",
					Method = "POST",
					UrlRegex = @"^/images$"
				},
				new Route
				{
					Callable = CameraHandler,
					Name = "Cameras Hanlder",
					Method = "POST",
					UrlRegex = @"^/cameras$"
				}
			};

			_server = new HttpServer(ip, port, routeConfig);
			_logAction = logAction;
		}

		public void Start() => _server.Start();

		public void Stop() => _server.Stop();

		#endregion


		#region / / / / /  Events  / / / / /

		public event EventHandler<Alert> AlertReceived;
		public event EventHandler<Image> ImageReceived;
		public event EventHandler<Camera> CameraReceived;

		#endregion
	}
}