using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FormationMemberSystem : SystemBase
{
    private EntityQuery group;

    protected override void OnCreate()
    {
        group = GetEntityQuery(ComponentType.ReadOnly(typeof(FormationMemberComponent)),
            ComponentType.ReadOnly(typeof(ShipComponent)),
            typeof(Translation),
            typeof(Rotation));
    }

    [BurstCompile]
    struct FormationMemberJob : IJobChunk
    {
        public float deltaTime;

        public FormationCenterComponent formationCenter;

        [ReadOnly] public ArchetypeChunkComponentType<FormationMemberComponent> formationMemberType;
        [ReadOnly] public ArchetypeChunkComponentType<ShipComponent> shipType;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Rotation> rotationType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkFormationMembers = chunk.GetNativeArray(formationMemberType);
            var chunkShips = chunk.GetNativeArray(shipType);
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRotations = chunk.GetNativeArray(rotationType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var targetPosition = formationCenter.position + math.mul(formationCenter.rotation, chunkFormationMembers[i].offset);
                var distance = math.distance(chunkTranslations[i].Value, targetPosition);

                if (distance < 0.0001f)
                    continue;

                var maxTravelDistance = chunkShips[i].speed * deltaTime;

                var nextPosition = math.lerp(chunkTranslations[i].Value, targetPosition, math.saturate(maxTravelDistance / distance));

                var direction = nextPosition - chunkTranslations[i].Value;
                var nextRotation = math.normalize(quaternion.LookRotation(direction, Vector3.up));

                chunkTranslations[i] = new Translation { Value = nextPosition };
                chunkRotations[i] = new Rotation { Value = nextRotation };
            }
        }
    }

    protected override void OnUpdate()
    {
        var formationCenter = GetSingleton<FormationCenterComponent>();

        var formationMemberType = GetArchetypeChunkComponentType<FormationMemberComponent>();
        var shipType = GetArchetypeChunkComponentType<ShipComponent>();
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var rotationType = GetArchetypeChunkComponentType<Rotation>();

        var job = new FormationMemberJob
        {
            deltaTime = Time.DeltaTime,
            formationCenter = formationCenter,
            formationMemberType = formationMemberType,
            shipType = shipType,
            rotationType = rotationType,
            translationType = translationType,
        };

        Dependency = job.Schedule(group, Dependency);
    }
}