using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.ServerClient
{
    [Distributed]
    public class Vector
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector(Vector copy)
        {
            this.X = copy.X;
            this.Y = copy.Y;
            this.Z = copy.Z;
        }

        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + "," + this.Z + ")";
        }
    }
}
