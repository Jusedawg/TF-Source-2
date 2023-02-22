using Sandbox;

namespace TFS2;

//
// This file contains weapons that are defined within 2 lines (library and class)
// so there is no need to have a bunch of C# classes for them in different files.
// In case a weapon needs extra logic make a different file.
//

#region Primary Weapons

[Library( "tf_weapon_scattergun", Title = "Scattergun" )]
public class Scattergun : Shotgun { }

[Library( "tf_weapon_revolver", Title = "Revolver" )]
public class Revolver : TFWeaponBase { }

#endregion

#region Secondary Weapons

[Library( "tf_weapon_shotgun", Title = "Shotgun" )]
public class Shotgun : TFWeaponBase { }

[Library( "tf_weapon_pistol", Title = "Pistol" )]
public class Pistol : TFWeaponBase { }

[Library( "tf_weapon_smg", Title = "SMG" )]
public class SMG : TFWeaponBase { }

#endregion

#region Melee Weapons

[Library( "tf_weapon_bat" )]
public class Bat : TFMeleeBase { }

[Library( "tf_weapon_shovel" )]
public class Shovel : TFMeleeBase { }

[Library( "tf_weapon_fireaxe" )]
public class FireAxe : TFMeleeBase { }

[Library( "tf_weapon_bonesaw" )]
public class BoneSaw : TFMeleeBase { }

[Library( "tf_weapon_kukri" )]
public class Kukri : TFMeleeBase { }

#endregion
