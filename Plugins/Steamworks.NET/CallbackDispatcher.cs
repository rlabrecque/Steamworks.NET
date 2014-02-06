// This was largely written by Ryan 'Azuisleet' Kistner for Steam4NET2
// This file is licensed for use under the Public Domain. 
// Where that dedication is not recognized, you are granted a perpetual,
// irrevokable license to copy and modify this file as you see fit.
// https://github.com/azuisleet
// https://github.com/SteamRE/open-steamworks/tree/master/Steam4NET2/Steam4NET2

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Steamworks {
	public interface ICallbackBase {
		void Run(IntPtr param);
	}

	public interface ICallResultBase {
		int GetCallbackSizeBytes();
		int GetICallback();
		void Run(IntPtr param, bool bIOFailure);
	}

	public class Callback<T> : ICallbackBase {
		public delegate void DispatchDelegate(T param);
		public event DispatchDelegate m_Func;

		public Callback() {
			CallbackDispatcher.RegisterCallback(this, CallbackIdentities.GetCallbackIdentity(typeof(T)));
		}

		public Callback(DispatchDelegate myFunc)
			: this() {
			if (myFunc == null)
				throw new Exception("Function must not be null.");
			
			this.m_Func += myFunc;
		}

		~Callback() {
			UnRegister();
		}

		public void UnRegister() {
			CallbackDispatcher.UnRegisterCallback(this, CallbackIdentities.GetCallbackIdentity(typeof(T)));
		}

		// ICallbackBase
		public void Run(IntPtr pubParam) {
			this.m_Func((T)Marshal.PtrToStructure(pubParam, typeof(T)));
		}
	}

	public class CallResult<T> : ICallResultBase {
		private SteamAPICall_t m_hAPICall = SteamAPICall_t.Invalid;
		private int m_iCallback;
		private int m_Size;

		public delegate void APIDispatchDelegate(T param, bool bIOFailure);
		public event APIDispatchDelegate m_Func;

		public CallResult() {
			m_iCallback = CallbackIdentities.GetCallbackIdentity(typeof(T));
			m_Size = Marshal.SizeOf(typeof(T));
		}

		public CallResult(APIDispatchDelegate myFunc)
			: this() {
			if (myFunc == null)
				throw new Exception("Function must not be null.");
			
			this.m_Func += myFunc;
		}

		public CallResult(APIDispatchDelegate myFunc, SteamAPICall_t hAPICall)
			: this(myFunc) {
			Set(hAPICall);
		}

		~CallResult() {
			Cancel();
		}

		public void Set(SteamAPICall_t hAPICall) {
			if (m_hAPICall != SteamAPICall_t.Invalid)
				Cancel();

			m_hAPICall = hAPICall;

			if(hAPICall != SteamAPICall_t.Invalid)
				CallbackDispatcher.RegisterCallResult(this, hAPICall);
		}

		public bool IsActive() {
			return (m_hAPICall != SteamAPICall_t.Invalid);
		}

		public void Cancel() {
			if (m_hAPICall != SteamAPICall_t.Invalid) {
				CallbackDispatcher.UnregisterCallResult(this, m_hAPICall);
				m_hAPICall = SteamAPICall_t.Invalid;
			}
		}

		// ICallResultBase
		public int GetCallbackSizeBytes() {
			return m_Size;
		}

		public int GetICallback() {
			return m_iCallback;
		}

		public void Run(IntPtr pubParam, bool bIOFailure) {
			m_hAPICall = SteamAPICall_t.Invalid; // Caller unregisters for us.
			this.m_Func((T)Marshal.PtrToStructure(pubParam, typeof(T)), bIOFailure);
		}
	}

	public class CallbackDispatcher {
		private static Dictionary<int, ICallbackBase> m_RegisteredCallbacks = new Dictionary<int, ICallbackBase>();
		private static Dictionary<SteamAPICall_t, ICallResultBase> m_RegisteredCallResults = new Dictionary<SteamAPICall_t, ICallResultBase>();

		public static HSteamPipe m_LastActivePipe { get; private set; }
		public static Callback<SteamAPICallCompleted_t> m_APICallbackCompleted = new Callback<SteamAPICallCompleted_t>(RunCallResult);


		public static void RegisterCallback(ICallbackBase callback, int iCallback) {
			try {
				m_RegisteredCallbacks.Add(iCallback, callback);
			}
			catch (ArgumentException e) {
				throw new Exception("You tried to register a specific Callback multiple times.\nIf you need a callback to end up in multiple places then register it once and delegate it elsewhere from there.\n" + e);
			}
		}

		public static void UnRegisterCallback(ICallbackBase callback, int iCallback) {
			if (m_RegisteredCallbacks[iCallback] == callback) {
				m_RegisteredCallbacks.Remove(iCallback);
			}
		}

		public static void RegisterCallResult(ICallResultBase callback, SteamAPICall_t hAPICall) {
			try {
				m_RegisteredCallResults.Add(hAPICall, callback);
			}
			catch (ArgumentException e) {
				throw new Exception("You tried to register a CallResult multiple times.\nIf you need a callresult to end up in multiple places then register it once and delegate it elsewhere from there.\n" + e);
			}
		}

		public static void UnregisterCallResult(ICallResultBase callback, SteamAPICall_t hAPICall) {
			if (m_RegisteredCallResults[hAPICall] == callback) {
				m_RegisteredCallResults.Remove(hAPICall);
			}
		}

		public static void RunCallbacks() {
			CallbackMsg_t callbackmsg;
			HSteamPipe pipe = SteamAPI.GetHSteamPipe();

			while (NativeMethods.Steam_BGetCallback(pipe, out callbackmsg)) {
				m_LastActivePipe = pipe;

				ICallbackBase callback;
				if (m_RegisteredCallbacks.TryGetValue(callbackmsg.m_iCallback, out callback)) {
					callback.Run(callbackmsg.m_pubParam);
				}

				NativeMethods.Steam_FreeLastCallback(pipe);
			}

			// These needs to be called every frame to process matchmaking results and poll the controller
			SteamUtils.RunFrame();
			SteamController.RunFrame();
		}

		public static void RunCallResult(SteamAPICallCompleted_t apicall) {
			ICallResultBase apiCallback;

			if (!m_RegisteredCallResults.TryGetValue(apicall.m_hAsyncCall, out apiCallback))
				return;

			IntPtr pData = IntPtr.Zero;
			bool bFailed;

			try {
				pData = Marshal.AllocHGlobal(apiCallback.GetCallbackSizeBytes());

				if (!NativeMethods.Steam_GetAPICallResult(m_LastActivePipe, apicall.m_hAsyncCall, pData, apiCallback.GetCallbackSizeBytes(), apiCallback.GetICallback(), out bFailed))
					return;

				apiCallback.Run(pData, bFailed);
			}
			finally {
				UnregisterCallResult(apiCallback, apicall.m_hAsyncCall);

				Marshal.FreeHGlobal(pData);
			}
		}
	}
}
