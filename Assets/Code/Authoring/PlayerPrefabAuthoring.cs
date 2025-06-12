using Code.Components;
using Unity.Entities;
using UnityEngine;

namespace Code.Authoring
{
	public class PlayerPrefabAuthoring : MonoBehaviour
	{
		[Header("Prefabs")]
		[SerializeField] private GameObject playerPrefab;
		[SerializeField] private GameObject bulletPrefab;
		
		[SerializeField] private float playerSpeed = 5f;
		[SerializeField] private float bulletSpeed = 40f;

		private class PlayerPrefabBaker : Baker<PlayerPrefabAuthoring>
		{
			public override void Bake(PlayerPrefabAuthoring authoring)
			{
				var playerPrefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic);
				var bulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic);
				
				var playerPrefabEntity = GetEntity(TransformUsageFlags.None);
				AddComponent(playerPrefabEntity, new PlayerPrefab
				{
					Prefab = playerPrefab,
					Speed = authoring.playerSpeed
				});
				
				var bulletPrefabEntity = GetEntity(TransformUsageFlags.None);
				AddComponent(bulletPrefabEntity, new BulletPrefab
				{
					Prefab = bulletPrefab,
					Speed = authoring.bulletSpeed
				});
			}
		}
	}
}