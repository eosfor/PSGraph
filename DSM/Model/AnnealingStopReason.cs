namespace PSGraph.Model;

public enum AnnealingStopReason
{
    None = 0,
    TemperatureDepleted = 1,
    StableLimitReached = 2,
    MaxRepeatReached = 3,
    EmptyGraph = 4
}
