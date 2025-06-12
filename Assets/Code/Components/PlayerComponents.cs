using Unity.Entities;
using Unity.Mathematics;

namespace Code.Components
{
	public struct PlayerTag : IComponentData {}
	
	public struct PlayerPrefab : IComponentData
	{
		public Entity Prefab;
		public float Speed;
	}

	public struct HealthState : IComponentData
	{
		public uint Value;
	}

	public struct MoveSpeed : IComponentData
	{
		public float Value;
	}
	
	public struct PlayerInputState : IComponentData
	{
		public float2 MoveDirection;
	}
}