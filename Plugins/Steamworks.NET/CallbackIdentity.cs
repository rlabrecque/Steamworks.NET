// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#if !DISABLESTEAMWORKS

using System;

namespace Steamworks {
	class CallbackIdentities {
		public static int GetCallbackIdentity(Type callbackStruct) {
#if UNITY_EDITOR || UNITY_STANDALONE || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX
            foreach (CallbackIdentityAttribute attribute in callbackStruct.GetCustomAttributes(typeof(CallbackIdentityAttribute), false)) {
				return attribute.Identity;
			}
#endif
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

#endif // !DISABLESTEAMWORKS
