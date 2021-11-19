using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public static partial class Networking
{
    class Server
    {
        // Server(IPAddress relayip, ushort relayport,
        //     Action<string> _fatalErrorHandle, Action<Connection> _connectedHandle,
        //     Action<Connection> _disconnectedHandle)
        // {
        //     this._relayIP = relayip;
        //     this._relayPort = relayport;

        //     this._connectedHandle = _connectedHandle;
        //     this._disconnectedHandle = _disconnectedHandle;
        //     this._fatalErrorHandle = _fatalErrorHandle;

        //     _server = new UdpClient();
        //     _server.Connect(relayip, relayport);

        //     lock (MX)
        //     {
        //         _receiveThread = new Thread(ReceiveLoop);
        //         _receiveThread.Start();
        //         _mainThread = new Thread(MainLoop);
        //         _mainThread.Start();
        //     }

        // }
    }
}
