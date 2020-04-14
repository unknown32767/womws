using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct UserCommandComponent : IComponentData
{
    public float3 destination;
}
