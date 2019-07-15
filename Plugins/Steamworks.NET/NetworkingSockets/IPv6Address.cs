using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IPv6Address : IEquatable<IPv6Address>
    {
        // Binary section
        [FieldOffset(0)]
        private ulong m_binaryLow;

        [FieldOffset(8)]
        private ulong m_binaryHigh;

        // IPv4 mapped into IPv6 section
        [FieldOffset(0)]
        private ulong m_ipv4_00000000_00000000;

        [FieldOffset(8)]
        private ushort m_ipv4_0000;

        [FieldOffset(10)]
        private ushort m_ipv4_ffff;

        [FieldOffset(12)]
        private IPv4Address m_ipv4;

        // IPv6 section
        [FieldOffset(0)]
        public byte m_00;

        [FieldOffset(1)]
        public byte m_01;

        [FieldOffset(2)]
        public byte m_02;

        [FieldOffset(3)]
        public byte m_03;

        [FieldOffset(4)]
        public byte m_04;

        [FieldOffset(5)]
        public byte m_05;

        [FieldOffset(6)]
        public byte m_06;

        [FieldOffset(7)]
        public byte m_07;

        [FieldOffset(8)]
        public byte m_08;

        [FieldOffset(9)]
        public byte m_09;

        [FieldOffset(10)]
        public byte m_10;

        [FieldOffset(11)]
        public byte m_11;

        [FieldOffset(12)]
        public byte m_12;

        [FieldOffset(13)]
        public byte m_13;

        [FieldOffset(14)]
        public byte m_14;

        [FieldOffset(15)]
        public byte m_15;

        /// <summary>
        /// IPv6 any address, used for listening on all local interfaces.
        /// </summary>
        public static readonly IPv6Address Any = default;

        /// <summary>
        /// IPv6 localhost address ::1
        /// </summary>
        public static readonly IPv6Address Localhost = new IPv6Address { m_15 = 1 };

        /// <summary>
        /// IPv4 localhost 127.0.0.1 mapped into IPv6.
        /// </summary>
        public static readonly IPv6Address LocalhostIPv4 = new IPv6Address(IPv4Address.Localhost);

        /// <summary>
        /// Gets value indicating whether ip address is IPv4 mapped into IPv6.
        /// </summary>
        public bool IsIPv4
        {
            get { return m_ipv4_00000000_00000000 == 0 && m_ipv4_0000 == 0 && m_ipv4_ffff == 0xffff; }
        }

        /// <summary>
        /// Gets IPv4 address mapped into IPv6.
        /// Use <see cref="IsIPv4"/> to make sure it's valid IPv4 mapped into IPv6.
        /// </summary>
        public IPv4Address IPv4
        {
            get { return m_ipv4; }
        }

        /// <summary>
        /// Gets value indicating whether IP address is IPv4 or IPv6 localhost.
        /// </summary>
        public bool IsLocalhost
        {
            get { return this == Localhost || this == LocalhostIPv4; }
        }

        /// <summary>
        /// Initializes a new IPv4 address mapped into IPv6.
        /// </summary>
        public IPv6Address(IPv4Address ipv4)
            : this()
        {
            m_ipv4_00000000_00000000 = 0;
            m_ipv4_0000 = 0;
            m_ipv4_ffff = 0xffff;
            m_ipv4 = ipv4;
        }

        /// <summary>
        /// Initializes a new IPv6 address.
        /// </summary>
        public IPv6Address(byte m00, byte m01, byte m02, byte m03, byte m04, byte m05, byte m06, byte m07, byte m08, byte m09, byte m10, byte m11, byte m12, byte m13, byte m14, byte m15)
            : this()
        {
            m_00 = m00;
            m_01 = m01;
            m_02 = m02;
            m_03 = m03;
            m_04 = m04;
            m_05 = m05;
            m_06 = m06;
            m_07 = m07;
            m_08 = m08;
            m_09 = m09;
            m_10 = m10;
            m_11 = m11;
            m_12 = m12;
            m_13 = m13;
            m_14 = m14;
            m_15 = m15;
        }

        public override string ToString()
        {
            // TODO: Could be made faster
            return $"{m_00:X2}{m_01:X2}:{m_02:X2}{m_03:X2}:{m_04:X2}{m_05:X2}:{m_06:X2}{m_07:X2}:{m_08:X2}{m_09:X2}:{m_10:X2}{m_11:X2}:{m_12:X2}{m_13:X2}:{m_14:X2}{m_15:X2}";
        }

        public bool Equals(IPv6Address other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is IPv6Address && this == (IPv6Address)obj;
        }

        public override int GetHashCode()
        {
            return m_binaryLow.GetHashCode() ^ m_binaryHigh.GetHashCode();
        }

        public static bool operator ==(IPv6Address x, IPv6Address y)
        {
            return x.m_binaryLow == y.m_binaryLow && x.m_binaryHigh == y.m_binaryHigh;
        }

        public static bool operator !=(IPv6Address a, IPv6Address b)
        {
            return !(a == b);
        }
    }
}
