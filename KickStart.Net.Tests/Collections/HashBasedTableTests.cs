using System.Linq;
using KickStart.Net.Collections;
using NUnit.Framework;

namespace KickStart.Net.Tests.Collections
{
    [TestFixture]
    public class HashBasedTableTests
    {
        [Test]
        public void can_put_value()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsFalse(table.Contains(1, 2));
            Assert.IsTrue(table.IsEmpty());

            Assert.IsNull(table.Put(1, 2, 3));

            Assert.IsTrue(table.Contains(1, 2));
            Assert.IsTrue(table.ContainsRow(1));
            Assert.IsTrue(table.ContainsColumn(2));
            Assert.IsTrue(table.ContainsValue(3));
            Assert.IsFalse(table.IsEmpty());
            
            Assert.AreEqual(3, table.Put(1, 2, 4));
            Assert.AreEqual(1, table.CellSet().Count);
        }

        [Test]
        public void can_get()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.AreEqual(null, table.Get(1, 2));
            table.Put(1, 2, 3);
            Assert.AreEqual(3, table.Get(1, 2));
        }

        [Test]
        public void put_rejects_null_keys()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsNull(table.Put(1, 2, 3));
            Assert.IsNull(table.Put(null, 2, 3));
            Assert.IsNull(table.Put(1, null, 3));
            Assert.IsNull(table.Put(null, null, 3));
            Assert.AreEqual(1, table.CellSet().Count);
        }

        [Test]
        public void can_clear()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsNull(table.Put(1, 2, 3));
            Assert.IsNull(table.Put(1, 3, 4));
            table.Clear();
            Assert.IsTrue(table.IsEmpty());
        }

        [Test]
        public void test_contains_handles_null()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsFalse(table.Contains(null, null));
            Assert.IsFalse(table.ContainsRow(null));
            Assert.IsFalse(table.ContainsColumn(null));
        }

        [Test]
        public void can_put_all()
        {
            var table = new HashBasedTable<int?, int?, int?>();

            var other = new HashBasedTable<int?, int?, int?>();
            other.Put(1, 2, 3);
            other.Put(2, 3, 4);

            table.PutAll(other);
            var set = table.CellSet();
            var otherSet = other.CellSet();
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(2, otherSet.Count);

            Assert.AreEqual(otherSet.ElementAt(0), set.ElementAt(0));
            Assert.AreEqual(otherSet.ElementAt(1), set.ElementAt(1));
            Assert.AreNotSame(otherSet.ElementAt(0), set.ElementAt(0));
            Assert.AreNotSame(otherSet.ElementAt(1), set.ElementAt(1));
        }

        [Test]
        public void test_cell_compare()
        {
            var cell1 = new HashBasedTable<int, int, int>.Cell(1, 2, 3);
            var cell2 = new HashBasedTable<int, int, int>.Cell(1, 2, 3);
            Assert.AreEqual(cell1, cell2);
            Assert.AreEqual(cell1, cell1);

            var cell3 = new HashBasedTable<int?, int?, int?>.Cell(null, null, null);
            var cell4 = new HashBasedTable<int?, int?, int?>.Cell(null, null, null);
            Assert.AreEqual(cell3, cell4);
            Assert.AreEqual(cell3, cell4);

            Assert.IsFalse(cell1.Equals(new object()));
        }

        [Test]
        public void test_remove()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsNull(table.Put(1, 2, 3));
            Assert.IsNull(table.Remove(1, 3));
            Assert.IsNull(table.Remove(2, 2));
            Assert.AreEqual(1, table.CellSet().Count);

            Assert.AreEqual(3, table.Remove(1, 2));
            Assert.IsTrue(table.IsEmpty());
        }

        [Test]
        public void test_row()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsNull(table.Row(1));
            table.Put(1, 2, 3);
            Assert.IsNotNull(table.Row(1));
            var dict = table.Row(1);
            Assert.AreEqual(3, dict[2]);
            Assert.AreEqual(1, dict.Count);
        }

        [Test]
        public void test_column()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            Assert.IsNull(table.Column(2));
            table.Put(1, 2, 3);
            Assert.IsNotNull(table.Column(2));
            var dict = table.Column(2);
            Assert.AreEqual(3, dict[1]);
            Assert.AreEqual(1, dict.Count);
        }

        [Test]
        public void test_values()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            var values = table.Values();
            Assert.IsNotNull(values);
            Assert.AreEqual(0, values.Count);

            table.Put(1, 2, 3);
            table.Put(2, 3, 3);
            table.Put(1, 3, null);
            table.Put(1, 4, null);
            values = table.Values();
            Assert.IsNotNull(values);
            Assert.AreEqual(4, values.Count);
            Assert.AreEqual(new int?[] {3, null, null, 3}, values);
        }

        [Test]
        public void test_row_key_set()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            var keys = table.RowKeySet();
            Assert.IsNotNull(keys);
            Assert.AreEqual(0, keys.Count);

            table.Put(1, 2, 3);
            table.Put(1, 3, 4);
            table.Put(2, 2, 4);
            keys = table.RowKeySet();
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new int?[] {1, 2}, keys);
        }

        [Test]
        public void test_column_key_set()
        {
            var table = new HashBasedTable<int?, int?, int?>();
            var keys = table.ColumnKeySet();
            Assert.IsNotNull(keys);
            Assert.AreEqual(0, keys.Count);

            table.Put(1, 2, 3);
            table.Put(1, 3, 4);
            table.Put(2, 2, 4);
            keys = table.ColumnKeySet();
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new int?[] { 2, 3 }, keys);
        }
    }
}
