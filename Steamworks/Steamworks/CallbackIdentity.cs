using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	class CallbackIdentities {
		public static int GetCallbackIdentity(Type callbackStruct) {
			foreach (CallbackIdentityAttribute attribute in callbackStruct.GetCustomAttributes(typeof(CallbackIdentityAttribute), false)) {
				return attribute.Identity;
			}

			throw new Exception("Callback number not found for struct " + callbackStruct);
		}
	}

	[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
	internal class CallbackIdentityAttribute : System.Attribute {
		public int Identity { get; set; }
		public CallbackIdentityAttribute(int callbackNum) {
			Identity = callbackNum;
		}
	}
}