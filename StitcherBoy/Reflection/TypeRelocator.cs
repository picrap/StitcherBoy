#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Reflection
{
    using System;
    using dnlib.DotNet;

    /// <summary>
    /// Base class to relocate complex types
    /// </summary>
    public abstract class TypeRelocator
    {
        /// <summary>
        /// Relocates the <see cref="TypeSig"/>.
        /// </summary>
        /// <param name="typeSig">The type sig.</param>
        /// <returns>A new type if it was relocated, null otherwise</returns>
        /// <exception cref="InvalidOperationException">If signature is of unknown type.</exception>
        public virtual TypeSig TryRelocateTypeSig(TypeSig typeSig)
        {
            if (typeSig == null)
                return null;

            if (typeSig is CorLibTypeSig)
                return null;

            if (typeSig is GenericInstSig)
                return TryRelocateGeneric((GenericInstSig)typeSig);

            if (typeSig is PtrSig)
                return null;

            if (typeSig is ByRefSig)
                return TryRelocateByRef((ByRefSig)typeSig);

            if (typeSig is ArraySig)
                return TryRelocateArray((ArraySig)typeSig);

            if (typeSig is SZArraySig)
                return TryRelocateSZArray((SZArraySig)typeSig);

            if (typeSig is GenericVar)
                return null; // TODO constraints

            if (typeSig is GenericMVar)
                return null; // TODO constraints

            if (typeSig is ValueTypeSig || typeSig is ClassSig)
            {
                var typeRef = typeSig.TryGetTypeRef();
                if (typeRef != null)
                    return TryRelocateTypeRef(typeRef);
                var typeDefOrRef = TryRelocateTypeDefOrRef(typeSig.ToTypeDefOrRef());
                return typeDefOrRef?.ToTypeSig();
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Tries to relocate the <see cref="ByRefSig"/>.
        /// </summary>
        /// <param name="byRefSig">The by reference sig.</param>
        /// <returns></returns>
        protected virtual TypeSig TryRelocateByRef(ByRefSig byRefSig)
        {
            var innerTypeSig = TryRelocateTypeSig(byRefSig.Next);
            if (innerTypeSig == null)
                return null;

            return new ByRefSig(innerTypeSig);
        }

        /// <summary>
        /// Tries to relocate the <see cref="GenericInstSig"/>.
        /// </summary>
        /// <param name="genericInstSig">The generic inst sig.</param>
        /// <returns></returns>
        protected virtual TypeSig TryRelocateGeneric(GenericInstSig genericInstSig)
        {
            bool relocated = false;
            var genericTypeSig = TryRelocateTypeSig(genericInstSig.GenericType) as ClassOrValueTypeSig;
            if (genericTypeSig != null)
            {
                genericInstSig.GenericType = genericTypeSig;
                relocated = true;
            }

            for (int genericParameterIndex = 0; genericParameterIndex < genericInstSig.GenericArguments.Count; genericParameterIndex++)
            {
                var genericParameterType = TryRelocateTypeSig(genericInstSig.GenericArguments[genericParameterIndex]);
                if (genericParameterType != null)
                {
                    genericInstSig.GenericArguments[genericParameterIndex] = genericParameterType;
                    relocated = true;
                }
            }

            return relocated ? genericInstSig : null;
        }

        /// <summary>
        /// Tries to relocate the <see cref="ArraySig"/>.
        /// </summary>
        /// <param name="arraySig">The array sig.</param>
        /// <returns></returns>
        protected virtual TypeSig TryRelocateArray(ArraySig arraySig)
        {
            var nextType = TryRelocateTypeSig(arraySig.Next);
            if (nextType == null)
                return null;
            return new ArraySig(nextType);
        }

        /// <summary>
        /// Tries to relocate the <see cref="SZArraySig"/>.
        /// </summary>
        /// <param name="szArraySig">The sz array sig.</param>
        /// <returns></returns>
        protected virtual TypeSig TryRelocateSZArray(SZArraySig szArraySig)
        {
            var nextType = TryRelocateTypeSig(szArraySig.Next);
            if (nextType == null)
                return null;
            return new SZArraySig(nextType);
        }

        /// <summary>
        /// Tries to relocate the type definition or reference.
        /// </summary>
        /// <param name="typeDefOrRef">The type definition or reference.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">For unknown implementations.</exception>
        protected virtual ITypeDefOrRef TryRelocateTypeDefOrRef(ITypeDefOrRef typeDefOrRef)
        {
            if (typeDefOrRef == null)
                return null;

            // no need to relocate
            var typeDef = typeDefOrRef as TypeDef;
            if (typeDef != null)
                return null;

            var typeRef = typeDefOrRef as TypeRef;
            if (typeRef != null)
                return TryRelocateTypeRef(typeRef).ToTypeDefOrRef();

            var typeSpec = typeDefOrRef as TypeSpec;
            if (typeSpec != null)
                return TryRelocateTypeSig(typeSpec.TypeSig).ToTypeDefOrRef();

            throw new InvalidOperationException($"typeDefOrRef of type {typeDefOrRef.GetType()} was unhandled");
        }

        /// <summary>
        /// Tries to relocate type reference.
        /// </summary>
        /// <param name="typeRef">The type reference.</param>
        /// <returns></returns>
        protected abstract TypeSig TryRelocateTypeRef(TypeRef typeRef);
    }
}
