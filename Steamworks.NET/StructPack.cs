using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	static class Packsize {
#if UNITY_STANDALONE_WIN
		public const int value = 8;
#elif UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
		public const int value = 4;
#else
#error ???
#endif

		public static bool SizeOf() {
			int size = Marshal.SizeOf(typeof(ValvePackingSentinel_t));
#if UNITY_STANDALONE_WIN
			if (size == 32)
				return true;
#elif UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
			if (size == 24)
				return true;
#endif
			return false;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
	struct ValvePackingSentinel_t {
		uint m_u32;
		ulong m_u64;
		ushort m_u16;
		double m_d;
	};
}