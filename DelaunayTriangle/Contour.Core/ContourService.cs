using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using Utils.Models;
using Point = System.Windows.Point;

namespace Contour.Core
{
    /* 等值线算法基本思路：
     * 1. 获取包含该等值线的三角形
     * 2. 首尾依次相连（可能会出现多个环或者在边缘处断裂）多个连通分量
     */
    public class ContourService
    {
        IMesh Mesh { get; }
        /* 高度信息在points內
         */
        public ContourService(IMesh mesh, List<TIN_Point> points)
        {
            Mesh = mesh;
            Data.Points = points.ToDictionary(x => x.Id, y => y);
        }
        public ContourService(IMesh mesh, Dictionary<int, TIN_Point> points)
        {
            Mesh = mesh;
            Data.Points = points;
        }
        public List<ContourLine> GetContourLines(double elevation)
        {
            var list = GetContourLine(elevation);
            Lines.AddRange(list);
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elevation"></param>
        /// <returns></returns>
        List<ContourLine> GetContourLine(double elevation)
        {
            List<ContourLine> lines = new List<ContourLine>();
            // 所有相交的三角形
            var tris = Mesh.Triangles.Where(x => x.IsIntersectionPoints(elevation));
            dicts = tris.ToDictionary(x => x.ID, x => x);
            Triangles = tris.ToList();
            #region 非闭合曲线
            do
            {
                var keyPair = dicts.FirstOrDefault(x => x.Value.IsEdgeAndIntersectionPoint(elevation).Item1);
                if (keyPair.Value != null)
                {
                    var tri = keyPair.Value;
                    ContourLine contourLine = new ContourLine() { Elevation = elevation };
                    GetContourLine2(tri, null, ref contourLine);
                    lines.Add(contourLine);
                }
            } while (dicts.Where(x => x.Value.IsEdgeAndIntersectionPoint(elevation).Item1).Any());
            #endregion

            #region 闭合曲线
            do
            {
                var keyPair = dicts.FirstOrDefault(x => x.Value.IsIntersectionPoints(elevation));
                if (keyPair.Value != null)
                {
                    var tri = keyPair.Value;
                    ContourLine contourLine = new ContourLine() { Elevation = elevation, IsClosed = true, };
                    GetContourLine2(tri, null, ref contourLine);
                    lines.Add(contourLine);
                }

            } while (dicts.Where(x => x.Value.IsIntersectionPoints(elevation)).Any());


            #endregion
            return lines;
        }
        public List<ContourLine> Lines { get; private set; } = new List<ContourLine>();
        public List<Triangle> Triangles = new List<Triangle>();
        Dictionary<int, Triangle> dicts = new Dictionary<int, Triangle>();
        bool remove(ITriangle tri)
        {
            if (!dicts.ContainsKey(tri.ID)) return false;
            //if (dicts[tri.ID].Count > 1) dicts[tri.ID].RemoveAt(0);
            //else dicts.Remove(tri.ID);
            dicts.Remove(tri.ID);
            return true;
        }

        /* 1. 判断初始点
         * 2. 连接
         */

        /// <summary>
        /// 生成等值线
        /// </summary>
        /// <param name="tri">等值线穿过的三角面（两条边上有点）</param>
        /// <param name="before"></param>
        /// <param name="elevation"></param>
        /// <param name="v1"></param>
        void GetContourLine2(ITriangle tri, ITriangle before, ref ContourLine line)
        {
            double elevation = line.Elevation;
            if (tri == null || !dicts.ContainsKey(tri.ID)) return;
            if (!((Triangle)tri).IsIntersectionPoints(line.Elevation))
                throw new ArgumentOutOfRangeException("错误!");
            int j = -1;
            List<ITriangle> triangles = new List<ITriangle> { tri.GetNeighbor(0), tri.GetNeighbor(1), tri.GetNeighbor(2) };
#if DEBUG

            TIN_Point p00 = Data.Points[tri.GetVertexID(0)];
            TIN_Point p01 = Data.Points[tri.GetVertexID(1)];
            TIN_Point p02 = Data.Points[tri.GetVertexID(2)];
            List<TIN_Point> ps = new List<TIN_Point>() { p00, p01, p02 };
            if (ps.FindIndex(x => x.Id == 191) != -1)
            {

            }
#endif
            if (before != null)
            {
                int i = triangles.FindIndex(x => x != null && x.ID == before.ID);
                if (i == -1)
                    throw new ArgumentException("不存在");
                TIN_Point p0 = Data.Points[tri.GetVertexID(i)];
                TIN_Point p1 = Data.Points[tri.GetVertexID((i + 1) % 3)];
                TIN_Point p2 = Data.Points[tri.GetVertexID((i + 2) % 3)];

                if ((p0.Height - elevation) * (p2.Height - elevation) <= 0)
                {
                    (double x, double y) = Data.InterpolationMethods(p0, p2, elevation);
                    line.Edges.Add(new TIN_Point[] { p0, p2 });
                    Point point = new Point(x, y);
                    // 
                    j = (i + 1) % 3;
                    line.Points.Add(point);
                }
                else if ((p0.Height - elevation) * (p1.Height - elevation) <= 0)
                {
                    (double x, double y) = Data.InterpolationMethods(p0, p1, elevation);
                    line.Edges.Add(new TIN_Point[] { p0, p1 });
                    Point point = new Point(x, y);
                    line.Points.Add(point);
                    j = (i + 2) % 3;
                }
            }
            else
            {
                // 新增两点
                Triangle triangle = (Triangle)tri;
                (bool b, int i) = triangle.IsEdgeAndIntersectionPoint(elevation);
                if (b)
                {
                    // 边缘三角形
                    TIN_Point p0 = Data.Points[tri.GetVertexID((i) % 3)];
                    TIN_Point p1 = Data.Points[tri.GetVertexID((i + 1) % 3)];
                    TIN_Point p2 = Data.Points[tri.GetVertexID((i + 2) % 3)];
                    (double x, double y) = Data.InterpolationMethods(p1, p2, elevation);
                    line.Points.Add(new Point(x, y));
                    if ((p0.Height - elevation) * (p1.Height - elevation) <= 0)
                    {
                        (x, y) = Data.InterpolationMethods(p0, p1, elevation);
                        line.Edges.Add(new TIN_Point[] { p0, p1 });
                        line.Points.Add(new Point(x, y));
                        // p2
                        j = (i + 2) % 3;
                    }
                    else if ((p0.Height - elevation) * (p2.Height - elevation) <= 0)
                    {
                        (x, y) = Data.InterpolationMethods(p0, p2, elevation);
                        line.Edges.Add(new TIN_Point[] { p0, p2 });
                        line.Points.Add(new Point(x, y));
                        // p1
                        j = (i + 1) % 3;
                    }
                }
                else
                {
                    // 普通三角形
                    List<bool> bs = new List<bool> { false, false, false };
                    for (int ii = 0; ii < 3; ii++)
                    {
                        int p = ii, q = (ii + 1) % 3, k = (ii + 2) % 3;
                        TIN_Point p0 = Data.Points[tri.GetVertexID(ii)];
                        TIN_Point p1 = Data.Points[tri.GetVertexID((ii + 1) % 3)];
                        TIN_Point p2 = Data.Points[tri.GetVertexID((ii + 2) % 3)];
                        // 
                        if ((p2.Height - elevation) * (p1.Height - elevation) <= 0)
                        {
                            (double x, double y) = Data.InterpolationMethods(p2, p1, elevation);
                            line.Edges.Add(new TIN_Point[] { p1, p2 });
                            Point point = new Point(x, y);
                            line.Points.Add(point);
                            bs[ii] = true;
                        }
                    }
                    var index = bs.FindLastIndex(x => x);
                    if (index != -1) j = index;

                }
            }
            dicts.Remove(tri.ID);
            GetContourLine2(tri.GetNeighbor(j), tri, ref line);
        }
        void GetContourLine(ITriangle tri, ITriangle before, double elevation, ref List<int[]> v1)
        {
            if (!dicts.ContainsKey(tri.ID)) return;
            for (int i = 0; i < 3; i++)
            {
                var p0 = Data.Points[tri.GetVertexID(i)];
                var p1 = Data.Points[tri.GetVertexID((i + 1) % 3)];
                if ((p0.Height - elevation) * (elevation - p1.Height) >= 0)
                {
                    v1.Add(new int[] { p0.Id, p1.Id });

                    if (!remove(tri)) { };
                    var nearTris = new List<ITriangle>()
                    {
                        tri.GetNeighbor(0),
                        tri.GetNeighbor(1),
                        tri.GetNeighbor(2),
                    };
                    var nearTriIndex = nearTris.FindIndex(x => x != null && dicts.ContainsKey(x.ID) && ((before != null && before.ID != x.ID) || before == null));

                    if (nearTriIndex != -1)
                    {
                        GetContourLine(nearTris[nearTriIndex], tri, elevation, ref v1);
                    }

                    //if (nearTri != null && (before == null || (before != null && nearTri.ID != before.ID)))
                    //{
                    //    GetContourLine(nearTri, tri, elevation, ref v1);
                    //}
                }
            }
        }

    }

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
