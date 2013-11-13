using System.Runtime.InteropServices;

namespace Steamworks {
	// Summary:
	//     Used to store a SteamID in callbacks (With proper alignment / padding).
	//     You probably don't want to use this type directly, convert it to CSteamID.
	[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
	public struct SteamID_t {
		public uint low32Bits;    // m_unAccountID (32)
		public uint high32Bits;   // m_unAccountInstance (20), m_EAccountType (4), m_EUniverse (8)
	}

	[StructLayout(LayoutKind.Sequential, Size = Packsize.value)]
	public struct GameID_t {
        public ulong m_ulGameID;
	}
}