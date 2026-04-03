using System.Linq;

namespace IDEK.Tools.ShocktroopUtils
{
    /// <summary>
    /// A dictionary key that uses multiple objects as keys, limits use in that the key is not easily useable data.
    /// </summary>
    [System.Serializable]
    public struct MultiKey : System.IEquatable<MultiKey>
    {
        public readonly object[] keys;

        public MultiKey(params object[] keys)
        {
            this.keys = keys;
        }

        public bool Equals(MultiKey other)
        {
            //return false if different, return true if both null, continue if both non null
            if(keys == null)
            {
                return other.keys == null; 
            }
            else if(other.keys == null) 
            {
                return keys == null;
            }

            foreach (object o in other.keys)
            {
                if (!keys.Contains(o))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsNull<T>(T key)
        {
            return (key == null || (key.GetType().IsEnum && key.Equals(default(T))));
        }

        public override int GetHashCode()
        {
            if (keys == null)
                return base.GetHashCode();
            int hash = 0;
            foreach (object o in keys)
                hash += o.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// A dictionary key that uses two objects as keys.
    /// isNullWildcard can be set to true to consider any of that Multikey's null (or 0 for enums) values to be any value.
    /// </summary>
    [System.Serializable]
    public struct MultiKey<T1, T2> : System.IEquatable<MultiKey<T1, T2>>
    {
#if UNITY_5_3_OR_NEWER
        [UnityEngine.HideInInspector]
#endif
        public bool isNullWildcard;
        public T1 key1;
        public T2 key2;

        public MultiKey(bool isNullWildcard, T1 key1, T2 key2)
        {
            this.isNullWildcard = isNullWildcard;
            this.key1 = key1;
            this.key2 = key2;
        }

        public bool Equals(MultiKey<T1, T2> other)
        {
            bool wildcardComparison = isNullWildcard || other.isNullWildcard;
            bool key1match = false; 
            bool key2match = false;
            if(IsNull(key1) || IsNull(other.key1))
            {
                if (IsNull(key1) && wildcardComparison)
                {
                    key1match = true;
                }
                else if (IsNull(other.key1) && wildcardComparison)
                {
                    key1match = true;
                }
                else if (IsNull(key1) && IsNull(other.key1))
                {
                    key1match = true;
                }
            }
            else if(key1.Equals(other.key1))
            {
                key1match = true;
            }

            if (IsNull(key2) || IsNull(other.key2))
            {
                if (IsNull(key2) && wildcardComparison)
                {
                    key2match = true;
                }
                else if (IsNull(other.key2) && wildcardComparison)
                {
                    key2match = true;
                }
                else if (IsNull(key2) && IsNull(other.key2))
                {
                    key2match = true;
                }

            }
            else if (key2.Equals(other.key2))
            {
                key2match = true;
            }

            return key1match && key2match;
        }

        private bool IsNull<T>(T key)
        {
            return (key == null || (key.GetType().IsEnum && key.Equals(default(T))));
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash += key1 == null ? base.GetHashCode() : key1.GetHashCode();
            hash += key2 == null ? base.GetHashCode() : key2.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// A dictionary key that uses three objects as keys. 
    /// isNullWildcard can be set to true to consider any of that Multikey's null (or 0 for enums) values to be any value.
    /// </summary>
    [System.Serializable]
    public struct MultiKey<T1, T2, T3> : System.IEquatable<MultiKey<T1, T2, T3>>
    {
#if UNITY_5_3_OR_NEWER
        [UnityEngine.HideInInspector]
#endif
        public bool isNullWildcard;
        public T1 key1;
        public T2 key2;
        public T3 key3;

        public MultiKey(bool isNullWildcard, T1 key1, T2 key2, T3 key3)
        {
            this.isNullWildcard = isNullWildcard;
            this.key1 = key1;
            this.key2 = key2;
            this.key3 = key3;
        }

        public bool Equals(MultiKey<T1, T2, T3> other)
        {
            bool wildcardComparison = isNullWildcard || other.isNullWildcard;
            bool key1match = false;
            bool key2match = false;
            bool key3match = false;
            if (IsNull(key1) || IsNull(other.key1))
            {
                if(IsNull(key1) && wildcardComparison)
                {
                    key1match = true;
                }
                else if(IsNull(other.key1) && isNullWildcard)
                {
                    key1match = true;
                }
                else if (IsNull(key1) && IsNull(other.key1))
                {
                    key1match = true;
                }
            }
            else if (key1.Equals(other.key1))
            {
                key1match = true;
            }

            if (IsNull(key2) || IsNull(other.key2))
            {
                if (IsNull(key2) && wildcardComparison)
                {
                    key2match = true;
                }
                else if (IsNull(other.key2) && wildcardComparison)
                {
                    key2match = true;
                }
                else if (IsNull(key2) && IsNull(other.key2))
                {
                    key2match = true;
                }

            }
            else if (key2.Equals(other.key2))
            {
                key2match = true;
            }

            if (IsNull(key3) || IsNull(other.key3))
            {
                if (IsNull(key3) && wildcardComparison)
                {
                    key3match = true;
                }
                else if (IsNull(other.key3) && wildcardComparison)
                {
                    key3match = true;
                }
                else if (IsNull(key3) && IsNull(other.key3))
                {
                    key3match = true;
                }

            }
            else if (key3.Equals(other.key3))
            {
                key3match = true;
            }

            return key1match && key2match && key3match;
        }

        private bool IsNull<T>(T key)
        {
            return (key == null || (key.GetType().IsEnum && key.Equals(default(T))));
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash += key1 == null ? base.GetHashCode() : key1.GetHashCode();
            hash += key2 == null ? base.GetHashCode() : key2.GetHashCode();
            hash += key3 == null ? base.GetHashCode() : key3.GetHashCode();
            return hash;
        }
    }
}