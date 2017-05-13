using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using KinectProject.Geometry;
using KinectProject.Processor;
using KinectProject.Constants;

namespace KinectProject.Windows
{
    public class PresentationWindow : CustomWindow
    {
        private List<Data> _datas = null;
        private Cube _scannedItem = null;
        private bool newData = false;
        private Mesh _mesh = null;

        public PresentationWindow()
            : base(800, 600)
        {
    //        Resize += ResizeHandler;
        //    UpdateFrame += UpdateHandler;
         //   RenderFrame += RenderHandler;
            Context.SwapInterval = 1;
        }
        

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            if (newData)
            {
                var voxels = _scannedItem.Vertices.ToVoxels();
                MarchingCubes.SetModeToCubes();
                _mesh = MarchingCubes.CreateMesh(voxels);

                newData = false;
            }
        }

        //private void ResizeHandler(object sender, EventArgs e)
        //{
        //    GL.Viewport(ClientRectangle);

        //    var aspectRatio = Width / (float)Height;
        //    Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 512);
        //    GL.MatrixMode(MatrixMode.Projection);
        //    GL.LoadMatrix(ref Projection);
        //}

        private void RenderHandler(object sender, FrameEventArgs e)
        {
            Context.MakeCurrent(WindowInfo);
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            var lookat = Matrix4.LookAt(Eye, Target, Up);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            if (Constants.Constants.ShowBoxOnDepthWindow)
            {
                Extensions.DrawBox();
            }



            if (_mesh != null)
            {
                GL.Color3(Color.White);
                for (int i = 0; i < _mesh.Triangles.Length; i += 3)
                {
                    var v1 = _mesh.Vertices[_mesh.Triangles[i]];
                    var v2 = _mesh.Vertices[_mesh.Triangles[i + 1]];
                    var v3 = _mesh.Vertices[_mesh.Triangles[i + 2]];

                    Extensions.DrawTriangle(v1, v2, v3);
                }
            }

            if (_scannedItem != null)
            {
                GL.Color3(Color.Yellow);
                _scannedItem.Draw();
            }

            Context.MakeCurrent(WindowInfo);
            Context.SwapBuffers();
        }

        public void AddNewScanResult(List<Data> datas, Cube scannedItem)
        {
            _scannedItem = scannedItem;
            _datas = new List<Data>(datas);
            newData = true;
        }
    }
}