using Assimp;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class Intersection
    {
        public Vector3[] intersectionPoints;
        public int intersectCount;
        public bool intersects;
        public Vector3 closestIntersect;
        public Primitive primitive;
        public float distance;
        public Intersection(Primitive _primitive, Ray _ray)
        {
            intersectionPoints = new Vector3[2];
            this.primitive = _primitive;
            switch (_primitive.typeID)
            {
                case "Sphere":
                    SphereIntersect(_primitive as Sphere, _ray);
                    break;

                case "Plane":
                    PlaneIntersect(_primitive, _ray);
                    break;
            }
            if (intersects)
                distance = Vector3.Distance(closestIntersect, _ray.startPosition);
        }

        private void SphereIntersect(Sphere sphere, Ray ray)
        { 
            //quadratic equation to figure out the scalar
            float a = (ray.direction.X* ray.direction.X) + (ray.direction.Y* ray.direction.Y) + (ray.direction.Z* ray.direction.Z);
            float b = 2 * (ray.direction.X * (ray.startPosition.X - sphere.position.X) +    
                           ray.direction.Y * (ray.startPosition.Y - sphere.position.Y)+
                           ray.direction.Z * (ray.startPosition.Z - sphere.position.Z));
            float c = (ray.startPosition.X - sphere.position.X) * (ray.startPosition.X - sphere.position.X) +
                      (ray.startPosition.Y - sphere.position.Y) * (ray.startPosition.Y - sphere.position.Y) +
                      (ray.startPosition.Z - sphere.position.Z) * (ray.startPosition.Z - sphere.position.Z) -
                      sphere.radius * sphere.radius;

            float discriminant = b * b - 4f * a * c;
            float scalar = 1;
            if (discriminant < 0) //no intersection
            {
                intersectCount = 0;
                intersects = false;
            }
            else if (discriminant == 0) //one intersection
            {
                intersectCount = 1;
                scalar = (-b) / (2 * a); //quadratic equation without the discriminant, since it is 0
                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Y * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z);
                intersects = true;
                closestIntersect = intersectionPoints[0];
            }
            else //two intersections
            {
                intersectCount = 2;
                scalar = ((-b) + (float)MathF.Sqrt(discriminant)) / (2 * a); //standard quadratic equation
                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Y * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z); //first intersection point

                scalar = ((-b) - (float)MathF.Sqrt(discriminant)) / (2 * a);
                float x2 = ray.startPosition.X + ray.direction.X * scalar;
                float y2 = ray.startPosition.Y + ray.direction.Y * scalar;
                float z2 = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[1] = new Vector3(x2, y2, z2); //second intersection point
                intersects = true;

                //check which point is closest
                float Distance1 = Vector3.Distance(intersectionPoints[0], ray.startPosition);
                float Distance2 = Vector3.Distance(intersectionPoints[1], ray.startPosition);

                if (Distance1 <= Distance2) //return the intersection point where the distance to the starting point of the ray is the smallest
                    closestIntersect = intersectionPoints[0];
                else
                    closestIntersect = intersectionPoints[1];
            }
        }

        private void AnotherSphereIntersect(Sphere sphere, Ray ray)
        {


        }

        private void PlaneIntersect(Primitive _primitive, Ray _ray) 
        { 
        
        }
    }
}
