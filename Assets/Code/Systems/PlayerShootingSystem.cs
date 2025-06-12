using Code.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	public struct Direction : IComponentData
	{
		public float2 Value;
	}
	
	[BurstCompile]
	public partial struct PlayerShootingSystem : ISystem, ISystemStartStop
	{
		private Entity _bullet;
		private float _timeSinceLastShot;
		private float _intervalBetweenShots;
		private float _bulletScale;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<BulletPrefab>();
			_timeSinceLastShot = 0;
		}
		
		public void OnStartRunning(ref SystemState state)
		{
			var bulletPrefab = SystemAPI.GetSingleton<BulletPrefab>();
			_bullet = bulletPrefab.Prefab;
			state.EntityManager.AddComponentData(_bullet, new MoveSpeed{Value = bulletPrefab.Speed});
			state.EntityManager.AddComponent<BulletTag>(_bullet);
			
			_intervalBetweenShots = 1f / bulletPrefab.ShootPerSecond;
			_bulletScale = bulletPrefab.Scale;
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;
			_timeSinceLastShot += deltaTime;
			if (_timeSinceLastShot < _intervalBetweenShots) return;

			Direction? direction = null;
			var playerPosition = new float2();
			var scale = 1f;
			foreach (var (_, input, playerTransform) in 
			         SystemAPI.Query<RefRO<PlayerTag>, RefRO<PlayerInputState>, RefRO<LocalTransform>>())
			{
				var cursorPosition = input.ValueRO.CursorPosition;
				playerPosition = new float2(playerTransform.ValueRO.Position.x, playerTransform.ValueRO.Position.y);
				direction = new Direction { Value = math.normalize(cursorPosition - playerPosition) };
				break;
			}
			
			if (!direction.HasValue) return;
			
			var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
			var bulletEntity = ecb.Instantiate(_bullet);
			ecb.AddComponent(bulletEntity, direction.Value);
			ecb.SetComponent(bulletEntity, new LocalTransform
			{
				Position = new float3(playerPosition.x, playerPosition.y, 0f),
				Scale = _bulletScale
			});

			_timeSinceLastShot = 0;
		}

		public void OnStopRunning(ref SystemState state)
		{
			state.EntityManager.DestroyEntity(_bullet);
		}
	}
	
	[BurstCompile]
	public partial struct BulletFlySystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;
			
			foreach (var (_, direction, bulletTransform, speed) in 
			         SystemAPI.Query<RefRO<BulletTag>, RefRO<Direction>, RefRW<LocalTransform>, RefRO<MoveSpeed>>())
			{
				bulletTransform.ValueRW.Position += new float3(direction.ValueRO.Value.x, direction.ValueRO.Value.y, 0) * speed.ValueRO.Value * deltaTime;
			}
		}
	}
}