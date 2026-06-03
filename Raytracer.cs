using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Template;
using System.Diagnostics;
using System.Security.Principal;


namespace INFOGRTemplate
{
    internal class Raytracer
    {
        public Surface screen;
        public Scene scene;
        public Camera camera;
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
            Sphere sphereElement1 = new Sphere(new Vector3(300, 50, 0), 40, new Color3(1, 0, 0));
            Sphere sphereElement2 = new Sphere(new Vector3(150, 50, 0), 40, new Color3(0, 1, 0));
            Sphere sphereElement3 = new Sphere(new Vector3(450, 50, 0), 40, new Color3(0, 0, 1));
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
            camera = new Camera(new Vector3(screen.width / 2, 300, 0), new Vector3(0, -1, 0), new Vector3(10, 0, 0));
        }

        public void Render()
        {
            int n = 0;
            for (int pixelX = 0; pixelX < screen.width; pixelX++) //for every pixel horizontally
            {
                for (int pixelY = 0; pixelY < screen.height; pixelY++)
                {
                    n++;
                    if (n > 4 && pixelY == screen.height / 2)
                    {
                        Vector3 pixelPos = new Vector3(pixelX, pixelY, camera.screenCorners[0].Z);
                        Ray ray = new Ray(camera.position, IntToPos(pixelPos) - camera.position);
                        screen.Line((int)ray.startPosition.X, (int)ray.startPosition.Y, (int)ray.startPosition.X + ((int)ray.direction.X * 500), (int)ray.startPosition.Y + ((int)ray.direction.Y * 500), new Color3(0, 0, 0));
                        n = 0;
                    }
                    //Color3 color = ShootRayThroughPixel(new Vector3(pixelX, pixelY, camera.screenCorners[0].Z));
                    //if (color != -1)
                    //    screen.Plot(pixelX, pixelY, color);
                }
            }
            DrawDebug(); //should be commented for now if not desired
        }

        private Vector3 IntToPos(Vector3 pixelPos)
        {
            float a = (pixelPos.X + 0.5f) / screen.width;
            float b = (pixelPos.Y + 0.5f) / screen.height;
            return new Vector3(a, b, pixelPos.Z);
        }

        private Color3 ShootRayThroughPixel(Vector3 pixelPos)
        {
            Ray ray = new Ray(camera.position, pixelPos - camera.position);
            Intersection finalIntersect = null;
            foreach(Primitive primitive in scene.primitives)
            {
                Intersection intersection = primitive.Intersect(ray);
                if (finalIntersect == null && intersection.intersects)
                    finalIntersect = intersection;
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
            foreach (Primitive primitive in scene.primitives) //draw each primitive in the scene
            {
                if (primitive is Sphere) //draw every sphere as a circle
                {
                    Sphere sphereToDraw = primitive as Sphere;
                    for (double angle = 0; angle < 360; angle++) //rotate 360 degrees, drawing the circle with a maximum of 360 pixels
                    {
                        float x_offset = sphereToDraw.radius * (float)Math.Cos(angle*(Math.PI/180));
                        float y_offset = sphereToDraw.radius * (float)Math.Sin(angle * (Math.PI / 180));
                        Vector3 vectortodraw = sphereToDraw.position + new Vector3(x_offset, y_offset, 0);
                        screen.Plot((int)vectortodraw.X, (int)vectortodraw.Y, primitive.color); //draw the desired pixel
                    }
                }
            }
            //debug camera
            screen.Box((int)camera.position.X, (int)camera.position.Y, (int)camera.position.X + 2, (int)camera.position.Y + 2, new Color3(1, 1, 0));
            //debug screen
            screen.Box((int)camera.screenCorners[0].X, (int)camera.screenCorners[0].Y, (int)camera.screenCorners[3].X + 2, (int)camera.screenCorners[3].Y + 2, new Color3(1, 1, 1));
            
        }

        private Vector3 DebugPos(Vector3 pos) 
        {
            return new Vector3(pos.X, pos.Z, 0);
        }

    }
}
