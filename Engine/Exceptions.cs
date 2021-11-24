/*
    *** Exceptions.cs ***
    Defines the various exceptions that can be thrown by the engine and its various components..
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Carcassonne;
using ExtraMath;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    [System.Serializable]
    public class IllegalMoveException : System.Exception
    {
        protected IllegalMoveException() { }
        public IllegalMoveException(string message) : base(message) { }
        public IllegalMoveException(string message, System.Exception inner) : base(message, inner) { }
        protected IllegalMoveException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class InvalidStateException : System.Exception
    {
        public InvalidStateException(GameEngine.State currentstate, GameEngine.State expectedstate)
            : this($"This operation requires the CurrentState to be {expectedstate}, but CurrentState=={currentstate}")
        {
            Assert(currentstate != expectedstate);
        }
        public InvalidStateException(string message) : base(message) { }
        public InvalidStateException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidStateException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [System.Serializable]
    public class HashCheckFailedException : System.Exception
    {
        public HashCheckFailedException()
            : this("A hash check has failed, the integrity of the provided byte array might be compromised. Are you trying to load a save from an incompatible version?") { }
        public HashCheckFailedException(string message) : base(message) { }
        public HashCheckFailedException(string message, System.Exception inner) : base(message, inner) { }
        protected HashCheckFailedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
