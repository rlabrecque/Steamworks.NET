using Steamworks.CoreCLR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks.NET.Tests;

internal static class SpecialDebugMain
{
	static async Task AsyncMain(string[] args)
	{
		SteamAPICall_t handle = new(0x0000_1111_2222_3333);
		var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<SteamAPICallCompleted_t>());

		SteamAPICallCompleted_t desired = new()
		{
			m_cubParam = 42,
			m_hAsyncCall = handle,
			m_iCallback = 703
		};
		Marshal.StructureToPtr(desired, buffer, false);
		var dispatcher = new ValueTaskDispatcher();
		var result = dispatcher.Register<SteamAPICallCompleted_t>(handle);
		Thread sender = new(() =>
		{
			Thread.Sleep(200);
			dispatcher.UnitTestTriggerCompletion(handle, buffer, true);
		});
		sender.IsBackground = true;
		sender.Start();

		await result;

		Debugger.Break();
	}

	static void Main(string[] args) => AsyncMain(args).GetAwaiter().GetResult();
}
