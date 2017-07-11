﻿using System;
using System.Collections.Generic;
using ExcelDataReader;

namespace ExcelMapper.Mappings.Items
{
    internal class MapStringValueMappingItem<T> : ISinglePropertyMappingItem
    {
        public IReadOnlyDictionary<string, T> Mapping { get; }
        public IEqualityComparer<string> Comparer { get; }

        public MapStringValueMappingItem(IDictionary<string, T> mapping, IEqualityComparer<string> comparer)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException();
            }

            Mapping = new Dictionary<string, T>(mapping, comparer);
            Comparer = comparer;
        }

        public PropertyMappingResult GetProperty(ExcelSheet sheet, int rowIndex, IExcelDataReader reader, int columnIndex, string stringValue)
        {
            if (!Mapping.TryGetValue(stringValue, out T result))
            {
                return PropertyMappingResult.Invalid();
            }

            return PropertyMappingResult.Success(result);
        }
    }
}
