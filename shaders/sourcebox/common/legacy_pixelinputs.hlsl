#ifndef SOURCEBOX_LEGACY_PIXELINPUTS_H
#define SOURCEBOX_LEGACY_PIXELINPUTS_H

#ifdef COMMON_PIXEL_MATERIAL_INPUTS_H
    #error Material inputs included!
#endif // COMMON_PIXEL_MATERIAL_INPUTS_H

// addressing mode is per-texture now
#define CUSTOM_TEXTURE_FILTERING

CreateInputTexture2D( Color, Linear, 8, "", "_color", "Material,10/10", Default4( 1.0, 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tColor ) < Channel( RGBA, Box( Color ), Linear ); OutputFormat( BC7 ); SrgbRead( true ); >;
// int g_nColorHDR	< UiGroup( "Attributes,11/2" ); Default(0); Range(0, 1); >;
float4 CONVERT_COLOR(float4 s) { return s; }

CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Material,10/11", Default3( 0.5, 0.5, 1.0 ) );
CreateTexture2D( g_tNormal ) < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

CreateInputTextureCube( EnvMap, Linear, 8, "", "", "Material,10/12", Default3( 1.0, 1.0, 1.0 ) );
// CreateTextureCube( g_tEnvMap ) < Channel( RGBA, Box( EnvMap ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( F_ENVMAP_HDR ? false : true ); >;
// To support HDR envmaps, we need linear reading and a fp output format
CreateTextureCube( g_tEnvMap ) < Channel( RGBA, Box( EnvMap ), Linear ); OutputFormat( RGB323232F ); SrgbRead( true ); >;
float4 CONVERT_ENVMAP(float4 s) { return s; }

CreateInputTexture2D( EnvMapMask, Linear, 8, "", "", "Material,10/14", Default3( 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tEnvMapMask ) < Channel( RGB, Box( EnvMapMask ), Linear ); OutputFormat( DXT1 ); SrgbRead( false ); >;

//CreateTextureCube( g_tEnvMap ) < Channel( RGB, Box( EnvMap ), Srgb ); OutputFormat( DXT1 ); SrgbRead( true ); >;
CreateInputTexture2D( LightmapTexture, Linear, 8, "", "", "Material,10/14", Default3( 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tLightmapTexture ) < Channel( RGBA, Box( LightmapTexture ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); AddressU( CLAMP ); AddressV( CLAMP ); Filter( TRILINEAR ); >;

// both warp textures must be RGB!
// love that this doesn't work for procedural stuff (unless i missed something)
// CreateInputTexture1D( LightWarpTexture, Linear, 8, "", "", "Material,10/15", Default3( 1.0, 1.0, 1.0 ) );
// CreateTexture1D( g_tLightWarpTexture ) < Channel( RGB, Box( LightWarpTexture ), Linear ); OutputFormat( RGB323232F ); SrgbRead( false ); >;
CreateInputTexture2D( LightWarpTexture, Linear, 8, "", "", "Material,10/15", Default3( 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tLightWarpTexture ) < Channel( RGB, Box( LightWarpTexture ), Linear ); OutputFormat( RGB323232F ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( false ); >;

// phongwarp texture
CreateInputTexture2D( SpecularWarpTexture, Linear, 8, "", "", "Material,10/16", Default3( 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tSpecularWarpTexture ) < Channel( RGB, Box( SpecularWarpTexture ), Linear ); OutputFormat( RGB323232F ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( false ); >;

CreateInputTexture2D( SpecularExponentTexture, Linear, 8, "", "", "Material,10/17", Default3( 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tSpecularExponentTexture ) < Channel( RGB, Box( SpecularExponentTexture ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); >;

CreateInputTexture2D( SelfIllumMaskTexture, Linear, 8, "", "", "Material,10/18", Default3( 1.0, 1.0, 1.0 ) );
CreateTexture2D( g_tSelfIllumMaskTexture ) < Channel( RGB, Box( SelfIllumMaskTexture ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); >;

#ifdef S_DETAILTEXTURE
    CreateInputTexture2D( DetailTexture, Linear, 8, "", "", "Material,10/19", Default3( 1.0, 1.0, 1.0 ) );

    #if (S_DETAIL_BLEND_MODE == 0)
        CreateTexture2D( g_tDetailTexture ) < Channel( RGB, Box( DetailTexture ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
    #else // (S_DETAIL_BLEND_MODE != 0)
        CreateTexture2D( g_tDetailTexture ) < Channel( RGB, Box( DetailTexture ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
    #endif // (S_DETAIL_BLEND_MODE != 0)

    float g_flDetailBlendFactor         < UiGroup( "Attributes,11/16" ); Default(1.0f); >;

    // int g_nDetailHDR	< UiGroup( "Attributes,11/2" ); Default(0); >;
    // float4 CONVERT_DETAIL(float4 s) { return float4(g_nDetailHDR ? s.rgb : pow(s.rgb, 2.2f), s.a); }
    float4 CONVERT_DETAIL(float4 s) { return s; }

#endif // S_DETAIL_TEXTURE

#ifdef S_WRINKLEMAP
    CreateInputTexture2D( Wrinkle, Linear, 8, "", "_wrinkle", "Material,10/100", Default3( 1.0, 1.0, 1.0 ) );
    CreateInputTexture2D( Stretch, Linear, 8, "", "_wrinklestretch", "Material,10/101", Default3( 1.0, 1.0, 1.0 ) );
    CreateInputTexture2D( WrinkleNormal, Linear, 8, "NormalizeNormals", "_wrinklenormal", "Material,10/102", Default3( 0.5, 0.5, 1.0 ) );
    CreateInputTexture2D( WrinkleStretchNormal, Linear, 8, "NormalizeNormals", "_wrinklestretchnormal", "Material,10/103", Default3( 0.5, 0.5, 1.0 ) );

    CreateTexture2D( g_tWrinkle ) < Channel( RGB, Box( Wrinkle ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
    CreateTexture2D( g_tStretch ) < Channel( RGB, Box( Stretch ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
    CreateTexture2D( g_tWrinkleNormal ) < Channel( RGBA, Box( WrinkleNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
    CreateTexture2D( g_tWrinkleStretchNormal ) < Channel( RGBA, Box( WrinkleStretchNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
#endif // S_WRINKLEMAP

float g_flSpecularBoost             < UiGroup( "Attributes,11/8" ); Default(1.0f); >;
float3 g_vSpecularTint              < UiType( Color ); UiGroup( "Attributes,11/9" ); Default3(1.0f, 1.0f, 1.0f); >;
float g_flRimBoost                  < UiGroup( "Attributes,11/10" ); Default(1.0f); >;
float g_flTintReplacementControl    < UiGroup( "Attributes,11/11" ); Default(1.0f); >;
float g_flEnvMapScale               < UiGroup( "Attributes,11/12" ); Default(1.0f); >;
float3 g_vEnvMapTint                < UiType( Color ); UiGroup( "Attributes,11/13" ); Default3(1.0f, 1.0f, 1.0f); >;
float3 g_vEnvMapContrast            < UiGroup( "Attributes,11/14" ); Default3(0.0f, 0.0f, 0.0f); >;
float3 g_vEnvMapSaturation          < UiGroup( "Attributes,11/15" ); Default3(1.0f, 1.0f, 1.0f); >;
float4 g_vDiffuseModulation         < UiType( Color ); UiGroup( "Attributes,11/16" ); Default4(1.0f, 1.0f, 1.0f, 1.0); >;
float g_flEnvMapShadowTweaks        < UiGroup( "Attributes,11/17" ); Default(1.0f); >;

// already defined somewhere?
float g_flSelfIllumScale            < UiGroup( "Attributes,11/18" ); Default(1.0f); >;
float g_flSelfIllumBias             < UiGroup( "Attributes,11/19" ); Default(0.0f); >;
float g_flSelfIllumExponent         < UiGroup( "Attributes,11/20" ); Default(1.0f); >;
float g_flSelfIllumBrightness       < UiGroup( "Attributes,11/21" ); Default(1.0f); >;
float3 g_vSelfIllumTint             < UiType( Color ); UiGroup( "Attributes,11/22" ); Default3(1.0f, 1.0f, 1.0f); >;

float g_flBaseMapAlphaPhongMask     < UiGroup( "Attributes,11/23" ); Default(0.0f); >;
float g_fInvertPhongMask            < UiGroup( "Attributes,11/24" ); Default(0.0f); >;

// Simplified for TF:S2, use the remap equation for fresnel ranges
// Change fresnel range encoding from (min, mid, max) to ((mid-min)*2, mid, (max-mid)*2)
// float3 g_vFresnelRanges             < UiGroup( "Attributes,11/25" ); Default3(0.0f, 0.0f, 0.0f); >;
float3 g_vSourceFresnelRanges             < UiGroup( "Attributes,11/25" ); Default3(0.0f, 0.0f, 0.0f); >;
#define g_vFresnelRanges float3((g_vSourceFresnelRanges.y - g_vSourceFresnelRanges.x)*2,g_vSourceFresnelRanges.y,(g_vSourceFresnelRanges.z-g_vSourceFresnelRanges.y)*2)

float g_flEnvMapFresnel             < UiGroup( "Attributes,11/26" ); Default(0.0f); >;

float g_flRimMask                   < UiGroup( "Attributes,11/27" ); Default(0.0f); >;
// use texture if == 0.0f
float g_flSpecularExponent          < UiGroup( "Attributes,11/28" ); Default(1.0f); >;

float g_flSelfIllumMaskControl      < UiGroup( "Attributes,11/29" ); Default(1.0f); >;
float g_flRimExponent               < UiGroup( "Attributes,11/30" ); Default(1.0f); >;


float4 g_vModulationColor           < UiType( Color ); UiGroup( "Attributes,11/31" ); Default4(1.0f, 1.0f, 1.0f, 1.0); >;

#endif // SOURCEBOX_LEGACY_PIXELINPUTS_H