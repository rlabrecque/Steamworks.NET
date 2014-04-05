// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2014 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	public class InteropHelp {
		public static void TestIfPlatformSupported() {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX && !UNITY_STANDALONE_OSX && !STEAMWORKS_WIN && !STEAMWORKS_LIN_OSX
			throw new System.InvalidOperationException("Steamworks functions can only be called on platforms that Steam is available on.");
#endif
		}

		public static void TestIfAvailableClient() {
			TestIfPlatformSupported();
			if (NativeMethods.SteamClient() == System.IntPtr.Zero) {
				throw new System.InvalidOperationException("Steamworks is not initialized.");
			}
		}
		public static void TestIfAvailableGameServer() {
			TestIfPlatformSupported();
			if (NativeMethods.SteamClientGameServer() == System.IntPtr.Zero) {
				throw new System.InvalidOperationException("Steamworks is not initialized.");
			}
		}
		
		public static string PtrToStringUTF8(IntPtr nativeUtf8) {
			if (nativeUtf8 == IntPtr.Zero)
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
			private IntPtr m_NativeString;

			public UTF8String(string managedString) {
				if (string.IsNullOrEmpty(managedString)) {
					m_NativeString = IntPtr.Zero;
					return;
				}

				byte[] buffer = new byte[System.Text.Encoding.UTF8.GetByteCount(managedString) + 1];
				System.Text.Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
				m_NativeString = Marshal.AllocHGlobal(buffer.Length);
				Marshal.Copy(buffer, 0, m_NativeString, buffer.Length);
			}

			~UTF8String() {
				if (m_NativeString != IntPtr.Zero) {
					Marshal.FreeHGlobal(m_NativeString);
				}
			}

			public static implicit operator IntPtr(UTF8String that) {
				return that.m_NativeString;
			}
		}

		// At somepoint this should become an IDisposable
		public class SteamParamStringArray {
			IntPtr m_pSteamParamStringArray;
			IntPtr[] m_Strings;

			public SteamParamStringArray(System.Collections.Generic.IList<string> strings) {
				if (strings == null) {
					m_pSteamParamStringArray = IntPtr.Zero;
					return;
				}

				m_Strings = new IntPtr[strings.Count];
				for (int index = 0; index < strings.Count; ++index) {
					m_Strings[index] = new UTF8String(strings[index]);
				}

				SteamParamStringArray_t stringArray = new SteamParamStringArray_t() {
					m_ppStrings = m_Strings,
					m_nNumStrings = strings.Count
				};

				m_pSteamParamStringArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SteamParamStringArray_t)));
				Marshal.StructureToPtr((object)stringArray, m_pSteamParamStringArray, false);
			}

			~SteamParamStringArray() {
				if (m_pSteamParamStringArray != IntPtr.Zero) {
					Marshal.FreeHGlobal(m_pSteamParamStringArray);
				}
			}

			public static implicit operator IntPtr(SteamParamStringArray that) {
				return that.m_pSteamParamStringArray;
			}
		}
	}
}
