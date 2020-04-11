using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ShipComponent : IComponentData
{
}

public struct FactionComponent : IComponentData
{
    public enum Faction
    {
        A,
        B
    };

    public Faction faction;
}

public struct BatteryComponent : IComponentData
{
    public float interval;
    public float speed;

    public float countDown;
}