using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    static class InterfaceHelper
    {
        [DllImport(NativeMethods.NativeLibraryName, EntryPoint = "SteamInternal_FindOrCreateUserInterface", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr SteamInternal_FindOrCreateUserInterface(HSteamUser hSteamUser, InteropHelp.UTF8StringHandle pszVersion);

        [DllImport(NativeMethods.NativeLibraryName, EntryPoint = "SteamInternal_FindOrCreateGameServerInterface", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr SteamInternal_FindOrCreateGameServerInterface(HSteamUser hSteamUser, InteropHelp.UTF8StringHandle pszVersion);

        public static IntPtr FindOrCreateUserInterface(HSteamUser hSteamUser, string interfaceVersion)
        {
            using (var pszVersion = new InteropHelp.UTF8StringHandle(interfaceVersion))
            {
                return SteamInternal_FindOrCreateUserInterface(hSteamUser, pszVersion);
            }
        }

        public static IntPtr FindOrCreateGameServerInterface(HSteamUser hSteamUser, string interfaceVersion)
        {
            using (var pszVersion = new InteropHelp.UTF8StringHandle(interfaceVersion))
            {
                return SteamInternal_FindOrCreateGameServerInterface(hSteamUser, pszVersion);
            }
        }

        public static IntPtr FindOrCreateUserInterface(string interfaceVersion)
        {
            using (var pszVersion = new InteropHelp.UTF8StringHandle(interfaceVersion))
            {
                return SteamInternal_FindOrCreateUserInterface((HSteamUser)NativeMethods.SteamAPI_GetHSteamUser(), pszVersion);
            }
        }

        public static IntPtr FindOrCreateGameServerInterface(string interfaceVersion)
        {
            using (var pszVersion = new InteropHelp.UTF8StringHandle(interfaceVersion))
            {
                return SteamInternal_FindOrCreateGameServerInterface((HSteamUser)NativeMethods.SteamGameServer_GetHSteamUser(), pszVersion);
            }
        }
    }
}
