HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Hologram Effect";
}


FEATURES
{
    #include "common/features.hlsl"

    Feature(F_SCREEN_SPACE_OVERLAY, 0..1, "");
}

COMMON
{
	#include "common/shared.hlsl"

    #define S_TRANSLUCENT 1

    float g_fScale< Default( 1 ); Range( 0, 1000 ); UiGroup( "TF:S2,10/" );>;
    FloatAttribute(g_fScale, g_fScale);

    float g_fTimeScale< Default( 1 ); Range( 0, 1000 ); UiGroup( "TF:S2,10/" );>;
    FloatAttribute(g_fTimeScale, g_fTimeScale);

    float g_OpacityTimeScale< Default( 1 ); Range( 0, 1000 ); UiGroup( "TF:S2,10/" );>;
    FloatAttribute(g_OpacityTimeScale, g_OpacityTimeScale);
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
        float scaledTime = g_flTime * g_fTimeScale;
        float pos = i.vPositionOs + scaledTime;
        pos = sin(pos);
        i.vPositionOs += (pos * g_fScale);

        PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}


PS
{
    #include "common/pixel.hlsl"

    StaticCombo( S_SCREEN_SPACE_OVERLAY, F_SCREEN_SPACE_OVERLAY, Sys( PC ) );

    float2 g_OverlayScroll < Default2( 0, 0 ); Range2( -10, -10, 10, 10 ); UiGroup( "TF:S2,10/" );>;
    Float2Attribute(g_OverlayScroll, g_OverlayScroll);

    float g_OverlayScale < Default( 1); Range( 0, 10 ); UiGroup( "TF:S2,10/" );>;
    Float2Attribute(g_OverlayScale, g_OverlayScale);

    CreateInputTexture2D( Overlay, Linear, 8, "", "", "TF:S2,10/", Default3( 0.5, 0.5, 1.0 ) );
    CreateTexture2D( g_tOverlay ) < AddressU( MIRROR ); AddressV( MIRROR ); Channel( RGBA, Box( Overlay ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

    float2 g_OpacityWobble < Default2( 1, 1); Range2( 0, 0, 1, 1 ); UiGroup( "TF:S2,10/" );>;
    Float2Attribute(g_OpacityWobble, g_OpacityWobble);

    float3 g_OverlayTint < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "TF:S2,10/Invun,10/1" );>;
    Float3Attribute(g_OverlayTint, g_OverlayTint);

	float4 MainPs( PS_INPUT i ) : SV_TARGET0
	{
		Material m = GatherMaterial( i );
		m.ClearcoatRoughness = 0;
        m.Emission = m.Albedo;
		
		float4 o = FinalizePixelMaterial( i, m );

        #if S_SCREEN_SPACE_OVERLAY
        float2 overlayUV = CalculateViewportUv( i.vPositionSs ) * g_OverlayScale;
        #else
        float2 overlayUV = i.vTextureCoords.xy * g_OverlayScale;
        #endif

        o.rgb += Tex2D(g_tOverlay, overlayUV + (g_OverlayScroll * g_flTime)).rgb * g_OverlayTint;
        o.a = lerp(g_OpacityWobble.x, g_OpacityWobble.y, abs(sin(g_flTime * g_OpacityTimeScale)));

        return o;
	}
}