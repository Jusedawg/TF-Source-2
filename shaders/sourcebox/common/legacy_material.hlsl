#ifndef SOURCEBOX_LEGACY_MATERIAL_H
#define SOURCEBOX_LEGACY_MATERIAL_H

#ifdef COMMON_PIXEL_MATERIAL_MINIMAL_H
#error legacy_material.hlsl must come before base s&box includes!
#endif // COMMON_PIXEL_MATERIAL_MINIMAL_H

// Sam's shading model API uses Material as input
// that doesn't really work for our use case, so here we add some helpers to bridge the gap
#define COMMON_PIXEL_MATERIAL_MINIMAL_H
struct Material {
    float3  Albedo;
    float   Opacity;
    float3  Normal;
    float3  AmbientOcclusion;

    float   Fresnel;

    float   RimMask;
    float   RimFresnel;
    float   RimExponent;

    float   SpecularMask;
    float3  SpecularTint;
    float   SpecularExponent;

    float3  SelfIllumMask;
    
    float3  StaticLightingColor;
    float3  EnvmapMask;

    // used for EyeRefract
    float3 CorneaNormal;
    float IrisHighlightMask;
    float AverageAmbient;

    // custom controls
    float4 DiffuseModControls;
    
    // Emulated PBR parameters, for S&box shading model
    // you shouldn't ever need to bother with these, they're just here to keep the compiler happy
    float  Roughness;            // default: 1.0
    float  Metalness;            // default: 0.0
    float3 Emission;             // default: float3(0.0, 0.0, 0.0)
    float  TintMask;             // default: 1.0

    float3 Sheen;                // default: float3(0.0)
    float  SheenRoughness;       // default: 0.0
    float  Clearcoat;            // default: 0.0
    float  ClearcoatRoughness;   // default: 0.03
    float3 ClearcoatNormal;      // default: float3(0.0, 0.0, 1.0)
    float  Anisotropy;            // default: 0.0
    float3 AnisotropyRotation;    // default: float3(1.0, 0.0, 0.0)
    float  Thickness;            // default: 0.5
    float  SubsurfacePower;      // default: 12.234
    float3 SheenColor;           // default: sqrt(baseColor)
    float3 Transmission;        // default: 1.0
    float3 Absorption;           // default float3(0.0, 0.0, 0.0)
    float  IndexOfRefraction;    // default: 1.5
    float  MicroThickness;       // default: 0.0
};

Material GetDefaultLegacyMaterial()
{
    Material m;

    m.Albedo = float3(1.0, 1.0, 1.0);
    m.Opacity = 1.0;
    m.Normal = float3(0.0, 0.0, 1.0);
    m.AmbientOcclusion = float3(1.0, 1.0, 1.0);
    m.Fresnel = 0.0;
    m.RimMask = 0.0;
    m.RimFresnel = 0.0;
    m.RimExponent = 0.0;
    m.SpecularMask = 1.0;
    m.SpecularTint = float3(1.0, 1.0, 1.0);

    // assume 0.5 roughness default for AO calc
    m.SpecularExponent = 43.93398;

    m.StaticLightingColor = float3(0.0, 0.0, 0.0);
    m.EnvmapMask = float3(1.0, 1.0, 1.0);
    m.SelfIllumMask = float3(1.0, 1.0, 1.0);

    // used for EyeRefract
    m.CorneaNormal = float3(0.0, 0.0, 1.0);
    m.IrisHighlightMask = 0.0;
    m.AverageAmbient = 1.0;
	
    // custom controls
    m.DiffuseModControls = float4(0, 0, 0, 0);
    
    // Emulated PBR parameters, for S&box shading model
    m.Roughness = 1.0;
    m.Metalness = 0.0;
    m.Emission = float3(0.0, 0.0, 0.0);
    m.TintMask = 1.0;

    m.Sheen = float3(0.0, 0.0, 0.0);
    m.SheenRoughness = 0.0;
    m.Clearcoat = 0.0;
    m.ClearcoatRoughness = 0.03;
    m.ClearcoatNormal = float3(0.0, 0.0, 1.0);
    m.Anisotropy = 0.0;
    m.AnisotropyRotation = float3(1.0, 0.0, 0.0);
    m.Thickness = 0.5;
    m.SubsurfacePower = 12.234;
    m.SheenColor = float3(0.0, 0.0, 0.0);
    m.Transmission = 1.0;
    m.Absorption = float3(0.0, 0.0, 0.0);
    m.IndexOfRefraction = 1.5;
    m.MicroThickness = 0.0;

    return m;
}

