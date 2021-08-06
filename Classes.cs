using System;
using System.Collections.Generic;
using System.Text;

namespace Droneboi_Server
{
    public enum Team
    {
        Green,
        Red,
        Blue,
        Yellow,
        None
    }
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float _x, float _y, float _z)
        {
            x = _x; y = _y; z = _z;
        }
    }
    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float _x, float _y)
        {
            x = _x; y = _y;
        }
    }
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public Quaternion(float _x, float _y, float _z, float _w)
        {
            x = _x; y = _y; z = _z; w = _w;
        }
    }
}
