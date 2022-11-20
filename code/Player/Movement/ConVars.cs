using Sandbox;

namespace TFS2;

partial class TFGameMovement
{
	[ConVar.Replicated] public static bool tf_duck_debug_spew { get; set; }
	[ConVar.Replicated] public static bool tf_showspeed { get; set; }
	[ConVar.Replicated] public static bool tf_collide_with_teammates { get; set; }
	[ConVar.Replicated] public static bool tf_avoidteammates_pushaway { get; set; } = true;
	[ConVar.Replicated] public static bool tf_solidobjects { get; set; } = true;
	[ConVar.Replicated] public static float tf_clamp_back_speed { get; set; } = 0.9f;
	[ConVar.Replicated] public static float tf_clamp_back_speed_min { get; set; } = 100;
	[ConVar.Replicated] public static bool tf_clamp_airducks { get; set; } = true;
	[ConVar.Replicated] public static bool tf_resolve_stuck_players { get; set; } = true;
	[ConVar.Replicated] public static float tf_scout_hype_mod { get; set; } = 55;
	[ConVar.Replicated] public static float tf_max_charge_speed { get; set; } = 750;

	[ConVar.Replicated] public static float cl_forwardspeed { get; set; } = 450;
	[ConVar.Replicated] public static float cl_backspeed { get; set; } = 450;
	[ConVar.Replicated] public static float cl_sidespeed { get; set; } = 450;
}
