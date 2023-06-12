using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using TriangleNet.Geometry;
using Point = System.Windows.Point;

namespace Utils.Models
{
    class Data
    {
        public static int Index { get; set; } = 0;
    }
    /// <summary>
    /// 点
    /// </summary>
    public class TIN_Point
    {
        public TIN_Point(Point3D point) : this(point.X, point.Y, point.Z)
        {
        }
        public TIN_Point(double x, double y, double z)
        {
            Point2D = new Point(x, y);
            Height = z;
            Id = Data.Index++;
            Vertex = new Vertex()
            {
                ID = Id,
                X = x,
                Y = y,
            };
        }

        public int Id { get; }
        public Vertex Vertex { get; }
        public Point Point2D { get; }
        public double Height { get; set; }
        public Point3D Point
        {
            get { return new Point3D(Point2D.X, Point2D.Y, Height); }
        }

        public static void Clear()
        {
            Data.Index = 0;
        }
        public static Point GetPoint(Vertex vertex)
        {
            return new Point(vertex.X, vertex.Y);
        }

    }
}
