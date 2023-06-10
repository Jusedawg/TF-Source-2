#ifndef SOURCEBOX_LEGACY_PIXEL_H
#define SOURCEBOX_LEGACY_PIXEL_H


#include "sbox_pixel.fxc"

// replaces pixel.material.hlsl
#include "sourcebox/common/legacy_material.hlsl"
#include "sourcebox/common/legacy_pixelinputs.hlsl"

#include "common/pixel.hlsl"

#include "sourcebox/common/legacy_lighting.hlsl"
#include "sourcebox/common/texcombine.hlsl"

// blending
#define BLEND_MODE_ALREADY_SET
RenderState( BlendEnable, F_ENABLE_BLEND ? true : false );
RenderState( SrcBlend, F_BLEND_MODE == 0 ? SRC_ALPHA : (F_BLEND_MODE == 1 ? ONE : SRC_ALPHA) );
RenderState( DstBlend, F_BLEND_MODE == 0 ? INV_SRC_ALPHA : (F_BLEND_MODE == 1 ? ONE : ONE) );
BoolAttribute( translucent, F_ENABLE_BLEND ? true : false );

// Don't conflict with S_ALPHA_TEST, we're doing this ourselves (to support all comparison ops)
StaticCombo( S_ALPHA_TEST_MANUAL				    , F_ALPHA_TEST				     , Sys( ALL ) );
// StaticCombo( S_ALPHA_TEST_FUNC				        , F_ALPHA_TEST_FUNC				 , Sys( ALL ) );
#define S_ALPHA_TEST_FUNC 6
float g_flAlphaTestReference < UiGroup( "Material,10/15" ); Default(0.3f); Range(0.0f, 1.0f); >;

#if (S_ALPHA_TEST_MANUAL)
    #if (S_ALPHA_TEST_FUNC == 0)
        // Never
        #define RUN_ALPHA_TEST(color) color.a = 1.0
    #elif (S_ALPHA_TEST_FUNC == 1)
        // Less
        #define RUN_ALPHA_TEST(color) if (!(color.a < g_flAlphaTestReference)) discard; color.a = 1.0
    #elif (S_ALPHA_TEST_FUNC == 2)
        // Equal
        #define RUN_ALPHA_TEST(color) if (!(color.a == g_flAlphaTestReference)) discard; color.a = 1.0
    #elif (S_ALPHA_TEST_FUNC == 3)
        // Less equal
        #define RUN_ALPHA_TEST(color) if (!(color.a <= g_flAlphaTestReference)) discard; color.a = 1.0
    #elif (S_ALPHA_TEST_FUNC == 4)
        // Greater
        #define RUN_ALPHA_TEST(color) if (!(color.a > g_flAlphaTestReference)) discard; color.a = 1.0
    #elif (S_ALPHA_TEST_FUNC == 5)
        // Not equal
        #define RUN_ALPHA_TEST(color) if (!(color.a != g_flAlphaTestReference)) discard; color.a = 1.0
    #elif (S_ALPHA_TEST_FUNC == 6)
        // Greater equal
        #define RUN_ALPHA_TEST(color) if (!(color.a >= g_flAlphaTestReference)) discard; color.a = 1.0
    #else
        // Always
        #define RUN_ALPHA_TEST(color) discard; color.a = 1.0
    #endif
#else // !S_ALPHA_TEST_MANUAL
    #define RUN_ALPHA_TEST(color)
#endif // !S_ALPHA_TEST_MANUAL

float4 FinalizeLegacyOutput(float4 result)
{
    RUN_ALPHA_TEST(result);
    // #if (S_ALPHA_TEST_FUNC == 0)
    //     // Never
    // #elif (S_ALPHA_TEST_FUNC == 1)
    //     // Less
    //     if (result.a < g_flAlphaTestReference) discard;
    // #elif (S_ALPHA_TEST_FUNC == 2)
    //     // Equal
    //     if (result.a == g_flAlphaTestReference) discard;
    // #elif (S_ALPHA_TEST_FUNC == 3)
    //     // Less equal
    //     if (result.a <= g_flAlphaTestReference) discard;
    // #elif (S_ALPHA_TEST_FUNC == 4)
    //     // Greater
    //     if (result.a > g_flAlphaTestReference) discard;
    // #elif (S_ALPHA_TEST_FUNC == 5)
    //     // Not equal
    //     if (result.a != g_flAlphaTestReference) discard;
    // #elif (S_ALPHA_TEST_FUNC == 6)
    //     // Greater equal
    //     if (result.a >= g_flAlphaTestReference) discard;
    // #else
    //     // Always
    //     discard;
    // #endif

    return result;
}

#endif // SOURCEBOX_LEGACY_PIXEL_H