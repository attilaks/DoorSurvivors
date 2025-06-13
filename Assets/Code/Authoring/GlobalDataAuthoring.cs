using Code.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Authoring
{
	public class GlobalDataAuthoring : MonoBehaviour
	{
		[SerializeField] private Transform groundTransform;
		[SerializeField] private uint maxEnemies = 100;
		
		private class GlobalDataBaker : Baker<GlobalDataAuthoring>
		{
			public override void Bake(GlobalDataAuthoring authoring)
			{
				var world = World.DefaultGameObjectInjectionWorld;
				var entityManager = world.EntityManager;
			
				var globalDataEntity = entityManager.CreateEntity();
				var maxXValue = authoring.groundTransform.localScale.x / 2;
				var maxYValue = authoring.groundTransform.localScale.y / 2;
				var minXValue = -maxXValue;
				var minYValue = -maxYValue;

				entityManager.AddComponentData(globalDataEntity, new ArenaBoundsData(
					new float2(maxXValue, maxYValue), new float2(minXValue, minYValue)));
				entityManager.AddComponentData(globalDataEntity, new EnemiesData { MaxEnemies = authoring.maxEnemies });
			}
		}
	}
}