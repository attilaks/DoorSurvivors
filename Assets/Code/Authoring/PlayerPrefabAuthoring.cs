using Code.Components;
using Unity.Entities;
using UnityEngine;

namespace Code.Authoring
{
	public class PlayerPrefabAuthoring : MonoBehaviour
	{
		[SerializeField] private GameObject playerPrefab;
		[SerializeField] private float speed = 5f;

		private class PlayerPrefabBaker : Baker<PlayerPrefabAuthoring>
		{
			public override void Bake(PlayerPrefabAuthoring authoring)
			{
				var entityPrefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic);
				
				var entity = GetEntity(TransformUsageFlags.None);
				AddComponent(entity, new PlayerPrefab
				{
					Prefab = entityPrefab,
					Speed = authoring.speed
				});
			}
		}
	}
}