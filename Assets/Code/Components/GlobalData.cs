using Unity.Entities;
using Unity.Mathematics;

namespace Code.Components
{
	public struct ArenaBoundsData : IComponentData
	{
		public readonly float2 MaxValues;
		public readonly float2 MinValues;

		public ArenaBoundsData(float2 maxValues, float2 minValues)
		{
			MaxValues = maxValues;
			MinValues = minValues;
		}
	}

	public struct EnemiesData : IComponentData
	{
		public uint MaxEnemies;
	}
}