using Code.Components;
using Code.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Authoring
{
	public class EnemyPrefabAuthoring : MonoBehaviour
	{
		[SerializeField] private GameObject enemyPrefab;
		[Range(1, 10)] [SerializeField] private float enemySpeed = 3f;
		
		private class EnemyPrefabBaker : Baker<EnemyPrefabAuthoring>
		{
			public override void Bake(EnemyPrefabAuthoring authoring)
			{
				if (!authoring.enemyPrefab) return;
				
				var enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);
				
				var enemyPrefabEntity = GetEntity(TransformUsageFlags.None);
				var enemyCollider = authoring.enemyPrefab.GetComponent<BoxCollider>();
				AddComponent(enemyPrefabEntity, new EnemyPrefab
				{
					Prefab = enemyPrefab,
					Speed = authoring.enemySpeed,
					ColliderData = new float2(enemyCollider.size.x, enemyCollider.size.y)
				});
			}
		}
	}
}