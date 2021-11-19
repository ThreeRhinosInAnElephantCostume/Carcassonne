using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    public enum RelayMessage
    {
        RELAY_BEFORE_FIRST = MessageType.MESAGE_TYPE_AFTER_LAST,

        RELAY_LOGIN_ADMIN,
        RELAY_LOGIN_CLIENT,
        RELAY_NOTIFY_LOGIN,
        RELAY_NOTIFY_KNOWN_HOST,
        RELAY_NOTIFY_DISCONNECT,
        RELAY_NOTIFY_SHUTDOWN,
        RELAY_GET_MY_STATUS,

        RELAY_ADMIN_BEFORE_FIRST,
        RELAY_ADMIN_KICK_MEMBER,
        RELAY_ADMIN_BAN_MEMBER,
        RELAY_ADMIN_SET_PASSWORD,
        RELAY_ADMIN_POWER_OFF,
        RELAY_ADMIN_AFTER_LAST,
        RELAY_CLIENT_BEFORE_FIRST = RELAY_ADMIN_AFTER_LAST,
        RELAY_CLIENT_GET_STATUS,
        RELAY_CLIENT_AFTER_LAST,
        RELAY_AFTER_LAST,
    }
    public enum DisconnectType
    {
        END = 0,
        KICK,
        BAN,
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MLogin
    {
        public string password;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MSetPassword
    {
        public string password;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MLoginResponse
    {
        public bool granted;
        public long newid;
        public bool isadmin;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MNotifyDisconnected
    {
        public long DisconnectedID;
        public DisconnectType Reason;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MNotifyLoggedExists
    {
        public long id;
        public bool isnew;
        public bool isadmin;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MStatus
    {
        public bool isadmin;
        public bool islogged;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MKickBan
    {
        public long ID;
    }

    class RelayServer : Peer
    {
        Action<NetException> _failHandle;
        Connection _adminConnection;
        List<Connection> _loggedIn = new List<Connection>();
        string _adminPassword;
        string _clientPassword = null;
        bool _adminLogging = false;
        public List<Connection> LoggedIn { get { lock (_loggedIn) { return _loggedIn.ToList(); } } }
        protected bool IsAllowedToMessage(Message msg, Connection con)
        {
            bool islogged = _loggedIn.Contains(con);
            bool isadmin = con == _adminConnection;
            if (msg.MType > (int)RelayMessage.RELAY_ADMIN_BEFORE_FIRST && msg.MType < (int)RelayMessage.RELAY_ADMIN_AFTER_LAST)
                return isadmin;
            if (msg.MType > (int)RelayMessage.RELAY_CLIENT_BEFORE_FIRST && msg.MType < (int)RelayMessage.RELAY_CLIENT_AFTER_LAST)
                return islogged;
            return true;
        }
        void RelayMessageTo(Message msg, Connection target, long sourceID, bool modsenderreceiver = true)
        {
            if (modsenderreceiver)
            {
                msg.SenderID = sourceID;
                msg.ReceiverID = target.ID;
            }
            msg.MessageID = target.GenerateMessageID();
            QueueSendMessage(msg, msg.RequestConfirmation);
        }
        void RelayToAll(Connection from, Message msg, bool modsenderreceiver = true, bool includefrom = false)
        {
            List<Connection> c; ;
            lock (_loggedIn)
            {
                c = _loggedIn.ToList();
            }
            if (!includefrom && c.Contains(from))
                c.Remove(from);
            c.ForEach(it =>
            {
                RelayMessageTo(msg, it, from.ID, modsenderreceiver);
            });
        }
        void SendToAll(Message msg, bool requestreturn = true)
        {
            List<Connection> c; ;
            lock (_loggedIn)
            {
                c = _loggedIn.ToList();
            }
            c.ForEach(it => QueueSendMessage(msg, requestreturn));
        }
        protected override void DisconnectConnection(Connection con, DisconnectReason reason)
        {
            base.DisconnectConnection(con, reason);

            Connections.Remove(con);
            lock (_loggedIn)
            {
                _loggedIn.Remove(con);
            }
        }
        protected MessageReaction ProcessRelayMessage(Message msg, Connection con)
        {
            if (msg.MType > (int)RelayMessage.RELAY_BEFORE_FIRST && msg.MType < (int)RelayMessage.RELAY_AFTER_LAST)
                return MessageReaction.ERROROUS;
            try
            {
                switch ((RelayMessage)msg.MType)
                {
                    case RelayMessage.RELAY_LOGIN_CLIENT:
                    case RelayMessage.RELAY_LOGIN_ADMIN:
                        {
                            bool isadmin = (RelayMessage)msg.MType == RelayMessage.RELAY_LOGIN_ADMIN;
                            if (msg.IsResponse)
                                return MessageReaction.MALFORMED;
                            if (_adminLogging && isadmin)
                                return MessageReaction.REJECTED;
                            var m = DeserializeStruct<MLogin>(msg.Data);

                            bool granted = (isadmin && m.password == _adminPassword || (!isadmin && m.password == _clientPassword));
                            if (!granted)
                            {
                                QueueSendMessage(BasicMessage(con, (int)RelayMessage.RELAY_LOGIN_ADMIN, true, new MLoginResponse()
                                {
                                    granted = granted,
                                }), true);
                            }
                            else
                            {
                                if (isadmin)
                                {
                                    AlternativeAddresses.Add(con.ID, ID_ADMIN);
                                }
                                _adminLogging = true;
                                QueueSendMessage(BasicMessage(con, (int)RelayMessage.RELAY_LOGIN_ADMIN, true, new MLoginResponse()
                                {
                                    granted = (m.password == _adminPassword),
                                    newid = (isadmin) ? ID_ADMIN : con.ID,
                                    isadmin = isadmin,
                                }), (sm) =>  // fail
                                {
                                    _adminLogging = false;
                                    AlternativeAddresses.Remove(con.ID);
                                }, (sm) =>  // success
                                {
                                    if (isadmin)
                                    {
                                        _adminConnection = con;
                                        con.ID = ID_ADMIN;
                                    }
                                    lock (_loggedIn)
                                    {
                                        _loggedIn.Add(con);
                                    }
                                    LogEvent($"Successful login as {((isadmin) ? "ADMIN" : "CLIENT")}. ID: {con.ID}.", Severity.INFO);
                                });
                            }
                            break;
                        }
                    case RelayMessage.RELAY_NOTIFY_SHUTDOWN:
                    case RelayMessage.RELAY_NOTIFY_KNOWN_HOST:
                        {
                            return MessageReaction.ERROROUS;
                        }
                    case RelayMessage.RELAY_NOTIFY_DISCONNECT:
                        {
                            if (_loggedIn.Contains(con))
                            {
                                RelayToAll(con, msg);
                                DisconnectConnection(con, DisconnectReason.END);
                            }
                            break;
                        }
                    case RelayMessage.RELAY_ADMIN_BAN_MEMBER:
                    case RelayMessage.RELAY_ADMIN_KICK_MEMBER:
                        {
                            bool ban = msg.MType == (int)RelayMessage.RELAY_ADMIN_BAN_MEMBER;
                            var m = DeserializeStruct<MKickBan>(msg.Data);
                            if (m.ID == ID_ALL || m.ID == ID_RELAY || m.ID == ID_ADMIN)
                            {
                                LogEvent($"Attempting to kick/ban a protected ID. FROM: {msg.SenderID} REMID: {m.ID}", Severity.ERROR);
                                return MessageReaction.ERROROUS;
                            }
                            var cons = LoggedIn;
                            var found = cons.Find(it => MatchIDs(it.ID, m.ID));
                            if (found == null)
                            {
                                LogEvent($"Attempted to kick/ban a non-existent peer. FROM: {msg.SenderID} REM: {m.ID}", Severity.WARNING);
                                return MessageReaction.MALFORMED;
                            }
                            SendToAll(BasicMessage(found, (int)RelayMessage.RELAY_NOTIFY_DISCONNECT, false,
                                new MNotifyDisconnected()
                                {
                                    DisconnectedID = m.ID,
                                    Reason = (ban) ? DisconnectType.BAN : DisconnectType.KICK
                                }), true);
                            DisconnectConnection(found, DisconnectReason.END);
                            break;
                        }
                    case RelayMessage.RELAY_ADMIN_POWER_OFF:
                        {
                            throw new NotImplementedException();
                            break;
                        }
                    case RelayMessage.RELAY_ADMIN_SET_PASSWORD:
                        {
                            var m = DeserializeStruct<MSetPassword>(msg.Data);
                            _adminPassword = m.password;
                            break;
                        }
                    case RelayMessage.RELAY_CLIENT_GET_STATUS:
                        {
                            QueueSendMessage(BasicMessage(con, (int)RelayMessage.RELAY_GET_MY_STATUS,
                            true, new MStatus
                            {
                                isadmin = con == _adminConnection,
                                islogged = LoggedIn.Contains(con),
                            }), true);
                            break;
                        }
                    default:
                        {
                            return MessageReaction.MALFORMED;
                        }
                }
            }
            catch (DeserializationFailedException)
            {
                return MessageReaction.ERROROUS;
            }
            return MessageReaction.ACCEPTED;
        }
        protected override MessageReaction ProcessMessage(IPEndPoint source, Message msg, Connection con)
        {
            if (!IsAllowedToMessage(msg, con))
                return MessageReaction.ERROROUS;
            if (msg.ReceiverID == ID_RELAY || MatchIDs(ID, msg.ReceiverID))
            {
                if (msg.MType > (int)MessageType.MESSAGE_TYPE_BEFORE_FIRST && msg.MType < (int)MessageType.MESAGE_TYPE_AFTER_LAST)
                    return base.ProcessMessage(source, msg, con);
                return ProcessRelayMessage(msg, con);
            }
            if (msg.ReceiverID == ID_UNKNOWN)
            {
                LogEvent("Attempting to relay to ID_UNKNOWN " + MessageInfo(msg), Severity.ERROR);
                return MessageReaction.ERROROUS;
            }
            if (msg.ReceiverID == ID_ALL)
            {
                RelayToAll(con, msg);
                return MessageReaction.ACCEPTED;
            }
            int nsent = 0;
            foreach (var it in _loggedIn.FindAll(it => it.ID != msg.SenderID && MatchIDs(it.ID, msg.ReceiverID)))
            {
                RelayMessageTo(msg, it, msg.SenderID);
                nsent++;
            }
            if (nsent == 0)
            {
                LogEvent($"Message that matched no connections ID: {msg.MessageID} FROM: {msg.SenderID} TO: {msg.ReceiverID}",
                    Severity.ERROR);
                return MessageReaction.ERROROUS;
            }
            return (nsent > 0) ? MessageReaction.ACCEPTED : MessageReaction.REJECTED;

        }
        protected override MessageReaction CheckAccepted(IPEndPoint source, Message msg, Connection con)
        {
            var br = base.CheckAccepted(source, msg, con);
            if (br == MessageReaction.MALFORMED || br == MessageReaction.ERROROUS)
                return br;
            if (msg.MType != (int)MessageType.MSG_CONFIRM)
                if (msg.MType > (int)MessageType.MESSAGE_TYPE_BEFORE_FIRST && msg.MType < (int)MessageType.MESAGE_TYPE_AFTER_LAST)
                    return br;
            Assert(con != null);
            if (!IsAllowedToMessage(msg, con))
                return MessageReaction.ERROROUS;
            if (msg.MType > (int)RelayMessage.RELAY_BEFORE_FIRST && msg.MType < (int)RelayMessage.RELAY_AFTER_LAST)
            {
                if (msg.ReceiverID == ID_RELAY)
                    return MessageReaction.ACCEPTED;
                else
                {
                    LogEvent($@"A message in the relay range addressed to someone else received.
                        ID: {msg.MessageID} FROM: {msg.SenderID} TO: {msg.ReceiverID} 
                        TYPE: {msg.MType}.", Severity.ERROR);
                    return MessageReaction.MALFORMED;
                }
            }
            if (msg.ReceiverID == ID_ADMIN)
                return MessageReaction.ACCEPTED;
            if (msg.ReceiverID == ID_ALL)
                return MessageReaction.ACCEPTED;
            if (Connections.Any(it => MatchIDs(it.ID, msg.ReceiverID)))
                return MessageReaction.ACCEPTED;
            return MessageReaction.REJECTED;
        }
        protected override void OnFailure(NetException ex)
        {
            FailureShutdown();
            _failHandle(ex);
        }
        protected override void OnLog(string msg, Severity s)
        {
            Console.WriteLine(s.ToString() + ": " + msg);
        }

        public RelayServer(ushort port, Action<NetException> _failHandle, string adminpassword, string clientpassword, bool start = false) : base(port, false)
        {
            this._failHandle = _failHandle;
            this._adminPassword = adminpassword;
            this._clientPassword = clientpassword;
            this.ID = ID_RELAY;
            if (start)
                Start();
        }
    }

}
