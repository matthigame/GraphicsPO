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

        public Sphere(Vector3 position, float radius, Color3 color) : base(color, position, "Sphere")
        {
            this.position = position;
            this.radius = radius;
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

    }
}
