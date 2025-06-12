using Code.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	[BurstCompile]
	public partial struct PlayerMovementSystem : ISystem
	{
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;

			foreach (var (transform, input, speed) in 
			         SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerInputState>, RefRO<MoveSpeed>>())
			{
				var move = new float3(input.ValueRO.MoveDirection.x, input.ValueRO.MoveDirection.y, 0f);
				transform.ValueRW.Position += move * speed.ValueRO.Value * deltaTime;
			}
		}
	}
}