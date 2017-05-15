using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using KinectProject.Geometry;
using KinectProject.Helpers;
using Microsoft.Kinect;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Constants = KinectProject.Constants.Constants;
using Matrix4 = OpenTK.Matrix4;
using KinectProject.Processor;

namespace KinectProject.Windows
{
    public class MainWindow : GameWindow
    {
        private const bool DebugWithoutKinect = true;

        private WindowStatus status = WindowStatus.ScanDataStage;

        protected Vector3 Eye = new Vector3(0, 0, 0);
        protected Vector3 Target = Constants.Constants.DefaultTargetPosition;
        protected Vector3 Up = new Vector3(0f, 1f, 0f);
        private double _phi = Math.PI / 8;
        protected Matrix4 Projection;
        private double _radius = 160;
        private double _theta = Math.PI;
        private bool _mouseCaptured;        
        private int _prevX;
        private int _prevY;

        private Mesh _mesh;

        DepthImagePixel[] _depthPixels;
        private double[,] _depthMap;
        private List<DrawablePoint3D> _depthPoints;
        public static KinectSensor Sensor;
        private List<DepthData> _datas = new List<DepthData>();
        private Geometry.Rectangle _fullCube;
        private Geometry.Rectangle _scannedItem;
        private Geometry.Rectangle _actualPreview;
        private int _kinectDepthImageHeight;
        private int _kinectDepthImageWidth;
        readonly int _widthSize =  120 ;
        readonly int _heightSize = 120;
        readonly int _depthSize = 120;

