﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Daemons.Util
{
    public class CopyOnWriteList<T> : IList<T>
    {
        private List<T> _list = new List<T>();

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(T item)
        {
            List<T> oldList;
            List<T> foundList;
            do
            {
                oldList = _list;
                var newList = CopyList(_list);
                newList.Add(item);
                foundList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (foundList != oldList);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            _list = new List<T>();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(T item)
        {
            List<T> oldList;
            List<T> foundList;
            bool result;
            do
            {
                oldList = _list;
                var newList = CopyList(_list, _list.Count);
                result = newList.Remove(item);
                foundList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (foundList != oldList);
            return result;
        }

        /// <summary>
        /// Gets the number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1"/>.
        /// </returns>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void Insert(int index, T item)
        {
            List<T> oldList;
            List<T> foundList;
            do
            {
                oldList = _list;
                var newList = CopyList(_list);
                newList.Insert(index, item);
                foundList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (foundList != oldList);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            List<T> oldList;
            List<T> foundList;
            do
            {
                oldList = _list;
                var newList = CopyList(_list);
                newList.RemoveAt(index);
                foundList = Interlocked.CompareExchange(ref _list, newList, oldList);
            } while (foundList != oldList);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="index"/> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count"/>. </exception>
        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                List<T> oldList;
                List<T> foundList;
                do
                {
                    oldList = _list;
                    var newList = CopyList(_list, _list.Count);
                    newList[index] = value;
                    foundList = Interlocked.CompareExchange(ref _list, newList, oldList);
                } while (foundList != oldList);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Determines whether every element in the <see cref="T:System.Collections.Generic.List`1"/> matches the conditions defined by the specified predicate.
        /// </summary>
        /// <returns>
        /// true if every element in the <see cref="T:System.Collections.Generic.List`1"/> matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions to check against the elements.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public bool TrueForAll(Predicate<T> match)
        {
            return _list.TrueForAll(match);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.List`1"/> to a new array.
        /// </summary>
        /// <returns>
        /// An array containing copies of the elements of the <see cref="T:System.Collections.Generic.List`1"/>.
        /// </returns>
        public T[] ToArray()
        {
            return _list.ToArray();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
        /// </returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>. </param><filterpriority>2</filterpriority>
        public bool Contains(object value)
        {
            return ((IList) _list).Contains(value);
        }
        

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception><exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception><filterpriority>2</filterpriority>
        public void CopyTo(Array array, int index)
        {
            ((ICollection) _list).CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        private List<T> CopyList(List<T> list)
        {
            return CopyList(list, list.Count + 1);
        }

        private List<T> CopyList(IEnumerable<T> list, int count)
        {
            var newList = new List<T>(count);
            newList.AddRange(list);
            return newList;
        }
    }
}