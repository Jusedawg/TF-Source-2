// https://github.com/ValveSoftware/source-sdk-2013/blob/master/sp/src/materialsystem/stdshaders/common_vertexlitgeneric_dx9.h
// Legacy VertexLitGeneric style lighting

#ifndef SOURCEBOX_LEGACY_LIGHTING_H
#define SOURCEBOX_LEGACY_LIGHTING_H

#ifndef SOURCEBOX_LEGACY_MATERIAL_H
#error Material struct must be overriden first!
#endif // SOURCEBOX_LEGACY_MATERIAL_H

struct LegacyLightResult
{
    float3 Diffuse;
    float3 Specular;
    float3 Iris;
    float3 Rim;

    static LegacyLightResult Init()
    {
        LegacyLightResult result;
        result.Diffuse = float3(0,0,0);
        result.Specular = float3(0,0,0);
        result.Iris = float3(0,0,0);
        result.Rim = float3(0,0,0);
        return result;
    }

    static LegacyLightResult Sum( const LegacyLightResult a, const LegacyLightResult b )
    {
        LegacyLightResult result;
        result.Diffuse = a.Diffuse + b.Diffuse;
        result.Specular = a.Specular + b.Specular;
        result.Iris = a.Iris + b.Iris;
        result.Rim = a.Rim + b.Rim;
        return result;
    }
};

// Use SourceBox code-controlled lighting instead of s&box builtin
// #define USE_MANUAL_AMBIENT
// #define USE_MANUAL_CUBEMAP

float3 GammaToLinear( const float3 gamma )
{
	return pow( gamma, 2.2f );
}

// Traditional fresnel term approximation
float Fresnel( const float3 vNormal, const float3 vEyeDir )
{
	float fresnel = saturate( 1 - dot( vNormal, vEyeDir ) );			// 1-(N.V) for Fresnel term
	return fresnel * fresnel;											// Square for a more subtle look
}

// Traditional fresnel term approximation which uses 4th power (square twice)
float Fresnel4( const float3 vNormal, const float3 vEyeDir )
{
	float fresnel = saturate( 1 - dot( vNormal, vEyeDir ) );			// 1-(N.V) for Fresnel term
	fresnel = fresnel * fresnel;										// Square
	return fresnel * fresnel;											// Square again for a more subtle look
}

//
// Custom Fresnel with low, mid and high parameters defining a piecewise continuous function
// with traditional fresnel (0 to 1 range) as input.  The 0 to 0.5 range blends between
// low and mid while the 0.5 to 1 range blends between mid and high
//
//    |
//    |    .  M . . . H
//    | . 
//    L
//    |
//    +----------------
//    0               1
//
float Fresnel( const float3 vNormal, const float3 vEyeDir, float3 vRanges )
{
	//float result, f = Fresnel( vNormal, vEyeDir );			// Traditional Fresnel
	//if ( f > 0.5f )
	//	result = lerp( vRanges.y, vRanges.z, (2*f)-1 );		// Blend between mid and high values
	//else
	//	result = lerp( vRanges.x, vRanges.y, 2*f );			// Blend between low and mid values

	// note: vRanges is now encoded as ((mid-min)*2, mid, (max-mid)*2) to optimize math
	float f = saturate( 1 - dot( vNormal, vEyeDir ) );
	f = f*f - 0.5;
	return vRanges.y + (f >= 0.0 ? vRanges.z : vRanges.x) * f;
}

struct ShadingLegacyConfig {
    bool DoDiffuse;

    bool SelfIllum;
    bool SelfIllumFresnel;

    // Use half lambert
    bool HalfLambert;

    // Should we do ambient occlusion?
    // TODO: find which materials actually use this in the SDK 
    bool DoAmbientOcclusion;

    // Use a lightwarp texture
    bool DoLightingWarp;

    // Use rim lighting
    bool DoRimLighting;

    bool DoSpecularWarp;

    // only used on Skin and Teeth
    bool DoSpecular;

    // used on EyeRefract
    bool DoIrisLighting;

    // use static lighting?
    bool StaticLight;
    // use ambient lighting?
    bool AmbientLight;

    #define cOverbright 2.0f
    #define cOOOverbright 0.5f

    static ShadingLegacyConfig GetDefault()
    {
        ShadingLegacyConfig config;

        config.DoDiffuse = true;

        config.SelfIllum = false;
        config.SelfIllumFresnel = false;

        config.HalfLambert = false;
        config.DoAmbientOcclusion = false;
        config.DoLightingWarp = false;
        config.DoRimLighting = false;
        config.DoSpecularWarp = false;
        config.DoSpecular = false;
        config.DoIrisLighting = false;

        config.StaticLight = false;
        config.AmbientLight = true;

        return config;
    }
};

