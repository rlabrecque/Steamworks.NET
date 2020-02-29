using mstring = System::String;
using namespace System;

namespace Steamworks::NET
{
	public ref class Utf8InterlopHelper
	{
	public:
		static mstring^ FromArray(array<unsigned char>^ utf8String)
		{
			using System::MemoryExtensions;
			auto span = MemoryExtensions::AsSpan(utf8String);
			
			int i = 0;
			for (; span[i] != 0 && i < span.Length; i++);

			return Text::Encoding::UTF8->GetString(utf8String, 0, i);
		}

		static void WriteArray(mstring^ str, array<unsigned char>^ output)
		{
			auto span = MemoryExtensions::AsSpan(str);
			int i = 0;
			
			auto iter = span.GetEnumerator();
			iter.


		}
	private:

	};

}