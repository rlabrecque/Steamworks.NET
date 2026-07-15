namespace Steamworks
{
	/// <summary>
	/// A marker that indicates implemented type is a async Steam callback result.
	/// Library consumer should not implement this interface.
	/// </summary>
	public interface ISteamCallbackIdentity
	{
		/// <summary>
		/// Steam async callback identity value.
		/// </summary>
		public static abstract int CallbackIdentity { get; }
	}
}
