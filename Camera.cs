using Assimp;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Camera
    {
        public Vector3 position = new Vector3(0, 0, 0), lookAtDirection = new Vector3(0, 0, 1), upDirection = new Vector3(0, 1, 0), rightDirection;
        public Vector3[] screenCorners = new Vector3[4];
        public float fov = 1, angleX = 0, angleY = 0;
        float aspectRatio = 1;

        public Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection, Surface screen)
        {
            this.position = position;
            this.lookAtDirection = Vector3.Normalize(lookAtDirection);
            this.upDirection = Vector3.Normalize(upDirection);
            rightDirection = Vector3.Normalize(Vector3.Cross(upDirection, lookAtDirection));
            aspectRatio = (float)screen.width / screen.height;
            UpdateCamera();
            
        }

        public void UpdateCamera()
        {
            float radianX = angleX * (MathF.PI / 180f);
            float radianY = angleY * (MathF.PI / 180f);

            lookAtDirection.X = MathF.Sin(radianY) * MathF.Cos(radianX);
            lookAtDirection.Y = MathF.Sin(radianX );
            lookAtDirection.Z = MathF.Cos(radianY) * MathF.Cos(radianX);
            lookAtDirection = Vector3.Normalize(lookAtDirection);

            rightDirection = Vector3.Normalize(Vector3.Cross(lookAtDirection, -Vector3.UnitY));

            upDirection = Vector3.Normalize(Vector3.Cross(lookAtDirection, rightDirection));

            //updating the screen
            Vector3 screenCenter = position + (fov * lookAtDirection);
            screenCorners[0] = screenCenter + upDirection - (aspectRatio * rightDirection);
            screenCorners[1] = screenCenter + upDirection + (aspectRatio * rightDirection);
            screenCorners[2] = screenCenter - upDirection - (aspectRatio * rightDirection);
            screenCorners[3] = screenCenter - upDirection + (aspectRatio * rightDirection);

        }
    }
}
