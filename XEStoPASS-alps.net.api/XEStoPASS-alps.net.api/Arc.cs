namespace XEStoPASS_alps.net.api
{
    public class Arc
{
    public string Source { get; }

    public string Target { get; }

    public Arc(string source, string target)
    {
        Source = source;
        Target = target;
    }
}
}