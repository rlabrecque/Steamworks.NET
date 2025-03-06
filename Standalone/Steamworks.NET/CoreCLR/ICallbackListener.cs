#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK
using System;

namespace Steamworks.CoreCLR {
	internal interface ICallbackListener {
		int Identity { get; }
		Type ResultType { get; }

		public void InvokeCallbacks(ref CallbackMsg_t callback);
	}
}
#endif