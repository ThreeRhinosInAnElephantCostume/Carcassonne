﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;


namespace Carcassonne
{
    public enum NodeType
    {
        ERR=0,
        FARM,
        ROAD,
        CITY,
    }
    public partial class GameEngine
    {
        public static string GetTypeAbrv(NodeType type)
        {
            return type switch
            {
                NodeType.FARM => "F",
                NodeType.ROAD => "R",
                NodeType.CITY => "C",
                _ => "!",
            };        
        }
        public static string GetTypeName(NodeType type)
        {
            string s = type.ToString().ToLower();
            Assert(s.Length > 0);
            s = (s[0] + "").ToUpper() + s.Substr(1, s.Length-1);
            return s;
        }
    }
}
