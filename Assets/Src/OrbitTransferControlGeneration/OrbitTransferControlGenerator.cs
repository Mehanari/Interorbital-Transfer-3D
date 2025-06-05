using Src.Model;
using Src.OptimizationFramework;
using Src.OptimizationFramework.DataModels;

namespace Src.OrbitTransferControlGeneration
{
	public abstract class OrbitTransferControlGenerator
	{
		public Spacecraft InitialState { get; set; }
		public Orbit GoalOrbit { get; set; }

		public abstract ControlData GenerateControl();
	}
}