namespace MyRedis.Core
{
    /// <summary>
    /// Defines What Type of Values Redis Can Store
    /// </summary>
    public enum DataType
    {
        None,
        String,
        List,
        Hash,
        Set,
        SortedSet
    }
}