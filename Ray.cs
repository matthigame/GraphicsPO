using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class Ray
    {
        public Vector3 startPosition, direction;
        public Ray(Vector3 _startPosition, Vector3 _direction) 
        { 
            startPosition = _startPosition;
            direction = Vector3.Normalize(_direction);
        }
    }
}
