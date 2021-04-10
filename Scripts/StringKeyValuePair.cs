#if !DISABLESTEAMWORKS
using System;
using System.Linq;

namespace HeathenEngineering.SteamAPI
{

    [Serializable]
    public struct StringKeyValuePair
    {
        public string key;
        public string value;
    }
}
#endif