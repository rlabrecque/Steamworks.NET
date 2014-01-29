namespace Steamworks {
	public struct ScreenshotHandle {
		public static readonly ScreenshotHandle Invalid = new ScreenshotHandle(0);
		public uint m_ScreenshotHandle;

		public ScreenshotHandle(uint value) {
			m_ScreenshotHandle = value;
		}

		public override string ToString() {
			return m_ScreenshotHandle.ToString();
		}
	}
}
