using System;
using System.Collections.Generic;
using System.Text;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketToFTDI.Tests.Behaviors
{
    public class TestBehavior : WebSocketBehavior
    {
        public static bool IsClosed { get; private set; } = false;
        public static bool IsOpen { get; private set; } = false;
        public static string LastMessage { get; private set; } = string.Empty;

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            IsOpen = false;
            IsClosed = true;
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            LastMessage = e.Data;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            IsOpen = true;
            IsClosed = false;
        }
    }
}
