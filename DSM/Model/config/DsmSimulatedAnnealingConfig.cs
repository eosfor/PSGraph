namespace PSGraph.DesignStructureMatrix;

public class DsmSimulatedAnnealingConfig: IAlgorithmConfig
{
    public int PowCc = 1;
    public int PowBid = 0;
    public int PowDep = 0;
    public int Times = 2;
    public int StableLimit = 2;
    public int MaxRepeat = 1000;
    public double? InitialTemperature = null;
    public double CoolingRate = 0.95;
    public double MinTemperature = 1e-3;
}
