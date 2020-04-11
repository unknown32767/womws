using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public GameObject template;

    public int count;
    public float radius;
    public float f;

    void Start()
    {
        var position = transform.position;
        SpawnFaction(position, FactionComponent.Faction.A);

        SpawnFaction(-position, FactionComponent.Faction.B);
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
                speed = Random.Range(150, 200),
            });
        }

        entityManager.DestroyEntity(prefab);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, radius);
    }
}
