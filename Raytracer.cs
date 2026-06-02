using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Template;


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
            Sphere sphereElement1 = new Sphere(new Vector3(256, 256, 0), 50, new Color3(1, 0, 0));
            Sphere sphereElement2 = new Sphere(new Vector3(150, 50, 0), 100, new Color3(0, 1, 0));
            sceneElements.Add(sphereElement1);
            sceneElements.Add(sphereElement2);

            //add all the lights in the scene
            List<Light> lightElements = new List<Light>();
            Light mainLight = new Light(new Vector3(10, 10, 10), new Color3(1, 1, 1));
            lightElements.Add(mainLight);

            scene = new Scene(sceneElements, lightElements);

        }

        private void SetupCamera()
        {
            camera = new Camera(default, default, default, 
                [new Vector3(-256, 256, 20),  //upper left screen corner
                new Vector3(256, 256, 20), //upper right screen corner
                new Vector3(256, -256,  20), //bottom right screen corner
                new Vector3(-256, -256, 20)]); //bottom left screen corner
        }

        public void Render()
        {
            for (int pixelX = (int)camera.screenCorners[0].X; pixelX < (int)camera.screenCorners[1].X; pixelX++) //for every pixel horizontally
            {
                for (int pixelY = (int)camera.screenCorners[0].Y; pixelY < (int)camera.screenCorners[3].Y; pixelY++)
                {
                    //temporarily hold to a default Z-value of 20 (same for all screen corners)
                    ShootRayThroughPixel(new Vector3(pixelX, pixelY, 20));
                }
            }
            DrawDebug(); //should be commented for now if not desired
        }

        private void ShootRayThroughPixel(Vector3 pixelPos)
        { 
            
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

            screen.Box((int)camera.position.X, (int)camera.position.Y, (int)camera.position.X + 2, (int)camera.position.Y + 2, new Color3(1, 1, 0));

        }

        private Vector3 DebugPos(Vector3 pos) 
        {
            return new Vector3(pos.X, pos.Z, 0);
        }

    }
}
