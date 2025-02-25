// ----------------------------------------------------------------------------
// The MIT License
// Simple Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2020 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;

namespace Blast.ECS {
    /// <summary>
    /// Base interface for all systems.
    /// </summary>
    public interface IECSSystem { }


    /// <summary>
    /// Interface for PreInit systems. PreInit() will be called before Init().
    /// </summary>
    public interface IECSPreInitSystem : IECSSystem {
        void PreInit ();
    }

    /// <summary>
    /// Interface for Init systems. Init() will be called before Run().
    /// </summary>
    public interface IECSInitSystem : IECSSystem {
        void Init ();
    }

    /// <summary>
    /// Interface for PostDestroy systems. PostDestroy() will be called after Destroy().
    /// </summary>
    public interface IECSPostDestroySystem : IECSSystem {
        void PostDestroy ();
    }

    /// <summary>
    /// Interface for Destroy systems. Destroy() will be called last in system lifetime cycle.
    /// </summary>
    public interface IECSDestroySystem : IECSSystem {
        void Dispose ();
    }

    /// <summary>
    /// Interface for Run systems.
    /// </summary>
    public interface IECSRunSystem : IECSSystem {
        void Run ();
    }

#if DEBUG
    /// <summary>
    /// Debug interface for systems events processing.
    /// </summary>
    public interface IECSSystemsDebugListener {
        void OnSystemsDestroyed (SystemGroup systems);
    }
#endif

    /// <summary>
    /// Logical group of systems.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public sealed class SystemGroup : IECSInitSystem, IECSDestroySystem, IECSRunSystem {
        public readonly string Name;
        public readonly World World;
        readonly ECSGrowList<IECSSystem> _allSystems = new ECSGrowList<IECSSystem> (64);
        readonly ECSGrowList<EcsSystemsRunItem> _runSystems = new ECSGrowList<EcsSystemsRunItem> (64);
        readonly Dictionary<int, int> _namedRunSystems = new Dictionary<int, int> (64);
        readonly Dictionary<Type, object> _injections = new Dictionary<Type, object> (32);
        bool _injected;
#if DEBUG
        bool _initialized;
        bool _destroyed;
        readonly List<IECSSystemsDebugListener> _debugListeners = new List<IECSSystemsDebugListener> (4);

        /// <summary>
        /// Adds external event listener.
        /// </summary>
        /// <param name="listener">Event listener.</param>
        public void AddDebugListener (IECSSystemsDebugListener listener) {
            if (listener == null) { throw new Exception ("listener is null"); }
            _debugListeners.Add (listener);
        }

        /// <summary>
        /// Removes external event listener.
        /// </summary>
        /// <param name="listener">Event listener.</param>
        public void RemoveDebugListener (IECSSystemsDebugListener listener) {
            if (listener == null) { throw new Exception ("listener is null"); }
            _debugListeners.Remove (listener);
        }
#endif

        /// <summary>
        /// Creates new instance of EcsSystems group.
        /// </summary>
        /// <param name="world">EcsWorld instance.</param>
        /// <param name="name">Custom name for this group.</param>
        public SystemGroup (World world, string name = null) {
            World = world;
            Name = name;
        }

        /// <summary>
        /// Adds new system to processing.
        /// </summary>
        /// <param name="system">System instance.</param>
        /// <param name="namedRunSystem">Optional name of system.</param>
        public SystemGroup Add (IECSSystem system, string namedRunSystem = null) {
#if DEBUG
            if (system == null) { throw new Exception ("System is null."); }
            if (_initialized) { throw new Exception ("Cant add system after initialization."); }
            if (_destroyed) { throw new Exception ("Cant touch after destroy."); }
            if (!string.IsNullOrEmpty (namedRunSystem) && !(system is IECSRunSystem)) { throw new Exception ("Cant name non-IEcsRunSystem."); }
#endif
            _allSystems.Add (system);
            if (system is IECSRunSystem) {
                if (namedRunSystem == null && system is SystemGroup ecsSystems) {
                    namedRunSystem = ecsSystems.Name;
                }
                if (namedRunSystem != null) {
#if DEBUG
                    if (_namedRunSystems.ContainsKey (namedRunSystem.GetHashCode ())) {
                        throw new Exception ($"Cant add named system - \"{namedRunSystem}\" name already exists.");
                    }
#endif
                    _namedRunSystems[namedRunSystem.GetHashCode ()] = _runSystems.Count;
                }
                _runSystems.Add (new EcsSystemsRunItem { Active = true, System = (IECSRunSystem) system });
            }
            return this;
        }

