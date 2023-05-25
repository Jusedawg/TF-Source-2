// Copyright Valve Corporation, All rights reserved.

#ifndef SOURCEBOX_TEXCOMBINE_H
#define SOURCEBOX_TEXCOMBINE_H

// texture combining modes for combining base and detail/basetexture2
#define TCOMBINE_RGB_EQUALS_BASE_x_DETAILx2 0				// original mode
#define TCOMBINE_RGB_ADDITIVE 1								// base.rgb+detail.rgb*fblend
#define TCOMBINE_DETAIL_OVER_BASE 2
#define TCOMBINE_FADE 3										// straight fade between base and detail.
#define TCOMBINE_BASE_OVER_DETAIL 4                         // use base alpha for blend over detail
#define TCOMBINE_RGB_ADDITIVE_SELFILLUM 5                   // add detail color post lighting
#define TCOMBINE_RGB_ADDITIVE_SELFILLUM_THRESHOLD_FADE 6
#define TCOMBINE_MOD2X_SELECT_TWO_PATTERNS 7				// use alpha channel of base to select between mod2x channels in r+a of detail
#define TCOMBINE_MULTIPLY 8
#define TCOMBINE_MASK_BASE_BY_DETAIL_ALPHA 9                // use alpha channel of detail to mask base
#define TCOMBINE_SSBUMP_BUMP 10								// use detail to modulate lighting as an ssbump
#define TCOMBINE_SSBUMP_NOBUMP 11							// detail is an ssbump but use it as an albedo. shader does the magic here - no user needs to specify mode 11

float4 TextureCombine( float4 baseColor, float4 detailColor, int combine_mode,
					   float fBlendFactor )
{
	[branch]
	if ( combine_mode == TCOMBINE_MOD2X_SELECT_TWO_PATTERNS)
	{
		float3 dc=lerp(detailColor.r,detailColor.a, baseColor.a);
		baseColor.rgb*=lerp(float3(1,1,1),2.0*dc,fBlendFactor);
	}
	else if ( combine_mode == TCOMBINE_RGB_EQUALS_BASE_x_DETAILx2)
		baseColor.rgb*=lerp(float3(1,1,1),2.0*detailColor.rgb,fBlendFactor);
	else if ( combine_mode == TCOMBINE_RGB_ADDITIVE )
 		baseColor.rgb += fBlendFactor * detailColor.rgb;
	else if ( combine_mode == TCOMBINE_DETAIL_OVER_BASE )
	{
		float fblend=fBlendFactor * detailColor.a;
		baseColor.rgb = lerp( baseColor.rgb, detailColor.rgb, fblend);
	}
	else if ( combine_mode == TCOMBINE_FADE )
	{
		baseColor = lerp( baseColor, detailColor, fBlendFactor);
	}
	else if ( combine_mode == TCOMBINE_BASE_OVER_DETAIL )
	{
		float fblend=fBlendFactor * (1-baseColor.a);
		baseColor.rgb = lerp( baseColor.rgb, detailColor.rgb, fblend );
		baseColor.a = detailColor.a;
	}
	else if ( combine_mode == TCOMBINE_MULTIPLY )
	{
		baseColor = lerp( baseColor, baseColor*detailColor, fBlendFactor);
	}
	else if (combine_mode == TCOMBINE_MASK_BASE_BY_DETAIL_ALPHA )
	{
		baseColor.a = lerp( baseColor.a, baseColor.a*detailColor.a, fBlendFactor );
	}
	else if ( combine_mode == TCOMBINE_SSBUMP_NOBUMP )
	{
		baseColor.rgb = baseColor.rgb * dot( detailColor.rgb, 2.0/3.0 );
	}
	return baseColor;
}

float3 TextureCombinePostLighting( float3 lit_baseColor, float4 detailColor, int combine_mode,
								   float fBlendFactor )
{
	[branch]
	if ( combine_mode == TCOMBINE_RGB_ADDITIVE_SELFILLUM )
 		lit_baseColor += fBlendFactor * detailColor.rgb;
	else if ( combine_mode == TCOMBINE_RGB_ADDITIVE_SELFILLUM_THRESHOLD_FADE )
	{
 		// fade in an unusual way - instead of fading out color, remap an increasing band of it from
 		// 0..1
		//if (fBlendFactor > 0.5)
		//	lit_baseColor += min(1, (1.0/fBlendFactor)*max(0, detailColor.rgb-(1-fBlendFactor) ) );
		//else
		//	lit_baseColor += 2*fBlendFactor*2*max(0, detailColor.rgb-.5);

		float f = fBlendFactor - 0.5;
		float fMult = (f >= 0) ? 1.0/fBlendFactor : 4*fBlendFactor;
		float fAdd = (f >= 0) ? 1.0-fMult : -0.5*fMult;
		lit_baseColor += saturate(fMult * detailColor.rgb + fAdd);
	}
	return lit_baseColor;
}

// used by LightmappedGeneric
float4 ConvertNormal( float4 normal )
{
	// float4 normalTexel = Tex2D( NormalSampler, tc );
	// return float4(normalTexel.xyz * 2.0f - 1.0f, normalTexel.a );
	return float4(normal.xyz * 2.0f - 1.0f, normal.a);
}

#endif // SOURCEBOX_TEXCOMBINE_H