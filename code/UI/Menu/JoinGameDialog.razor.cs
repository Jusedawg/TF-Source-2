﻿using Sandbox;
using Sandbox.UI;

namespace TFS2.Menu;

public partial class JoinGameDialog : MenuOverlay
{
	public string SearchText { get; set; } = "";

	public void OnClickCancel()
	{
		Close();
	}

	public void OnClickConnect()
	{
		ConsoleSystem.Run( $"connect {SearchText}" );
	}
}
