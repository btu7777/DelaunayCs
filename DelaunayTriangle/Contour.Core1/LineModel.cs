using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Models;

namespace Contour.Core1
{
    internal class LineModel
    {

        public double Elevation { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();
        public List<TIN_Point[]> Edges { get; set; } = new List<TIN_Point[]>();
        public bool IsClosed { get; set; } = false;
    }
}
