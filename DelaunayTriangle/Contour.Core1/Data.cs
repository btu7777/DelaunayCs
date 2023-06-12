using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using Utils.Models;

namespace Contour.Core1
{
    public static class Data
    {
        public static Dictionary<int, TIN_Point> Points { get; set; } = new Dictionary<int, TIN_Point>();

        /// <summary>
        /// 该三角形是否有等值线穿过
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static bool IsIntersectionPoints(this Triangle triangle, double h)
        {
            if (triangle == null) return false;
            try
            {
                TIN_Point[] points = new TIN_Point[3] { Points[triangle.GetVertexID(0)], Points[triangle.GetVertexID(1)], Points[triangle.GetVertexID(2)], };

                double max = points.Max(x => x.Height);
                double min = points.Min(x => x.Height);
                return h >= min && h <= max;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 边缘三角面
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static (bool, int) IsEdgeAndIntersectionPoint(this Triangle triangle, double h)
        {
            int i = -1;
            bool b = triangle.IsIntersectionPoints(h);
            if (!b) return (false, -1);
            if (triangle.GetNeighbor(0) == null) i = 0;
            else if (triangle.GetNeighbor(1) == null) i = 1;
            else if (triangle.GetNeighbor(2) == null) i = 2;
            if (i == -1) return (false, -1);
            var v1 = triangle.GetVertex((i + 1) % 3);
            var v2 = triangle.GetVertex((i + 2) % 3);
            return ((Points[v1.ID].Height - h) * (Points[v2.ID].Height - h) <= 0, i);
        }


        public static bool IsEdge(this Triangle triangle)
        {
            if (triangle.GetNeighbor(0) == null) return true;
            else if (triangle.GetNeighbor(1) == null) return true;
            else if (triangle.GetNeighbor(2) == null) return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static Vertex[] GetIntersectionPoints(this Triangle triangle, double h)
        {
            Vertex[] vertices = new Vertex[2] { null, null };
            try
            {
                List<TIN_Point> points = new List<TIN_Point> { Points[triangle.GetVertexID(0)], Points[triangle.GetVertexID(1)], Points[triangle.GetVertexID(2)], };
                // 0 1 2
                points.Sort((a, b) => (int)(a.Height - b.Height));
                (double x1, double y1) = InterpolationMethods(points[0].Point2D.X, points[0].Point2D.Y, points[0].Height,
                        points[2].Point2D.X, points[2].Point2D.Y, points[2].Height, h);
                double x2, y2;
                if (points[1].Height >= h) // (0,2) (1,2)
                {
                    (x2, y2) = InterpolationMethods(points[1].Point2D.X, points[1].Point2D.Y, points[1].Height,
                        points[2].Point2D.X, points[2].Point2D.Y, points[2].Height, h);
                }
                else // (0,2) (0,1)
                {
                    (x2, y2) = InterpolationMethods(points[0].Point2D.X, points[0].Point2D.Y, points[0].Height,
                        points[1].Point2D.X, points[1].Point2D.Y, points[1].Height, h);
                }
                vertices[0] = new Vertex() { X = x1, Y = y1 };
                vertices[1] = new Vertex() { X = x2, Y = x2 };
            }
            catch (Exception)
            {

            }
            return vertices;
        }
        /// <summary>
        /// 插值法
        /// </summary>
        /// <returns></returns>
        static (double, double) InterpolationMethods(double x1, double y1, double z1, double x2, double y2, double z2, double z0)
        {
            Vector3 v1 = new Vector3((float)x1, (float)y1, (float)z1);
            Vector3 v2 = new Vector3((float)x2, (float)y2, (float)z2);
            var v = v2 - v1;
            if (z1 == z2 && (z0 != z1 || z0 != z2)) return (float.NaN, float.NaN);
            else if (z1 == z2) return ((x1 + x2) / 2, (y1 + y2) / 2);
            double k = (z0 - z1) / (z2 - z1);
            return (k * v.X + x1, k * v.Y + y1);
        }
        public static (double, double) InterpolationMethods(TIN_Point p1, TIN_Point p2, double z0)
        {
            return InterpolationMethods(p1.Point2D.X, p1.Point2D.Y, p1.Height,
                p2.Point2D.X, p2.Point2D.Y, p2.Height, z0);
        }
    }
}
