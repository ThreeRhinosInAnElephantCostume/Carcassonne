/* NetworkingConstants.cs

    Networking constants

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
    public const int MAX_EXTRA_DELAY = 300;
    public const int FORCED_DISCONNECT_TIMEOUT = 3000;
    public const int AUTO_ECHO_TIMEOUT = 300;
    public const int AUTO_ECHO_AFTER = 300;
    public const int IMPORTANT_RETRIES = 3;
    public const int UNIMPORTANT_RETRIES = 1;
    public const int DEFAULT_TIMEOUT = 500;
    public const int PING_SAMPLES = 10;
    public const int PACKET_LOSS_SAMPLES = 100;
    public const int CONNECTION_TIMEOUT_MS = 2000;
    public const int MAX_MESSAGES_PER_SECOND = 2048;
    public const int MAX_MESSAGES_PER_MINUTE = 32768;
    public const int MAX_ERROROUS = 1024;
    public const int ID_ADMIN = 1;
    public const int ID_RELAY = 0;
    public const int ID_NULL = -1;
    public const int ID_ALL = -2;
    public const int ID_UNKNOWN = -3;
    public const int MSG_ID_NO_CONNECTION = -1;
}
