using Code.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	[BurstCompile]
	public partial struct PlayerMovementSystem : ISystem, ISystemStartStop
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
			var deltaTime = SystemAPI.Time.DeltaTime;

			foreach (var (transform, input, speed) in 
			         SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerInputState>, RefRO<MoveSpeed>>())
			{
				var moveX = SetAxisMove(input.ValueRO.MoveDirection.x, transform.ValueRO.Position.x,
					_arenaBounds.MaxValues.x, _arenaBounds.MinValues.x);
				var moveY = SetAxisMove(input.ValueRO.MoveDirection.y, transform.ValueRO.Position.y,
					_arenaBounds.MaxValues.y, _arenaBounds.MinValues.y);
		
				if (moveX == 0 && moveY == 0) return;
				
				var move = new float3(moveX, moveY, 0f);
				transform.ValueRW.Position += move * speed.ValueRO.Value * deltaTime;
			}
		}

		public void OnStopRunning(ref SystemState state) { }

		private float SetAxisMove(float axisInput, float currentPlayerPositionAxisValue, float maxValue, float minValue)
		{
			var moveAxis = axisInput;
			if (maxValue < moveAxis + currentPlayerPositionAxisValue)
			{
				moveAxis = maxValue - currentPlayerPositionAxisValue;
			}
			else if (minValue > moveAxis + currentPlayerPositionAxisValue)
			{
				moveAxis = minValue - currentPlayerPositionAxisValue;
			}
			
			return moveAxis;
		}
	}
}