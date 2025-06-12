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
		
		[Header("Settings")]
		[Range(1, 10)] [SerializeField] private float playerSpeed = 5f;
		[Range(1, 100)] [SerializeField] private float bulletSpeed = 40f;
		[Range(1, 20)] [SerializeField] private float shootsPerSecond = 5f;

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
					Speed = authoring.bulletSpeed,
					ShootPerSecond = authoring.shootsPerSecond,
					Scale = authoring.bulletPrefab.transform.localScale.x
				});
			}
		}
	}
}