// S&box glue
class ShadingBuiltinConfig
{  
    //
    // Sets the shading model to use for ambient occlusion
    //
    // Can be one of the following values:
    // 0 - AMBIENT_OCCLUSION_SIMPLE ( Default )
    // 1 - AMBIENT_OCCLUSION_MULTIBOUNCE
    // 2 - AMBIENT_OCCLUSION_BENT_NORMALS ( + MultiBounce )
    //
    #define AMBIENT_OCCLUSION_SIMPLE 0
    #define AMBIENT_OCCLUSION_MULTIBOUNCE 1
    #define AMBIENT_OCCLUSION_BENT_NORMALS 2
    uint AmbientOcclusionModel;
};

// Processed pixel inputs to what's easier to be consumed by the shader
class LegacyShadeInputs
{
    float3 PositionWs;
    float3 PositionWithOffsetWs;
    float3 ViewRayWs;
    float3 NormalWs;
    float3 VertexNormalWs;

    float3 ReflectRayWs;

    float2 PositionSs;
    
    float  NoV;

    // Needed for indirect lightmaps
    #if ( D_BAKED_LIGHTING_FROM_LIGHTMAP )
        float2 PositionLightmap;
	#endif

    // used for EyeRefract
    float3 TangentWs;
    float3 BinormalWs;
    float3 ViewRayTs;
    float3 CorneaNormalTs;
    float3 CorneaNormalWs;
    float3 CorneaReflectionWs;

    // TODO: use s&box builtin functions instead
    float3 Vec3WorldToTangent( float3 iWorldVector, float3 iWorldNormal, float3 iWorldTangent, float3 iWorldBinormal )
    {
        float3 vTangentVector;
        vTangentVector.x = dot( iWorldVector.xyz, iWorldTangent.xyz );
        vTangentVector.y = dot( iWorldVector.xyz, iWorldBinormal.xyz );
        vTangentVector.z = dot( iWorldVector.xyz, iWorldNormal.xyz );
        return vTangentVector.xyz; // Return without normalizing
    }

    float3 Vec3WorldToTangentNormalized( float3 iWorldVector, float3 iWorldNormal, float3 iWorldTangent, float3 iWorldBinormal )
    {
        return normalize( Vec3WorldToTangent( iWorldVector, iWorldNormal, iWorldTangent, iWorldBinormal ) );
    }

    float3 Vec3TangentToWorld( float3 iTangentVector, float3 iWorldNormal, float3 iWorldTangent, float3 iWorldBinormal )
    {
        float3 vWorldVector;
        vWorldVector.xyz = iTangentVector.x * iWorldTangent.xyz;
        vWorldVector.xyz += iTangentVector.y * iWorldBinormal.xyz;
        vWorldVector.xyz += iTangentVector.z * iWorldNormal.xyz;
        return vWorldVector.xyz; // Return without normalizing
    }

    float3 Vec3TangentToWorldNormalized( float3 iTangentVector, float3 iWorldNormal, float3 iWorldTangent, float3 iWorldBinormal )
    {
        return normalize( Vec3TangentToWorld( iTangentVector, iWorldNormal, iWorldTangent, iWorldBinormal ) );
    }

    void GetPixelInputs( const PixelInput pixelInput, const Material material )
    {
        PositionWithOffsetWs = pixelInput.vPositionWithOffsetWs.xyz;
        PositionWs = PositionWithOffsetWs + g_vCameraPositionWs;

        // View ray in World Space
        ViewRayWs = CalculatePositionToCameraDirWs( PositionWs );

        // Surface Normal in World Space        
        NormalWs = material.Normal;
        VertexNormalWs = pixelInput.vNormalWs.xyz;

        // Dot product of both
        NoV = dot( NormalWs, ViewRayWs );

        PositionSs = pixelInput.vPositionSs.xy;

        // Reflection ray
        ReflectRayWs = reflect( -ViewRayWs, NormalWs );

        #if ( D_BAKED_LIGHTING_FROM_LIGHTMAP )
            PositionLightmap = pixelInput.vLightmapUV.xy;
        #endif

        // used for EyeRefract
        #if ( PS_INPUT_HAS_TANGENT_BASIS )
            TangentWs = normalize(pixelInput.vTangentUWs);
            BinormalWs = normalize(pixelInput.vTangentVWs.xyz);
        #else // !PS_INPUT_HAS_TANGENT_BASIS
            TangentWs = float3(1.0, 0.0, 0.0);
            BinormalWs = float3(0.0, 1.0, 0.0);
        #endif // !PS_INPUT_HAS_TANGENT_BASIS

        ViewRayTs = Vec3WorldToTangentNormalized(ViewRayWs.xyz, NormalWs.xyz, TangentWs.xyz, BinormalWs.xyz);
        CorneaNormalTs = material.CorneaNormal;
        CorneaNormalWs = Vec3TangentToWorldNormalized( CorneaNormalTs.xyz, NormalWs.xyz, TangentWs.xyz, BinormalWs.xyz );
        CorneaReflectionWs = reflect( -ViewRayWs.xyz, CorneaNormalWs.xyz );
    }
};

