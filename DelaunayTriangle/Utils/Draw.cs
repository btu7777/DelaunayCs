using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using TriangleNet.Topology;
using Utils.Models;

namespace Utils
{
    public class Draw
    {
        Canvas canvas;
        Color Color = Colors.Green;
        Color SubColor = Colors.AliceBlue;
        public Draw(Canvas canvas)
        {
            this.canvas = canvas;
        }
        bool isScaled = false;
        public Dictionary<string, List<UIElement>> dicts = new Dictionary<string, List<UIElement>>();

        public void SetColor(Color color)
        {
            Color = color;
        }
        public void SetSubColor(Color color)
        {
            SubColor = color;
        }
        public void Reset()
        {
            isScaled = false;
            ratioX = 1;
            ratioY = 1;
            dicts = new Dictionary<string, List<UIElement>>();
        }
        public void SetRatio(IEnumerable<Point> points, bool isSetLine = true)
        {
            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxX = points.Max(p => p.X);
            double maxY = points.Max(p => p.Y);
            startX = minX;
            startY = minY;
            if (isSetLine)
                SetOriginLine();
            ratioX = canvas.ActualWidth / (maxX - minX);
            ratioY = canvas.ActualHeight / (maxY - minY);
            startX *= ratioX;
            startY *= ratioY;
            isScaled = true;
        }

        public double startX { get; private set; }
        public double startY { get; private set; }
        public double ratioX { get; private set; } = 1;
        public double ratioY { get; private set; } = 1;




        bool getOriginP(double minX, double maxX, double minY, double maxY)
        {
            if (!isScaled)
            {
                startX = minX;
                startY = minY;
                SetOriginLine();
                ratioX = canvas.ActualWidth / (maxX - minX);
                ratioY = canvas.ActualHeight / (maxY - minY);
                startX *= ratioX;
                startY *= ratioY;
            }
            isScaled = true;
            return true;
        }
        public void clean(string name)
        {
            if (dicts.ContainsKey(name))
            {
                dicts[name].ForEach(m => canvas.Children.Remove(m));
            }
        }
        void add(string name, UIElement element)
        {
            if (dicts.ContainsKey(name))
            {
                dicts[name].Add(element);
            }
            else
            {
                dicts.Add(name, new List<UIElement>() { element });
            }
        }
        void add(string name, List<UIElement> elements)
        {
            if (dicts.ContainsKey(name))
            {
                dicts[name].AddRange(elements);
            }
            else
            {
                dicts.Add(name, new List<UIElement>(elements));
            }
        }

