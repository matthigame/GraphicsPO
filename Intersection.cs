using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class Intersection
    {
        public Vector3[] intersectionPoints;
        public int intersectCount;
        public Intersection(Primitive _primitive, Ray _ray)
        {
            intersectionPoints = new Vector3[2];
            switch (_primitive.typeID)
            {
                case "Sphere":
                    SphereIntersect(_primitive as Sphere, _ray);
                    break;

                case "Plane":
                    PlaneIntersect(_primitive, _ray);
                    break;
            }
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
            }
            else if (discriminant == 0) //one intersection
            {
                intersectCount = 1;
                scalar = (-b) / (2 * a); //quadratic equation without the discriminant, since it is 0
                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Z * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z);
            }
            else //two intersections
            {
                intersectCount = 2;
                scalar = ((-b) + discriminant) / (2 * a); //standard quadratic equation
                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Z * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z); //first intersection point

                scalar = ((-b) - discriminant) / (2 * a);
                float x2 = ray.startPosition.X + ray.direction.X * scalar;
                float y2 = ray.startPosition.Y + ray.direction.Z * scalar;
                float z2 = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[1] = new Vector3(x2, y2, z2); //second intersection point
            }
        }

        private void PlaneIntersect(Primitive _primitive, Ray _ray) 
        { 
        
        }
    }
}
