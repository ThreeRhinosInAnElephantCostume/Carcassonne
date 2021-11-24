/*
    *** Hash.cs ***

    Functions for generating hashes that can be used to verify state integrity between different machines.

    See GetHash()/HashData() to see the algorithm used.

    Do note that RNG defaults to Xorshift64s for its algorithm.
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
using System.Xml.Serialization;
using Carcassonne;
using ExtraMath;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        public byte[] GetHash()
        {
            byte[] hash = new byte[16];
            for (int i = 0; i < hash.Length; i++)
                hash[i] = (byte)(i * hash.Length);
            int indx = 0;
            List<object> AlreadyScanned = new List<object>(1024);
            void HashData(byte[] dt)
            {
                foreach (var it in dt)
                {
                    hash[indx] ^= it;
                    indx++;
                    indx %= hash.Length;
                }
                var rngh = new RNG(BitConverter.ToUInt64(hash, 0));
                var rngl = new RNG(BitConverter.ToUInt64(hash, 8));
                for (int i = 0; i < hash.Length; i++)
                {
                    hash[i] ^= rngh.NextBytes(1)[0];
                    hash[i] ^= rngl.NextBytes(1)[0];
                }
            }
            void HashInt(int dt)
            {
                HashData(BitConverter.GetBytes(dt));
            }
            int depth = 0;
            void HashRecursively(object o)
            {
                depth++;
                if (o == null)
                {
                    HashInt(0);
                    goto exit;
                }
                if (AlreadyScanned.Contains(o))
                    goto exit;
                AlreadyScanned.Add(o);
                var type = o.GetType();
                int width = 0;
                if (o.GetType().IsPrimitive)
                {
                    HashInt(o.GetHashCode());
                }
                else if (o is System.Collections.IEnumerable array)
                {
                    foreach (var val in array)
                    {
                        HashRecursively(o);
                    }
                }
                else if (type.IsClass && (type.Namespace == null || !type.Namespace.StartsWith("System")))
                {
                    foreach (var it in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        object v = it.GetValue(o);
                        if (v is string || it.Name == "MetaData")
                            continue;
                        width++;
                        HashRecursively(v);
                    }
                    foreach (var it in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        var ipars = it.GetIndexParameters();
                        if (ipars.Length > 0)
                        {
                            continue;
                        }
                        object v = it.GetValue(o);
                        if (v is string || it.Name == "MetaData")
                            continue;
                        width++;
                        HashRecursively(v);
                    }
                    HashInt(width);
                }
exit:;
                HashInt(depth);
                depth--;
                return;
            }
            HashRecursively(this);
            return hash;
        }
        public string GetHashBase16()
        {
            string ret = "";
            byte[] hash = GetHash();
            foreach (var it in hash)
            {
                int high = (it & 0b11110000) >> 4;
                int low = (it & 0b1111);
                ret += high.ToString("x");
                ret += low.ToString("x");
            }
            return ret;
        }
    }
}
