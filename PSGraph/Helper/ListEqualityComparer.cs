using PSGraph.Model;

namespace PSGraph.DesignStructureMatrix;

// https://stackoverflow.com/questions/73495011/hashset-of-lists
public class ListEqualityComparer : IEqualityComparer<List<PSVertex>>
{
    public bool Equals(List<PSVertex> x, List<PSVertex> y)
    {
        if (x is null && y is null)
        {
            return true;
        }
        else if (x is null || y is null)
        {
            return false;
        }
        
        if (x.Count != y.Count)
        {
            return false;
        }
        
        return x.SequenceEqual(y);
    }
    

    // taken from https://stackoverflow.com/a/8094931/1165998
    public int GetHashCode(List<PSVertex> obj)
    {
        unchecked
        {
            int hash = 19;
            foreach (var item in obj)
            {
                hash = hash * 31 + item.GetHashCode();
            }
            return hash;
        }
    }
}