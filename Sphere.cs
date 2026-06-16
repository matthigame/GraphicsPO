using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            float scale = 10f;

            //calculate the radians
            float U_Rad = MathF.Acos(normalVector.Z);
            float V_Rad = MathF.Asin(normalVector.Y / MathF.Sin(U_Rad));

            //make the radians into angles
            float U_Angle = U_Rad * (180 / MathF.PI);
            float V_Angle = V_Rad * (180 / MathF.PI);

            //place them onto the grid
            int U = (int)Math.Floor(U_Angle / scale);
            int V = (int)Math.Floor(V_Angle / scale);

            // if the Tile is even, color it black (- the given color).
            bool Tile = ((U + V) % 2) == 0;

            if (Tile)
                return new Color3(1f, 1f, 1f) - color;
            else
                return color;
        }

    }
}
