using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectProject.Geometry
{
    public class Data
    {
        public Cube Cube { get; set; }
        public double[,] DepthMap { get; set; }



        public static Cube ProcessData(List<Data> datas)
        {
            var first = datas.FirstOrDefault();
            if (first != null)
            {
                var firstCube = first.Cube;
                var xLen = firstCube.Vertices.GetLength(0);
                var yLen = firstCube.Vertices.GetLength(1);
                var zLen = firstCube.Vertices.GetLength(2);

                var result = new Cube
                {
                    Center = firstCube.Center,
                    Vertices = new CubePoint[xLen, yLen, zLen]
                };

                for (var x = 0; x < xLen; x++)
                {
                    for (var y = 0; y < yLen; y++)
                    {
                        for (var z = 0; z < zLen; z++)
                        {
                            result.Vertices[x, y, z] = (CubePoint)firstCube.Vertices[x, y, z].Clone();
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