using MehaMath.Math.Components;
using Src.Model;

namespace Src.SpacecraftDynamics.CentralBodyDynamics
{
	public abstract class CentralBodyDynamics : ISpacecraftDynamics
	{
		public Vector CentralBodyPosition { get; set; }
		public double GravitationalParameter { get; set; }
		
		
		public abstract Spacecraft PropagateState(Spacecraft spacecraft, double deltaT);
	}
}