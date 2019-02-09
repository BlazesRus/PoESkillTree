using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace POESKillTree.Common
{
    /// <summary>
    /// Editable base Dictionary that extends from dictionary without actually using Dictionary as base class 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    class ExtendableDictionary<TKey, TValue>
    {
        /// <summary>
        /// The actual Dictionary Stored within
        /// </summary>
        public Dictionary<TKey, TValue> DicStorage;

        /// <summary>
        ///     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        ///     that is empty, has the default initial capacity, and uses the default equality
        //     comparer for the key type.
        /// </summary>
        public ExtendableDictionary() { DicStorage = new Dictionary<TKey, TValue>(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendableDictionary{TKey, TValue}"/> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the Dictionary can contain.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public ExtendableDictionary(int capacity) { DicStorage = new Dictionary<TKey, TValue>(capacity); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendableDictionary{TKey, TValue}"/> class that is empty, has the default initial capacity, and uses the specified IEqualityComparer.
        /// </summary>
        /// <param name="comparer">The IEqualityComparer implementation to use when comparing keys, or null to use the default IEqualityComparer for the type of the key.</param>
        public ExtendableDictionary(IEqualityComparer<TKey> comparer) { DicStorage = new Dictionary<TKey, TValue>(comparer); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendableDictionary{TKey, TValue}"/> class
        /// that contains elements copied from the specified IDictionary
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The IDictionary whose elements are copied to the new Dictionary</param>
        /// <exception cref="System.ArgumentNullException">dictionary is null.</exception>
        /// <exception cref="System.ArgumentException">dictionary contains one or more duplicate keys.</exception>
        public ExtendableDictionary(IDictionary<TKey, TValue> dictionary) { DicStorage = new Dictionary<TKey, TValue>(dictionary); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendableDictionary{TKey, TValue}"/> class
        /// that is empty, has the specified initial capacity, and uses the specified IEqualityComparer.
        /// </summary>
        /// <param name="capacity">
        ///  The initial number of elements that the Dictionary
        ///  can contain.
        /// </param>
        /// <param name="comparer">
        ///  The IEqualityComparer implementation to use when
        ///  comparing keys, or null to use the default EqualityComparer
        ///  for the type of the key.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
        public ExtendableDictionary(int capacity, IEqualityComparer<TKey> comparer) { DicStorage = new Dictionary<TKey, TValue>(capacity,comparer); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendableDictionary{TKey, TValue}"/> class
        ///  that contains elements copied from the specified IDictionary
        ///  and uses the specified IEqualityComparer.
        /// </summary>
        /// <param name="dictionary">
        /// The IDictionary whose elements are copied to the
        /// new Dictionary.
        /// </param>
        /// <param name="comparer">
        ///     The IEqualityComparer implementation to use when
        ///     comparing keys, or null to use the default EqualityComparer
        ///     for the type of the key.
        /// </param>
        /// <exception cref="System.ArgumentNullException">dictionary is null.</exception>
        /// <exception cref="System.ArgumentException">dictionary contains one or more duplicate keys.</exception>
        public ExtendableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) { DicStorage = new Dictionary<TKey, TValue>(dictionary, comparer); }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">
        /// The key of the value to get or set.
        /// </param>
        /// <returns>
        ///     The value associated with the specified key. If the specified key is not found,
        ///     a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        ///     a set operation creates a new element with the specified key.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
        public TValue this[TKey key] {
            get
            {
                return DicStorage[key];
            }
            set
            {
                DicStorage[key] = value;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in the Dictionary
        /// </summary>
        /// <returns>
        /// A Dictionary.ValueCollection containing the values in the Dictionary
        /// </returns>
        public Dictionary<TKey,TValue>.ValueCollection Values { get {return DicStorage.Values;} }

        /// <summary>
        /// Gets a collection containing the keys in the Dictionary
        /// </summary>
        /// <returns>A KeyCollection containing the keys in the Dictionary</returns>
        public Dictionary<TKey, TValue>.KeyCollection Keys { get { return DicStorage.Keys; } }

        /// <summary>
        /// Gets the number of key/value pairs contained in the Dictionary.
        /// </summary>
        public int Count { get {return DicStorage.Count;} }

        /// <summary>
        ///     Gets the IEqualityComparer that is used to determine
        ///     equality of keys for the dictionary.
        /// </summary>
        /// <returns>
        ///     The System.Collections.Generic.IEqualityComparer`1 generic interface implementation
        ///     that is used to determine equality of keys for the current System.Collections.Generic.Dictionary`2
        ///     and to provide hash values for the keys.
        /// </returns>
        public IEqualityComparer<TKey> Comparer { get { return DicStorage.Comparer; } }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the Dictionary</exception>
        public void Add(TKey key, TValue value) { DicStorage.Add(key, value); }

        /// <summary>
        /// Removes all keys and values from the Dictionary.
        /// </summary>
        public void Clear() { DicStorage.Clear(); }

        /// <summary>
        /// Determines whether the Dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the Dictionary.</param>
        /// <returns>
        ///   <c>true</c> if the Dictionary contains specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key) { return DicStorage.ContainsKey(key); }

        /// <summary>
        /// Determines whether the  Dictionary contains a specified value.
        /// </summary>
        /// <param name="value">The value to locate in the Dictionary. The value can be null for reference types.</param>
        /// <returns>
        ///   <c>true</c> if the Dictionary contains an element with the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(TValue value) { return DicStorage.ContainsValue(value); }

        /// <summary>
        /// Returns an enumerator that iterates through the Dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() { return DicStorage.GetEnumerator(); }

        /// <summary>
        ///     Implements the System.Runtime.Serialization.ISerializable interface and returns
        ///     the data needed to serialize the Dictionary instance.
        /// </summary>
        /// <param name="info">
        ///    A System.Runtime.Serialization.SerializationInfo object that contains the information
        ///    required to serialize the Dictionary instance.
        /// </param>
        /// <param name="context">
        ///    A System.Runtime.Serialization.StreamingContext structure that contains the source
        ///    and destination of the serialized stream associated with the Dictionary instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">info is null.</exception>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { DicStorage.GetObjectData(info, context); }

        /// <summary>
        ///     Implements the System.Runtime.Serialization.ISerializable interface and raises
        ///     the deserialization event when the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        ///     The System.Runtime.Serialization.SerializationInfo object associated with the
        ///     current Dictionary instance is invalid.
        /// </exception>
        public virtual void OnDeserialization(object sender) { DicStorage.OnDeserialization(sender); }
        //
        // Summary:
        //     Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove.
        //
        // Returns:
        //     true if the element is successfully found and removed; otherwise, false. This
        //     method returns false if key is not found in the System.Collections.Generic.Dictionary`2.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        /// <summary>
        /// Removes the value with the specified key from the Dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     true if the element is successfully found and removed; otherwise, false. This
        ///     method returns false if key is not found in the Dictionary
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key) { return DicStorage.Remove(key); }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key,
        ///     if the key is found; otherwise, the default value for the type of the value parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     true if the Dictionary contains an element with
        ///     the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value) { return DicStorage.TryGetValue(key, out value); }

  }
}
