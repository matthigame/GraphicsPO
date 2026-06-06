using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Plane: Primitive
    {
        public Vector3 normalVector;
        float distance;
        public float equationD;
        public Plane(Vector3 normal, float _distance, Color3 _color, Materials _material) : base(_color, new Vector3(0, 0, 0), PrimitiveTypes.Plane, _material) 
        {
            normalVector = normal;
            this.distance = _distance;

            Vector3 referencePoint = distance * -normalVector;
            base.position = referencePoint;
            Vector3 temp = referencePoint * normalVector;
            equationD = -(temp.X + temp.Y + temp.Z);
        }

    }
}
