using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.DSM
{
    public class DSM
    {
        public int[,] matrix = null;
        public DSM(int i, int j)
        {
            matrix = new int[i, j];
        }

        public void Cluster()
        {
            throw new NotImplementedException();
        }
    }
}
