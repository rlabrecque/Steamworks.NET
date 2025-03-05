#if NET8_0_OR_GREATER
using Steamworks.CoreCLR;
using System.Runtime.InteropServices;

namespace Steamworks.NET.Tests.ClientApiTests;

[TestFixture]
public class ValueTaskCallResultSourceTests
{
	private ValueTask<SteamAPICallCompleted_t> result;
	private ValueTaskDispatcher dispatcher;
	private SteamAPICall_t handle = new(0x0000_1111_2222_3333);

	private nint buffer;

	[OneTimeSetUp]
	public void SetupOnce()
	{
		buffer = Marshal.AllocHGlobal(Marshal.SizeOf<SteamAPICallCompleted_t>());
		SteamAPICallCompleted_t desired = new()
		{
			m_cubParam = 42,
			m_hAsyncCall = handle,
			m_iCallback = 703
		};
		Marshal.StructureToPtr(desired, buffer, false);
	}

	[OneTimeTearDown]
	public void CleanupOnce()
	{
		Marshal.FreeHGlobal(buffer);
		buffer = 0;
	}

	[SetUp]
	public void Setup()
	{
		dispatcher = new ValueTaskDispatcher();
		result = dispatcher.Register<SteamAPICallCompleted_t>(handle);
	}

	[Test]
	public async Task TestSuccess()
	{
		SpawnSenderThread(false);
		var answer = await result;
		Assert.Multiple(() =>
		{
			Assert.That(answer.m_cubParam, Is.EqualTo(42));
			Assert.That(answer.m_hAsyncCall, Is.EqualTo(handle));
			Assert.That(answer.m_iCallback, Is.EqualTo(703));
		});
	}

	[Test]
	public void TestIOFail()
	{
		SpawnSenderThread(true);

		Assert.That(async () =>
		{
			_ = await result;
		}, Throws.TypeOf<SteamIOFailureException>());
	}

	void SpawnSenderThread(bool ioFail)
	{
		Thread sender = new(() =>
		{
			Thread.Sleep(200);
			dispatcher.NotifyCompletion(handle, false, buffer, ioFail);
		});
		sender.IsBackground = true;
		sender.Start();
	}
}
#endif