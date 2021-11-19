using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Networking;
using static Utils;

public static partial class Networking
{
    public abstract partial class Peer
    {
        public class Connection
        {
            public long ID;
            public IPAddress Address;
            public ushort Port;
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
            public DateTime LastUpdate { get; protected set; }
            public DateTime FirstUpdate { get; protected set; }
            public DateTime LastSent { get; protected set; }
            public IPEndPoint Endpoint => new IPEndPoint(Address, Port);
            public bool IsPingValid { get; protected set; } = false;
            public double Ping { get; protected set; } = 0; // in seconds, measured over 10 samples
            public double PacketLoss { get; protected set; } = 0; // 0.0 - 1.0, in the past 100 packets
            public bool Disconnected { get; set; } = false;

            FIFO<double> _pingCount = new FIFO<double>(PING_SAMPLES);
            FIFO<bool> _packetLossCount = new FIFO<bool>(PACKET_LOSS_SAMPLES);

            public ulong TotalSentTo { get; protected set; } = 0;
            public ulong TotalReceivedFrom { get; protected set; } = 0;
            public ulong TotalAccepted { get; protected set; } = 0;
            public ulong TotalDiscared { get; protected set; } = 0;
            public ulong TotalRejected { get; protected set; } = 0;
            public ulong TotalMalformed { get; protected set; } = 0;
            public ulong TotalErrorous { get; protected set; } = 0;
            public ulong TotalLost { get; protected set; } = 0;
            public ulong TotalDelayed { get; protected set; } = 0;
            DateTime _minuteUpdate = DateTime.Now;
            ulong _messagesThisMinute = 0;
            DateTime _secondUpdate = DateTime.Now;
            ulong _messagesThisSecond = 0;
            public long LastID { get; protected set; }
            long _oldestAcceptable = 0;
            long _gapstart = 0;
            public List<(ReceivedMessage rmsg, DateTime rtime)> DelayedMessages = new List<(ReceivedMessage, DateTime)>();
            void UpdatePing(double s)
            {
                _pingCount.Enqueue(s);
                IsPingValid = _pingCount.Full;
                Ping = 0;
                foreach (var v in _pingCount)
                {
                    Ping += v / _pingCount.Count;
                }
            }
            void UpdatePacketLoss()
            {
                if (_packetLossCount.Full)
                {
                    double pl = 0;
                    foreach (var it in _packetLossCount)
                    {
                        pl += (it) ? 0 : 1;
                    }
                    PacketLoss /= _packetLossCount.Capacity;
                }
            }
            void UpdateConnectionStatus()
            {
                UpdatePacketLoss();
                var sincelast = DateTime.Now - LastUpdate;
                Assert<NetConnectionTimeoutException>(sincelast.TotalMilliseconds < CONNECTION_TIMEOUT_MS);
                Assert<NetConnectionDDOSException>(_messagesThisMinute < MAX_MESSAGES_PER_MINUTE);
                Assert<NetConnectionDDOSException>(_messagesThisSecond < MAX_MESSAGES_PER_SECOND);
                Assert<NetConnectionInsaneException>(TotalErrorous < MAX_ERROROUS);
            }
            public void Touch()
            {
                LastUpdate = DateTime.Now;
            }
            public void NoteDeayed()
            {
                Touch();
                TotalReceivedFrom++;
                TotalDelayed++;
                UpdateConnectionStatus();
            }
            public void NoteMessage(ReceivedMessage rmsg, MessageReaction reaction, TimeSpan time, bool wasdelayed)
            {
                Touch();
                LastID = rmsg.Msg.MessageID;
                if (reaction == MessageReaction.DISCARDED)
                    return;
                _packetLossCount.Enqueue(true);
                if (!wasdelayed)
                    TotalReceivedFrom++;
                var x = reaction switch
                {
                    MessageReaction.ACCEPTED => TotalAccepted++,
                    MessageReaction.DISCARDED => TotalDiscared++,
                    MessageReaction.REJECTED => TotalRejected++,
                    MessageReaction.MALFORMED => TotalMalformed++,
                    MessageReaction.ERROROUS => TotalErrorous++,
                    _ => throw new AssertionFailureException("Invalid message reaction"),
                };

                var now = DateTime.Now;

                if ((now - _secondUpdate).TotalSeconds > 1)
                    _messagesThisSecond = 0;
                _messagesThisSecond++;

                if ((now - _secondUpdate).TotalMinutes > 1)
                    _messagesThisMinute = 0;
                _messagesThisMinute++;

                UpdatePing(time.TotalSeconds);
                UpdateConnectionStatus();
            }
            public void NoteMessageSent()
            {
                TotalSentTo++;
                UpdateConnectionStatus();
                LastSent = DateTime.Now;
            }
            public void NoteMessageConfirmed()
            {
                _packetLossCount.Enqueue(true);
            }
            public void NoteMessageLost()
            {
                TotalLost++;
                _packetLossCount.Enqueue(false);
                UpdateConnectionStatus();
            }
            long _idCounter = 1;
            public long GenerateMessageID()
            {
                return _idCounter++;
            }
            public Connection(long ID, IPAddress address, ushort port, string publickey, string privatekey)
            {
                this.ID = ID;
                this.Address = address;
                this.Port = port;
                this.LastUpdate = DateTime.Now;
                this.FirstUpdate = DateTime.Now;
                this.PublicKey = publickey;
                this.PrivateKey = privatekey;
            }
        }
    }
}
