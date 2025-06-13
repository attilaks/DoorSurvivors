using Code.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Code.Systems
{
	[BurstCompile]
	public partial struct SpawnEnemiesSystem : ISystem, ISystemStartStop
	{
		private EnemyPrefab _enemyPrefab;
		private EnemiesData _enemiesData;
		private ArenaBoundsData _arenaBounds;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<ArenaBoundsData>();
			state.RequireForUpdate<EnemiesData>();
		}

		[BurstCompile]
		public void OnStartRunning(ref SystemState state)
		{
			_arenaBounds = SystemAPI.GetSingleton<ArenaBoundsData>();
			_enemiesData = SystemAPI.GetSingleton<EnemiesData>();
			
			foreach (var prefab in
			         SystemAPI.Query<RefRO<EnemyPrefab>>())
			{
				_enemyPrefab = prefab.ValueRO;
				break;
			}
			
			var entity = state.EntityManager.CreateEntity();
			state.EntityManager.AddComponentData(entity, new RandomState
			{
				Random = Random.CreateFromIndex(1)
			});
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var randomState = SystemAPI.GetSingletonRW<RandomState>();
			ref var random = ref randomState.ValueRW.Random;
			
		    var enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build();
		    var enemyCount = enemyQuery.CalculateEntityCount();
		    if (enemyCount >= _enemiesData.MaxEnemies) return;
		    
		    var spawnPosition = GetSpawnPositionOutsideArena(ref random);
		    var targetPosition = GetNearestPointOnArenaBorder(spawnPosition);
		    
		    var ecb = new EntityCommandBuffer(Allocator.TempJob);
		    var newEnemy = ecb.Instantiate(_enemyPrefab.Prefab);
		    ecb.SetComponent(newEnemy, LocalTransform.FromPosition(
		        new float3(spawnPosition.x, spawnPosition.y, 0)));
		    
		    var direction = math.normalize(targetPosition - spawnPosition);
		    ecb.AddComponent(newEnemy, new EnemyMovement
		    {
		        Direction = new float2(direction.x, direction.y),
		        Speed = _enemyPrefab.Speed
		    });
		    ecb.AddComponent<EnemyTag>(newEnemy);
		    ecb.AddComponent<MovesToPlayerFlag>(newEnemy);
		    ecb.SetComponentEnabled<MovesToPlayerFlag>(newEnemy, false);
		    
		    ecb.Playback(state.EntityManager);
		    ecb.Dispose();
		}
		
		private float2 GetSpawnPositionOutsideArena(ref Random random)
		{
		    var side = random.NextInt(0, 4);
		    float x, z;
		    
		    switch (side)
		    {
		        case 0:
		            x = random.NextFloat(_arenaBounds.MinValues.x, _arenaBounds.MaxValues.x);
		            z = _arenaBounds.MaxValues.y + random.NextFloat(1f, 5f);
		            break;
		        case 1:
		            x = _arenaBounds.MaxValues.x + random.NextFloat(1f, 5f);
		            z = random.NextFloat(_arenaBounds.MinValues.y, _arenaBounds.MaxValues.y);
		            break;
		        case 2:
		            x = random.NextFloat(_arenaBounds.MinValues.x, _arenaBounds.MaxValues.x);
		            z = _arenaBounds.MinValues.y - random.NextFloat(1f, 5f);
		            break;
		        case 3:
		            x = _arenaBounds.MinValues.x - random.NextFloat(1f, 5f);
		            z = random.NextFloat(_arenaBounds.MinValues.y, _arenaBounds.MaxValues.y);
		            break;
		        default:
		            x = 0;
		            z = 0;
		            break;
		    }
		    
		    return new float2(x, z);
		}
		
		private float2 GetNearestPointOnArenaBorder(float2 position)
		{
		    var x = math.clamp(position.x, _arenaBounds.MinValues.x, _arenaBounds.MaxValues.x);
		    var y = math.clamp(position.y, _arenaBounds.MinValues.y, _arenaBounds.MaxValues.y);
		    
		    if (Mathf.Approximately(position.x, x) && Mathf.Approximately(position.y, y))
		    {
		        return new float2(_arenaBounds.MinValues.x, _arenaBounds.MinValues.y);
		    }
		    
		    if (position.x < _arenaBounds.MinValues.x)
		        return new float2(_arenaBounds.MinValues.x, y);
		    if (position.x > _arenaBounds.MaxValues.x)
		        return new float2(_arenaBounds.MaxValues.x, y);
		    if (position.y < _arenaBounds.MinValues.y)
		        return new float2(x, _arenaBounds.MinValues.y);
		    if (position.y > _arenaBounds.MaxValues.y)
		        return new float2(x, _arenaBounds.MaxValues.y);
		    
		    return new float2(
		        (_arenaBounds.MinValues.x + _arenaBounds.MaxValues.x) * 0.5f,
		        (_arenaBounds.MinValues.y + _arenaBounds.MaxValues.y) * 0.5f);
		}

		public void OnStopRunning(ref SystemState state) { }
	}
}