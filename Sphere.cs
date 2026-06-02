using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Sphere : Primitive
    {
        Vector3 position;
        float radius;

        public Sphere(Vector3 position, float radius, Color3 color) : base(color)
        {
            this.position = position;
            this.radius = radius;
        }
    }
}
