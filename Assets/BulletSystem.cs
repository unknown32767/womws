using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class BulletSystem : SystemBase
{
    private EntityQuery group;

    private EntityCommandBufferSystem barrier;

    protected override void OnCreate()
    {
        group = GetEntityQuery(typeof(BulletComponent), typeof(Translation));
        barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct BulletJob : IJobChunk
    {
        public float deltaTime;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<BulletComponent> bulletComponentType;
        [ReadOnly] public ArchetypeChunkEntityType entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkBullets = chunk.GetNativeArray(bulletComponentType);
            var chunkEntities = chunk.GetNativeArray(entityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var vec0 = chunkBullets[i].vec;
                var vec1 = vec0 - math.up() * 9.8f * deltaTime;
                var newPos = chunkTranslations[i].Value + (vec0 + vec1) * 0.5f * deltaTime;

                chunkBullets[i] = new BulletComponent { vec = vec1 };
                chunkTranslations[i] = new Translation { Value = newPos };

                if (newPos.y < 0 && vec1.y < 0)
                {
                    commandBuffer.DestroyEntity(chunkIndex, chunkEntities[i]);
                }
            }
        }
    }

    protected override void OnUpdate()
    {
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var bulletComponentType = GetArchetypeChunkComponentType<BulletComponent>();
        var entityType = GetArchetypeChunkEntityType();

        var job = new BulletJob
        {
            deltaTime = Time.DeltaTime,
            commandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
            bulletComponentType = bulletComponentType,
            translationType = translationType,
            entityType = entityType,
        };

        Dependency = job.Schedule(group, Dependency);
        barrier.AddJobHandleForProducer(Dependency);
    }
}
