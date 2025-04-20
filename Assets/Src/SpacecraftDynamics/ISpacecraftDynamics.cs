using Src.Model;

namespace Src.SpacecraftDynamics
{
	public interface ISpacecraftDynamics
	{
		public Spacecraft PropagateState(Spacecraft spacecraft, double deltaT);
	}
}