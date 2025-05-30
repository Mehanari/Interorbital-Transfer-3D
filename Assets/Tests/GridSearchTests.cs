using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Src.OptimizationFramework.MathComponents;
using Vector = MehaMath.Math.Components.Vector;

public class GridSearchTests
{
    [Test]
    [TestCaseSource(nameof(GridTestCases))]
    public void TestGridGeneration(Vector zeroPoint, Vector difference, int pointsPerDimension, Vector[] expectedGrid)
    {
        var actualGrid = GridSearcher.GenerateGrid(zeroPoint, difference, pointsPerDimension);
        Assert.AreEqual(expectedGrid.Length, actualGrid.Length);

        for (int i = 0; i < actualGrid.Length; i++)
        {
            var actualPoint = actualGrid[i];
            var expectedPoint = expectedGrid[i];
            Assert.AreEqual(expectedPoint.Length, actualPoint.Length);
            for (int j = 0; j < actualPoint.Length; j++)
            {
                Assert.AreEqual(expectedPoint[j], actualPoint[j], 1e-6);
            }
        }
    }
    
    private static IEnumerable GridTestCases()
    {
        var zeroPoint = new Vector(0, 0);
        var difference = new Vector(1, 1);
        var pointsPerDimension = 3;
        var expectedGrid = new Vector[]
        {
            new(0, 0), new(1, 0), new Vector(2, 0),
            new(0, 1), new(0, 2), new(1, 1),
            new(1, 2), new(2, 1), new(2, 2)
        };
        yield return new TestCaseData(zeroPoint, difference, pointsPerDimension, expectedGrid);
        
        // Test case 1: 2x2 grid
        yield return new TestCaseData(
            new Vector(0, 0),
            new Vector(1, 1), 
            2,
            new Vector[]
            {
                new(0, 0), new(1, 0),
                new(0, 1), new(1, 1)
            });
        
        // Test case 3: Non-uniform differences
        yield return new TestCaseData(
            new Vector(0, 0),
            new Vector(0.5, 2.0),
            3,
            new Vector[]
            {
                new(0, 0), new(0.5, 0), new(1.0, 0),
                new(0, 2.0), new(0, 4.0), new(0.5, 2.0),
                new(0.5, 4.0), new(1.0, 2.0), new(1.0, 4.0)
            });
        
        // Test case 4: 3D grid (2x2x2)
        yield return new TestCaseData(
            new Vector(0, 0, 0),
            new Vector(1, 1, 1),
            2,
            new Vector[]
            {
                new(0, 0, 0), new(1, 0, 0),
                new(0, 1, 0), new(1, 1, 0),
                new(0, 0, 1), new(1, 0, 1),
                new(0, 1, 1), new(1, 1, 1)
            });
        
        // Test case 5: Single point (edge case)
        yield return new TestCaseData(
            new Vector(3, 4),
            new Vector(1, 1),
            1,
            new Vector[]
            {
                new(3, 4)
            });
        
        // Test case 6: Negative starting point
        yield return new TestCaseData(
            new Vector(-2, -3),
            new Vector(1, 1),
            2,
            new Vector[]
            {
                new(-2, -3), new(-1, -3),
                new(-2, -2), new(-1, -2)
            });
    }
    
    [Test]
    [TestCaseSource(nameof(GridAroundTestCases))]
    public void TestGenerateGridAround(Vector center, Vector difference, int pointsPerDimension, Vector[] expectedGrid)
    {
        var actualGrid = GridSearcher.GenerateGridAround(center, difference, pointsPerDimension);
        Assert.AreEqual(expectedGrid.Length, actualGrid.Length);

        for (int i = 0; i < actualGrid.Length; i++)
        {
            var actualPoint = actualGrid[i];
            var expectedPoint = expectedGrid[i];
            Assert.AreEqual(expectedPoint.Length, actualPoint.Length);
            for (int j = 0; j < actualPoint.Length; j++)
            {
                Assert.AreEqual(expectedPoint[j], actualPoint[j], 1e-6);
            }
        }
    }

    private static IEnumerable GridAroundTestCases()
    {
        var center = new Vector(0, 0, 0, 0);
        var diff = new Vector(1, 1, 1, 1);
        var pointsPerDirection = 1;
        var expectedGrid = new Vector[]
        {
            new(0, 0, 0, 0), new(1, 0, 0, 0), new(-1, 0, 0, 0),
            new(0, 1, 0, 0), new(1, 1, 0, 0), new(-1, 1, 0, 0),
            new(0, -1, 0, 0), new(1, -1, 0, 0), new(-1, -1, 0, 0),

            new(0, 0, 1, 0), new(1, 0, 1, 0), new(-1, 0, 1, 0),
            new(0, 1, 1, 0), new(1, 1, 1, 0), new(-1, 1, 1, 0),
            new(0, -1, 1, 0), new(1, -1, 1, 0), new(-1, -1, 1, 0),

            new(0, 0, -1, 0), new(1, 0, -1, 0), new(-1, 0, -1, 0),
            new(0, 1, -1, 0), new(1, 1, -1, 0), new(-1, 1, -1, 0),
            new(0, -1, -1, 0), new(1, -1, -1, 0), new(-1, -1, -1, 0),



            new(0, 0, 0, 1), new(1, 0, 0, 1), new(-1, 0, 0, 1),
            new(0, 1, 0, 1), new(1, 1, 0, 1), new(-1, 1, 0, 1),
            new(0, -1, 0, 1), new(1, -1, 0, 1), new(-1, -1, 0, 1),

            new(0, 0, 1, 1), new(1, 0, 1, 1), new(-1, 0, 1, 1),
            new(0, 1, 1, 1), new(1, 1, 1, 1), new(-1, 1, 1, 1),
            new(0, -1, 1, 1), new(1, -1, 1, 1), new(-1, -1, 1, 1),

            new(0, 0, -1, 1), new(1, 0, -1, 1), new(-1, 0, -1, 1),
            new(0, 1, -1, 1), new(1, 1, -1, 1), new(-1, 1, -1, 1),
            new(0, -1, -1, 1), new(1, -1, -1, 1), new(-1, -1, -1, 1),



            new(0, 0, 0, -1), new(1, 0, 0, -1), new(-1, 0, 0, -1),
            new(0, 1, 0, -1), new(1, 1, 0, -1), new(-1, 1, 0, -1),
            new(0, -1, 0, -1), new(1, -1, 0, -1), new(-1, -1, 0, -1),

            new(0, 0, 1, -1), new(1, 0, 1, -1), new(-1, 0, 1, -1),
            new(0, 1, 1, -1), new(1, 1, 1, -1), new(-1, 1, 1, -1),
            new(0, -1, 1, -1), new(1, -1, 1, -1), new(-1, -1, 1, -1),

            new(0, 0, -1, -1), new(1, 0, -1, -1), new(-1, 0, -1, -1),
            new(0, 1, -1, -1), new(1, 1, -1, -1), new(-1, 1, -1, -1),
            new(0, -1, -1, -1), new(1, -1, -1, -1), new(-1, -1, -1, -1),
        };
        yield return new TestCaseData(center, diff, pointsPerDirection, expectedGrid);
    }
}
