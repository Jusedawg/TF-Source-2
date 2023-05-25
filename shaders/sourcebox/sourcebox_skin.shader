//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	// CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "SourceBox skin shader. See license.txt for license information";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "sourcebox/common/legacy_features.hlsl"

    Feature( F_CUBEMAP,                         0..1, "Rendering" );
    Feature( F_CUSTOM_CUBEMAP,                  0..1, "Rendering" );
    FeatureRule( Requires1( F_CUSTOM_CUBEMAP, F_CUBEMAP ), "" );

    Feature( F_SELFILLUM,                       0..1, "Rendering" );
    Feature( F_SELFILLUMFRESNEL,                0..1, "Rendering" );
    Feature( F_LIGHTWARPTEXTURE,                0..1, "Rendering" );
    Feature( F_PHONGWARPTEXTURE,                0..1, "Rendering" );
    Feature( F_WRINKLEMAP,                      0..1, "Rendering" );
    Feature( F_RIMLIGHT,                        0..1, "Rendering" );
    Feature( F_BLENDTINTBYBASEALPHA,            0..1, "Rendering" );

    Feature( F_DETAILTEXTURE,                   0..1, "Rendering" );
    // The SDK only supports blendmodes 0-6 on skin
    Feature( F_DETAIL_BLEND_MODE, 0..6(0="RGB_EQUALS_BASE_x_DETAILx2",1="RGB_ADDITIVE",2="DETAIL_OVER_BASE",3="FADE",4="BASE_OVER_DETAIL",5="RGB_ADDITIVE_SELFILLUM",6="RGB_ADDITIVE_SELFILLUM_THRESHOLD_FADE"), "Rendering" );
    FeatureRule( Requires1( F_DETAIL_BLEND_MODE, F_DETAILTEXTURE ), "Requires detail texture" );

    FeatureRule(Requires1(F_SELFILLUMFRESNEL, F_SELFILLUM), "");
    // not necessary, but was a constraint of the original shader
    FeatureRule(Allow1(F_LIGHTWARPTEXTURE, F_SELFILLUMFRESNEL), "");

    FeatureRule(Allow1(F_BLENDTINTBYBASEALPHA, F_SELFILLUM), "");

    // this is enforced in SDK code
	FeatureRule( Allow1( F_ALPHA_TEST, F_SELFILLUM ), "" );
	// FeatureRule( Allow1( F_ALPHA_TEST, F_BASEALPHAENVMAPMASK ), "" );
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
    VrForward();													// Indicates this shader will be used for main rendering
    Depth( "vr_depth_only.vfx" ); 									// Shader that will be used for shadowing and depth prepass
    ToolsVis( S_MODE_TOOLS_VIS ); 									// Ability to see in the editor
    // ToolsWireframe( "vr_tools_wireframe.vfx" ); 					// Allows for mat_wireframe to work
	// ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" ); 	// Shows how expensive drawing is in debug view
}

