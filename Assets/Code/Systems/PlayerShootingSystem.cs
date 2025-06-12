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
			state.RequireForUpdate<ArenaBoundsData>();
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
	public partial struct BulletFlySystem : ISystem, ISystemStartStop
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

			var entitiesToDestroy = new NativeList<Entity>(Allocator.Temp);
			
			foreach (var (_, direction, bulletTransform, speed, entity) in 
			         SystemAPI.Query<RefRO<BulletTag>, RefRO<Direction>, RefRW<LocalTransform>, RefRO<MoveSpeed>>()
				         .WithEntityAccess())
			{
				var newXPosition = bulletTransform.ValueRW.Position.x + direction.ValueRO.Value.x 
					* speed.ValueRO.Value * deltaTime;
				if (newXPosition > _arenaBounds.MaxValues.x || newXPosition < _arenaBounds.MinValues.x)
				{
					entitiesToDestroy.Add(entity);
					continue;
				}
				
				var newYPosition = bulletTransform.ValueRW.Position.y + direction.ValueRO.Value.y 
					* speed.ValueRO.Value * deltaTime;
				if (newYPosition > _arenaBounds.MaxValues.y || newYPosition < _arenaBounds.MinValues.y)
				{
					entitiesToDestroy.Add(entity);
					continue;
				}
				
				bulletTransform.ValueRW.Position = new float3(newXPosition, newYPosition, 0);
			}

			for (var i = 0; i < entitiesToDestroy.Length; i++)
			{
				state.EntityManager.DestroyEntity(entitiesToDestroy[i]);
			}

			entitiesToDestroy.Dispose();
		}

		public void OnStopRunning(ref SystemState state) { }
	}
}