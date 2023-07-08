using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TFS2.Menu;

public partial class BlogView : MenuOverlay
{
	const string UNKNOWN_BLOG_URL = "https://tfsource2.com/news";
	public string Url
	{
		get { return WebPanel?.Surface?.Url ?? ""; }
		set { if ( WebPanel?.Surface?.Url != null ) WebPanel.Surface.Url = value; }
	}

	public WebPanel WebPanel { get; set; }

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			WebPanel.Surface.Url = UNKNOWN_BLOG_URL;
		}
	}

	protected override int BuildHash()
	{
		// this will force a rebuild every time the date time string changes
		return HashCode.Combine( DateTime.Now.ToString() );
	}

	protected void OnClickClose() => Close();
}
