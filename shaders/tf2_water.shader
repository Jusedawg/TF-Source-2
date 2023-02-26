HEADER
{
	Description = "TF2 basic water";
}

MODES
{
	VrForward();
    Depth( "vr_depth_only.vfx" );
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.vfx" );
	ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" );
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"

    #define S_TRANSLUCENT 1
    #define DEPTH_STATE_ALREADY_SET 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PS_INPUT
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

	RenderState( DepthEnable, true );
	RenderState( DepthWriteEnable, false );

    float flRefraction < Default( 6 ); Range(0, 10); UiGroup( "TF:S2,10/,10/1" );>;
    FloatAttribute(flRefraction, flRefraction);

    float4 g_FogColorTint < Default4( 0.2f, 0.2f, 0.2f, 0.5f ); UiType( Color ); UiGroup( "TF:S2,10/,10/1" ); >;
    Float4Attribute(g_FogColorTint, g_FogColorTint);

    float2 g_fDepthScale < Default2( 0, 50 ); Range2(0, 0, 1000, 1000); UiGroup( "TF:S2,10/,10/1" );>;
    Float2Attribute(g_fDepthScale, g_fDepthScale);

    float flAlphaFade < Default( 0 ); Range(0, 1); UiGroup( "TF:S2,10/,10/1" );>;
    FloatAttribute(flAlphaFade, flAlphaFade);

    float2 g_fAnimatedGrid < Default2( 2.0, 2.0 ); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    Float2Attribute(g_fAnimatedGrid, g_fAnimatedGrid);

    float g_fNumAnimationCells < Range( 0.0, 100.0 ); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    FloatAttribute(g_fNumAnimationCells, g_fNumAnimationCells);

    float g_fAnimationTimePerFrame < Range( 0.0, 100.0 ); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    FloatAttribute(g_fAnimationTimePerFrame, g_fAnimationTimePerFrame);

    float2 g_fNormalScroll < Default2(0.0, 0.0); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    FloatAttribute(g_fNormalScroll, g_fNormalScroll);

    float4 g_vViewport < Source( Viewport ); >;

	CreateTexture2D( g_tDepthBufferCopyTexture ) <AsSceneDepth; SrgbRead( false );    AddressU( CLAMP ); AddressV( CLAMP ); Filter( POINT ); >;
    BoolAttribute( bWantsFBCopyTexture, true );
    CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( "FrameBufferCopyTexture" ); SrgbRead( false );  AddressU( CLAMP ); AddressV( CLAMP ); Filter( MIN_MAG_MIP_LINEAR ); >;

    //
    // Helper methods for transforming depth values into other forms!
    // https://discord.com/channels/833983068468936704/893738580885782538/1066971556963680316


    //
    // Remap the depth buffers value to be relative to the current
    // viewport range
    //
    float RemapDepthToViewport( float flDepth )
    {
        return RemapValClamped( flDepth, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0 );
    }

    //
    // Takes our depth value and remap it to engine units
    //
    float DepthToApproximateWorldUnits( float flDepth )
    {
        // Transform our depth to relative to the viewport
        float flViewportDepth = RemapDepthToViewport( flDepth );

        // Fetch our scale & transformation
        float flZScale = g_vInvProjRow3.z;
        float flZTran = g_vInvProjRow3.w;
        
        // Get our actual depth
        return flViewportDepth * flZScale + flZTran;
    }

    //
    // Get the current depth value dependant on the current
    // camera ray
    //
    float DepthToRealDepthWorldUnits( float flDepth, float3 vCameraRayWs )
    {
        // Make sure we're normalized
        vCameraRayWs = normalize( vCameraRayWs );

        // Get the length of the ray
        float flRayLength = DepthToApproximateWorldUnits( flDepth );

        // How far along that ray is with our camera ray
        return flRayLength * dot( g_vCameraDirWs.xyz, vCameraRayWs.xyz );
    }

    // 
    // Rebuild the world position based on the current depth value. Essentially Depth -> World Position
    //
    float3 ReconstructPositionFromDepth( float flDepth, float3 vCameraRayWs )
    {
        // No need to normalize the camera ray, DepthToRealDepthWorldUnits does this!
        float flRayLength = DepthToRealDepthWorldUnits( flDepth, vCameraRayWs );

        // Rebuild position!
        return g_vCameraPositionWs.xyz + ( vCameraRayWs.xyz / flRayLength );
    }

    float FetchDepth( float2 ssUV, float3 vPositionWs )
    {
        //float2 mvuv = ScreenspaceCorrectionMultiview( CalculateViewportUvFromInvSize( ssUV - g_vViewportOffset.xy, g_vInvViewportSize.xy ) );
        float flProjectedDepth = Tex2D(g_tDepthBufferCopyTexture, ssUV).r;
        float3 d = ReconstructPositionFromDepth(flProjectedDepth, CalculatePositionToCameraDirWs( vPositionWs ));
        return distance(float3(0, 0, d.z), float3(0, 0, vPositionWs.z));
    }

	float4 MainPs( PS_INPUT i ) : SV_TARGET0
	{   
		Material m = GatherMaterial( i );

        //Animated Normal
        //float2 scaledWorldPos = ((i.vPositionWithOffsetWs.xy + g_vCameraPositionWs.xy) * g_fScale);
        float2 texUv_scroll = i.vTextureCoords.xy + (g_fNormalScroll * g_flTime);
        float2 uv = (texUv_scroll - floor(texUv_scroll));

		float2 scale = float2(1 / g_fAnimatedGrid.x, 1 / g_fAnimatedGrid.y);
		uv *= scale;
		int currentFrame = (g_flTime / g_fAnimationTimePerFrame) % (g_fNumAnimationCells - 1);
		int yRow = currentFrame / g_fAnimatedGrid.y;
		float2 uvStart = float2((currentFrame % g_fAnimatedGrid.x) / g_fAnimatedGrid.x, yRow / g_fAnimatedGrid.y);

        //Sample normal with new animated uvs
        float3 sampledNormal = Tex2DS(g_tNormal, TextureFiltering, clamp(uv + uvStart, float2(0.001, 0.001), float2(0.999, 0.999))).rgb;

        m.Normal = normalize(sampledNormal * 2.0f - 1.0f);

        //Refraction
        float3 vPositionWs = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs;
        float4 vPositionPs = Position3WsToPs( vPositionWs + ((float3(m.Normal.x, m.Normal.y, 0) * 2.0f - 1.0f) * flRefraction) );
        vPositionPs.xy /= vPositionPs.w;
        float2 vPositionSs_refracted = PsToSs( vPositionPs );
        vPositionSs_refracted.x = 1.0f - vPositionSs_refracted.x;
        //vPositionSs_refracted = ScreenspaceCorrectionMultiview( CalculateViewportUvFromInvSize( vPositionSs_refracted - g_vViewportOffset.xy, g_vInvViewportSize.xy ) );

        //Getting depth for normal refracted pixel position
        //float water_depth = length(CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz ));
        float refracted_scene_depth = FetchDepth(vPositionSs_refracted, vPositionWs);
        //float real_scene_depth = FetchDepth(i.vPositionSs.xy, vPositionWs);

        //Use the unaltered depth + screen space pos if the refracted one is in front of the water.
        //bool depthBehindWater = refracted_scene_depth > water_depth;
        float2 position_ss = vPositionSs_refracted;//depthBehindWater ? vPositionSs_refracted : i.vPositionSs.xy;
        float scene_depth = refracted_scene_depth;//depthBehindWater ? refracted_scene_depth : real_scene_depth;

        float fogVal = RemapValClamped( scene_depth, g_fDepthScale.x, g_fDepthScale.y, 0.0, 1.0 );
        float3 frameBuffer = Tex2D(g_tFrameBufferCopyTexture, position_ss);

        //Set albedo based on the final fog value (depth and alpha)
        fogVal = min(g_FogColorTint.a, fogVal);
        m.Albedo = lerp(frameBuffer, g_FogColorTint.rgb, fogVal);
        //Make the water emissive based on fog amount
        m.Emission = m.Albedo * (1 - fogVal);
        //Smoothly step into water using alpha. Helps mitigate the harsh transition
        //float fade = smoothstep(0, flAlphaFade, abs(water_depth - scene_depth));

        float4 finalisedColor = FinalizePixelMaterial( i, m );
        return finalisedColor;
	}
}