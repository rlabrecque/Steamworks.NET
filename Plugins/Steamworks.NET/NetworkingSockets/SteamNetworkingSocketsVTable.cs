#define STEAMNETWORKINGSOCKETS_ENABLE_SDR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    [SuppressUnmanagedCodeSecurity]
    internal class SteamNetworkingSocketsVTable
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate HSteamListenSocket CreateListenSocketIPDelegate(IntPtr instance, in SteamNetworkingIPAddr localAddress);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate HSteamNetConnection ConnectByIPAddressDelegate(IntPtr instance, in SteamNetworkingIPAddr address);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate HSteamListenSocket CreateListenSocketP2PDelegate(IntPtr instance, int nVirtualPort);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate HSteamNetConnection ConnectP2PDelegate(IntPtr instance, in SteamNetworkingIdentity identityRemote, int nVirtualPort);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate EResult AcceptConnectionDelegate(IntPtr instance, HSteamNetConnection hConn);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool CloseConnectionDelegate(IntPtr instance, HSteamNetConnection hPeer, int nReason, InteropHelp.UTF8StringHandle pszDebug, bool bEnableLinger);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool CloseListenSocketDelegate(IntPtr instance, HSteamListenSocket hSocket);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool SetConnectionUserDataDelegate(IntPtr instance, HSteamNetConnection hPeer, Int64 nUserData);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate Int64 GetConnectionUserDataDelegate(IntPtr instance, HSteamNetConnection hPeer);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate void SetConnectionNameDelegate(IntPtr instance, HSteamNetConnection hPeer, InteropHelp.UTF8StringHandle pszName);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool GetConnectionNameDelegate(IntPtr instance, HSteamNetConnection hPeer, IntPtr pszName, int nMaxLen);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate EResult SendMessageToConnectionDelegate(IntPtr instance, HSteamNetConnection hConn, IntPtr pData, UInt32 cbData, int nSendFlags);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate EResult FlushMessagesOnConnectionDelegate(IntPtr instance, HSteamNetConnection hConn);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int ReceiveMessagesOnConnectionDelegate(IntPtr instance, HSteamNetConnection hConn, IntPtr ppOutMessages, int nMaxMessages);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int ReceiveMessagesOnListenSocketDelegate(IntPtr instance, HSteamListenSocket hSocket, IntPtr ppOutMessages, int nMaxMessages);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool GetConnectionInfoDelegate(IntPtr instance, HSteamNetConnection hConn, out SteamNetConnectionInfo_t pInfo);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool GetQuickConnectionStatusDelegate(IntPtr instance, HSteamNetConnection hConn, out SteamNetworkingQuickConnectionStatus pStats);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int GetDetailedConnectionStatusDelegate(IntPtr instance, HSteamNetConnection hConn, IntPtr pszBuf, int cbBuf);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool GetListenSocketAddressDelegate(IntPtr instance, HSteamListenSocket hSocket, out SteamNetworkingIPAddr address);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool CreateSocketPairDelegate(IntPtr instance, out HSteamNetConnection pOutConnection1, out HSteamNetConnection pOutConnection2, bool bUseNetworkLoopback, in SteamNetworkingIdentity pIdentity1, in SteamNetworkingIdentity pIdentity2);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool GetIdentityDelegate(IntPtr instance, out SteamNetworkingIdentity pIdentity);

        public readonly CreateListenSocketIPDelegate CreateListenSocketIP;
        public readonly ConnectByIPAddressDelegate ConnectByIPAddress;
        public readonly CreateListenSocketP2PDelegate CreateListenSocketP2P;
        public readonly ConnectP2PDelegate ConnectP2P;
        public readonly AcceptConnectionDelegate AcceptConnection;
        public readonly CloseConnectionDelegate CloseConnection;
        public readonly CloseListenSocketDelegate CloseListenSocket;
        public readonly SetConnectionUserDataDelegate SetConnectionUserData;
        public readonly GetConnectionUserDataDelegate GetConnectionUserData;
        public readonly SetConnectionNameDelegate SetConnectionName;
        public readonly GetConnectionNameDelegate GetConnectionName;
        public readonly SendMessageToConnectionDelegate SendMessageToConnection;
        public readonly FlushMessagesOnConnectionDelegate FlushMessagesOnConnection;
        public readonly ReceiveMessagesOnConnectionDelegate ReceiveMessagesOnConnection;
        public readonly ReceiveMessagesOnListenSocketDelegate ReceiveMessagesOnListenSocket;
        public readonly GetConnectionInfoDelegate GetConnectionInfo;
        public readonly GetQuickConnectionStatusDelegate GetQuickConnectionStatus;
        public readonly GetDetailedConnectionStatusDelegate GetDetailedConnectionStatus;
        public readonly GetListenSocketAddressDelegate GetListenSocketAddress;
        public readonly CreateSocketPairDelegate CreateSocketPair;
        public readonly GetIdentityDelegate GetIdentity;
        //public readonly IntPtr ReceivedRelayAuthTicket;
        //public readonly IntPtr FindRelayAuthTicketForServer;
        //public readonly IntPtr ConnectToHostedDedicatedServer;
        //public readonly IntPtr GetHostedDedicatedServerPort;
        //public readonly IntPtr GetHostedDedicatedServerPOPID;
        //public readonly IntPtr GetHostedDedicatedServerAddress;
        //public readonly IntPtr CreateHostedDedicatedServerListenSocket;

        public SteamNetworkingSocketsVTable(IntPtr nativeVTablePtr)
        {
            int methodIndex = 0;
            Add(nativeVTablePtr, ref methodIndex, out CreateListenSocketIP);
            Add(nativeVTablePtr, ref methodIndex, out ConnectByIPAddress);
#if STEAMNETWORKINGSOCKETS_ENABLE_SDR
            Add(nativeVTablePtr, ref methodIndex, out CreateListenSocketP2P);
            Add(nativeVTablePtr, ref methodIndex, out ConnectP2P);
#endif
            Add(nativeVTablePtr, ref methodIndex, out AcceptConnection);
            Add(nativeVTablePtr, ref methodIndex, out CloseConnection);
            Add(nativeVTablePtr, ref methodIndex, out CloseListenSocket);
            Add(nativeVTablePtr, ref methodIndex, out SetConnectionUserData);
            Add(nativeVTablePtr, ref methodIndex, out GetConnectionUserData);
            Add(nativeVTablePtr, ref methodIndex, out SetConnectionName);
            Add(nativeVTablePtr, ref methodIndex, out GetConnectionName);
            Add(nativeVTablePtr, ref methodIndex, out SendMessageToConnection);
            Add(nativeVTablePtr, ref methodIndex, out FlushMessagesOnConnection);
            Add(nativeVTablePtr, ref methodIndex, out ReceiveMessagesOnConnection);
            Add(nativeVTablePtr, ref methodIndex, out ReceiveMessagesOnListenSocket);
            Add(nativeVTablePtr, ref methodIndex, out GetConnectionInfo);
            Add(nativeVTablePtr, ref methodIndex, out GetQuickConnectionStatus);
            Add(nativeVTablePtr, ref methodIndex, out GetDetailedConnectionStatus);
            Add(nativeVTablePtr, ref methodIndex, out GetListenSocketAddress);
            Add(nativeVTablePtr, ref methodIndex, out CreateSocketPair);
            Add(nativeVTablePtr, ref methodIndex, out GetIdentity);
            //Add(nativeVTablePtr, ref methodIndex, out ReceivedRelayAuthTicket);
            //Add(nativeVTablePtr, ref methodIndex, out FindRelayAuthTicketForServer);
            //Add(nativeVTablePtr, ref methodIndex, out ConnectToHostedDedicatedServer);
            //Add(nativeVTablePtr, ref methodIndex, out GetHostedDedicatedServerPort);
            //Add(nativeVTablePtr, ref methodIndex, out GetHostedDedicatedServerPOPID);
            //Add(nativeVTablePtr, ref methodIndex, out GetHostedDedicatedServerAddress);
            //Add(nativeVTablePtr, ref methodIndex, out CreateHostedDedicatedServerListenSocket);
        }

        static void Add<T>(IntPtr nativeVTablePtr, ref int methodIndex, out T delegateVariable)
            where T : Delegate
        {
            IntPtr methodPtr = Marshal.ReadIntPtr(nativeVTablePtr, IntPtr.Size * methodIndex);
            delegateVariable = (T)Marshal.GetDelegateForFunctionPointer(methodPtr, typeof(T));
            methodIndex++;
        }
    }
}