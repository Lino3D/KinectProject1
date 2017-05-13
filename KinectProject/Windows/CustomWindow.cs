using System;
using OpenTK;
using OpenTK.Input;

namespace KinectProject.Windows
{
    public class CustomWindow : GameWindow
    {
        protected Vector3 Eye = Constants.Constants.DefaultEyePosition;
        protected Vector3 Target = Constants.Constants.DefaultTargetPosition;
        protected Vector3 Up = Constants.Constants.DefaultUpVetor;
        private double _phi = Constants.Constants.DefaultPhiAngle;
        protected Matrix4 Projection;
        private double _radius = Constants.Constants.DefaultRadius;
        private double _theta = Constants.Constants.DefaultThetaAngle;

        public CustomWindow(int width, int height) : base(width, height)
        {
            UpdateFrame += OnUpdateFrame;
        }
        

        private void OnUpdateFrame(object sender, FrameEventArgs e)
        {
            Eye.X = (float) (Target.X + _radius*Math.Cos(_phi)*Math.Sin(_theta));
            Eye.Z = (float) (Target.Z + _radius*Math.Cos(_theta));
            Eye.Y = (float) (Target.Y + _radius*Math.Sin(_phi));
        }      
    }
}