// Processed material params to what's easier to be consumed by the shader
class LegacyShadeParams {
    float3  Albedo;
    float   Opacity;
    
    float3  AmbientOcclusion;

    float   Fresnel;

    float   RimMask;
    float   RimFresnel;
    float   RimExponent;

    float   SpecularMask;
    float3  SpecularTint;
    float   SpecularExponent;

    float3  StaticLightingColor;
    float3  EnvmapMask;

    float3  SelfIllumMask;

    // used by EyeRefract
    float IrisHighlightMask;
    float AverageAmbient;

    // custom controls
    float4 DiffuseModControls;

    LegacyShadeInputs inputs;
    ShadingBuiltinConfig config;

    void GetCommonParams( const PixelInput pixel, const Material m )
    {
        // Diffuse Color
        // LegacyMaterial m = MaterialToLegacy(material);
        Albedo = m.Albedo;
        Opacity = m.Opacity;
        AmbientOcclusion = m.AmbientOcclusion;
        Fresnel = m.Fresnel;
        RimMask = m.RimMask;
        RimFresnel = m.RimFresnel;
        RimExponent = m.RimExponent;
        SpecularMask = m.SpecularMask;
        SpecularTint = m.SpecularTint;
        SpecularExponent = m.SpecularExponent;
        SelfIllumMask = m.SelfIllumMask;
        StaticLightingColor = m.StaticLightingColor;
        EnvmapMask = m.EnvmapMask;

        // used by EyeRefract
        IrisHighlightMask = m.IrisHighlightMask;
        AverageAmbient = m.AverageAmbient;

        // custom controls
        DiffuseModControls = m.DiffuseModControls;


        config.AmbientOcclusionModel = AMBIENT_OCCLUSION_MULTIBOUNCE;
    }

    //
    // 
    //
    void GetPixelParameters( const PixelInput pixel, const Material material )
    {
        // Pixel parameters
        inputs.GetPixelInputs( pixel, material );

        // Material parameters
        GetCommonParams( pixel, material );
    }

    //
    // Converts from Material to ShadeParams
    // This makes it easier for shading code to be consumed by the shader
    // Most of it is inlined and optimized by the compiler
    //
    static LegacyShadeParams ProcessMaterial( const PixelInput pixelInput, const Material material )
    {
        LegacyShadeParams params;

        params.GetPixelParameters(pixelInput, material);

        return params;
    }
};

#include "sourcebox/common/legacy_lighting.ambientocclusion.hlsl"

class ShadingModelLegacy : ShadingModel
{
    //-----------------------------------------------------------------------------
    // Purpose: Compute scalar diffuse term with various optional tweaks such as
    //          Half Lambert and ambient occlusion
    //-----------------------------------------------------------------------------
    static float3 DiffuseTerm( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, const Light light )
    {
        float fResult;

        float NDotL = dot( shadeParams.inputs.NormalWs, light.Direction );				// Unsaturated dot (-1 to 1 range)
        // builtin NDotL is already saturated
        // TODO: ognik fixed this
        //float NDotL = light.NdotL;

        if ( config.HalfLambert )
        {
            fResult = saturate(NDotL * 0.5 + 0.5);				// Scale and bias to 0 to 1 range

            if ( !config.DoLightingWarp )
            {
                fResult *= fResult;								// Square
            }
        }
        else
        {
            fResult = saturate( NDotL );						// Saturate pure Lambertian term
        }

        if ( config.DoAmbientOcclusion && !config.DoIrisLighting )
        {
            // From SDK: Raise to higher powers for darker AO values
    //		float fAOPower = lerp( 4.0f, 1.0f, fAmbientOcclusion );
    //		result *= pow( NDotL * 0.5 + 0.5, fAOPower );
            // fResult *= GetAmbientOcclusion(shadeParams);
            fResult *= lerp(1.0f, shadeParams.AmbientOcclusion.r, g_flAmbientOcclusionDirectDiffuse * (1 - g_flAmbientOcclusionDirectPostLightwarp));
        }

        float3 fOut = float3( fResult, fResult, fResult );
		
		// from SDK, only works if lightwarp texture is RGB
        if ( config.DoLightingWarp )
        {
            // fOut = 2.0f * Tex1D( g_tLightWarpTexture, fResult );
            fOut = 2.0f * Tex2DLevel( g_tLightWarpTexture, float2( fResult, 0 ), 0 ).rgb;
        }

        // TF:S2 change: always apply AO post-lightwarp
        if ( config.DoAmbientOcclusion && (config.DoIrisLighting || true) )
        {
            // TODO: this was RGB AO originally?
            // fOut *= GetAmbientOcclusion(shadeParams);
            fOut *= lerp(1.0f, shadeParams.AmbientOcclusion, g_flAmbientOcclusionDirectDiffuse * g_flAmbientOcclusionDirectPostLightwarp);
        }

        return fOut;
    }

