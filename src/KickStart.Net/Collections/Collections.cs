using System.Collections.Generic;

namespace KickStart.Net.Collections
{
    public static class Lists<T>
    {
        /// <summary>
        /// An empty <see cref="List{T}"/> constant.
        /// </summary>
        internal static readonly List<T> EmptyList = new List<T>();
        /// <summary>
        /// An empty <see cref="LinkedList{T}"/> constant.
        /// </summary>
        internal static readonly LinkedList<T> EmptyLinkedList = new LinkedList<T>(); 
    }

    public static class Dictionaries<TK, TV>
    {
        /// <summary>
        /// An empty <see cref="Dictionary{TK, TV}"/> constant.
        /// </summary>
        internal static readonly Dictionary<TK, TV> EmptyDictionary = new Dictionary<TK, TV>();
    }
}
