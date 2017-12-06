using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebSocketToFTDI.Tests
{
    /// <summary>
    /// Tests to run against <see cref="FTDIManager"/> with FTDI chip plugged in
    /// </summary>
    [TestClass]
    public class FTDITest
    {
        private FTDIManager manager = new FTDIManager();

        /// <summary>
        /// Opening of FTDI
        /// </summary>
        [TestMethod]
        public void OpenFTDI()
        {
            manager.Start();
            Assert.AreEqual(FT_STATUS.FT_OK, manager.Status);
            Assert.IsTrue(manager.IsConnected);
            manager.Stop();
        }

        /// <summary>
        /// Close FTDI
        /// </summary>
        [TestMethod]
        public void CloseFTDI()
        {
            manager.Start();
            Assert.AreEqual(FT_STATUS.FT_OK, manager.Status);
            Assert.IsTrue(manager.IsConnected);
            manager.Stop();
            Assert.IsFalse(manager.IsConnected);
        }

        /// <summary>
        /// Dispose <see cref="FTDIManager"/>
        /// </summary>
        [TestMethod]
        public void DisposeFTDI()
        {
            manager.Start();
            Assert.AreEqual(FT_STATUS.FT_OK, manager.Status);
            Assert.IsTrue(manager.IsConnected);
            manager.Dispose();
            Assert.IsFalse(manager.IsConnected);
        }

        /// <summary>
        /// Write data to FTDI
        /// </summary>
        [TestMethod]
        public void WriteData()
        {
            manager.Start();
            Assert.AreEqual(FT_STATUS.FT_OK, manager.Status);
            Assert.IsTrue(manager.IsConnected);

            //Test values out of range
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { manager.SetValue(-1, 0); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { manager.SetValue(513, 0); });

            //test values in range
            manager.SetValue(0, 0);
            manager.SetValue(512, 0);

            //Test replace array of different size
            var badSizeArray = new byte[45];
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { manager.UpdateBuffer(badSizeArray); });
            Assert.ThrowsException<ArgumentNullException>(() => { manager.UpdateBuffer(null); });

            var goodSizeArray = new byte[513];
            manager.UpdateBuffer(goodSizeArray);

            manager.Stop();
            Assert.IsFalse(manager.IsConnected);
        }
    }
}
