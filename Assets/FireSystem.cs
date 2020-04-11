﻿using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FireSystem : SystemBase, IDeclareReferencedPrefabs
{
    private EntityQuery group;

    private EntityCommandBufferSystem barrier;

    private Entity prefab;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Resources.Load<GameObject>("Sphere"));
    }

    protected override void OnCreate()
    {
        group = GetEntityQuery(ComponentType.ReadOnly<ShipComponent>(), typeof(BatteryComponent));
        barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Resources.Load<GameObject>("Sphere"), settings);
    }

    [BurstCompile]
    struct FireJob : IJobChunk
    {
        public float deltaTime;

        public EntityCommandBuffer.Concurrent commandBuffer;
        public Entity prefab;
        public Unity.Mathematics.Random random;

        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<FactionComponent> factionComponentType;
        public ArchetypeChunkComponentType<BatteryComponent> batteryComponentType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkBatteries = chunk.GetNativeArray(batteryComponentType);
            var chunkFactions = chunk.GetNativeArray(factionComponentType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var countDown = chunkBatteries[i].countDown - deltaTime;
                if (countDown < 0)
                {
                    int selected;
                    do
                    {
                        selected = random.NextInt(chunk.Count);
                    }
                    while (chunkFactions[i].faction == chunkFactions[selected].faction);

                    var vec = chunkTranslations[selected].Value - chunkTranslations[i].Value;
                    var d = math.length(vec);
                    var g = 9.8f;
                    var v = chunkBatteries[i].speed;

                    if ((d * g) / (v * v) <= 1)
                    {
                        var instance = commandBuffer.Instantiate(chunkIndex, prefab);
                        var theta = math.asin((d * g) / (v * v)) / 2;

                        var vxz = math.normalize(new float3(vec.x, 0, vec.z));
                        var velocity = math.up() * math.sin(theta) * v + vxz * math.cos(theta) * v;
                        commandBuffer.AddComponent<BulletComponent>(chunkIndex, instance, new BulletComponent { vec = velocity });
                        commandBuffer.SetComponent(chunkIndex, instance, new Translation { Value = chunkTranslations[i].Value });
                    }
                }
            }
        }
    }


    protected override void OnUpdate()
    {
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var factionComponentType = GetArchetypeChunkComponentType<FactionComponent>();
        var batteryComponentType = GetArchetypeChunkComponentType<BatteryComponent>();

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));

        var job = new FireJob
        {
            deltaTime = Time.DeltaTime,
            commandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
            prefab = prefab,
            random = random,
            batteryComponentType = batteryComponentType,
            factionComponentType = factionComponentType,
            translationType = translationType,
        };

        Dependency = job.Schedule(group, Dependency);
        barrier.AddJobHandleForProducer(Dependency);
    }
}
