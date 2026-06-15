using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class PrimaryRay: Ray
    {
        public int bounces;
        public float sourceRefractIndex = 1; //assume we are starting in air;

        public PrimaryRay(Vector3 _startingPoint, Vector3 _direction, int _bounces) : base(_startingPoint, _direction)
        {
            bounces = _bounces;
        }
    }
}
