using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    public static class SteamNetworkingSocketsExtensions
    {
        /// Fetch the next available message(s) from the connection, if any.
        /// Returns the number of messages returned into your array, up to nMaxMessages.
        /// If the connection handle is invalid, -1 is returned.
        ///
        /// The order of the messages returned in the array is relevant.
        /// Reliable messages will be received in the order they were sent (and with the
        /// same sizes --- see SendMessageToConnection for on this subtle difference from a stream socket).
        ///
        /// Unreliable messages may be dropped, or delivered out of order withrespect to
        /// each other or with respect to reliable messages.  The same unreliable message
        /// may be received multiple times.
        ///
        /// If any messages are returned, you MUST call SteamNetworkingMessage_t::Release() on each
        /// of them free up resources after you are done.  It is safe to keep the object alive for
        /// a little while (put it into some queue, etc), and you may call Release() from any thread.
        public static int ReceiveMessagesOnConnection(this SteamNetworkingSockets sockets, HSteamNetConnection hConn, SteamNetworkingMessagePointer[] result)
        {
            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                return sockets.ReceiveMessagesOnConnection(hConn, handle.AddrOfPinnedObject(), result.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        /// Same as ReceiveMessagesOnConnection, but will return the next message available
        /// on any connection that was accepted through the specified listen socket.  Examine
        /// SteamNetworkingMessage_t::m_conn to know which client connection.
        ///
        /// Delivery order of messages among different clients is not defined.  They may
        /// be returned in an order different from what they were actually received.  (Delivery
        /// order of messages from the same client is well defined, and thus the order of the
        /// messages is relevant!)
        public static int ReceiveMessagesOnListenSocket(this SteamNetworkingSockets sockets, HSteamListenSocket hSocket, SteamNetworkingMessagePointer[] result)
        {
            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                return sockets.ReceiveMessagesOnListenSocket(hSocket, handle.AddrOfPinnedObject(), result.Length);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
