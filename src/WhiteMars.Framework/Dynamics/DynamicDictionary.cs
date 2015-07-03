using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    public class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        IDictionary<string, object> map = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public override bool TryGetMember(
                 GetMemberBinder binder, out object result)
        {

            result = null;
            if (this.map.ContainsKey(binder.Name))
            {
                result = this.map[binder.Name];
                return true;
            }
            return true; // always return true to skip the exception
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this.map[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return base.TryInvokeMember(binder, args, out result);
        }


        public void Add(string key, object value)
        {
            this.map.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.map.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return this.map.Keys; }
        }

        public bool Remove(string key)
        {
            return this.map.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.map.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return this.map.Values; }
        }

        public object this[string key]
        {
            get
            {
                return this.map[key];
            }
            set
            {
                this.map[key] = value;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.map.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.map.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.map.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this.map.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.map.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.map.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this.map.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.map).GetEnumerator();
        }
    }
}
