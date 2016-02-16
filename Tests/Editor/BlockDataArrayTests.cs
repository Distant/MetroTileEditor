using NUnit.Framework;
using MetroTileEditor;
using System;

namespace UnityTest.MetroTileEditor
{
    [TestFixture]
    public class BlockDataArrayTests
    {
        [Test]
        public void BlockArraySize()
        {
            BlockDataArray array = new BlockDataArray(4, 6, 8);
            Assert.AreEqual(4, array.Width);
            Assert.AreEqual(6, array.Height);
            Assert.AreEqual(8, array.Depth);
        }

        [Test]
        public void BlockArraySizeNegative()
        {
            BlockDataArray array = new BlockDataArray(-1, -1, 0);
            Assert.AreEqual(1, array.Width);
            Assert.AreEqual(1, array.Height);
            Assert.AreEqual(1, array.Depth);
        }

        [Test]
        public void BlockArrayAdd()
        {
            BlockDataArray array = new BlockDataArray(10, 10, 10);
            BlockData data = new BlockData();
            array.SetBlock(4, 5, 3, data);
            Assert.AreSame(data, array.GetBlockData(4, 5, 3));
        }

        [Test]
        public void BlockArrayNullDelete()
        {
            BlockDataArray array = new BlockDataArray(10, 10, 10);
            Assert.IsNull(array.GetBlockData(4, 5, 3), "null initially");
            BlockData data = new BlockData();
            array.SetBlock(4, 5, 3, data);
            array.DeleteBlock(4, 5, 3);
            Assert.IsNull(array.GetBlockData(4, 5, 3), "null after delete");
        }

        [Test]
        public void BlockArrayTrimSingleData()
        {
            BlockDataArray array = new BlockDataArray(4, 6, 8);
            BlockData data = new BlockData();
            data.placed = true;
            array.SetBlock(2, 5, 3, data);
            array.TrimArray();
            Assert.AreEqual(1, array.Width, "width");
            Assert.AreEqual(1, array.Height, "height");
            Assert.AreEqual(1, array.Depth, "depth");
            Assert.AreSame(data, array.GetBlockData(0, 0, 0), "data");
        }

        [Test]
        public void BlockArrayTrimNoData()
        {
            BlockDataArray array = new BlockDataArray(4, 6, 8);
            array.TrimArray();
            Assert.AreEqual(1, array.Width, "width");
            Assert.AreEqual(1, array.Height, "height");
            Assert.AreEqual(1, array.Depth, "depth");
        }

        [Test]
        public void BlockArrayAddInvalidIndex()
        {
            BlockDataArray array = new BlockDataArray(4, 6, 8);
            array.SetBlock(10, 10, 10, new BlockData());
            Assert.AreEqual(0, array.GetCount());
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void BlockArrayGetInvalidIndex()
        {
            BlockDataArray array = new BlockDataArray(4, 4, 4);
            array.GetBlockData(4, 4, 4);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void BlockArrayGetNegativeIndex()
        {
            BlockDataArray array = new BlockDataArray(4, 4, 4);
            array.GetBlockData(-1, -1, -1);
        }
    }
}