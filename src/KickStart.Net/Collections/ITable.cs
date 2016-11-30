using System.Collections.Generic;

namespace KickStart.Net.Collections
{
    /// <summary>
    /// A collection that associates an ordered pair of keys, a row key and a column key, with a value.
    /// </summary>
    /// <typeparam name="TR">The type of the table row keys</typeparam>
    /// <typeparam name="TC">The type of the table column keys</typeparam>
    /// <typeparam name="TV">The type of the values</typeparam>
    public interface ITable<TR, TC, TV>
    {
        /// <summary>
        /// Returns true if the table contains a mapping with the row and column keys.
        /// </summary>
        /// <param name="rowKey">row key to search for</param>
        /// <param name="columnKey">column key to search for</param>
        bool Contains(TR rowKey, TC columnKey);
        /// <summary>
        /// Returns true if the table contains a mapping with the specified row.
        /// </summary>
        /// <param name="rowKey">row key to search for</param>
        bool ContainsRow(TR rowKey);
        /// <summary>
        /// Returns true if the table contains a mapping with the specified column.
        /// </summary>
        /// <param name="columnKey">column key to search for</param>
        bool ContainsColumn(TC columnKey);
        /// <summary>
        /// Returns true if the table contains a mapping with the specific value.
        /// </summary>
        /// <param name="value">value to search for</param>
        bool ContainsValue(TV value);
        /// <summary>
        /// Returns the value mapped to the given row and column key or default if no such mapping exists.
        /// </summary>
        TV Get(TR rowKey, TC columnKey);
        /// <summary>
        /// Return true is the table is empty.
        /// </summary>
        bool IsEmpty();
        /// <summary>
        /// Removes all mappings from the table.
        /// </summary>
        void Clear();
        /// <summary>
        /// Add the value and associated keys into the table. If the table already
        /// contains a mapping for those keys, the old value is replace with 
        /// <paramref name="value"/>.
        /// </summary>
        /// <returns>the value previously associated with the keys, or null if no mapping existed</returns>
        TV Put(TR rowKey, TC columnKey, TV value);
        /// <summary>
        /// Copies all mappings from the <paramref name="input"/> into the table. The effect is 
        /// the same as calling <see cref="Put"/> for each mapping in <paramref name="input"/>.
        /// </summary>
        void PutAll(ITable<TR, TC, TV> input);
        /// <summary>
        /// Removes the mapping if exist.
        /// </summary>
        /// <returns>the value previously associated with the keys, or null if no mapping existed</returns>
        TV Remove(TR rowKey, TC columnKey);
        /// <summary>
        /// Returns a readonly view of all mappings that have the given row key.
        /// </summary>
        IReadOnlyDictionary<TC, TV> Row(TR rowKey);
        /// <summary>
        /// Returns a readonly view of all mappings that have the given column key.
        /// </summary>
        IReadOnlyDictionary<TR, TV> Column(TC columnKey);
        /// <summary>
        /// Returns a readonly view of all mapping triplets in the table.
        /// </summary>
        IReadOnlyCollection<ICell<TR, TC, TV>> CellSet();
        /// <summary>
        /// Returns a readonly view of all row keys in the table.
        /// </summary>
        IReadOnlyCollection<TR> RowKeySet();
        /// <summary>
        /// Returns a readonly view of all distinct column keys in the table.
        /// </summary>
        IReadOnlyCollection<TC> ColumnKeySet();
        /// <summary>
        /// Returns a readonly view of all values in the table.
        /// </summary>
        /// <remarks>The returned values may contain duplicates.</remarks>
        IReadOnlyCollection<TV> Values();
    }

    public interface ICell<TR, TC, TV>
    {
        /// <summary>
        /// Returns the row key of the cell.
        /// </summary>
        TR RowKey { get; }
        /// <summary>
        /// Returns the column key of the cell.
        /// </summary>
        TC ColumnKey { get; }
        /// <summary>
        /// Returns the value of the cell.
        /// </summary>
        TV Value { get; }
    }
}
