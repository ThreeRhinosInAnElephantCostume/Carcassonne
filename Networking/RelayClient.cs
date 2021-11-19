using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;
public static partial class Networking
{
    class RelayClient : Peer
    {
        Action<NetException> _failHandle;
        Action<Message> _onReceived;
        System.Action _onLoginFailure;
        Action<Connection> _onLoginSuccess;
        Action<Connection, bool> _onPeerDiscovered;
        Action<Connection, DisconnectReason> _onDisconnect;
        Action<MStatus> _onStatus;
        Connection _relayConnection;
        Connection _admin;
        List<Connection> _knownLoggedIn = new List<Connection>();
        List<Connection> _fellowUsers = new List<Connection>();
        string _password;
        bool _wantsAdmin;
        public IPEndPoint RelayEndpoint { get; protected set; }
        public bool IsAdmin { get; protected set; } = false;
        public bool IsLoggedIn { get; protected set; } = false;
        protected override MessageReaction CheckAccepted(IPEndPoint source, Message msg, Connection con)
        {
            var br = base.CheckAccepted(source, msg, con);
            if (br == MessageReaction.MALFORMED || br == MessageReaction.ERROROUS)
                return br;
            if (msg.MType > (int)MessageType.MESSAGE_TYPE_BEFORE_FIRST && msg.MType < (int)MessageType.MESAGE_TYPE_AFTER_LAST)
                return br;
            if (con == _relayConnection && !CompareEndpoints(source, _relayConnection.Endpoint))
            {
                LogEvent("Received a relay message that's not actually from relay's endpoint. Possible impersonation attempt?", Severity.ERROR);
                return MessageReaction.ERROROUS;
            }
            if (msg.MType > (int)RelayMessage.RELAY_BEFORE_FIRST && msg.MType < (int)RelayMessage.RELAY_ADMIN_AFTER_LAST)
            {
                if (_relayConnection != null && msg.SenderID == _relayConnection.ID && con.ID == _relayConnection.ID)
                {
                    return MessageReaction.ACCEPTED;
                }
                else
                {
                    LogEvent("Received a relay message from a non-relay source", Severity.ERROR);
                    return MessageReaction.ERROROUS;
                }
            }
            return MessageReaction.ACCEPTED;
        }
        protected MessageReaction ProcessRelayClientMessage(Message msg, Connection con)
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
                            if (!msg.IsResponse)
                            {
                                LogEvent("Client received a login request " + MessageInfo(msg), Severity.ERROR);
                                return MessageReaction.ERROROUS;
                            }
                            var m = DeserializeStruct<MLoginResponse>(msg.Data);
                            if (m.granted)
                            {
                                this.ID = m.newid;
                                LogEvent($"Logged in {(m.isadmin ? "as admin" : "as client")}", Severity.INFO);
                                IsLoggedIn = true;
                                _onLoginSuccess(con);
                            }
                            else
                            {
                                LogEvent("Failed to log in!", Severity.FAIL);
                                _onLoginFailure();
                            }
                            break;
                        }
                    case RelayMessage.RELAY_NOTIFY_LOGIN:
                    case RelayMessage.RELAY_NOTIFY_KNOWN_HOST:
                        {
                            if (!IsLoggedIn)
                            {
                                LogEvent("Received a host notification even though not logged in " + MessageInfo(msg), Severity.ERROR);
                                return MessageReaction.ERROROUS;
                            }
                            var m = DeserializeStruct<MNotifyLoggedExists>(msg.Data);
                            if (m.id == ID)
                            {
                                LogEvent("Received a login notification for self " + MessageInfo(msg), Severity.WARNING);
                                return MessageReaction.MALFORMED;
                            }
                            if (Connections.Any(it => it.ID == m.id))
                            {
                                LogEvent("Received a duplicate login notification " + MessageInfo(msg), Severity.WARNING);
                                return MessageReaction.MALFORMED;
                            }
                            var ncon = new Connection(m.id, _relayConnection.Address, _relayConnection.Port,
                                 _relayConnection.PublicKey, _relayConnection.PrivateKey);
                            Connections.Add(ncon);
                            NetAssert(!(m.isadmin && IsAdmin));
                            if (m.isadmin)
                            {
                                _admin = ncon;
                                LogEvent($"New admin: {m.id}", Severity.INFO);
                            }
                            else
                            {
                                LogEvent($"New peer: {m.id}", Severity.INFO);
                                _fellowUsers.Add(ncon);
                            }
                            _onPeerDiscovered(ncon, m.isadmin);
                            break;
                        }
                    case RelayMessage.RELAY_GET_MY_STATUS:
                        {
                            if (!msg.IsResponse)
                            {
                                LogEvent("Client received a server-only status request " + MessageInfo(msg), Severity.ERROR);
                                return MessageReaction.ERROROUS;
                            }
                            var m = DeserializeStruct<MStatus>(msg.Data);
                            NetAssert(m.isadmin == IsAdmin);
                            NetAssert(m.islogged == IsLoggedIn);
                            LogEvent($"Status received. IsLogged: {m.islogged} IsAdmin: {m.isadmin}", Severity.INFO);
                            if (_onStatus != null)
                                _onStatus(m);
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
                LogEvent("Failed to deserialize payload " + MessageInfo(msg), Severity.ERROR);
                return MessageReaction.ERROROUS;
            }
            return MessageReaction.ACCEPTED;
        }
        protected override MessageReaction ProcessMessage(IPEndPoint source, Message msg, Connection con)
        {
            if (msg.MType > (int)MessageType.MESSAGE_TYPE_BEFORE_FIRST && msg.MType < (int)MessageType.MESAGE_TYPE_AFTER_LAST)
                return base.ProcessMessage(source, msg, con);
            if (msg.MType > (int)RelayMessage.RELAY_BEFORE_FIRST && msg.MType < (int)RelayMessage.RELAY_AFTER_LAST)
                return ProcessRelayClientMessage(msg, con);

            _onReceived(msg);
            return MessageReaction.ACCEPTED;
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
        void LogIn(string password, bool asadmin)
        {
            QueueSendMessage(BasicMessage(_relayConnection, (int)((asadmin) ? RelayMessage.RELAY_LOGIN_ADMIN : RelayMessage.RELAY_LOGIN_CLIENT),
            false, new MLogin() { password = password }),
                (sm) =>  // fail 
                {
                    _onLoginFailure();
                },
                (sm) =>  // success 
                {

                });
        }
        protected override Connection FinalizeConnection(IPEndPoint endpoint, long ID, string publickey, string privatekey)
        {
            var con = base.FinalizeConnection(endpoint, ID, publickey, privatekey);
            _relayConnection = con;
            LogIn(_password, _wantsAdmin);
            return con;
        }
        public override void Start()
        {
            base.Start();
            ConnectToServer(RelayEndpoint);
        }
        public RelayClient(IPEndPoint relayserver, ushort myport, string password, bool isadmin, bool start = false) : base(myport, false)
        {
            this._password = password;
            this._wantsAdmin = isadmin;
            this.RelayEndpoint = relayserver;
            this.Port = myport;
            if (start)
            {
                Start();
            }
        }
    }
}
