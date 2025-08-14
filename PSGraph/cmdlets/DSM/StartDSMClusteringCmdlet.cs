using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using System.Collections;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Start, "DSMClustering")]
    public class StartDSMClusteringCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public IDsm? Dsm;

        [Parameter(Mandatory = false)]
        public ClusteringAlgorithmOptions ClusteringAlgorithm = ClusteringAlgorithmOptions.Classic;

        // Generic configuration object (Hashtable / PSObject / strongly-typed record)
        // The cmdlet will coerce it to the appropriate algorithm config type at runtime.
        [Parameter(Mandatory = false)]
        public PSObject? AlgorithmConfig { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Detailed;

        protected override void ProcessRecord()
        {
            if (Dsm == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentNullException(nameof(Dsm)), "NullDsm", ErrorCategory.InvalidArgument, null));
            }
            IDsm ret;
            IDsmPartitionAlgorithm algo;
            switch (ClusteringAlgorithm)
            {
                default:
                case ClusteringAlgorithmOptions.Classic:
                    var saCfg = CoerceConfig<DsmSimulatedAnnealingConfig>();
                    algo = new DsmSimulatedAnnealingAlgorithm(Dsm, saCfg);
                    break;
                case ClusteringAlgorithmOptions.GraphBased:
                    var gbCfg = CoerceConfig<DsmGraphPartitioningConfig>();
                    algo = new DsmGraphPartitioningAlgorithm(Dsm, gbCfg);
                    break;
            }

            if (Detailed.IsPresent)
            {
                var extended = algo.PartitionWithDetails();
                WriteObject(extended);
            }
            else
            {
                ret = algo.Partition();
                var result = new PartitioningResult() { Dsm = ret, Algorithm = algo };
                WriteObject(result);
            }
        }

        private T? CoerceConfig<T>() where T : class
        {
            if (AlgorithmConfig == null) return null;

            // Direct instance
            if (AlgorithmConfig.BaseObject is T tDirect) return tDirect;

            // Hashtable / IDictionary path
            if (AlgorithmConfig.BaseObject is IDictionary dict)
            {
                var built = TryBuildRecordFromDictionary<T>(dict);
                if (built != null) return built;
            }

            // PSCustomObject properties path
            var propCollection = AlgorithmConfig.Properties; // PSMemberInfoCollection<PSPropertyInfo>
            if (propCollection != null && propCollection.GetEnumerator().MoveNext())
            {
                var dictLike = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in propCollection)
                {
                    if (p != null)
                        dictLike[p.Name] = p.Value;
                }
                var built = TryBuildRecordFromDictionary<T>(dictLike);
                if (built != null) return built;
            }

            // Last resort: PowerShell conversion
            try
            {
                var converted = LanguagePrimitives.ConvertTo(AlgorithmConfig, typeof(T)) as T;
                if (converted != null) return converted;
            }
            catch { }

            WriteWarning($"AlgorithmConfig could not be converted to {typeof(T).Name}; defaults will be used.");
            return null;
        }

        private T? TryBuildRecordFromDictionary<T>(IDictionary dict) where T : class
        {
            try
            {
                var targetType = typeof(T);
                // Choose primary constructor (record primary or the one with most params)
                var ctor = targetType.GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
                if (ctor == null) return null;

                var ctorParams = ctor.GetParameters();
                var args = new object?[ctorParams.Length];
                for (int i = 0; i < ctorParams.Length; i++)
                {
                    var p = ctorParams[i];
                    object? value = null;
                    // Case-insensitive lookup in supplied dictionary
                    foreach (var key in dict.Keys)
                    {
                        if (key is string sk && sk.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            value = dict[key];
                            break;
                        }
                    }
                    if (value != null)
                    {
                        try { value = LanguagePrimitives.ConvertTo(value, p.ParameterType); } catch { }
                        args[i] = value;
                    }
                    else if (p.HasDefaultValue)
                    {
                        args[i] = p.DefaultValue;
                    }
                    else
                    {
                        args[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                    }
                }
                return ctor.Invoke(args) as T;
            }
            catch { return null; }
        }
    }
}
