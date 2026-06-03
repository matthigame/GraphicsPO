using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Primitive
    {
        public Color3 color;
        public Vector3 position;
        public string typeID;

        public Primitive(Color3 _color, Vector3 _position, string _typeID)
        {
            this.color = _color;
            this.position = _position;
            this.typeID = _typeID;
        }

        public Intersection Intersect(Ray ray)
        {
            return new Intersection(this, ray);
        }

    }
}
