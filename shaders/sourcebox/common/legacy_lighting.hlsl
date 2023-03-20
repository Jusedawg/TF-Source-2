// https://github.com/ValveSoftware/source-sdk-2013/blob/master/sp/src/materialsystem/stdshaders/common_vertexlitgeneric_dx9.h
// Legacy VertexLitGeneric style lighting

#ifndef SOURCEBOX_LEGACY_LIGHTING_H
#define SOURCEBOX_LEGACY_LIGHTING_H

#ifndef SOURCEBOX_LEGACY_MATERIAL_H
#error Material struct must be overriden first!
#endif // SOURCEBOX_LEGACY_MATERIAL_H

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


class ShadingLegacyConfig {
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
            TangentWs = pixelInput.vTangentUWs;
            BinormalWs = ( pixelInput.vTangentVWs.xyz * 2.0f ) - 1.0f;
        #else // !PS_INPUT_HAS_TANGENT_BASIS
            TangentWs = float3(1.0, 0.0, 0.0);
            BinormalWs = float3(0.0, 1.0, 0.0);
        #endif // !PS_INPUT_HAS_TANGENT_BASIS

        ViewRayTs = Vec3WorldToTangentNormalized(ViewRayWs.xyz, NormalWs.xyz, TangentWs.xyz, BinormalWs.xyz);
        CorneaNormalTs = material.CorneaNormal;
        CorneaNormalWs = Vec3TangentToWorldNormalized( CorneaNormalTs.xyz, NormalWs.xyz, TangentWs.xyz, BinormalWs.xyz );
        CorneaReflectionWs = reflect( ViewRayWs.xyz, CorneaNormalWs.xyz );
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
    float3  EnvMapColor;

    float3  SelfIllumMask;

    // used by EyeRefract
    float IrisHighlightMask;
    float AverageAmbient;

    LegacyShadeInputs inputs;

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
        EnvMapColor = m.EnvMapColor;
        
        // used by EyeRefract
        IrisHighlightMask = m.IrisHighlightMask;
        AverageAmbient = m.AverageAmbient;
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

class ShadingModelLegacy : ShadingModel
{
    LegacyShadeParams shadeParams;
    ShadingLegacyConfig config;
	
    // Some things operate on the final sums of these values (modulate, iris lighting, envmap multiplier),
    // and PostProcess() doesn't give us separate diffuse/specular. So we add them up internally, and apply them
    // to the result manually within PostProcess()
	float3 m_sumDiffuseLighting;
	float3 m_sumSpecularLighting;
	float3 m_sumRimLighting;
	float3 m_sumIrisLighting;
	
    //
    // Consumes a material and converts it to the internal shading parameters,
    // That is more easily consumed by the shader.
    //
    void Init( const PixelInput pixelInput, const Material material )
    {
        shadeParams = LegacyShadeParams::ProcessMaterial( pixelInput, material );
		m_sumRimLighting = float3( 0.0, 0.0, 0.0 );
		m_sumDiffuseLighting = float3( 0.0, 0.0, 0.0 );
		m_sumSpecularLighting = float3( 0.0, 0.0, 0.0 );
		m_sumIrisLighting = float3( 0.0, 0.0, 0.0 );
    }
    
    //-----------------------------------------------------------------------------
    // Purpose: Compute scalar diffuse term with various optional tweaks such as
    //          Half Lambert and ambient occlusion
    //-----------------------------------------------------------------------------
    float3 DiffuseTerm( const LightData light )
    {
        float fResult;

        float NDotL = dot( shadeParams.inputs.NormalWs, light.LightDir );				// Unsaturated dot (-1 to 1 range)
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
            fResult *= shadeParams.AmbientOcclusion.r;
        }

        float3 fOut = float3( fResult, fResult, fResult );
		
		// from SDK, only works if lightwarp texture is RGB
        if ( config.DoLightingWarp )
        {
            // fOut = 2.0f * Tex1D( g_tLightWarpTexture, fResult );
            fOut = 2.0f * Tex2DLevel( g_tLightWarpTexture, float2( fResult, 0 ), 0 ).rgb;
        }

