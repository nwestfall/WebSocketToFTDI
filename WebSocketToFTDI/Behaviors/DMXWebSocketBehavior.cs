using System;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketToFTDI.Behaviors
{
    /// <summary>
    /// Prebuilt <see cref="WebSocketBehavior"/>  for DMX connections
    /// </summary>
    public class DMXWebSocketBehavior : WebSocketBehavior
    {
        private static int _numberOfConnections = 0;

        private FTDIManager _ftdiManager;

        public DMXWebSocketBehavior()
        {
            EmitOnPing = true;
        }

        /// <summary>
        /// Handles closing of a connection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            _numberOfConnections--;

            if(_numberOfConnections == 0 && _ftdiManager != null)
            {
                _ftdiManager.Stop();
                _ftdiManager = null;
            }
        }

        /// <summary>
        /// Handles error
        /// </summary>
        /// <param name="e"></param>
        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }

        /// <summary>
        /// Handle new message received
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            if (e.IsText)
                Send("DWM does not support text messages");
            
            if(e.IsBinary)
            {
                var data = e.RawData;

                if (data.Length != _ftdiManager.Options.BUFFER_SIZE)
                    Send("DMX binary length does not match FTDI buffer size");
                else
                    _ftdiManager.UpdateBuffer(data);
            }
        }

        /// <summary>
        /// Handle opening of new connection on web socket server
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();

            _numberOfConnections++;

            if (_ftdiManager == null)
            {
                if (FTDIManager.IsInstanceRunning)
                    _ftdiManager = FTDIManager.Instance;
                else
                    _ftdiManager = new FTDIManager();
            }
        }
    }
}
