using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests.ServerAPITests
{
	[SetUpFixture]
	public class SteamManager
	{
		[OneTimeSetUp]
		public void SteamInit()
		{
			Assert.That(GameServer.Init(0, 27016, 27017, EServerMode.eServerModeNoAuthentication, "S.NET Automated Test"), Is.True);
			// Assert.That(SteamAPI.Init(), Is.True);
		}

		[OneTimeTearDown]
		public void SteamShutdown()
		{
			GameServer.Shutdown();
			// SteamAPI.Shutdown();
		}

	}
}
