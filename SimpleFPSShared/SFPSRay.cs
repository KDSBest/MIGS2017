using System;
using System.Collections.Generic;

namespace SimpleFPSShared
{
    public class SFPSRay
    {
        public SFPSVector3 Origin;

        public SFPSVector3 Direction;

        public SFPSRay()
        {
            Origin = new SFPSVector3();
            Direction = new SFPSVector3();
        }

        public SFPSRay(SFPSVector3 origin, SFPSVector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(Origin.Serialize());
            data.AddRange(Direction.Serialize());

            return data.ToArray();
        }

        public int Deserialize(byte[] data, int offset)
        {
            offset = Origin.Deserialize(data, offset);
            offset = Direction.Deserialize(data, offset);

            return offset;
        }

        // Intersects ray r = p + td, |d| = 1, with sphere s and, if intersecting, 
        // returns t value of intersection and intersection point q 
        public bool IntersectSphere(SFPSVector3 center, float radius, out float time, out SFPSVector3 point)
        {
            Direction = Direction.Normalize();
            time = -1;
            point = null;
            SFPSVector3 m = Origin - center;
            float b = SFPSVector3.Dot(m, Direction);
            float c = m.MagnitudeSquared() - radius * radius;

            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
            if (c > 0.0f && b > 0.0f)
                return false;

            float discr = b * b - c;

            // A negative discriminant corresponds to ray missing sphere 
            if (discr < 0.0f)
                return false;

            // Ray now found to intersect sphere, compute smallest t value of intersection
            time = -b - (float)Math.Sqrt(discr);

            // If t is negative, ray started inside sphere so clamp t to zero 
            if (time < 0.0f)
                time = 0.0f;

            point = Origin + Direction * time;

            return true;
        }
    }
}