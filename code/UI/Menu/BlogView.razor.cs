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
	public string Url { get; set; }

	public WebPanel WebPanel { get; set; }

	protected override void OnAfterTreeRender(bool firstTime)
	{
		if(string.IsNullOrEmpty(Url))
			Url = UNKNOWN_BLOG_URL;

		if(WebPanel?.Surface != null && WebPanel.Surface.Url != Url)
		{
			WebPanel.Surface.Url = Url;
		}
	}

	protected override int BuildHash()
	{
		// this will force a rebuild every time the date time string changes
		return HashCode.Combine( DateTime.Now.ToString(), Url );
	}

	protected void OnClickClose() => Close();
}
