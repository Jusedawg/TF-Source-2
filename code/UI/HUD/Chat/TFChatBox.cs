﻿using Sandbox.UI;
using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breaker;

namespace TFS2.UI
{
	public partial class TFChatBox : Panel, Logging.ILogger
	{
		public static TFChatBox Instance { get; set; }
		public bool IsOpen { get; set; }
		ChatType Type { get; set; }
		TextEntry TextField { get; set; }
		Panel MessagesScroll { get; set; }
		Panel MessagesContainer { get; set; }
		Label ChannelNameLabel { get; set; }
		Panel SwitchGlyph { get; set; }

		public TFChatBox() => Instance = this;

		[GameEvent.Client.BuildInput]
		public void ProcessClientInput()
		{
			if ( Input.Pressed( "Chat" ))
			{
				if ( TFClientSettings.Current.ShowTextChat )
					Open();
				else
					AddInformation( "Text chat is currently disabled. You can toggle this option in your settings." );
			}
		}

		public override void OnButtonEvent( ButtonEvent e )
		{
			base.OnButtonEvent( e );

			if ( e.Pressed )
			{
				if ( TimeSinceInteraction > 0.1f && e.Button == "tab" )
				{
					CycleChatType();
					TimeSinceInteraction = 0;
				}
			}
		}

		TimeSince TimeSinceInteraction { get; set; }


		public void CycleChatType()
		{
			Type++;
			if ( !Enum.IsDefined( typeof( ChatType ), Type ) )
				Type = 0;

			SetChatType( Type );

			Sound.FromScreen( "ui.button.rollover" );
		}

		public void SetChatType( ChatType type )
		{
			Type = type;
			ChannelNameLabel.Text = $"{Type}:";
		}

		public void Open()
		{
			AddClass( "focused" );
			IsOpen = true;
			TextField.Focus();
			MessagesScroll.TryScrollToBottom();

			// This resets all the UI elements to match our type in case we hotloaded the template and it reset.
			SetChatType( Type );
		}

		public void Close()
		{
			IsOpen = false;
			RemoveClass( "focused" );
			TextField.Blur();
			MessagesScroll.TryScrollToBottom();
		}

		public void Submit()
		{
			Close();

			var msg = TextField.Text.Trim();
			TextField.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Say( msg, Type );
		}

		[ClientRpc]
		public static void AddInformation( string message )
		{
			if ( Instance == null )
				return;

			var msg = new ColorFormattedString();
			msg.AddText( message );
			Instance.AddString( msg );
		}

		[ClientRpc]
		public static void AddClientMessage( IClient author, string message, ChatType type )
		{
			// Log.NetInfo( $"TFChatBox::AddClientMessage({author}, {message}, {type})" );
			if ( Instance == null )
				return;

			var msg = new ColorFormattedString();

			if ( author == null )
			{
				// author is the server
				msg.AddText( "Server" );
			}
			else
			{
				var team = author.GetTeam();
				var isAlive = author.IsAlive();

				// *SPEC*
				if ( !team.IsPlayable() )
					msg.AddText( "*SPEC* " );

				// *DEAD*
				if ( team.IsPlayable() && !isAlive )
					msg.AddText( "*DEAD* " );

				// (TEAM)
				if ( type == ChatType.Team )
					msg.AddText( "(TEAM) " );

				// Player Name
				switch ( team )
				{
					case TFTeam.Red: msg.AddTextWithClasses( author.Name, "red" ); break;
					case TFTeam.Blue: msg.AddTextWithClasses( author.Name, "blue" ); break;
					default: msg.AddTextWithClasses( author.Name, "spectator" ); break;
				}
			}

			msg.AddText( ": " );
			msg.AddText( message );

			Instance.AddString( msg );
		}

		[ClientRpc]
		public static void AddClientVoiceCommand( IClient author, string command )
		{
			// Log.NetInfo( $"TFChatBox::AddClientVoiceCommand({author}, {command}" );
			if ( Instance == null )
				return;

			if ( author == null )
				return;

			var msg = new ColorFormattedString();
			var team = author.GetTeam();

			if ( !team.IsPlayable() )
				return;

			// Player Name
			switch ( team )
			{
				case TFTeam.Red: msg.AddTextWithClasses( $"(Voice) {author.Name}", "red" ); break;
				case TFTeam.Blue: msg.AddTextWithClasses( $"(Voice) {author.Name}", "blue" ); break;
			}

			msg.AddText( ": " );
			msg.AddText( command );

			Instance.AddString( msg );
		}

		const int MAX_MESSAGES = 100;
		public void AddString( ColorFormattedString message )
		{
			// Check if the razor layout is done
			if ( GetRenderTreeChecksum() == null )
				return;

			var entry = new TFChatBoxEntry( message );
			MessagesContainer?.AddChild( entry );
			if ( MessagesContainer.ChildrenCount > MAX_MESSAGES )
				MessagesContainer?.GetChild( 0 )?.Delete();
		}

		public void Say( string message, ChatType type )
		{
			switch ( type )
			{
				case ChatType.Global:
					SDKGame.Command_SendMessage( message );
					break;

				case ChatType.Team:
					SDKGame.Command_SendTeamMessage( message );
					break;
			}
		}

		public override void Tick()
		{
			base.Tick();

			if ( !IsOpen && !MessagesScroll.IsScrollAtBottom )
				MessagesScroll.TryScrollToBottom();

			if ( TimeSinceInteraction < 0.05f )
			{
				if ( !SwitchGlyph.HasClass( "pressed" ) )
					SwitchGlyph.AddClass( "pressed" );
			}
			else
			{
				if ( SwitchGlyph.HasClass( "pressed" ) )
					SwitchGlyph.RemoveClass( "pressed" );
			}
		}

		#region ILogger
		bool Logging.ILogger.Server => false;

		bool Logging.ILogger.Client => true;

		void Logging.ILogger.Log( string message, MessageType type )
		{
			const string ERROR_COLOR = "#FF2222";
			const string ANNOUNCE_PREFIX_COLOR = "#FFDD33";
			const string ANNOUNCE_COLOR = "#FFBB22";

			ColorFormattedString msg = new();
			switch(type)
			{
				case MessageType.Error:
					msg.AddColoredText( message, ERROR_COLOR );
					break;
				case MessageType.Announcement:
					msg.AddColoredText("[ANNOUNCEMENT] ", ANNOUNCE_PREFIX_COLOR );
					msg.AddColoredText( message, ANNOUNCE_COLOR );
					break;
				default:
					msg.AddText( message );
					break;
			}

			AddString( msg );
		}

		[BRKEvent.ConfigLoaded]
		void OnConfigLoaded()
		{
			Logging.RegisterLogger( this );
		}
		#endregion
	}
}
