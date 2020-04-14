using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class MouseInputSystem : SystemBase
{
    private World world;

    private EntityQuery group;

    protected override void OnCreate()
    {
        group = GetEntityQuery(typeof(FormationCenterComponent), typeof(Translation));

        world = World.DefaultGameObjectInjectionWorld;
    }

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray, out var hitInfo, LayerMask.GetMask("Terrain"));

            SetSingleton(new UserCommandComponent
            {
                destination = hitInfo.point
            });

            //var formationCenterUpdateSystem = world.GetExistingSystem<FormationCenterUpdateSystem>();
            //var navMeshQuery = formationCenterUpdateSystem.navMeshQuery;

            //var translation = group.GetSingleton<Translation>();

            //var start = navMeshQuery.MapLocation(translation.Value, Vector3.one * 10, 0);
            //var end = navMeshQuery.MapLocation(hitInfo.point, Vector3.one * 10, 0);
            
            //formationCenterUpdateSystem.queryDone = false;
            //navMeshQuery.BeginFindPath(start, end);
        }
    }
}