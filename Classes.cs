using System;
using System.Collections.Generic;
using System.Text;

namespace Droneboi_Server
{
    public class Vehicle
    {
        public string veh_key;
        public List<Part> parts;
        public List<Block> thrusters, momentumWheels, connectors, solarPanels, miningLasers;

        public Vehicle()
        {
            parts = new List<Part>();
            thrusters = new List<Block>();
            momentumWheels = new List<Block>();
            connectors = new List<Block>();
            solarPanels = new List<Block>();
            miningLasers = new List<Block>();
        }
    }
    public class Part
    {
        public Vector2 position;
        public float eulerAnglesZ;
        public Vector2 velocity;
        public float angularVelocity;
    }
    public class Block
    {
        public int health;
        public bool isPlaying, powered, charging, drilling;
    }
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