        public int GetNamedRunSystem (string name) {
            return _namedRunSystems.TryGetValue (name.GetHashCode (), out var idx) ? idx : -1;
        }

        /// <summary>
        /// Sets IEcsRunSystem active state.
        /// </summary>
        /// <param name="idx">Index of system.</param>
        /// <param name="state">New state of system.</param>
        public void SetRunSystemState (int idx, bool state) {
#if DEBUG
            if (idx < 0 || idx >= _runSystems.Count) { throw new Exception ("Invalid index"); }
#endif
            _runSystems.Items[idx].Active = state;
        }

        /// <summary>
        /// Gets IEcsRunSystem active state.
        /// </summary>
        /// <param name="idx">Index of system.</param>
        public bool GetRunSystemState (int idx) {
#if DEBUG
            if (idx < 0 || idx >= _runSystems.Count) { throw new Exception ("Invalid index"); }
#endif
            return _runSystems.Items[idx].Active;
        }

        /// <summary>
        /// Get all systems. Important: Don't change collection!
        /// </summary>
        public ECSGrowList<IECSSystem> GetAllSystems () {
            return _allSystems;
        }

        /// <summary>
        /// Gets all run systems. Important: Don't change collection!
        /// </summary>
        public ECSGrowList<EcsSystemsRunItem> GetRunSystems () {
            return _runSystems;
        }

        /// <summary>
        /// Injects instance of object type to all compatible fields of added systems.
        /// </summary>
        /// <param name="obj">Instance.</param>
        /// <param name="overridenType">Overriden type, if null - typeof(obj) will be used.</param>
        public SystemGroup Inject (object obj, Type overridenType = null) {
#if DEBUG
            if (_initialized) { throw new Exception ("Cant inject after initialization."); }
            if (obj == null) { throw new Exception ("Cant inject null instance."); }
            if (overridenType != null && !overridenType.IsInstanceOfType (obj)) { throw new Exception ("Invalid overriden type."); }
#endif
            if (overridenType == null) {
                overridenType = obj.GetType ();
            }
            _injections[overridenType] = obj;
            return this;
        }

