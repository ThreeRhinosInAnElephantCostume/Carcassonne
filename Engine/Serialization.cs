﻿/*
    *** Serialization.cs ***

    Various functions for serializing and deserializing GameEngine instances.
    The resulting byte[] arrays should be saved and loaded as such, 
    even though they may happen to contain UTF-8 strings
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
    public partial class GameEngine
    {
        ///<summary>
        /// Serializes a given action. 
        /// Do note that while the resulting byte array may contain some UTF-8 strings, it should not be thought of as an UTF-8 string.
        ///</summary>
        public static byte[] SerializeAction(Action act)
        {
            Assert(act != null);
            Assert(act.IsFilled);
            void ToDictionaryRecursively(object o, Dictionary<string, object> dict)
            {
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
            return Encoding.UTF8.GetBytes(sv.ToString());
        }
        ///<summary>
        /// Serializes an engine instance. 
        /// Do note that while the resulting byte array may contain some UTF-8 strings, it should not be thought of as an UTF-8 string.
        /// Additionally, the resulting byte array is prepended by a hash generated by GetHash(). 
        ///</summary>
        public static byte[] Serialize(GameEngine engine)
        {
            Assert(engine.History.Count > 0);
            byte[] hash = engine.GetHash();
            List<byte> output = new List<byte>(engine.History.Count * 64 + hash.Length + 4);
            output.AddRange(BitConverter.GetBytes((int)hash.Length));
            output.AddRange(hash);
            foreach (var act in engine.History)
            {
                byte[] dt = SerializeAction(act);
                output.AddRange(BitConverter.GetBytes(dt.Length));
                output.AddRange(dt);
            }
            return output.ToArray();
        }

        ///<summary> See static byte[] Serialize(GameEngine engine) </summary>
        public byte[] Serialize()
        {
            return GameEngine.Serialize(this);
        }
        ///<summary>
        /// Deserializes an Action
        ///</summary>
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
        }
        ///<summary>
        /// Deserializes a GameEngine instance.
        /// An optional (enabled by default) hash check is provided.
        ///</summary>
        public static GameEngine Deserialize(IExternalDataSource dataSource, byte[] data, bool checkhash = true)
        {
            List<Action> actions = new List<Action>();
            int i = 0;
            Assert(i + sizeof(int) < data.Length);
            int hashlen = BitConverter.ToInt32(data, i);
            i += sizeof(int);
            Assert(i + hashlen < data.Length);
            byte[] hash = data.Skip(i).Take(hashlen).ToArray();
            i += hashlen;
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
            var eng = CreateFromHistory(dataSource, actions);
            if (checkhash)
            {
                var nhash = eng.GetHash();
                Assert(nhash.Length == hash.Length);
                RepeatN(hash.Length, (i) => Assert(nhash[i] == hash[i]));
            }
            return eng;
        }
    }
}
