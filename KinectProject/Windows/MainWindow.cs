using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private const float RotateAngleStep = 0.01f;
        private WindowStatus status = WindowStatus.ScanDataStage;
        public const double KinectFocalLength = 0.00173667;
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
        private List<ObjectInfo> _depthDataList = new List<ObjectInfo>();
        private Geometry.Rectangle _referenceRectangle;
        private Geometry.Rectangle _scannedObject;
        private Geometry.Rectangle _mainRectangle;
        private int _kinectDepthImageHeight;
        private int _kinectDepthImageWidth;
        private const int _rectSize = 120;

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

            _referenceRectangle = new Geometry.Rectangle
            {
                Center = new Vector3(
                    Constants.Constants.HalfRectWidth,
                    Constants.Constants.HalfRectHeight,
                    Constants.Constants.HalfRectDepth),
                Vertices = new DrawablePoint3D[_rectSize, _rectSize, _rectSize]
            };
            for (var x = 0; x < _rectSize; x++)
            {
                for (var y = 0; y < _rectSize; y++)
                {
                    for (var z = 0; z < _rectSize; z++)
                    {
                        _referenceRectangle.Vertices[x, y, z] = new DrawablePoint3D
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
            _theta += -dx * RotateAngleStep;
            _phi += dy * RotateAngleStep;
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

            if (e.Key == Key.Enter)
            {
                ScanObject();
            }
            
            if( e.Key == Key.C)
            {
                SwitchToVoxels();
            }

            if (e.Key == Key.Left)
            {
                RotateScannedObject(0, MathHelper.DegreesToRadians(20), 0);
            }

            if (e.Key == Key.Right)
            {
                RotateScannedObject(0, MathHelper.DegreesToRadians(20), 0);
            }

            if (e.Key == Key.Escape)
            {
                Exit();
            }
        }

        private void RotateScannedObject(float angleX, float angleY, float angleZ)
        {
            _referenceRectangle.Rotate(angleX, angleY, angleZ);
            _scannedObject?.Rotate(angleX, angleY, angleZ);
        }
        

        private void ResizeHandler(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            var aspectRatio = Width / (float)Height;
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 512);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
        }

        private void ScanObject()
        {
            var depthData = new ObjectInfo
            {
                DepthMap = _depthMap,
                ObjectGeometry = _scannedObject ?? _referenceRectangle
            };
            _depthDataList.Add(depthData);
            if (_depthDataList.Count == 1)
                _mainRectangle = _depthDataList.FirstOrDefault()?.ObjectGeometry;
            _scannedObject = ProcessData();

        }
        private Geometry.Rectangle ProcessData()
        {
 
            var lengthX = _mainRectangle.Vertices.GetLength(0);
            var lengthY = _mainRectangle.Vertices.GetLength(1);
            var lengthZ = _mainRectangle.Vertices.GetLength(2);

            var result = new Geometry.Rectangle
            {
                Center = _mainRectangle.Center,
                Vertices = new DrawablePoint3D[lengthX, lengthY, lengthZ]
            };

            for (var x = 0; x < lengthX; x++)
            {
                for (var y = 0; y < lengthY; y++)
                {
                    for (var z = 0; z < lengthZ; z++)
                    {
                        result.Vertices[x, y, z] = (DrawablePoint3D)_mainRectangle.Vertices[x, y, z].Clone();
                        result.Vertices[x, y, z].DrawPoint = true;
                    }
                }
            }

            foreach (var depthData in _depthDataList)
            {
                for (var x = 0; x < lengthX; x++)
                {
                    for (var y = 0; y < lengthY; y++)
                    {
                        for (var z = 0; z < lengthZ; z++)
                        {
                            var rectVertex = depthData.ObjectGeometry.Vertices[x, y, z];
                            if (!rectVertex.InRectDepth() || rectVertex.Z < depthData.DepthMap[(int)rectVertex.X, (int)rectVertex.Y])
                            {
                                result.Vertices[x, y, z].DrawPoint = false;
                            }
                        }
                    }
                }
            }
            return result;
        }


        private void OnLoad(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            Sensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            if (Sensor == null) return;
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Sensor.DepthFrameReady += SensorDepthFrameReady;
            _depthPixels = new DepthImagePixel[Sensor.DepthStream.FramePixelDataLength];
            _kinectDepthImageWidth = Sensor.DepthStream.FrameWidth;
            _kinectDepthImageHeight = Sensor.DepthStream.FrameHeight;

            try
            {
                Sensor.Start();
            }
            catch (IOException)
            {
                Sensor = null;
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null) return;
                depthFrame.CopyDepthImagePixelDataTo(this._depthPixels);
            }
        } 

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            if (status != WindowStatus.ScanDataStage)
                return;
            if (_depthPixels == null) 
                return; 

            var depthPoints = new List<DrawablePoint3D>();
            var depthMap = new double[_rectSize, _rectSize];
            CleanDepthMap(depthMap);

            for (var y = 0; y < _kinectDepthImageHeight; y++)
            {
                for (var x = 0; x < _kinectDepthImageWidth; x++)
                {
                    var offset = x+ y * _kinectDepthImageWidth;
                    var rawDepth = _depthPixels[offset].Depth;
                    if (Math.Abs(rawDepth) < 0.001) continue;

                    var newX = (int) ((x - 320) * KinectFocalLength * rawDepth / 10 + Constants.Constants.HalfRectWidth);
                    var newY = (int) ((-y + 240) * KinectFocalLength * rawDepth / 10 + Constants.Constants.HalfRectWidth);
                    double newZ = rawDepth / 10 - Constants.Constants.DistanceToRect;

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
            if (Helpers.Helpers.InRectNoDepth(newX, newY))
            {
                if (newZ > Constants.Constants.DistanceToRect + Constants.Constants.RectDepth)
                    newDepthMap[newX, newY] = Constants.Constants.RectDepth;

                if (newZ < Constants.Constants.DistanceToRect)
                    newDepthMap[newX, newY] = 0;
            }
            else if (!Helpers.Helpers.InRectDepth(newX, newY, newZ))
            {
                return;
            }
            if (newDepthMap[newX, newY] <= 0.001 || newDepthMap[newX, newY] >= (Constants.Constants.RectDepth - 0.001))
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
            for (var x = 0; x < _rectSize; x++)
            {
                for (var y = 0; y < _rectSize; y++)
                {
                    newDepthMap[x, y] = double.MaxValue;
                }
            }
        }

        private void RenderHandler(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.StencilBufferBit
                |ClearBufferMask.DepthBufferBit );
            var lookAtMatrix = Matrix4.LookAt(Eye, Target, Up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookAtMatrix);            
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
                default:
                    throw new ArgumentOutOfRangeException();
            }            
        }
        private void DrawModelDisplayStageObjects()
        {
            if (_mesh != null)
            {
                GL.Color3(Color.White);
                for (var i = 0; i < _mesh.Triangles.Length; i += 3)
                {
                    var v1 = _mesh.Vertices[_mesh.Triangles[i]];
                    var v2 = _mesh.Vertices[_mesh.Triangles[i + 1]];
                    var v3 = _mesh.Vertices[_mesh.Triangles[i + 2]];

                    Helpers.Helpers.DrawTriangle(v1, v2, v3);
                }
            }

            if (_scannedObject == null) return;
            GL.Color3(Color.Orange);
            _scannedObject.Draw();
        }

        private void DrawScanningStageObjects()
        {
            if (_depthPoints == null) return;
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Points);

            foreach (var point3D in _depthPoints)
            {
                point3D?.DrawWithShift();
            }
            GL.End();
        }

        private void SwitchToVoxels()
        {
            if( status == WindowStatus.DisplayModelStage)
            {
                status = WindowStatus.ScanDataStage;
            }
            if (status != WindowStatus.ScanDataStage) return;
            var voxels = _scannedObject.Vertices.ToVoxels();
            MarchingRects.SetModeToRects();
            _mesh = MarchingRects.CreateMesh(voxels);               
            status = WindowStatus.DisplayModelStage;
        }
    }
}