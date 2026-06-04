using OpenTK.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class Camera
    {
        public Vector3 position = new Vector3(0, 0, 0), lookAtDirection = new Vector3(0, 0, 1), upDirection = new Vector3(0, 1, 0), rightDirection;
        public Vector3[] screenCorners = new Vector3[4];
        public float fov = 1;
        float aspectRatio = 1;

        public Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection)
        {
            this.position = position;
            this.lookAtDirection = lookAtDirection;
            this.upDirection = upDirection;
            rightDirection = Vector3.Cross(lookAtDirection, upDirection);
            Vector3 screenCenter = position + (fov * lookAtDirection);

            screenCorners[0] = screenCenter + upDirection - (aspectRatio * rightDirection);
            screenCorners[1] = screenCenter + upDirection + (aspectRatio * rightDirection);
            screenCorners[2] = screenCenter - upDirection - (aspectRatio * rightDirection);
            screenCorners[3] = screenCenter - upDirection + (aspectRatio * rightDirection);
        }
    }
}
