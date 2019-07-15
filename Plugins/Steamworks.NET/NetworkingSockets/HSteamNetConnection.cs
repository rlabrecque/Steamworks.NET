using System.Runtime.InteropServices;

namespace Steamworks
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HSteamNetConnection
    {
        public const uint InvalidHandle = 0;

        public readonly uint Handle;

        public HSteamNetConnection(uint handle)
        {
            Handle = handle;
        }
    }
}