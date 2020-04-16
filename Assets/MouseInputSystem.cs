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

            var formationSystem = world.GetExistingSystem<FormationSystem>();
            formationSystem.UpdatePath(hitInfo.point);
        }
    }
}