        /// <summary>
        /// 坐标转换，确保点在Canvas内部
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        (double, double) transform(double x, double y)
        {
            return (x * ratioX - startX, y * ratioY - startY);
        }
        public Canvas drawTriangle(List<Point[]> triangles, string name = "default")
        {
            double maxX = triangles.Max(t => t.Max(p => p.X));
            double maxY = triangles.Max(t => t.Max(p => p.Y));
            double minX = triangles.Min(t => t.Min(p => p.X));
            double minY = triangles.Min(t => t.Min(p => p.Y));
            getOriginP(minX, maxX, minY, maxY);
            List<UIElement> elements = new List<UIElement>();
            triangles.ForEach(m =>
            {
                (double x0, double y0) = transform(m[0].X, m[0].Y);
                (double x1, double y1) = transform(m[1].X, m[1].Y);
                (double x2, double y2) = transform(m[2].X, m[2].Y);
                GeometryGroup group = new GeometryGroup();
                LineGeometry g0 = new LineGeometry()
                {
                    StartPoint = new Point(x0, y0),
                    EndPoint = new Point(x1, y1),
                };
                LineGeometry g1 = new LineGeometry()
                {
                    StartPoint = new Point(x1, y1),
                    EndPoint = new Point(x2, y2),
                };
                LineGeometry g2 = new LineGeometry()
                {
                    StartPoint = new Point(x2, y2),
                    EndPoint = new Point(x0, y0),
                };
                group.Children.Add(g0);
                group.Children.Add(g1);
                group.Children.Add(g1);
                PathGeometry g = new PathGeometry()
                {
                    Figures = new PathFigureCollection()
                     {
                         new PathFigure(){
                             StartPoint = new Point(x0, y0),
                             Segments=new PathSegmentCollection()
                             {
                                 new LineSegment(){Point=new Point(x1,y1)},
                                 new LineSegment(){Point=new Point(x2,y2)},
                             },
                             IsClosed = true,
                             IsFilled = true,
                         }
                     },
                };
                Path path = new Path()
                {
                    Data = g,
                    Fill = new SolidColorBrush(Color),
                    //RenderTransform = new TranslateTransform(o.X, o.Y),
                    Opacity = 0.5
                };
                elements.Add(path);
                canvas.Children.Add(path);

                //geometry.Figures.Add(pathFigure);
            });
            add(name, elements);

            return canvas;
        }
        public void drawPoints(List<TIN_Point> points,
            bool isShowIndex = false,
            bool isShowXY = false,
            bool isShowZ = false,
            int presion = 0,
            string name = "default")
        {
            List<UIElement> elements = new List<UIElement>();
            double maxX = points.Max(t => t.Point.X);
            double maxY = points.Max(t => t.Point.Y);
            double minX = points.Min(t => t.Point.X);
            double minY = points.Min(t => t.Point.Y);
            getOriginP(minX, maxX, minY, maxY);
            points.ForEach(p =>
            {
                (double x, double y) = transform(p.Point.X, p.Point.Y);
                EllipseGeometry geometry = new EllipseGeometry()
                {
                    Center = new Point(x, y),
                    RadiusX = 5,
                    RadiusY = 5,
                };
                string f = "F" + presion;
                bool b = isShowIndex || isShowXY || isShowZ;
                if (b)
                {
                    string text = (isShowIndex ? $"{p.Id}\n" : "") +
                    (isShowXY ? $"X:{p.Point.X.ToString(f)}\nY:{p.Point.Y.ToString(f)}\n" : "") +
                    (isShowZ ? $"Z:{p.Point.Z.ToString(f)}" : "");
                    TextBlock tbx = new TextBlock()
                    {
                        Text = text,
                        RenderTransform = new TranslateTransform(x, y - 15),
                        Foreground = new SolidColorBrush(SubColor),
                    };
                    canvas.Children.Add(tbx);
                    elements.Add(tbx);
                }
                Path path = new Path()
                {
                    Data = geometry,
                    Stroke = new SolidColorBrush(Color),
                    //RenderTransform = new TranslateTransform(o.X, o.Y)
                };
                canvas.Children.Add(path);
                elements.Add(path);
            });

            add(name, elements);
        }
        public void drawPoints(List<Point3D> points, bool isShowIndex = false, bool isShowXY = false,
            bool isShowZ = false, int presion = 0,
            string name = "default")
        {
            double maxX = points.Max(t => t.X);
            double maxY = points.Max(t => t.Y);
            double minX = points.Min(t => t.X);
            double minY = points.Min(t => t.Y);
            getOriginP(minX, maxX, minY, maxY);
            List<UIElement> elements = new List<UIElement>();
            int i = 0;
            points.ForEach(p =>
            {
                (double x, double y) = transform(p.X, p.Y);
                EllipseGeometry geometry = new EllipseGeometry()
                {
                    Center = new Point(x, y),
                    RadiusX = 5,
                    RadiusY = 5,
                };
                string f = "F" + presion;
                string text = (isShowIndex ? $"{i++}\n" : "") +
                (isShowXY ? $"X:{p.X.ToString(f)}\nY:{p.Y.ToString(f)}\n" : "") +
                (isShowZ ? $"Z:{p.Z.ToString(f)}" : "");
                TextBlock tbx = new TextBlock()
                {
                    Text = text,
                    RenderTransform = new TranslateTransform(x, y - 15),
                    Foreground = new SolidColorBrush(Colors.DarkRed),
                };
                Path path = new Path()
                {
                    Data = geometry,
                    Stroke = new SolidColorBrush(Colors.Red),
                    //RenderTransform = new TranslateTransform(o.X, o.Y)
                };
                canvas.Children.Add(tbx);
                canvas.Children.Add(path);
                elements.Add(tbx);
                elements.Add(path);
            });
            add(name, elements);
        }

