using Code.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Code.Systems
{
	public struct ColliderData : IComponentData
	{
		public float2 Size;
	}
	
	[UpdateAfter(typeof(SpawnPlayerSystem))]
	public partial struct CollisionSystem : ISystem
	{
		private EntityQuery _enemiesQuery;
		private EntityQuery _bulletsQuery;
		private EntityQuery _playerQuery;

		public void OnCreate(ref SystemState state)
		{
			_enemiesQuery = SystemAPI.QueryBuilder()
				.WithPresent<EnemyTag, LocalTransform, ColliderData>()
				.Build();
			_bulletsQuery = SystemAPI.QueryBuilder()
				.WithPresent<BulletTag, LocalTransform, ColliderData>()
				.Build();
			_playerQuery = SystemAPI.QueryBuilder()
				.WithPresent<PlayerTag, LocalTransform, ColliderData, HealthState>()
				.Build();
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var enemyEntities = _enemiesQuery.ToEntityArray(Allocator.Temp);
			var bulletEntities = _bulletsQuery.ToEntityArray(Allocator.Temp);
			var player = _playerQuery.GetSingletonEntity();
			
			var transforms = SystemAPI.GetComponentLookup<LocalTransform>(true);
			var colliders = SystemAPI.GetComponentLookup<ColliderData>(true);
			var healths = SystemAPI.GetComponentLookup<HealthState>();
			
			var playerTransform = transforms[player];
			var playerCollider = colliders[player];
			ref var playerHealth = ref healths.GetRefRW(player).ValueRW;

			var entitiesToDestroy = new NativeList<Entity>(Allocator.Temp);

			for (var i = 0; i < enemyEntities.Length; i++)
			{
				var enemy = enemyEntities[i];
				var enemyTransform = transforms[enemy];
				var enemyCollider = colliders[enemy];

				if (CheckAABBCollision(enemyTransform.Position.xy, enemyCollider.Size,
					    playerTransform.Position.xy, playerCollider.Size))
				{
					playerHealth.Value -= 2;
					entitiesToDestroy.Add(enemy);

					if (playerHealth.Value <= 0)
					{
						//todo
					}
					continue;
				}

				for (var j = 0; j < bulletEntities.Length; j++)
				{
					var bullet = bulletEntities[j];
					var bulletTransform = transforms[bullet];
					var bulletCollider = colliders[bullet];
					if (CheckAABBCollision(enemyTransform.Position.xy, enemyCollider.Size,
						    bulletTransform.Position.xy, bulletCollider.Size))
					{
						entitiesToDestroy.Add(bullet);
						entitiesToDestroy.Add(enemy);
					}
				}
			}

			for (var i = 0; i < entitiesToDestroy.Length; i++)
			{
				state.EntityManager.DestroyEntity(entitiesToDestroy[i]);
			}
		}

		private bool CheckAABBCollision(float2 posA, float2 sizeA, float2 posB, float2 sizeB)
		{
			var collisionX = posA.x + sizeA.x / 2 > posB.x - sizeB.x / 2 &&
			                 posA.x - sizeA.x / 2 < posB.x + sizeB.x / 2;
			if (!collisionX) return false;
        
			var collisionY = posA.y + sizeA.y / 2 > posB.y - sizeB.y / 2 &&
			                 posA.y - sizeA.y / 2 < posB.y + sizeB.y / 2;
        
			return collisionY;
		}
	}
}