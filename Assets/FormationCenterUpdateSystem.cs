using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

//[UpdateAfter(typeof(MouseInputSystem))]
//public class FormationCenterUpdateSystem : SystemBase
//{
//    private const int NodePoolSize = 1024;
//    private const int MaxIterations = 1024;

//    public NavMeshQuery navMeshQuery;
//    public bool queryDone;

//    private NativeArray<PolygonId> result;
//    private int length;

//    protected override void OnCreate()
//    {
//        navMeshQuery = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, NodePoolSize);

//        result = new NativeArray<PolygonId>(NodePoolSize, Allocator.Persistent);
//        queryDone = true;
//    }

//    protected override void OnDestroy()
//    {
//        navMeshQuery.Dispose();
//        result.Dispose();
//    }

//    protected override void OnUpdate()
//    {
//        if (queryDone)
//        {
//            var formationCenter = GetSingletonEntity<FormationCenterComponent>();
//        }
//        else
//        {
//            if (navMeshQuery.UpdateFindPath(MaxIterations, out _) == PathQueryStatus.Success)
//            {
//                var finalStatus = navMeshQuery.EndFindPath(out length);
//                var pathResult = navMeshQuery.GetPathResult(result);
//                queryDone = true;
//            }
//        }
//    }
//}

public class FormationCenterUpdateSystem : SystemBase
{
    private NavMeshPath path;
    private int currentSegment;

    protected override void OnCreate()
    {
        path = new NavMeshPath();
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
            return;

        var formationCenter = GetSingleton<FormationCenterComponent>();

        Vector3 nextPos = formationCenter.position;
        var speed = formationCenter.speed;
        var rotation = formationCenter.rotation;
        var travelDistance = speed * Time.DeltaTime;

        while (travelDistance > 0 && currentSegment < path.corners.Length - 1)
        {
            var segmentLength = math.distance(nextPos, path.corners[currentSegment + 1]);

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

        if (currentSegment < path.corners.Length - 1)
            rotation = math.normalize(quaternion.LookRotation(path.corners[currentSegment + 1] - nextPos, Vector3.up));

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