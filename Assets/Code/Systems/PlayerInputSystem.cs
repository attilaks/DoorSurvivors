using Code.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Code.Systems
{
	[BurstCompile]
	public partial struct PlayerInputJob : IJobEntity
	{
		public float2 MoveInput;
		public float2 CursorPosition;

		private void Execute([EntityIndexInQuery] int index, ref PlayerInputState input)
		{
			input.MoveDirection = MoveInput;
			input.CursorPosition = CursorPosition;
		}
	}
	
	[BurstCompile]
	public partial class PlayerInputSystem : SystemBase
	{
		private PlayerInputActions _input;
		private Camera _camera;

		protected override void OnCreate()
		{
			_input = new PlayerInputActions();
			_input.Enable();
			_camera = Camera.main;
		}
		
		protected override void OnUpdate()
		{
			var moveInput = _input.Player.Move.ReadValue<Vector2>();
			var screenCursorPosition = _input.Player.Look.ReadValue<Vector2>();
			var worldCursorPosition = _camera.ScreenToWorldPoint(new Vector3(screenCursorPosition.x, screenCursorPosition.y, _camera.nearClipPlane));
			var cursorPosition = new float2(worldCursorPosition.x, worldCursorPosition.y);
			
			new PlayerInputJob
			{
				MoveInput = moveInput,
				CursorPosition = cursorPosition
			}.ScheduleParallel();
		}

		protected override void OnDestroy()
		{
			_input.Disable();
			_input.Dispose();
		}
	}
}