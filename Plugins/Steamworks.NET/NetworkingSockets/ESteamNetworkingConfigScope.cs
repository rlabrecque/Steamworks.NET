using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    /// Configuration values can be applied to different types of objects.
    public enum ESteamNetworkingConfigScope
    {

        /// Get/set global option, or defaults.  Even options that apply to more specific scopes
        /// have global scope, and you may be able to just change the global defaults.  If you
        /// need different settings per connection (for example), then you will need to set those
        /// options at the more specific scope.
        k_ESteamNetworkingConfig_Global = 1,

        /// Some options are specific to a particular interface.  Note that all connection
        /// and listen socket settings can also be set at the interface level, and they will
        /// apply to objects created through those interfaces.
        k_ESteamNetworkingConfig_SocketsInterface = 2,

        /// Options for a listen socket.  Listen socket options can be set at the interface layer,
        /// if  you have multiple listen sockets and they all use the same options.
        /// You can also set connection options on a listen socket, and they set the defaults
        /// for all connections accepted through this listen socket.  (They will be used if you don't
        /// set a connection option.)
        k_ESteamNetworkingConfig_ListenSocket = 3,

        /// Options for a specific connection.
        k_ESteamNetworkingConfig_Connection = 4,

        k_ESteamNetworkingConfigScope__Force32Bit = 0x7fffffff
    };
}
