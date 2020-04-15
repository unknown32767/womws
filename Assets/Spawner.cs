using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public GameObject template;

    public int count;
    public float radius;
    public float f;

    private List<Vector3> offsets = new List<Vector3>
    {
        new Vector3(0, 0, 2),
        new Vector3(-2, 0, 0),
        new Vector3(2, 0, 0),
        new Vector3(4, 0, -2),
        new Vector3(-4, 0, -2),
    };

    void Start()
    {
        var basePosition = transform.position;

        SpawnFaction(-basePosition, FactionComponent.Faction.B);

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityManager.CreateEntity(typeof(UserCommandComponent));
        var formationCenter = entityManager.CreateEntity(typeof(FormationCenterComponent), typeof(Translation), typeof(Rotation));

        NavMesh.SamplePosition(basePosition, out var hit, 10.0f, NavMesh.AllAreas);
        entityManager.SetComponentData(formationCenter, new Translation
        {
            Value = hit.position,
        });
        entityManager.SetComponentData(formationCenter, new Rotation
        {
            Value = quaternion.identity,
        });
        entityManager.SetComponentData(formationCenter, new FormationCenterComponent
        {
            speed = 10,
        });

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(template, settings);
        foreach (var offset in offsets)
        {
            var instance = entityManager.Instantiate(prefab);

            var position = basePosition + offset;
            var rotation = quaternion.identity;

            entityManager.SetComponentData(instance, new Translation { Value = position });
            entityManager.SetComponentData(instance, new Rotation { Value = rotation });
            entityManager.AddComponentData(instance, new ShipComponent
            {
                speed = 10
            });
            entityManager.AddComponentData(instance, new FactionComponent
            {
                faction = FactionComponent.Faction.A
            });
            entityManager.AddComponentData(instance, new BatteryComponent
            {
                interval = Random.Range(5, 10),
                speed = 40,
            });
            entityManager.AddComponentData(instance, new FormationMemberComponent
            {
                offset = offset
            });
        }

        entityManager.DestroyEntity(prefab);
    }

    private void SpawnFaction(Vector3 basePos, FactionComponent.Faction faction)
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(template, settings);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int i = 0; i < count; i++)
        {
            var instance = entityManager.Instantiate(prefab);

            var offset = Random.insideUnitCircle * radius;
            var position = basePos + new Vector3(offset.x, 0, offset.y);
            var rotation = quaternion.LookRotation(-position, math.up());

            entityManager.SetComponentData(instance, new Translation { Value = position });
            entityManager.SetComponentData(instance, new Rotation { Value = rotation });
            entityManager.AddComponentData(instance, new OrbitalMovementComponent { angularSpeed = 2 * math.PI / f });
            entityManager.AddComponentData(instance, new ShipComponent());
            entityManager.AddComponentData(instance, new FactionComponent
            {
                faction = faction
            });
            entityManager.AddComponentData(instance, new BatteryComponent
            {
                interval = Random.Range(5, 10),
                speed = 40,
            });
        }

        entityManager.DestroyEntity(prefab);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}