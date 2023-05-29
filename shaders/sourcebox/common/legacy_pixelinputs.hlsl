#ifndef SOURCEBOX_LEGACY_PIXELINPUTS_H
#define SOURCEBOX_LEGACY_PIXELINPUTS_H

#ifdef COMMON_PIXEL_MATERIAL_INPUTS_H
    #error Material inputs included!
#endif // COMMON_PIXEL_MATERIAL_INPUTS_H

// addressing mode is per-texture now
#define CUSTOM_TEXTURE_FILTERING


CreateInputTexture2D( Color,            Srgb,   8, "",                  "_color",   "Material,10/10",   Default3( 1.0, 1.0, 1.0 ) );
CreateInputTexture2D( Translucency,     Linear, 8, "",                  "_trans",   "Material,10/11",   Default( 1.0 ) );
CreateInputTexture2D( Normal,           Linear, 8, "NormalizeNormals",  "_normal",  "Material,10/12",   Default3( 0.5, 0.5, 1.0 ) );
CreateInputTexture2D( LightWarpTexture, Linear, 8, "",                  "_lightwarp","Material,10/13",  Default3( 1.0, 1.0, 1.0 ) );

#if AO_TEXTURE_IS_SRGB
    CreateInputTexture2D( AmbientOcclusion, Srgb, 8, "",                  "_ao",      "Material,10/Ambient Occlusion,14/1",   Default( 1.0 ) );
#else // !AO_TEXTURE_IS_SRGB
    CreateInputTexture2D( AmbientOcclusion, Linear, 8, "",                  "_ao",      "Material,10/Ambient Occlusion,14/1",   Default( 1.0 ) );
#endif // AO_TEXTURE_IS_SRGB

float g_flAmbientOcclusionDirectDiffuse         < UiGroup( "Material,10/Ambient Occlusion,14/2" ); Range(0.0f, 1.0f); Default(1.0f); >;
float g_flAmbientOcclusionDirectPostLightwarp   < UiGroup( "Material,10/Ambient Occlusion,14/3" ); Range(0.0f, 1.0f); Default(1.0f); >;
float g_flAmbientOcclusionDirectSpecular        < UiGroup( "Material,10/Ambient Occlusion,14/4" ); Range(0.0f, 1.0f); Default(1.0f); >;
float g_flAmbientOcclusionDirectAmbient         < UiGroup( "Material,10/Ambient Occlusion,14/5" ); Range(0.0f, 1.0f); Default(1.0f); >;

CreateInputTexture2D( TintMask,      Linear,   8, "",                  "_tint",    "Tint,20/10",       Default( 1.0 ) );
float g_flTintReplacementControl    < UiGroup( "Tint,20/11" ); Range(0.0f, 1.0f); Default(1.0f); >;
float4 g_vDiffuseModulation         < UiGroup( "Tint,20/12" ); UiType( Color ); Default4(1.0f, 1.0f, 1.0f, 1.0); >;
float4 g_vModulationColor           < UiGroup( "Tint,20/13" ); UiType( Color ); Default4(1.0f, 1.0f, 1.0f, 1.0); >;

CreateInputTexture2D( DetailTexture, Linear, 8, "", "_detail", "Detail,30/10", Default4( 1.0, 1.0, 1.0, 1.0 ) );
float g_flDetailBlendFactor         < UiGroup( "Detail,30/11" ); Range(0.0f, 1.0f); Default(1.0f); >;


CreateInputTexture2D( SelfIllumMaskTexture, Linear, 8, "", "_selfillum", "Self Illum,40/10", Default3( 1.0, 1.0, 1.0 ) );
float3 g_vSelfIllumFresnelMinMaxExp < UiGroup( "Self Illum,40/11" ); Range3(0.0f, 0.0f, 0.0f, 10.0f, 10.0f, 10.0f); Default3(0.0f, 1.0f, 1.0f); >;
#define g_vSelfIllumFresnelMin g_vSelfIllumFresnelMinMaxExp.r
#define g_vSelfIllumFresnelMax g_vSelfIllumFresnelMinMaxExp.g
#define g_vSelfIllumFresnelExp g_vSelfIllumFresnelMinMaxExp.b

#define g_flSelfIllumBias (( g_vSelfIllumFresnelMax != 0.0f ) ? ( g_vSelfIllumFresnelMin / g_vSelfIllumFresnelMax ) : 0.0f)
#define g_flSelfIllumScale (1.0 - g_flSelfIllumBias)
#define g_flSelfIllumExponent g_vSelfIllumFresnelExp
#define g_flSelfIllumBrightness g_vSelfIllumFresnelMax

float3 g_vSelfIllumTint             < UiGroup( "Self Illum,40/15" ); UiType( Color ); Default3(1.0f, 1.0f, 1.0f); >;
bool g_bSelfIllumMaskControl        < UiGroup( "Self Illum,40/16" ); Default(1); >;