    static float3 PixelShaderDoGeneralDiffuseLight( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, const Light light )
    {
        return light.Color * light.Attenuation * light.Visibility * DiffuseTerm( shadeParams, config, light );
    }

    static void SpecularAndRimTerms( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, const Light light, const float3 color, out float3 specularLighting, out float3 rimLighting )
    {
        rimLighting = float3(0.0f, 0.0f, 0.0f);

        //float3 vReflect = reflect( -vEyeDir, vWorldNormal );				// Reflect view through normal
        //float3 vReflect = 2 * vWorldNormal * dot( vWorldNormal , vEyeDir ) - vEyeDir; // Reflect view through normal
        float3 vReflect = shadeParams.inputs.ReflectRayWs;

        float LdotR = saturate(dot( vReflect, light.Direction ));					// L.R	(use half-angle instead?)
        specularLighting = pow( LdotR, shadeParams.SpecularExponent );					// Raise to specular exponent

        // Optionally warp as function of scalar specular and fresnel
        if ( config.DoSpecularWarp )
            specularLighting *= Tex2DLevel( g_tSpecularWarpTexture, float2(specularLighting.x, shadeParams.Fresnel), 0 ).rgb; // Sample at { (L.R)^k, fresnel }

        specularLighting *= saturate(dot( shadeParams.inputs.NormalWs, light.Direction ));		// Mask with N.L
        specularLighting *= color;											// Modulate with light color
        
        if ( config.DoAmbientOcclusion && !config.DoIrisLighting )			// Optionally modulate with ambient occlusion
        {
            // specularLighting *= specularAO( shadeParams, GetAmbientOcclusion(shadeParams) );
            specularLighting *= lerp(1.0f, shadeParams.AmbientOcclusion.r, g_flAmbientOcclusionDirectSpecular);
        }

        if ( config.DoRimLighting )											// Optionally do rim lighting
        {
            rimLighting  = pow( LdotR, shadeParams.RimExponent );			// Raise to rim exponent
            rimLighting *= saturate(dot( shadeParams.inputs.NormalWs, light.Direction )); // Mask with N.L
            rimLighting *= color;											// Modulate with light color
        }

        // not in SDK
        specularLighting *= light.Visibility;
		// rim lighting shouldn't be affected by shadows
        // rimLighting *= light.Visibility;
    }

    static void PixelShaderDoSpecularLight( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, const Light light, out float3 specularLighting, out float3 rimLighting )
    {
        // Compute Specular and rim terms
        SpecularAndRimTerms( shadeParams, config, light, light.Color * light.Attenuation, specularLighting, rimLighting );
    }

    static float3 DiffuseModulate( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, float3 diffuse )
    {
        float3 albedo = shadeParams.Albedo.rgb;
        float3 diffuseComponent = albedo * diffuse;

        if ( config.SelfIllum )
        {
            // This will apply a fresnel term based on the vertex normal to help fake and internal glow look
            if ( config.SelfIllumFresnel )
            {
                float3 vVertexNormal = shadeParams.inputs.VertexNormalWs;
                float flSelfIllumFresnel = ( pow( saturate( dot( vVertexNormal.xyz, shadeParams.inputs.ViewRayWs ) ), g_flSelfIllumExponent ) * g_flSelfIllumScale ) + g_flSelfIllumBias;

                float3 selfIllumComponent = g_vSelfIllumTint * albedo * g_flSelfIllumBrightness;
                diffuseComponent = lerp( diffuseComponent, selfIllumComponent, shadeParams.SelfIllumMask * saturate( flSelfIllumFresnel ) );
            }
            else
            {
                float3 selfIllumComponent = g_vSelfIllumTint * albedo;
                diffuseComponent = lerp( diffuseComponent, selfIllumComponent, shadeParams.SelfIllumMask );
            }
        }

        return diffuseComponent;
    }

    static float3 SpecularModulate( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, float3 specular )
    {
        return specular * shadeParams.SpecularTint;
    }

