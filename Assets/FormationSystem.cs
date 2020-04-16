using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class FormationSystem : SystemBase
{
    private NavMeshPath path;
    private int currentSegment;

    private float radius;

    protected override void OnCreate()
    {
        path = new NavMeshPath();

        radius = Spawner.offsets.Select(x => x.magnitude).Max();
    }

    public void UpdatePath(Vector3 end)
    {
        var start = GetSingleton<FormationCenterComponent>().position;

        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

        currentSegment = 0;
    }

    protected override void OnUpdate()
    {
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            return;
        }

        var formationCenter = GetSingleton<FormationCenterComponent>();

        Vector3 nextPos = formationCenter.position;
        var speed = formationCenter.speed;
        var rotation = formationCenter.rotation;
        var travelDistance = speed * Time.DeltaTime;

        while (travelDistance > 0 && currentSegment < path.corners.Length - 1)
        {
            var segmentLength = math.distance(nextPos, path.corners[currentSegment + 1]);

            //在该段路线的起点
            if (nextPos == path.corners[currentSegment])
            {
                //转向未完成
                if (math.dot(math.normalize(math.mul(rotation, Vector3.forward)) , 
                        math.normalize(path.corners[currentSegment + 1] - path.corners[currentSegment])) < 1)
                {
                    var turnAngle = math.radians(Vector3.SignedAngle(math.mul(rotation, Vector3.forward),
                        path.corners[currentSegment + 1] - path.corners[currentSegment], Vector3.up));

                    var maxTurnAngle = travelDistance / radius;

                    if (maxTurnAngle >= math.abs(turnAngle))
                    {
                        rotation = math.normalize(quaternion.LookRotation(
                            path.corners[currentSegment + 1] - path.corners[currentSegment], Vector3.up));
                        travelDistance -= math.abs(turnAngle) * radius;
                    }
                    else
                    {
                        rotation = math.mul(rotation, quaternion.Euler(0, maxTurnAngle * math.sign(turnAngle), 0));
                        travelDistance = 0;
                        break;
                    }
                }
            }

            //剩余可用路程长度在本段之内
            if (travelDistance < segmentLength)
            {
                nextPos = math.lerp(nextPos, path.corners[currentSegment + 1], travelDistance / segmentLength);
                travelDistance = 0;
            }
            else
            {
                travelDistance -= segmentLength;
                nextPos = path.corners[currentSegment + 1];
                currentSegment += 1;
            }
        }

        SetSingleton(new FormationCenterComponent
        {
            speed = formationCenter.speed,
            position = nextPos,
            rotation = rotation,
        });

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.blue);
        }

        Debug.DrawRay(nextPos, math.mul(rotation, Vector3.forward), Color.red);
    }
}