using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace INFOGRTemplate
{
    internal class ShadowRay: Ray
    {
        public ShadowRay(Vector3 _startingPoint, Vector3 _direction) : base(_startingPoint, _direction)
        {
        }
    }
}
