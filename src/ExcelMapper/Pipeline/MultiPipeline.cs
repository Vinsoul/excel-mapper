﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelMapper.Pipeline.Items;
using ExcelMapper.Utilities;

namespace ExcelMapper.Pipeline
{
    public abstract class MultiPipeline<T, TElement> : Pipeline<TElement>
    {
        private EnumerableType Type { get; }
        private Func<ICollection<TElement>> Factory { get; }

        public MultiPipeline(int capacity, MemberInfo member) : base(member)
        {
            if (typeof(Array).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()))
            {
                Type = EnumerableType.Array;
                Factory = () => new List<TElement>(capacity);
            }
            else if (typeof(T) == typeof(IEnumerable<TElement>) || typeof(T) == typeof(ICollection<TElement>))
            {
                Type = EnumerableType.Interface;
                Factory = () => new List<TElement>(capacity);
            }
            else if (typeof(T).ImplementsInterface(typeof(ICollection<TElement>)))
            {
                Type = EnumerableType.ConcreteType;
                Factory = () => (ICollection<TElement>)Activator.CreateInstance<T>();
            }
            else
            {
                throw new ExcelMappingException();
            }

            EmptyFallback = new FixedValuePipelineItem<TElement>(default(TElement));
            InvalidFallback = new ThrowIfStatusPipelineItem<TElement>(PipelineStatus.Invalid);
        }

        protected object CompletePipeline(IEnumerable<string> stringValues)
        {
            ICollection<TElement> elements = Factory();

            foreach (string stringValue in stringValues)
            {
                if (stringValue == null)
                {
                    if (EmptyFallback != null)
                    {
                        var result = new PipelineResult<TElement>(PipelineStatus.Empty, stringValue, default(TElement));
                        result = EmptyFallback.TryMap(result);
                        elements.Add(result.Result);
                    }
                    else
                    {
                        elements.Add(default(TElement));
                    }
    
                    continue;
                }

                try
                {
                    TElement element = (TElement)Convert.ChangeType(stringValue, typeof(TElement));
                    elements.Add(element);
                }
                catch when (InvalidFallback != null)
                {
                    var result = new PipelineResult<TElement>(PipelineStatus.Invalid, stringValue, default(TElement));
                    result = InvalidFallback.TryMap(result);
                    elements.Add(result.Result);
                }
            }

            if (Type == EnumerableType.Array)
            {
                return elements.ToArray();
            }

            return elements;
        }
    }
}
