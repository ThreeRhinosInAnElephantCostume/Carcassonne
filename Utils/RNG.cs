
/* RNG.cs

An abstract Random Number Generator interface, batteries included.

*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using static System.Math;

public static partial class Utils
{
    public class RNG
    {
        public abstract class RNGGenerator
        {
            public abstract unsafe void GetBytes(byte* buf, int n);
            public virtual unsafe ulong GetLong()
            {
                ulong ret = 0;
                GetBytes((byte*)&ret, 8);
                return ret;
            }
        }
        public class SystemRandom : RNGGenerator
        {
            Random rng;
            ulong seed;
            public override unsafe void GetBytes(byte* buf, int n)
            {
                byte[] bts = new byte[n];
                rng.NextBytes(bts);
                fixed (byte* btsp = bts)
                {
                    int i = 0;
                    while (i < bts.Length)
                    {
                        buf[i] = btsp[i];
                        i++;
                    }
                }
            }
            public override unsafe ulong GetLong()
            {
                return (((ulong)rng.Next()) << 32) + ((ulong)rng.Next());
            }
            void init()
            {
                rng = new Random((int)((seed << 32) >> 32));
            }
            public SystemRandom()
            {
                byte[] bytes = new byte[8];
                new Random().NextBytes(bytes);
                this.seed = BitConverter.ToUInt64(bytes, 0);
                init();
            }
            public SystemRandom(ulong seed)
            {
                this.seed = seed;
                init();
            }
        }
        public class Xorshift64s : RNGGenerator
        {
            ulong seed;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe ulong Generate()
            {
                ulong x = seed;
                x ^= x >> 12;
                x ^= x << 25;
                x ^= x >> 27;
                seed = x;
                return x * (0x2545F4914F6CDD1Dul);
            }
            public unsafe override void GetBytes(byte* buf, int n)
            {
                int i = 0;
                int e = n - (n % 8);
                while (i < e)
                {
                    *((ulong*)buf) = Generate();
                    buf += 8;
                    i += 8;
                }
                if (i < n)
                {
                    ulong v = Generate();
                    byte* dt = (byte*)&v;
                    while (i < n)
                    {
                        *buf = *dt;
                        dt++;
                        buf++;
                        i++;
                    }
                }
            }
            public override unsafe ulong GetLong()
            {
                return Generate();
            }
            public Xorshift64s(ulong seed)
            {
                Assert(seed != 0);
                this.seed = seed;
            }
        }
        RNGGenerator rng;
        //Random rng1;
        public double NextDouble()
        {
            const double b = 1 / (uint.MaxValue + 1.0);
            double v;
            uint u = (uint)rng.GetLong();
            v = u * b;
            return v;
        }
        public float NextFloat()
        {
            return (float)NextDouble();
        }
        public double NextDouble(double min, double max)
        {
            double dif = max - min;
            return min + (dif * NextDouble());
        }
        public unsafe long NextLong()
        {
            return (long)rng.GetLong();
        }
        public unsafe void NextBytes(byte[] buffer)
        {
            fixed (byte* buf = buffer)
            {
                rng.GetBytes(buf, buffer.Length);
            }
        }
        public unsafe byte[] NextBytes(int len)
        {
            byte[] ret = new byte[len];
            fixed (byte* buf = ret)
            {
                rng.GetBytes(buf, ret.Length);
            }
            return ret;
        }
        public long NextLong(long min, long max)
        {
            long dif = max - min;
            return min + AbsMod(NextLong(), dif);
        }
        public unsafe ulong NextULong()
        {
            return rng.GetLong();
        }
        public ulong NextULong(ulong min, ulong max)
        {
            ulong dif = max - min;
            return min + NextULong() % dif;
        }
        public RNG(ulong seed)
        {
            rng = new Xorshift64s(seed);
        }
        public RNG() : this((ulong)DateTime.Now.Ticks) { }
        public RNG(RNGGenerator rng)
        {
            this.rng = rng;
        }
        public RNG(Type tp, ulong seed)
        {
            Activator.CreateInstance(tp, args: new object[] { seed });
        }
        public RNG(Type tp) : this(tp, (ulong)DateTime.Now.Ticks) { }
    }
    public class Span
    {
        public double min, max;
        public double GetRandom(RNG rng)
        {
            return rng.NextDouble(min, max);
        }
        public double GetRandom(ulong seed)
        {
            return GetRandom(new RNG(seed));
        }
        public Span(double max)
        {
            this.min = 0;
            this.max = max;
        }
        public Span(double min, double max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public delegate T ElementGeneratorSimple<T>();
    public delegate T ElementGenerator<T>(int indx);
    static public List<T> ListGenerator<T>(ElementGenerator<T> generator, int n)
    {
        List<T> ret = new List<T>(n);
        int i = 0;
        while (i < n)
        {
            ret.Add(generator(i));
            i++;
        }
        return ret;
    }
    static public List<T> ListGenerator<T>(ElementGeneratorSimple<T> generator, int n)
    {
        List<T> ret = new List<T>(n);
        int i = 0;
        while (i < n)
        {
            ret.Add(generator());
            i++;
        }
        return ret;
    }
}
