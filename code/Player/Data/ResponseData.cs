using Sandbox;
using Amper.FPS;

namespace TFS2;

[GameResource( "TF:S2 Talker Script", "tftalker", "Defines the voice responses for TF2 characters.", Icon = "record_voice_over", IconBgColor = "#4287f5", IconFgColor = "#0e0e0e" )]
public partial class TFResponseData : ResponseData<TFResponseConcept, TFResponseContext>
{
	[ResourceType( "tftalker" )]
	public override string Base { get; set; }
}


public enum TFResponseContext
{
	LookAtClass,
	LookAtEnemy,
	OnFriendlyControlPoint,
	OnCappableControlPoint
}

public enum TFResponseConcept
{
	// Voice Menu 1
	VoiceMedic,
	VoiceThanks,
	VoiceGo,
	VoiceMoveUp,
	VoiceGoLeft,
	VoiceGoRight,
	VoiceYes,
	VoiceNo,

	// Voice Menu 2
	VoiceIncoming,
	VoiceSpy,
	VoiceSentryAhead,
	VoiceTeleporterHere,
	VoiceDispenserHere,
	VoiceSentryHere,
	VoiceActivateUbercharge,
	VoiceUberchargeReady,

	// Voice Menu 3
	VoiceHelp,
	VoiceBattleCry,
	VoiceCheers,
	VoiceJeers,
	VoicePositive,
	VoiceNegative,
	VoiceNiceShot,
	VoiceGoodJob,

	Pain
}
