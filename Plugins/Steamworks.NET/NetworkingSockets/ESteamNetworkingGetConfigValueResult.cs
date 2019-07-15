using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    public enum ESteamNetworkingGetConfigValueResult
    {
        k_ESteamNetworkingGetConfigValue_BadValue = -1, // No such configuration value
        k_ESteamNetworkingGetConfigValue_BadScopeObj = -2,  // Bad connection handle, etc
        k_ESteamNetworkingGetConfigValue_BufferTooSmall = -3, // Couldn't fit the result in your buffer
        k_ESteamNetworkingGetConfigValue_OK = 1,
        k_ESteamNetworkingGetConfigValue_OKInherited = 2, // A value was not set at this level, but the effective (inherited) value was returned.

        k_ESteamNetworkingGetConfigValueResult__Force32Bit = 0x7fffffff
    };
}
