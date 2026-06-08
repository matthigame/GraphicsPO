using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool checkers;
        public Plane(Vector3 normal, float _distance, Color3 _color, Materials _material, bool _checkers) : base(_color, new Vector3(0, 0, 0), PrimitiveTypes.Plane, _material) 
        {
            normalVector = normal;
            this.distance = _distance;
            checkers = _checkers;

            Vector3 referencePoint = distance * -normalVector;
            base.position = referencePoint;
            Vector3 temp = referencePoint * normalVector;
            equationD = -(temp.X + temp.Y + temp.Z);
        }

        public Color3 checkerBoards(Vector3 hitPoint)
        {
            float scale = 1f;

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

            float vFac = Vector3.Dot(target, vVector);
            float uFac = Vector3.Dot(target, uVector);

            int U = (int)Math.Floor(uFac / scale);
            int V = (int)Math.Floor(vFac / scale);

            bool Tile = ((U + V) % 2) == 0;

            if (Tile)
                return new Color3(1f, 1f, 1f) - color;
            else
                return color;
        }
    }
}
