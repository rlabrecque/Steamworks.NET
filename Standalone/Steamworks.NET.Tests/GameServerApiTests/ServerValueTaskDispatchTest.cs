using Steamworks.CoreCLR;
using System.Diagnostics;

namespace Steamworks.NET.Tests.GameServerApiTests;

public class ServerValueTaskDispatchTest
{
	private ulong modFAGE_id = 1579583001;


	[Test]
	public void TestServerAwait()
	{
		var queryHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest([new(modFAGE_id)], 1);
		Assert.ThatAsync(async () =>
		{
			var queryResult = await SteamGameServerUGC.SendQueryUGCRequest(queryHandle).AsTask<SteamUGCQueryCompleted_t>();


			Assert.That(queryResult.m_eResult, Is.EqualTo(EResult.k_EResultOK));

		}, Throws.Nothing.Or.InstanceOf<SteamIOFailureException>());
		// completionHolder[i].Set(queryApiCallHandle);
	}

	[Test]
	public void TestCallbackRegister()
	{
		Assert.Multiple(() =>
		{
			CallbackTest cbTest = new();

			cbTest.registration = ValueTaskCallback.RegisterServerCallback<GameWebCallback_t>(cbTest.TestMethod);
			ProcessStartInfo si = new(CallbackTest.TestSteamUrl)
			{
				UseShellExecute = true
			};
			Process.Start(si);
			Thread.Sleep(200);
			Process.Start(si);

		});
	}


	private class CallbackTest
	{
		public int CalledTimes = 0;
		public const string TestSteamUrl = "steam://snet-automated-test/callback-dispatch";

		public CallbackRegistration<GameWebCallback_t> registration;

		public void TestMethod(GameWebCallback_t v)
		{
			if (v.m_szURL == TestSteamUrl)
			{
				CalledTimes++;

				if (CalledTimes != 1)
				{
					Assert.Fail();
				}

				Assert.That(CalledTimes, Is.EqualTo(1));
				registration.Dispose();
			}
		}
	}

}
