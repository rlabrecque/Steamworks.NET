// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2014 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

// Are we running under Unity?
#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5
	#define UNITY_BUILD

	// If we're running in the Unity Editor we need the editors platform.
	#if UNITY_EDITOR_WIN
		#define WINDOWS_BUILD
	#elif UNITY_EDITOR_OSX
		#define UNIX_BUILD
	// Otherwise we want the target platform.
	#elif UNITY_STANDALONE_WIN
		#define WINDOWS_BUILD
	#elif UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
		#define UNIX_BUILD
	#endif

	// Unity 32bit Mono on Windows crashes with ThisCall for some reason, StdCall without the 'this' ptr is the only thing that works..? 
	#if WINDOWS_BUILD && (UNITY_EDITOR || !UNITY_64)
		#define NOTHISPTR
	#endif
#else
// If we're not a UNITY_BUILD:
	#if STEAMWORKS_LIN_OSX
		#define UNIX_BUILD
	#else
		#define WINDOWS_BUILD
		// We want to be explicit about what platform we're building for or else porting becomes troublesome.
		#if !STEAMWORKS_WIN
			#warning You need to define STEAMWORKS_WIN, or STEAMWORKS_LIN_OSX. Refer to the readme for more details.
		#endif
	#endif
#endif

using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	public sealed class Callback<T> {
		private CCallbackBaseVTable VTable;
		private IntPtr m_pVTable = IntPtr.Zero;
		private CCallbackBase m_CCallbackBase;
		private GCHandle m_pCCallbackBase;

		public delegate void DispatchDelegate(T param);
		private event DispatchDelegate m_Func;

		private bool m_bGameServer;
		private readonly int m_size = Marshal.SizeOf(typeof(T));

		// Temporary Hack
		static System.Collections.Generic.List<Callback<T>> GCKeepAlive = new System.Collections.Generic.List<Callback<T>>();
		static bool bWarnedOnce = false;

		public static Callback<T> Create(DispatchDelegate func) {
			return new Callback<T>(func, bGameServer: false, bKeepAlive: false);
		}

		public static Callback<T> CreateGameServer(DispatchDelegate func) {
			return new Callback<T>(func, bGameServer: true, bKeepAlive: false);
		}

		public Callback(DispatchDelegate func, bool bGameServer = false, bool bKeepAlive = true) {
			m_bGameServer = bGameServer;
			BuildCCallbackBase();
			Register(func);

			// This is a temporary hack to preserve backwards compatability with the old CallbackDispatcher.
			// If this is still here in 5.0.0 yell at me.
			if (bKeepAlive) {
				if (!bWarnedOnce) {
					bWarnedOnce = true;
					const string deprecatedMsg = "Please use the new (as of 3.0.0) api for creating Callbacks. Callback<Type>.Create(func). You must now maintain a handle to the callback so that the GC does not clean it up prematurely.";
#if UNITY_BUILD
					UnityEngine.Debug.LogWarning(deprecatedMsg);
#else
					throw new System.InvalidOperationException(deprecatedMsg);
#endif
				}

				GCKeepAlive.Add(this);
			}
		}

		~Callback() {
			Unregister();

			if (m_pVTable != IntPtr.Zero) {
				Marshal.FreeHGlobal(m_pVTable);
			}

			if (m_pCCallbackBase.IsAllocated) {
				m_pCCallbackBase.Free();
			}
		}

		// Manual registration of the callback
		public void Register(DispatchDelegate func) {
			if (func == null) {
				throw new Exception("Callback function must not be null.");
			}

			if ((m_CCallbackBase.m_nCallbackFlags & CCallbackBase.k_ECallbackFlagsRegistered) == CCallbackBase.k_ECallbackFlagsRegistered) {
				Unregister();
			}

			if (m_bGameServer) {
				SetGameserverFlag();
			}

			m_Func = func;

			// k_ECallbackFlagsRegistered is set by SteamAPI_RegisterCallback.
			NativeMethods.SteamAPI_RegisterCallback(m_pCCallbackBase.AddrOfPinnedObject(), CallbackIdentities.GetCallbackIdentity(typeof(T)));
		}

		public void Unregister() {
			// k_ECallbackFlagsRegistered is removed by SteamAPI_UnregisterCallback.
			NativeMethods.SteamAPI_UnregisterCallback(m_pCCallbackBase.AddrOfPinnedObject());
		}

		public void SetGameserverFlag() { m_CCallbackBase.m_nCallbackFlags |= CCallbackBase.k_ECallbackFlagsGameServer; }

		private void OnRunCallback(
#if !NOTHISPTR
			IntPtr thisptr,
#endif
			IntPtr pvParam) {
			m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)));
		}

		// Shouldn't get ever get called here, but this is what C++ Steamworks does!
		private void OnRunCallResult(
#if !NOTHISPTR
			IntPtr thisptr,
#endif
			IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
			m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)));
		}

		private int OnGetCallbackSizeBytes(
#if !NOTHISPTR
			IntPtr thisptr
#endif
			) {
			return m_size;
		}

		// Steamworks.NET Specific
		private void BuildCCallbackBase() {
			VTable = new CCallbackBaseVTable() {
				m_RunCallResult = OnRunCallResult,
				m_RunCallback = OnRunCallback,
				m_GetCallbackSizeBytes = OnGetCallbackSizeBytes
			};
			m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CCallbackBaseVTable)));
			Marshal.StructureToPtr(VTable, m_pVTable, false);

			m_CCallbackBase = new CCallbackBase() {
				m_vfptr = m_pVTable,
				m_nCallbackFlags = 0,
				m_iCallback = CallbackIdentities.GetCallbackIdentity(typeof(T))
			};
			m_pCCallbackBase = GCHandle.Alloc(m_CCallbackBase, GCHandleType.Pinned);
		}
	}

	public sealed class CallResult<T> {
		private CCallbackBaseVTable VTable;
		private IntPtr m_pVTable = IntPtr.Zero;
		private CCallbackBase m_CCallbackBase;
		private GCHandle m_pCCallbackBase;

		public delegate void APIDispatchDelegate(T param, bool bIOFailure);
		private event APIDispatchDelegate m_Func;

		private SteamAPICall_t m_hAPICall = SteamAPICall_t.Invalid;
		private readonly int m_size = Marshal.SizeOf(typeof(T));

		public static CallResult<T> Create(APIDispatchDelegate func = null) {
			return new CallResult<T>(func);
		}

		public CallResult(APIDispatchDelegate func = null) {
			m_Func = func;
			BuildCCallbackBase();
		}

		~CallResult() {
			Cancel();

			if (m_pVTable != IntPtr.Zero) {
				Marshal.FreeHGlobal(m_pVTable);
			}

			if (m_pCCallbackBase.IsAllocated) {
				m_pCCallbackBase.Free();
			}
		}

		public void Set(SteamAPICall_t hAPICall, APIDispatchDelegate func = null) {
			// Unlike the official SDK we let the user assign a single function during creation,
			// and allow them to skip having to do so every time that they call .Set()
			if (func != null) {
				m_Func = func;
			}

			if (m_Func == null) {
				throw new Exception("CallResult function was null, you must either set it in the CallResult Constructor or in Set()");
			}

			if (m_hAPICall != SteamAPICall_t.Invalid) {
				NativeMethods.SteamAPI_UnregisterCallResult(m_pCCallbackBase.AddrOfPinnedObject(), (ulong)m_hAPICall);
			}

			m_hAPICall = hAPICall;

			if (hAPICall != SteamAPICall_t.Invalid) {
				NativeMethods.SteamAPI_RegisterCallResult(m_pCCallbackBase.AddrOfPinnedObject(), (ulong)hAPICall);
			}
		}

		public bool IsActive() {
			return (m_hAPICall != SteamAPICall_t.Invalid);
		}

		public void Cancel() {
			if (m_hAPICall != SteamAPICall_t.Invalid) {
				NativeMethods.SteamAPI_UnregisterCallResult(m_pCCallbackBase.AddrOfPinnedObject(), (ulong)m_hAPICall);
				m_hAPICall = SteamAPICall_t.Invalid;
			}
		}

		public void SetGameserverFlag() { m_CCallbackBase.m_nCallbackFlags |= CCallbackBase.k_ECallbackFlagsGameServer; }

		// Shouldn't get ever get called here, but this is what C++ Steamworks does!
		private void OnRunCallback(
#if !NOTHISPTR
			IntPtr thisptr,
#endif
			IntPtr pvParam) {
			m_hAPICall = SteamAPICall_t.Invalid; // Caller unregisters for us
			m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)), false);
		}


		private void OnRunCallResult(
#if !NOTHISPTR
			IntPtr thisptr,
#endif
			IntPtr pvParam, bool bFailed, ulong hSteamAPICall) {
			if ((SteamAPICall_t)hSteamAPICall == m_hAPICall) {
				m_hAPICall = SteamAPICall_t.Invalid; // Caller unregisters for us
				m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)), bFailed);
			}
		}
		
		private int OnGetCallbackSizeBytes(
#if !NOTHISPTR
			IntPtr thisptr
#endif
			) {
			return m_size;
		}

		// Steamworks.NET Specific
		private void BuildCCallbackBase() {
			VTable = new CCallbackBaseVTable() {
				m_RunCallback = OnRunCallback,
				m_RunCallResult = OnRunCallResult,
				m_GetCallbackSizeBytes = OnGetCallbackSizeBytes
			};
			m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CCallbackBaseVTable)));
			Marshal.StructureToPtr(VTable, m_pVTable, false);

			m_CCallbackBase = new CCallbackBase() {
				m_vfptr = m_pVTable,
				m_nCallbackFlags = 0,
				m_iCallback = CallbackIdentities.GetCallbackIdentity(typeof(T))
			};
			m_pCCallbackBase = GCHandle.Alloc(m_CCallbackBase, GCHandleType.Pinned);
		}
	}

	//
	[StructLayout(LayoutKind.Sequential)]
	public class CCallbackBase {
		public const byte k_ECallbackFlagsRegistered = 0x01;
		public const byte k_ECallbackFlagsGameServer = 0x02;
		public IntPtr m_vfptr;
		public byte m_nCallbackFlags;
		public int m_iCallback;
	};

	[StructLayout(LayoutKind.Sequential)]
	internal class CCallbackBaseVTable {
#if NOTHISPTR
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void RunCBDel(IntPtr pvParam);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void RunCRDel(IntPtr pvParam, [MarshalAs(UnmanagedType.I1)] bool bIOFailure, ulong hSteamAPICall);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int GetCallbackSizeBytesDel();
#else
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void RunCBDel(IntPtr thisptr, IntPtr pvParam);
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void RunCRDel(IntPtr thisptr, IntPtr pvParam, [MarshalAs(UnmanagedType.I1)] bool bIOFailure, ulong hSteamAPICall);
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate int GetCallbackSizeBytesDel(IntPtr thisptr);
#endif

		// RunCallback and RunCallResult are swapped in MSVC ABI
#if WINDOWS_BUILD
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public RunCRDel m_RunCallResult;
#endif
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public RunCBDel m_RunCallback;
#if !WINDOWS_BUILD
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public RunCRDel m_RunCallResult;
#endif
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public GetCallbackSizeBytesDel m_GetCallbackSizeBytes;
	}
}
