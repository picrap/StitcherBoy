// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

namespace StitcherBoy.Reflection
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;

    /// <summary>
    /// Equality comparer for assembly name
    /// </summary>
    public class AssemblyEqualityComparer : IEqualityComparer<IAssembly>
    {
        private readonly AssemblyNameMatchingLevel _level;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyEqualityComparer"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        public AssemblyEqualityComparer(AssemblyNameMatchingLevel level = AssemblyNameMatchingLevel.Name | AssemblyNameMatchingLevel.Version | AssemblyNameMatchingLevel.Key)
        {
            _level = level;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type to compare.</param>
        /// <param name="y">The second object of type to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(IAssembly x, IAssembly y)
        {
            if (x.Name != y.Name)
                return false;
            if (_level.HasFlag(AssemblyNameMatchingLevel.Version) && x.Version != y.Version)
                return false;
            if (_level.HasFlag(AssemblyNameMatchingLevel.Key))
            {
                var xn = new AssemblyName(x.FullNameToken);
                var yn = new AssemblyName(y.FullNameToken);
                var xp = xn.GetPublicKeyToken();
                var yp = yn.GetPublicKeyToken();
                if (xp == null)
                {
                    if (yp != null)
                        return false;
                }
                else
                {
                    if (yp == null)
                        return false;
                    if (!xp.SequenceEqual(yp))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="a">the assembly.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(IAssembly a)
        {
            var h = a.GetHashCode();
            if (_level.HasFlag(AssemblyNameMatchingLevel.Version))
                h ^= a.Version.GetHashCode();
            if (_level.HasFlag(AssemblyNameMatchingLevel.Key))
                h ^= a.Version.GetHashCode();
            return h;
        }
    }
}
