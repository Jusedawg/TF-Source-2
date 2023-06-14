//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	// CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "SourceBox eyerefract shader. See license.txt for license information";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "sourcebox/common/legacy_features.hlsl"

    Feature( F_HALFLAMBERT,                         0..1, "Rendering" );
    Feature( F_LIGHTWARPTEXTURE,                    0..1, "Rendering" );
    Feature( F_SPHERETEXKILLCOMBO,                  0..1, "Rendering" );
    Feature( F_RAYTRACESPHERE,                      0..1, "Rendering" );

    FeatureRule(Requires1(F_SPHERETEXKILLCOMBO, F_RAYTRACESPHERE), "");
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
    VrForward();													// Indicates this shader will be used for main rendering
    Depth( "depth_only.vfx" ); 									// Shader that will be used for shadowing and depth prepass
    ToolsVis( S_MODE_TOOLS_VIS ); 									// Ability to see in the editor
    // ToolsWireframe( "tools_wireframe.vfx" ); 					// Allows for mat_wireframe to work
	// ToolsShadingComplexity( "tools_shading_complexity.vfx" ); 	// Shows how expensive drawing is in debug view
}

//=========================================================================================================================
COMMON
{
	//#include "common/shared.hlsl"
    #include "system.fxc" // This should always be the first include in COMMON
    #include "sbox_shared.fxc"
    #define VS_INPUT_HAS_TANGENT_BASIS 1
    #define PS_INPUT_HAS_TANGENT_BASIS 1

    // defaults to 0. only used for intro?
    float3 g_vEyeOrigin            < UiGroup( "Attributes,11/14" ); Default3(0.0f, 0.0f, 0.0f); >;

    // kDefaultIrisU/kDefaultIrisV
    // defaults to make it look good in the material editor
    float4 g_vIrisProjectionU      < UiGroup( "Attributes,11/14" ); Default4(0.0f, 0.05f, 0.0f, 0.5f); >;
    float4 g_vIrisProjectionV      < UiGroup( "Attributes,11/14" ); Default4(0.0f, 0.0f, 0.05f, 0.5f); >;
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
    // float3 vWorldTangent : TEXCOORD14;
    // float3 vWorldBinormal : TEXCOORD15;
    float3 vTangentViewVector : TEXCOORD14; 
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	//
	// Main
	//

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
    
    // https://github.com/ValveSoftware/source-sdk-2013/blob/0d8dceea4310fde5706b3ce1c70609d72a38efdf/sp/src/materialsystem/stdshaders/eye_refract_vs20.fxc
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );

        // Note: I'm relying on the iris projection vector math not changing or this will break
        float3 vEyeSocketUpVector = normalize( -g_vIrisProjectionV.xyz );
        float3 vEyeSocketLeftVector = normalize( -g_vIrisProjectionU.xyz );

        // Normal = (Pos - Eye origin)
        float3 vWorldNormal = normalize( o.vPositionWs - g_vEyeOrigin.xyz );
        o.vNormalWs.xyz = vWorldNormal.xyz;

        float3 vWorldTangent = normalize( cross( vEyeSocketUpVector.xyz, vWorldNormal.xyz ) );
        // o.vWorldTangent.xyz = vWorldTangent.xyz;
        o.vTangentUWs.xyz = vWorldTangent.xyz;

        float3 vWorldBinormal = normalize( cross( vWorldNormal.xyz, vWorldTangent.xyz ) );
        // o.vWorldBinormal.xyz = vWorldBinormal.xyz * 0.5f + 0.5f;
        // o.vTangentVWs.xyz = vWorldBinormal.xyz * 0.5f + 0.5f;
        o.vTangentVWs.xyz = vWorldBinormal.xyz;


        float3 vWorldViewVector = -CalculatePositionToCameraDirWs( o.vPositionWs );
        // float3 vTangentViewVector = Vec3WorldToTangentNormalized(vWorldViewVector.xyz, vWorldNormal.xyz, vWorldTangent.xyz, vWorldBinormal.xyz);
        o.vTangentViewVector = Vec3WorldToTangentNormalized(vWorldViewVector.xyz, vWorldNormal.xyz, vWorldTangent.xyz, vWorldBinormal.xyz);
        
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
    StaticCombo( S_HALFLAMBERT                          , F_HALFLAMBERT                         , Sys( ALL ) );
    StaticCombo( S_LIGHTWARPTEXTURE                     , F_LIGHTWARPTEXTURE                    , Sys( ALL ) );
    StaticCombo( S_SPHERETEXKILLCOMBO                   , F_SPHERETEXKILLCOMBO                  , Sys( ALL ) );
    StaticCombo( S_RAYTRACESPHERE                       , F_RAYTRACESPHERE                      , Sys( ALL ) );

    // Cornea normal - not SRGB!
    CreateInputTexture2D( Cornea, Linear, 8, "", "", "Eyes,1/1", Default3( 1.0, 1.0, 1.0 ) );
    CreateTexture2D( g_tCornea ) < Channel( RGB, Box( Cornea ), Linear ); OutputFormat( DXT5 ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( false ); >;

    // Iris
    CreateInputTexture2D( Iris, Srgb, 8, "", "", "Eyes,1/2", Default3( 1.0, 1.0, 1.0 ) );
    CreateTexture2D( g_tIris ) < Channel( RGB, Box( Iris ), Srgb ); OutputFormat( BC7 ); AddressU( CLAMP ); AddressV( CLAMP ); SrgbRead( true ); >;

    // Eye reflection cubemap
    // CreateInputTextureCube( Envmap, Srgb, 8, "", "", "Material,10/14", Default3( 1.0, 1.0, 1.0 ) );
    // CreateTextureCube( g_tEnvmap ) < Channel( RGBA, Box( Envmap ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( true ); >;

    // the eye AO texture is SRGB.
    #define AO_TEXTURE_IS_SRGB 1

    // Diffuse warp texture
    // CreateInputTexture2D( LightwarpTexture, Linear, 8, "", "", "Material,10/16", Default3( 1.0, 1.0, 1.0 ) );
    // CreateTexture2D( g_tLightWarpTexture ) < Channel( R, Box( LightwarpTexture ), Linear ); OutputFormat( R32F ); SrgbRead( false ); >;


    float g_flDilationFactor       < UiGroup( "Eyes,1/3" ); Range(0.0f, 1.0f); Default(0.5f); >;
    float g_flGlossiness           < UiGroup( "Eyes,1/4" ); Range(0.0f, 1.0f); Default(1.0f); >;
    // float g_flAverageAmbient       < UiGroup( "Eyes,1/5" ); Range(0.0f, 1.0f); Default(0.5f); >;
    float g_flCorneaBumpStrength   < UiGroup( "Eyes,1/6" ); Range(0.0f, 1.0f); Default(1.0f); >;
    float g_flEyeballRadius        < UiGroup( "Eyes,1/7" ); Range(0.0f, 1.0f); Default(0.5f); >;
    float g_flParallaxStrength     < UiGroup( "Eyes,1/8" ); Range(0.0f, 1.0f); Default(0.25f); >;
    float3 g_vAmbientOcclColor     < UiType( Color ); UiGroup( "Eyes,1/9" ); Default3(0.33f, 0.33f, 0.33f); >;

    #define USE_MANUAL_CUBEMAP 1
    float3 GetEnvmapColor( float3 envmapBase, float3 envmapMask, float fresnelRanges )
    {
        return g_flGlossiness * envmapBase;
    }

    #include "sourcebox/common/legacy_pixel.hlsl"
    

    // Ray sphere intersect returns distance along ray to intersection ================================
    float IntersectRaySphere( float3 cameraPos, float3 ray, float3 sphereCenter, float sphereRadius)
    {
        float3 dst = cameraPos.xyz - sphereCenter.xyz;
        float B = dot(dst, ray);
        float C = dot(dst, dst) - (sphereRadius * sphereRadius);
        float D = B*B - C;
        return (D > 0) ? (-B - sqrt(D)) : 0;
    }

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

    // https://github.com/ValveSoftware/source-sdk-2013/blob/0d8dceea4310fde5706b3ce1c70609d72a38efdf/sp/src/materialsystem/stdshaders/eye_refract_ps2x.fxc
	float4 MainPs( PixelInput i ) : SV_Target0
	{
        bool bDoDiffuseWarp = S_LIGHTWARPTEXTURE ? true : false;
        bool bRayCast = S_RAYTRACESPHERE ? true : false;
	    bool bRayCastTexKill = S_SPHERETEXKILLCOMBO ? true : false;

        float3 vWorldPosition = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs;

        // Interpolated vectors
        float3 vWorldNormal = i.vNormalWs.xyz;
        // float3 vWorldTangent = i.vWorldTangent.xyz;
        float3 vWorldTangent = normalize(i.vTangentUWs.xyz);
        float3 vWorldBinormal = normalize(i.vTangentVWs.xyz);

        float3 vTangentViewVector = i.vTangentViewVector.xyz;
        float3 vWorldViewVector = -CalculatePositionToCameraDirWs( vWorldPosition );

        // move the world position around (doesn't affect lighting, SDK shader uses the constant directly)
        if ( bRayCast )
        {
            float fSphereRayCastDistance = IntersectRaySphere( g_vCameraPositionWs.xyz, vWorldViewVector.xyz, g_vEyeOrigin.xyz, g_flEyeballRadius );
            vWorldPosition.xyz = g_vCameraPositionWs.xyz + ( vWorldViewVector.xyz * fSphereRayCastDistance );
            if (fSphereRayCastDistance == 0)
            {
                if ( bRayCastTexKill )
                    discard; // texkill to get a better silhouette
                vWorldPosition.xyz = g_vEyeOrigin.xyz + ( vWorldNormal.xyz * g_flEyeballRadius );
            }
        }

        float2 vCorneaUv; // Note: Cornea texture is a cropped version of the iris texture
        vCorneaUv.x = dot( g_vIrisProjectionU, float4( vWorldPosition, 1.0f ) );
        vCorneaUv.y = dot( g_vIrisProjectionV, float4( vWorldPosition, 1.0f ) );
        float2 vSphereUv = ( vCorneaUv.xy * 0.5f ) + 0.25f;

        //=================================//
        // Hacked parallax mapping on iris //
        //=================================//
        float fIrisOffset = Tex2D( g_tCornea, vCorneaUv.xy ).b;
        
        float2 vParallaxVector = ( ( vTangentViewVector.xy * fIrisOffset * g_flParallaxStrength ) / ( 1.0f - vTangentViewVector.z ) ); // Note: 0.25 is a magic number
        vParallaxVector.x = -vParallaxVector.x; //Need to flip x...not sure why.

        float2 vIrisUv = vSphereUv.xy - vParallaxVector.xy;

        // Note: We fetch from this texture twice right now with different uv's for the color and alpha
        float2 vCorneaNoiseUv = vSphereUv.xy + ( vParallaxVector.xy * 0.5 );
        float fCorneaNoise = Tex2D( g_tIris, vCorneaNoiseUv.xy ).a;
        
        //===============//
        // Cornea normal //
        //===============//
        // Sample 2D normal from texture
        float3 vCorneaTangentNormal = float3(0.0, 0.0, 1.0);
        float4 vCorneaSample = Tex2D( g_tCornea, vCorneaUv.xy );
        vCorneaTangentNormal.xy = vCorneaSample.rg - 0.5f; // Note: This scales the bump to 50% strength

        // Scale strength of normal
        vCorneaTangentNormal.xy *= g_flCorneaBumpStrength;

        // Add in surface noise and imperfections (NOTE: This should be baked into the normal map!)
        vCorneaTangentNormal.xy += fCorneaNoise * 0.1f;

        // Normalize tangent vector
        vCorneaTangentNormal.xyz = normalize( vCorneaTangentNormal.xyz );

        // Transform into world space
        float3 vCorneaWorldNormal = Vec3TangentToWorldNormalized( vCorneaTangentNormal.xyz, vWorldNormal.xyz, vWorldTangent.xyz, vWorldBinormal.xyz );

        //==============//
        // Dilate pupil //
        //==============//
        vIrisUv.xy -= 0.5f; // Center around (0,0)
        float fPupilCenterToBorder = saturate( length( vIrisUv.xy ) / 0.2f ); //Note: 0.2 is the uv radius of the iris
        float fPupilDilateFactor = g_flDilationFactor; // This value should be between 0-1
        vIrisUv.xy *= lerp (1.0f, fPupilCenterToBorder, saturate( fPupilDilateFactor ) * 2.5f - 1.25f );
        vIrisUv.xy += 0.5f;

        //============//
        // Iris color //
        //============//
        float4 cIrisColor = Tex2D( g_tIris, vIrisUv.xy );
        
        //===================//
        // Ambient occlusion //
        //===================//
        float3 cAmbientOcclFromTexture = Tex2DS( g_tAmbientOcclusionTexture, TextureFiltering, i.vTextureCoords.xy ).rgb;
        float3 cAmbientOcclColor = lerp( g_vAmbientOcclColor, 1.0f, cAmbientOcclFromTexture.rgb ); // Color the ambient occlusion
        
        //==========================//
        // Reflection from cube map //
        //==========================//
        // float3 vCorneaReflectionVector = reflect( vWorldViewVector.xyz, vCorneaWorldNormal.xyz );
	    // float3 cReflection = g_flGlossiness * TexCube( g_tEnvMap, vCorneaReflectionVector.xyz ).rgb;


        // AV - I think this will effectively make the eyeball less rounded left to right to help vertext lighting quality
        // AV - Note: This probably won't look good if put on an exposed eyeball
        float3 vEyeSocketUpVector = normalize( -g_vIrisProjectionV.xyz );
        float3 vEyeSocketLeftVector = normalize( -g_vIrisProjectionU.xyz );
        float vNormalDotSideVec = -dot( vWorldNormal, vEyeSocketLeftVector) * 0.5f;
        float3 vBentWorldNormal = normalize(vNormalDotSideVec * vEyeSocketLeftVector + vWorldNormal);
        


        // DO LIGHTING (originally in VS)
        ShadingLegacyConfig config = ShadingLegacyConfig::GetDefault();
        config.DoDiffuse = true;
        config.HalfLambert = S_HALFLAMBERT ? true : false;
        config.DoAmbientOcclusion = true;
        config.DoLightingWarp = S_LIGHTWARPTEXTURE ? true : false;
        config.DoRimLighting = false;
        config.DoSpecularWarp = false;
        config.DoSpecular = false;
        config.DoIrisLighting = true;

        config.SelfIllum = false;
        config.SelfIllumFresnel = false;

        config.StaticLight = false;
        config.AmbientLight = true;

        // Diffuse = cIrisColor + fCorneaNoise * 0.1f
        // Normal = vBentWorldNormal
        // EnvMapColor = cReflection

        // LegacyMaterial m = GetDefaultLegacyMaterial();
        Material m = GetDefaultLegacyMaterial();
        m.Albedo = cIrisColor.rgb + fCorneaNoise * 0.1f;
        m.Normal = vBentWorldNormal;
        m.AmbientOcclusion = cAmbientOcclColor;
        m.EnvmapMask = float3(1.0, 1.0, 1.0);
        m.CorneaNormal = vCorneaTangentNormal.xyz;
        m.IrisHighlightMask = Tex2D( g_tCornea, vCorneaUv.xy ).a;
        // m.AverageAmbient = g_flAverageAmbient;

        // PixelInput, Material, Shading Model
        return FinalizeLegacyOutput(ShadingModelLegacy::Shade( i, m, config ));
	}
}