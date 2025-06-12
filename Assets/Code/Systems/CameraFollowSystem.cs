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
				_camera.transform.position = playerTransform.ValueRO.Position + new float3(0, 0, _camera.transform.position.z);
				_camera.transform.LookAt(playerTransform.ValueRO.Position);
				return;
			}
		}
	}
}