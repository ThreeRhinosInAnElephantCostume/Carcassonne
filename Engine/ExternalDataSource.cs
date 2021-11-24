/*
    *** ExternalDatasource.cs ***
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    ///<summary>
    ///     Sources external data (like tilesets) for the engine
    ///</summary>
    public interface IExternalDataSource
    {
        ITileset GetTileset(string name);
    }
}
