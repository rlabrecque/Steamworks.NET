#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System;
using System.Runtime.InteropServices;
using System.Security;
using Unity.Collections;
using IntPtr = System.IntPtr;

namespace Steamworks
{
    public struct SteamNetworkingUtils
    {
        //void (*FSteamNetworkingSocketsDebugOutput)( ESteamNetworkingSocketsDebugOutputType nType, const char *pszMsg )
        [UnmanagedFunctionPointer(CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        public delegate void FSteamNetworkingSocketsDebugOutput(ESteamNetworkingSocketsDebugOutputType nType, IntPtr pszMsg);

        private readonly IntPtr m_ptr;
        private readonly SteamNetworkingUtilsVTable m_vtable;

        const string STEAMNETWORKINGUTILS_INTERFACE_VERSION = "SteamNetworkingUtils001";

        static SteamNetworkingUtils m_user;
        static SteamNetworkingUtils m_gameServer;

        public static SteamNetworkingUtils UserInstance
        {
            get
            {
                if (m_user.m_ptr == IntPtr.Zero)
                {
                    m_user = new SteamNetworkingUtils(InterfaceHelper.FindOrCreateUserInterface(STEAMNETWORKINGUTILS_INTERFACE_VERSION));
                }
                return m_user;
            }
        }

        public static SteamNetworkingUtils GameServerInstance
        {
            get
            {
                if (m_gameServer.m_ptr == IntPtr.Zero)
                {
                    m_gameServer = new SteamNetworkingUtils(InterfaceHelper.FindOrCreateGameServerInterface(STEAMNETWORKINGUTILS_INTERFACE_VERSION));
                }
                return m_gameServer;
            }
        }

        private SteamNetworkingUtils(IntPtr ptr)
        {
            m_ptr = ptr;
            m_vtable = new SteamNetworkingUtilsVTable(Marshal.ReadIntPtr(m_ptr, 0));
        }

        public bool InitializeRelayNetworkAccess()
        {
            // Inline function in SteamSDK
            return CheckPingDataUpToDate(1e10f);
        }

        public bool CheckPingDataUpToDate(float flMaxAgeSeconds)
        {
            return m_vtable.CheckPingDataUpToDate(m_ptr, flMaxAgeSeconds);
        }

        public SteamNetworkingMicroseconds GetLocalTimestamp()
        {
            return m_vtable.GetLocalTimestamp(m_ptr);
        }
    }
}

#endif // !DISABLESTEAMWORKS