        public void drawLine(List<Point> edges, bool isClosed = false, bool isShowIndex = false, int startIndex = 0,
            string name = "default")
        {
            double maxX = edges.Max(t => t.X);
            double maxY = edges.Max(t => t.Y);
            double minX = edges.Min(t => t.X);
            double minY = edges.Min(t => t.Y);
            getOriginP(minX, maxX, minY, maxY);
            List<Point> newEdges = edges.Select(e =>
            {
                (double x, double y) = transform(e.X, e.Y);
                return new Point(x, y);
            }).ToList();
            PathFigure figure = new PathFigure()
            {
                StartPoint = newEdges[0],
                IsClosed = isClosed
            };
            newEdges.Skip(1).ToList().ForEach(e =>
            {
                figure.Segments.Add(new LineSegment() { Point = e });
            });
            PathGeometry pathGeometry = new PathGeometry()
            {

                Figures = new PathFigureCollection() { figure },
            };
            Path path = new Path()
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush(Color)
            };
            if (newEdges.Count > 0 && isShowIndex)
            {
                TextBlock tbk = new TextBlock()
                {
                    Text = startIndex.ToString(),
                    RenderTransform = new TranslateTransform(newEdges[0].X, newEdges[0].Y),
                };
                Ellipse e = new Ellipse()
                {
                    RenderTransform = new TranslateTransform(newEdges[0].X, newEdges[0].Y - 2),
                    Width = 15,
                    Height = 15,
                    Stroke = new SolidColorBrush(Color),
                    StrokeThickness = 1
                };
                canvas.Children.Add(e);
                canvas.Children.Add(tbk);
                add(name, e);
                add(name, tbk);
            }
            canvas.Children.Add(path);
            add(name, path);
        }
        public void drawLine(List<TIN_Point[]> edges, string name = "default")
        {
            var es = edges.SelectMany(x => x).ToList();
            double maxX = es.Max(p => p.Point2D.X);
            double minX = es.Min(p => p.Point2D.X);
            double maxY = es.Max(p => p.Point2D.Y);
            double minY = es.Min(p => p.Point2D.Y);
            getOriginP(minX, maxX, minY, maxY);
            List<UIElement> elements = new List<UIElement>();
            edges.ForEach(m =>
            {
                (double x0, double y0) = transform(m[0].Point2D.X, m[0].Point2D.Y);
                (double x1, double y1) = transform(m[1].Point2D.X, m[1].Point2D.Y);
                Line line = new Line()
                {
                    X1 = x0,
                    X2 = x1,
                    Y1 = y0,
                    Y2 = y1,
                    Stroke = new SolidColorBrush(Color)
                };
                canvas.Children.Add(line);
                add(name, line);
            });
        }


        public void drawLine(List<Point[]> edges, string name = "default")
        {

            double maxX = edges.Max(t => t.Max(p => p.X));
            double maxY = edges.Max(t => t.Max(p => p.Y));
            double minX = edges.Min(t => t.Min(p => p.X));
            double minY = edges.Min(t => t.Min(p => p.Y));
            getOriginP(minX, maxX, minY, maxY);
            List<UIElement> elements = new List<UIElement>();
            edges.ForEach(m =>
            {
                (double x0, double y0) = transform(m[0].X, m[0].Y);
                (double x1, double y1) = transform(m[1].X, m[1].Y);
                Line line = new Line()
                {
                    X1 = x0,
                    X2 = x1,
                    Y1 = y0,
                    Y2 = y1,
                    Stroke = new SolidColorBrush(Color)
                };
                canvas.Children.Add(line);

                add(name, line);
            });
        }

        void SetOriginLine()
        {
            Color color = Color;
            Line line1 = new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = canvas.ActualHeight,
                Stroke = new SolidColorBrush(color)
            };
            Line line2 = new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = canvas.ActualWidth,
                Y2 = 0,
                Stroke = new SolidColorBrush(color)
            };
            Line line3 = new Line()
            {
                X1 = 0,
                Y1 = canvas.ActualHeight,
                X2 = canvas.ActualWidth,
                Y2 = canvas.ActualHeight,
                Stroke = new SolidColorBrush(color)
            };
            Line line4 = new Line()
            {
                X1 = canvas.ActualWidth,
                Y1 = 0,
                X2 = canvas.ActualWidth,
                Y2 = canvas.ActualHeight,
                Stroke = new SolidColorBrush(color)
            };
            canvas.Children.Add(line1);
            canvas.Children.Add(line2);
            canvas.Children.Add(line3);
            canvas.Children.Add(line4);
        }
    }
}
