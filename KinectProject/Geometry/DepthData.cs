using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectProject.Geometry
{
    public class DepthData
    {
        public Rectangle Cube { get; set; }
        public double[,] DepthMap { get; set; }

        public static Rectangle ProcessData(List<DepthData> datas)
        {
            var first = datas.FirstOrDefault();
            if (first != null)
            {
                var firstRectangle = first.Cube;
                var xLen = firstRectangle.Vertices.GetLength(0);
                var yLen = firstRectangle.Vertices.GetLength(1);
                var zLen = firstRectangle.Vertices.GetLength(2);

                var result = new Rectangle
                {
                    Center = firstRectangle.Center,
                    Vertices = new DrawablePoint3D[xLen, yLen, zLen]
                };

                for (var x = 0; x < xLen; x++)
                {
                    for (var y = 0; y < yLen; y++)
                    {
                        for (var z = 0; z < zLen; z++)
                        {
                            result.Vertices[x, y, z] = (DrawablePoint3D)firstRectangle.Vertices[x, y, z].Clone();
                            result.Vertices[x, y, z].Value = true;
                        }
                    }
                }

                foreach (var data in datas)
                {
                    for (var x = 0; x < xLen; x++)
                    {
                        for (var y = 0; y < yLen; y++)
                        {
                            for (var z = 0; z < zLen; z++)
                            {
                                var cubePoint = data.Cube.Vertices[x, y, z];
                                if (cubePoint.NotInCube() ||
                                    cubePoint.Z < data.DepthMap[(int)cubePoint.X, (int)cubePoint.Y])
                                {
                                    result.Vertices[x, y, z].Value = false;
                                }
                            }
                        }
                    }
                }
                return result;
            }
            return null;
        }
    }
}