using Code.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	[BurstCompile]
	public partial struct EnemyMovementSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach (var (transform, movement) in 
			         SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyMovement>>())
			{
				var move = new float3(movement.ValueRO.Direction.x, movement.ValueRO.Direction.y, 0f);
				transform.ValueRW.Position += move * movement.ValueRO.Speed * SystemAPI.Time.DeltaTime;
			}
		}
	}
}