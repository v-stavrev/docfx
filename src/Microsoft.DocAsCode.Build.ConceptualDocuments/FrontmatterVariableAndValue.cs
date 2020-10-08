using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    struct FrontmatterVariableAndValue : IEquatable<FrontmatterVariableAndValue>
    {
        public readonly string Key;
        public readonly string Value;

        private string DebuggerDisplay => $"{Key}:{Value}";

        public FrontmatterVariableAndValue(string key, string value)
        {
            Key = key.ToLowerInvariant();
            Value = value?.ToLowerInvariant() ?? string.Empty;
        }

        public FrontmatterVariableAndValue(KeyValuePair<string, object> keyValuePair)
        {
            Key = keyValuePair.Key.ToLowerInvariant();
            Value = keyValuePair.Value?.ToString().ToLowerInvariant() ?? string.Empty;
        }

        public bool Equals(FrontmatterVariableAndValue other)
        {
            return
                string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is FrontmatterVariableAndValue other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 31;
            hashCode = hashCode * 17 + Key.GetHashCode();
            hashCode = hashCode * 17 + Value.GetHashCode();
            return hashCode;
        }
    }
}
