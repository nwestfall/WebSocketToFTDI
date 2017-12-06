[![Build status](https://ci.appveyor.com/api/projects/status/h8blk2fewx9a2yon/branch/master?svg=true)](https://ci.appveyor.com/project/nwestfall/websockettoftdi/branch/master)
[![NuGet version](https://badge.fury.io/nu/WebSocketToFTDI.svg)](https://badge.fury.io/nu/WebSocketToFTDI)

# WebSocketToFTDI 
Connect to an FTDI chip via a web socket.  Or use the library to have a wrapper for the FTDI chip and/or Web Sockets.

This library was originally created to communicate with an Enttecc OpenDMX device featuring a FTDI chip.  But it should be able to used for anything with a FTDI chip.

#### Requires .NET Standard 2.0

## Usage
There are two main classes that will be used.  FTIManager.cs and WebSocketManager.cs.  Both of these files existin the namespace WebSocketToFTDI.

### FDTIManager
Basic Usage
```c#
using WebSocketToFTDI;

namespace Program
{
  static Main(string[] args)
  {
    // Create new manager.  Connects automatically.  Can also be called with .Start()
    var manager = new FTDIManager();
    
    // Update values in buffer
    manager.SetValue(0, 50);
    
    // Update whole buffer
    var newBuffer = new byte[513];
    manager.UpdateBuffer(newBuffer);
    
    // Stop connection
    manager.Stop();
  }
}
```
Advanced Options
```c#
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
```

### WebSocketManager
Basic Usage
```c#
using WebSocketToFTDI;
using WebSocketToFTDI.Behaviors;

namespace Program
{
  static Main(string[] args)
  {
    // Creates a new manager and automatically starts the web socket server.  Can also call .Start()
    var manager = new WebSocketManager();
    
    // Add a new service
    manager.AddService<DMXWebSocketBehavior>("/dmx");
    
    // Remove a service
    manager.RemoveService("/dmx");
    
    // Stop server
    manager.Stop();
  }
}
```
Available Constructors
```c#
/// <summary>
/// Default ctor
/// </summary>
public WebSocketManager();

/// <summary>
/// Default ctor with port and secure flag
/// </summary>
/// <param name="port">The port.</param>
/// <param name="secure">The secure flag.</param>
public WebSocketManager(int port, bool secure);

/// <summary>
/// Default ctor with the url
/// </summary>
/// <param name="url">The url.</param>
public WebSocketManager(string url);

/// <summary>
/// Default ctor with url and port
/// </summary>
/// <param name="url">The url.</param>
/// <param name="port">The port.</param>
public WebSocketManager(string url, int port);
```
Available Properties
```c#
/// <summary>
/// Get the list of paths setup with the web socket
/// </summary>
public IEnumerable<string> AvailablePaths;
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
```

## Problems
If you are experiencing any issues, please submit an issue.
