using UnityEngine;
using UnityEngine.Rendering;

namespace MehaMath.VisualisationTools
{
    public static class MeshMaker
    {
        public static Mesh GetPlaneMesh(float length, float width, int xDots, int yDots, Color vertexColor)
        {
            var vertices = new Vector3[xDots * yDots];
            var triangles = new int[((xDots - 1) * (yDots - 1) * 6)*2];
            var colors = new Color[xDots * yDots];
            var xStep = length / (xDots - 1);
            var yStep = width / (yDots - 1);
            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    var index = x * yDots + y;
                    vertices[index] = new Vector3(x * xStep, 0, y * yStep);
                    colors[index] = Color.white;
                }
            }

            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    if (y == yDots - 1 || x == xDots - 1)
                    {
                        continue;
                    }
                    //Making lower left triangle
                    var lowerLeftIndex = x * yDots + y;
                    var upperLeftIndex = lowerLeftIndex + yDots;
                    var lowerRightIndex = lowerLeftIndex + 1;
                    triangles[(x * (yDots - 1) + y) * 6] = lowerLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 1] = upperLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 2] = lowerRightIndex;
                    //Making upper right triangle
                    var upperRightIndex = upperLeftIndex + 1;
                    triangles[(x * (yDots - 1) + y) * 6 + 3] = lowerRightIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 4] = upperLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 5] = upperRightIndex;
                }
            }
            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    if (y == yDots - 1 || x == xDots - 1)
                    {
                        continue;
                    }
                    //Making lower left triangle
                    var lowerLeftIndex = x * yDots + y;
                    var upperLeftIndex = lowerLeftIndex + yDots;
                    var lowerRightIndex = lowerLeftIndex + 1;
                    triangles[(x * (yDots - 1) + y) * 6 ] = lowerRightIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 1] = upperLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 2] = lowerLeftIndex;
                    //Making upper right triangle
                    var upperRightIndex = upperLeftIndex + 1;
                    triangles[(x * (yDots - 1) + y) * 6 + 3] = lowerRightIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 4] = upperLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 5] = upperRightIndex;
                }
            }

            var mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };
            mesh.name = "Custom mesh";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            return mesh;
        }
    }
}