    static float3 DoIrisCausticLighting( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, const Light light )
    {
        float3 vIrisTangentNormal = shadeParams.inputs.CorneaNormalTs.xyz;
        vIrisTangentNormal.xy *= -2.5f; // I'm not normalizing on purpose
        
        // for ( int j=0; j < nNumLights; j++ )
        // {
            // World light vector
            float3 vWorldLightVector = light.Direction;

            // Tangent light vector
            float3 vTangentLightVector;
            vTangentLightVector.x = dot( vWorldLightVector.xyz, shadeParams.inputs.TangentWs.xyz );
            vTangentLightVector.y = dot( vWorldLightVector.xyz, shadeParams.inputs.BinormalWs.xyz );
            vTangentLightVector.z = dot( vWorldLightVector.xyz, shadeParams.inputs.NormalWs.xyz );
            vTangentLightVector = normalize(vTangentLightVector);

            // Adjust the tangent light vector to generate the iris lighting
            float3 tmpv = -vTangentLightVector.xyz;
            tmpv.xy *= -0.5f; //Flatten tangent view
            tmpv.z = max( tmpv.z, 0.5f ); //Clamp z of tangent view to help maintain highlight
            tmpv.xyz = normalize( tmpv.xyz );

            // Core iris lighting math
            float fIrisFacing = pow( abs( dot( vIrisTangentNormal, tmpv.xyz ) ), 6.0f ) * 0.5f; // Yes, 6.0 and 0.5 are magic numbers

            // Cone of darkness to darken iris highlights when light falls behind eyeball past a certain point
            float flConeOfDarkness = pow( 1.0f - saturate( ( -vTangentLightVector.z - 0.25f ) / 0.75f ), 4.0f );

            // Tint by iris color and cone of darkness
            float3 cIrisLightingTmp = fIrisFacing * shadeParams.IrisHighlightMask * flConeOfDarkness;

            // Attenuate by light color and light falloff
            // cIrisLightingTmp.rgb *= i.vLightFalloffCosine01.x * PixelShaderGetLightColor( g_sLightInfo, 0 );
            cIrisLightingTmp.rgb *= light.Attenuation * light.Color;

            // Sum into final variable
            return cIrisLightingTmp.rgb;
        // }
    }

    //
    // Executed for every direct light
    //
    static LegacyLightResult Direct( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config, const Light light )
    {
        LegacyLightResult lightShade;
        
        lightShade.Diffuse = float3( 0.0, 0.0, 0.0 );
        if ( config.DoDiffuse )
        {
            // Don't add it in, we need this data for EyeRefract cornea stuff
            // lightShade.Diffuse = DiffuseModulate( PixelShaderDoGeneralDiffuseLight( light ) );
            lightShade.Diffuse = PixelShaderDoGeneralDiffuseLight( shadeParams, config, light );
        }

        lightShade.Iris = float3( 0.0, 0.0, 0.0 );
        if ( config.DoIrisLighting )
        {
            lightShade.Iris = DoIrisCausticLighting( shadeParams, config, light );
        }

        lightShade.Specular = float3( 0.0, 0.0, 0.0 );
        lightShade.Rim = float3( 0.0, 0.0, 0.0 );

        if ( config.DoSpecular )
        {
            float3 specularLighting = float3( 0.0f, 0.0f, 0.0f );
	        float3 rimLighting = float3( 0.0f, 0.0f, 0.0f );

            PixelShaderDoSpecularLight( shadeParams, config, light, specularLighting, rimLighting );

            // Modulate with spec mask, boost and tint
            specularLighting *= shadeParams.SpecularMask * g_flSpecularBoost;

            if ( config.DoRimLighting )
            {
                // float fRimMultiply = shadeParams.RimMask * shadeParams.RimFresnel; // both unit range: [0, 1]
                
                // Add in rim light modulated with tint, mask and traditional Fresnel (not using Fresnel ranges)
                // rimLighting *= fRimMultiply;

                // Fold rim lighting into specular term by using the max so that we don't really add light twice...
                // TODO: This isn't strictly accurate. The SDK does this max on the *sum* of all specular lighting,
                // whereas here this will occur for each individual light. Not sure what the visual consequences of this will be...
                // specularLighting = max( specularLighting, rimLighting );
				
				// HACK: sum up the rim lighting here, add later in ambient, to avoid the issues outlined above
				lightShade.Rim = rimLighting;
				
                // Add in view-ray lookup from ambient cube (moved to Indirect)
            }

			lightShade.Specular = specularLighting;
        }

        // Giant iris specular highlights
        if ( config.DoIrisLighting )
        {
			lightShade.Specular += pow( saturate( dot( shadeParams.inputs.CorneaReflectionWs.xyz, light.Direction ) ), 128.0f ) * light.Attenuation * light.Color;
        }

        return lightShade;
    }
    
    #ifndef USE_MANUAL_AMBIENT
        static void GetAmbientCube( const LegacyShadeParams shadeParams, out float3 ambientCube[6] )
        {
            AmbientLight light;

            //
            // Position
            //
            light.Position = shadeParams.inputs.PositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs.xyz;

            SampleLightProbeVolume( ambientCube, light.Position );
        }
    #endif // USE_MANUAL_AMBIENT

