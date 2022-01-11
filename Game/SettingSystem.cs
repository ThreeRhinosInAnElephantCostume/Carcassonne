using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Threading;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

namespace SettingsSystem
{
    [Serializable]
    public class NotifyValue
    {
        [JsonRequired]
        protected object _value;
        [JsonIgnore]
        protected object _mx = new object();
        [JsonIgnore]
        public string Name;
        [JsonIgnore]
        public NotifyLink Parent;
        [JsonIgnore]
        public bool Modified { get; protected set; } = false;
        [JsonIgnore]
        public Action<object> OnModification = o => { };
        [JsonIgnore]
        public object Value
        {
            get => _value;
            set
            {
                lock (_mx)
                {
                    _value = value;
                    if (Parent != null)
                    {
                        Modified = true;
                        Parent.NotifyChange(Name, _value);
                    }
                }
                OnModification(_value);
            }
        }
        public void ClearModified()
        {
            Modified = false;
        }
    }
    public class NotifyLink
    {
        [JsonRequired]
        protected string Name;
        [JsonIgnore]
        protected object _mx = new object();
        [JsonIgnore]
        public bool Modified { get; protected set; } = false;
        [JsonIgnore]
        protected NotifyLink _domLink = null;
        [JsonIgnore]
        protected List<NotifyLink> _subLinks = new List<NotifyLink>();
        [JsonIgnore]
        protected List<NotifyValue> _subProperties = new List<NotifyValue>();
        [JsonIgnore]
        public Action<string, object> OnChangeHandle = (s, o) => { };
        public void NotifyChange(string name, object val)
        {
            lock (_mx)
            {
                Modified = true;
            }
            OnChangeHandle(name, val);
            if (_domLink != null)
                _domLink.NotifyChange(ConcatPaths(Name, name), val);
        }
        public void ClearModified()
        {
            lock (_mx)
            {
                Modified = false;
                _subLinks.ForEach(it => it.ClearModified());
                _subProperties.ForEach(it => it.ClearModified());
            }
        }
        protected void init()
        {
            _subLinks.Clear();
            _subProperties.Clear();
            foreach (var prop in this.GetType().GetProperties())
            {
                var o = prop.GetValue(this);
                if (o is NotifyLink link)
                {
                    link._domLink = this;
                    link.Name = prop.Name;
                    link.init();
                    this._subLinks.Add(link);
                }
                else if (o is NotifyValue nval)
                {
                    nval.Name = prop.Name;
                    nval.Parent = this;
                    _subProperties.Add(nval);
                }
            }
            ClearModified();
        }
        // public string Serialize()
        // {
        //     return Serialize(this);
        // }
        // public static string Serialize(NotifyLink link)
        // {
        //     return JsonConvert.SerializeObject(link);
        // }
        // public static NotifyLink Deserialize(string data)
        // {
        //     var link = JsonConvert.DeserializeObject<NotifyLink>(data);
        //     link.init();
        //     return link;
        // }
        public NotifyLink(string name = "")
        {
            this.Name = name;
            init();
        }
    }

    public class SettingsProperty<T> : NotifyValue
    {
        [JsonIgnore]
        public new Action<T> OnModification = o => { };
        [JsonIgnore]
        public new T Value
        {
            get => (T)base.Value;
            set => base.Value = value;
        }
        public static implicit operator T(SettingsProperty<T> prop) => prop.Value;
        public static implicit operator SettingsProperty<T>(T val) => new SettingsProperty<T>(val);
        public void TriggerModified()
        {
            OnModification(Value);
        }
        public SettingsProperty()
        {
            base.OnModification = o => OnModification((T)o);
        }
        public SettingsProperty(T value) : this()
        {
            this.Value = value;
        }
    }
    public class SettingsPropertyOptions<T> : SettingsProperty<int>
    {
        [JsonIgnore]
        public T[] Values { get; protected set; }
        [JsonIgnore]
        public new T Value
        {
            get => Values[base.Value];
        }
        public static implicit operator T(SettingsPropertyOptions<T> prop) => prop.Value;
        public static implicit operator SettingsPropertyOptions<T>(T[] vals) => new SettingsPropertyOptions<T>(vals);
        public T SetIndex(int indx)
        {
            Assert(indx >= 0 && indx < Values.Length, "Setting index out of range");
            base.Value = indx;
            return Value;
        }
        public T NextValue()
        {
            base.Value = AbsMod(base.Value + 1, Values.Length);
            return Value;
        }
        public T PreviousValue()
        {
            base.Value = AbsMod(base.Value - 1, Values.Length);
            return Value;
        }
        public SettingsPropertyOptions(T[] values)
        {
            Assert(values.Length > 0);
            this.Values = values;
            this.SetIndex(0);
        }
    }
}
