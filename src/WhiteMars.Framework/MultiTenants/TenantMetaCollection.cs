using System;
using System.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections.Generic;

namespace WhiteMars.Framework
{
    public class TenantMetaCollection : IList<TenantMeta>
    {
        private List<TenantMeta> data = new List<TenantMeta>();

        #region IList implementation

        public int IndexOf(TenantMeta item)
        {
            return this.data.IndexOf(item);
        }

        public void Insert(int index, TenantMeta item)
        {
            this.data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.data.RemoveAt(index);
        }

        [XmlIgnore]
        public TenantMeta this [int index]
        {
            get
            {
                return this.data[index];
            }
            set
            {
                this.data[index] = value;
            }
        }

        #endregion

        #region ICollection implementation

        public void Add(TenantMeta item)
        {
            this.data.Add(item);
        }

        public void Clear()
        {
            this.data.Clear();
        }

        public bool Contains(TenantMeta item)
        {
            return this.data.Contains(item);
        }

        public void CopyTo(TenantMeta[] array, int arrayIndex)
        {
            this.data.CopyTo(array, arrayIndex);
        }

        public bool Remove(TenantMeta item)
        {
            return this.data.Remove(item);
        }

        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<TenantMeta> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        #endregion

        #region Additional Methods

        /// <summary>
        /// Gets the by URL. If tenant does not exist, NULL will be returned.
        /// </summary>
        /// <returns>The by URL.</returns>
        /// <param name="url">URL.</param>
        /// <param name="ignoreCase">If set to <c>true</c> ignore case.</param>
        public TenantMeta GetByUrl(string url, bool ignoreCase = false)
        {
            return this.data.FirstOrDefault(c => string.Compare(url, c.UniqueUrl, ignoreCase) == 0);
        }

        /// <summary>
        /// Check if a tenant with URL exists.
        /// </summary>
        /// <returns><c>true</c>, if URL was existsed, <c>false</c> otherwise.</returns>
        /// <param name="url">URL.</param>
        /// <param name="ignoreCase">If set to <c>true</c> ignore case.</param>
        public bool Contains(string url, bool ignoreCase = false)
        {
            return this.data.Any(c => string.Compare(url, c.UniqueUrl, ignoreCase) == 0);
        }

        #endregion
        
    }
}

