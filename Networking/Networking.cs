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
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Networking
{
    public class Peer 
    {
        public long ID;
        public IPAddress Address;
        public ushort Port;
        public string PublicKey{get; set;}
        public string PrivateKey{get; set;}
        public DateTime LastUpdate;
        
        public void Touch()
        {
            LastUpdate = DateTime.Now;
        }

    }
    public enum MessageType
    {
        ERR=0,
        
        SHARED_MESSAGES_START,
        MSG_CONFIRM,
        SHARED_MESSAGES_END,

        CLIENT_MESSAGES_START,
        CLIENT_ESTABLISH,
        CLIENT_AUTHENTICATE,
        CLIENT_SYNC,
        CLIENT_IS_ALIVE,
        CLIENT_DISCONNECT,
        CLIENT_MOVE,
        CLIENT_MESSAGES_END,

        SERVER_MESSAGES_START,
        SERVER_ESTABLISH,
        SERVER_SYNC,
        SERVER_OUT_OF_SYNC,
        SERVER_IS_CLIENT_ALIVE,
        SERVER_ACCEPTED,
        SERVER_REJECTED,
        SERVER_SHUTDOWN,
        SERVER_KICKED,
        SERVER_MESSAGES_END,
    }
    public struct Message 
    {
        public long MessageID;
        public long ReceiverID;
        public long SenderID;
        public bool Encrypted;
        public DateTime TimeSent;
        public bool Confirmed{get; set;}
        public MessageType MType;
        public byte[] Data;
    
    public class RelayedMessage
    {
        public IPAddress Address;
        public ushort Port;
        public Message message;
    }
}