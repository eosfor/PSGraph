public class UnorderedPair<T>
{
    public T First { get; }
    public T Second { get; }

    public UnorderedPair(T a, T b)
    {
        if (Comparer<T>.Default.Compare(a, b) <= 0)
        {
            First = a;
            Second = b;
        }
        else
        {
            First = b;
            Second = a;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is UnorderedPair<T> other &&
               EqualityComparer<T>.Default.Equals(First, other.First) &&
               EqualityComparer<T>.Default.Equals(Second, other.Second);
    }

    public override int GetHashCode()
    {
        int hash1 = First?.GetHashCode() ?? 0;
        int hash2 = Second?.GetHashCode() ?? 0;
        return hash1 ^ hash2;
    }
}