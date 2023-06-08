using Sandbox;
using System;
using Sandbox.Diagnostics;

namespace TFS2;

public static class TFAssert
{
	public static void ClientOrGameMenu()
	{
		Assert.True( !Game.InGame || Game.IsClient );
	}
}
