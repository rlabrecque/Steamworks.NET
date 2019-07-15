using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    public enum ESteamNetworkingConfigValue
    {
        k_ESteamNetworkingConfig_Invalid = 0,

        /// [global float, 0--100] Randomly discard N pct of packets instead of sending/recv
        /// This is a global option only, since it is applied at a low level
        /// where we don't have much context
        k_ESteamNetworkingConfig_FakePacketLoss_Send = 2,
        k_ESteamNetworkingConfig_FakePacketLoss_Recv = 3,

        /// [global int32].  Delay all outbound/inbound packets by N ms
        k_ESteamNetworkingConfig_FakePacketLag_Send = 4,
        k_ESteamNetworkingConfig_FakePacketLag_Recv = 5,

        /// [global float] 0-100 Percentage of packets we will add additional delay
        /// to (causing them to be reordered)
        k_ESteamNetworkingConfig_FakePacketReorder_Send = 6,
        k_ESteamNetworkingConfig_FakePacketReorder_Recv = 7,

        /// [global int32] Extra delay, in ms, to apply to reordered packets.
        k_ESteamNetworkingConfig_FakePacketReorder_Time = 8,

        /// [global float 0--100] Globally duplicate some percentage of packets we send
        k_ESteamNetworkingConfig_FakePacketDup_Send = 26,
        k_ESteamNetworkingConfig_FakePacketDup_Recv = 27,

        /// [global int32] Amount of delay, in ms, to delay duplicated packets.
        /// (We chose a random delay between 0 and this value)
        k_ESteamNetworkingConfig_FakePacketDup_TimeMax = 28,

        /// [connection int32] Timeout value (in ms) to use when first connecting
        k_ESteamNetworkingConfig_TimeoutInitial = 24,

        /// [connection int32] Timeout value (in ms) to use after connection is established
        k_ESteamNetworkingConfig_TimeoutConnected = 25,

        /// [connection int32] Upper limit of buffered pending bytes to be sent,
        /// if this is reached SendMessage will return k_EResultLimitExceeded
        /// Default is 512k (524288 bytes)
        k_ESteamNetworkingConfig_SendBufferSize = 9,

        /// [connection int32] Minimum/maximum send rate clamp, 0 is no limit.
        /// This value will control the min/max allowed sending rate that 
        /// bandwidth estimation is allowed to reach.  Default is 0 (no-limit)
        k_ESteamNetworkingConfig_SendRateMin = 10,
        k_ESteamNetworkingConfig_SendRateMax = 11,

        /// [connection int32] Nagle time, in microseconds.  When SendMessage is called, if
        /// the outgoing message is less than the size of the MTU, it will be
        /// queued for a delay equal to the Nagle timer value.  This is to ensure
        /// that if the application sends several small messages rapidly, they are
        /// coalesced into a single packet.
        /// See historical RFC 896.  Value is in microseconds. 
        /// Default is 5000us (5ms).
        k_ESteamNetworkingConfig_NagleTime = 12,

        /// [connection int32] Don't automatically fail IP connections that don't have
        /// strong auth.  On clients, this means we will attempt the connection even if
        /// we don't know our identity or can't get a cert.  On the server, it means that
        /// we won't automatically reject a connection due to a failure to authenticate.
        /// (You can examine the incoming connection and decide whether to accept it.)
        k_ESteamNetworkingConfig_IP_AllowWithoutAuth = 23,

        //
        // Settings for SDR relayed connections
        //

        /// [int32 global] If the first N pings to a port all fail, mark that port as unavailable for
        /// a while, and try a different one.  Some ISPs and routers may drop the first
        /// packet, so setting this to 1 may greatly disrupt communications.
        k_ESteamNetworkingConfig_SDRClient_ConsecutitivePingTimeoutsFailInitial = 19,

        /// [int32 global] If N consecutive pings to a port fail, after having received successful 
        /// communication, mark that port as unavailable for a while, and try a 
        /// different one.
        k_ESteamNetworkingConfig_SDRClient_ConsecutitivePingTimeoutsFail = 20,

        /// [int32 global] Minimum number of lifetime pings we need to send, before we think our estimate
        /// is solid.  The first ping to each cluster is very often delayed because of NAT,
        /// routers not having the best route, etc.  Until we've sent a sufficient number
        /// of pings, our estimate is often inaccurate.  Keep pinging until we get this
        /// many pings.
        k_ESteamNetworkingConfig_SDRClient_MinPingsBeforePingAccurate = 21,

        /// [int32 global] Set all steam datagram traffic to originate from the same
        /// local port. By default, we open up a new UDP socket (on a different local
        /// port) for each relay.  This is slightly less optimal, but it works around
        /// some routers that don't implement NAT properly.  If you have intermittent
        /// problems talking to relays that might be NAT related, try toggling
        /// this flag
        k_ESteamNetworkingConfig_SDRClient_SingleSocket = 22,

        /// [global string] Code of relay cluster to force use.  If not empty, we will
        /// only use relays in that cluster.  E.g. 'iad'
        k_ESteamNetworkingConfig_SDRClient_ForceRelayCluster = 29,

        /// [connection string] For debugging, generate our own (unsigned) ticket, using
        /// the specified  gameserver address.  Router must be configured to accept unsigned
        /// tickets.
        k_ESteamNetworkingConfig_SDRClient_DebugTicketAddress = 30,

        /// [global string] For debugging.  Override list of relays from the config with
        /// this set (maybe just one).  Comma-separated list.
        k_ESteamNetworkingConfig_SDRClient_ForceProxyAddr = 31,

        //
        // Log levels for debuging information.  A higher priority
        // (lower numeric value) will cause more stuff to be printed.  
        //
        k_ESteamNetworkingConfig_LogLevel_AckRTT = 13, // [connection int32] RTT calculations for inline pings and replies
        k_ESteamNetworkingConfig_LogLevel_PacketDecode = 14, // [connection int32] log SNP packets send
        k_ESteamNetworkingConfig_LogLevel_Message = 15, // [connection int32] log each message send/recv
        k_ESteamNetworkingConfig_LogLevel_PacketGaps = 16, // [connection int32] dropped packets
        k_ESteamNetworkingConfig_LogLevel_P2PRendezvous = 17, // [connection int32] P2P rendezvous messages
        k_ESteamNetworkingConfig_LogLevel_SDRRelayPings = 18, // [global int32] Ping relays

        k_ESteamNetworkingConfigValue__Force32Bit = 0x7fffffff
    };
}
