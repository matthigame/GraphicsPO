using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Template;
using System.Diagnostics;
using System.Security.Principal;
using System.Net.Quic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection.Metadata.Ecma335;


namespace INFOGRTemplate
{
    internal class Raytracer
    {
        public Surface screen;
        public Scene scene;
        public Camera camera;
        KeyboardState keyboardState;
        public Raytracer(Surface _screen)
        {
            screen = _screen;
            InitializeScene();
            SetupCamera();
        }

        private void InitializeScene()
        { 
            //add all the objects in the scene
            List<Primitive> sceneElements = new List<Primitive>();
            Sphere sphereElement1 = new Sphere(new Vector3(1, 0, 5), 2, new Color3(1, 0, 0));
            Sphere sphereElement2 = new Sphere(new Vector3(-5, 0, 4), 3, new Color3(0, 1, 0));
            Sphere sphereElement3 = new Sphere(new Vector3(4, 0, 8), 2, new Color3(0, 0, 1));
            sceneElements.Add(sphereElement1);
            sceneElements.Add(sphereElement2);
            sceneElements.Add(sphereElement3);

            //add all the lights in the scene
            List<Light> lightElements = new List<Light>();
            Light mainLight = new Light(new Vector3(10, 10, 10), new Color3(1, 1, 1));
            lightElements.Add(mainLight);

            scene = new Scene(sceneElements, lightElements);

        }

        private void SetupCamera()
        {
            camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), screen);
        }

        public void Render()
        {
            Vector2 debugOrigin = new Vector2(screen.width / 2, 300);
            int n = 0;
            for (int pixelX = 0; pixelX < screen.width; pixelX++) //for every pixel horizontally
            {
                for (int pixelY = 0; pixelY < screen.height; pixelY++)
                {
                    Vector3 direction = (camera.screenCorners[0] + ((pixelX + 0.5f) / screen.width * (camera.screenCorners[1] - camera.screenCorners[0])) + ((pixelY + 0.5f) / screen.height * (camera.screenCorners[2] - camera.screenCorners[0]))) - camera.position;
                    direction = Vector3.Normalize(direction);
                    PrimaryRay ray = new PrimaryRay(camera.position, direction);


                    //This is the ray for the debug
                    //n++;
                    //if (pixelY % 15 == 0 && pixelX % 15 == 0)
                    //{
                    //    Vector2 startPosition = new Vector2(ray.startPosition.X, -ray.startPosition.Z) + debugOrigin;
                    //    screen.Line((int)startPosition.X, (int)startPosition.Y, (int)startPosition.X + (int)(ray.direction.X * 750), (int)startPosition.Y + (int)(-ray.direction.Z * 750), new Color3(0, 0, 0));
                    //}

                    //This is the actual ray
                    Color3 color = ShootRayThroughPixel(ray);
                    if (color != -1)
                        screen.Plot(pixelX, pixelY, color);
                }
            }
            //DrawDebug(); //should be commented for now if not desired
        }

        private Color3 ShootRayThroughPixel(PrimaryRay ray)
        {
            Intersection finalIntersect = null;
            foreach(Primitive primitive in scene.primitives)
            {
                Intersection intersection = primitive.Intersect(ray);
                if (finalIntersect == null && intersection.intersects)
                    finalIntersect = intersection;
                else if (finalIntersect == null)
                    continue;
                else if (intersection.intersects && ClosestIntersect(finalIntersect, intersection))
                    finalIntersect = intersection;
            }
            if (finalIntersect != null) 
                return finalIntersect.primitive.color;
            return -1;

        }

        private bool ClosestIntersect(Intersection intersection1, Intersection intersection2)
        {
            if (intersection1.distance < intersection2.distance) 
                return false;
            return true;
        }

        //method for drawing the debug
        private void DrawDebug()
        {
            Vector2 debugOrigin = new Vector2(screen.width / 2, 300);
            int scalar = 30;
            foreach (Primitive primitive in scene.primitives) //draw each primitive in the scene
            {
                if (primitive is Sphere) //draw every sphere as a circle
                {
                    Sphere sphereToDraw = primitive as Sphere;
                    for (double angle = 0; angle < 360; angle++) //rotate 360 degrees, drawing the circle with a maximum of 360 pixels
                    {
                        float x_offset = sphereToDraw.radius * (float)Math.Cos(angle*(Math.PI/180));
                        float y_offset = sphereToDraw.radius * (float)Math.Sin(angle * (Math.PI / 180));
                        Vector2 vectortodraw = debugOrigin + scalar * (new Vector2(sphereToDraw.position.X, -sphereToDraw.position.Z)+ new Vector2(x_offset, y_offset));
                        screen.Plot((int)vectortodraw.X, (int)vectortodraw.Y, primitive.color); //draw the desired pixel
                    }
                }
            }
            //debug camera
            screen.Box((int)camera.position.X + (int)debugOrigin.X, (int)camera.position.Y + (int)debugOrigin.Y, (int)camera.position.X + 2 + (int)debugOrigin.X, (int)camera.position.Y + 2 + (int)debugOrigin.Y, new Color3(1, 1, 0));
            //debug screenc
            screen.Line((int)(scalar * (camera.position.X + camera.screenCorners[0].X) + debugOrigin.X), (int)(scalar * (-camera.position.Z - camera.fov) + debugOrigin.Y), (int)(scalar  * (camera.position.X + camera.screenCorners[1].X) + debugOrigin.X), (int)(scalar * (-camera.position.Z - camera.fov) + debugOrigin.Y), new Color3(1, 1, 1));
            
        }


        private Vector3 DebugPos(Vector3 pos) 
        {
            return new Vector3(pos.X, pos.Z, 0);
        }

    }
}
