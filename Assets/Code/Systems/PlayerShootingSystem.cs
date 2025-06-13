using Code.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Code.Systems
{
	public struct BulletTag : IComponentData {}
	
	public struct BulletPrefab : IComponentData
	{
		public Entity Prefab;
		public float Speed;
		public float ShootPerSecond;
		public float Scale;
		public float2 ColliderData;
	}
	
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
			state.EntityManager.AddComponentData(_bullet, new ColliderData { Size = bulletPrefab.ColliderData });
			
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
			var ecb = new EntityCommandBuffer(Allocator.TempJob);
    
			new BulletFlyJob
			{
				DeltaTime = deltaTime,
				ArenaBounds = _arenaBounds,
				ECB = ecb.AsParallelWriter()
			}.ScheduleParallel();
    
			state.Dependency.Complete();
			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}

		public void OnStopRunning(ref SystemState state) { }
	}
	
	[BurstCompile]
	internal partial struct BulletFlyJob : IJobEntity
	{
		public float DeltaTime;
		public ArenaBoundsData ArenaBounds;
		public EntityCommandBuffer.ParallelWriter ECB;

		private void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, 
			ref LocalTransform transform, in BulletTag tag, in Direction direction, in MoveSpeed speed)
		{
			var newXPosition = transform.Position.x + direction.Value.x * speed.Value * DeltaTime;
			var newYPosition = transform.Position.y + direction.Value.y * speed.Value * DeltaTime;
        
			if (newXPosition > ArenaBounds.MaxValues.x || newXPosition < ArenaBounds.MinValues.x ||
			    newYPosition > ArenaBounds.MaxValues.y || newYPosition < ArenaBounds.MinValues.y)
			{
				ECB.DestroyEntity(chunkIndex, entity);
				return;
			}
        
			transform.Position = new float3(newXPosition, newYPosition, 0);
		}
	}
}