        if ( config.DoAmbientOcclusion && config.DoIrisLighting )
        {
            fOut *= shadeParams.AmbientOcclusion.rgb;
        }

        return fOut;
    }

    float3 PixelShaderDoGeneralDiffuseLight( const LightData light )
    {
        return light.Color * light.Attenuation * light.Visibility * DiffuseTerm( light );
    }

    void SpecularAndRimTerms( const LightData light, const float3 color, out float3 specularLighting, out float3 rimLighting )
    {
        rimLighting = float3(0.0f, 0.0f, 0.0f);

        //float3 vReflect = reflect( -vEyeDir, vWorldNormal );				// Reflect view through normal
        //float3 vReflect = 2 * vWorldNormal * dot( vWorldNormal , vEyeDir ) - vEyeDir; // Reflect view through normal
        float3 vReflect = shadeParams.inputs.ReflectRayWs;

        float LdotR = saturate(dot( vReflect, light.LightDir ));					// L.R	(use half-angle instead?)
        specularLighting = pow( LdotR, shadeParams.SpecularExponent );					// Raise to specular exponent

        // Optionally warp as function of scalar specular and fresnel
        if ( config.DoSpecularWarp )
            specularLighting *= Tex2DLevel( g_tSpecularWarpTexture, float2(specularLighting.x, shadeParams.Fresnel), 0 ).rgb; // Sample at { (L.R)^k, fresnel }

        // specularLighting *= saturate(dot( vWorldNormal, vLightDir ));		// Mask with N.L
        specularLighting *= saturate(light.NdotL);		                            // Mask with N.L
        specularLighting *= color;											// Modulate with light color
        
        if ( config.DoAmbientOcclusion && !config.DoIrisLighting )			// Optionally modulate with ambient occlusion
            specularLighting *= shadeParams.AmbientOcclusion.r;

        if ( config.DoRimLighting )											// Optionally do rim lighting
        {
            rimLighting  = pow( LdotR, shadeParams.RimExponent );			// Raise to rim exponent
            // rimLighting *= saturate(dot( vWorldNormal, vLightDir ));		// Mask with N.L
            rimLighting *= saturate(light.NdotL);		                    // Mask with N.L
            rimLighting *= color;											// Modulate with light color
        }

        // not in SDK
        specularLighting *= light.Visibility;
		// rim lighting shouldn't be affected by shadows
        // rimLighting *= light.Visibility;
    }

    void PixelShaderDoSpecularLight( const LightData light, out float3 specularLighting, out float3 rimLighting )
    {
        // Compute Specular and rim terms
        SpecularAndRimTerms( light, light.Color * light.Attenuation, specularLighting, rimLighting );
    }

    float3 DiffuseModulate( float3 diffuse )
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

    float3 SpecularModulate( float3 specular )
    {
        return specular * shadeParams.SpecularTint;
    }

    float3 DoIrisCausticLighting( const LightData light )
    {
        float3 vIrisTangentNormal = shadeParams.inputs.CorneaNormalTs.xyz;
        vIrisTangentNormal.xy *= -2.5f; // I'm not normalizing on purpose
        
        // for ( int j=0; j < nNumLights; j++ )
        // {
            // World light vector
            float3 vWorldLightVector = light.LightDir;

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
    LightShade Direct( const LightData light )
    {
        LightShade lightShade;
        
        lightShade.Diffuse = float3( 0.0, 0.0, 0.0 );
        if ( config.DoDiffuse )
        {
            // Don't add it in, we need this data for EyeRefract cornea stuff
            // lightShade.Diffuse = DiffuseModulate( PixelShaderDoGeneralDiffuseLight( light ) );
            m_sumDiffuseLighting += PixelShaderDoGeneralDiffuseLight( light );
        }

        if ( config.DoIrisLighting )
        {
            m_sumIrisLighting += DoIrisCausticLighting( light );
        }

        lightShade.Specular = float3( 0.0, 0.0, 0.0 );

        if ( config.DoSpecular )
        {
            float3 specularLighting = float3( 0.0f, 0.0f, 0.0f );
	        float3 rimLighting = float3( 0.0f, 0.0f, 0.0f );

            PixelShaderDoSpecularLight( light, specularLighting, rimLighting );

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
				m_sumRimLighting += rimLighting;
				
                // Add in view-ray lookup from ambient cube (moved to Indirect)
            }
            
            // lightShade.Specular = SpecularModulate( specularLighting );
			m_sumSpecularLighting += specularLighting;
        }

        // Giant iris specular highlights
        if ( config.DoIrisLighting )
        {
			m_sumSpecularLighting += pow( saturate( dot( shadeParams.inputs.CorneaReflectionWs.xyz, light.LightDir ) ), 128.0f ) * light.Attenuation * light.Color;
        }

        return lightShade;
    }
    
