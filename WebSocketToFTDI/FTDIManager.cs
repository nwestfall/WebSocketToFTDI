using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace WebSocketToFTDI
{
    /// <summary>
    /// FTDI Chip Manager
    /// </summary>
    public class FTDIManager : IDisposable
    {
        #region DLL Imports
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Open(UInt32 uiPort, ref uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Close(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Write(uint ftHandle, IntPtr lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesWritten);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetDataCharacteristics(uint ftHandle, byte uWordLength, byte uStopBits, byte uParity);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetFlowControl(uint ftHandle, char usFlowControl, byte uXon, byte uXoff);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Purge(uint ftHandle, UInt32 dwMask);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_ClrRts(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetBreakOn(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetBreakOff(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_ResetDevice(uint ftHandle);
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_SetDivisor(uint ftHandle, char usDivisor);
        #endregion

        #region Properties
        public FTDIOptions Options { get; private set; }
        public FT_STATUS Status
        {
            get => _status;
        }
        public bool IsConnected
        {
            get => _open;
        }

        private bool _open = false;
        private byte[] _buffer;
        private uint _handle;
        private int _bytesWritten = 0;
        private FT_STATUS _status;
        private readonly object _ftLock = new object();
        private Thread _writeLoop;
        private CancellationTokenSource _writeLoopCancelToken;
        #endregion

        /// <summary>
        /// Default ctor
        /// </summary>
        public FTDIManager()
        {
            Options = new FTDIOptions();
            _buffer = new byte[Options.BUFFER_SIZE];
            _handle = 0;

            Start();
        }

        /// <summary>
        /// Default ctor with <see cref="FTDIOptions"/>
        /// </summary>
        /// <param name="options"></param>
        public FTDIManager(FTDIOptions options)
        {
            Options = options;
            _buffer = new byte[Options.BUFFER_SIZE];
            _handle = 0;

            Start();
        }

        /// <summary>
        /// Start <see cref="FTDIManager"/> connection
        /// </summary>
        public void Start()
        {
            Open();

            _writeLoopCancelToken = new CancellationTokenSource();
            _writeLoop = new Thread(new ThreadStart(WriteData));
            _writeLoop.Start();
        }

        /// <summary>
        /// Stop <see cref="FTDIManager"/> connection
        /// </summary>
        public void Stop()
        {
            _writeLoopCancelToken?.Cancel();
            _writeLoop?.Abort();

            if (_open)
                _status = FT_Close(_handle);

            _handle = 0;
            _open = false;
        }

        /// <summary>
        /// Update entire buffer
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="ArgumentNullException">Data cannot be null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Size of data does not match buffer size</exception>
        public void UpdateBuffer(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length != Options.BUFFER_SIZE)
                throw new ArgumentOutOfRangeException("Size of new buffer does not match original buffer");

            lock(_ftLock)
            {
                for (var i = 0; i < Options.BUFFER_SIZE; i++)
                    _buffer[i] = data[i];
            }
        }

        /// <summary>
        /// Set value of a position in the buffer
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Position must be between 0 and the buffer size</exception>
        public void SetValue(int pos, byte value)
        {
            if (pos < 0)
                throw new ArgumentOutOfRangeException("Position cannot be less than 0");
            if (pos >= Options.BUFFER_SIZE)
                throw new ArgumentOutOfRangeException("Position cannot be greater than the buffer size");

            lock (_ftLock)
                _buffer[pos] = value;
        }

        private void Open()
        {
            if(!_open)
            {
                lock (_ftLock)
                    _status = FT_Open(0, ref _handle);
            }

            if (_status == FT_STATUS.FT_OK)
            {
                _open = true;
                InitChip();
            }
            else
                _open = false;
        }

        private void InitChip()
        {
            lock(_ftLock)
            {
                if(_open && _status == FT_STATUS.FT_OK)
                {
                    _status = FT_ResetDevice(_handle);
                    _status = FT_SetDivisor(_handle, (char)Options.BAUD_RATE); //Baud Rate
                    _status = FT_SetDataCharacteristics(_handle, Options.BITS, Options.STOP_BITS, Options.PARITY_NONE);
                    _status = FT_SetFlowControl(_handle, (char)Options.FLOW_NONE, Options.UX_ON, Options.UX_OFF);
                    _status = FT_ClrRts(_handle);
                    _status = FT_Purge(_handle, Options.PURGE_TX);
                    _status = FT_Purge(_handle, Options.PURGE_RX);
                }
            }
            _bytesWritten = 0;
        }

        private void WriteData()
        {
            do
            {
                if (_status == FT_STATUS.FT_OK)
                {
                    lock (_ftLock)
                    {
                        _status = FT_SetBreakOn(_handle);
                        _status = FT_SetBreakOff(_handle);
                        if (_status == FT_STATUS.FT_OK)
                            _bytesWritten = Write(_handle, _buffer, _buffer.Length);
                    }

                    Thread.Sleep(25);
                }
                else
                    Open();
                    
            } while (!_writeLoopCancelToken.IsCancellationRequested);
        }

        private int Write(uint handle, byte[] data, int length)
        {
            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.Copy(data, 0, ptr, length);
            uint bytesWritten = 0;
            lock (_ftLock)
                _status = FT_Write(_handle, ptr, (uint)length, ref bytesWritten);
            Marshal.FreeHGlobal(ptr);
            return (int)bytesWritten;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Stop();
                }

                _buffer = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// FTDI Chip Options
    /// </summary>
    public class FTDIOptions
    {
        /// <summary>
        /// Get or set Baud Rate (default: 12)
        /// </summary>
        public int BAUD_RATE { get; set; } = 12;
        /// <summary>
        /// Get or set Bits option (default: 8)
        /// </summary>
        public byte BITS { get; set; } = 8;
        /// <summary>
        /// Get or set Stop Bits (default: 2)
        /// </summary>
        public byte STOP_BITS { get; set; } = 2;
        /// <summary>
        /// Get or set Parity None (default: 0)
        /// </summary>
        public byte PARITY_NONE { get; set; } = 0;
        /// <summary>
        /// Get or set Flow None (default: 0)
        /// </summary>
        public UInt16 FLOW_NONE { get; set; } = 0;
        /// <summary>
        /// Get or set UX On (default: 0)
        /// </summary>
        public byte UX_ON { get; set; } = 0;
        /// <summary>
        /// Get or set UX Off (default: 0)
        /// </summary>
        public byte UX_OFF { get; set; } = 0;
        /// <summary>
        /// Get or set Purge RX (default: 1)
        /// </summary>
        public byte PURGE_RX { get; set; } = 1;
        /// <summary>
        /// Get or set Purge TX (default: 2)
        /// </summary>
        public byte PURGE_TX { get; set; } = 2;
        /// <summary>
        /// Get or set Buffer Size (default: 513)
        /// </summary>
        public int BUFFER_SIZE { get; set; } = 513;
    }

    /// <summary>
    /// Enumaration containing the varios return status for the DLL functions.
    /// </summary>
    public enum FT_STATUS
    {
        FT_OK = 0,
        FT_INVALID_HANDLE,
        FT_DEVICE_NOT_FOUND,
        FT_DEVICE_NOT_OPENED,
        FT_IO_ERROR,
        FT_INSUFFICIENT_RESOURCES,
        FT_INVALID_PARAMETER,
        FT_INVALID_BAUD_RATE,
        FT_DEVICE_NOT_OPENED_FOR_ERASE,
        FT_DEVICE_NOT_OPENED_FOR_WRITE,
        FT_FAILED_TO_WRITE_DEVICE,
        FT_EEPROM_READ_FAILED,
        FT_EEPROM_WRITE_FAILED,
        FT_EEPROM_ERASE_FAILED,
        FT_EEPROM_NOT_PRESENT,
        FT_EEPROM_NOT_PROGRAMMED,
        FT_INVALID_ARGS,
        FT_OTHER_ERROR
    };
}
