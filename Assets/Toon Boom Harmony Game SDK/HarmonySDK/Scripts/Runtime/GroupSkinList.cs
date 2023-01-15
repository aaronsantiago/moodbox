using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Animations;

namespace ToonBoom.Harmony
{
    // Since we cannot animate arrays, we enforce a hard limit of 8 group skins
    [System.Serializable]
    public struct GroupSkinList : IList<GroupSkin>, IList
    {
        public const int MAX_COUNT = 8;

        [SerializeField, NotKeyable] private int _count;

        [SerializeField] private GroupSkin _item0;
        [SerializeField] private GroupSkin _item1;
        [SerializeField] private GroupSkin _item2;
        [SerializeField] private GroupSkin _item3;
        [SerializeField] private GroupSkin _item4;
        [SerializeField] private GroupSkin _item5;
        [SerializeField] private GroupSkin _item6;
        [SerializeField] private GroupSkin _item7;

        public GroupSkinList(int size)
        {
            _count = ClampSize(size);
            _item0 = new GroupSkin();
            _item1 = new GroupSkin();
            _item2 = new GroupSkin();
            _item3 = new GroupSkin();
            _item4 = new GroupSkin();
            _item5 = new GroupSkin();
            _item6 = new GroupSkin();
            _item7 = new GroupSkin();
        }

        public IEnumerator<GroupSkin> GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        int IList.Add(object value)
        {
            Add((GroupSkin)value);
            return _count - 1;
        }

        public void Add(GroupSkin value)
        {
            if (_count >= MAX_COUNT)
                throw new ArgumentException($"This array cannot have more than '{MAX_COUNT}' items.");

            Set(_count, value);

            ++_count;
        }

        public void Clear()
        {
            _count = 0;
        }

        int IList.IndexOf(object value) => IndexOf((GroupSkin)value);

        public int IndexOf(GroupSkin value)
        {
            for (int i = 0; i < _count; ++i)
            {
                if (Get(i).Equals(value))
                    return i;
            }

            return -1;
        }

        bool IList.Contains(object value) => Contains((GroupSkin)value);

        public bool Contains(GroupSkin value)
        {
            for (int i = 0; i < _count; ++i)
            {
                if (Get(i).Equals(value))
                    return true;
            }

            return false;
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _count; i++)
            {
                array.SetValue(Get(i), i + arrayIndex);
            }
        }

        public void CopyTo(GroupSkin[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < _count; i++)
            {
                array[i + arrayIndex] = Get(i);
            }
        }

        void IList.Remove(object value) { Remove((GroupSkin)value); }

        public bool Remove(GroupSkin value)
        {
            for (int i = 0; i < _count; ++i)
            {
                if (Get(i).Equals(value))
                {
                    for (; i < _count - 1; ++i)
                    {
                        Set(i, Get(i + 1));
                    }

                    --_count;
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            CheckOutOfRangeIndex(index);

            for (int i = index; i < _count - 1; ++i)
            {
                Set(i, Get(i + 1));
            }

            --_count;
        }

        void IList.Insert(int index, object value) => Insert(index, (GroupSkin)value);

        public void Insert(int index, GroupSkin value)
        {
            if (_count >= MAX_COUNT)
                throw new ArgumentException($"This array cannot have more than '{MAX_COUNT}' items.");

            CheckOutOfRangeIndex(index);

            if (index >= _count)
            {
                Add(value);
                return;
            }

            for (int i = _count; i > index; --i)
            {
                Set(i, Get(i - 1));
            }

            Set(index, value);
            ++_count;
        }

        private static int ClampSize(int size)
        {
            return Mathf.Clamp(size, 0, MAX_COUNT);
        }

        private void CheckOutOfRangeIndex(int index)
        {
            if (index < 0 || index >= MAX_COUNT)
                throw new IndexOutOfRangeException($"Index {index} is out of range of '{_count}' Length.");
        }

        private GroupSkin Get(int index)
        {
            CheckOutOfRangeIndex(index);

            switch (index)
            {
                case 0: return _item0;
                case 1: return _item1;
                case 2: return _item2;
                case 3: return _item3;
                case 4: return _item4;
                case 5: return _item5;
                case 6: return _item6;
                case 7: return _item7;
            }

            // Shouldn't happen.
            return _item0;
        }

        private void Set(int index, GroupSkin value)
        {
            CheckOutOfRangeIndex(index);

            switch (index)
            {
                case 0: _item0 = value; break;
                case 1: _item1 = value; break;
                case 2: _item2 = value; break;
                case 3: _item3 = value; break;
                case 4: _item4 = value; break;
                case 5: _item5 = value; break;
                case 6: _item6 = value; break;
                case 7: _item7 = value; break;
            }
        }

        public void SetGroup(int index, int groupId)
        {
            var item = Get(index);
            item.GroupId = groupId;

            Set(index, item);
        }

        public int GetGroup(int index)
        {
            return Get(index).GroupId;
        }

        public void SetSkin(int index, int skinId)
        {
            var item = Get(index);
            item.SkinId = skinId;

            Set(index, item);
        }

        public int GetSkin(int index)
        {
            return Get(index).SkinId;
        }

        object IList.this[int index] { get => (object)Get(index); set => Set(index, (GroupSkin)value); }

        public GroupSkin this[int index] { get => Get(index); set => Set(index, value); }

        public int Count { get => _count; }

        public bool IsReadOnly { get => false; }

        public bool IsFixedSize { get => false; }

        bool ICollection.IsSynchronized { get => true; }

        object ICollection.SyncRoot { get => null; }

        [System.Serializable]
        public struct Enumerator : IEnumerator<GroupSkin>
        {
            private GroupSkinList m_Array;
            private int m_Index;

            public Enumerator(ref GroupSkinList array)
            {
                m_Array = array;
                m_Index = -1;
            }

            public bool MoveNext()
            {
                m_Index++;
                return (m_Index < m_Array.Count);
            }

            public void Reset()
            {
                m_Index = -1;
            }

            void IDisposable.Dispose() { }

            public GroupSkin Current => m_Array.Get(m_Index);

            object IEnumerator.Current => Current;
        }
    }
}
