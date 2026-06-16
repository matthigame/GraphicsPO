using OpenTK.Graphics.OpenGL;
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
        Refractive,
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
        public bool checkers;


        public float refraction_index = 1.52f; //An objects default refractiveness is that of glass (thus making the default refractive material glass)

        public Primitive(Color3 _color, Vector3 _position, PrimitiveTypes _type, Materials _material, bool checkers)
        {
            this.color = _color;
            this.position = _position;
            this.type = _type;
            material = _material;
            this.checkers = checkers;
        }

        public Intersection Intersect(Ray ray)
        {
            return new Intersection(this, ray);
        }

        //overridable method to prevent some checks in another part of the code
        public virtual Color3 checkerBoards(Vector3 hitPoint, Vector3 normalVector, float distance)
        {
            return new Color3(0, 0, 0); //default
        }



    }
}
