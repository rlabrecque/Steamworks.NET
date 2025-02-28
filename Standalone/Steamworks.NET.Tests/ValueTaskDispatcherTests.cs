using Steamworks.CoreCLR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests;

[TestFixture]
public class ValueTaskDispatcherTests
{
	private ValueTaskDispatcher dispatcher;
	private ulong modFAGE_id = 1579583001;
	private CancellationTokenSource dispatchThreadStop;
	private Thread dispatchThread = null!;

	[OneTimeSetUp]
	public void SetupOnce()
	{
		SteamAPI.Init();
		dispatcher = ValueTaskDispatcher.Singleton;
		dispatchThreadStop = new();
		dispatchThread = new(() =>
		{
			while (!dispatchThreadStop.IsCancellationRequested)
			{
				SteamAPI.RunCallbacks();
			}
		})
		{
			Name = "Steamworks.NET dispatcher thread",
			IsBackground = true
		};
		dispatchThread.Start();
	}

	[OneTimeTearDown]
	public void TeardownOnce()
	{
		dispatchThreadStop.Cancel();
		dispatchThreadStop.Dispose();
		SteamAPI.Shutdown();
	}


	[SetUp]
	public void Setup()
	{

	}


	[Test]
	public void TestAwait()
	{
		var queryHandle = SteamUGC.CreateQueryUGCDetailsRequest([new(modFAGE_id)], 1);
		Assert.ThatAsync(async () =>
		{
			var queryResult = await SteamUGC.SendQueryUGCRequest(queryHandle).AsTask<SteamUGCQueryCompleted_t>();


			Assert.That(queryResult.m_eResult, Is.EqualTo(EResult.k_EResultOK));

		}, Throws.Nothing.Or.InstanceOf<SteamIOFailureException>());
		// completionHolder[i].Set(queryApiCallHandle);
	}

	[Test]
	public void TestServerAwait()
	{
		var queryHandle = SteamUGC.CreateQueryUGCDetailsRequest([new(modFAGE_id)], 1);
		Assert.ThatAsync(async () =>
		{
			var queryResult = await SteamGameServerUGC.SendQueryUGCRequest(queryHandle).AsServerTask<SteamUGCQueryCompleted_t>();


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

			ValueTaskCallback.RegisterCallback<GameWebCallback_t>(cbTest.TestMethod);
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
				ValueTaskCallback.UnregisterCallback<GameWebCallback_t>(TestMethod);
			}
		}
	}
}
