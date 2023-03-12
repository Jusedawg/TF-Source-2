HEADER
{
	Description = "TF2 Water post effect";
}

FEATURES
{
}

MODES
{
    VrForward();
    Default();
}

COMMON
{
	#include "postprocess/shared.hlsl"
}

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	// VS only
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	// PS only
	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_ScreenPosition;
	#endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

PS
{
    #include "postprocess/common.hlsl"

    CreateInputTexture2D( warpNormalTexture , Linear, 8, "NormalizeNormals", "",  "", Default3( 1, 1, 1) );
    CreateTexture2D( warpNormal ) < Channel( RGB , Box( warpNormalTexture ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
    TextureAttribute(warpNormal, warpNormal);
    float3 g_colorTint < Default3( 0.0f, 0.0f, 1.0f ); UiType( Color ); UiGroup( "" ); >;
    float g_warpScale< Default( 0.15f ); Range(0.0f, 2.0f); UiGroup( "" ); >;

    //Animated normal
    float2 g_fAnimatedGrid < Default2( 2.0, 2.0 ); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    Float2Attribute(g_fAnimatedGrid, g_fAnimatedGrid);
    float g_fNumAnimationCells < Range( 0.0, 100.0 ); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    FloatAttribute(g_fNumAnimationCells, g_fNumAnimationCells);
    float g_fAnimationTimePerFrame < Range( 0.0, 100.0 ); UiGroup( "TF:S2,10/Animated Normal,10/1" );>;
    FloatAttribute(g_fAnimationTimePerFrame, g_fAnimationTimePerFrame);

    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

    CreateTexture2D( g_tColorBuffer ) < Attribute( "ColorBuffer" );  	SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
    CreateTexture2D( g_tDepthBuffer ) < Attribute( "DepthBuffer" ); 	SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;

    struct PixelOutput
    {
        float4 vColor : SV_Target0;
    };

    float4 MainPs(PixelInput i) : SV_Target0
    {
        float2 uv = i.vTexCoord.xy - g_vViewportOffset.xy / g_vRenderTargetSize;

		float2 scale = float2(1 / g_fAnimatedGrid.x, 1 / g_fAnimatedGrid.y);
		float2 scaledUv = uv * scale;
		int currentFrame = (g_flTime / g_fAnimationTimePerFrame) % (g_fNumAnimationCells - 1);
		int yRow = currentFrame / g_fAnimatedGrid.y;
		float2 uvStart = float2((currentFrame % g_fAnimatedGrid.x) / g_fAnimatedGrid.x, yRow / g_fAnimatedGrid.y);

        float2 warpSample = Tex2D(warpNormal, scaledUv + uvStart).rg * 2.0f - 1.0f;

        //Move the UV based on the normal sample
        float4 o = Tex2D(g_tColorBuffer, uv.xy + (warpSample * g_warpScale));
        //Tint output using overlay texture.
        o.rgb *= g_colorTint;
        return o;
    }
}