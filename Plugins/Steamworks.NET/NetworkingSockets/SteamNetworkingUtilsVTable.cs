#define STEAMNETWORKINGSOCKETS_ENABLE_SDR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    [SuppressUnmanagedCodeSecurity]
    internal class SteamNetworkingUtilsVTable
    {
#if STEAMNETWORKINGSOCKETS_ENABLE_SDR
        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate float GetLocalPingLocationDelegate(IntPtr instance, ref SteamNetworkPingLocation_t result);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int EstimatePingTimeBetweenTwoLocationsDelegate(IntPtr instance, in SteamNetworkPingLocation_t location1, in SteamNetworkPingLocation_t location2);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int EstimatePingTimeFromLocalHostDelegate(IntPtr instance, in SteamNetworkPingLocation_t remoteLocation);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate void ConvertPingLocationToStringDelegate(IntPtr instance, in SteamNetworkPingLocation_t location, IntPtr pszBuf, int cchBufSize);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool ParsePingLocationStringDelegate(IntPtr instance, InteropHelp.UTF8StringHandle pszString, out SteamNetworkPingLocation_t result);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool CheckPingDataUpToDateDelegate(IntPtr instance, float flMaxAgeSeconds);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool IsPingMeasurementInProgressDelegate(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int GetPingToDataCenterDelegate(IntPtr instance, SteamNetworkingPOPID popID, IntPtr pViaRelayPoP);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int GetDirectPingToPOPDelegate(IntPtr instance, SteamNetworkingPOPID popID);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int GetPOPCountDelegate(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate int GetPOPListDelegate(IntPtr instance, IntPtr list, int nListSz);
#endif // #ifdef STEAMNETWORKINGSOCKETS_ENABLE_SDR

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate SteamNetworkingMicroseconds GetLocalTimestampDelegate(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate void SetDebugOutputFunctionDelegate(IntPtr instance, ESteamNetworkingSocketsDebugOutputType eDetailLevel, IntPtr pfnFunc);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool SetConfigValueDelegate(IntPtr instance, ESteamNetworkingConfigValue eValue, ESteamNetworkingConfigScope eScopeType, IntPtr scopeObj, ESteamNetworkingConfigDataType eDataType, IntPtr pArg);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate ESteamNetworkingGetConfigValueResult GetConfigValueDelegate(IntPtr instance, ESteamNetworkingConfigValue eValue, ESteamNetworkingConfigScope eScopeType, IntPtr scopeObj, out ESteamNetworkingConfigDataType pOutDataType, IntPtr pResult, ref UIntPtr cbResult);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool GetConfigValueInfoDelegate(IntPtr instance, ESteamNetworkingConfigValue eValue, out IntPtr pOutName, out ESteamNetworkingConfigDataType pOutDataType, out ESteamNetworkingConfigScope pOutScope, out ESteamNetworkingConfigValue pOutNextValue);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate ESteamNetworkingConfigValue GetFirstConfigValueDelegate(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate void SteamNetworkingIPAddr_ToStringDelegate(IntPtr instance, in SteamNetworkingIPAddr addr, IntPtr buf, UIntPtr cbBuf, bool bWithPort);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool SteamNetworkingIPAddr_ParseStringDelegate(IntPtr instance, out SteamNetworkingIPAddr pAddr, InteropHelp.UTF8StringHandle pszStr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate void SteamNetworkingIdentity_ToStringDelegate(IntPtr instance, in SteamNetworkingIdentity identity, IntPtr buf, UIntPtr cbBuf);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall), SuppressUnmanagedCodeSecurity]
        public delegate bool SteamNetworkingIdentity_ParseStringDelegate(IntPtr instance, out SteamNetworkingIdentity pIdentity, InteropHelp.UTF8StringHandle pszStr);


#if STEAMNETWORKINGSOCKETS_ENABLE_SDR
        public readonly GetLocalPingLocationDelegate GetLocalPingLocation;
        public readonly EstimatePingTimeBetweenTwoLocationsDelegate EstimatePingTimeBetweenTwoLocations;
        public readonly EstimatePingTimeFromLocalHostDelegate EstimatePingTimeFromLocalHost;
        public readonly ConvertPingLocationToStringDelegate ConvertPingLocationToString;
        public readonly ParsePingLocationStringDelegate ParsePingLocationString;
        public readonly CheckPingDataUpToDateDelegate CheckPingDataUpToDate;
        public readonly IsPingMeasurementInProgressDelegate IsPingMeasurementInProgress;
        public readonly GetPingToDataCenterDelegate GetPingToDataCenter;
        public readonly GetDirectPingToPOPDelegate GetDirectPingToPOP;
        public readonly GetPOPCountDelegate GetPOPCount;
        public readonly GetPOPListDelegate GetPOPList;
#endif // #ifdef STEAMNETWORKINGSOCKETS_ENABLE_SDR                 

        public readonly GetLocalTimestampDelegate GetLocalTimestamp;
        public readonly SetDebugOutputFunctionDelegate SetDebugOutputFunction;
        public readonly SetConfigValueDelegate SetConfigValue;
        public readonly GetConfigValueDelegate GetConfigValue;
        public readonly GetConfigValueInfoDelegate GetConfigValueInfo;
        public readonly GetFirstConfigValueDelegate GetFirstConfigValue;
        public readonly SteamNetworkingIPAddr_ToStringDelegate SteamNetworkingIPAddr_ToString;
        public readonly SteamNetworkingIPAddr_ParseStringDelegate SteamNetworkingIPAddr_ParseString;
        public readonly SteamNetworkingIdentity_ToStringDelegate SteamNetworkingIdentity_ToString;
        public readonly SteamNetworkingIdentity_ParseStringDelegate SteamNetworkingIdentity_ParseString;

        public SteamNetworkingUtilsVTable(IntPtr nativeVTablePtr)
        {
            int methodIndex = 0;

#if STEAMNETWORKINGSOCKETS_ENABLE_SDR
            Add(nativeVTablePtr, ref methodIndex, out GetLocalPingLocation);
            Add(nativeVTablePtr, ref methodIndex, out EstimatePingTimeBetweenTwoLocations);
            Add(nativeVTablePtr, ref methodIndex, out EstimatePingTimeFromLocalHost);
            Add(nativeVTablePtr, ref methodIndex, out ConvertPingLocationToString);
            Add(nativeVTablePtr, ref methodIndex, out ParsePingLocationString);
            Add(nativeVTablePtr, ref methodIndex, out CheckPingDataUpToDate);
            Add(nativeVTablePtr, ref methodIndex, out IsPingMeasurementInProgress);
            Add(nativeVTablePtr, ref methodIndex, out GetPingToDataCenter);
            Add(nativeVTablePtr, ref methodIndex, out GetDirectPingToPOP);
            Add(nativeVTablePtr, ref methodIndex, out GetPOPCount);
            Add(nativeVTablePtr, ref methodIndex, out GetPOPList);
#endif // #ifdef STEAMNETWORKINGSOCKETS_ENABLE_SDR                 

            Add(nativeVTablePtr, ref methodIndex, out GetLocalTimestamp);
            Add(nativeVTablePtr, ref methodIndex, out SetDebugOutputFunction);
            Add(nativeVTablePtr, ref methodIndex, out SetConfigValue);
            Add(nativeVTablePtr, ref methodIndex, out GetConfigValue);
            Add(nativeVTablePtr, ref methodIndex, out GetConfigValueInfo);
            Add(nativeVTablePtr, ref methodIndex, out GetFirstConfigValue);
            Add(nativeVTablePtr, ref methodIndex, out SteamNetworkingIPAddr_ToString);
            Add(nativeVTablePtr, ref methodIndex, out SteamNetworkingIPAddr_ParseString);
            Add(nativeVTablePtr, ref methodIndex, out SteamNetworkingIdentity_ToString);
            Add(nativeVTablePtr, ref methodIndex, out SteamNetworkingIdentity_ParseString);
        }

        static void Add<T>(IntPtr nativeVTablePtr, ref int methodIndex, out T delegateVariable)
            where T : Delegate
        {
            IntPtr methodPtr = Marshal.ReadIntPtr(nativeVTablePtr, IntPtr.Size * methodIndex);
            delegateVariable = (T)Marshal.GetDelegateForFunctionPointer(methodPtr, typeof(T));
            methodIndex++;
        }

        static void Add(IntPtr nativeVTablePtr, ref int methodIndex, out IntPtr methodPointerVariable)
        {
            methodPointerVariable = Marshal.ReadIntPtr(nativeVTablePtr, IntPtr.Size * methodIndex);
            methodIndex++;
        }
    }
}