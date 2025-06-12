using Code.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

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
			
			foreach (var input in SystemAPI.Query<RefRW<PlayerInputState>>())
			{
				input.ValueRW.MoveDirection = moveInput;
			}
		}

		protected override void OnDestroy()
		{
			_input.Dispose();
		}
	}
}