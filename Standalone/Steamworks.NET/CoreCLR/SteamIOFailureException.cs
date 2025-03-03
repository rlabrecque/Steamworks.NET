using System;

namespace Steamworks.CoreCLR {

	[Serializable]
	public class SteamIOFailureException : Exception {
		public SteamIOFailureException() { }
		public SteamIOFailureException(string message, ESteamAPICallFailure reason) : base(message) {
			Reason = reason;
		}
		public SteamIOFailureException(string message, ESteamAPICallFailure reason, Exception inner) : base(message, inner) {
			Reason = reason;
		}

		[Obsolete]
		protected SteamIOFailureException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		public ESteamAPICallFailure Reason { get; }
	}
}