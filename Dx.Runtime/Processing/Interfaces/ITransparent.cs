namespace Dx.Runtime
{
    /// <summary>
    /// Defines required information for distributed objects.  The post-processor
    /// will implement this interface for you.
    /// </summary>
    public interface ITransparent
    {
        string NetworkName { get; set; }
        ILocalNode Node { get; set; }
    }
}
