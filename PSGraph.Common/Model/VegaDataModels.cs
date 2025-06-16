namespace PSGraph.Model.VegaDataModels;

public interface IGraphRecord
{
    public int id { get; set; }
    public string name { get; set; }
}
public class GraphRootRecord: IGraphRecord
{
    public int id { get;  set; }
    public string name { get ; set; }
}

public class GraphRecord : GraphRootRecord
{
    public int parent;
}

public record NodeRecord(string name, int group, int index);
public record LinkRecord(int source, int target, int value);