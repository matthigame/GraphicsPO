using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    enum Materials 
    { 
        Diffuse,
        Plastic,
        Metallic,
        Reflective,
    }

    enum PrimitiveTypes
    {
        Sphere,
        Plane,
        Triangle
    }

    internal class Primitive
    {
        public Color3 color;
        public Vector3 position;
        public PrimitiveTypes type;
        public Materials material;

        public Primitive(Color3 _color, Vector3 _position, PrimitiveTypes _type, Materials _material)
        {
            this.color = _color;
            this.position = _position;
            this.type = _type;
            material = _material;
        }

        public Intersection Intersect(Ray ray)
        {
            return new Intersection(this, ray);
        }

    }
}