// struct LegacyMaterial {
//     float3  Diffuse;
//     float   Opacity;
//     float3  Normal;
//     float3  AmbientOcclusion;

//     float   Fresnel;

//     float   RimMask;
//     float   RimFresnel;
//     float   RimExponent;

//     float   SpecularMask;
//     float3  SpecularTint;
//     float   SpecularExponent;

//     float3  SelfIllumMask;
    
//     float3  StaticLightingColor;
//     float3  EnvMapColor;
// };

// LegacyMaterial GetDefaultLegacyMaterial()
// {
//     LegacyMaterial m;

//     m.Diffuse = float3(1.0, 1.0, 1.0);
//     m.Opacity = 1.0;
//     m.Normal = float3(0.0, 0.0, 1.0);
//     m.AmbientOcclusion = float3(1.0, 1.0, 1.0);
//     m.Fresnel = 0.0;
//     m.RimMask = 0.0;
//     m.RimFresnel = 0.0;
//     m.RimExponent = 0.0;
//     m.SpecularMask = 1.0;
//     m.SpecularTint = float3(1.0, 1.0, 1.0);
//     m.SpecularExponent = 1.0;
//     m.StaticLightingColor = float3(0.0, 0.0, 0.0);
//     m.EnvMapColor = float3(0.0, 0.0, 0.0);
//     m.SelfIllumMask = float3(1.0, 1.0, 1.0);
//     return m;
// }

// LegacyMaterial MaterialToLegacy( Material m )
// {
//     LegacyMaterial o;
//     o.Diffuse = m.Albedo;
//     o.Opacity = m.Opacity;
//     o.Normal = m.Normal;
//     o.AmbientOcclusion = m.AmbientOcclusion;
    
//     o.Fresnel = m.SheenRoughness;

//     o.RimMask = m.ClearcoatRoughness;
//     o.RimFresnel = m.Thickness;
//     o.RimExponent = m.SubsurfacePower;

//     o.SpecularMask = m.Metalness;
//     o.SpecularTint = m.SubsurfaceColor;
//     o.SpecularExponent = m.Roughness;

//     o.SelfIllumMask = m.SheenColor;

//     o.StaticLightingColor = m.Sheen;
//     o.EnvMapColor = m.Emission;
//     return o;
// }

// Material LegacyToMaterial( LegacyMaterial m )
// {
//     Material o;
//     o.Albedo = m.Diffuse;
//     o.Opacity = m.Opacity;
//     o.Normal = m.Normal;
//     o.AmbientOcclusion = m.AmbientOcclusion;
    
//     o.SheenRoughness = m.Fresnel;
//     o.ClearcoatRoughness = m.RimMask;

//     o.Thickness = m.RimFresnel;
//     o.SubsurfacePower = m.RimExponent;

//     o.Metalness = m.SpecularMask;
//     o.SubsurfaceColor = m.SpecularTint;
//     o.Roughness = m.SpecularExponent;

//     o.SheenColor = m.SelfIllumMask;

//     o.Sheen = m.StaticLightingColor;
//     o.Emission = m.EnvMapColor;
    
//     // defaults
//     o.Transmission = float3(0.0, 0.0, 0.0);
//     o.Clearcoat = 0.0;
//     o.ClearcoatNormal = float3(0.0, 0.0, 1.0);
//     o.Anisotropy = 0.0;
//     o.AnisotropyRotation = float3(1.0, 0.0, 0.0);
//     o.TintMask = 1.0;

//     return o;
// }

#endif // SOURCEBOX_LEGACY_MATERIAL_H