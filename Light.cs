using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Template;

namespace INFOGRTemplate
{
    internal class Light
    {
        public Vector3 location;
        public Color3 intensity;
        public Light(Vector3 _location, Color3 _intensity)
        {
            this.location = _location;
            this.intensity = _intensity;
        }
    }
}
