using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Carcassonne;
using ExtraMath;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Networking
{
    class Server
    {
        public enum Mode
        {
            ERR=0,
            FAILED,
            CONNECTING,
            WAITING_FOR_PEERS,
            WAITING_FOR_START,
            IN_GAME,
            GAME_ENDED
        }
        IPAddress _relayIP;
        ushort _relayPort;
        UdpClient _server;
        Action<string> _fatalErrorHandle;
        Action<Peer> _connectedHandle;
        Action<Peer> _disconnectedHandle;
        public object MX{get;} = new object();

        Thread _receiveThread;
        Thread _mainThread;

        void MainLoop()
        {

        }
        void ReceiveLoop()
        {
            _receiver.
        }

        Server(IPAddress relayip, ushort relayport, 
            Action<string> _fatalErrorHandle,  Action<Peer> _connectedHandle,
            Action<Peer> _disconnectedHandle)
        {
            this._relayIP = relayip;
            this._relayPort = relayport;

            this._connectedHandle = _connectedHandle;
            this._disconnectedHandle = _disconnectedHandle;
            this._fatalErrorHandle = _fatalErrorHandle;

            _server = new UdpClient();

            lock(MX)
            {
                _mainThread = new Thread(MainLoop);
                _receiveThread = new Thread(ReceiveLoop);
                _mainThread.Start();
                _receiveThread.Start();
            }
            
        }
    }
}