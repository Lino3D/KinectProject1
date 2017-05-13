using System;
using OpenTK;
using OpenTK.Input;

namespace KinectProject.Windows
{
    public class CustomWindow : GameWindow
    {
        protected Vector3 Eye = Configuration.DefaultEyePosition;
        protected Vector3 Target = Configuration.DefaultTargetPosition;
        protected Vector3 Up = Configuration.DefaultUpVetor;
        private bool _mouseCaptured;
        private double _phi = Configuration.DefaultPhiAngle;
        private int _prevX;
        private int _prevY;
        protected Matrix4 Projection;
        private double _radius = Configuration.DefaultRadius;
        private double _theta = Configuration.DefaultThetaAngle;

        public CustomWindow(int width, int height) : base(width, height)
        {
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;
            UpdateFrame += OnUpdateFrame;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _radius -= e.Delta*Configuration.WheelStep;
        }

        private void OnUpdateFrame(object sender, FrameEventArgs e)
        {
            Eye.X = (float) (Target.X + _radius*Math.Cos(_phi)*Math.Sin(_theta));
            Eye.Z = (float) (Target.Z + _radius*Math.Cos(_theta));
            Eye.Y = (float) (Target.Y + _radius*Math.Sin(_phi));
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (!_mouseCaptured)
                return;
            var dx = e.X - _prevX;
            var dy = e.Y - _prevY;
            _theta += -dx*Configuration.RotateAngleStep;
            _phi += dy*Configuration.RotateAngleStep;
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

        public void ResetEye()
        {
            _phi = Configuration.DefaultPhiAngle;
            _theta = Configuration.DefaultThetaAngle;
            _radius = Configuration.DefaultRadius;
        }
    }
}