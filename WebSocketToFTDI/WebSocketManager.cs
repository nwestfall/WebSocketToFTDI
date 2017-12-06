using System;
using System.Collections.Generic;
using System.Linq;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketToFTDI
{
    /// <summary>
    /// Manager for Web Socket Server
    /// </summary>
    public class WebSocketManager : IDisposable
    {
        #region Properties
        private WebSocketServer WebSocketServer { get; set; }
        private IList<string> _availablePaths = new List<string>();
        /// <summary>
        /// Get the list of paths setup with the web socket
        /// </summary>
        public IEnumerable<string> AvailablePaths
        {
            get => _availablePaths;
        }
        /// <summary>
        /// Get the Port of the web scoket
        /// </summary>
        public int? Port { get; private set; } = null;
        /// <summary>
        /// Get if the web socket is setup to be secure
        /// </summary>
        public bool Secure { get; private set; }
        /// <summary>
        /// Get the web socket url
        /// </summary>
        public string Url { get; private set; }
        #endregion

        /// <summary>
        /// Default ctor
        /// </summary>
        public WebSocketManager()
        {
            Port = 3000;
            Secure = false;

            Start();
        }

        /// <summary>
        /// Default ctor with port and secure flag
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="secure">The secure flag.</param>
        public WebSocketManager(int port, bool secure)
        {
            Port = port;
            Secure = secure;

            Start();
        }

        /// <summary>
        /// Default ctor with the url
        /// </summary>
        /// <param name="url">The url.</param>
        public WebSocketManager(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            if(url.Contains(":"))
            {
                var splitUrl = url.Split(':');
                if (int.TryParse(splitUrl[2], out int port))
                {
                    Url = $"{splitUrl[0]}:{splitUrl[1]}";
                    Port = port;
                }
                else
                    Url = url;

            }
            else
                Url = url;

            Secure = url.StartsWith("wss");
                

            Start();
        }

        public WebSocketManager(string url, int port)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
            Port = port;
            Secure = url.StartsWith("wss");

            Start();
        }

        /// <summary>
        /// Start the Web Socket Server
        /// </summary>
        public void Start()
        {
            if (string.IsNullOrEmpty(Url))
                WebSocketServer = new WebSocketServer(System.Net.IPAddress.Any, Port.Value, Secure);
            else
            {
                if (Port.HasValue)
                    WebSocketServer = new WebSocketServer($"{Url}:{Port}");
                else
                    WebSocketServer = new WebSocketServer(Url);
            }
            WebSocketServer.Start();
        }

        /// <summary>
        /// Stop the Web Socket Server
        /// </summary>
        public void Stop() => WebSocketServer?.Stop(CloseStatusCode.Normal, "Manager Stopping");

        /// <summary>
        /// Add a service to the web socket
        /// </summary>
        /// <typeparam name="T">The <see cref="WebSocketBehavior"/></typeparam>
        /// <param name="path">The path.</param>
        public void AddService<T>(string path)
            where T : WebSocketBehavior, new()
        {
            if (WebSocketServer == null)
                throw new ArgumentNullException(nameof(WebSocketServer));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (_availablePaths.Any(a => string.Equals(a, path, StringComparison.InvariantCultureIgnoreCase)))
                throw new ArgumentException("Path already setup");

            _availablePaths.Add(path);
            WebSocketServer.AddWebSocketService<T>(path);
        }

        /// <summary>
        /// Remove a service to the web socket
        /// </summary>
        /// <param name="path">The path.</param>
        public void RemoveService(string path)
        {
            if (WebSocketServer == null)
                throw new ArgumentNullException(nameof(WebSocketServer));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            _availablePaths.Remove(path);
            WebSocketServer.RemoveWebSocketService(path);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (WebSocketServer?.IsListening == true)
                        WebSocketServer?.Stop(CloseStatusCode.Abnormal, "Manager dispoed");

                    _availablePaths.Clear();
                    _availablePaths = null;
                    WebSocketServer = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebSocketManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
