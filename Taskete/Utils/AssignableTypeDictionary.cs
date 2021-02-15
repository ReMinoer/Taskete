using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Taskete.Utils
{
    public class AssignableTypeDictionary<T> : IDictionary<Type, T>
    {
        private readonly Dictionary<Type, T> _dictionary = new Dictionary<Type, T>();
        private IDictionary<Type, T> Dictionary => _dictionary;

        public IEnumerable<T> GetAllValues(Type key)
        {
            for (Type currentType = key; currentType != null; currentType = currentType.GetTypeInfo().BaseType)
                if (_dictionary.TryGetValue(currentType, out T value))
                    yield return value;

            foreach (Type implementedInterface in key.GetTypeInfo().ImplementedInterfaces)
                if (_dictionary.TryGetValue(implementedInterface, out T value))
                    yield return value;
        }

        public bool HasValues(Type key)
        {
            for (Type currentType = key; currentType != null; currentType = currentType.GetTypeInfo().BaseType)
                if (_dictionary.ContainsKey(currentType))
                    return true;

            foreach (Type implementedInterface in key.GetTypeInfo().ImplementedInterfaces)
                if (_dictionary.ContainsKey(implementedInterface))
                    return true;

            return false;
        }

        public T this[Type key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public bool TryGetValue(Type key, out T value) => Dictionary.TryGetValue(key, out value);

        public void Add(Type key, T value) => _dictionary.Add(key, value);
        public bool Remove(Type key) => _dictionary.Remove(key);
        public void Clear() => _dictionary.Clear();

        int ICollection<KeyValuePair<Type, T>>.Count => Dictionary.Count;
        bool ICollection<KeyValuePair<Type, T>>.IsReadOnly => Dictionary.IsReadOnly;
        void ICollection<KeyValuePair<Type, T>>.Add(KeyValuePair<Type, T> item) => Add(item.Key, item.Value);
        bool ICollection<KeyValuePair<Type, T>>.Remove(KeyValuePair<Type, T> item) => Remove(item.Key);
        bool ICollection<KeyValuePair<Type, T>>.Contains(KeyValuePair<Type, T> item) => Dictionary.Contains(item);
        void ICollection<KeyValuePair<Type, T>>.CopyTo(KeyValuePair<Type, T>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

        ICollection<Type> IDictionary<Type, T>.Keys => Dictionary.Keys;
        ICollection<T> IDictionary<Type, T>.Values => Dictionary.Values;
        bool IDictionary<Type, T>.ContainsKey(Type key) => Dictionary.ContainsKey(key);

        IEnumerator<KeyValuePair<Type, T>> IEnumerable<KeyValuePair<Type, T>>.GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();
    }
}