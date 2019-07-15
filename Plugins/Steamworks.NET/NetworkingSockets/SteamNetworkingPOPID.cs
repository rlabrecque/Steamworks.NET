using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    public struct SteamNetworkingPOPID
    {
        public readonly uint Id;

        public SteamNetworkingPOPID(uint id)
        {
            Id = id;
        }
    }
}
