using System.Management.Automation;
using MathNet.Numerics.Data.Text;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;

namespace PSGraph.Cmdlets;

[Cmdlet(VerbsData.Export, "DSM", DefaultParameterSetName = PlainDsmParameterSet)]
public class ExportDSMCmdlet : PSCmdlet
{
    private const string PlainDsmParameterSet = "PlainDsm";
    private const string PartitionedDsmParameterSet = "PartitionedDsm";
    private const string SequencedDsmParameterSet = "SequencedDsm";

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = PlainDsmParameterSet)]
    public IDsm Dsm = null!;

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = PartitionedDsmParameterSet)]
    public PartitioningResult Result = null!;

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = SequencedDsmParameterSet)]
    public IDsm SequencedDsm = null!;

    [Parameter(Position = 1, Mandatory = false, ParameterSetName = PlainDsmParameterSet)]
    [Parameter(Position = 1, Mandatory = false, ParameterSetName = PartitionedDsmParameterSet)]
    [Parameter(Position = 1, Mandatory = false, ParameterSetName = SequencedDsmParameterSet)]
    [Parameter(Mandatory = false)]
    public string? Path;

    [Parameter(Position = 2, Mandatory = false, ParameterSetName = PlainDsmParameterSet)]
    [Parameter(Position = 3, Mandatory = false, ParameterSetName = PartitionedDsmParameterSet)]
    [Parameter(Position = 2, Mandatory = false, ParameterSetName = SequencedDsmParameterSet)]
    public DSMExportTypes Format = DSMExportTypes.TEXT;

    protected override void ProcessRecord()
    {
        var dsm = ResolveDsm();
        string result;

        switch (Format)
        {
            case DSMExportTypes.TEXT:
                result = ExportText(dsm);
                break;
            default:
                ThrowTerminatingError(CreateVisualExportMovedException());
                return;
        }

        if (MyInvocation.BoundParameters.ContainsKey(nameof(Path)))
        {
            File.WriteAllText(Path!, result);
            return;
        }

        WriteObject(result);
    }

    private IDsm ResolveDsm()
    {
        return ParameterSetName switch
        {
            PlainDsmParameterSet => Dsm,
            PartitionedDsmParameterSet => Result.Dsm,
            SequencedDsmParameterSet => SequencedDsm,
            _ => throw new InvalidOperationException($"Unknown parameter set '{ParameterSetName}'.")
        };
    }

    private static string ExportText(IDsm dsm)
    {
        using var sw = new StringWriter();
        DelimitedWriter.Write(sw, dsm.DsmMatrixView, ",");
        return sw.ToString();
    }

    private ErrorRecord CreateVisualExportMovedException()
    {
        return new ErrorRecord(
            new NotSupportedException($"Visual DSM export format '{Format}' is no longer provided by PSGraph. Use PSGraphView and Export-DSMView instead."),
            "DsmVisualExportMovedToPSGraphView",
            ErrorCategory.NotImplemented,
            Format);
    }
}
