using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectProject.Windows;

namespace KinectProject
{
    class Program
    {
        private static DepthWindow _depthWindow;
        static void Main(string[] args)
        {
       
                using (var depthWindow = new DepthWindow())
                {
                    _depthWindow = depthWindow;
                    //_depthWindow.SnapshotMade += DepthWindowOnSnapshotMade;
                    _depthWindow.Title = "Kinect Scanner 3D - scanning";
                    _depthWindow.Run(30.0, 0.0);
                }
            

        }
    }
}
