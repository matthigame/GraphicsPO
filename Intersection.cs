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
        public Ray ray;
        public Intersection(Primitive _primitive, Ray _ray)
        {
            intersectionPoints = new Vector3[2];
            this.primitive = _primitive;
            this.ray = _ray;
            switch (_primitive.type)
            {
                case PrimitiveTypes.Sphere:
                    SphereIntersect(primitive as Sphere, ray);

                    //calculate the normal vector if we intersect
                    if (Intersects)
                    {
                        //the normal vector is the vector from the spheres center to the intersection point on the spheres surface, normalized
                        normalVector = Vector3.Normalize(closestIntersect - primitive.position);
                    }
                    break;

                case PrimitiveTypes.Plane:
                    PlaneIntersect(primitive as Plane, _ray);
                    //normalVector gets set in the plane intersection method
                    break;
                case PrimitiveTypes.Triangle:
                    TriangleIntersect(primitive, ray);
                    //normalVector gets set in the TriangleIntersect method
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
                intersectCount--;
            }
            else if (discriminant == 0) //one intersection
            {
                intersectCount = 1;
                scalar = (-b) / (2 * a); //quadratic equation without the discriminant, since it is 0

                float x = ray.startPosition.X + ray.direction.X * scalar;
                float y = ray.startPosition.Y + ray.direction.Y * scalar;
                float z = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[0] = new Vector3(x, y, z);

                bool intersect = true;
                float distanceToIntersect = Vector3.Distance(intersectionPoints[0], ray.startPosition);
                //check if the intersection is allowed
                if (distanceToIntersect < 0.0001f || scalar < 0)
                {
                    intersectCount--;
                    intersect = false;
                }

                if (intersect)
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

                bool intersect1 = true;
                float Distance1 = Vector3.Distance(intersectionPoints[0], ray.startPosition);
                //check if the intersection is allowed
                if (Distance1 < 0.0001f || scalar < 0)
                {
                    intersectCount--;
                    intersect1 = false;
                }
                scalar = ((-b) - (float)MathF.Sqrt(discriminant)) / (2 * a);

                float x2 = ray.startPosition.X + ray.direction.X * scalar;
                float y2 = ray.startPosition.Y + ray.direction.Y * scalar;
                float z2 = ray.startPosition.Z + ray.direction.Z * scalar;
                intersectionPoints[1] = new Vector3(x2, y2, z2); //second intersection point

                bool intersect2 = true;
                float Distance2 = Vector3.Distance(intersectionPoints[1], ray.startPosition);
                //check if the intersection is allowed
                if (Distance2 < 0.0001f || scalar < 0)
                {
                    intersectCount--;
                    intersect2 = false;
                }

                //check if only 1 of the intersects is allowed, if so choose the other
                if (intersect1 && !intersect2)
                    closestIntersect = intersectionPoints[0];
                else if (!intersect1 && intersect2)
                    closestIntersect = intersectionPoints[1];
                else //if both are allowed return the intersection where the distance tot the starting point of the ray is the smallest
                {
                    if (Distance1 <= Distance2)
                        closestIntersect = intersectionPoints[0];
                    else
                        closestIntersect = intersectionPoints[1];
                }
            }
        }

        private void PlaneIntersect(Primitive primitive, Ray ray) 
        {
            Plane plane = primitive as Plane;

            var result = plane.intersectionPoint(ray.startPosition, ray.direction);
            Vector3 intersectionPoint = result.Item1;
            intersectCount = result.Item2;

            if (intersectCount > 0) 
            {
                closestIntersect = intersectionPoint;
                intersectionPoints[0] = closestIntersect;
                normalVector = plane.normalVector;
            }
        }

        private void TriangleIntersect(Primitive primitive, Ray ray) 
        { 
            Triangle triangle = primitive as Triangle;

            Plane trianglePlane = new Plane(triangle.normalVector, triangle.A, triangle.color, triangle.material, false);

            Intersection planeIntersect = trianglePlane.Intersect(ray);


            if (planeIntersect.Intersects)
            {
                float check1 = Vector3.Dot(Vector3.Cross(triangle.B - triangle.A, planeIntersect.closestIntersect - triangle.A), triangle.normalVector);
                float check2 = Vector3.Dot(Vector3.Cross(triangle.C - triangle.B, planeIntersect.closestIntersect - triangle.B), triangle.normalVector);
                float check3 = Vector3.Dot(Vector3.Cross(triangle.A - triangle.C, planeIntersect.closestIntersect - triangle.C), triangle.normalVector);

                if (check1 >= 0 &&
                    check2 >= 0 && 
                    check3 >= 0)   
                {
                    intersectCount++;
                    closestIntersect = planeIntersect.closestIntersect;
                    intersectionPoints[0] = closestIntersect;

                    if (Vector3.Dot(ray.direction, triangle.normalVector) > 0)
                        normalVector = -triangle.normalVector;
                    else
                        normalVector = triangle.normalVector;

                }
                else
                {
                    intersectCount = 0;
                }
            }
            else //if we dont intersect the plane, we for sure dont intersect the triangle, so we dont calculate further
            {
                intersectCount = 0;
            }


        }
    }
}  
