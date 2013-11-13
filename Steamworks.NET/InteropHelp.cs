using System.Runtime.InteropServices;

namespace Steamworks {
	public class InteropHelp {
		public static string PtrToStringUTF8(System.IntPtr nativeUtf8) {
			if (nativeUtf8 == System.IntPtr.Zero)
				return string.Empty;

			int len = 0;

			while (Marshal.ReadByte(nativeUtf8, len) != 0)
				++len;

			if (len == 0) 
				return string.Empty;

			byte[] buffer = new byte[len];
			Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
			return System.Text.Encoding.UTF8.GetString(buffer);
		}

		// At somepoint this should become an IDisposable
		public class UTF8String {
			public System.IntPtr m_NativeString = System.IntPtr.Zero;

			public UTF8String(string managedString) {
				byte[] buffer = new byte[System.Text.Encoding.UTF8.GetByteCount(managedString) + 1];
				System.Text.Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
				m_NativeString = Marshal.AllocHGlobal(buffer.Length);
				Marshal.Copy(buffer, 0, m_NativeString, buffer.Length);
			}

			~UTF8String() {
				if (m_NativeString != System.IntPtr.Zero) {
					Marshal.FreeHGlobal(m_NativeString);
				}
			}

			public static implicit operator System.IntPtr(UTF8String that) {
				return that.m_NativeString;
			}
		}
	}
}