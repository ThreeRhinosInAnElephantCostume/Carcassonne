#if GODOT
using Godot;
#elif UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
using System;
using System.Runtime.InteropServices;

#if GODOT_REAL_T_IS_DOUBLE
using real_t = System.Double;
#else
using real_t = System.Single;
#endif

namespace ExtraMath
{
    /// <summary>
    /// 3-element structure that can be used to represent 3D grid coordinates or sets of integers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4I : IEquatable<Vector4I>
    {
        public enum Axis
        {
            X = 0,
            Y,
            Z,
            W
        }

        public int x;
        public int y;
        public int z;
        public int w;

        public Vector3I XYZ
        {
            get
            {
                return new Vector3I(x, y, z);
            }
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        return;
                    case 1:
                        y = value;
                        return;
                    case 2:
                        z = value;
                        return;
                    case 3:
                        w = value;
                        return;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Vector4I Abs()
        {
            return new Vector4I(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z), Mathf.Abs(w));
        }

        public int DistanceSquaredTo(Vector4I b)
        {
            return (b - this).LengthSquared();
        }

        public real_t DistanceTo(Vector4I b)
        {
            return (b - this).Length();
        }

        public int Dot(Vector4I b)
        {
            return x * b.x + y * b.y + z * b.z + w * b.w;
        }

        public real_t Length()
        {
            int x2 = x * x;
            int y2 = y * y;
            int z2 = z * z;
            int w2 = w * w;

            return Mathf.Sqrt(x2 + y2 + z2 + w2);
        }

        public int LengthSquared()
        {
            int x2 = x * x;
            int y2 = y * y;
            int z2 = z * z;
            int w2 = w * w;

            return x2 + y2 + z2 + w2;
        }

        public Axis MaxAxis()
        {
            byte index = 0;
            for (byte i = 1; i < 4; i++)
            {
                if (this[i] > this[index])
                {
                    index = i;
                }
            }
            return (Axis)index;
        }

        public Axis MinAxis()
        {
            byte index = 0;
            for (byte i = 1; i < 4; i++)
            {
                if (this[i] < this[index])
                {
                    index = i;
                }
            }
            return (Axis)index;
        }

#if GODOT
        public Vector4I PosMod(int mod)
        {
            Vector4I v = this;
            v.x = Mathf.PosMod(v.x, mod);
            v.y = Mathf.PosMod(v.y, mod);
            v.z = Mathf.PosMod(v.z, mod);
            v.w = Mathf.PosMod(v.w, mod);
            return v;
        }

        public Vector4I PosMod(Vector4I modv)
        {
            Vector4I v = this;
            v.x = Mathf.PosMod(v.x, modv.x);
            v.y = Mathf.PosMod(v.y, modv.y);
            v.z = Mathf.PosMod(v.z, modv.z);
            v.w = Mathf.PosMod(v.w, modv.w);
            return v;
        }
#endif

        public Vector4I Sign()
        {
            Vector4I v = this;
#if GODOT
            v.x = Mathf.Sign(v.x);
            v.y = Mathf.Sign(v.y);
            v.z = Mathf.Sign(v.z);
            v.w = Mathf.Sign(v.w);
#else
            v.x = v.x < 0 ? -1 : 1;
            v.y = v.y < 0 ? -1 : 1;
            v.z = v.z < 0 ? -1 : 1;
            v.w = v.w < 0 ? -1 : 1;
#endif
            return v;
        }

        public Vector2I[] UnpackVector2()
        {
            Vector2I[] arr = new Vector2I[2];
            arr[0] = new Vector2I(x, y);
            arr[1] = new Vector2I(z, w);
            return arr;
        }

        public void UnpackVector2(out Vector2I xy, out Vector2I zw)
        {
            xy = new Vector2I(x, y);
            zw = new Vector2I(z, w);
        }

        // Constants
        private static readonly Vector4I _zero = new Vector4I(0, 0, 0, 0);
        private static readonly Vector4I _one = new Vector4I(1, 1, 1, 1);
        private static readonly Vector4I _negOne = new Vector4I(-1, -1, -1, -1);

        private static readonly Vector4I _unitX = new Vector4I(1, 0, 0, 0);
        private static readonly Vector4I _unitY = new Vector4I(0, 1, 0, 0);
        private static readonly Vector4I _unitZ = new Vector4I(0, 0, 1, 0);
        private static readonly Vector4I _unitW = new Vector4I(0, 0, 0, 1);

        public static Vector4I Zero { get { return _zero; } }
        public static Vector4I One { get { return _one; } }
        public static Vector4I NegOne { get { return _negOne; } }

        public static Vector4I UnitX { get { return _unitX; } }
        public static Vector4I UnitY { get { return _unitY; } }
        public static Vector4I UnitZ { get { return _unitZ; } }
        public static Vector4I UnitW { get { return _unitW; } }

        // Constructors
        public Vector4I(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public Vector4I(Vector4I v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }
        public Vector4I(Vector4 v)
        {
            this.x = Mathf.RoundToInt(v.x);
            this.y = Mathf.RoundToInt(v.y);
            this.z = Mathf.RoundToInt(v.z);
            this.w = Mathf.RoundToInt(v.w);
        }
        public Vector4I(Vector3I xyz, int w)
        {
            x = xyz.x;
            y = xyz.y;
            z = xyz.z;
            this.w = w;
        }
        public Vector4I(Vector2I xy, Vector2I zw)
        {
            x = xy.x;
            y = xy.y;
            z = zw.x;
            w = zw.y;
        }

        public static implicit operator Vector4(Vector4I value)
        {
            return new Vector4(value.x, value.y, value.z, value.w);
        }

        public static explicit operator Vector4I(Vector4 value)
        {
            return new Vector4I(value);
        }

#if UNITY_5_3_OR_NEWER
        public static implicit operator UnityEngine.Vector4(Vector4I value)
        {
            return new UnityEngine.Vector4(value.x, value.y, value.z, value.w);
        }

        public static explicit operator Vector4I(UnityEngine.Vector4 value)
        {
            return new Vector4I((Vector4)value);
        }
#endif

        public static Vector4I operator +(Vector4I left, Vector4I right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            left.w += right.w;
            return left;
        }

        public static Vector4I operator -(Vector4I left, Vector4I right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            left.w -= right.w;
            return left;
        }

        public static Vector4I operator -(Vector4I vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;
            vec.w = -vec.w;
            return vec;
        }

        public static Vector4I operator *(Vector4I vec, int scale)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            vec.w *= scale;
            return vec;
        }

        public static Vector4I operator *(int scale, Vector4I vec)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            vec.w *= scale;
            return vec;
        }

