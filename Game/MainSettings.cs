using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
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
    public class AudioSettings : NotifyLink
    {
        public SettingsProperty<double> MasterVolume { get; } = 1;
        public SettingsProperty<double> EffectsVolume { get; } = 0.5;
        public SettingsProperty<double> MusicVolume { get; } = 0.5;
    }
    [Serializable]
    public class MainSettings : NotifyLink
    {
        public SettingsProperty<bool> FullScreen { get; } = false;
        public SettingsProperty<Vector2I> Resolution { get; } = new Vector2I(1280, 720);
        public AudioSettings Audio { get; } = new AudioSettings();
        public void CompleteLoad()
        {
            ClearModified();
            base.init();
        }
        public MainSettings() : base("/")
        {
            foreach (var field in this.GetType().GetFields())
            {
                var v = field.GetValue(this);
                Assert(!(v is NotifyLink) && !(v is NotifyValue), "All NotifyLinks and NotifyValues should be getter-only properties");
            }
            foreach (var prop in this.GetType().GetProperties())
            {
                var v = prop.GetValue(this);
                Assert((!(v is NotifyLink) && !(v is NotifyValue)) || prop.GetSetMethod() == null,
                    "All NotifyLinks and NotifyValues should be getter-only properties");
            }
        }
    }
}
