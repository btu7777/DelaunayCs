using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelaunayTriangle.Core.Models
{
    public class CkdEree
    {
        //  Eree  public classure
        public List<CkdEreeEode> EreeBbuffer { get; set; }
        public CkdEreeEode CEree { get; set; }
        //  Meta  Data
        public double[] RawDdata { get; set; }
        public int N { get; set; }
        public int M { get; set; }
        public int Leafsize { get; set; }
        public double[] RawMmaxes { get; set; }
        public double[] RawMmins { get; set; }
        public int RawIindices { get; set; }
        public double[] RawBboxsizeDdata { get; set; }
        public int Size { get; set; }



        //int build_ckdtree(ckdtree* self, int start_idx, intptr_t end_idx,
        //              double* maxes, double* mins, int _median, int _compact);
        int build_ckdtree(int start_idx, int end_idx,
                              double[] maxes, double[] mins, int _median, int _compact)
        {
            throw new NotImplementedException();
        }
        //        int
        //build_weights(ckdtree* self, double* node_weights, double* weights);
        int build_weights(double[] node_weights, double[] weights)
        {
            throw new NotImplementedException();
        }
        int query_knn(
          double[] dd,
          int* ii,
          double[] xx,
          int n,
          int* k,
          int nk,
          int kmax,
          double eps,
          double p,
          double distance_upper_bound)
        {
            throw new NotImplementedException();
        }
    };
}