    float3 PixelShaderAmbientLight( const float3 worldNormal, const float3 cAmbientCube[6] )
    {
        float3 linearColor, nSquared = worldNormal * worldNormal;
        float3 isNegative = ( worldNormal >= 0.0 ) ? 0 : nSquared;
        float3 isPositive = ( worldNormal >= 0.0 ) ? nSquared : 0;
        linearColor = isPositive.x * cAmbientCube[0] + isNegative.x * cAmbientCube[1] +
                    isPositive.y * cAmbientCube[2] + isNegative.y * cAmbientCube[3] +
                    isPositive.z * cAmbientCube[4] + isNegative.z * cAmbientCube[5];
        return linearColor;
    }

    //
    // Executed for indirect lighting, combine ambient occlusion, etc.
    //
    LightShade Indirect()
    {
        LightShade lightShade;

        // float3 ambient = PixelShaderAmbientLight( shadeParams.inputs.NormalWs, shadeParams.AmbientCube );
        #ifdef USE_MANUAL_AMBIENT
            float3 ambientCube[6];
            // shadeParams.GetAmbientCube( ambientCube );
            GetAmbientCube( shadeParams.inputs.PositionWs, ambientCube );
            float3 ambient = PixelShaderAmbientLight( shadeParams.inputs.NormalWs, ambientCube );
        #else // !USE_MANUAL_AMBIENT
            float3 ambient = SampleLightProbeVolume( shadeParams.inputs.PositionWs, shadeParams.inputs.NormalWs );
        #endif // !USE_MANUAL_AMBIENT

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
                if ( config.DoAmbientOcclusion && !config.DoIrisLighting )
                    ambient *= shadeParams.AmbientOcclusion.r * shadeParams.AmbientOcclusion.r;	// Note squaring...

                linearColor += ambient;
            }

