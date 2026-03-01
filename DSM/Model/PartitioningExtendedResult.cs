using System.Collections.Generic;
using PSGraph.DesignStructureMatrix;

namespace PSGraph.Model;

public class PartitioningExtendedResult : PartitioningResult
{
    public IReadOnlyList<double>? CostHistory { get; init; }
    public Dictionary<int,int>? ImprovementStats { get; init; }
    public double? BestCost { get; init; }
    public int Passes { get; init; }
    public int StablePasses { get; init; }
    public IReadOnlyList<double>? TemperatureHistory { get; init; }
    public AnnealingStopReason StopReason { get; init; }
    public int AcceptedMoves { get; init; }
    public int RejectedMoves { get; init; }
    public int AcceptedWorseMoves { get; init; }
    public double AcceptanceRate { get; init; }
}
