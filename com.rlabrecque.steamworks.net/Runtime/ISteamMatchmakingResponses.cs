// IL2CPP-safe Steamworks.NET matchmaking callbacks
// Adjusted for use with IL2CPP delegate marshaling
// Delegates are static, decorated with MonoPInvokeCallback
// Instance methods are mapped via a static dictionary

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace Steamworks {

#region ISteamMatchmakingServerListResponse

    public class ISteamMatchmakingServerListResponse {
        public delegate void ServerResponded(HServerListRequest hRequest, int iServer);
        public delegate void ServerFailedToRespond(HServerListRequest hRequest, int iServer);
        public delegate void RefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response);

        private static readonly Dictionary<IntPtr, ISteamMatchmakingServerListResponse> _instances = new();

        private VTable m_VTable;
        private IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private IntPtr m_instancePtr;

        private ServerResponded m_ServerResponded;
        private ServerFailedToRespond m_ServerFailedToRespond;
        private RefreshComplete m_RefreshComplete;

        public ISteamMatchmakingServerListResponse(ServerResponded onServerResponded, ServerFailedToRespond onServerFailedToRespond, RefreshComplete onRefreshComplete) {
            m_ServerResponded = onServerResponded ?? throw new ArgumentNullException();
            m_ServerFailedToRespond = onServerFailedToRespond ?? throw new ArgumentNullException();
            m_RefreshComplete = onRefreshComplete ?? throw new ArgumentNullException();

            m_VTable = new VTable {
                m_VTServerResponded = InternalOnServerResponded,
                m_VTServerFailedToRespond = InternalOnServerFailedToRespond,
                m_VTRefreshComplete = InternalOnRefreshComplete
            };

            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);
            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
            m_instancePtr = m_pGCHandle.AddrOfPinnedObject();

            lock (_instances) _instances[m_instancePtr] = this;
        }

        ~ISteamMatchmakingServerListResponse() {
            lock (_instances) _instances.Remove(m_instancePtr);
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);
            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerResponded(IntPtr thisptr, HServerListRequest hRequest, int iServer);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerFailedToRespond(IntPtr thisptr, HServerListRequest hRequest, int iServer);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalRefreshComplete(IntPtr thisptr, HServerListRequest hRequest, EMatchMakingServerResponse response);

        [MonoPInvokeCallback(typeof(InternalServerResponded))]
        private static void InternalOnServerResponded(IntPtr thisptr, HServerListRequest hRequest, int iServer) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_ServerResponded(hRequest, iServer);
        }

        [MonoPInvokeCallback(typeof(InternalServerFailedToRespond))]
        private static void InternalOnServerFailedToRespond(IntPtr thisptr, HServerListRequest hRequest, int iServer) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_ServerFailedToRespond(hRequest, iServer);
        }

        [MonoPInvokeCallback(typeof(InternalRefreshComplete))]
        private static void InternalOnRefreshComplete(IntPtr thisptr, HServerListRequest hRequest, EMatchMakingServerResponse response) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_RefreshComplete(hRequest, response);
        }

        [StructLayout(LayoutKind.Sequential)]
        private class VTable {
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalServerResponded m_VTServerResponded;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalServerFailedToRespond m_VTServerFailedToRespond;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalRefreshComplete m_VTRefreshComplete;
        }

        public static explicit operator IntPtr(ISteamMatchmakingServerListResponse that) => that.m_instancePtr;
    }

#endregion

#region ISteamMatchmakingPingResponse

    public class ISteamMatchmakingPingResponse {
        public delegate void ServerResponded(gameserveritem_t server);
        public delegate void ServerFailedToRespond();

        private static readonly Dictionary<IntPtr, ISteamMatchmakingPingResponse> _instances = new();

        private VTable m_VTable;
        private IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private IntPtr m_instancePtr;

        private ServerResponded m_ServerResponded;
        private ServerFailedToRespond m_ServerFailedToRespond;

        public ISteamMatchmakingPingResponse(ServerResponded onResponded, ServerFailedToRespond onFailed) {
            m_ServerResponded = onResponded ?? throw new ArgumentNullException();
            m_ServerFailedToRespond = onFailed ?? throw new ArgumentNullException();

            m_VTable = new VTable {
                m_VTServerResponded = OnServerResponded,
                m_VTServerFailedToRespond = OnServerFailedToRespond
            };

            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);
            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
            m_instancePtr = m_pGCHandle.AddrOfPinnedObject();

            lock (_instances) _instances[m_instancePtr] = this;
        }

        ~ISteamMatchmakingPingResponse() {
            lock (_instances) _instances.Remove(m_instancePtr);
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);
            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerResponded(IntPtr thisptr, gameserveritem_t server);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerFailedToRespond(IntPtr thisptr);

        [MonoPInvokeCallback(typeof(InternalServerResponded))]
        private static void OnServerResponded(IntPtr thisptr, gameserveritem_t server) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_ServerResponded(server);
        }

        [MonoPInvokeCallback(typeof(InternalServerFailedToRespond))]
        private static void OnServerFailedToRespond(IntPtr thisptr) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_ServerFailedToRespond();
        }

        [StructLayout(LayoutKind.Sequential)]
        private class VTable {
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalServerResponded m_VTServerResponded;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalServerFailedToRespond m_VTServerFailedToRespond;
        }

        public static explicit operator IntPtr(ISteamMatchmakingPingResponse that) => that.m_instancePtr;
    }

#endregion

