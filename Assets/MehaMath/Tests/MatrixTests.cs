using System.Collections;
using MehaMath.Math.Components;
using NUnit.Framework;

namespace MehaMath.Tests
{
	public class MatrixTests
	{
		[Test]
		[TestCaseSource(nameof(MultiplicationCases))]
		public void MultiplicationTest(SquareMatrix a, SquareMatrix b, SquareMatrix expectedResult)
		{
			var actualResult = a * b;
			Assert.AreEqual(expectedResult.Size, actualResult.Size);
			for (int i = 0; i < a.Size; i++)
			{
				for (int j = 0; j < a.Size; j++)
				{
					Assert.AreEqual(expectedResult[i,j], actualResult[i,j], 0.000001);
				}
			}
		}
	
		private static IEnumerable MultiplicationCases()
		{
			var a = new SquareMatrix(new double[,]
			{
				{1, 2, 3},
				{4, 5, 6},
				{7, 8, 9}
			});
			var b = new SquareMatrix(new double[,]
			{
				{9, 8, 7},
				{6, 5, 3},
				{3, 2, 1}
			});
			var result = new SquareMatrix(new double[,]
			{
				{30, 24, 16},
				{84, 69, 49},
				{138, 114, 82}
			});
			yield return new TestCaseData(a, b, result);
			//Next test case is when we change the order of a and b. That is b goes first and a second.
			result = new SquareMatrix(new double[,]
			{
				{90, 114, 138},
				{47, 61, 75},
				{18, 24, 30}
			});
			yield return new TestCaseData(b, a, result);
		}

		[Test]
		[TestCaseSource(nameof(VectorMatrixMultiplicationCases))]
		public void VectorMatrixMultiplicationTest(Vector vector, SquareMatrix matrix, Vector expectedResult)
		{
			var actualResult = vector * matrix;
			Assert.AreEqual(actualResult.Length, expectedResult.Length);
			for (int i = 0; i < vector.Length; i++)
			{
				Assert.AreEqual(expectedResult[i], actualResult[i], 0.000001);
			}
		}

		private static IEnumerable VectorMatrixMultiplicationCases()
		{
			var vector = new Vector(1, 2, 3);
			var matrix = new SquareMatrix(new double[,]
			{
				{ 3, 2, 1 },
				{ 6, 5, 4 },
				{ 7, 8, 9 }
			});
			var expectedResult = new Vector(10, 28, 50);
			yield return new TestCaseData(vector, matrix, expectedResult);
		}
	}
	
	
}
