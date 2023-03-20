
#include "common/features.hlsl"

Feature( F_ALPHA_TEST,          0..1, "Rendering" );
// Feature( F_ALPHA_TEST_FUNC,     0..7(0="Never",1="Less",2="Equal",3="LessEqual",4="Greater",5="NotEqual",6="GreaterEqual",7="Always"), "Rendering" );

Feature( F_ENABLE_BLEND,          0..1, "Blending" );
Feature( F_BLEND_MODE,          0..2(0="Blend", 1="Add", 2="Blend Add"), "Blending" );
FeatureRule( ChildOf(F_BLEND_MODE, F_ENABLE_BLEND), "" );

