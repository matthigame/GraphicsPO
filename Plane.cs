using Assimp;
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
        public Plane(Vector3 _normal, float _distance, Color3 _color, Materials _material, bool _checkers) : base(_color, new Vector3(0, 0, 0), PrimitiveTypes.Plane, _material, _checkers) 
        {
            normalVector = _normal;
            this.distance = _distance;

            Vector3 referencePoint = distance * -normalVector;
            base.position = referencePoint;
            Vector3 temp = referencePoint * normalVector;
            equationD = -(temp.X + temp.Y + temp.Z);
        }

        //alternative constructor where you can input a reference point instead of a distance
        public Plane(Vector3 _normal, Vector3 _referencePoint, Color3 _color, Materials _material, bool _checkers) : base(_color, _referencePoint, PrimitiveTypes.Plane, _material, _checkers)
        {
            normalVector = _normal;
            Vector3 temp = _referencePoint * normalVector;
            equationD = -(temp.X + temp.Y + temp.Z);
        }

        public (Vector3, int) intersectionPoint(Vector3 source, Vector3 direction)
        {
            direction = Vector3.Normalize(direction);
            Vector3 vector1 = normalVector * source;
            float denominator = Vector3.Dot(normalVector, direction);
            Vector3 vector2 = normalVector * direction;
            float numerator = Vector3.Dot(normalVector, source) + equationD;


            float lambda = -numerator / denominator;


            if (numerator == 0 && denominator != 0) //no intersections
            {
                return (default, 0);
            }
            else if (MathF.Abs(denominator) < 0.00001f) //ray lies in the plane: infinite intersection points
            {
                return (source, 1); //the source is the closest intersection point, and we only care about this point, so we act like we have only 1 intersection
            }
            else if (lambda <= 0) //looking backwards
            {
                return (default, 0); //the returned Vector should be ignored since the intersection count is 0
            }
            else
            {
                Vector3 intersect = source + lambda * direction; //we have found our intersect point. Yay!
                //Debug.WriteLine($"lambda: {lambda}, source: {source}, direction: {direction}");
                return (intersect, 1);
            }

        }

        public override Color3 checkerBoards(Vector3 hitPoint, Vector3 normalVector, float distance)
        {
            float scale = 1f;

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
