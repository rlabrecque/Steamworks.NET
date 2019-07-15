using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    /// Object that describes a "location" on the Internet with sufficient
    /// detail that we can reasonably estimate an upper bound on the ping between
    /// the two hosts, even if a direct route between the hosts is not possible,
    /// and the connection must be routed through the Steam Datagram Relay network.
    /// This does not contain any information that identifies the host.  Indeed,
    /// if two hosts are in the same building or otherwise have nearly identical
    /// networking characteristics, then it's valid to use the same location
    /// object for both of them.
    ///
    /// NOTE: This object should only be used in the same process!  Do not serialize it,
    /// send it over the wire, or persist it in a file or database!  If you need
    /// to do that, convert it to a string representation using the methods in
    /// ISteamNetworkingUtils().
    [StructLayout(LayoutKind.Sequential, Size = 512)]
    public struct SteamNetworkPingLocation_t
    {
    };
}
