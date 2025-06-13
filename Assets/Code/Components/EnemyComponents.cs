using Unity.Entities;
using Unity.Mathematics;

namespace Code.Components
{
	public struct EnemyTag : IComponentData {}
	
	public struct EnemyPrefab : IComponentData
	{
		public Entity Prefab;
		public float Speed;
		public float2 ColliderData;
	}
	
	public struct EnemyMovement : IComponentData
	{
		public float2 Direction;
		public float Speed;
	}
	
	public struct MovesToPlayerFlag : IComponentData, IEnableableComponent {}
}