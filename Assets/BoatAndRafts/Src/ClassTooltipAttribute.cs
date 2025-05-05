using System;

namespace BoatAndRafts.Src
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ClassTooltipAttribute : Attribute
	{
		public readonly string description;

		public ClassTooltipAttribute(string description)
		{
			this.description = description;
		}
	}
}