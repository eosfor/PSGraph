using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.DesignStructureMatrix
{
    public class DSMCluster
    {

        private double _pow_bid = 0.5;
        private int _pow_dep = 2;
        private int _pow_cc = 1;

        public List<PSVertex> Objects = new List<PSVertex>();
        public System.Drawing.Color Color = System.Drawing.Color.FromArgb(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255));

        public List<System.ValueTuple<double, double, PSVertex>> TccHistory { get; private set; }

        public HashSet<DSMCluster> GetOutEdges(List<DSMCluster> clusters, Dsm dsm)
        {
            var ret = new HashSet<DSMCluster>();
            foreach (var item in Objects)
            {
                var column = dsm.Column(item);
                foreach (var i in dsm.ColIndex)
                {
                    if (! Objects.Contains(i.Key))
                    {
                        if (dsm[i.Key, item] == 1)
                        {
                            ret.Add(dsm.FindDsmObjectCluster(i.Key));
                        }
                    }
                }
            }

            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(",", (object[])Objects.ToArray());

            return sb.ToString();
            //return base.ToString();
        }

        public double CoordinationCost(PSVertex obj, DsmStorage dsm)
        {
            double intraCost = 0;
            double extraCost = 0;
            if (Objects.Contains(obj))
            {
                // calculate intra cost
                intraCost = CalculateIntraClusterCost(obj, dsm);
            }
            else
            {
                //calculate extra cost
                extraCost = CalculateExtraClusterCost(obj, dsm);
            }

            TccHistory.Add((intraCost, extraCost, obj));
            return intraCost + extraCost;
        }

        private double CalculateExtraClusterCost(PSVertex obj, DsmStorage dsm)
        {
            double extraCost = 0;

            foreach (var o in Objects)
            {
                extraCost += dsm[obj, o] + dsm[o, obj];
            }

            return extraCost * Math.Pow(dsm.Size, _pow_cc);
        }

        private double CalculateIntraClusterCost(PSVertex obj, DsmStorage dsm)
        {
            double intraCost = 0;

            foreach (var o in Objects)
            {
                intraCost += dsm[obj, o] + dsm[o, obj];
            }

            return intraCost * Math.Pow(Objects.Count, _pow_cc);
        }

        public double Bid(PSVertex obj, DsmStorage dsm)
        {
            double bid = 0.0;
            foreach (var dsmObject in Objects)
            {
                if (dsmObject != obj) // avoid diagonal elements
                {
                    var a = dsm[obj, dsmObject];
                    var b = dsm[dsmObject, obj];
                    bid += Math.Pow(a + b, _pow_dep) / Math.Pow(Objects.Count, _pow_bid);
                }
            }

            return bid;
        }

        public override bool Equals(object? obj)
        {
            bool res = false;

            if (null != obj)
            {
                if (((DSMCluster)obj).Objects.Count == Objects.Count)
                {
                    foreach (var item in ((DSMCluster)obj).Objects)
                    {
                        if (Objects.Contains(item)) { res = true; break; }
                        else { res = false; }
                    }
                }

            }

            return obj is DSMCluster cluster && res;
        }

        public override int GetHashCode()
        {
            return Objects.GetHashCode();
        }

        #region contructors
        public DSMCluster()
        {
            TccHistory = new List<System.ValueTuple<double, double, PSVertex>>();
        }
        #endregion cosntructors
    }
}
