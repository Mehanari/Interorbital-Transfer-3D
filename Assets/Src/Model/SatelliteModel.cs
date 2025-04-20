using MehaMath.Math.Components;

namespace Src.Model
{
	public class SatelliteModel
	{
		public Vector EarthPosition { get; set; }
		public Spacecraft Spacecraft { get; set; }
		public double GravitationalParameter{ get; set; }
	}
}