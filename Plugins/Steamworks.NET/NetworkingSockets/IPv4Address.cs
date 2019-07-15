using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IPv4Address : IEquatable<IPv4Address>
    {
        /// <summary>
        /// IPv4 any address, used for listening on all local interfaces.
        /// </summary>
        public static readonly IPv4Address Any = default;

        /// <summary>
        /// IPv4 localhost address 127.0.0.1
        /// </summary>
        public static readonly IPv4Address Localhost = new IPv4Address(127, 0, 0, 1);

        [FieldOffset(0)]
        private uint m_binary;

        [FieldOffset(0)]
        public byte A;

        [FieldOffset(1)]
        public byte B;

        [FieldOffset(2)]
        public byte C;

        [FieldOffset(3)]
        public byte D;

        /// <summary>
        /// Gets or sets value in host byte order.
        /// </summary>
        public uint Value
        {
            get { return (uint)((A << 24) | (B << 16) | (C << 8) | D); }
            set { this = new IPv4Address(value); }
        }

        /// <summary>
        /// Creates new IP Address in host byte order.
        /// </summary>
        public IPv4Address(uint value)
            : this()
        {
            A = (byte)(value >> 24);
            B = (byte)(value >> 16);
            C = (byte)(value >> 8);
            D = (byte)(value);
        }

        /// <summary>
        /// Creates new IP Address A.B.C.D
        /// </summary>
        public IPv4Address(byte a, byte b, byte c, byte d)
            : this()
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public override string ToString()
        {
            return $"{A}.{B}.{C}.{D}";
        }

        public bool Equals(IPv4Address other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is IPv4Address && this == (IPv4Address)obj;
        }

        public override int GetHashCode()
        {
            return (int)m_binary;
        }

        public static bool operator ==(IPv4Address x, IPv4Address y)
        {
            return x.m_binary == y.m_binary;
        }

        public static bool operator !=(IPv4Address a, IPv4Address b)
        {
            return !(a == b);
        }
    }
}
