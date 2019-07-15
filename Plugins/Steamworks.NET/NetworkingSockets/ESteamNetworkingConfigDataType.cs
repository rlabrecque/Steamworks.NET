using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    // Different configuration values have different data types
    public enum ESteamNetworkingConfigDataType
    {
        k_ESteamNetworkingConfig_Int32 = 1,
        k_ESteamNetworkingConfig_Int64 = 2,
        k_ESteamNetworkingConfig_Float = 3,
        k_ESteamNetworkingConfig_String = 4,
        k_ESteamNetworkingConfig_FunctionPtr = 5, // NOTE: When setting	callbacks, you should put the pointer into a variable and pass a pointer to that variable.

        k_ESteamNetworkingConfigDataType__Force32Bit = 0x7fffffff
    };
}
