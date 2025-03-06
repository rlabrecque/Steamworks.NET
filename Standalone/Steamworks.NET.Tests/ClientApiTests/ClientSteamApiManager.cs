using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests.ClientApiTests
{
	[SetUpFixture]
	public class ClientSteamApiManager
	{
		private CancellationTokenSource dispatchThreadStop;
		private Thread dispatchThread;

		[OneTimeSetUp]
		public void SteamInit()
		{
			var inited = SteamAPI.Init();
			Assert.That(inited, Is.True);
			dispatchThreadStop = new();
			dispatchThread = new(() =>
			{
				while (!dispatchThreadStop.IsCancellationRequested)
				{
					SteamAPI.RunCallbacks();
					Thread.Sleep(100); // valve recommended frequency
				}
			})
			{
				Name = "Steamworks.NET dispatcher thread",
				IsBackground = true
			};
			dispatchThread.Start();
		}

		[OneTimeTearDown]
		public void SteamShutdown()
		{
			dispatchThreadStop.Cancel();
			dispatchThreadStop.Dispose();
			SteamAPI.Shutdown();
		}

	}
}