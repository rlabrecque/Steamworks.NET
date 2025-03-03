using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests.GameServerApiTests
{
	[SetUpFixture]
	public class GSSteamApiManager
	{
		private CancellationTokenSource dispatchThreadStop;
		private Thread dispatchThread;

		[OneTimeSetUp]
		public void SteamInit()
		{
			Assert.That(GameServer.Init(0, 27016, 27017, EServerMode.eServerModeNoAuthentication, "S.NET Automated Test"), Is.True);
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
			// Assert.That(SteamAPI.Init(), Is.True);
		}

		[OneTimeTearDown]
		public void SteamShutdown()
		{
			dispatchThreadStop.Cancel();
			dispatchThreadStop.Dispose();
			GameServer.Shutdown();
		}

	}
}
