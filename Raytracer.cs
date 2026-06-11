using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Template;
using System.Diagnostics;
using System.Security.Principal;
using System.Net.Quic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System.Reflection.Metadata.Ecma335;
using OpenTK.Audio.OpenAL;
using Assimp;
using System.Linq.Expressions;
using System.Security.AccessControl;


namespace INFOGRTemplate
{
    internal class Raytracer
    {
        public Surface screen;
        public Scene scene;
        public Camera camera;
        public float sensitivity = 3f;
        public KeyboardState KeyBoardState {  get; set; }
        public MouseState MouseState { get; set; }
        PrimaryRay mainRay;

        List<PrimaryRay> primaryRays;
        List<ShadowRay> shadowRays;

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
            Sphere sphereElement1 = new Sphere(new Vector3(-5, 1, 6), 1, new Color3(1, 0, 0), Materials.Diffuse);
            Sphere sphereElement2 = new Sphere(new Vector3(-2, 2, 6), 1.5f, new Color3(0, 1, 0), Materials.Reflective);
            Sphere sphereElement3 = new Sphere(new Vector3(2, 2, 6), 2, new Color3(0, 0, 0), Materials.Reflective);

            Plane basePlane = new Plane(new Vector3(0, 1, 0), 1f, new Color3(0f, 0f, 0f), Materials.Diffuse, true); //floor
            Plane wallPlane = new Plane(new Vector3(0, 0, -1), 20f, new Color3(0, 0, 0), Materials.Reflective, false); //backboard

            sceneElements.Add(sphereElement1);
            sceneElements.Add(sphereElement2);
            sceneElements.Add(sphereElement3);


            sceneElements.Add(basePlane);
            sceneElements.Add(wallPlane);

            //add all the lights in the scene
            List<Light> lightElements = new List<Light>();
            Light mainLight = new Light(new Vector3(-8, 25, 7), new Color3(600, 600, 600));
            //Light secondaryLight = new Light(new Vector3(-5, 4, 12), new Color3(1, 1, 1));
            lightElements.Add(mainLight);
            //lightElements.Add(secondaryLight);

            scene = new Scene(sceneElements, lightElements);

        }

