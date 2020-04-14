using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

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

    private EntityQuery group;

    protected override void OnCreate()
    {
        path = new NavMeshPath();
        
        group = GetEntityQuery(typeof(FormationCenterComponent), typeof(Translation));
    }

    protected override void OnUpdate()
    {
        var start = group.GetSingleton<Translation>().Value;
        var end = GetSingleton<UserCommandComponent>().destination;

        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

        foreach (var pos in path.corners)
        {
        }
    }
}