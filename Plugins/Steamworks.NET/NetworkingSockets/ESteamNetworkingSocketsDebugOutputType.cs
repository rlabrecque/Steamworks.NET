using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    public enum ESteamNetworkingSocketsDebugOutputType
    {
        k_ESteamNetworkingSocketsDebugOutputType_None = 0,
        k_ESteamNetworkingSocketsDebugOutputType_Bug = 1, // You used the API incorrectly, or an internal error happened
        k_ESteamNetworkingSocketsDebugOutputType_Error = 2, // Run-time error condition that isn't the result of a bug.  (E.g. we are offline, cannot bind a port, etc)
        k_ESteamNetworkingSocketsDebugOutputType_Important = 3, // Nothing is wrong, but this is an important notification
        k_ESteamNetworkingSocketsDebugOutputType_Warning = 4,
        k_ESteamNetworkingSocketsDebugOutputType_Msg = 5, // Recommended amount
        k_ESteamNetworkingSocketsDebugOutputType_Verbose = 6, // Quite a bit
        k_ESteamNetworkingSocketsDebugOutputType_Debug = 7, // Practically everything
        k_ESteamNetworkingSocketsDebugOutputType_Everything = 8, // Wall of text, detailed packet contents breakdown, etc

        k_ESteamNetworkingSocketsDebugOutputType__Force32Bit = 0x7fffffff
    };
}
