namespace StitcherBoy.Weaving
{
    using System.Reflection;

    internal class Stitcher
    {
        public MethodInfo EntryMethod { get; }

        public Stitcher(MethodInfo entryMethod)
        {
            EntryMethod = entryMethod;
        }
    }
}