        /// <summary>
        /// Processes injections immediately.
        /// Can be used to DI before Init() call.
        /// </summary>
        public SystemGroup ProcessInjects () {
#if DEBUG
            if (_initialized) { throw new Exception ("Cant inject after initialization."); }
            if (_destroyed) { throw new Exception ("Cant touch after destroy."); }
#endif
            if (!_injected) {
                _injected = true;
                for (int i = 0, iMax = _allSystems.Count; i < iMax; i++) {
                    if (_allSystems.Items[i] is SystemGroup nestedSystems) {
                        foreach (var pair in _injections) {
                            nestedSystems._injections[pair.Key] = pair.Value;
                        }
                        nestedSystems.ProcessInjects ();
                    } else {
                        InjectDataToSystem (_allSystems.Items[i], World, _injections);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Registers component type as one-frame for auto-removing at this point in execution sequence.
        /// </summary>
        public SystemGroup OneFrame<T> () where T : struct {
            return Add (new RemoveOneFrame<T> ());
        }

        /// <summary>
        /// Closes registration for new systems, initialize all registered.
        /// </summary>
        public void Init () {
#if DEBUG
            if (_initialized) { throw new Exception ("Already initialized."); }
            if (_destroyed) { throw new Exception ("Cant touch after destroy."); }
#endif
            ProcessInjects ();
            // IEcsPreInitSystem processing.
            for (int i = 0, iMax = _allSystems.Count; i < iMax; i++) {
                var system = _allSystems.Items[i];
                if (system is IECSPreInitSystem preInitSystem) {
                    preInitSystem.PreInit ();
#if DEBUG
                    World.CheckForLeakedEntities ($"{preInitSystem.GetType ().Name}.PreInit()");
#endif
                }
            }
            // IEcsInitSystem processing.
            for (int i = 0, iMax = _allSystems.Count; i < iMax; i++) {
                var system = _allSystems.Items[i];
                if (system is IECSInitSystem initSystem) {
                    initSystem.Init ();
#if DEBUG
                    World.CheckForLeakedEntities ($"{initSystem.GetType ().Name}.Init()");
#endif
                }
            }
#if DEBUG
            _initialized = true;
#endif
        }

        /// <summary>
        /// Processes all IEcsRunSystem systems.
        /// </summary>
        public void Run () {
#if DEBUG
            if (!_initialized) { throw new Exception ($"[{Name ?? "NONAME"}] EcsSystems should be initialized before."); }
            if (_destroyed) { throw new Exception ("Cant touch after destroy."); }
#endif
            for (int i = 0, iMax = _runSystems.Count; i < iMax; i++) {
                var runItem = _runSystems.Items[i];
                if (runItem.Active) {
                    runItem.System.Run ();
                }
#if DEBUG
                if (World.CheckForLeakedEntities (null)) {
                    throw new Exception ($"Empty entity detected, possible memory leak in {_runSystems.Items[i].GetType ().Name}.Run ()");
                }
#endif
            }
        }

        /// <summary>
        /// Destroys registered data.
        /// </summary>
        public void Dispose () {
#if DEBUG
            if (_destroyed) { throw new Exception ("Already destroyed."); }
            _destroyed = true;
#endif
            // IEcsDestroySystem processing.
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                var system = _allSystems.Items[i];
                if (system is IECSDestroySystem destroySystem) {
                    destroySystem.Dispose ();
#if DEBUG
                    World.CheckForLeakedEntities ($"{destroySystem.GetType ().Name}.Destroy ()");
#endif
                }
            }
            // IEcsPostDestroySystem processing.
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                var system = _allSystems.Items[i];
                if (system is IECSPostDestroySystem postDestroySystem) {
                    postDestroySystem.PostDestroy ();
#if DEBUG
                    World.CheckForLeakedEntities ($"{postDestroySystem.GetType ().Name}.PostDestroy ()");
#endif
                }
            }
#if DEBUG
            for (int i = 0, iMax = _debugListeners.Count; i < iMax; i++) {
                _debugListeners[i].OnSystemsDestroyed (this);
            }
#endif
        }

        /// <summary>
        /// Injects custom data to fields of ISystem instance.
        /// </summary>
        /// <param name="system">ISystem instance.</param>
        /// <param name="world">EcsWorld instance.</param>
        /// <param name="injections">Additional instances for injection.</param>
        public static void InjectDataToSystem (IECSSystem system, World world, Dictionary<Type, object> injections) {
            var systemType = system.GetType ();
            var worldType = world.GetType ();
            var filterType = typeof (ComponentFilter);
            var ignoreType = typeof (EcsIgnoreInjectAttribute);

            foreach (var f in systemType.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                // skip statics or fields with [EcsIgnoreInject] attribute.
                if (f.IsStatic || Attribute.IsDefined (f, ignoreType)) {
                    continue;
                }
                // EcsWorld
                if (f.FieldType.IsAssignableFrom (worldType)) {
                    f.SetValue (system, world);
                    continue;
                }
                // EcsFilter
#if DEBUG
                if (f.FieldType == filterType) {
                    throw new Exception ($"Cant use EcsFilter type at \"{system}\" system for dependency injection, use generic version instead");
                }
#endif
                if (f.FieldType.IsSubclassOf (filterType)) {
                    f.SetValue (system, world.GetFilter (f.FieldType));
                    continue;
                }
                // Other injections.
                foreach (var pair in injections) {
                    if (f.FieldType.IsAssignableFrom (pair.Key)) {
                        f.SetValue (system, pair.Value);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// System for removing OneFrame component.
    /// </summary>
    /// <typeparam name="T">OneFrame component type.</typeparam>
    sealed class RemoveOneFrame<T> : IECSRunSystem where T : struct {
        readonly EcsFilter<T> _oneFrames = null;

        void IECSRunSystem.Run () {
            for (var idx = _oneFrames.GetEntitiesCount () - 1; idx >= 0; idx--) {
                _oneFrames.GetEntity (idx).Del<T> ();
            }
        }
    }

    /// <summary>
    /// IEcsRunSystem instance with active state.
    /// </summary>
    public sealed class EcsSystemsRunItem {
        public bool Active;
        public IECSRunSystem System;
    }
}