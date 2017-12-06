using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WebSocketSharp;

namespace WebSocketToFTDI.Tests
{
    /// <summary>
    /// Tests to run against <see cref="WebSocketManager"/>
    /// </summary>
    [TestClass]
    public class WebSocketTest
    {
        /// <summary>
        /// Open Web Socket Test
        /// </summary>
        [TestMethod]
        public void OpenWebSocket()
        {
            var manager = new WebSocketManager();
            Assert.AreEqual(3000, manager.Port);
            Assert.IsFalse(manager.Secure);
            Assert.IsTrue(string.IsNullOrEmpty(manager.Url));
            manager.Stop();
        }

        /// <summary>
        /// Open Custom Web Socket Test
        /// </summary>
        [TestMethod]
        public void OpenCustomWebSocket()
        {
            var manager = new WebSocketManager(9999, false);
            Assert.AreEqual(9999, manager.Port);
            Assert.IsFalse(manager.Secure);
            Assert.IsTrue(string.IsNullOrEmpty(manager.Url));
            manager.Stop();
        }

        /// <summary>
        /// Open web socket from url
        /// </summary>
        [TestMethod]
        public void OpenUrlWebSocket()
        {
            var manager = new WebSocketManager("ws://localhost:3000");
            Assert.AreEqual(3000, manager.Port.Value);
            Assert.IsFalse(manager.Secure);
            Assert.AreEqual("ws://localhost", manager.Url);
            manager.Stop();
        }

        /// <summary>
        /// Connect to service via web socket
        /// </summary>
        [TestMethod]
        public void ConnectServiceToWebSocket()
        {
            var manager = new WebSocketManager("ws://localhost:3000");
            manager.AddService<Behaviors.TestBehavior>("/test");

            var client = new WebSocket("ws://localhost:3000/test");
            client.Connect();

            Thread.Sleep(500);

            Assert.IsTrue(Behaviors.TestBehavior.IsOpen);

            client.Send("Test Message");

            Thread.Sleep(500);

            Assert.AreEqual("Test Message", Behaviors.TestBehavior.LastMessage);

            client.Close();

            Thread.Sleep(500);

            Assert.IsTrue(Behaviors.TestBehavior.IsClosed);

            manager.Stop();
        }
    }
}
