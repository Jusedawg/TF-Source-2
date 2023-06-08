using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.Menu;

internal class Music
{
	public struct Track
	{
		public string SoundName { get; set; }
		public int MinPlays { get; set; }
		public float Chance { get; set; }

		public Track( string soundName, int minPlays, float chance )
		{
			SoundName = soundName;
			MinPlays = minPlays;
			Chance = chance;
		}

		public override string ToString()
		{
			return SoundName;
		}
	}

	const float MIN_TRACK_PAUSE = 60f;
	const float TRACK_CHANCE_INTERVAL = 10f;

	static List<Track> _tracks = new()
	{
		new("music.main_menu.1", 0, 0.7f),
		new("music.main_menu.2", 0, 0.45f),
		new("music.main_menu.4", 1, 0.2f),
		new("music.main_menu.7", 1, 0.2f),
		new("music.main_menu.8", 1, 0.2f),
		new("music.main_menu.6", 1, 0.2f),
		new("music.main_menu.3", 2, 0.05f),
		new("music.main_menu.5", 3, 0.01f)
	};

	public bool Enabled { get; set; } = true;
	List<Track> playedTracks = new();
	float timeSinceLastTrack;
	SoundHandle currentTrack;
	bool isPlaying;

	public Music()
	{
		timeSinceLastTrack = 0;
	}

	public void Tick()
	{
		if ( !Enabled ) return;

		if ( isPlaying)
		{
			if( currentTrack.IsPlaying )
			{
				currentTrack.Volume = TFClientSettings.Current.MenuMusicVolume;
				return;
			}
			Stop();
		}

		timeSinceLastTrack += Time.Delta;
		if((timeSinceLastTrack >= MIN_TRACK_PAUSE || playedTracks.Count == 0) && (timeSinceLastTrack % TRACK_CHANCE_INTERVAL).AlmostEqual( 0, Game.TickInterval + 0.01f ) )
		{
			var pick = Game.Random.FromList( _tracks.Where(t => playedTracks.Count >= t.MinPlays).ToList() );
			Log.Info( $"Music play attempt: picked {pick}" );

			if ( Game.Random.NextSingle() < pick.Chance )
			{
				Log.Info( "Play sucess!" );
				PlayTrack( pick );
			}
		}
	}

	void PlayTrack(Track t)
	{
		currentTrack = Audio.Play( t.SoundName );
		currentTrack.ListenLocal = true;
		playedTracks.Add( t );
		isPlaying = true;
	}

	public void Stop()
	{
		if(!currentTrack.Equals( default ))
			currentTrack.Stop(true);
		currentTrack = default;

		isPlaying = false;
		timeSinceLastTrack = 0;
	}
}
