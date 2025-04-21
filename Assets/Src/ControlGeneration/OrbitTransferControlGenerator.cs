using Src.Model;

namespace Src.ControlGeneration
{
	public abstract class OrbitTransferControlGenerator
	{
		public Spacecraft InitialState { get; set; }
		public Orbit GoalOrbit { get; set; }

		public abstract ControlData GenerateControl();
	}
}