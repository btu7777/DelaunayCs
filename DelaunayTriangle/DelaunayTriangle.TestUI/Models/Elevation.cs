//using MathNet.Spatial.Triangulation;
using Prism.Mvvm;
using System.Windows.Media;
namespace DelaunayTriangle.TestUI.Models
{
    public class Elevation : BindableBase
    {
        /// <summary>
        /// 类型 
        /// SubElevation:0
        /// MainElevation:1
        /// </summary>
        public int Type { get; set; }
        public double Height { get; set; }
        public Color Color { get; set; }
    }


}
