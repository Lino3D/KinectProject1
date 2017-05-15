using System.Drawing;
using KinectProject.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace KinectProject.Helpers
{
    public static class Helpers
    {
        public static readonly Vector3 Shift = new Vector3( -Constants.Constants.HalfCubeWidth,
                        -Constants.Constants.HalfCubeHeight,
                        Constants.Constants.DistanceToCube);
 

        public static void DrawWithShift(this DrawablePoint3D drawablePoint3D)
        {
            drawablePoint3D.DoShift(Shift).Draw();
        }

        public static void Draw(this DrawablePoint3D drawablePoint3D)
        {
            GL.Vertex3(drawablePoint3D.X, drawablePoint3D.Y, drawablePoint3D.Z);
        }

        private static DrawablePoint3D DoShift(this DrawablePoint3D drawablePoint3D, Vector3 shift)
        {
            return new DrawablePoint3D
            {
                X = drawablePoint3D.X + shift.X,
                Y = drawablePoint3D.Y + shift.Y,
                Z = drawablePoint3D.Z + shift.Z,
                DrawPoint = drawablePoint3D.DrawPoint,
            };
        }
        

        public static bool InCube(this DrawablePoint3D p)
        {
            return p.X >= 0 && p.X <=  Constants.Constants.CubeWidth &&
                   p.Y >= 0 && p.Y <= Constants.Constants.CubeHeight &&
                   p.Z >= 0 && p.Z <= Constants.Constants.CubeDepth;
        }

        public static bool InCubeWithoutDepth(this DrawablePoint3D p)
        {
            return p.X >= 0 && p.X <= Constants.Constants.CubeWidth &&
                   p.Y >= 0 && p.Y <= Constants.Constants.CubeHeight;
        }

        public static bool InCube(double x, double y, double z)
        {
            return x >= 0 && x < Constants.Constants.CubeWidth &&
                   y >= 0 && y < Constants.Constants.CubeHeight &&
                   z >= 0 && z < Constants.Constants.CubeDepth;
        }

        public static bool InCubeWithoutDepth(int x, int y)
        {
            return x >= 0 && x < Constants.Constants.CubeWidth &&
                   y >= 0 && y < Constants.Constants.CubeHeight;
        }

       

        public static void Draw(this Geometry.Rectangle actualCube, bool drawAll = false)
        {
            GL.Begin(PrimitiveType.Points);
            var lengthX = actualCube.Vertices.GetLength(0);
            for (var x = 0; x < lengthX; x++)
            {
                var lengthY = actualCube.Vertices.GetLength(1);
                for (var y = 0; y < lengthY; y++)
                {
                    var lengthZ = actualCube.Vertices.GetLength(2);
                    for (var z = 0; z < lengthZ; z++)
                    {
                        var vertex = actualCube.Vertices[x, y, z];
                        if (drawAll ||
                            (
                                (vertex.InCube()) 
                                && vertex.DrawPoint 
                            )
                        )
                            vertex.DrawWithShift();
                    }
                }
            }
            GL.End();
        }

        
        public static float[,,] ToVoxels(this DrawablePoint3D[,,] vertices)
        {
            var lengthX = vertices.GetLength(0);
            var lengthY = vertices.GetLength(1);
            var lengthZ = vertices.GetLength(2);
            var voxels = new float[lengthX,lengthY,lengthZ];
            for (var x = 0; x < lengthX; x++)
            {
                for (var y = 0; y < lengthY; y++)
                {
                    for (var z = 0; z < lengthZ; z++)
                    {
                        voxels[x, y, z] = vertices[x, y, z].DrawPoint ? 1 : 0;
                    }
                }
            }
            return voxels;
        }

        public static void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(v1 + Shift);
            GL.Vertex3(v2 + Shift);
            GL.Vertex3(v3 + Shift);
            GL.End();
        }
    }
}