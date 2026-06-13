using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Triangle: Primitive
    {
        public Vector3 normalVector;
        public Vector3[] vertices;
        public Vector3 A, B, C;

        public Triangle(Color3 _color, Vector3[] _vertices, Materials _material) : base(_color, _vertices[0], PrimitiveTypes.Triangle, _material) 
        { 
            vertices = _vertices;
            A = vertices[0];
            B = vertices[1];
            C = vertices[2];
            normalVector = Vector3.Normalize(Vector3.Cross(B-A, C-A)); //compute the normal vector by taking the cross product of [B-A] and [C-A]   
            Debug.WriteLine(normalVector);
        }
    }
}
