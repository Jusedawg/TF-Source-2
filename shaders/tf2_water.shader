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
    #define VS_INPUT_HAS_TANGENT_BASIS 1
	#define PS_INPUT_HAS_TANGENT_BASIS 1
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

    DynamicCombo( D_MULTIVIEW_INSTANCING, 0..1, Sys(PC) );
	RenderState( DepthEnable, true );
	RenderState( DepthWriteEnable, false );

    float flNormalStrength< Default(1.0); Range(1.0, 10.0); UiGroup( "TF:S2,10/,10/1" );>;

    float flRefraction < Default(1.005); Range(0.0, 2.0); UiGroup( "TF:S2,10/,10/1" );>;
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

    float3 FetchDepth( float2 ssUV, float3 vPositionWs )
    {
        //float2 mvuv = ScreenspaceCorrectionMultiview( CalculateViewportUvFromInvSize( ssUV - g_vViewportOffset.xy, g_vInvViewportSize.xy ) );
        float flProjectedDepth = Tex2D(g_tDepthBufferCopyTexture, ssUV * g_vFrameBufferCopyInvSizeAndUvScale.zw).r;
        return ReconstructPositionFromDepth(flProjectedDepth, CalculatePositionToCameraDirWs( vPositionWs ));
    }

	float4 MainPs( PS_INPUT i ) : SV_TARGET0
	{   
        float3 vViewRayWs = normalize(i.vPositionWithOffsetWs.xyz);
		Material m = Material::From( i );

        //
        // Multiview instancing
        //
        uint nView = uint(0);
        #if (D_MULTIVIEW_INSTANCING)
                nView = i.nView;
        #endif

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
        float3 sampledNormal = Tex2DS(g_tNormal, TextureFiltering, uv + uvStart).rgb;
        m.Normal = sampledNormal * 2.0f - 1.0f;
        m.Normal = Vec3TsToWsNormalized(m.Normal, i.vNormalWs.xyz, i.vTangentUWs, i.vTangentVWs);

        //Refraction
        float3 vPositionWs = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs;

        float3 vRefractRayWs = refract(vViewRayWs, m.Normal, flRefraction);
        vRefractRayWs = vRefractRayWs * 4;
        float3 vRefractWorldPosWs = i.vPositionWithOffsetWs.xyz + vRefractRayWs;
        
        float4 vPositionPs = Position4WsToPsMultiview(nView, float4(vRefractWorldPosWs, 0));
        float2 vPositionSs = vPositionPs.xy / vPositionPs.w;
        vPositionSs = vPositionSs * 0.5 + 0.5;
        vPositionSs.y = 1.0 - vPositionSs.y;

        //
        // Multiview
        //
        #if (D_MULTIVIEW_INSTANCING)
        {
            vPositionSs.x *= 0.5;
            vPositionSs.x += nView * 0.5;
        }
        #endif

        //Getting depth for normal refracted pixel position
        //float water_depth = length(CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz ));
        float3 depthPos = FetchDepth(vPositionSs, vPositionWs);
        float depthFromWaterTop = abs(vPositionWs.z - depthPos.z);
        float fogVal = RemapValClamped( depthFromWaterTop, g_fDepthScale.x, g_fDepthScale.y, 0.0, 1.0 );
        float2 vUV = float2(vPositionSs) * g_vFrameBufferCopyInvSizeAndUvScale.zw;
        float3 frameBuffer = Tex2D(g_tFrameBufferCopyTexture, vUV);

        //Set albedo based on the final fog value (depth and alpha)
        fogVal = min(g_FogColorTint.a, fogVal);
        m.Albedo = lerp(frameBuffer, g_FogColorTint.rgb, fogVal) * m.Albedo;
        //Make the water emissive based on fog amount
        m.Emission = m.Albedo * (1 - fogVal);

        //Smoothly step into water using alpha. Helps mitigate the harsh transition
        m.Opacity = smoothstep(0, flAlphaFade, abs(length(i.vPositionWithOffsetWs.xyz) - distance(g_vCameraPositionWs, depthPos)));
        float4 finalisedColor = ShadingModelStandard::Shade( i, m );
        return finalisedColor;
	}
}