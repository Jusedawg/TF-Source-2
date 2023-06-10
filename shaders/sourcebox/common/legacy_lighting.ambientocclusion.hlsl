#ifndef SOURCEBOX_LEGACY_LIGHTING_AMBIENTOCCLUSION_H
#define SOURCEBOX_LEGACY_LIGHTING_AMBIENTOCCLUSION_H

// This is copied straight from pixel.shading.ambientocclusion.hlsl
// would be great if we didn't have to do that, but specularAO takes in ShadeParams
// and was included somewhere earlier, so we can't replace it with a macro

/*
float specularAO( LegacyShadeParams i, float ambientOcclusion ) 
{
    float specularAO = 1.0;

    float NoV = dot( i.inputs.NormalWs, normalize( i.inputs.PositionWithOffsetWs ) );
    float3 NrV = reflect( normalize( i.inputs.PositionWithOffsetWs ), i.inputs.NormalWs );

    // https://computergraphics.stackexchange.com/questions/1515/what-is-the-accepted-method-of-converting-shininess-to-roughness-and-vice-versa
    // https://media.contentapi.ea.com/content/dam/eacom/frostbite/files/course-notes-moving-frostbite-to-pbr-v2.pdf
    // const float roughness = i.Roughness;
    const float roughness = pow(saturate(1.0f - i.SpecularExponent / 150.0f), 2.0f);

    if( i.config.AmbientOcclusionModel > AMBIENT_OCCLUSION_BENT_NORMALS )
    {
        #if defined(MATERIAL_HAS_BENT_NORMAL)
            //specularAO = SpecularAO_Cones(shading_bentNormal, ambientOcclusion, roughness, NrV);
        #else
            specularAO = SpecularAO_Cones(i.inputs.NormalWs, ambientOcclusion, roughness, NrV);
        #endif

        float3 bn = 0;
        //bn = textureLod(light_ssao, float3(cache.uv, 1.0), 0.0).xyz;

        bn = unpackBentNormal(bn);
        bn = normalize(bn);

        float ssSpecularAO = SpecularAO_Cones(bn, ambientOcclusion, roughness, NrV);
        // Combine the specular AO from the texture with screen space specular AO
        specularAO = min(specularAO, ssSpecularAO);

        // For now we don't use the screen space AO bent normal for the diffuse because the
        // AO bent normal is currently a face normal.
    }
    else
    {
        // TODO: Should we even bother computing this when screen space bent normals are enabled?
        specularAO = SpecularAO_Lagarde(NoV, ambientOcclusion, roughness);
    }

    return SpecularAO_Cones(i.inputs.NormalWs, ambientOcclusion, roughness, NrV);

    return specularAO;
}
*/

float specularAO( LegacyShadeParams i, float ambientOcclusion )
{
    return ambientOcclusion;
}

#endif // SOURCEBOX_LEGACY_LIGHTING_AMBIENTOCCLUSION_H