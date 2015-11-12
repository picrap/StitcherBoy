namespace StitcherBoy.Project
{
    using System.Collections.Generic;

    public  interface IReferences
    {
        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        IEnumerable<AssemblyReference> References { get; }
    }
}
