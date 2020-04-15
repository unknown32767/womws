using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct FormationMemberComponent : IComponentData
{
    public float3 offset;
}