using System;
using System.Collections.Generic;
using System.Text;

namespace INFOGRTemplate
{
    internal class Scene
    {
        public List<Primitive> primitives;
        public List<Light> lightSources;
        public Scene(List<Primitive> _primitives, List<Light> _lights) 
        { 
            primitives = _primitives;
            lightSources = _lights;
        }
    }
}
