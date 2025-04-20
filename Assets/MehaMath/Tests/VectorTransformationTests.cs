using System.Collections;
using MehaMath.Math.Components;
using NUnit.Framework;

namespace MehaMath.Tests
{
	public class VectorTransformationTests
	{
		[Test]
		[TestCaseSource(nameof(NewReferenceFrameCases))]
		public void TestTransformToNewFrame(Vector a, Vector b, Vector expectedResult)
		{
			var actualResult = VectorTransformations3D.TransformToNewFrame(a, b);
			Assert.AreEqual(a.Length, b.Length);
			for (int i = 0; i < a.Length; i++)
			{
				Assert.AreEqual(expectedResult[i], actualResult[i], 0.00001, "Expected value: " + expectedResult.ToString() + ", actual value: " + actualResult.ToString());
			}
		}
		
		private static IEnumerable NewReferenceFrameCases()
		{
			var a = new Vector(1, 0, 0);
			var b = new Vector(1, 1, 1);
			var expected = new Vector(1, 1, 1);
			yield return new TestCaseData(a, b, expected);

			//If x-axis in the new frame becomes z-axis, then new vector's z value must be equal to old vector's x value.
			a = new Vector(1, 0, 0);
			b = new Vector(1, 0, 0);
			expected = new Vector(0, 0, 1);
			yield return new TestCaseData(a, b, expected);
			
			//X becomes z, z becomes x, y doesn't change
			a = new Vector(1, 0, 0);
			b = new Vector(1, 2, 3);
			expected = new Vector(2, 3, 1);
			yield return new TestCaseData(a, b, expected);

			//If y-axis in the new frame becomes z-axis, then new vector's x value remains unchanged
			a = new Vector(0, 1, 0);
			b = new Vector(1, 0, 0);
			expected = new Vector(1, 0, 0);
			yield return new TestCaseData(a, b, expected);

			a = new Vector(0, 1, 0);
			b = new Vector(-1, 0, 0);
			expected = new Vector(-1, 0, 0);
			yield return new TestCaseData(a, b, expected);

			a = new Vector(0, 1, 0);
			b = new Vector(-2, 3, 0);
			expected = new Vector(-2, 0, 3);
			yield return new TestCaseData(a, b, expected);

			a = new Vector(1, 1, 1);
			b = new Vector(2, 0, 0);
			expected = new Vector(1.6329931619, 0, 1.1547005384);
			yield return new TestCaseData(a, b, expected);
			
		}
	}
}