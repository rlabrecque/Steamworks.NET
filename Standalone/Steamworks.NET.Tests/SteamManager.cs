using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests
{
	[SetUpFixture]
	public class SteamManager
	{
		[OneTimeSetUp]
		public void SteamInit() => Assert.That(SteamAPI.Init(), Is.True);

		[OneTimeTearDown]
		public void SteamShutdown() => SteamAPI.Shutdown();

	}
}
