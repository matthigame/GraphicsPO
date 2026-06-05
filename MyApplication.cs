using INFOGRTemplate;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Globalization;

namespace Template
{
    class MyApplication
    {
        // member variables
        public Surface screen;
        private readonly Stopwatch timer = new();
        public Raytracer raytracer;
        public KeyboardState KeyBoardState { get; set; }
        public MouseState MouseState { get; set; }
        // constructor
        public MyApplication(Surface screen)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.screen = screen;
        }
        // initialize
        public void Init()
        {
            raytracer = new Raytracer(screen);
            // (optional) example of how you can load a triangle mesh in any file format supported by Assimp
            object? mesh = Util.ImportMesh("../../../assets/cube.obj");
        }
        // tick: renders one frame
        private TimeSpan deltaTime = new();
        private uint frames = 0;
        private string timeString = "---- ms/frame";
        public void Tick()
        {
            timer.Restart();

            screen.Clear(0);

            raytracer.KeyBoardState = KeyBoardState;
            raytracer.MouseState = MouseState;

            for (int row = 0; row < screen.height; row++)
            {
                for (int column = 0; column < screen.width; column++)
                {
                    // REPLACE THIS WITH THE CORRECT COLOR FOR THIS PIXEL FROM YOUR RAY TRACER
                    screen.Plot(column, row, new Color3(0.5f, 0.5f, 0.5f));
                }
            }

            raytracer.Render(); //render every tick

            deltaTime += timer.Elapsed;
            frames++;
            if (deltaTime.TotalSeconds > 1)
            {
                timeString = (deltaTime.TotalMilliseconds / frames).ToString("F1") + " ms/frame";
                frames = 0;
                deltaTime = TimeSpan.Zero;
            }


            screen.PrintOutlined(timeString, 2, 2, Color4.White);
        }
    }
}