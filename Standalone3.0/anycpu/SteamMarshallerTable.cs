using System;

namespace Steamworks
{
	/// <summary>
	/// A static class that provides marshalling functionality for Steamworks structures.
	/// </summary>
	/// <remarks>
	/// All Steamworks.NET.AnyCPU unmanaged-to-managed marshalling should done through this class.
	/// </remarks>
	public static partial class SteamMarshallerTable
	{
		// private static readonly FrozenDictionary<Type, Func<IntPtr, object>> s_marshallerLookupTable;

		private static partial class Impl<T> where T : struct
		{
			public static readonly Func<IntPtr, T> Marshaller =
				unmanaged => System.Runtime.InteropServices.Marshal.PtrToStructure<T>(unmanaged);
		}

		// partial, in generated file
		// static ConditionalMarshallerTable();

		/// <summary>
		/// Obtain correct managed form of data from unmanaged pointer. Smooths out platform differences; 
		/// Use this method to correctly convert unmanaged pointers to managed types in AnyCPU builds.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="unmanagetype"></param>
		/// <returns></returns>
		public static T Marshal<T>(IntPtr unmanagetype)
			where T : struct {
			return Impl<T>.Marshaller(unmanagetype);
		}
	}
}
