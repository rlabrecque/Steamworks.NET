using NUnit.Framework.Constraints;
using Steamworks;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests;

[TestFixture]
public class DispatcherTests
{
	private ulong modFAGE_id = 1579583001;
	private CancellationTokenSource dispatchThreadStop;
	private Thread dispatchThread = null!;
	private CallResult<SteamUGCQueryCompleted_t> callresult;
	private Callback<GameWebCallback_t> callback;

	[OneTimeSetUp]
	public void SetupOnce()
	{
		SteamAPI.Init();
		// dispatcher = ValueTaskDispatcher.Singleton;
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

	[TearDown]
	public void Teardown()
	{
		callresult.Dispose();
	}

	[Test]
	public void TestAwait()
	{
		var queryHandle = SteamUGC.CreateQueryUGCDetailsRequest([new(modFAGE_id)], 1);
		var handle = SteamUGC.SendQueryUGCRequest(queryHandle);

		callresult = new CallResult<SteamUGCQueryCompleted_t>();
		Barrier barrier = new(2);
		callresult.Set(handle, (queryResult, fail) =>
		{
			if (fail)
			{
				Assert.Pass();
			}

			Assert.That(queryResult.m_eResult, Is.EqualTo(EResult.k_EResultOK));
			barrier.SignalAndWait(2);
		});
		barrier.SignalAndWait(2);
	}

	[Test]
	public void TestCallback()
	{
		Assert.Multiple(() =>
		{
			CallbackTest cbTest = new();

			callback = new Callback<GameWebCallback_t>((w) => { 
				if (w.m_szURL == CallbackTest.TestSteamUrl)
				{
					cbTest.CalledTimes++;
				}
				callback.Unregister();
			});
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
	}
}
