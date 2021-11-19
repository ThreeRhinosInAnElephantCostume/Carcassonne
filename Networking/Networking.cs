/* Networking.cs

    Main defines, basic message definitions, Message

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
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
    public enum MessageType
    {
        ERR = 0,

        MESSAGE_TYPE_BEFORE_FIRST,
        MSG_CONFIRM,
        MSG_ECHO,
        MSG_HAND_SHAKE,
        MSG_DISCONNECT,
        MESAGE_TYPE_AFTER_LAST,
    }
    public enum DisconnectReason
    {
        END,
        MY_FAULT,
        TIMEOUT,
        DDOS,
        INSANITY
    }
    public enum MessageReaction
    {
        ACCEPTED,
        DISCARDED,
        REJECTED,
        MALFORMED,
        ERROROUS
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MConfirm
    {
        public DateTime TimeReceived;
        public long MessageID;
        public MessageReaction reaction;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MHandshake
    {
        public bool Accepted;
        public long SenderID;
        public long ReceiverID;
        public string SenderPublicKey;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MDisconnect
    {
        public DisconnectReason Reason;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Message
    {
        private ulong Hash;
        public long MessageID;
        public long ReceiverID;
        public long SenderID;
        public bool Encrypted;
        public DateTime TimeSent;
        public bool RequestConfirmation;
        public bool IsResponse;
        public int MType;
        public byte[] Data;
        private void UpdateHash()
        {
            this.Hash = CalculateHash(this);
        }
        private unsafe static ulong CalculateHash(Message msg) // TODO: A better hashing function
        {
            ulong hash = long.MaxValue - int.MaxValue;
            msg.Hash = 666;
            byte* shiftptr = (byte*)&hash;
            int shiftpos = 0;
            foreach (var b in SerializeStruct(msg))
            {
                shiftptr[b] ^= b;
                hash ^= new RNG(hash + b).NextULong();
                shiftpos = (shiftpos + 1) % sizeof(ulong);
            }
            return hash;
        }
        private bool IsValid()
        {
            return CalculateHash(this) == this.Hash;
        }
        public unsafe byte[] Serialize()
        {
            UpdateHash();
            return SerializeStruct(this);
        }
        public static unsafe byte[] Serialize(Message msg)
        {
            return msg.Serialize();
        }
        public static unsafe Message Deserialize(byte[] dt)
        {
            Assert(dt != null);
            Message msg;
            try
            {
                msg = DeserializeStruct<Message>(dt);
            }
            catch (Exception)
            {
                throw new NetMessageMalformedException();
            }
            Assert<NetMessageInvalidHashException>(msg.IsValid());

            return msg;
        }
        public static Message BasicMessage(long ID, long SenderID, long ReceiverID, int type, bool response)
        {
            return new Message()
            {
                MessageID = ID,
                SenderID = SenderID,
                ReceiverID = ReceiverID,
                MType = type,
                TimeSent = DateTime.Now,
                IsResponse = response,
            };
        }
    }
    public static bool IsPeerID(long ID)
    {
        return ID >= 0;
    }
    public static (string PublicKey, string PrivateKey) GenerateKeyPair()
    {
        return ("123", "321");
    }
    public static string MessageInfo(Message msg)
    {
        return $"FROM: {msg.SenderID} TO: {msg.ReceiverID} ID: {msg.MessageID} TYPE: {msg.MType}";
    }
    public static bool CompareEndpoints(IPEndPoint v0, IPEndPoint v1)
    {
        return (v0.Port == v1.Port) && (v0.Address.Equals(v1.Address));
    }
}