            m_sumDiffuseLighting += linearColor;
        }

        // finalize the lighting
        if ( config.DoIrisLighting )
        {
            float3 vIrisTangentNormal = shadeParams.inputs.CorneaNormalTs.xyz;
            vIrisTangentNormal.xy *= -2.5f; // I'm not normalizing on purpose
        
            // Add slight view dependent iris lighting based on ambient light intensity to enhance situations with no local lights (0.5f is to help keep it subtle)
            m_sumIrisLighting.rgb += saturate( dot( vIrisTangentNormal, -shadeParams.inputs.ViewRayTs.xyz ) ) * shadeParams.AverageAmbient * shadeParams.IrisHighlightMask * 0.5f;

            // iris lighting is added to final diffuse, before it gets modulated by iris/pupil/sclera color (material.Albedo)
            m_sumDiffuseLighting += m_sumIrisLighting;
        }

        lightShade.Diffuse = DiffuseModulate( m_sumDiffuseLighting );
        // PixelShaderDoSpecularLighting does nothing for ambient. Individual shaders (VertexLitGeneric, skin, etc) should add their own ambient envmapping.
        lightShade.Specular = SpecularModulate( m_sumSpecularLighting );

        
        if ( config.DoSpecular && config.DoRimLighting )
        {
            float fRimMultiply = shadeParams.RimMask * shadeParams.RimFresnel; // both unit range: [0, 1]
 
			// Add in rim light modulated with tint, mask and traditional Fresnel (not using Fresnel ranges)
            m_sumRimLighting *= fRimMultiply;

            lightShade.Specular = max( lightShade.Specular, m_sumRimLighting );
			
			// Add in view-ray lookup from ambient cube
            lightShade.Specular += (ambient * g_flRimBoost) * saturate(fRimMultiply * shadeParams.inputs.NormalWs.z);
        }
        
        return lightShade;
    }

    // copied from pixel.shading.standard.indirect.hlsl
    // Keep these in sync if Sam changes anything (but make sure to delete roughness LOD stuff since we aren't PBR)
    static float4 GetCubemap( LegacyShadeParams input, uint nCubeIndex, float2 vAnisotropy = 0.0f, float flRetroReflectivity = 0.0f )
    {
        const float3 vPositionWs = input.inputs.PositionWs;
        const float3 vNormalWs = input.inputs.NormalWs;
        
        // Todo: fixme these
        const float3 vTangentUWs = 0.0f;
        const float3 vTangentVWs = 0.0f;
        const uint nView = 0;

        float3 vCubePos = mul( float4( vPositionWs.xyz, 1.0 ), EnvMapWorldToLocal( nCubeIndex ) ).xyz;

        const float3 vEnvMapMin = EnvMapBoxMins( nCubeIndex );
        const float3 vEnvMapMax = EnvMapBoxMaxs( nCubeIndex );

        float3 vIntersectA = min( ( vCubePos - vEnvMapMin ), ( vEnvMapMax - vCubePos ) ) ;
        float3 vIntersectB = min( ( vCubePos - vEnvMapMax ), ( vEnvMapMin - vCubePos ) ) ;

        float flDistance = min(
            min( vIntersectA.x, min( vIntersectA.y, vIntersectA.z ) ),
            min( -vIntersectB.x, -min( vIntersectB.y, vIntersectB.z ) )
        );

        //
        // Normalize cubemaps to maintain appropriate specular/diffuse balance
        //
        
        float3 vParallaxReflectionCubemapLocal = CalcParallaxReflectionCubemapLocal( vPositionWs, vNormalWs, vAnisotropy, flRetroReflectivity, vTangentUWs, vTangentVWs, nView, nCubeIndex );
        float3 vCubeMapTexel = SampleEnvironmentMapLevel( vParallaxReflectionCubemapLocal.xyz, 0, nCubeIndex, g_vEnvCubeMapArrayIndices[nCubeIndex] );

        return float4( vCubeMapTexel, flDistance );
    }

    float3 GetAllCubemaps( LegacyShadeParams input, uint nCubeIndex )
    {
        const float fSmoothRatio = 0.02;

        uint2 vTile = GetTileForScreenPosition( input.inputs.PositionSs );

        const uint nNumEnvMaps = GetNumEnvMaps( vTile );

        float3 vEnvmapColor = 0;
        float flDistAccumulated = 0;

        for(uint i=0;i<=nNumEnvMaps; i++)
        {
            uint nCubeIndex = TranslateEnvMapIndex( nNumEnvMaps - i, vTile );
            float4 vSampledCube = GetCubemap( input, nCubeIndex );

            vEnvmapColor.xyz = lerp( vEnvmapColor, vSampledCube.xyz, 1.0 - flDistAccumulated );
            flDistAccumulated += clamp( vSampledCube.w * fSmoothRatio, 0.0, 1.0);

            if( flDistAccumulated >= 1.0 )
                break;
        }

        return vEnvmapColor.xyz;
    }

    float4 PostProcess( float4 vColor )
    {
        #if (USE_MANUAL_CUBEMAP)
            float3 envMapColor = shadeParams.EnvMapColor;
        #else // !USE_MANUAL_CUBEMAP
            float3 envMapColor = GetAllCubemaps(shadeParams, 0);
        #endif // !USE_MANUAL_CUBEMAP

        if ( config.DoIrisLighting )
        {
            // for eyes, the reflection is multiplied by the diffuse term (without albedo color or iris lighting)
            envMapColor *= m_sumDiffuseLighting;
        }

        // TODO fog
        float3 result = vColor.rgb + envMapColor;

        return float4( result, shadeParams.Opacity );
    }
};

#endif // SOURCEBOX_LEGACY_LIGHTING_H