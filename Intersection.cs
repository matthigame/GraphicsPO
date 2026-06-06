using Assimp;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace INFOGRTemplate
{
    internal class Intersection
    {
        public Vector3[] intersectionPoints;
        public int intersectCount;
        public bool Intersects => intersectCount > 0;
        public Vector3 closestIntersect;
        public Primitive primitive;
        public float distance;
        public Vector3 normalVector;
        public Intersection(Primitive _primitive, Ray _ray)
        {
            intersectionPoints = new Vector3[2];
            this.primitive = _primitive;
            switch (_primitive.type)
            {
                case PrimitiveTypes.Sphere:
                    SphereIntersect(_primitive as Sphere, _ray);

                    //calculate the normal vector if we intersect
                    if (Intersects)
                    {
                        //the normal vector is the vector from the spheres center to the intersection point on the spheres surface, normalized
                        normalVector = Vector3.Normalize(closestIntersect - primitive.position);
                    }
                    break;

                case PrimitiveTypes.Plane:
                    PlaneIntersect(_primitive as Plane, _ray);
                    //normalVector gets set in the plane intersection method
                    break;
            }
            if (Intersects)
            {
                distance = Vector3.Distance(closestIntersect, _ray.startPosition);
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
                NoIntersect();
            }
            else if (discriminant == 0) //one intersection
            {
                intersectCount = 1;
                scalar = (-b) / (2 * a); //quadratic equation without the discriminant, since it is 0
                if (scalar < 0)
                    NoIntersect();

                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Y * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z);
                closestIntersect = intersectionPoints[0];
            }
            else //two intersections
            {
                intersectCount = 2;
                scalar = ((-b) + (float)MathF.Sqrt(discriminant)) / (2 * a); //standard quadratic equation
                if (scalar < 0)
                    NoIntersect();

                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Y * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z); //first intersection point

                scalar = ((-b) - (float)MathF.Sqrt(discriminant)) / (2 * a);
                if (scalar < 0)
                    NoIntersect();

                float x2 = ray.startPosition.X + ray.direction.X * scalar;
                float y2 = ray.startPosition.Y + ray.direction.Y * scalar;
                float z2 = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[1] = new Vector3(x2, y2, z2); //second intersection point

                //check which point is closest
                float Distance1 = Vector3.Distance(intersectionPoints[0], ray.startPosition);
                float Distance2 = Vector3.Distance(intersectionPoints[1], ray.startPosition);

                if (Distance1 <= Distance2) //return the intersection point where the distance to the starting point of the ray is the smallest
                    closestIntersect = intersectionPoints[0];
                else
                    closestIntersect = intersectionPoints[1];
            }
        }

        private void NoIntersect()
        {
            intersectCount--;
        }

        private void PlaneIntersect(Primitive primitive, Ray ray) 
        {
            Plane plane = primitive as Plane;
            //plug the x, y and z values of the ray into the plane equation (in parts to make it simpler on the eyes)
            Vector3 vector1 = plane.normalVector * ray.startPosition;
            float denominator = vector1.X + vector1.Y + vector1.Z + plane.equationD;
            Vector3 vector2 = plane.normalVector * ray.direction;
            float numerator = vector2.X + vector2.Y + vector2.Z;

            float lambda = (-1 * denominator) / numerator;

            if (numerator == 0 && denominator != 0) //no intersections
            {
                intersectCount = 0;
                return; //prevent division by 0
            }
            else if (numerator == 0 && denominator == 0) //ray lies in the plane: infinite intersection points
            {
                intersectCount = 1; //not the real number but does not matter here
                return; //same thing here, we are NOT dividing by 0
            }
            else if (lambda < 1) //looking backwards
            {
                intersectCount = 0;
                return;
            }
            else 
            {
                intersectCount = 1;
            }

            closestIntersect = ray.startPosition + lambda*ray.direction; //we have found our intersect point. Yay!
            intersectionPoints[0] = closestIntersect;
            normalVector = plane.normalVector;
        }
    }
}
