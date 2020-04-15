using Unity.Entities;

public struct ShipComponent : IComponentData
{
    public float speed;
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