#region ISteamMatchmakingPlayersResponse

    public class ISteamMatchmakingPlayersResponse {
        public delegate void AddPlayerToList(string name, int score, float timePlayed);
        public delegate void PlayersFailedToRespond();
        public delegate void PlayersRefreshComplete();

        private static readonly Dictionary<IntPtr, ISteamMatchmakingPlayersResponse> _instances = new();

        private VTable m_VTable;
        private IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private IntPtr m_instancePtr;

        private AddPlayerToList m_AddPlayerToList;
        private PlayersFailedToRespond m_Failed;
        private PlayersRefreshComplete m_Complete;

        public ISteamMatchmakingPlayersResponse(AddPlayerToList add, PlayersFailedToRespond fail, PlayersRefreshComplete complete) {
            m_AddPlayerToList = add ?? throw new ArgumentNullException();
            m_Failed = fail ?? throw new ArgumentNullException();
            m_Complete = complete ?? throw new ArgumentNullException();

            m_VTable = new VTable {
                m_VTAddPlayerToList = OnAddPlayerToList,
                m_VTPlayersFailedToRespond = OnPlayersFailedToRespond,
                m_VTPlayersRefreshComplete = OnPlayersRefreshComplete
            };

            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);
            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
            m_instancePtr = m_pGCHandle.AddrOfPinnedObject();

            lock (_instances) _instances[m_instancePtr] = this;
        }

        ~ISteamMatchmakingPlayersResponse() {
            lock (_instances) _instances.Remove(m_instancePtr);
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);
            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalAddPlayerToList(IntPtr thisptr, IntPtr pName, int score, float timePlayed);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalPlayersFailedToRespond(IntPtr thisptr);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalPlayersRefreshComplete(IntPtr thisptr);

        [MonoPInvokeCallback(typeof(InternalAddPlayerToList))]
        private static void OnAddPlayerToList(IntPtr thisptr, IntPtr pName, int score, float timePlayed) {
            if (_instances.TryGetValue(thisptr, out var inst)) {
                inst.m_AddPlayerToList(InteropHelp.PtrToStringUTF8(pName), score, timePlayed);
            }
        }

        [MonoPInvokeCallback(typeof(InternalPlayersFailedToRespond))]
        private static void OnPlayersFailedToRespond(IntPtr thisptr) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_Failed();
        }

        [MonoPInvokeCallback(typeof(InternalPlayersRefreshComplete))]
        private static void OnPlayersRefreshComplete(IntPtr thisptr) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_Complete();
        }

        [StructLayout(LayoutKind.Sequential)]
        private class VTable {
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalAddPlayerToList m_VTAddPlayerToList;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalPlayersFailedToRespond m_VTPlayersFailedToRespond;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalPlayersRefreshComplete m_VTPlayersRefreshComplete;
        }

        public static explicit operator IntPtr(ISteamMatchmakingPlayersResponse that) => that.m_instancePtr;
    }

#endregion

#region ISteamMatchmakingRulesResponse

    public class ISteamMatchmakingRulesResponse {
        public delegate void RulesResponded(string rule, string value);
        public delegate void RulesFailedToRespond();
        public delegate void RulesRefreshComplete();

        private static readonly Dictionary<IntPtr, ISteamMatchmakingRulesResponse> _instances = new();

        private VTable m_VTable;
        private IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private IntPtr m_instancePtr;

        private RulesResponded m_RulesResponded;
        private RulesFailedToRespond m_Failed;
        private RulesRefreshComplete m_Complete;

        public ISteamMatchmakingRulesResponse(RulesResponded rule, RulesFailedToRespond fail, RulesRefreshComplete complete) {
            m_RulesResponded = rule ?? throw new ArgumentNullException();
            m_Failed = fail ?? throw new ArgumentNullException();
            m_Complete = complete ?? throw new ArgumentNullException();

            m_VTable = new VTable {
                m_VTRulesResponded = OnRulesResponded,
                m_VTRulesFailedToRespond = OnRulesFailedToRespond,
                m_VTRulesRefreshComplete = OnRulesRefreshComplete
            };

            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);
            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
            m_instancePtr = m_pGCHandle.AddrOfPinnedObject();

            lock (_instances) _instances[m_instancePtr] = this;
        }

        ~ISteamMatchmakingRulesResponse() {
            lock (_instances) _instances.Remove(m_instancePtr);
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);
            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalRulesResponded(IntPtr thisptr, IntPtr rule, IntPtr value);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalRulesFailedToRespond(IntPtr thisptr);
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalRulesRefreshComplete(IntPtr thisptr);

        [MonoPInvokeCallback(typeof(InternalRulesResponded))]
        private static void OnRulesResponded(IntPtr thisptr, IntPtr rule, IntPtr value) {
            if (_instances.TryGetValue(thisptr, out var inst)) {
                inst.m_RulesResponded(InteropHelp.PtrToStringUTF8(rule), InteropHelp.PtrToStringUTF8(value));
            }
        }

        [MonoPInvokeCallback(typeof(InternalRulesFailedToRespond))]
        private static void OnRulesFailedToRespond(IntPtr thisptr) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_Failed();
        }

        [MonoPInvokeCallback(typeof(InternalRulesRefreshComplete))]
        private static void OnRulesRefreshComplete(IntPtr thisptr) {
            if (_instances.TryGetValue(thisptr, out var inst)) inst.m_Complete();
        }

        [StructLayout(LayoutKind.Sequential)]
        private class VTable {
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalRulesResponded m_VTRulesResponded;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalRulesFailedToRespond m_VTRulesFailedToRespond;
            [MarshalAs(UnmanagedType.FunctionPtr)] public InternalRulesRefreshComplete m_VTRulesRefreshComplete;
        }

        public static explicit operator IntPtr(ISteamMatchmakingRulesResponse that) => that.m_instancePtr;
    }

#endregion

} // namespace Steamworks
