using System;
using System.Collections.Generic;
using System.Drawing;
using KinectProject.Geometry;
using Microsoft.Kinect;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Constants = KinectProject.Constants.Constants;
using Matrix4 = OpenTK.Matrix4;


namespace KinectProject.Windows
{
    public class DepthWindow : CustomWindow
    {
        private const bool DebugWithoutKinect = true;


        DepthImagePixel[] _depthPixels;
        private double[,] _depthMap;
        private List<CubePoint> _depthPoints;
        public static KinectSensor Sensor;
        private List<Data> _datas = new List<Data>();
        private Cube _fullCube;
        private Cube _scannedItem;
        private Cube _actualPreview;
        private int _kinectDepthImageHeight;
        private int _kinectDepthImageWidth;
        readonly int _widthSize = (int)Math.Ceiling(Constants.Constants.CubeWidth);
        readonly int _heightSize = (int)Math.Ceiling(Constants.Constants.CubeHeight);
        readonly int _depthSize = (int)Math.Ceiling(Constants.Constants.CubeDepth);

        public DepthWindow()
            : base(800, 600)
        {
            Load += LoadHandler;
            Resize += ResizeHandler;
            UpdateFrame += UpdateHandler;
            RenderFrame += RenderHandler;
            KeyUp += OnKeyUp;
            Context.SwapInterval = 1;

            _fullCube = new Cube
            {
                Center = new Vector3(
                    Constants.Constants.HalfCubeWidth, 
                    Constants.Constants.HalfCubeHeight, 
                    Constants.Constants.HalfCubeDepth),
                Vertices = new CubePoint[_widthSize, _heightSize, _depthSize]
            };
            for (var x = 0; x < _widthSize; x++)
            {
                for (var y = 0; y < _heightSize; y++)
                {
                    for (var z = 0; z < _depthSize; z++)
                    {
                        _fullCube.Vertices[x, y, z] = new CubePoint
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            Value = true,
                        };
                    }
                }
            }
        }

        //public event Program.SnapchotMade SnapshotMade;

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }

            if (e.Key == Key.Space)
            {
              //  MakePreview();
            }

            if (e.Key == Key.Enter)
            {
               // MakeSnapshot();
            }

            if (e.Key == Key.R)
            {
                ResetEye();
                ResetPreview();
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

        private void ResetPreview()
        {
            _actualPreview = null;
        }

        //private void MakeSnapshot()
        //{
        //    var data = new Data
        //    {
        //        DepthMap = _depthMap,
        //        Cube = _scannedItem ?? _fullCube
        //    };
        //    _datas.Add(data);
        //    _scannedItem = Data.ProcessData(_datas);
        //    _actualPreview = null;

        //    if (SnapshotMade != null)
        //        SnapshotMade(this, _datas, (Cube) _scannedItem.Clone());
        //}

        //private void MakePreview()
        //{
        //    var data = new Data
        //    {
        //        DepthMap = _depthMap,
        //        Cube = _scannedItem ?? _fullCube
        //    };
        //    var datasCopy = new List<Data>(_datas) {data};

        //    _actualPreview = Data.ProcessData(datasCopy);
        //}

        private void LoadHandler(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    Sensor = potentialSensor;
                    break;
                }
            }

            if (null != Sensor)
            {
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

            if (null == Sensor && !DebugWithoutKinect)
            {
                throw new Exception("No kinect connected");
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    
                    _depthPixels = new DepthImagePixel[Sensor.DepthStream.FramePixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(_depthPixels);
                }
            }
        }

        private void ResizeHandler(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            var aspectRatio = Width/(float) Height;
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 512);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
        }

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            if (_depthPixels == null) 
                return; 

            var depthPoints = new List<CubePoint>();
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

                    var cp = new CubePoint()
                    {
                        X = newX,
                        Y = newY,
                        Z = newZ,
                        Value = true
                    };
                    depthPoints.Add(cp);

                    UpdateDepthMapByPoint(depthMap, newX, newY, newZ);
                }
            }
            _depthPoints = depthPoints;
            _depthMap = depthMap;
        }

        private void UpdateDepthMapByPoint(double[,] newDepthMap, int newX, int newY, double newZ)
        {
            if (Extensions.InCubeWithoutDepth(newX, newY))
            {
                if (newZ > Constants.Constants.DistanceToCube + Constants.Constants.CubeDepth)
                    newDepthMap[newX, newY] = Constants.Constants.CubeDepth;

                if (newZ < Constants.Constants.DistanceToCube)
                    newDepthMap[newX, newY] = 0;
            }
            else if (!Extensions.InCube(newX, newY, newZ))
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

            if (_depthPoints != null)
            {
                GL.Color3(Color.White);
                GL.Begin(PrimitiveType.Points);
            
                for (var i = 0; i < _depthPoints.Count; i += Constants.Constants.Skip)
                {
                    if (_depthPoints[i] == null || (!_depthPoints[i].InCube() && !Constants.Constants.DrawDepthImageOutsideBox))
                        continue;
                    _depthPoints[i].DrawWithShift();
                }
                GL.End();
            }

            if (_actualPreview != null)
            {
                GL.Color3(Color.Red);
                _actualPreview.Draw();
            }
            
            if (_scannedItem != null)
            {
                GL.Color3(Color.Yellow);
                _scannedItem.Draw();
            }

            if (_fullCube != null && Constants.Constants.ShowFullCube)
            {
                GL.Color3(Color.DimGray);
                _fullCube.Draw();
            }

            Context.MakeCurrent(WindowInfo);
            Context.SwapBuffers();
        }
    }
}