//=========================================================================================================================
COMMON
{
    #include "system.fxc" // This should always be the first include in COMMON
    #include "sbox_shared.fxc"
    #define VS_INPUT_HAS_TANGENT_BASIS 1
    #define PS_INPUT_HAS_TANGENT_BASIS 1


    // vDetailTextureCoords in pixelinput.hlsl
    StaticCombo( S_DETAILTEXTURE                    , F_DETAILTEXTURE                   , Sys( ALL ) );
    #define S_DETAIL_TEXTURE S_DETAILTEXTURE

    // disabled to reduce compile time
    //StaticCombo( S_WRINKLEMAP                          , F_WRINKLEMAP                         , Sys( ALL ) );
    // #define S_WRINKLE_MAP S_WRINKLEMAP
    // #define S_WRINKLE S_WRINKLEMAP
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
    #include "common/vertex.hlsl"

	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );

        #if S_DETAILTEXTURE
            o.vDetailTextureCoords = i.vTexCoord.xy;
        #endif // S_DETAILTEXTURE

        #if S_WRINKLEMAP
            o.vNormalWs.w = i.vNormalOs.w;
        #endif // S_WRINKLEMAP

		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
    StaticCombo( S_CUBEMAP                          , F_CUBEMAP                         , Sys( ALL ) );
    StaticCombo( S_CUSTOM_CUBEMAP                   , F_CUSTOM_CUBEMAP                  , Sys( ALL ) );
    StaticCombo( S_SELFILLUM                        , F_SELFILLUM                       , Sys( ALL ) );
    StaticCombo( S_SELFILLUMFRESNEL                 , F_SELFILLUMFRESNEL                , Sys( ALL ) );
    StaticCombo( S_LIGHTWARPTEXTURE                 , F_LIGHTWARPTEXTURE                , Sys( ALL ) );
    StaticCombo( S_PHONGWARPTEXTURE                 , F_PHONGWARPTEXTURE                , Sys( ALL ) );

    StaticCombo( S_RIMLIGHT                         , F_RIMLIGHT                        , Sys( ALL ) );
    StaticCombo( S_BLENDTINTBYBASEALPHA             , F_BLENDTINTBYBASEALPHA            , Sys( ALL ) );
    
    // StaticCombo( S_DETAIL_BLEND_MODE                , F_DETAIL_BLEND_MODE               , Sys( ALL ) );
    int g_nDetailBlendMode < Expression(F_DETAIL_BLEND_MODE); >;
    #define DETAIL_BLEND_MODE g_nDetailBlendMode

    #define USE_MANUAL_CUBEMAP (S_CUBEMAP && S_CUSTOM_CUBEMAP)
    #if S_BLENDTINTBYBASEALPHA
        #define BASE_COLOR_ALPHA_NAME   TintMask
    #elif ( S_SELFILLUMFRESNEL == 1 )
        #define BASE_COLOR_ALPHA_NAME   SelfIllumMaskTexture
    #elif ( !S_SELFILLUM && !S_BLENDTINTBYBASEALPHA )
        #define BASE_COLOR_ALPHA_NAME   Translucency
    // #else
        // #define BASE_COLOR_ALPHA_NAME   EnvmapMask
    #endif

    #define NORMAL_ALPHA_NAME       SpecMask

    float3 GetEnvmapColor( float3 envmapBase, float3 envmapMask, float fresnelRanges )
    {
        #if S_CUBEMAP
		    return (g_flEnvMapScale *
							lerp(1, fresnelRanges, g_flEnvMapFresnel) *
							lerp(envmapMask.r, 1-envmapMask.r, g_fInvertPhongMask)) *
                            envmapBase *
							g_vEnvMapTint.xyz;
        #else // !S_CUBEMAP
            return float3(0.0, 0.0, 0.0);
        #endif // !S_CUBEMAP
    }
    
    #include "sourcebox/common/legacy_pixel.hlsl"

	float4 MainPs( PixelInput i ) : SV_Target0
	{
        ShadingModelLegacy sm;
        sm.config.DoDiffuse = true;
        sm.config.HalfLambert = true;
        sm.config.DoAmbientOcclusion = /*S_AMBIENT_OCCLUSION ? */true /*: false*/;
        sm.config.DoLightingWarp = S_LIGHTWARPTEXTURE ? true : false;
        sm.config.DoRimLighting = S_RIMLIGHT ? true : false;
        sm.config.DoSpecularWarp = S_PHONGWARPTEXTURE ? true : false;
        sm.config.DoSpecular = true;

        sm.config.SelfIllum = S_SELFILLUM ? true : false;
        sm.config.SelfIllumFresnel = S_SELFILLUMFRESNEL ? true : false;

        sm.config.StaticLight = false;
        sm.config.AmbientLight = true;

        #if S_WRINKLEMAP
            float fWrinkleWeight = i.vNormalWs.w;
            float flWrinkleAmount = saturate( -fWrinkleWeight );					// One of these two is zero
            float flStretchAmount = saturate(  fWrinkleWeight );					// while the other is in the 0..1 range

            float flTextureAmount = 1.0f - flWrinkleAmount - flStretchAmount;		// These should sum to one
        #endif // S_WRINKLEMAP

        float2 vUV = i.vTextureCoords.xy;
        float4 baseColor = CONVERT_COLOR(Tex2D( g_tColor, vUV ));
        #if S_WRINKLEMAP
            float4 wrinkleColor = Tex2D( g_tWrinkle, vUV );
            float4 stretchColor = Tex2D( g_tStretch, vUV );

            // Apply wrinkle blend to only RGB.  Alpha comes from the base texture
            baseColor.rgb = flTextureAmount * baseColor.rgb + flWrinkleAmount * wrinkleColor.rgb + flStretchAmount * stretchColor.rgb;
        #endif // S_WRINKLEMAP

        // #if S_AMBIENT_OCCLUSION
        float flAmbientOcclusion = Tex2D( g_tAmbientOcclusionTexture, vUV ).r;
        // #endif // S_AMBIENT_OCCLUSION

        #if S_DETAILTEXTURE
            // float4 detailColor = Tex2D( g_tDetailTexture, i.vTextureCoords.zw );
            // packed in SDK
            float4 detailColor = CONVERT_DETAIL(Tex2D( g_tDetailTexture, i.vDetailTextureCoords.xy ));
            baseColor = TextureCombine( baseColor, detailColor, DETAIL_BLEND_MODE, g_flDetailBlendFactor );
        #endif // S_DETAILTEXTURE

        // float fogFactor = CalcPixelFogFactor( PIXELFOGTYPE, g_FogParams, g_EyePos_SpecExponent.z, vWorldPos.z, vProjPos.z );
        float3 positionWs = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs;
        // View ray in World Space
        float3 vEyeDir = normalize(CalculatePositionToCameraDirWs( positionWs ));

        float3 worldSpaceNormal, tangentSpaceNormal;
        float fSpecMask = 1.0f;
        float4 normalTexel = Tex2D( g_tNormal, vUV );
        // inverted normals
		normalTexel.y = 1 - normalTexel.y;

        #if S_WRINKLEMAP
            float4 wrinkleNormal = Tex2D( g_tWrinkleNormal,	vUV );
            float4 stretchNormal = Tex2D( g_tWrinkleStretchNormal, vUV );
            // inverted normals
		    wrinkleNormal.y = 1 - wrinkleNormal.y;
		    stretchNormal.y = 1 - stretchNormal.y;
            normalTexel = flTextureAmount * normalTexel + flWrinkleAmount * wrinkleNormal + flStretchAmount * stretchNormal;
        #endif // S_WRINKLEMAP

        // #if (FASTPATH_NOBUMP == 0 )
        tangentSpaceNormal = g_bBaseMapAlphaPhongMask ? float3(0, 0, 1) : 2.0f * normalTexel.xyz - 1.0f;
        fSpecMask = g_bBaseMapAlphaPhongMask ? baseColor.a : normalTexel.a;
        // #else
        //     tangentSpaceNormal = float3(0, 0, 1);
        //     fSpecMask = baseColor.a;
        // #endif

	    // worldSpaceNormal = normalize( mul( i.tangentSpaceTranspose, tangentSpaceNormal ) );
	    worldSpaceNormal = TransformNormal( i, tangentSpaceNormal );

        float fFresnelRanges = Fresnel( worldSpaceNormal, vEyeDir, g_vFresnelRanges );
	    float fRimFresnel = Fresnel4( worldSpaceNormal, vEyeDir );
        
        // envmap params
        // Mask is either normal map alpha or base map alpha
        #if ( S_SELFILLUMFRESNEL == 1 ) // This is to match the 2.0 version of vertexlitgeneric
            float fEnvMapMask = g_bEnvMapShadowTweaks ? g_fInvertPhongMask : baseColor.a;
        #else
            float fEnvMapMask = g_bEnvMapShadowTweaks ? fSpecMask : baseColor.a;
        #endif

        float3 vSpecularTint = float3(1.0, 1.0, 1.0);
        float fRimMask = 0;
        float fSpecExp = 1;

        float4 vSpecExpMap = Tex2D( g_tSpecularExponentTexture, vUV );
	
        // if ( !bFlashlight )
        // {
            fRimMask = lerp( 1.0f, vSpecExpMap.a, g_flRimMask );						// Select rim mask
        // }

	    fSpecExp = g_bConstantSpecularExponent ? g_flSpecularExponent : (1.0f + 149.0f * vSpecExpMap.r);

        vSpecularTint = lerp( float3(1.0f, 1.0f, 1.0f), baseColor.rgb, vSpecExpMap.g );
        vSpecularTint = g_bConstantSpecularTint ? g_vSpecularTint.rgb : vSpecularTint;
            
        float3 albedo = baseColor.rgb;

        // If we didn't already apply Fresnel to specular warp, modulate the specular
        #if ( !S_PHONGWARPTEXTURE )
            fSpecMask *= fFresnelRanges;
        #endif // ( !S_PHONGWARPTEXTURE )

        #if S_BLENDTINTBYBASEALPHA
            float3 tintedColor = albedo * g_vDiffuseModulation.rgb;
            tintedColor = lerp(tintedColor, g_vDiffuseModulation.rgb, g_flTintReplacementControl);
            albedo = lerp(albedo, tintedColor, baseColor.a);
        #else // !S_BLENDTINTBYBASEALPHA
            albedo = albedo * g_vDiffuseModulation.rgb;
        #endif // S_BLENDTINTBYBASEALPHA

        Material m = GetDefaultLegacyMaterial();
        m.Albedo = albedo;
        // m.Normal = TransformNormal( i, DecodeHemiOctahedronNormal( normalTexel.xy ) );
        m.Normal = worldSpaceNormal;
        
        m.SpecularMask = fSpecMask;
        m.SpecularExponent = fSpecExp;
        m.SpecularTint = vSpecularTint;
        m.Fresnel = fFresnelRanges;
        m.EnvmapMask = float3(fEnvMapMask, fEnvMapMask, fEnvMapMask);
        m.RimMask = fRimMask;
        m.RimFresnel = fRimFresnel;
        m.RimExponent = g_flRimExponent;
        
        // #if S_AMBIENT_OCCLUSION
        m.AmbientOcclusion = float3(flAmbientOcclusion, flAmbientOcclusion, flAmbientOcclusion);
        // #endif // S_AMBIENT_OCCLUSION

        #if ( S_SELFILLUMFRESNEL == 1 )
            // this one just uses the base color alpha
            m.SelfIllumMask = baseColor.a;
        #else
            // whereas this samples a mask
            float3 vSelfIllumMask = Tex2D( g_tSelfIllumMaskTexture, vUV ).rgb;
            vSelfIllumMask = g_bSelfIllumMaskControl ? vSelfIllumMask : baseColor.aaa;
            m.SelfIllumMask = vSelfIllumMask;
        #endif

        float alpha = g_vDiffuseModulation.a;
        #if ( !S_SELFILLUM && !S_BLENDTINTBYBASEALPHA )
            alpha = g_bBaseMapAlphaPhongMask ? alpha : baseColor.a * alpha;
        #endif // ( !S_SELFILLUM && !S_BLENDTINTBYBASEALPHA )
        m.Opacity = alpha;
        
        return FinalizeLegacyOutput(FinalizePixelMaterial( i, m, sm ));
	}
}