using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelaunayTriangle.Core.Models
{

    public class WeightedTree
    {
        public CkdEree Eree { get; set; }
        public Double Weights { get; set; }
        public Double EodeWweights { get; set; }
    };

    public class CNBParams
    {
        public Double r { get; set; }
        public Action Results { get; set; }  /*  Will  Be  Casted  Inside  */
        public WeightedTree Self { get; set; }
        public WeightedTree Other { get; set; }
        public int Cumulative { get; set; }
    };

}
