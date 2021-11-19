/* NetworkErrors.cs

    Various exceptions and assertions

*/

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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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

    [System.Serializable]
    public class NetException : System.Exception
    {
        public NetException() { }
        public NetException(string message) : base(message) { }
        public NetException(string message, System.Exception inner) : base(message, inner) { }
        protected NetException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetAssertionException : NetException
    {
        public NetAssertionException() { }
        public NetAssertionException(string message) : base(message) { }
        public NetAssertionException(string message, System.Exception inner) : base(message, inner) { }
        protected NetAssertionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetMessageMalformedException : NetException
    {
        public NetMessageMalformedException() { }
        public NetMessageMalformedException(string message) : base(message) { }
        public NetMessageMalformedException(string message, System.Exception inner) : base(message, inner) { }
        protected NetMessageMalformedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetMessageInvalidHashException : NetException
    {
        public NetMessageInvalidHashException() { }
        public NetMessageInvalidHashException(string message) : base(message) { }
        public NetMessageInvalidHashException(string message, System.Exception inner) : base(message, inner) { }
        protected NetMessageInvalidHashException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public abstract class NetConnectionBrokenException : NetException
    {
        public NetConnectionBrokenException() { }
        public NetConnectionBrokenException(string message) : base(message) { }
        public NetConnectionBrokenException(string message, System.Exception inner) : base(message, inner) { }
        protected NetConnectionBrokenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetConnectionTimeoutException : NetConnectionBrokenException
    {
        public NetConnectionTimeoutException() { }
        public NetConnectionTimeoutException(string message) : base(message) { }
        public NetConnectionTimeoutException(string message, System.Exception inner) : base(message, inner) { }
        protected NetConnectionTimeoutException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetConnectionDDOSException : NetConnectionBrokenException
    {
        public NetConnectionDDOSException() { }
        public NetConnectionDDOSException(string message) : base(message) { }
        public NetConnectionDDOSException(string message, System.Exception inner) : base(message, inner) { }
        protected NetConnectionDDOSException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetConnectionInsaneException : NetConnectionBrokenException
    {
        public NetConnectionInsaneException() { }
        public NetConnectionInsaneException(string message) : base(message) { }
        public NetConnectionInsaneException(string message, System.Exception inner) : base(message, inner) { }
        protected NetConnectionInsaneException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class NetFailedToConnectToServerException : NetException
    {
        public NetFailedToConnectToServerException() { }
        public NetFailedToConnectToServerException(string message) : base(message) { }
        public NetFailedToConnectToServerException(string message, System.Exception inner) : base(message, inner) { }
        protected NetFailedToConnectToServerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public static void NetAssert(bool b, string msg = "An unspecified network assertion has been triggered")
    {
        if (!b)
            throw new NetAssertionException(msg);
    }

}
