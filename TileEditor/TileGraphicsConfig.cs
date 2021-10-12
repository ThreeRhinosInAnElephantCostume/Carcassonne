using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

[Serializable]
public class TileGraphicsConfig
{
    [Serializable]
    public class Config
    {
        public int Rotation { get; set; } = 0;
        public Dictionary<string, int> NodeAssociations { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AttributeAssociations { get; set; } = new Dictionary<string, int>();
        public List<string> Unassociated { get; set; } = new List<string>();

        public void SetNodeAssociaiton(string group, int node)
        {
            if (node == -1)
            {
                if (NodeAssociations.ContainsKey(group))
                    NodeAssociations.Remove(group);
                return;
            }
            if (!NodeAssociations.ContainsKey(group))
                NodeAssociations.Add(group, node);
            else
                NodeAssociations[group] = node;
        }
        public void RemoveNodeAssociation(string group)
        {
            SetNodeAssociaiton(group, -1);
        }
        public void SetAttributeAssociation(string group, int node)
        {
            if (node == -1)
            {
                if (AttributeAssociations.ContainsKey(group))
                    AttributeAssociations.Remove(group);
                return;
            }
            if (!AttributeAssociations.ContainsKey(group))
                AttributeAssociations.Add(group, node);
            else
                AttributeAssociations[group] = node;
        }
        public void SetUnassociated(string group)
        {
            if (!Unassociated.Contains(group))
                Unassociated.Add(group);
        }
        public void RemoveUnassociated(string group)
        {
            if (Unassociated.Contains(group))
                Unassociated.Remove(group);
        }
        public void RemoveAttributeAssociation(string group)
        {
            SetAttributeAssociation(group, -1);
        }
        public void RemoveAssociations(string group)
        {
            RemoveNodeAssociation(group);
            RemoveAttributeAssociation(group);
            RemoveUnassociated(group);
        }

    }
    public string Path { get; set; } = "";
    public Dictionary<string, Config> Configs { get; set; } = new Dictionary<string, Config>();
}
