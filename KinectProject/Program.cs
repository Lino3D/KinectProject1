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
        private static MainWindow _depthWindow;
        static void Main(string[] args)
        {
            using (var depthWindow = new MainWindow())
            {
                _depthWindow = depthWindow;               
                _depthWindow.Title = "Kinect Project";
                _depthWindow.Run(30.0, 0.0);
            }
        }
    }
}
