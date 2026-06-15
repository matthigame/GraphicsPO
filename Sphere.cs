using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Sphere : Primitive
    {
        public float radius;
        public bool checkers;

        public Sphere(Vector3 _position, float _radius, Color3 _color, Materials _material, bool _checkers) : base(_color, _position, PrimitiveTypes.Sphere, _material)
        {
            this.position = _position;
            this.radius = _radius;
            checkers = _checkers;
        }

        //calculates the intersection point closest the place the ray was fired from
        public Vector3 ClosestIntersectPoint(Ray ray) 
        {
            Intersection intersection = Intersect(ray);
            if (intersection.intersectCount == 1)
            {
                return intersection.intersectionPoints[0];
            }
            else if (intersection.intersectCount == 2) 
            {
                float Distance1 = Vector3.Distance(intersection.intersectionPoints[0], ray.startPosition);
                float Distance2 = Vector3.Distance(intersection.intersectionPoints[1], ray.startPosition);

                if (Distance1 <= Distance2) //return the intersection point where the distance to the starting point of the ray is the smallest
                    return intersection.intersectionPoints[0];
                else
                    return intersection.intersectionPoints[1];
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Color3 checkerBoards(Vector3 hitPoint, Vector3 normalVector, float distance)
        {
            float scale = 1f;

            Vector3 referencePoint = distance * -normalVector;
            base.position = referencePoint;
            Vector3 temp = referencePoint * normalVector;
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
