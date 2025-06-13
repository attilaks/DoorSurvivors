using Code.Components;
using Unity.Burst;
using Unity.Entities;

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
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			
		}

		public void OnStopRunning(ref SystemState state) { }
	}
}