    static float3 PixelShaderAmbientLight( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config )
    {
        // #ifdef USE_MANUAL_AMBIENT
            const float3 worldNormal = shadeParams.inputs.NormalWs;
            float3 cAmbientCube[6];
            GetAmbientCube( shadeParams, cAmbientCube );

            float3 linearColor, nSquared = worldNormal * worldNormal;
            float3 isNegative = ( worldNormal >= 0.0 ) ? 0 : nSquared;
            float3 isPositive = ( worldNormal >= 0.0 ) ? nSquared : 0;
            linearColor = isPositive.x * cAmbientCube[0] + isNegative.x * cAmbientCube[1] +
                        isPositive.y * cAmbientCube[2] + isNegative.y * cAmbientCube[3] +
                        isPositive.z * cAmbientCube[4] + isNegative.z * cAmbientCube[5];
            return linearColor;
        // #else // !USE_MANUAL_AMBIENT
        //     return AmbientLight::From( i, m ).Color
        //     return SampleLightProbeVolume( shadeParams.inputs.PositionWs, shadeParams.inputs.NormalWs );
        // #endif // !USE_MANUAL_AMBIENT
    }

    //
    // Executed for indirect lighting, combine ambient occlusion, etc.
    //
    static LegacyLightResult Indirect( const LegacyShadeParams shadeParams, const ShadingLegacyConfig config )
    {
        LegacyLightResult lightShade;

        float3 ambient = PixelShaderAmbientLight( shadeParams, config );

        lightShade.Diffuse = float3( 0.0, 0.0, 0.0 );
        if ( config.DoDiffuse )
        {
            float3 linearColor = float3( 0.0, 0.0, 0.0 );
            if ( config.StaticLight )
            {
                // The static lighting comes in in gamma space and has also been premultiplied by $cOOOverbright
                // need to get it into linear space so that we can do adds.
                linearColor += GammaToLinear( shadeParams.StaticLightingColor * cOverbright );
            }

            if ( config.AmbientLight )
            {
                float3 diffAmbient = ambient;
                if ( config.DoAmbientOcclusion && !config.DoIrisLighting )
                {
                    // ambient *= shadeParams.AmbientOcclusion.r * shadeParams.AmbientOcclusion.r;	// Note squaring...
                    float flAO = GetAmbientOcclusion(shadeParams);
                    diffAmbient *= lerp(1.0f, flAO * flAO, g_flAmbientOcclusionDirectAmbient);	// Note squaring...
                }

                linearColor += diffAmbient;
            }

            lightShade.Diffuse = linearColor;
        }

        lightShade.Specular = float3(0.0, 0.0, 0.0);

        // finalize the lighting
        lightShade.Iris = float3(0.0, 0.0, 0.0);
        if ( config.DoIrisLighting )
        {
            float3 vIrisTangentNormal = shadeParams.inputs.CorneaNormalTs.xyz;
            vIrisTangentNormal.xy *= -2.5f; // I'm not normalizing on purpose
        
            // Add slight view dependent iris lighting based on ambient light intensity to enhance situations with no local lights (0.5f is to help keep it subtle)
            // lightShade.Iris = saturate( dot( vIrisTangentNormal, -shadeParams.inputs.ViewRayTs.xyz ) ) * shadeParams.AverageAmbient * shadeParams.IrisHighlightMask * 0.5f;
            lightShade.Iris = saturate( dot( vIrisTangentNormal, -shadeParams.inputs.ViewRayTs.xyz ) ) * ambient * shadeParams.IrisHighlightMask * 0.5f;
        }

        lightShade.Rim = float3(0.0, 0.0, 0.0);

        return lightShade;
    }

    static float3 PixelShaderCubemap( PixelInput i, Material m, const LegacyShadeParams shadeParams, const ShadingLegacyConfig config )
    {
        #if USE_MANUAL_CUBEMAP
            // TODO: CUBEMAP_SPHERE_LEGACY?
            float3 vReflect = config.DoIrisLighting ? shadeParams.inputs.CorneaReflectionWs : shadeParams.inputs.ReflectRayWs;
            float3 envmapBase = CONVERT_ENVMAP(Tex3D( g_tEnvMap, vReflect )).rgb;
        #else // !USE_MANUAL_CUBEMAP
            float flAccumulatedWeight = 0.0;
            float3 envmapBase = float3(0.0, 0.0, 0.0);
            
            [unroll(NUM_CUBES)]
            for ( uint index = 0; index < 1; index++ )
            {
                Light light = EnvironmentMapLight::From( i, m, index );
                
                flAccumulatedWeight += light.Attenuation;
                if( flAccumulatedWeight >= 1.0 )
                    break;

                envmapBase = lerp( envmapBase, light.Color, 1.0 - flAccumulatedWeight );
            }
        #endif // !USE_MANUAL_CUBEMAP

        return GetEnvmapColor(envmapBase, shadeParams.EnvmapMask, shadeParams.Fresnel);
    }

