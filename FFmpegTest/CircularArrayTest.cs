using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegTest;

using FFmpeg.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[TestClass]
public class CircularArrayTests
{
    [TestMethod]
    public void Add_MatchesList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        for (int i = 0; i < 1000; i++)
        {
            circular.Add(i);
            list.Add(i);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void RemoveAt_MatchesList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        for (int i = 0; i < 100; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        var random = new Random(12345);

        while (list.Count > 0)
        {
            int index = random.Next(list.Count);

            circular.RemoveAt(index);
            list.RemoveAt(index);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void Insert_MatchesList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        var random = new Random(12345);

        for (int i = 0; i < 1000; i++)
        {
            int index = random.Next(list.Count + 1);

            circular.Insert(index, i);
            list.Insert(index, i);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertRange_MatchesList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        var random = new Random(12345);

        for (int i = 0; i < 500; i++)
        {
            int index = random.Next(list.Count + 1);
            int count = random.Next(1, 20);

            var values = new int[count];

            for (int j = 0; j < values.Length; j++)
                values[j] = random.Next();

            circular.InsertRange(index, (IEnumerable<int>)values);
            list.InsertRange(index, values);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertRange_MatchesList_List()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        var random = new Random(12345);

        for (int i = 0; i < 500; i++)
        {
            int index = random.Next(list.Count + 1);
            int count = random.Next(1, 20);

            var values = new int[count].ToList();

            for (int j = 0; j < values.Count; j++)
                values[j] = random.Next();

            circular.InsertRange(index, (IEnumerable<int>)values);
            list.InsertRange(index, values);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertRange_MatchesList_ReadOnlyList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        var random = new Random(12345);

        for (int i = 0; i < 500; i++)
        {
            int index = random.Next(list.Count + 1);
            int count = random.Next(1, 20);

            var values = new Queue<int>();

            for (int j = 0; j < count; j++)
                values.Enqueue(random.Next());

            circular.InsertRange(index, values);
            list.InsertRange(index, values);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertAndRemove_MatchesList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        var random = new Random(12345);

        for (int i = 0; i < 5000; i++)
        {
            if (list.Count == 0 || random.Next(2) == 0)
            {
                int index = random.Next(list.Count + 1);
                int value = random.Next();

                circular.Insert(index, value);
                list.Insert(index, value);
            }
            else
            {
                int index = random.Next(list.Count);

                circular.RemoveAt(index);
                list.RemoveAt(index);
            }

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertAndRemove_WithWrapping_MatchesList()
    {
        var circular = new CircularArray<int>(16);
        var list = new List<int>();

        // Move Head away from zero and create a wrapped representation.
        for (int i = 0; i < 12; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        for (int i = 0; i < 7; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        AssertEqual(list, circular);

        var random = new Random(42);

        for (int i = 0; i < 5000; i++)
        {
            if (list.Count == 0 || random.Next(2) == 0)
            {
                int index = random.Next(list.Count + 1);
                int value = random.Next();

                circular.Insert(index, value);
                list.Insert(index, value);
            }
            else
            {
                int index = random.Next(list.Count);

                circular.RemoveAt(index);
                list.RemoveAt(index);
            }

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertAtBeginning_WhenWrapped_MatchesList()
    {
        var circular = new CircularArray<int>(10);
        var list = new List<int>();

        for (int i = 0; i < 7; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        // Move Head into the middle of the backing array.
        for (int i = 0; i < 4; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        AssertEqual(list, circular);

        for (int i = 0; i < 100; i++)
        {
            circular.Insert(0, i);
            list.Insert(0, i);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void InsertAtEnd_WhenWrapped_MatchesList()
    {
        var circular = new CircularArray<int>(10);
        var list = new List<int>();

        for (int i = 0; i < 7; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        for (int i = 0; i < 4; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        AssertEqual(list, circular);

        for (int i = 0; i < 100; i++)
        {
            circular.Insert(circular.Count, i);
            list.Insert(list.Count, i);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void RemoveFromBeginning_WhenWrapped_MatchesList()
    {
        var circular = new CircularArray<int>(16);
        var list = new List<int>();

        for (int i = 0; i < 12; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        for (int i = 0; i < 5; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        AssertEqual(list, circular);

        while (list.Count > 0)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void RemoveFromEnd_WhenWrapped_MatchesList()
    {
        var circular = new CircularArray<int>(16);
        var list = new List<int>();

        for (int i = 0; i < 12; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        for (int i = 0; i < 5; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        AssertEqual(list, circular);

        while (list.Count > 0)
        {
            int index = list.Count - 1;

            circular.RemoveAt(index);
            list.RemoveAt(index);

            AssertEqual(list, circular);
        }
    }

    [TestMethod]
    public void Clear_MatchesList()
    {
        var circular = new CircularArray<int>();

        for (int i = 0; i < 100; i++)
            circular.Add(i);

        circular.Clear();

        Assert.AreEqual(0, circular.Count);
        Assert.AreEqual(0, circular.Capacity >= 0 ? circular.Count : -1);
    }

    [TestMethod]
    public void Indexer_MatchesList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        for (int i = 0; i < 100; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        // Move Head.
        for (int i = 0; i < 37; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        for (int i = 0; i < list.Count; i++)
        {
            Assert.AreEqual(list[i], circular[i]);

            circular[i] = -list[i];
            list[i] = -list[i];

            Assert.AreEqual(list[i], circular[i]);
        }

        AssertEqual(list, circular);
    }

    [TestMethod]
    public void ContainsAndIndexOf_MatchList()
    {
        var circular = new CircularArray<int>();
        var list = new List<int>();

        for (int i = 0; i < 100; i++)
        {
            circular.Add(i % 17);
            list.Add(i % 17);
        }

        // Force wrapping.
        for (int i = 0; i < 31; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        for (int value = -2; value < 20; value++)
        {
            Assert.AreEqual(list.IndexOf(value), circular.IndexOf(value));
            Assert.AreEqual(list.Contains(value), circular.Contains(value));
        }
    }

    [TestMethod]
    public void CopyTo_MatchesList()
    {
        var circular = new CircularArray<int>(10);
        var list = new List<int>();

        for (int i = 0; i < 8; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        // Force the circular representation to wrap.
        for (int i = 0; i < 5; i++)
        {
            circular.RemoveAt(0);
            list.RemoveAt(0);
        }

        for (int i = 8; i < 15; i++)
        {
            circular.Add(i);
            list.Add(i);
        }

        AssertEqual(list, circular);

        var a = new int[30];
        var b = new int[30];

        circular.CopyTo(a, 7);
        list.CopyTo(b, 7);

        CollectionAssert.AreEqual(b, a);
    }

    [TestMethod]
    public void Constructors_MatchList()
    {
        int[] values = new int[100];

        for (int i = 0; i < values.Length; i++)
            values[i] = i * 7;

        var expected = new List<int>(values);

        AssertEqual(expected, new CircularArray<int>(values));
        AssertEqual(expected, new CircularArray<int>((IReadOnlyList<int>)values));
        AssertEqual(expected, new CircularArray<int>((IList<int>)values));
        AssertEqual(expected, new CircularArray<int>((ICollection<int>)values));
        AssertEqual(expected, new CircularArray<int>((IEnumerable<int>)values));
        AssertEqual(expected, new CircularArray<int>(values.AsSpan()));
    }

    [TestMethod]
    public void RandomizedOperations_MatchList()
    {
        var circular = new CircularArray<int>(4);
        var list = new List<int>();

        var random = new Random(0x12345678);

        for (int operation = 0; operation < 100_000; operation++)
        {
            int action = random.Next(100);

            if (list.Count == 0)
                action = 0;

            switch (action)
            {
                case < 25:
                {
                    int value = random.Next();

                    circular.Add(value);
                    list.Add(value);

                    break;
                }

                case < 50:
                {
                    int index = random.Next(list.Count + 1);
                    int value = random.Next();

                    circular.Insert(index, value);
                    list.Insert(index, value);

                    break;
                }

                case < 65:
                {
                    int index = random.Next(list.Count + 1);
                    int count = random.Next(1, 15);

                    var values = new int[count];

                    for (int i = 0; i < count; i++)
                        values[i] = random.Next();

                    circular.InsertRange(index, (IEnumerable<int>)values);
                    list.InsertRange(index, values);

                    break;
                }

                case < 90:
                {
                    int index = random.Next(list.Count);

                    circular.RemoveAt(index);
                    list.RemoveAt(index);

                    break;
                }

                default:
                {
                    circular.Clear();
                    list.Clear();

                    break;
                }
            }

            AssertEqual(list, circular);
        }
    }

    private static void AssertEqual<T>(
        IReadOnlyList<T> expected,
        CircularArray<T> actual)
    {
        Assert.AreEqual(expected.Count, actual.Count);

        for (int i = 0; i < expected.Count; i++)
            Assert.AreEqual(expected[i], actual[i]);

        CollectionAssert.AreEqual(
            new List<T>(expected),
            new List<T>(actual));
    }
}
