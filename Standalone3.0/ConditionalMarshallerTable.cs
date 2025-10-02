using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Steamworks
{
	internal static partial class ConditionalMarshallerTable
	{
		private static readonly FrozenDictionary<Type, Func<IntPtr, object>> s_marshallerLookupTable;

		// partial, in generated file
		// static ConditionalMarshallerTable();

		public static T Marshal<T>(IntPtr unmanagetype)
		{
			return (T)s_marshallerLookupTable[typeof(T)](unmanagetype);
		}
	}
}