    // TODO: copied from pixel.shading.standard.indirect.hlsl
    static float GetAmbientOcclusion( LegacyShadeParams i )
    {
        float fAmbientOcclusion = i.AmbientOcclusion.r;          // Texture Ambient Occlusion
        fAmbientOcclusion *= GetDynamicAmbientOcclusion( i );  // Dynamic Ambient Occlusion ( eg AOProxies )
        fAmbientOcclusion *= GetBakedAmbientOcclusion( i );    // Baked Ambient Occlusion ( eg Lightmap )

        return fAmbientOcclusion;
    }

    static float GetDynamicAmbientOcclusion( LegacyShadeParams i )
    {
        // --------------------------------------------------------------------------------------------
        if ( g_bAmbientOcclusionProxiesEnabled )
		{
            // Fixme: Add depth to Z channel
            float3 vPositionSs = float3( i.inputs.PositionSs, 1.0f );
			
            //
			// Validity mask - only want the lights on our side of the hemisphere
            // Fixme: Geometric normal
			//
			float4 vLightDirectionValidMask;
			vLightDirectionValidMask.x = dot( g_vAmbientOcclusionProxyLightPositions[0].xyz, i.inputs.NormalWs.xyz );
			vLightDirectionValidMask.y = dot( g_vAmbientOcclusionProxyLightPositions[1].xyz, i.inputs.NormalWs.xyz );
			vLightDirectionValidMask.z = dot( g_vAmbientOcclusionProxyLightPositions[2].xyz, i.inputs.NormalWs.xyz );
			vLightDirectionValidMask.w = dot( g_vAmbientOcclusionProxyLightPositions[3].xyz, i.inputs.NormalWs.xyz );
			vLightDirectionValidMask.xyzw = max( vLightDirectionValidMask.xyzw, float4( 0.0, 0.0, 0.0, 0.0 ) );

			const float flAoProxyDownres = g_vAoProxyDownres.x;
			float2 vAoProxyTexelSize = 1.0 / TextureDimensions2D( g_tDynamicAmbientOcclusionDepth, 0 ).xy;

			//
			// Gather depth values in 2x2 neighborhood
			//
			float2 vAoProxyUv = floor( vPositionSs.xy * flAoProxyDownres ) * vAoProxyTexelSize.xy + 0.5 * vAoProxyTexelSize.xy;
			float4 v4Depths = g_tDynamicAmbientOcclusionDepth.GatherRed( g_sCookieSampler, vAoProxyUv.xy );
			float4 vDepthDiffs = ( v4Depths.xyzw - vPositionSs.zzzz );

			//
			// Calculate offset to smallest depth difference
			//
			float flMinDist = vDepthDiffs.w;
			float2 vOffset = float2( 0.0, 0.0 );
			if ( abs( vDepthDiffs.z ) < flMinDist )
			{
				flMinDist = vDepthDiffs.z;
				vOffset = float2( vAoProxyTexelSize.x, 0.0 );
			}
			if ( abs( vDepthDiffs.x ) < flMinDist )
			{
				flMinDist = vDepthDiffs.x;
				vOffset = float2( 0.0, vAoProxyTexelSize.y );
			}
			if ( abs( vDepthDiffs.y ) < flMinDist )
			{
				flMinDist = vDepthDiffs.y;
				vOffset = vAoProxyTexelSize.xy;
			}

			//
			// Lookup directional AO texel
			//
			float4 vAoProxyTexel = AttributeTex2DS( g_tDynamicAmbientOcclusion, g_sPointClamp, vAoProxyUv.xy + vOffset.xy );

			//
			// Calculate relative contributions of all the lights
			//
			float4 vLightingComposition = vLightDirectionValidMask.xyzw;
			float flSum = dot( vLightingComposition.xyzw, float4( 1.0, 1.0, 1.0, 1.0 ) ) + g_vAmbientOcclusionProxyAmbientStrength.x;
			flSum = rcp( flSum );

			return flSum * ( g_vAmbientOcclusionProxyAmbientStrength.x + dot( vLightingComposition.xyzw, vAoProxyTexel.xyzw ) );
		}

        return 1.0f;

    }

    //
    // Get ambient occlusion info from the lightmap if available
    //
    static float GetBakedAmbientOcclusion( LegacyShadeParams i )
    {
        #if ( D_BAKED_LIGHTING_FROM_LIGHTMAP )
            float2 vLightmapUV = i.inputs.PositionLightmap;
            float4 vAHDData = Tex2DArrayS( LightMap( 3 ), g_sTrilinearClamp, float3( i.inputs.PositionLightmap, 0 ) );
            return vAHDData.w;
        #endif

        return 1.0f;
    }


