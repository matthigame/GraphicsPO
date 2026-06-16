using Assimp;
using OpenTK.Audio.OpenAL;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.Quic;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using Template;


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

        ConcurrentBag<PrimaryRay> primaryRays;

        bool aliasingToggle;
        Vector2[] pixelOffsets = [new Vector2(-0.25f, -0.25f), new Vector2(0f, -0.25f), new Vector2(0.25f, -0.25f), 
                                  new Vector2(-0.25f, 0f), new Vector2(0f, 0f), new Vector2(0.25f, 0f),
                                  new Vector2(-0.25f, 0.25f), new Vector2(0f, 0.25f), new Vector2(0.25f, 0.25f)];

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
            Sphere sphereElement1 = new Sphere(new Vector3(-2, 1, 6), 1, new Color3(1, 0, 0), Materials.Diffuse, true);
            Sphere sphereElement2 = new Sphere(new Vector3(-5, 0, 6), 1.5f, new Color3(0, 1, 0), Materials.Reflective, false);
            //Sphere sphereElement3 = new Sphere(new Vector3(2, 2, 6), 2, new Color3(0, 0, 0), Materials.Reflective, false);
            Sphere sphereElement4 = new Sphere(new Vector3(0, 1, 5), 1.2f, new Color3(1, 1f, 1f), Materials.Refractive, false);


            Plane basePlane = new Plane(new Vector3(0, 1, 0), 1f, new Color3(0.02f, 0.08f, 0.2f), Materials.Diffuse, true); //floor
            Plane wallPlane = new Plane(new Vector3(0, 0, -1), 20f, new Color3(0.4f, 0.1f, 0.7f), Materials.Metallic, false); //backboard


            Triangle triangle1 = new Triangle(new Color3(1, 0, 0), [new Vector3(-2, 0, 16), new Vector3(2, 0, 16), new Vector3(0, 2, 16)], Materials.Diffuse, false);
            Triangle triangle2 = new Triangle(new Color3(0, 1, 1), [new Vector3(0, 1, 3), new Vector3(1, 1, 3), new Vector3(0, 2, 3)], Materials.Diffuse, true);
            Triangle triangle3 = new Triangle(new Color3(1, 0, 1), [new Vector3(0, 0, 3), new Vector3(0, 1, 3), new Vector3(-1, 1, 3)], Materials.Diffuse, false);
            Triangle triangle4 = new Triangle(new Color3(1, 1, 1), [new Vector3(-1, 1, 3), new Vector3(0, 1, 3), new Vector3(0, 2, 3)], Materials.Diffuse, false);

            Triangle piramid1 = new Triangle(new Color3(1, 0, 0), [new Vector3(2, 1, 3), new Vector3(6, 1, 3), new Vector3(4, 3, 5)], Materials.Diffuse, true);
            Triangle piramid2 = new Triangle(new Color3(0, 0, 1), [new Vector3(4, 1, 7), new Vector3(2, 1, 3), new Vector3(4, 3, 5)], Materials.Plastic, false);
            Triangle piramid3 = new Triangle(new Color3(0, 1, 0), [new Vector3(6, 1, 3), new Vector3(4, 1, 7), new Vector3(4, 3, 5)], Materials.Metallic, false);
            Triangle piramid4 = new Triangle(new Color3(1, 1, 1), [new Vector3(2, 1, 3), new Vector3(6, 1, 3), new Vector3(4, 1, 7)], Materials.Reflective, false);


            sceneElements.Add(sphereElement1);
            sceneElements.Add(sphereElement2);
            //sceneElements.Add(sphereElement3);
            sceneElements.Add(sphereElement4);


            sceneElements.Add(basePlane);
            sceneElements.Add(wallPlane);

            sceneElements.Add(triangle1);
            //sceneElements.Add(triangle2);
            //sceneElements.Add(triangle3);
            //sceneElements.Add(triangle4);

            sceneElements.Add(piramid1);   
            sceneElements.Add(piramid2);
            sceneElements.Add(piramid3);
            sceneElements.Add(piramid4);


            //add all the lights in the scene
            List<Light> lightElements = new List<Light>();
            Light mainLight = new Light(new Vector3(-8, 25, 7), new Color3(400, 400, 400));
            SpotLight spotLight = new SpotLight(new Vector3(4, 8, -3), new Color3(100, 100, 100), new Vector3(0, -1, 1), 15);
            //Light secondaryLight = new Light(new Vector3(-5, 4, 12), new Color3(1, 1, 1));
            lightElements.Add(mainLight);
            //lightElements.Add(secondaryLight);
            lightElements.Add(spotLight);

            scene = new Scene(sceneElements, lightElements);

        }

        private void SetupCamera()
        {
            camera = new Camera(new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), screen);
        }

        public void Render()
        {
            Vector2 debugOrigin = new Vector2(screen.width / 2 - camera.position.X, 300 + camera.position.Z);
            primaryRays = new ConcurrentBag<PrimaryRay>(); //reset the list every render

            //loop through every row, and parallelly render every pixel in that row (muti-threading)
            Parallel.For(0, screen.height, pixelY => RenderRow(pixelY));

            //when K is down, the debug screen is shown
            if (KeyBoardState.IsKeyDown(Keys.K))
                DrawDebug();

            //when F is down, anti-aliasing is toggled
            if (KeyBoardState.IsKeyPressed(Keys.F))
                if (aliasingToggle)
                    aliasingToggle = false;
                else
                    aliasingToggle = true;

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
            camera.fov -= 2f * MouseState.ScrollDelta.Y;

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

            //spotlight = flashlight for camera
            //SpotLight spotLight = scene.lightSources[1] as SpotLight;
            //spotLight.location = camera.position;
            //spotLight.direction = camera.lookAtDirection;

        }

        public void RenderRow(int pixelY) 
        {
            for (int pixelX = 0; pixelX < screen.width; pixelX++) //loop through all the pixels in the row
            {
                if (aliasingToggle) //anti-aliasing is active
                {
                    Color3 alColor = new Color3(0, 0, 0); //define a blank color, so that we can add on top
                    for (int pixelOff = 0; pixelOff < pixelOffsets.Length; pixelOff++) //loop through all the offsets that are defined at the top of the class
                    {
                        //add the offsets to the middle of the pixel
                        Vector3 direction = (camera.screenCorners[0] + ((pixelX + 0.5f + pixelOffsets[pixelOff].X) / screen.width * (camera.screenCorners[1] - camera.screenCorners[0])) + ((pixelY + 0.5f + pixelOffsets[pixelOff].Y) / screen.height * (camera.screenCorners[2] - camera.screenCorners[0]))) - camera.position;
                        direction = Vector3.Normalize(direction);
                        alColor += ActuallyRender(direction, pixelX, pixelY); //add the result of each ray
                    }
                    alColor = alColor / pixelOffsets.Length; //divide to get average
                    screen.Plot(pixelX, pixelY, alColor);
                }
                else
                {
                    Vector3 direction = (camera.screenCorners[0] + ((pixelX + 0.5f) / screen.width * (camera.screenCorners[1] - camera.screenCorners[0])) + ((pixelY + 0.5f) / screen.height * (camera.screenCorners[2] - camera.screenCorners[0]))) - camera.position;
                    direction = Vector3.Normalize(direction);
                    Color3 color = ActuallyRender(direction, pixelX, pixelY);
                    screen.Plot(pixelX, pixelY, color);
                }
            }
        }

        public Color3 ActuallyRender(Vector3 direct, int pixX, int pixY)
        {
            //make the actual ray
            PrimaryRay ray = new PrimaryRay(camera.position, direct, 10);

            //add 1 in 15 rays of the middle screen row to a list to display in the debug screen
            if (pixX % 15 == 0 && pixY == screen.height / 2)
            {
                primaryRays.Add(new PrimaryRay(camera.position, direct, 10));
            }

            //This is the actual ray
            if (!KeyBoardState.IsKeyDown(Keys.K))
                return ShootRayThroughPixel(ray);
            return new Color3(0, 0, 0);
            
        }
        private Color3 ShootRayThroughPixel(PrimaryRay ray)
        {
            Intersection finalIntersect = FindFinalIntersection(ray);

            if (finalIntersect != null)
            {
                //If the material is reflective, shoot the reflected ray and replace the final intersection
                if (finalIntersect.primitive.material == Materials.Reflective && ray.bounces > 0)
                {
                    //black mirror reflects everything normally, coloured mirrors add their color to the final colour
                    if (finalIntersect.primitive.color != new Color3(0, 0, 0))
                        return DecidePixelColor(finalIntersect) + finalIntersect.primitive.color * ShootRayThroughPixel(new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(ray.direction, finalIntersect.normalVector), ray.bounces - 1));
                    return ShootRayThroughPixel(new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(ray.direction, finalIntersect.normalVector), ray.bounces - 1));
                }

                //if the material is refractive, we shoot another ray trough it at a skewed angle depending on the refraction indices
                if (finalIntersect.primitive.material == Materials.Refractive && ray.bounces > 0)
                {
                    Vector3 normalToUse = finalIntersect.normalVector;
                    if (Vector3.Dot(finalIntersect.normalVector, ray.direction) > 0)
                        normalToUse *= -1; //flip the normal vector for correct refraction math

                    float enteringRefractionIndex = finalIntersect.primitive.refraction_index; //get the refraction index from the primitive

                    float dnDot = Vector3.Dot(ray.direction, normalToUse);
                    float discriminant = 1 - (ray.sourceRefractIndex * ray.sourceRefractIndex) / (enteringRefractionIndex * enteringRefractionIndex) * (1 - dnDot * dnDot);

                    if (MathF.Abs(ray.sourceRefractIndex - enteringRefractionIndex) < 0.0001f && discriminant < 0) //total internal refraction
                        enteringRefractionIndex = 1; //when the ray comes from inside the primitive, assume we are going into air

                    //formula given in the slides
                    Vector3 newDirection = Vector3.Normalize((ray.sourceRefractIndex/ enteringRefractionIndex) *(ray.direction - dnDot*normalToUse) - MathF.Sqrt(discriminant)*normalToUse);


                    //create a new ray, going in the new direction and set the source refraction index to the material it is currectly going into
                    PrimaryRay refractionRay = new PrimaryRay(finalIntersect.closestIntersect, newDirection, ray.bounces - 1) { sourceRefractIndex = enteringRefractionIndex };

                    //also create a reflected ray to allow partial reflection when relevant
                    PrimaryRay reflectionRay = new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(ray.direction, normalToUse), ray.bounces - 1);

                    //Fresnel coeëficient calculations
                    float FresnelR0 = (enteringRefractionIndex - ray.sourceRefractIndex) / (enteringRefractionIndex + ray.sourceRefractIndex) * (enteringRefractionIndex - ray.sourceRefractIndex) / (enteringRefractionIndex + ray.sourceRefractIndex);
                    float Fresnel = FresnelR0 + MathF.Pow((1 - FresnelR0) * (1 - Vector3.Dot(-ray.direction, normalToUse)), 5);

                    return finalIntersect.primitive.color * (Fresnel * ShootRayThroughPixel(reflectionRay) + (1f - Fresnel) * ShootRayThroughPixel(refractionRay)); //reflected and refracted colors balanced by Fresnel, modulated by the color of the primitive
                }

                return DecidePixelColor(finalIntersect); //Diffuse. So we dont need to go into recursion
            }
            return new Color3(58f/255f, 166f/255f, 242f/255f); //default color
        }


        private Intersection FindFinalIntersection(PrimaryRay ray)
        {
            //Loop through all primitives and return the primitive with the closest intersection to the starting point
            Intersection finalIntersect = null;
            foreach (Primitive primitive in scene.primitives)
            {
                Intersection intersection = primitive.Intersect(ray);
                if (finalIntersect == null && intersection.Intersects)
                    finalIntersect = intersection;
                else if (finalIntersect == null)
                    continue;
                else if (intersection.Intersects && ClosestIntersect(finalIntersect, intersection) )
                    finalIntersect = intersection;
            }
            return finalIntersect;
        }
        
        

        private Color3 DecidePixelColor(Intersection initialIntersect)
        {
            Color3 diffuseColor = initialIntersect.primitive.color;
            List<Light> lightsReached = LightsReached(initialIntersect); //a list of light sources to consider
            Color3 finalColor = new Color3(0, 0, 0); //start off black

            //if checkers is true, change the diffuse color based the u and v values at the intersect point
            if (initialIntersect.primitive.checkers)
                diffuseColor = initialIntersect.primitive.checkerBoards(initialIntersect.closestIntersect, initialIntersect.normalVector, Vector3.Distance(camera.position, initialIntersect.closestIntersect));


            foreach (Light light in lightsReached)
            {
                //add the effects of glossy materials (plastic & metal)
                if (initialIntersect.primitive.material == Materials.Plastic || initialIntersect.primitive.material == Materials.Metallic) 
                {
                    Vector3 toCamera = Vector3.Normalize(initialIntersect.ray.startPosition - initialIntersect.closestIntersect);
                    Vector3 reflectedLight = Vector3.Normalize(Vector3.Reflect(initialIntersect.closestIntersect - light.location, initialIntersect.normalVector));
                    float vrDot = Vector3.Dot(toCamera, reflectedLight);

                    float glossyFactor = 10;

                    Color3 glossColor;
                    if (initialIntersect.primitive.material == Materials.Metallic)
                        glossColor = initialIntersect.primitive.color; //metallic object reflect their own color
                    else
                        glossColor = new Color3(1, 1, 1); //plastic objects reflect white

                    finalColor += light.intensity * (1 / (Vector3.Distance(initialIntersect.closestIntersect, light.location) * Vector3.Distance(initialIntersect.closestIntersect, light.location))) *
                                  glossColor * MathF.Pow(MathF.Max(0, vrDot), glossyFactor);

                }

                //intensity*distance attenuation(1/r*r) modulated by the angle of the light and modulated by the diffuse color
                finalColor += light.intensity *
                              (1 / (Vector3.Distance(initialIntersect.closestIntersect, light.location) * Vector3.Distance(initialIntersect.closestIntersect, light.location))) *
                              Math.Max(0, Vector3.Dot(initialIntersect.normalVector, Vector3.Normalize(light.location - initialIntersect.closestIntersect))) *
                              diffuseColor;
            }

            finalColor += new Color3(0.1f, 0.1f, 0.1f) * diffuseColor; //add diffuse lighting
            return finalColor;
        }


        //only needed for the debugging
        ConcurrentBag<Vector3> endPoints;
        private List<Light> LightsReached (Intersection source)
        {
            if (KeyBoardState.IsKeyDown(Keys.K))
                endPoints = new ConcurrentBag<Vector3> ();
            
            ShadowRay shadowRay = null;
            List<Light> finalResult = new List<Light>(); //store the lights here
            for (int j = 0; j < scene.lightSources.Count; j++) //for all lights in the scene
            {
                if (shadowRay == null) //first time in the loop
                {
                    //shoot a ray from the intersect point to the light
                    shadowRay = new ShadowRay(source.closestIntersect, Vector3.Normalize(scene.lightSources[j].location - source.closestIntersect));
                }
                else
                {
                    shadowRay.startPosition = source.closestIntersect;
                    shadowRay.direction = Vector3.Normalize(scene.lightSources[j].location - source.closestIntersect);
                }

                bool blocked = false;

                //go through all primitives to check if the light is blocked
                foreach (Primitive primitive in scene.primitives)
                {
                    Intersection intersect = primitive.Intersect(shadowRay);
                    for (int i = 0; i < intersect.intersectCount; i++)
                    {
                        float distanceToIntersect = Vector3.Distance(intersect.intersectionPoints[i], shadowRay.startPosition);
                        float distanceToLight = Vector3.Distance(source.closestIntersect, scene.lightSources[j].location);
                        if (distanceToLight > distanceToIntersect && distanceToIntersect > 0.0001f) //prevent intersection with self, and ignores intersection past the light
                        {
                            if(KeyBoardState.IsKeyDown(Keys.K))
                                endPoints.Add(intersect.closestIntersect);
                            blocked = true;
                        }

                    }

                    if (blocked) //dont check every primitive after finding one that blocks the light
                        break;

                }

                if (!blocked)
                {
                    if (scene.lightSources[j] is SpotLight)
                    {
                        SpotLight spotLight = (SpotLight)scene.lightSources[j];
                        if (spotLight.checkAngle(shadowRay.direction))
                            finalResult.Add(scene.lightSources[j]);
                    }
                    else
                    {
                        finalResult.Add(scene.lightSources[j]);
                        if (KeyBoardState.IsKeyDown(Keys.K))
                            endPoints.Add(scene.lightSources[j].location);
                    }
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
            foreach (PrimaryRay primaryRay in primaryRays) 
            {
                Intersection finalIntersect = FindFinalIntersection(primaryRay);

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

                    //Draw the mirror rays
                    if (finalIntersect.primitive.material == Materials.Reflective || finalIntersect.primitive.material == Materials.Refractive)
                        DebugMirrorRays(new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(primaryRay.direction, finalIntersect.normalVector), primaryRay.bounces - 1));

                    if (finalIntersect.primitive.material == Materials.Refractive)
                        DebugRefractionRays(primaryRay);


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
                        Vector2 vectortodraw = debugOrigin + scalar * (DebugPos(light.location) + new Vector2(x_offset, y_offset) + offset);
                        screen.Plot((int)vectortodraw.X, (int)vectortodraw.Y, light.intensity); //draw the desired pixel
                    }
                }

            }

            //debug camera
            screen.Box((int)camera.position.X + (int)debugOrigin.X, (int)-camera.position.Z + (int)debugOrigin.Y, (int)camera.position.X + 2 + (int)debugOrigin.X, (int)-camera.position.Z + 2 + (int)debugOrigin.Y, new Color3(1, 1, 0));
            //debug screenc
            screen.Line((int)(scalar * (camera.screenCorners[0].X + offset.X) + debugOrigin.X - offset.X), (int)(scalar * (-camera.screenCorners[0].Z + offset.Y) + debugOrigin.Y - offset.Y), (int)(scalar  * (camera.screenCorners[1].X + offset.X) + debugOrigin.X - offset.X), (int)(scalar * (-camera.screenCorners[1].Z + offset.Y) + debugOrigin.Y - offset.Y), new Color3(1, 1, 1));
            
        }

        private void DebugMirrorRays(PrimaryRay ray)
        {
            Vector2 debugOrigin = new Vector2(screen.width / 2 - camera.position.X, 300 + camera.position.Z);
            Vector2 offset = new Vector2(-camera.position.X, camera.position.Z);
            int scalar = 30;

            Intersection finalIntersection = FindFinalIntersection(ray);

            Vector3 endpoint = 1000 * ray.direction; //the default endpoint is VERY far away
            if (finalIntersection != null)
                endpoint = finalIntersection.closestIntersect; //make the intersection point the end point


            //translate to debug coordinates
            Vector2 debugStart = scalar * (DebugPos(ray.startPosition) + offset) + debugOrigin;
            Vector2 debugEnd = debugOrigin + scalar * (DebugPos(endpoint) + offset);


            screen.Line((int)debugStart.X, (int)debugStart.Y, (int)debugEnd.X, (int)debugEnd.Y, new Color3(1, 1, 1)); //draw the ray

            if (finalIntersection != null)
                if (finalIntersection.primitive.material == Materials.Reflective && ray.bounces > 0)
                    DebugMirrorRays(new PrimaryRay(finalIntersection.closestIntersect, Vector3.Reflect(ray.direction, finalIntersection.normalVector), ray.bounces - 1));

        }

        private void DebugRefractionRays(PrimaryRay ray)
        {
            Vector2 debugOrigin = new Vector2(screen.width / 2 - camera.position.X, 300 + camera.position.Z);
            Vector2 offset = new Vector2(-camera.position.X, camera.position.Z);
            int scalar = 30;

            Intersection finalIntersect = FindFinalIntersection(ray);

            Vector3 normalToUse = finalIntersect.normalVector;
            if (Vector3.Dot(finalIntersect.normalVector, ray.direction) > 0)
                normalToUse *= -1; //flip the normal vector for correct refraction math

            float enteringRefractionIndex = finalIntersect.primitive.refraction_index; //get the refraction index from the primitive

            float dnDot = Vector3.Dot(ray.direction, normalToUse);
            float discriminant = 1 - (ray.sourceRefractIndex * ray.sourceRefractIndex) / (enteringRefractionIndex * enteringRefractionIndex) * (1 - dnDot * dnDot);

            if (MathF.Abs(ray.sourceRefractIndex - enteringRefractionIndex) < 0.0001f && discriminant < 0) //total internal refraction
                enteringRefractionIndex = 1; //when the ray comes from inside the primitive, assume we are going into air

            //formula given in the slides
            Vector3 newDirection = Vector3.Normalize((ray.sourceRefractIndex / enteringRefractionIndex) * (ray.direction - dnDot * normalToUse) - MathF.Sqrt(discriminant) * normalToUse);


            //create a new ray, going in the new direction and set the source refraction index to the material it is currectly going into
            PrimaryRay refractionRay = new PrimaryRay(finalIntersect.closestIntersect, newDirection, ray.bounces - 1) { sourceRefractIndex = enteringRefractionIndex };

            finalIntersect = FindFinalIntersection(refractionRay);

            Vector3 endpoint = 1000 * refractionRay.direction; //the default endpoint is VERY far away
            if (finalIntersect != null)
                endpoint = finalIntersect.closestIntersect; //make the intersection point the end point

            //translate to debug coordinates
            Vector2 debugStart = scalar * (DebugPos(refractionRay.startPosition) + offset) + debugOrigin;
            Vector2 debugEnd = debugOrigin + scalar * (DebugPos(endpoint) + offset);


            screen.Line((int)debugStart.X, (int)debugStart.Y, (int)debugEnd.X, (int)debugEnd.Y, new Color3(0, 1, 0)); //draw the ray

            if (finalIntersect != null)
            {
                if ((finalIntersect.primitive.material == Materials.Reflective || finalIntersect.primitive.material == Materials.Refractive) && ray.bounces > 0)
                    DebugMirrorRays(new PrimaryRay(finalIntersect.closestIntersect, Vector3.Reflect(ray.direction, finalIntersect.normalVector), ray.bounces - 1));

                if (finalIntersect.primitive.material == Materials.Refractive)
                    DebugRefractionRays(refractionRay);
            }
        }


        private Vector2 DebugPos(Vector3 pos) 
        {
            return new Vector2(pos.X, -pos.Z); // we are looking from above, so the y-axis on the debug screen should represent the z-axis
        }

    }
}
