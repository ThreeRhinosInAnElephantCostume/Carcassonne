/*
    *** Action.cs ***

    Definition for the Action class, as well as the entire Action execution system lives here.

    Actions allow for data to represent discrete changes in the engine's state.
    The engine's state can be recreated by repeating a given sequence of actions. 
    See GameEngine.History, GameEngineCreateFromHistory(), and Engine/Serialization.cs
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Carcassonne;
using ExtraMath;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        MethodInfo[] actionmethods = typeof(GameEngine).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(ActionExec), false).Length > 0)
            .ToArray();
        public abstract class Action
        {
            public bool IsFilled { get; protected set; } = false;
        }
        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class ActionExec : System.Attribute
        {
            public Type type { get; protected set; }
            public ActionExec(Type type)
            {
                this.type = type;
            }
        }
        protected delegate void ActionDelegate(Action act);
        public void ExecuteAction(Action action)
        {
            Assert(actionmethods.Length > 0);

            if (!action.IsFilled)
                throw new Exception("Attempting to execute an incomplete action!");

            bool found = false;
            foreach (var it in actionmethods)
            {
                if (((ActionExec)it.GetCustomAttribute(typeof(ActionExec))).type == action.GetType())
                {
                    found = true;

                    ActionDelegate d = (ActionDelegate)Delegate.CreateDelegate(typeof(ActionDelegate), this, it, true);

                    d(action);
                }
            }
            if (!found)
                throw new Exception("Unsupported action!");
            _history.Add(action);
        }
        static Type[] GetAllActionTypes()
        {
            return typeof(GameEngine).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(t => (typeof(Action).IsAssignableFrom(t))).ToArray();
        }
        public static GameEngine CreateFromAction(IExternalDataSource datasource, Action action)
        {
            GameEngine eng = new GameEngine(datasource);
            eng.ExecuteAction(action);
            return eng;
        }
        public static GameEngine CreateFromHistory(IExternalDataSource datasource, List<Action> history)
        {
            GameEngine eng = new GameEngine(datasource);
            foreach (var it in history)
            {
                eng.ExecuteAction(it);
            }
            return eng;
        }
    }
}
