using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utils.Models;

namespace Contour.Core
{
    public class ContourLine
    {
        public double Elevation { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();
        public List<TIN_Point[]> Edges { get; set; } = new List<TIN_Point[]>();
        public bool IsClosed { get; set; } = false;
    }
}
