using Code.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	public struct ColliderData : IComponentData
	{
		public float2 Size;
	}
	
	[UpdateAfter(typeof(SpawnPlayerSystem))]
	public partial struct CollisionSystem : ISystem
	{
		private EntityQuery _playerQuery;

		public void OnCreate(ref SystemState state)
		{
			_playerQuery = SystemAPI.QueryBuilder()
				.WithPresent<PlayerTag, LocalTransform, ColliderData, HealthState>()
				.Build();
			
			state.RequireForUpdate(_playerQuery);
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var transforms = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var colliders = SystemAPI.GetComponentLookup<ColliderData>(true);
            var healths = SystemAPI.GetComponentLookup<HealthState>();
            
            var playerEntity = _playerQuery.GetSingletonEntity();
            var playerTransform = transforms[playerEntity];
            var playerCollider = colliders[playerEntity];
            ref var playerHealth = ref healths.GetRefRW(playerEntity).ValueRW;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (enemyTransform, enemyCollider, enemyEntity) 
                in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ColliderData>>()
                    .WithAll<EnemyTag>()
                    .WithEntityAccess())
            {
                bool enemyDestroyed = false;
                
                if (CheckAABBCollision(
                    enemyTransform.ValueRO.Position.xy, enemyCollider.ValueRO.Size,
                    playerTransform.Position.xy, playerCollider.Size))
                {
                    playerHealth.Value -= 2;
                    enemyDestroyed = true;
                    
                    if (playerHealth.Value <= 0)
                    {
                        // Обработка смерти игрока
                        //todo
                    }
                }
                
                if (!enemyDestroyed)
                {
                    foreach (var (bulletTransform, bulletCollider, bulletEntity) 
                        in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ColliderData>>()
                            .WithAll<BulletTag>()
                            .WithEntityAccess())
                    {
                        if (CheckAABBCollision(
                            enemyTransform.ValueRO.Position.xy, enemyCollider.ValueRO.Size,
                            bulletTransform.ValueRO.Position.xy, bulletCollider.ValueRO.Size))
                        {
                            ecb.DestroyEntity(bulletEntity);
                            enemyDestroyed = true;
                            break;
                        }
                    }
                }
                
                if (enemyDestroyed)
                {
                    ecb.DestroyEntity(enemyEntity);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
		}

		private bool CheckAABBCollision(float2 posA, float2 sizeA, float2 posB, float2 sizeB)
		{
			var minA = posA - sizeA * 0.5f;
			var maxA = posA + sizeA * 0.5f;
			var minB = posB - sizeB * 0.5f;
			var maxB = posB + sizeB * 0.5f;

			return !(maxA.x < minB.x || minA.x > maxB.x || 
			         maxA.y < minB.y || minA.y > maxB.y);
		}
	}
}