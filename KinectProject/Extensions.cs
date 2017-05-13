using System.Drawing;
using KinectProject.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using KinectProject.Constants;

namespace KinectProject
{
    public static class Extensions
    {
        static Vector3 _shift = new Vector3( -Constants.HalfCubeWidth,
                        -Constants.HalfCubeHeight,
                        Constants.DistanceToCube);
 

        public static void DrawWithShift(this CubePoint cubePoint)
        {
            cubePoint.Shift(_shift).Draw();
        }

        public static void Draw(this CubePoint cubePoint)
        {
            GL.Vertex3(cubePoint.X, cubePoint.Y, cubePoint.Z);
        }

        private static CubePoint Shift(this CubePoint cubePoint, Vector3 shift)
        {
            return new CubePoint
            {
                X = cubePoint.X + shift.X,
                Y = cubePoint.Y + shift.Y,
                Z = cubePoint.Z + shift.Z,
                Value = cubePoint.Value,
            };
        }

        public static bool NotInCube(this CubePoint p)
        {
            return !p.InCube();
        }

        public static bool InCube(this CubePoint p)
        {
            return p.X >= 0 && p.X <= Constants.CubeWidth &&
                   p.Y >= 0 && p.Y <= Constants.CubeHeight &&
                   p.Z >= 0 && p.Z <= Constants.CubeDepth;
        }

        public static bool InCubeWithoutDepth(this CubePoint p)
        {
            return p.X >= 0 && p.X <= Constants.CubeWidth &&
                   p.Y >= 0 && p.Y <= Constants.CubeHeight;
        }

        public static bool InCube(double x, double y, double z)
        {
            return x >= 0 && x < Constants.CubeWidth &&
                   y >= 0 && y < Constants.CubeHeight &&
                   z >= 0 && z < Constants.CubeDepth;
        }

        public static bool InCubeWithoutDepth(int x, int y)
        {
            return x >= 0 && x < Constants.CubeWidth &&
                   y >= 0 && y < Constants.CubeHeight;
        }

        public static void DrawBox()
        {
            GL.Color3(Color.Red);

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Constants.CubeCorners[0]);
            GL.Vertex3(Constants.CubeCorners[1]);

            GL.Vertex3(Constants.CubeCorners[0]);
            GL.Vertex3(Constants.CubeCorners[3]);

            GL.Vertex3(Constants.CubeCorners[0]);
            GL.Vertex3(Constants.CubeCorners[4]);

            GL.Vertex3(Constants.CubeCorners[1]);
            GL.Vertex3(Constants.CubeCorners[2]);

            GL.Vertex3(Constants.CubeCorners[1]);
            GL.Vertex3(Constants.CubeCorners[5]);

            GL.Vertex3(Constants.CubeCorners[2]);
            GL.Vertex3(Constants.CubeCorners[3]);

            GL.Vertex3(Constants.CubeCorners[2]);
            GL.Vertex3(Constants.CubeCorners[6]);

            GL.Vertex3(Constants.CubeCorners[3]);
            GL.Vertex3(Constants.CubeCorners[7]);

            GL.Vertex3(Constants.CubeCorners[4]);
            GL.Vertex3(Constants.CubeCorners[5]);

            GL.Vertex3(Constants.CubeCorners[4]);
            GL.Vertex3(Constants.CubeCorners[7]);

            GL.Vertex3(Constants.CubeCorners[5]);
            GL.Vertex3(Constants.CubeCorners[6]);

            GL.Vertex3(Constants.CubeCorners[6]);
            GL.Vertex3(Constants.CubeCorners[7]);
            GL.End();
        }

        public static void Draw(this Cube actualCube, bool drawAll = false)
        {
            GL.Begin(PrimitiveType.Points);
            var xLen = actualCube.Vertices.GetLength(0);
            for (int x = 0; x < xLen; x++)
            {
                var yLen = actualCube.Vertices.GetLength(1);
                for (int y = 0; y < yLen; y++)
                {
                    var zLen = actualCube.Vertices.GetLength(2);
                    for (int z = 0; z < zLen; z++)
                    {
                        var vertex = actualCube.Vertices[x, y, z];
                        if (drawAll ||
                            (
                                (Constants.ShowPointsOutsideBox || vertex.InCube()) 
                                && vertex.Value 
                                && ShouldDrawThisVertex(x, y, z)
                            )
                        )
                            vertex.DrawWithShift();
                    }
                }
            }
            GL.End();
        }

        public static bool ShouldDrawThisVertex(int x, int y, int z)
        {
            return x % Constants.Skip == 0
                && y % Constants.Skip == 0
                && z % Constants.Skip == 0;
        }

        public static float[,,] ToVoxels(this CubePoint[,,] vertices)
        {
            var xLen = vertices.GetLength(0);
            var yLen = vertices.GetLength(1);
            var zLen = vertices.GetLength(2);
            float[,,] voxels = new float[xLen,yLen,zLen];
            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    for (int z = 0; z < zLen; z++)
                    {
                        voxels[x, y, z] = vertices[x, y, z].Value ? 1 : 0;
                    }
                }
            }
            return voxels;
        }

        public static void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(v1 + _shift);
            GL.Vertex3(v2 + _shift);
            GL.Vertex3(v3 + _shift);
            GL.End();
        }
    }
}