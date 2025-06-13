using Unity.Entities;
using Unity.Mathematics;

namespace Code.Components
{
	public struct RandomState : IComponentData
	{
		public Random Random;
	}
}