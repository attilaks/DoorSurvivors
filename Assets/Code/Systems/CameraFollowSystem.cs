using Code.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Code.Systems
{
	public partial class CameraFollowSystem : SystemBase
	{
		private Camera _camera;

		protected override void OnCreate()
		{
			_camera = Camera.main;
		}
		
		protected override void OnUpdate()
		{
			if (!_camera) return;

			foreach (var (_, playerTransform) in 
			         SystemAPI.Query<RefRO<PlayerTag>, RefRO<LocalTransform>>())
			{
				Vector3 playerPosition = playerTransform.ValueRO.Position;
				_camera.transform.position = new Vector3(playerPosition.x, playerPosition.y, _camera.transform.position.z);
				_camera.transform.LookAt(playerPosition);
				return;
			}
		}
	}
}