        public static Vector4I operator *(Vector4I left, Vector4I right)
        {
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;
            left.w *= right.w;
            return left;
        }

        public static Vector4I operator /(Vector4I vec, int scale)
        {
            vec.x /= scale;
            vec.y /= scale;
            vec.z /= scale;
            vec.w /= scale;
            return vec;
        }

        public static Vector4I operator /(Vector4I left, Vector4I right)
        {
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;
            left.w /= right.w;
            return left;
        }

        public static Vector4I operator %(Vector4I vec, int divisor)
        {
            vec.x %= divisor;
            vec.y %= divisor;
            vec.z %= divisor;
            vec.w %= divisor;
            return vec;
        }

        public static Vector4I operator %(Vector4I vec, Vector4I divisorv)
        {
            vec.x %= divisorv.x;
            vec.y %= divisorv.y;
            vec.z %= divisorv.z;
            vec.w %= divisorv.w;
            return vec;
        }

        public static Vector4I operator &(Vector4I vec, int and)
        {
            vec.x &= and;
            vec.y &= and;
            vec.z &= and;
            vec.w &= and;
            return vec;
        }

        public static Vector4I operator &(Vector4I vec, Vector4I andv)
        {
            vec.x &= andv.x;
            vec.y &= andv.y;
            vec.z &= andv.z;
            vec.w &= andv.w;
            return vec;
        }

        public static bool operator ==(Vector4I left, Vector4I right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4I left, Vector4I right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Vector4I left, Vector4I right)
        {
            if (left.x == right.x)
            {
                if (left.y == right.y)
                {
                    if (left.z == right.z)
                    {
                        return left.w < right.w;
                    }
                    return left.z < right.z;
                }
                return left.y < right.y;
            }
            return left.x < right.x;
        }

        public static bool operator >(Vector4I left, Vector4I right)
        {
            if (left.x == right.x)
            {
                if (left.y == right.y)
                {
                    if (left.z == right.z)
                    {
                        return left.w > right.w;
                    }
                    return left.z > right.z;
                }
                return left.y > right.y;
            }
            return left.x > right.x;
        }

        public static bool operator <=(Vector4I left, Vector4I right)
        {
            if (left.x == right.x)
            {
                if (left.y == right.y)
                {
                    if (left.z == right.z)
                    {
                        return left.w <= right.w;
                    }
                    return left.z < right.z;
                }
                return left.y < right.y;
            }
            return left.x < right.x;
        }

        public static bool operator >=(Vector4I left, Vector4I right)
        {
            if (left.x == right.x)
            {
                if (left.y == right.y)
                {
                    if (left.z == right.z)
                    {
                        return left.w >= right.w;
                    }
                    return left.z > right.z;
                }
                return left.y > right.y;
            }
            return left.x > right.x;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4I)
            {
                return Equals((Vector4I)obj);
            }

            return false;
        }

        public bool Equals(Vector4I other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }

        public override int GetHashCode()
        {
            return y.GetHashCode() ^ x.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", new object[]
            {
                x.ToString(),
                y.ToString(),
                z.ToString(),
                w.ToString()
            });
        }

        public string ToString(string format)
        {
            return String.Format("({0}, {1}, {2}, {3})", new object[]
            {
                x.ToString(format),
                y.ToString(format),
                z.ToString(format),
                w.ToString(format)
            });
        }
    }
}
