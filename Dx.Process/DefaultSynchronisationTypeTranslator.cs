using System.Collections.Generic;
using Dx.Runtime;
using Mono.Cecil;

namespace Dx.Process
{
    public class DefaultSynchronisationTypeTranslator : ISynchronisationTypeTranslator
    {
        public TypeReference GetDistributedType(TypeReference original)
        {
            var listType = original.Module.Import(typeof(List<>));
            var generic = original as GenericInstanceType;
            if (generic != null &&
                generic.ElementType.FullName == listType.FullName &&
                generic.GenericArguments.Count == 1)
            {
                // Construct DList<> reference for this.
                var distributedListType = original.Module.Import(typeof(DList<>));
                var distributedListGeneric = new GenericInstanceType(distributedListType);
                distributedListGeneric.GenericArguments.Add(generic.GenericArguments[0]);
                return distributedListGeneric;
            }
            return original;
        }
    }
}

