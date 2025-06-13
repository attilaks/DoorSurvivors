using Code.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	[BurstCompile]
	public partial struct EnemyMovementSystem : ISystem, ISystemStartStop
	{
		private ArenaBoundsData _arenaBounds;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<ArenaBoundsData>();
		}
		
		public void OnStartRunning(ref SystemState state)
		{
			_arenaBounds = SystemAPI.GetSingleton<ArenaBoundsData>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.TempJob);
			var deltaTime = SystemAPI.Time.DeltaTime;
			
			foreach (var (transform, movement, entity) in 
			         SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyMovement>>()
				         .WithDisabled<MovesToPlayerFlag>()
				         .WithPresent<EnemyTag>()
				         .WithEntityAccess())
			{
				var move = new float3(movement.ValueRO.Direction.x, movement.ValueRO.Direction.y, 0f);
				transform.ValueRW.Position += move * movement.ValueRO.Speed * deltaTime;

				if (transform.ValueRO.Position.x < _arenaBounds.MaxValues.x
				    && transform.ValueRO.Position.x > _arenaBounds.MinValues.x
				    && transform.ValueRO.Position.y < _arenaBounds.MaxValues.y
				    && transform.ValueRO.Position.y > _arenaBounds.MinValues.y)
				{
					ecb.SetComponentEnabled<MovesToPlayerFlag>(entity, true);
				}
			}
			
			var playerPosition = float3.zero;
			foreach (var (_, playerTransform) in 
			         SystemAPI.Query<RefRO<PlayerTag>, RefRO<LocalTransform>>())
			{
				playerPosition = playerTransform.ValueRO.Position;
				break;
			}

			foreach (var (transform, movement, entity) in
			         SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyMovement>>()
				         .WithAll<MovesToPlayerFlag>()
				         .WithPresent<EnemyTag>()
				         .WithEntityAccess())
			{
				var direction = math.normalize(playerPosition - transform.ValueRO.Position);
				transform.ValueRW.Position += direction * movement.ValueRO.Speed * deltaTime;
			}
			
			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}

		public void OnStopRunning(ref SystemState state) { }
	}
}