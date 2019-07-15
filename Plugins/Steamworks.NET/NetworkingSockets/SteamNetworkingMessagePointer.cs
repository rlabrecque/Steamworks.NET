using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    public struct SteamNetworkingMessagePointer
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        delegate void ReleaseDelegate(IntPtr msgPtr);

        static int m_dataPtrOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_pData)).ToInt32();
        static int m_dataLenOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_cbSize)).ToInt32();
        static int m_connectionOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_conn)).ToInt32();
        static int m_senderOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_sender)).ToInt32();
        static int m_connectionUserDataOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_nConnUserData)).ToInt32();
        static int m_timeReceivedOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_usecTimeReceived)).ToInt32();
        static int m_messageNumberOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_nMessageNumber)).ToInt32();
        static int m_releaseFuncOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_pfnRelease)).ToInt32();
        static int m_channelOffset = Marshal.OffsetOf<SteamNetworkingMessage_t>(nameof(SteamNetworkingMessage_t.m_nChannel)).ToInt32();

        [ThreadStatic]
        static IntPtr m_cachePtr;

        [ThreadStatic]
        static ReleaseDelegate m_cacheDelegate;

        public readonly IntPtr Pointer;

        public IntPtr DataPointer
        {
            get { return Marshal.ReadIntPtr(Pointer, m_dataPtrOffset); }
        }

        public int DataLength
        {
            get { return Marshal.ReadInt32(Pointer, m_dataLenOffset); }
        }

        public HSteamNetConnection Connection
        {
            get { return new HSteamNetConnection((uint)Marshal.ReadInt32(Pointer, m_connectionOffset)); }
        }

        public SteamNetworkingIdentity Sender
        {
            // Too complicated without pointers
            get { return GetMessage().m_sender; }
        }

        public long ConnectionUserData
        {
            get { return Marshal.ReadInt64(Pointer, m_connectionUserDataOffset); }
        }

        public SteamNetworkingMicroseconds TimeReceived
        {
            get { return new SteamNetworkingMicroseconds(Marshal.ReadInt64(Pointer, m_timeReceivedOffset)); }
        }

        public long MessageNumber
        {
            get { return Marshal.ReadInt64(Pointer, m_messageNumberOffset); }
        }

        public int Channel
        {
            get { return Marshal.ReadInt32(Pointer, m_channelOffset); }
        }

        public SteamNetworkingMessagePointer(IntPtr nativePointer)
        {
            Pointer = nativePointer;
        }

        public void Release()
        {
            Release(Pointer);
        }

        public void GetMessage(out SteamNetworkingMessage_t msg)
        {
#if UNSAFE
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.CopyPtrToStructure((void*)Pointer, out SteamNetworkingMessage_t msg);
#else
            msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(Pointer);
#endif
        }

        public SteamNetworkingMessage_t GetMessage()
        {
            GetMessage(out var msg);
            return msg;
        }

        [SuppressUnmanagedCodeSecurity]
        static void Release(IntPtr messagePointer)
        {
            IntPtr releaseFuncPtr = Marshal.ReadIntPtr(messagePointer, m_releaseFuncOffset);

            // Null release function, do nothing
            if (releaseFuncPtr == IntPtr.Zero)
                return;

            // Release function does not change for lifetime of application in most cases
            if (releaseFuncPtr != m_cachePtr)
            {
                m_cacheDelegate = Marshal.GetDelegateForFunctionPointer<ReleaseDelegate>(releaseFuncPtr);
                m_cachePtr = releaseFuncPtr;
            }
            m_cacheDelegate(messagePointer);
        }
    }
}
