using Code.Components;
using Unity.Collections;
using Unity.Entities;

namespace Code.Systems
{
	public partial struct SpawnPlayerSystem : ISystem, ISystemStartStop
	{
		public void OnStartRunning(ref SystemState state)
		{
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			
			foreach (var prefab in
			         SystemAPI.Query<RefRO<PlayerPrefab>>())
			{
				var playerEntity = ecb.Instantiate(prefab.ValueRO.Prefab);
				ecb.AddComponent<PlayerTag>(playerEntity);
				ecb.AddComponent(playerEntity, new HealthState { Value = 10 });
				ecb.AddComponent(playerEntity, new MoveSpeed { Value = prefab.ValueRO.Speed });
				ecb.AddComponent<PlayerInputState>(playerEntity);
				ecb.AddComponent(playerEntity, new ColliderData{Size = prefab.ValueRO.ColliderData});
			}

			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}

		public void OnStopRunning(ref SystemState state) { }
	}
}