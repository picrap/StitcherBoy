namespace StitcherBoy.Project
{
    using System.Collections.Generic;

    /// <summary>
    /// References interface
    /// </summary>
    public interface IReferences
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