        private void SetupCamera()
        {
            camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), screen);
        }

        public void Render()
        {
            Vector2 debugOrigin = new Vector2(screen.width / 2 - camera.position.X, 300 + camera.position.Z);
            int n = 0;
            primaryRays = new List<PrimaryRay>(); //reset the list every render


            for (int pixelX = 0; pixelX < screen.width; pixelX++) //for every pixel horizontally
            {
                for (int pixelY = 0; pixelY < screen.height; pixelY++)
                {
                    Vector3 direction = (camera.screenCorners[0] + ((pixelX + 0.5f) / screen.width * (camera.screenCorners[1] - camera.screenCorners[0])) + ((pixelY + 0.5f) / screen.height * (camera.screenCorners[2] - camera.screenCorners[0]))) - camera.position;
                    direction = Vector3.Normalize(direction);
                    if (mainRay == null) //the first time, the ray has to be instantiated
                    {
                        mainRay = new PrimaryRay(camera.position, direction, 10);
                    }
                    else
                    {
                        mainRay.startPosition = camera.position;
                        mainRay.direction = Vector3.Normalize(direction);
                    }

                    //add 1 in 15 rays of the middle screen row to a list to display in the debug screen
                    if (pixelX % 15 == 0 && pixelY == screen.height/2)
                    {
                        primaryRays.Add(new PrimaryRay(camera.position, direction, 10));
                    }
                    n++;


                    ////This is the ray for the debug
                    //if (KeyBoardState.IsKeyDown(Keys.K))
                    //{
                    //    //n++;
                    //    //if (pixelY % 15 == 0 && pixelX % 15 == 0)
                    //    //{
                    //    //    Vector2 startPosition = new Vector2(mainRay.startPosition.X, -mainRay.startPosition.Z) + debugOrigin;
                    //    //    screen.Line((int)startPosition.X, (int)startPosition.Y, (int)startPosition.X + (int)(mainRay.direction.X * 750), (int)startPosition.Y + (int)(-mainRay.direction.Z * 750), new Color3(0, 0, 0));
                    //    //}
                    //}

                    //This is the actual ray
                    if (!KeyBoardState.IsKeyDown(Keys.K))
                    {
                        Color3 color = ShootRayThroughPixel(mainRay);
                        if (color != -1)
                            screen.Plot(pixelX, pixelY, color);
                    }
                }
            }

            //when K is down, the debug screen is shown
            if (KeyBoardState.IsKeyDown(Keys.K))
                DrawDebug();

            //Camera movement
            if (KeyBoardState.IsKeyDown(Keys.A))
                camera.position -= 0.2f * camera.rightDirection;
            if (KeyBoardState.IsKeyDown(Keys.D))
                camera.position += 0.2f * camera.rightDirection;
            if (KeyBoardState.IsKeyDown(Keys.W))
                camera.position += 0.2f * camera.lookAtDirection;
            if (KeyBoardState.IsKeyDown(Keys.S))
                camera.position -= 0.2f * camera.lookAtDirection;
            if (KeyBoardState.IsKeyDown(Keys.Space))
                camera.position += 0.2f * Vector3.UnitY;
            if (KeyBoardState.IsKeyDown(Keys.LeftShift))
                camera.position -= 0.2f * Vector3.UnitY;


            //scrollen om de fov te veranderen (in of uit te zoomen)
            camera.fov += 0.1f * MouseState.ScrollDelta.Y;

            //looking around with mouse
            //camera.angleX -= sensitivity * MouseState.Delta.Y;
            //camera.angleY += sensitivity * MouseState.Delta.X;

            //looking around with arrow keys
            if (KeyBoardState.IsKeyDown(Keys.Up))
                camera.angleX += sensitivity;
            if (KeyBoardState.IsKeyDown(Keys.Down))
                camera.angleX -= sensitivity;
            if (KeyBoardState.IsKeyDown(Keys.Right))
                camera.angleY += sensitivity;
            if (KeyBoardState.IsKeyDown(Keys.Left))
                camera.angleY -= sensitivity;

            camera.UpdateCamera();

            //making the light move, just for fun
            //scene.lightSources[0].location += new Vector3(-0.1f, 0, 0);


        }

        private Color3 ShootRayThroughPixel(PrimaryRay ray)
        {
            Intersection finalIntersect = null;
            foreach(Primitive primitive in scene.primitives)
            {
                Intersection intersection = primitive.Intersect(ray);
                if (finalIntersect == null && intersection.Intersects)
                    finalIntersect = intersection;
                else if (finalIntersect == null)
                    continue;
                else if (intersection.Intersects && ClosestIntersect(finalIntersect, intersection) )
                    finalIntersect = intersection;
            }

            if (finalIntersect != null)
            {
                //If the material is reflective, shoot the reflected ray and replace the final intersection
                if (finalIntersect.primitive.material == Materials.Reflective && ray.bounces > 0)
                {
                    //black mirror reflects everything normally, coloured mirrors add their color to the final colour
                    if (finalIntersect.primitive.color != new Color3(0, 0, 0))
                        return finalIntersect.primitive.color * ShootRayThroughPixel(new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(ray.direction, finalIntersect.normalVector), ray.bounces - 1));
                    return ShootRayThroughPixel(new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(ray.direction, finalIntersect.normalVector), ray.bounces - 1));
                }
                return DecidePixelColor(finalIntersect);
            }
            return new Color3(0.5f, 0.5f, 0.5f);
        }
        
        

        private Color3 DecidePixelColor(Intersection initialIntersect)
        {
            Color3 diffuseColor = initialIntersect.primitive.color;
            List<Light> lightsReached = LightsReached(initialIntersect);
            Color3 finalColor = new Color3(0, 0, 0); //start off black
            if (initialIntersect.primitive is Plane)
            {
                Plane plane = initialIntersect.primitive as Plane;
                if (plane.checkers)
                    diffuseColor = plane.checkerBoards(initialIntersect.closestIntersect);
            }

            foreach (Light light in lightsReached) 
            {
                //intensity*distance attenuation(1/r*r) modulated by the angle of the light and modulated by the diffuse color
                finalColor += light.intensity *
                              (1 / (Vector3.Distance(initialIntersect.closestIntersect, light.location) * Vector3.Distance(initialIntersect.closestIntersect, light.location))) *
                              Math.Max(0, Vector3.Dot(initialIntersect.normalVector, Vector3.Normalize(light.location - initialIntersect.closestIntersect))) *
                              diffuseColor;
            }

            finalColor += new Color3(0.05f, 0.05f, 0.05f); //add diffuse lighting
            return finalColor;
        }


        //only needed for the debugging
        List<Vector3> endPoints;
        private List<Light> LightsReached (Intersection source)
        {
            endPoints = new List<Vector3> ();
            ShadowRay shadowRay = null;
            List<Light> finalResult = new List<Light>();
            foreach (Light light in scene.lightSources)
            {
                if (shadowRay == null) //first time in the loop
                {
                    //shoot a ray from the intersect point to the light
                    shadowRay = new ShadowRay(source.closestIntersect, Vector3.Normalize(light.location - source.closestIntersect));
                }
                else
                {
                    shadowRay.startPosition = source.closestIntersect;
                    shadowRay.direction = Vector3.Normalize(light.location - source.closestIntersect);
                }

                bool blocked = false;
                foreach (Primitive primitive in scene.primitives)
                {
                    Intersection intersect = primitive.Intersect(shadowRay);
                    for (int i = 0; i < intersect.intersectCount; i++)
                    {
                        float distanceToIntersect = Vector3.Distance(intersect.intersectionPoints[i], shadowRay.startPosition);
                        float distanceToLight = Vector3.Distance(source.closestIntersect, light.location);
                        if (distanceToLight > distanceToIntersect && distanceToIntersect > 0.0001f) //prevent intersection with self, and ignores intersection past the light
                        {
                            endPoints.Add(intersect.closestIntersect);
                            blocked = true;
                        }

                    }

                    if (blocked) //dont check every primitive after finding one that blocks the light
                        break;

                }

                if (!blocked)
                {
                    finalResult.Add(light);
                    endPoints.Add(light.location);
                }
            }
            return finalResult;
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
            Vector2 debugOrigin = new Vector2(screen.width / 2 - camera.position.X, 300 + camera.position.Z);
            Vector2 offset = new Vector2(-camera.position.X, camera.position.Z);
            int scalar = 30;


            //planes should be drawn before the spheres, so that the spheres dont turn invisible
            foreach (Primitive primitive in scene.primitives)
            {
                if (primitive is Plane)
                {
                    Plane planeToDraw = primitive as Plane;
                    if (planeToDraw.normalVector.Y != 0) //the plane is not completely vertical, seen from above, so it is visible everywhere
                    {
                        for (int row = 0; row < screen.height; row++)
                        {
                            for (int column = 0; column < screen.width; column++)
                            {
                                screen.Plot(column, row, planeToDraw.color);
                            }
                        }
                    }
                }
            }

            foreach (Primitive primitive in scene.primitives) //draw each primitive in the scene
            {
                
                if (primitive is Sphere) //draw every sphere as a circle
                {
                    Sphere sphereToDraw = primitive as Sphere;
                    for (double angle = 0; angle < 360; angle++) //rotate 360 degrees, drawing the circle with a maximum of 360 pixels
                    {
                        float x_offset = sphereToDraw.radius * (float)Math.Cos(angle*(Math.PI/180));
                        float y_offset = sphereToDraw.radius * (float)Math.Sin(angle * (Math.PI / 180));
                        Vector2 vectortodraw = debugOrigin + scalar * (new Vector2(sphereToDraw.position.X, -sphereToDraw.position.Z)+ new Vector2(x_offset, y_offset) + offset);
                        screen.Plot((int)vectortodraw.X, (int)vectortodraw.Y, primitive.color); //draw the desired pixel
                    }
                }

            }

            //debug 1/15 of the primary rays in the middle of the screen
            foreach (PrimaryRay primaryRay in primaryRays) {
                Intersection finalIntersect = null;

                //check the nearest intersection point
                foreach (Primitive primitive in scene.primitives)
                {
                    Intersection intersection = primitive.Intersect(primaryRay);
                    if (finalIntersect == null && intersection.Intersects)
                        finalIntersect = intersection;
                    else if (finalIntersect == null)
                        continue;
                    else if (intersection.Intersects && ClosestIntersect(finalIntersect, intersection))
                        finalIntersect = intersection;
                }

                Vector3 endpoint = 1000 * primaryRay.direction; //the default endpoint is VERY far away
                if (finalIntersect != null)
                    endpoint = finalIntersect.closestIntersect; //make the intersection point the end point


                //translate to debug coordinates
                Vector2 debugStart = DebugPos(primaryRay.startPosition) + debugOrigin;
                Vector2 debugEnd = debugOrigin + scalar * (DebugPos(endpoint) + offset);


                screen.Line((int)debugStart.X, (int)debugStart.Y, (int)debugEnd.X, (int)debugEnd.Y, new Color3(0.5f, 0f, 0f)); //draw the ray
               
                //now draw the shadow rays from each intersection
                if (finalIntersect != null)
                {
                    debugStart = scalar * (DebugPos(finalIntersect.closestIntersect) + offset) + debugOrigin;
                    List<Light> lights = LightsReached(finalIntersect); //during this method the endPoints list is updated
                    foreach (Vector3 endPoint in endPoints)
                    { 
                        debugEnd = scalar * (DebugPos(endPoint) + offset) + debugOrigin;
                        screen.Line((int)debugStart.X, (int)debugStart.Y, (int)debugEnd.X, (int)debugEnd.Y, new Color3(0.8f, 0.8f, 0)); //draw the shadow ray
                    }
                }
                
            }

            //draw the light sources
            foreach (Light light in scene.lightSources)
            {
                for (int i = 0; i < 2; i++) //2 times for 2 rings around the lightsource
                {
                    for (double angle = 0; angle < 360; angle += 16 * (1+i)) //rotate 360 degrees in intervals of 32 for the outer layer and 16 for the inner, for the correct visual effect
                    {
                        //offset are * (1+i) in order to get two rings with a 1/2 radius ratio
                        float x_offset = (1+i) * 0.25f * (float)Math.Cos(angle * (Math.PI / 180));
                        float y_offset = (1+i) * 0.25f * (float)Math.Sin(angle * (Math.PI / 180));
                        Vector2 vectortodraw = debugOrigin + scalar * (DebugPos(light.location) + new Vector2(x_offset, y_offset));
                        screen.Plot((int)vectortodraw.X, (int)vectortodraw.Y, light.intensity); //draw the desired pixel
                    }
                }

            }

            //debug camera
            screen.Box((int)camera.position.X + (int)debugOrigin.X, (int)-camera.position.Z + (int)debugOrigin.Y, (int)camera.position.X + 2 + (int)debugOrigin.X, (int)-camera.position.Z + 2 + (int)debugOrigin.Y, new Color3(1, 1, 0));
            //debug screenc
            screen.Line((int)(scalar * (camera.screenCorners[0].X + offset.X) + debugOrigin.X - offset.X), (int)(scalar * (-camera.screenCorners[0].Z + offset.Y) + debugOrigin.Y - offset.Y), (int)(scalar  * (camera.screenCorners[1].X + offset.X) + debugOrigin.X - offset.X), (int)(scalar * (-camera.screenCorners[1].Z + offset.Y) + debugOrigin.Y - offset.Y), new Color3(1, 1, 1));
            
        }


        private Vector2 DebugPos(Vector3 pos) 
        {
            return new Vector2(pos.X, -pos.Z); // we are looking from above, so the y-axis on the debug screen should represent the z-axis
        }

    }
}
