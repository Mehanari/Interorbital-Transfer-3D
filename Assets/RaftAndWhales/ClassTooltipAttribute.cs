using System;

namespace RaftAndWhales
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