CreateInputTextureCube( EnvMap,     Srgb, 8, "", "_cube",          "Envmap,50/10", Default3( 1.0, 1.0, 1.0 ) );
CreateInputTexture2D(   EnvmapMask, Linear, 8, "", "_envmapmask",   "Envmap,50/11", Default( 1.0 ) );
float g_flEnvMapScale               < UiGroup( "Envmap,50/12" ); Range(0.0f, 4.0f); Default(1.0f); >;
float3 g_vEnvMapTint                < UiGroup( "Envmap,50/13" ); UiType( Color ); Default3(1.0f, 1.0f, 1.0f); >;
float3 g_vEnvMapContrast            < UiGroup( "Envmap,50/14" ); Range3(0.0f, 0.0f, 0.0f, 4.0f, 4.0f, 4.0f); Default3(0.0f, 0.0f, 0.0f); >;
float3 g_vEnvMapSaturation          < UiGroup( "Envmap,50/15" ); Range3(0.0f, 0.0f, 0.0f, 4.0f, 4.0f, 4.0f); Default3(1.0f, 1.0f, 1.0f); >;
bool g_bEnvMapShadowTweaks          < UiGroup( "Envmap,50/17" ); Default(1); >;

CreateInputTexture2D( SpecMask,                 Linear, 8, "", "_spec", "Specular,60/Spec Mask,10/10", Default( 1.0 ) );
// Use the "Color" alpha instead of the normal alpha (SpecMask)
bool g_bBaseMapAlphaPhongMask       < UiGroup( "Specular,60/Spec Mask,10/11" ); Default(0); >;
float g_fInvertPhongMask            < UiGroup( "Specular,60/Spec Mask,10/12" ); Range(0.0f, 1.0f); Default(0.0f); >;

CreateInputTexture2D( SpecularExponentTexture,  Linear, 8, "", "_specexp",      "Specular,60/Spec Exponent,20/10", Default( 1.0 ) );
bool g_bConstantSpecularExponent    < UiGroup( "Specular,60/Spec Exponent,20/11" ); Default(1); >;
float g_flSpecularExponent          < UiGroup( "Specular,60/Spec Exponent,20/12" ); Range(0.0f, 255.0f); Default(20.0f); >;

CreateInputTexture2D( SpecularTintTexture,      Linear, 8, "", "_spectint",      "Specular,60/Spec Tint,30/10", Default( 1.0 ) );
bool g_bConstantSpecularTint        < UiGroup( "Specular,60/Spec Tint,30/11" ); Default(1); >;
float3 g_vSpecularTint              < UiGroup( "Specular,60/Spec Tint,30/12" ); UiType( Color ); Default3(1.0f, 1.0f, 1.0f); >;

CreateInputTexture2D( SpecularWarpTexture,      Linear, 8, "", "_phongwarp",      "Specular,60/60", Default3( 1.0, 1.0, 1.0 ) );
float g_flSpecularBoost             < UiGroup( "Specular,60/61" ); Range(0.0f, 20.0f); Default(1.0f); >;

// Simplified for TF:S2, use the remap equation for fresnel ranges
// Change fresnel range encoding from (min, mid, max) to ((mid-min)*2, mid, (max-mid)*2)
// float3 g_vFresnelRanges             < UiGroup( "Specular,60/62" ); Default3(0.0f, 0.0f, 0.0f); >;
float3 g_vSourceFresnelRanges             < UiGroup( "Specular,60/62" ); Default3(0.0f, 0.0f, 0.0f); >;
#define g_vFresnelRanges float3((g_vSourceFresnelRanges.y - g_vSourceFresnelRanges.x)*2,g_vSourceFresnelRanges.y,(g_vSourceFresnelRanges.z-g_vSourceFresnelRanges.y)*2)

float g_flEnvMapFresnel             < UiGroup( "Specular,60/63" ); Range(0.0f, 1.0f); Default(0.0f); >;

CreateInputTexture2D( RimMaskTexture, Linear, 8, "", "_rimmask", "Rimlight,70/10", Default( 1.0 ) );
float g_flRimBoost                  < UiGroup( "Rimlight,70/11" ); Range(0.0f, 4.0f); Default(1.0f); >;
float g_flRimMask                   < UiGroup( "Rimlight,70/12" ); Range(0.0f, 1.0f); Default(0.0f); >;
float g_flRimExponent               < UiGroup( "Rimlight,70/13" ); Range(0.0f, 20.0f); Default(1.0f); >;

CreateInputTexture2D( DistAlphaMask, Linear, 8, "", "_dist", "Distance Alpha,80/10", Default( 1.0 ) );



// determine the base color alpha behavior
#ifdef BASE_COLOR_ALPHA_NAME
    CreateTexture2D( g_tColor ) < Channel( RGB, Box( Color ), Srgb ); Channel( A, Box( BASE_COLOR_ALPHA_NAME ), Linear ); OutputFormat( BC7 ); SrgbRead( true ); >;
