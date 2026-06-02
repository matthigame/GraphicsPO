using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class Camera
    {
        public Vector3 position = new Vector3(0, 0, 0), lookAtDirection = new Vector3(0, 0, 1), upDirection = new Vector3(0, 1, 0);
        public Vector3[] screenCorners = new Vector3[4];   

        public Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection, Vector3[] screenCorners)
        {
            this.position = position;
            this.lookAtDirection = lookAtDirection;
            this.upDirection = upDirection;
            this.screenCorners = screenCorners;
        }
    }
}
