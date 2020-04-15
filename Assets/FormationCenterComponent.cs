using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct FormationCenterComponent : IComponentData
{
    public float speed;

    public float3 position;
    public quaternion rotation;
}