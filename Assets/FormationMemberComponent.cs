using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public class FormationMemberComponent : IComponentData
{
    public float3 offset;
}
