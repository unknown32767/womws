using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class OrbitalMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .ForEach((ref Translation translation, ref Rotation rotation, in OrbitalMovementComponent orbitalMovement) =>
            {
                var rot = quaternion.AxisAngle(math.up(), orbitalMovement.angularSpeed * deltaTime);

                rotation.Value = math.mul(math.normalize(rotation.Value), rot);
                translation.Value = math.mul(rot, translation.Value);
            })
            .ScheduleParallel();
    }
}
