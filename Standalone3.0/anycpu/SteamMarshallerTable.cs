using System;
using System.Runtime.InteropServices;

namespace Steamworks
{
	public static partial class SteamMarshallerTable
	{
		// private static readonly FrozenDictionary<Type, Func<IntPtr, object>> s_marshallerLookupTable;

		private static partial class Impl<T>
		{
			public static readonly Func<IntPtr, T> Marshaller =
				unmanaged => System.Runtime.InteropServices.Marshal.PtrToStructure<T>(unmanaged);
		}

		// partial, in generated file
		// static ConditionalMarshallerTable();

		public static T Marshal<T>(IntPtr unmanagetype)
		{
			return Impl<T>.Marshaller(unmanagetype);
        }
	}
}
