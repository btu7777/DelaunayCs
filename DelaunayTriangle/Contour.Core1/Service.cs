

using System.Collections.Generic;
using System.Linq;
using TriangleNet.Meshing;
using Utils.Models;

namespace Contour.Core1
{

    /* 1. 生成三角形等值线链表
     * 2. 
     */
    public class Service
    {
        IMesh _mesh;
        public Service(IMesh mesh, List<TIN_Point> points)
        {
            _mesh = mesh;
            Data.Points = points.ToDictionary(x => x.Id, y => y);
        }

    }

}