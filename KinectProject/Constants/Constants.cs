using System;
using OpenTK;

namespace KinectProject.Constants
{
    public class Constants
    {
        #region Ustawienia kostki (wartoœci w cm)
        public const float CubeWidth = 120;
        public const float CubeHeight = 120;
        public const float CubeDepth = 120;
        public const float DistanceToCube = 80;
        #endregion

        #region Wyœwietlanie
        public const int Skip = 3;
        public const bool ShowPointsOutsideBox = false;
        #endregion


        #region Kamera i jej sterowanie

        public static readonly Vector3 DefaultEyePosition = new Vector3(0, 0, 0);
        public static readonly Vector3 DefaultTargetPosition = new Vector3(0f, 0f, DistanceToCube + HalfCubeDepth);
        public static readonly Vector3 DefaultUpVetor = new Vector3(0f, 1f, 0f);
        #endregion

        #region Poboczne
        public const float HalfCubeWidth = CubeWidth / 2;
        public const float HalfCubeHeight = CubeHeight / 2;
        public const float HalfCubeDepth = CubeDepth / 2;

        public static readonly Vector3[] RectCorners =
        {
            new Vector3(-HalfCubeWidth, -HalfCubeHeight, DistanceToCube),
            new Vector3(HalfCubeWidth, -HalfCubeHeight, DistanceToCube),
            new Vector3(HalfCubeWidth, HalfCubeHeight, DistanceToCube),
            new Vector3(-HalfCubeWidth, HalfCubeHeight, DistanceToCube),
            new Vector3(-HalfCubeWidth, -HalfCubeHeight, DistanceToCube + CubeDepth),
            new Vector3(HalfCubeWidth, -HalfCubeHeight, DistanceToCube + CubeDepth),
            new Vector3(HalfCubeWidth, HalfCubeHeight, DistanceToCube + CubeDepth),
            new Vector3(-HalfCubeWidth, HalfCubeHeight, DistanceToCube + CubeDepth)
        };



        #endregion

    }
}