        public MainWindow()
            : base(800, 600)
        {
            this.Load += OnLoad;            
            Resize += ResizeHandler;
            this.UpdateFrame += UpdateHandler;
            this.RenderFrame += RenderHandler;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseMove += OnMouseMove;
            this.KeyUp += OnKeyUp;
            this.Context.SwapInterval = 1;

            _fullCube = new Geometry.Rectangle
            {
                Center = new Vector3(
                    Constants.Constants.HalfCubeWidth,
                    Constants.Constants.HalfCubeHeight,
                    Constants.Constants.HalfCubeDepth),
                Vertices = new DrawablePoint3D[_widthSize, _heightSize, _depthSize]
            };
            for (var x = 0; x < _widthSize; x++)
            {
                for (var y = 0; y < _heightSize; y++)
                {
                    for (var z = 0; z < _depthSize; z++)
                    {
                        _fullCube.Vertices[x, y, z] = new DrawablePoint3D
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            DrawPoint = true,
                        };
                    }
                }
            }
        }
        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (!_mouseCaptured)
                return;
            var dx = e.X - _prevX;
            var dy = e.Y - _prevY;
            _theta += -dx * Constants.Constants.RotateAngleStep;
            _phi += dy * Constants.Constants.RotateAngleStep;
            _prevX = e.X;
            _prevY = e.Y;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left)
                return;
            _mouseCaptured = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left)
                return;
            _mouseCaptured = true;
            _prevX = e.X;
            _prevY = e.Y;
        }
        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }

            if (e.Key == Key.Space)
            {
                MakePreview();
            }

            if (e.Key == Key.Enter)
            {
                MakeSnapshot();
            }
            
            if( e.Key == Key.C)
            {
                SwitchDisplayModel();
            }

            if (e.Key == Key.Left)
            {
                RotateItem(0, -Constants.Constants.ItemAngleStep, 0);
            }

            if (e.Key == Key.Right)
            {
                RotateItem(0, Constants.Constants.ItemAngleStep, 0);
            }
        }

        private void RotateItem(float angleX, float angleY, float angleZ)
        {
            _fullCube.Rotate(angleX, angleY, angleZ);
            if (_scannedItem != null)
                _scannedItem.Rotate(angleX, angleY, angleZ);
        }

        //private void ResetPreview()
        //{
        //    _actualPreview = null;
        //}

        private void ResizeHandler(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            var aspectRatio = Width / (float)Height;
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 512);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
        }

        private void MakeSnapshot()
        {
            var data = new DepthData
            {
                DepthMap = _depthMap,
                Rect = _scannedItem ?? _fullCube
            };
            _datas.Add(data);
            _scannedItem = DepthData.ProcessData(_datas);
            _actualPreview = null;

         //   if (SnapshotMade != null)
         //       SnapshotMade(this, _datas, (Rect)_scannedItem.Clone());
        }

        private void MakePreview()
        {
            var data = new DepthData
            {
                DepthMap = _depthMap,
                Rect = _scannedItem ?? _fullCube
            };
            var datasCopy = new List<DepthData>(_datas) { data };

            _actualPreview = DepthData.ProcessData(datasCopy);
        }

        private void Create3DMesh()
        {
            var voxels = _scannedItem.Vertices.ToVoxels();
            MarchingCubes.SetModeToCubes();
            _mesh = MarchingCubes.CreateMesh(voxels);

        }

        private void OnLoad(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);

            Sensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

            if (Sensor == null) return;
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Sensor.DepthFrameReady += SensorDepthFrameReady;

            _kinectDepthImageWidth = Sensor.DepthStream.FrameWidth;
            _kinectDepthImageHeight = Sensor.DepthStream.FrameHeight;

            try
            {
                Sensor.Start();
            }
            catch (Exception)
            {
                Sensor = null;
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null) return;
                _depthPixels = new DepthImagePixel[Sensor.DepthStream.FramePixelDataLength];
                depthFrame.CopyDepthImagePixelDataTo(_depthPixels);
            }
        } 

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            if (status != WindowStatus.ScanDataStage)
                return;
            if (_depthPixels == null) 
                return; 

            var depthPoints = new List<DrawablePoint3D>();
            var depthMap = new double[_widthSize, _heightSize];
            CleanDepthMap(depthMap);

            for (var origY = 0; origY < _kinectDepthImageHeight; origY++)
            {
                for (var origX = 0; origX < _kinectDepthImageWidth; origX++)
                {
                    var i = origY * _kinectDepthImageWidth + origX;
                    
                    double z = _depthPixels[i].Depth;
                    if (Math.Abs(z) < 10e-3) continue;

                    int newX = (int) ((origX - 320) * Constants.Constants.FocalLength * z / 10 + Constants.Constants.HalfCubeWidth);
                    int newY = (int) ((-origY + 240) * Constants.Constants.FocalLength * z / 10 + Constants.Constants.HalfCubeWidth);
                    double newZ = z / 10 - Constants.Constants.DistanceToCube;

                    var cp = new DrawablePoint3D()
                    {
                        X = newX,
                        Y = newY,
                        Z = newZ,
                        DrawPoint = true
                    };
                    depthPoints.Add(cp);

                    UpdateDepthMapByPoint(depthMap, newX, newY, newZ);
                }
            }
            _depthPoints = depthPoints;
            _depthMap = depthMap;


            // Update camera parameters
            Eye.X = (float)(Target.X + _radius * Math.Cos(_phi) * Math.Sin(_theta));
            Eye.Z = (float)(Target.Z + _radius * Math.Cos(_theta));
            Eye.Y = (float)(Target.Y + _radius * Math.Sin(_phi));
        }

        private void UpdateDepthMapByPoint(double[,] newDepthMap, int newX, int newY, double newZ)
        {
            if (Helpers.Helpers.InCubeWithoutDepth(newX, newY))
            {
                if (newZ > Constants.Constants.DistanceToCube + Constants.Constants.CubeDepth)
                    newDepthMap[newX, newY] = Constants.Constants.CubeDepth;

                if (newZ < Constants.Constants.DistanceToCube)
                    newDepthMap[newX, newY] = 0;
            }
            else if (!Helpers.Helpers.InCube(newX, newY, newZ))
            {
                return;
            }

            if (newDepthMap[newX, newY] <= 10e-3 || newDepthMap[newX, newY] >= (Constants.Constants.CubeDepth - 10e-3))
            {
                newDepthMap[newX, newY] = newZ;
            }
            else
            {
                newDepthMap[newX, newY] += newZ;
                newDepthMap[newX, newY] /= 2;
            }
        }

        private void CleanDepthMap(double[,] newDepthMap)
        {
            for (var x = 0; x < _widthSize; x++)
            {
                for (var y = 0; y < _heightSize; y++)
                {
                    newDepthMap[x, y] = double.MaxValue;
                }
            }
        }

        private void RenderHandler(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            var lookat = Matrix4.LookAt(Eye, Target, Up);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            Helpers.Helpers.DrawBox();
            DrawObjectsByStage();
            Context.SwapBuffers();
        }

        private void DrawObjectsByStage()
        {
            switch( status)
            {
                case WindowStatus.ScanDataStage:
                    DrawScanningStageObjects();
                    break;
                case WindowStatus.DisplayModelStage:
                    DrawModelDisplayStageObjects();
                    break;
            }            
        }
        private void DrawModelDisplayStageObjects()
        {
            if (_mesh != null)
            {
                GL.Color3(Color.White);
                for (int i = 0; i < _mesh.Triangles.Length; i += 3)
                {
                    var v1 = _mesh.Vertices[_mesh.Triangles[i]];
                    var v2 = _mesh.Vertices[_mesh.Triangles[i + 1]];
                    var v3 = _mesh.Vertices[_mesh.Triangles[i + 2]];

                    Helpers.Helpers.DrawTriangle(v1, v2, v3);
                }
            }

            if (_scannedItem == null) return;
            GL.Color3(Color.Orange);
            _scannedItem.Draw();
        }

        private void DrawScanningStageObjects()
        {
            if (_depthPoints != null)
            {
                GL.Color3(Color.White);
                GL.Begin(PrimitiveType.Points);

                for (var i = 0; i < _depthPoints.Count; i += Constants.Constants.Skip)
                {
                    if (_depthPoints[i] == null) // || (!_depthPoints[i].InCube() && !Constants.Constants.DrawDepthImageOutsideBox))
                        continue;
                    _depthPoints[i].DrawWithShift();
                }
                GL.End();
            }

            if (_actualPreview != null)
            {
                GL.Color3(Color.Green);
                _actualPreview.Draw();
            }

            if (_scannedItem == null) return;
            GL.Color3(Color.Blue);
            _scannedItem.Draw();
        }

        private void SwitchDisplayModel()
        {
            if( status == WindowStatus.DisplayModelStage)
            {
                status = WindowStatus.ScanDataStage;
            }
            if( status == WindowStatus.ScanDataStage)
            {
                var voxels = _scannedItem.Vertices.ToVoxels();
                MarchingCubes.SetModeToCubes();
                _mesh = MarchingCubes.CreateMesh(voxels);               

                status = WindowStatus.DisplayModelStage;
            }
        }
    }
}