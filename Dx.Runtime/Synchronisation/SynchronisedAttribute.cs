using System;

namespace Dx.Runtime
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SynchronisedAttribute : Attribute
    {
        public Type StrategyType { get; set; }

        public SynchronisedAttribute()
        {
            this.StrategyType = typeof(AssignmentStrategy);
        }

        public SynchronisedAttribute(Type strategyType)
        {
            if (strategyType == null)
                strategyType = typeof(AssignmentStrategy);
            this.StrategyType = strategyType;
        }
    }
}

