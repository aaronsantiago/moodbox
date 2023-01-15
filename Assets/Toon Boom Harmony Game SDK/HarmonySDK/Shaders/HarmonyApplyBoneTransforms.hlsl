#ifndef HARMONY_SKINNING_H
#define HARMONY_SKINNING_H

float4x4 _Bones[32];

void HarmonyApplyBoneTransforms_float(float4 boneParams, float4 position, out float3 outputPosition)
{
#ifdef SHADERGRAPH_PREVIEW
	// Pass through without bone skinning in shader graph preview
	outputPosition = position.xyz;
#else
	const float4x4 skinMatrix = boneParams.y * _Bones[int(boneParams.x)] + boneParams.w * _Bones[int(boneParams.z)];
	outputPosition = mul(skinMatrix, position).xyz;
#endif
}

#endif // HARMONY_SKINNING_H