#else
    // otherwise, assume packed data
    CreateTexture2D( g_tColor ) < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
#endif // BASE_COLOR_ALPHA_NAME


// int g_nColorHDR	< UiGroup( "Attributes,11/2" ); Default(0); Range(0, 1); >;
float4 CONVERT_COLOR(float4 s) { return s; }

#ifdef NORMAL_ALPHA_NAME
    CreateTexture2D( g_tNormal ) < Channel( RGB, Box( Normal ), Linear ); Channel( A, Box( NORMAL_ALPHA_NAME ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;
#else
    // otherwise, assume packed data
    CreateTexture2D( g_tNormal ) < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;
#endif

// To support HDR envmaps, we need linear reading and a fp output format
// TODO: is this needed anymore for TF:S2?
// CreateTextureCube( g_tEnvMap ) < Channel( RGBA, Box( EnvMap ), Linear ); OutputFormat( RGB323232F ); SrgbRead( true ); >;
CreateTextureCube( g_tEnvMap ) < Channel( RGBA, Box( EnvMap ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( true ); >;
float4 CONVERT_ENVMAP(float4 s) { return s; }

#if S_ENVMAPMASK
    #if S_SELFILLUM_ENVMAPMASK_ALPHA
        CreateTexture2D( g_tEnvMapMask ) < Channel( RGBA, Box( EnvmapMask ), Linear ); OutputFormat( DXT1 ); SrgbRead( false ); >;
    #else // !S_SELFILLUM_ENVMAPMASK_ALPHA
        CreateTexture2D( g_tEnvMapMask ) < Channel( RGB, Box( EnvmapMask ), Linear ); OutputFormat( DXT1 ); SrgbRead( false ); >;
    #endif // !S_SELFILLUM_ENVMAPMASK_ALPHA
#endif // S_ENVMAPMASK

// both warp textures must be RGB!
// love that this doesn't work for procedural stuff (unless i missed something)
// CreateTexture1D( g_tLightWarpTexture ) < Channel( RGB, Box( LightWarpTexture ), Linear ); OutputFormat( RGB323232F ); SrgbRead( false ); >;
CreateTexture2D( g_tLightWarpTexture ) < Channel( RGB, Box( LightWarpTexture ), Linear ); OutputFormat( RGB323232F ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( false ); >;

// phongwarp
CreateTexture2D( g_tSpecularWarpTexture ) < Channel( RGB, Box( SpecularWarpTexture ), Linear ); OutputFormat( RGB323232F ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( false ); >;
CreateTexture2D( g_tSpecularExponentTexture ) < Channel( R, Box( SpecularExponentTexture ), Linear ); Channel( G, Box( SpecularTintTexture ), Linear ); Channel( A, Box( RimMaskTexture ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); >;

CreateTexture2D( g_tSelfIllumMaskTexture ) < Channel( RGB, Box( SelfIllumMaskTexture ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); >;

#ifdef S_DETAILTEXTURE
    #if (S_DETAIL_BLEND_MODE == 0)
        #define DETAIL_SRGB SrgbRead( false )
        #define DETAIL_SRGB_ENUM Linear
    #else // (S_DETAIL_BLEND_MODE != 0)
        #define DETAIL_SRGB SrgbRead( true )
        #define DETAIL_SRGB_ENUM Srgb
    #endif // (S_DETAIL_BLEND_MODE != 0)

    #if S_DISTANCEALPHA && (S_DISTANCEALPHAFROMDETAIL == 1)
        #define DETAIL_CHANNELS Channel( RGB, Box( DetailTexture ), DETAIL_SRGB_ENUM ); Channel( A, Box( DistAlphaMask ), Linear )
    #else // !(S_DISTANCEALPHA && (S_DISTANCEALPHAFROMDETAIL == 1))
        #define DETAIL_CHANNELS Channel( RGBA, Box( DetailTexture ), DETAIL_SRGB_ENUM )
    #endif // !(S_DISTANCEALPHA && (S_DISTANCEALPHAFROMDETAIL == 1))

    CreateTexture2D( g_tDetailTexture ) < DETAIL_CHANNELS; DETAIL_SRGB; OutputFormat( BC7 ); >;

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

// for some ungodly reason, the EyeRefract AO texture is set up as SRGB in the SDK
#if AO_TEXTURE_IS_SRGB
    CreateTexture2D( g_tAmbientOcclusionTexture ) < Channel( RGBA, Box( AmbientOcclusion ), Srgb ); OutputFormat( RGBA8888 ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( true ); >;
#else // AO_TEXTURE_IS_SRGB
    CreateTexture2D( g_tAmbientOcclusionTexture ) < Channel( RGBA, Box( AmbientOcclusion ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); >;
#endif // AO_TEXTURE_IS_SRGB

#endif // SOURCEBOX_LEGACY_PIXELINPUTS_H