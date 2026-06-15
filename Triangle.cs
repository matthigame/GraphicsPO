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
        public bool checkers;

        public Triangle(Color3 _color, Vector3[] _vertices, Materials _material, bool _checkers) : base(_color, _vertices[0], PrimitiveTypes.Triangle, _material) 
        { 
            vertices = _vertices;
            A = vertices[0];
            B = vertices[1];
            C = vertices[2];
            normalVector = Vector3.Normalize(Vector3.Cross(B-A, C-A)); //compute the normal vector by taking the cross product of [B-A] and [C-A]
            Debug.WriteLine(normalVector);
            checkers = _checkers;
        }

        public Color3 checkerBoards(Vector3 hitPoint)
        {
            float scale = 1f;

            Vector3 temp = A * normalVector;
            float equationD = -(temp.X + temp.Y + temp.Z);

            // Create a new point to find the plane directions.
            float X = position.X + 1;
            float Y = position.Y;
            float Z;
            if (normalVector.Z == 0)
                Z = position.Z;
            else
                Z = -(normalVector.X * X + normalVector.Y * Y + equationD) / normalVector.Z;

            Vector3 secPoint = new Vector3(X, Y, Z);
            Vector3 uVector = Vector3.Normalize(secPoint - position);
            Vector3 vVector = Vector3.Normalize(Vector3.Cross(uVector, normalVector));
            Vector3 target = hitPoint - position;

            // Obtain the U and V directions of the plane in their respective directions.
            float vFac = Vector3.Dot(target, vVector);
            float uFac = Vector3.Dot(target, uVector);

            int U = (int)Math.Floor(uFac / scale);
            int V = (int)Math.Floor(vFac / scale);

            // if the Tile is even, color it black (- the given color).
            bool Tile = ((U + V) % 2) == 0;

            if (Tile)
                return new Color3(1f, 1f, 1f) - color;
            else
                return color;
        }
    }
}
