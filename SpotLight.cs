using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Template;
using System.Diagnostics;

namespace INFOGRTemplate
{
    internal class SpotLight : Light
    {
        public Vector3 direction;
        public float angle;
        public SpotLight(Vector3 _location, Color3 _intensity, Vector3 _direction, float _angle) : base(_location, _intensity)
        {
            direction = Vector3.Normalize(_direction);
            angle = _angle;
        }

        public bool checkAngle(Vector3 ray)
        {
            // Necessities for the calculation of the radian angle
            float dot = Vector3.Dot(-ray, direction);
            float RayMag = ray.Length();
            float DirectMag = direction.Length();

            // Calculate the radian angle between the incoming shadowray and the direction of the spotlight
            float rayForm = dot / (RayMag * DirectMag);
            float rayRad = MathF.Acos(rayForm);

            // Check if said angle falls in the bounds of the spotlight's cone
            return rayRad < angle * MathF.PI / 180;
        }
    }
}
