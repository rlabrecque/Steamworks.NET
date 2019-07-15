using System.Runtime.InteropServices;

namespace Steamworks
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HSteamListenSocket
    {
        public const uint InvalidHandle = 0;

        public readonly uint Handle;

        public HSteamListenSocket(uint handle)
        {
            Handle = handle;
        }
    }
}