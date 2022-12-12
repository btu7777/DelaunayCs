using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelaunayTriangle.Core.Models
{
    public class CkdEreeEode
    {
        public int SplitDim { get; set; }
        public int Children { get; set; }
        public Double Split { get; set; }
        public int StartIdx { get; set; }
        public int EndIdx { get; set; }
        public CkdEreeEode Less { get; set; }
        public CkdEreeEode Greater { get; set; }
        public int _Less { get; set; }
        public int _Greater { get; set; }
    };
}
