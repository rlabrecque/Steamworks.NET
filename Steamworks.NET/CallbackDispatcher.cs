// This was largely written by Ryan 'Azuisleet' Kistner for Steam4NET2
// Licensed for use under the Public Domain
// https://github.com/azuisleet
// https://github.com/SteamRE/open-steamworks/tree/master/Steam4NET2/Steam4NET2

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Steamworks {
	public interface ICallback {
		void Run(IntPtr param);
	}

	public interface ICallResult : ICallback {
		int GetExpectedSize();
		int GetExpectedCallback();
		void ClearAPICallHandle();
	}

	public class Callback<CallbackType> : ICallback {
		public delegate void DispatchDelegate(CallbackType param);
		public event DispatchDelegate OnRun;

		public Callback() {
			CallbackDispatcher.RegisterCallback(this, CallbackIdentities.GetCallbackIdentity(typeof(CallbackType)));
		}

		public Callback(DispatchDelegate myFunc)
			: this() {
			this.OnRun += myFunc;
		}

		~Callback() {
			UnRegister();
		}

		public void UnRegister() {
			CallbackDispatcher.UnRegisterCallback(this, CallbackIdentities.GetCallbackIdentity(typeof(CallbackType)));
		}

		public void Run(IntPtr pubParam) {
			if (this.OnRun != null)
				this.OnRun((CallbackType)Marshal.PtrToStructure(pubParam, typeof(CallbackType)));
		}
	}

	public class CallResult<CallbackType> : ICallResult {
		private int callback;
		private int size;
		private UInt64 callhandle = 0;

		public delegate void APIDispatchDelegate(ulong callHandle, CallbackType param);
		public event APIDispatchDelegate OnRun;

		public CallResult() {
			callback = CallbackIdentities.GetCallbackIdentity(typeof(CallbackType));
			size = Marshal.SizeOf(typeof(CallbackType));
		}

		public CallResult(APIDispatchDelegate myFunc)
			: this() {
			this.OnRun += myFunc;
		}

		public CallResult(APIDispatchDelegate myFunc, UInt64 apicallhandle)
			: this(myFunc) {
			SetAPICallHandle(apicallhandle);
		}

		~CallResult() {
			ClearAPICallHandle();
		}

		public void SetAPICallHandle(UInt64 newcallhandle) {
			if (callhandle != 0)
				ClearAPICallHandle();

			callhandle = newcallhandle;
			CallbackDispatcher.RegisterCallResult(this, newcallhandle);
		}

		public void ClearAPICallHandle() {
			CallbackDispatcher.ClearCallResult(this, callhandle);
		}

		public void Run(IntPtr pubParam) {
			if (this.OnRun != null)
				this.OnRun(callhandle, (CallbackType)Marshal.PtrToStructure(pubParam, typeof(CallbackType)));
		}

		public int GetExpectedSize() {
			return size;
		}

		public int GetExpectedCallback() {
			return callback;
		}
	}

	public class CallbackDispatcher {
		private static Dictionary<int, ICallback> registeredCallbacks = new Dictionary<int, ICallback>();
		private static Dictionary<UInt64, ICallResult> registeredAPICallbacks = new Dictionary<UInt64, ICallResult>();

		public static HSteamPipe LastActivePipe { get; private set; }
		public static Callback<SteamAPICallCompleted_t> APICallbackCompleted = new Callback<SteamAPICallCompleted_t>(RunAPICallback);


		public static void RegisterCallback(ICallback callback, int iCallback) {
			registeredCallbacks.Add(iCallback, callback);
		}

		public static void UnRegisterCallback(ICallback callback, int iCallback) {
			if (registeredCallbacks[iCallback] == callback) {
				registeredCallbacks.Remove(iCallback);
			}
		}

		public static void RegisterCallResult(ICallResult callback, UInt64 callhandle) {
			registeredAPICallbacks.Add(callhandle, callback);
		}

		public static void ClearCallResult(ICallResult callback, UInt64 callhandle) {
			registeredAPICallbacks.Remove(callhandle);
		}

		public static void RunCallbacks() {
			CallbackMsg_t callbackmsg = new CallbackMsg_t();
			HSteamPipe pipe = SteamAPI.GetHSteamPipe();

			// This needs to be called every frame to process matchmaking results
			SteamUtils.RunFrame();

			while (NativeMethods.Steam_BGetCallback(pipe, ref callbackmsg)) {
				LastActivePipe = pipe;

				ICallback callback;
				if (registeredCallbacks.TryGetValue(callbackmsg.m_iCallback, out callback)) {
					callback.Run(callbackmsg.m_pubParam);
				}

				NativeMethods.Steam_FreeLastCallback(pipe);
			}
		}

		public static void RunAPICallback(SteamAPICallCompleted_t apicall) {
			ICallResult apiCallback;

			if (!registeredAPICallbacks.TryGetValue(apicall.m_hAsyncCall, out apiCallback))
				return;

			IntPtr pData = IntPtr.Zero;
			bool bFailed = false;

			try {
				pData = Marshal.AllocHGlobal(apiCallback.GetExpectedSize());

				if (!NativeMethods.Steam_GetAPICallResult(LastActivePipe, apicall.m_hAsyncCall, pData, apiCallback.GetExpectedSize(), apiCallback.GetExpectedCallback(), ref bFailed))
					return;

				if (bFailed)
					return;

				apiCallback.Run(pData);
			}
			finally {
				apiCallback.ClearAPICallHandle();

				Marshal.FreeHGlobal(pData);
			}
		}
	}
}
