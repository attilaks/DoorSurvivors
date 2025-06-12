using Code.Components;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Code.Systems
{
	[BurstCompile]
	public partial class PlayerInputSystem : SystemBase
	{
		private PlayerInputActions _input;

		protected override void OnCreate()
		{
			_input = new PlayerInputActions();
			_input.Enable();
		}
		
		protected override void OnUpdate()
		{
			var moveInput = _input.Player.Move.ReadValue<Vector2>();
			var cursorPosition = _input.Player.Look.ReadValue<Vector2>();
			
			foreach (var (_, input) in SystemAPI.Query<RefRO<PlayerTag>,  RefRW<PlayerInputState>>())
			{
				input.ValueRW.MoveDirection = moveInput;
				input.ValueRW.CursorPosition = cursorPosition;
			}
		}

		protected override void OnDestroy()
		{
			_input.Dispose();
		}
	}
}