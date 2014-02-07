#if UNITY_STANDALONE_WIN || STEAMWORKS_WIN
#define VALVE_CALLBACK_PACK_LARGE
#elif UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_LIN || STEAMWORKS_OSX
#define VALVE_CALLBACK_PACK_SMALL
#else
#error ???
#endif

using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	public static class Packsize {
#if VALVE_CALLBACK_PACK_LARGE
		public const int value = 8;
#elif VALVE_CALLBACK_PACK_SMALL
		public const int value = 4;
#endif

		public static bool Test() {
			int sentinelSize = Marshal.SizeOf(typeof(ValvePackingSentinel_t));
			int subscribedFilesSize = Marshal.SizeOf(typeof(RemoteStorageEnumerateUserSubscribedFilesResult_t));
#if VALVE_CALLBACK_PACK_LARGE
			if (sentinelSize != 32 || subscribedFilesSize != (1 + 1 + 1 + 50 + 100) * 4 + 4)
				return false;
#elif VALVE_CALLBACK_PACK_SMALL
			if (sentinelSize != 24 || subscribedFilesSize != (1 + 1 + 1 + 50 + 100) * 4)
				return false;
#endif
			return true;
		}

		[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
		struct ValvePackingSentinel_t {
			uint m_u32;
			ulong m_u64;
			ushort m_u16;
			double m_d;
		};
	}
}