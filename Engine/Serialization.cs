﻿using System;
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
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        public static byte[] SerializeAction(Action act)
        {
            Assert(act != null);
            Assert(act.IsFilled);
            void ToDictionaryRecursively(object o, Dictionary<string, object> dict)
            {
                Type[] extraprimitives = new Type[]
                {
                    typeof(Vector2), typeof(Vector3),
                    typeof(Vector2I), typeof(Vector3I),
                };
                foreach (var oit in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .ToList().ConvertAll<object>(v => (object)v)
                    .Concat(o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .ToList().ConvertAll(v => (object)v)))
                {
                    MemberInfo it = (MemberInfo)oit;
                    object v = null;
                    if (it is PropertyInfo nv)
                    {
                        if (nv.GetIndexParameters().Length > 0)
                            continue;
                        if (!nv.CanWrite)
                            continue;
                        v = nv.GetValue(o);
                    }
                    else if (it is FieldInfo nnv)
                    {
                        if (nnv.IsStatic)
                            continue;
                        v = nnv.GetValue(o);
                    }
                    else
                        Assert(false);
                    var type = v.GetType();
                    if (type.IsPrimitive || type.Namespace.StartsWith("System"))
                        dict.Add(it.Name, v);
                    else
                    {
                        Dictionary<string, object> ndict = new Dictionary<string, object>();
                        ToDictionaryRecursively(v, ndict);
                        dict.Add(it.Name, ndict);
                    }
                }
            }
            Dictionary<string, object> dict = new Dictionary<string, object>();
            ToDictionaryRecursively(act, dict);
            dict.Add("$TYPE", act.GetType().Name);
            string sv = JsonConvert.SerializeObject(dict);
            return sv.ToString().ToUTF8();
        }
        public static byte[] Serialize(GameEngine engine)
        {
            Assert(engine.History.Count > 0);
            List<byte> output = new List<byte>(engine.History.Count * 64);
            foreach (var act in engine.History)
            {
                byte[] dt = SerializeAction(act);
                output.AddRange(BitConverter.GetBytes(dt.Length));
                output.AddRange(dt);
            }
            return output.ToArray();
        }
        public byte[] Serialize()
        {
            return GameEngine.Serialize(this);
        }
        public static Action DeserializeAction(byte[] data, int start, int count)
        {
            void FromDictionaryRecursively(object o, Dictionary<string, object> dict)
            {
                foreach (var oit in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .ToList().ConvertAll<object>(v => (object)v)
                    .Concat(o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .ToList().ConvertAll(v => (object)v)))
                {
                    MemberInfo it = (MemberInfo)oit;
                    if (!dict.ContainsKey(it.Name))
                        continue;
                    object val = dict[it.Name];
                    if (it is PropertyInfo nv)
                    {
                        if (val is Dictionary<string, object> ndict)
                        {
                            object no = Activator.CreateInstance(nv.PropertyType);
                            FromDictionaryRecursively(no, ndict);
                            nv.SetValue(o, no);
                        }
                        else
                            nv.SetValue(o, Convert.ChangeType(val, nv.PropertyType));
                    }
                    else if (it is FieldInfo nnv)
                    {
                        if (val is Dictionary<string, object> ndict)
                        {
                            object no = Activator.CreateInstance(nnv.FieldType);
                            FromDictionaryRecursively(no, ndict);
                            nnv.SetValue(o, no);
                        }
                        else
                            nnv.SetValue(o, Convert.ChangeType(val, nnv.FieldType));
                    }
                    else
                        Assert(false);
                }
            }
            Assert(data.Length > 0);
            string sv = Encoding.UTF8.GetString(data, start, count);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(sv);

            Assert(dict.ContainsKey("$TYPE"));

            var typestr = (string)dict["$TYPE"];

            var type = GetAllActionTypes().First(tp => tp.Name == typestr);

            Assert(type != null);

            Action act = (Action)JsonConvert.DeserializeObject(sv, type);

            //Action act = (Action) Activator.CreateInstance(type, true);

            //FromDictionaryRecursively(act, dict);

            Assert(act != null);

            foreach (var it in act.GetType().GetProperties())
            {
                if (!dict.ContainsKey(it.Name))
                    continue;
                try
                {
                    if (it.PropertyType != dict[it.Name].GetType())
                        it.SetValue(act, Convert.ChangeType(dict[it.Name], it.PropertyType));
                    else
                        it.SetValue(act, dict[it.Name]);
                }
                catch (Exception)
                {

                }
            }
            foreach (var it in act.GetType().GetFields())
            {
                if (!dict.ContainsKey(it.Name))
                    continue;
                try
                {
                    if (it.FieldType != dict[it.Name].GetType())
                        it.SetValue(act, Convert.ChangeType(dict[it.Name], it.FieldType));
                    else
                        it.SetValue(act, dict[it.Name]);
                }
                catch (Exception)
                {

                }
            }

            Assert(act.IsFilled);

            return act;



            //var sw = new StringReader(str);
            //Action act = (Action)ser.Deserialize(sw);
            //Assert(act.IsFilled);
            //return act;
        }
        public static GameEngine Deserialize(IExternalDataSource dataSource, byte[] data)
        {
            List<Action> actions = new List<Action>();
            int i = 0;
            while (i < data.Length)
            {
                Assert(i + sizeof(int) < data.Length);
                int len = BitConverter.ToInt32(data, i);
                i += sizeof(int);
                Assert(i + len <= data.Length);
                var act = DeserializeAction(data, i, len);
                Assert(act != null);
                Assert(act.IsFilled);
                actions.Add(act);
                i += len;
            }
            return CreateFromHistory(dataSource, actions);
        }
    }
}