    static float4 Shade( PixelInput i, Material m, ShadingLegacyConfig config )
    {
        LegacyShadeParams shadeParams = LegacyShadeParams::ProcessMaterial( i, m );

        LegacyLightResult vLightResult = LegacyLightResult::Init();


        //
        // Shade direct lighting for dynamic and static lights
        //
        uint index;
        for ( index = 0; index < DynamicLight::Count( i ); index++ )
        {
            Light light = DynamicLight::From( i, index );
            vLightResult = LegacyLightResult::Sum( vLightResult, Direct( shadeParams, config, light ) );
        }

        [unroll]
        for ( index = 0; index < StaticLight::Count( i ); index++ )
        {
            Light light = StaticLight::From( i, index );
            if( light.Visibility > 0.0f )
                vLightResult = LegacyLightResult::Sum( vLightResult, Direct( shadeParams, config, light ) );
        }

        //
        // Shade indirect lighting
        //
        vLightResult = LegacyLightResult::Sum( vLightResult, Indirect( shadeParams, config ) );

        // modulate the final results

        // Apply Iris lighting
        // iris lighting is added to final diffuse, before it gets modulated by iris/pupil/sclera color (material.Albedo)
        float3 finalDiffuse = DiffuseModulate( shadeParams, config, vLightResult.Diffuse + vLightResult.Iris );

        #ifdef CUSTOM_DIFFUSE_MODULATE
            finalDiffuse = CustomDiffuseModulate(finalDiffuse, shadeParams.Albedo, shadeParams.SelfIllumMask, shadeParams.DiffuseModControls);
        #endif // CUSTOM_DIFFUSE_MODULATE

        // PixelShaderDoSpecularLighting does nothing for ambient. Individual shaders (VertexLitGeneric, skin, etc) should add their own ambient envmapping.
        float3 finalSpecular = SpecularModulate( shadeParams, config, vLightResult.Specular );

        
        // Apply Rim lighting
        if ( config.DoSpecular && config.DoRimLighting )
        {
            float3 ambient = PixelShaderAmbientLight( shadeParams, config );

            float fRimMultiply = shadeParams.RimMask * shadeParams.RimFresnel; // both unit range: [0, 1]
 
			// Add in rim light modulated with tint, mask and traditional Fresnel (not using Fresnel ranges)
            vLightResult.Rim *= fRimMultiply;

            finalSpecular = max( finalSpecular, vLightResult.Rim );
			
            // not in SDK
            float3 specAmbient = ambient;
            if ( config.DoAmbientOcclusion )
            {
                float3 vAmbientOcclusion = specularAO( shadeParams, GetAmbientOcclusion(shadeParams) );
                /*
                if( shadeParams.config.AmbientOcclusionModel >= AMBIENT_OCCLUSION_MULTIBOUNCE )
                    vAmbientOcclusion = gtaoMultiBounce( vAmbientOcclusion.x, shadeParams.Albedo.rgb ).xxx;
                */

                specAmbient *= lerp(float3(1.0f, 1.0f, 1.0f), vAmbientOcclusion, g_flAmbientOcclusionDirectAmbient);
            }

			// Add in view-ray lookup from ambient cube
            finalSpecular += (specAmbient * g_flRimBoost) * saturate(fRimMultiply * shadeParams.inputs.NormalWs.z);
        }

        float4 result = float4( finalDiffuse + finalSpecular, m.Opacity );

        // Apply Envmap
        float3 envMapColor = PixelShaderCubemap( i, m, shadeParams, config );

        if ( config.DoIrisLighting )
        {
            // for eyes, the reflection is multiplied by the diffuse term (without albedo color or iris lighting)
            envMapColor *= vLightResult.Diffuse;
        }

        // Not in SDK:
        // cubemaps also count as indirect specular lighting, so multiply by specular AO here aswell
        if ( config.DoAmbientOcclusion )
        {
            float3 vAmbientOcclusion = specularAO( shadeParams, GetAmbientOcclusion(shadeParams) );
            /*
            if( shadeParams.config.AmbientOcclusionModel >= AMBIENT_OCCLUSION_MULTIBOUNCE )
                vAmbientOcclusion = gtaoMultiBounce( vAmbientOcclusion.x, shadeParams.Albedo.rgb ).xxx;
            */

            envMapColor *= vAmbientOcclusion;
        }

        result.rgb += envMapColor;

        return ShadingModel::Finalize( i, m, result );
    }
};

#endif // SOURCEBOX_LEGACY_LIGHTING_H