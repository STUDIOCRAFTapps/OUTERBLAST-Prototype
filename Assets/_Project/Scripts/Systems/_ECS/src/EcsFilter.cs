// ----------------------------------------------------------------------------
// The MIT License
// Simple Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2020 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace Blast.ECS {
#if LEOECS_FILTER_EVENTS
    /// <summary>
    /// Common interface for all filter listeners.
    /// </summary>
    public interface IEcsFilterListener {
        void OnEntityAdded (in EcsEntity entity);
        void OnEntityRemoved (in EcsEntity entity);
    }
#endif
    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public abstract class ComponentFilter {
        protected Entity[] Entities;
        protected readonly Dictionary<int, int> EntitiesMap;
        protected int EntitiesCount;
        protected int LockCount;
        protected readonly int EntitiesCacheSize;

        DelayedOp[] _delayedOps;
        int _delayedOpsCount;
#if LEOECS_FILTER_EVENTS
        protected IEcsFilterListener[] Listeners = new IEcsFilterListener[4];
        protected int ListenersCount;
#endif
        protected internal int[] IncludedTypeIndices;
        protected internal int[] ExcludedTypeIndices;

        public Type[] IncludedTypes;
        public Type[] ExcludedTypes;
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected ComponentFilter (World world) {
            EntitiesCacheSize = world.Config.FilterEntitiesCacheSize;
            Entities = new Entity[EntitiesCacheSize];
            EntitiesMap = new Dictionary<int, int> (EntitiesCacheSize);
            _delayedOps = new DelayedOp[EntitiesCacheSize];
        }
#if DEBUG
        public Dictionary<int, int> GetInternalEntitiesMap () {
            return EntitiesMap;
        }
#endif
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator () {
            return new Enumerator (this);
        }

        /// <summary>
        /// Gets entity by index.
        /// </summary>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Entity GetEntity (in int idx) {
            return ref Entities[idx];
        }

        /// <summary>
        /// Gets entities count.
        /// </summary>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetEntitiesCount () {
            return EntitiesCount;
        }

        /// <summary>
        /// Is filter not contains entities.
        /// </summary>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty () {
            return EntitiesCount == 0;
        }
#if LEOECS_FILTER_EVENTS
        /// <summary>
        /// Subscribes listener to filter events.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void AddListener (IEcsFilterListener listener) {
#if DEBUG
            for (int i = 0, iMax = ListenersCount; i < iMax; i++) {
                if (Listeners[i] == listener) {
                    throw new Exception ("Listener already subscribed.");
                }
            }
#endif
            if (Listeners.Length == ListenersCount) {
                Array.Resize (ref Listeners, ListenersCount << 1);
            }
            Listeners[ListenersCount++] = listener;
        }

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Unsubscribes listener from filter events.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public void RemoveListener (IEcsFilterListener listener) {
            for (int i = 0, iMax = ListenersCount; i < iMax; i++) {
                if (Listeners[i] == listener) {
                    ListenersCount--;
                    // cant fill gap with last element due listeners order is important.
                    Array.Copy (Listeners, i + 1, Listeners, i, ListenersCount - i);
                    break;
                }
            }
        }
#endif
        /// <summary>
        /// Is filter compatible with components on entity with optional added / removed component.
        /// </summary>
        /// <param name="entityData">Entity data.</param>
        /// <param name="addedRemovedTypeIndex">Optional added (greater 0) or removed (less 0) component. Will be ignored if zero.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        internal bool IsCompatible (in World.EcsEntityData entityData, int addedRemovedTypeIndex) {
            var incIdx = IncludedTypeIndices.Length - 1;
            for (; incIdx >= 0; incIdx--) {
                var typeIdx = IncludedTypeIndices[incIdx];
                var idx = entityData.ComponentsCountX2 - 2;
                for (; idx >= 0; idx -= 2) {
                    var typeIdx2 = entityData.Components[idx];
                    if (typeIdx2 == -addedRemovedTypeIndex) {
                        continue;
                    }
                    if (typeIdx2 == addedRemovedTypeIndex || typeIdx2 == typeIdx) {
                        break;
                    }
                }
                // not found.
                if (idx == -2) {
                    break;
                }
            }
            // one of required component not found.
            if (incIdx != -1) {
                return false;
            }
            // check for excluded components.
            if (ExcludedTypeIndices != null) {
                for (var excIdx = 0; excIdx < ExcludedTypeIndices.Length; excIdx++) {
                    var typeIdx = ExcludedTypeIndices[excIdx];
                    for (var idx = entityData.ComponentsCountX2 - 2; idx >= 0; idx -= 2) {
                        var typeIdx2 = entityData.Components[idx];
                        if (typeIdx2 == -addedRemovedTypeIndex) {
                            continue;
                        }
                        if (typeIdx2 == addedRemovedTypeIndex || typeIdx2 == typeIdx) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        protected bool AddDelayedOp (bool isAdd, in Entity entity) {
            if (LockCount <= 0) {
                return false;
            }
            if (_delayedOps.Length == _delayedOpsCount) {
                Array.Resize (ref _delayedOps, _delayedOpsCount << 1);
            }
            ref var op = ref _delayedOps[_delayedOpsCount++];
            op.IsAdd = isAdd;
            op.Entity = entity;
            return true;
        }
#if LEOECS_FILTER_EVENTS
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        protected void ProcessListeners (bool isAdd, in EcsEntity entity) {
            if (isAdd) {
                for (int i = 0, iMax = ListenersCount; i < iMax; i++) {
                    Listeners[i].OnEntityAdded (entity);
                }
            } else {
                for (int i = 0, iMax = ListenersCount; i < iMax; i++) {
                    Listeners[i].OnEntityRemoved (entity);
                }
            }
        }
#endif
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void Lock () {
            LockCount++;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        void Unlock () {
#if DEBUG
            if (LockCount <= 0) {
                throw new Exception ($"Invalid lock-unlock balance for \"{GetType ().Name}\".");
            }
#endif
            LockCount--;
            if (LockCount == 0 && _delayedOpsCount > 0) {
                // process delayed operations.
                for (int i = 0, iMax = _delayedOpsCount; i < iMax; i++) {
                    ref var op = ref _delayedOps[i];
                    if (op.IsAdd) {
                        OnAddEntity (op.Entity);
                    } else {
                        OnRemoveEntity (op.Entity);
                    }
                }
                _delayedOpsCount = 0;
            }
        }

#if DEBUG
        /// <summary>
        /// For debug purposes. Check filters equality by included / excluded components.
        /// </summary>
        /// <param name="filter">Filter to compare.</param>
        internal bool AreComponentsSame (ComponentFilter filter) {
            if (IncludedTypeIndices.Length != filter.IncludedTypeIndices.Length) {
                return false;
            }
            for (var i = 0; i < IncludedTypeIndices.Length; i++) {
                if (Array.IndexOf (filter.IncludedTypeIndices, IncludedTypeIndices[i]) == -1) {
                    return false;
                }
            }
            if ((ExcludedTypeIndices == null && filter.ExcludedTypeIndices != null) ||
                (ExcludedTypeIndices != null && filter.ExcludedTypeIndices == null)) {
                return false;
            }
            if (ExcludedTypeIndices != null) {
                if (filter.ExcludedTypeIndices == null || ExcludedTypeIndices.Length != filter.ExcludedTypeIndices.Length) {
                    return false;
                }
                for (var i = 0; i < ExcludedTypeIndices.Length; i++) {
                    if (Array.IndexOf (filter.ExcludedTypeIndices, ExcludedTypeIndices[i]) == -1) {
                        return false;
                    }
                }
            }
            return true;
        }
#endif

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        public abstract void OnAddEntity (in Entity entity);

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        public abstract void OnRemoveEntity (in Entity entity);

        public struct Enumerator : IDisposable {
            readonly ComponentFilter _filter;
            readonly int _count;
            int _idx;

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            internal Enumerator (ComponentFilter filter) {
                _filter = filter;
                _count = _filter.GetEntitiesCount ();
                _idx = -1;
                _filter.Lock ();
            }

            public int Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _idx;
            }

#if ENABLE_IL2CPP
            [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
            [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public void Dispose () {
                _filter.Unlock ();
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return ++_idx < _count;
            }
        }

        struct DelayedOp {
            public bool IsAdd;
            public Entity Entity;
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public class EcsFilter<Inc1> : ComponentFilter
        where Inc1 : struct {
        int[] _get1;

        readonly bool _allow1;

        readonly EcsComponentPool<Inc1> _pool1;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc1 Get1 (in int idx) {
            return ref _pool1.Items[_get1[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc1> Get1Ref (in int idx) {
            return _pool1.Ref (_get1[idx]);
        }

        /// <summary>
        /// Optimizes filtered data for fast access.
        /// </summary>
        public void Optimize () {
#if DEBUG
            if (LockCount > 0) { throw new Exception ("Can't optimize locked filter."); }
#endif
            OptimizeSort (0, EntitiesCount - 1);
        }

        void OptimizeSort (int left, int right) {
            if (left < right) {
                var q = OptimizeSortPartition (left, right);
                OptimizeSort (left, q - 1);
                OptimizeSort (q + 1, right);
            }
        }

        int OptimizeSortPartition (int left, int right) {
            var pivot = _get1[right];
            var pivotE = Entities[right];
            var i = left;
            for (var j = left; j < right; j++) {
                if (_get1[j] <= pivot) {
                    var c = _get1[j];
                    _get1[j] = _get1[i];
                    _get1[i] = c;
                    var e = Entities[j];
                    Entities[j] = Entities[i];
                    Entities[i] = e;
                    i++;
                }
            }
            _get1[right] = _get1[i];
            _get1[i] = pivot;
            Entities[right] = Entities[i];
            Entities[i] = pivotE;
            return i;
        }
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected EcsFilter (World world) : base (world) {
            _allow1 = !EcsComponentType<Inc1>.IsIgnoreInFilter;
            _pool1 = world.GetPool<Inc1> ();
            _get1 = _allow1 ? new int[EntitiesCacheSize] : null;
            IncludedTypeIndices = new[] {
                EcsComponentType<Inc1>.TypeIndex
            };
            IncludedTypes = new[] {
                EcsComponentType<Inc1>.Type
            };
        }

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnAddEntity (in Entity entity) {
            if (AddDelayedOp (true, entity)) { return; }
            if (Entities.Length == EntitiesCount) {
                var newSize = EntitiesCount << 1;
                Array.Resize (ref Entities, newSize);
                if (_allow1) { Array.Resize (ref _get1, newSize); }
            }
            // inlined and optimized EcsEntity.Get() call.
            ref var entityData = ref entity.Owner.GetEntityData (entity);
            var allow1 = _allow1;
            for (int i = 0, iMax = entityData.ComponentsCountX2, left = 1; left > 0 && i < iMax; i += 2) {
                var typeIdx = entityData.Components[i];
                var itemIdx = entityData.Components[i + 1];
                if (allow1 && typeIdx == EcsComponentType<Inc1>.TypeIndex) {
                    _get1[EntitiesCount] = itemIdx;
                    allow1 = false;
                    left--;
                }
            }
            EntitiesMap[entity.GetInternalId ()] = EntitiesCount;
            Entities[EntitiesCount++] = entity;
#if LEOECS_FILTER_EVENTS
            ProcessListeners (true, entity);
#endif
        }

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveEntity (in Entity entity) {
            if (AddDelayedOp (false, entity)) { return; }
            var entityId = entity.GetInternalId ();
            var idx = EntitiesMap[entityId];
            EntitiesMap.Remove (entityId);
            EntitiesCount--;
            if (idx < EntitiesCount) {
                Entities[idx] = Entities[EntitiesCount];
                EntitiesMap[Entities[idx].GetInternalId ()] = idx;
                if (_allow1) { _get1[idx] = _get1[EntitiesCount]; }
            }
#if LEOECS_FILTER_EVENTS
            ProcessListeners (false, entity);
#endif
        }

        public class Exclude<Exc1> : EcsFilter<Inc1>
            where Exc1 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type
                };
            }
        }

        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1>
            where Exc1 : struct
            where Exc2 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex,
                    EcsComponentType<Exc2>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type,
                    EcsComponentType<Exc2>.Type
                };
            }
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public class EcsFilter<Inc1, Inc2> : ComponentFilter
        where Inc1 : struct
        where Inc2 : struct {
        int[] _get1;
        int[] _get2;

        readonly bool _allow1;
        readonly bool _allow2;

        readonly EcsComponentPool<Inc1> _pool1;
        readonly EcsComponentPool<Inc2> _pool2;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc1 Get1 (in int idx) {
            return ref _pool1.Items[_get1[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc2 Get2 (in int idx) {
            return ref _pool2.Items[_get2[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc1> Get1Ref (in int idx) {
            return _pool1.Ref (_get1[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc2> Get2Ref (in int idx) {
            return _pool2.Ref (_get2[idx]);
        }
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected EcsFilter (World world) : base (world) {
            _allow1 = !EcsComponentType<Inc1>.IsIgnoreInFilter;
            _allow2 = !EcsComponentType<Inc2>.IsIgnoreInFilter;
            _pool1 = world.GetPool<Inc1> ();
            _pool2 = world.GetPool<Inc2> ();
            _get1 = _allow1 ? new int[EntitiesCacheSize] : null;
            _get2 = _allow2 ? new int[EntitiesCacheSize] : null;
            IncludedTypeIndices = new[] {
                EcsComponentType<Inc1>.TypeIndex,
                EcsComponentType<Inc2>.TypeIndex
            };
            IncludedTypes = new[] {
                EcsComponentType<Inc1>.Type,
                EcsComponentType<Inc2>.Type
            };
        }

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnAddEntity (in Entity entity) {
            if (AddDelayedOp (true, entity)) { return; }
            if (Entities.Length == EntitiesCount) {
                var newSize = EntitiesCount << 1;
                Array.Resize (ref Entities, newSize);
                if (_allow1) { Array.Resize (ref _get1, newSize); }
                if (_allow2) { Array.Resize (ref _get2, newSize); }
            }
            // inlined and optimized EcsEntity.Get() call.
            ref var entityData = ref entity.Owner.GetEntityData (entity);
            var allow1 = _allow1;
            var allow2 = _allow2;
            for (int i = 0, iMax = entityData.ComponentsCountX2, left = 2; left > 0 && i < iMax; i += 2) {
                var typeIdx = entityData.Components[i];
                var itemIdx = entityData.Components[i + 1];
                if (allow1 && typeIdx == EcsComponentType<Inc1>.TypeIndex) {
                    _get1[EntitiesCount] = itemIdx;
                    allow1 = false;
                    left--;
                    continue;
                }
                if (allow2 && typeIdx == EcsComponentType<Inc2>.TypeIndex) {
                    _get2[EntitiesCount] = itemIdx;
                    allow2 = false;
                    left--;
                }
            }
            EntitiesMap[entity.GetInternalId ()] = EntitiesCount;
            Entities[EntitiesCount++] = entity;
#if LEOECS_FILTER_EVENTS
            ProcessListeners (true, entity);
#endif
        }

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveEntity (in Entity entity) {
            if (AddDelayedOp (false, entity)) { return; }
            var entityId = entity.GetInternalId ();
            var idx = EntitiesMap[entityId];
            EntitiesMap.Remove (entityId);
            EntitiesCount--;
            if (idx < EntitiesCount) {
                Entities[idx] = Entities[EntitiesCount];
                EntitiesMap[Entities[idx].GetInternalId ()] = idx;
                if (_allow1) { _get1[idx] = _get1[EntitiesCount]; }
                if (_allow2) { _get2[idx] = _get2[EntitiesCount]; }
            }
#if LEOECS_FILTER_EVENTS
            ProcessListeners (false, entity);
#endif
        }

        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2>
            where Exc1 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type
                };
            }
        }

        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2>
            where Exc1 : struct
            where Exc2 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex,
                    EcsComponentType<Exc2>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type,
                    EcsComponentType<Exc2>.Type
                };
            }
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public class EcsFilter<Inc1, Inc2, Inc3> : ComponentFilter
        where Inc1 : struct
        where Inc2 : struct
        where Inc3 : struct {
        int[] _get1;
        int[] _get2;
        int[] _get3;

        readonly bool _allow1;
        readonly bool _allow2;
        readonly bool _allow3;

        readonly EcsComponentPool<Inc1> _pool1;
        readonly EcsComponentPool<Inc2> _pool2;
        readonly EcsComponentPool<Inc3> _pool3;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc1 Get1 (in int idx) {
            return ref _pool1.Items[_get1[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc2 Get2 (in int idx) {
            return ref _pool2.Items[_get2[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc3 Get3 (in int idx) {
            return ref _pool3.Items[_get3[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc1> Get1Ref (in int idx) {
            return _pool1.Ref (_get1[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc2> Get2Ref (in int idx) {
            return _pool2.Ref (_get2[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc3> Get3Ref (in int idx) {
            return _pool3.Ref (_get3[idx]);
        }
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected EcsFilter (World world) : base (world) {
            _allow1 = !EcsComponentType<Inc1>.IsIgnoreInFilter;
            _allow2 = !EcsComponentType<Inc2>.IsIgnoreInFilter;
            _allow3 = !EcsComponentType<Inc3>.IsIgnoreInFilter;
            _pool1 = world.GetPool<Inc1> ();
            _pool2 = world.GetPool<Inc2> ();
            _pool3 = world.GetPool<Inc3> ();
            _get1 = _allow1 ? new int[EntitiesCacheSize] : null;
            _get2 = _allow2 ? new int[EntitiesCacheSize] : null;
            _get3 = _allow3 ? new int[EntitiesCacheSize] : null;
            IncludedTypeIndices = new[] {
                EcsComponentType<Inc1>.TypeIndex,
                EcsComponentType<Inc2>.TypeIndex,
                EcsComponentType<Inc3>.TypeIndex
            };
            IncludedTypes = new[] {
                EcsComponentType<Inc1>.Type,
                EcsComponentType<Inc2>.Type,
                EcsComponentType<Inc3>.Type
            };
        }

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnAddEntity (in Entity entity) {
            if (AddDelayedOp (true, entity)) { return; }
            if (Entities.Length == EntitiesCount) {
                var newSize = EntitiesCount << 1;
                Array.Resize (ref Entities, newSize);
                if (_allow1) { Array.Resize (ref _get1, newSize); }
                if (_allow2) { Array.Resize (ref _get2, newSize); }
                if (_allow3) { Array.Resize (ref _get3, newSize); }
            }
            // inlined and optimized EcsEntity.Get() call.
            ref var entityData = ref entity.Owner.GetEntityData (entity);
            var allow1 = _allow1;
            var allow2 = _allow2;
            var allow3 = _allow3;
            for (int i = 0, iMax = entityData.ComponentsCountX2, left = 3; left > 0 && i < iMax; i += 2) {
                var typeIdx = entityData.Components[i];
                var itemIdx = entityData.Components[i + 1];
                if (allow1 && typeIdx == EcsComponentType<Inc1>.TypeIndex) {
                    _get1[EntitiesCount] = itemIdx;
                    allow1 = false;
                    left--;
                    continue;
                }
                if (allow2 && typeIdx == EcsComponentType<Inc2>.TypeIndex) {
                    _get2[EntitiesCount] = itemIdx;
                    allow2 = false;
                    left--;
                    continue;
                }
                if (allow3 && typeIdx == EcsComponentType<Inc3>.TypeIndex) {
                    _get3[EntitiesCount] = itemIdx;
                    allow3 = false;
                    left--;
                }
            }
            EntitiesMap[entity.GetInternalId ()] = EntitiesCount;
            Entities[EntitiesCount++] = entity;
#if LEOECS_FILTER_EVENTS
            ProcessListeners (true, entity);
#endif
        }

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveEntity (in Entity entity) {
            if (AddDelayedOp (false, entity)) { return; }
            var entityId = entity.GetInternalId ();
            var idx = EntitiesMap[entityId];
            EntitiesMap.Remove (entityId);
            EntitiesCount--;
            if (idx < EntitiesCount) {
                Entities[idx] = Entities[EntitiesCount];
                EntitiesMap[Entities[idx].GetInternalId ()] = idx;
                if (_allow1) { _get1[idx] = _get1[EntitiesCount]; }
                if (_allow2) { _get2[idx] = _get2[EntitiesCount]; }
                if (_allow3) { _get3[idx] = _get3[EntitiesCount]; }
            }
#if LEOECS_FILTER_EVENTS
            ProcessListeners (false, entity);
#endif
        }

        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2, Inc3>
            where Exc1 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type
                };
            }
        }

        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2, Inc3>
            where Exc1 : struct
            where Exc2 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex,
                    EcsComponentType<Exc2>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type,
                    EcsComponentType<Exc2>.Type
                };
            }
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public class EcsFilter<Inc1, Inc2, Inc3, Inc4> : ComponentFilter
        where Inc1 : struct
        where Inc2 : struct
        where Inc3 : struct
        where Inc4 : struct {
        int[] _get1;
        int[] _get2;
        int[] _get3;
        int[] _get4;

        readonly bool _allow1;
        readonly bool _allow2;
        readonly bool _allow3;
        readonly bool _allow4;

        readonly EcsComponentPool<Inc1> _pool1;
        readonly EcsComponentPool<Inc2> _pool2;
        readonly EcsComponentPool<Inc3> _pool3;
        readonly EcsComponentPool<Inc4> _pool4;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc1 Get1 (in int idx) {
            return ref _pool1.Items[_get1[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc2 Get2 (in int idx) {
            return ref _pool2.Items[_get2[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc3 Get3 (in int idx) {
            return ref _pool3.Items[_get3[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc4 Get4 (in int idx) {
            return ref _pool4.Items[_get4[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc1> Get1Ref (in int idx) {
            return _pool1.Ref (_get1[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc2> Get2Ref (in int idx) {
            return _pool2.Ref (_get2[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc3> Get3Ref (in int idx) {
            return _pool3.Ref (_get3[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc4> Get4Ref (in int idx) {
            return _pool4.Ref (_get4[idx]);
        }
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected EcsFilter (World world) : base (world) {
            _allow1 = !EcsComponentType<Inc1>.IsIgnoreInFilter;
            _allow2 = !EcsComponentType<Inc2>.IsIgnoreInFilter;
            _allow3 = !EcsComponentType<Inc3>.IsIgnoreInFilter;
            _allow4 = !EcsComponentType<Inc4>.IsIgnoreInFilter;
            _pool1 = world.GetPool<Inc1> ();
            _pool2 = world.GetPool<Inc2> ();
            _pool3 = world.GetPool<Inc3> ();
            _pool4 = world.GetPool<Inc4> ();
            _get1 = _allow1 ? new int[EntitiesCacheSize] : null;
            _get2 = _allow2 ? new int[EntitiesCacheSize] : null;
            _get3 = _allow3 ? new int[EntitiesCacheSize] : null;
            _get4 = _allow4 ? new int[EntitiesCacheSize] : null;
            IncludedTypeIndices = new[] {
                EcsComponentType<Inc1>.TypeIndex,
                EcsComponentType<Inc2>.TypeIndex,
                EcsComponentType<Inc3>.TypeIndex,
                EcsComponentType<Inc4>.TypeIndex
            };
            IncludedTypes = new[] {
                EcsComponentType<Inc1>.Type,
                EcsComponentType<Inc2>.Type,
                EcsComponentType<Inc3>.Type,
                EcsComponentType<Inc4>.Type
            };
        }

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnAddEntity (in Entity entity) {
            if (AddDelayedOp (true, entity)) { return; }
            if (Entities.Length == EntitiesCount) {
                var newSize = EntitiesCount << 1;
                Array.Resize (ref Entities, newSize);
                if (_allow1) { Array.Resize (ref _get1, newSize); }
                if (_allow2) { Array.Resize (ref _get2, newSize); }
                if (_allow3) { Array.Resize (ref _get3, newSize); }
                if (_allow4) { Array.Resize (ref _get4, newSize); }
            }
            // inlined and optimized EcsEntity.Get() call.
            ref var entityData = ref entity.Owner.GetEntityData (entity);
            var allow1 = _allow1;
            var allow2 = _allow2;
            var allow3 = _allow3;
            var allow4 = _allow4;
            for (int i = 0, iMax = entityData.ComponentsCountX2, left = 4; left > 0 && i < iMax; i += 2) {
                var typeIdx = entityData.Components[i];
                var itemIdx = entityData.Components[i + 1];
                if (allow1 && typeIdx == EcsComponentType<Inc1>.TypeIndex) {
                    _get1[EntitiesCount] = itemIdx;
                    allow1 = false;
                    left--;
                    continue;
                }
                if (allow2 && typeIdx == EcsComponentType<Inc2>.TypeIndex) {
                    _get2[EntitiesCount] = itemIdx;
                    allow2 = false;
                    left--;
                    continue;
                }
                if (allow3 && typeIdx == EcsComponentType<Inc3>.TypeIndex) {
                    _get3[EntitiesCount] = itemIdx;
                    allow3 = false;
                    left--;
                    continue;
                }
                if (allow4 && typeIdx == EcsComponentType<Inc4>.TypeIndex) {
                    _get4[EntitiesCount] = itemIdx;
                    allow4 = false;
                    left--;
                }
            }
            EntitiesMap[entity.GetInternalId ()] = EntitiesCount;
            Entities[EntitiesCount++] = entity;
#if LEOECS_FILTER_EVENTS
            ProcessListeners (true, entity);
#endif
        }

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveEntity (in Entity entity) {
            if (AddDelayedOp (false, entity)) { return; }
            var entityId = entity.GetInternalId ();
            var idx = EntitiesMap[entityId];
            EntitiesMap.Remove (entityId);
            EntitiesCount--;
            if (idx < EntitiesCount) {
                Entities[idx] = Entities[EntitiesCount];
                EntitiesMap[Entities[idx].GetInternalId ()] = idx;
                if (_allow1) { _get1[idx] = _get1[EntitiesCount]; }
                if (_allow2) { _get2[idx] = _get2[EntitiesCount]; }
                if (_allow3) { _get3[idx] = _get3[EntitiesCount]; }
                if (_allow4) { _get4[idx] = _get4[EntitiesCount]; }
            }
#if LEOECS_FILTER_EVENTS
            ProcessListeners (false, entity);
#endif
        }

        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type
                };
            }
        }

        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : struct
            where Exc2 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex,
                    EcsComponentType<Exc2>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type,
                    EcsComponentType<Exc2>.Type
                };
            }
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public class EcsFilter<Inc1, Inc2, Inc3, Inc4, Inc5> : ComponentFilter
        where Inc1 : struct
        where Inc2 : struct
        where Inc3 : struct
        where Inc4 : struct
        where Inc5 : struct {
        int[] _get1;
        int[] _get2;
        int[] _get3;
        int[] _get4;
        int[] _get5;

        readonly bool _allow1;
        readonly bool _allow2;
        readonly bool _allow3;
        readonly bool _allow4;
        readonly bool _allow5;

        readonly EcsComponentPool<Inc1> _pool1;
        readonly EcsComponentPool<Inc2> _pool2;
        readonly EcsComponentPool<Inc3> _pool3;
        readonly EcsComponentPool<Inc4> _pool4;
        readonly EcsComponentPool<Inc5> _pool5;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc1 Get1 (in int idx) {
            return ref _pool1.Items[_get1[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc2 Get2 (in int idx) {
            return ref _pool2.Items[_get2[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc3 Get3 (in int idx) {
            return ref _pool3.Items[_get3[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc4 Get4 (in int idx) {
            return ref _pool4.Items[_get4[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc5 Get5 (in int idx) {
            return ref _pool5.Items[_get5[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc1> Get1Ref (in int idx) {
            return _pool1.Ref (_get1[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc2> Get2Ref (in int idx) {
            return _pool2.Ref (_get2[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc3> Get3Ref (in int idx) {
            return _pool3.Ref (_get3[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc4> Get4Ref (in int idx) {
            return _pool4.Ref (_get4[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc5> Get5Ref (in int idx) {
            return _pool5.Ref (_get5[idx]);
        }
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected EcsFilter (World world) : base (world) {
            _allow1 = !EcsComponentType<Inc1>.IsIgnoreInFilter;
            _allow2 = !EcsComponentType<Inc2>.IsIgnoreInFilter;
            _allow3 = !EcsComponentType<Inc3>.IsIgnoreInFilter;
            _allow4 = !EcsComponentType<Inc4>.IsIgnoreInFilter;
            _allow5 = !EcsComponentType<Inc5>.IsIgnoreInFilter;
            _pool1 = world.GetPool<Inc1> ();
            _pool2 = world.GetPool<Inc2> ();
            _pool3 = world.GetPool<Inc3> ();
            _pool4 = world.GetPool<Inc4> ();
            _pool5 = world.GetPool<Inc5> ();
            _get1 = _allow1 ? new int[EntitiesCacheSize] : null;
            _get2 = _allow2 ? new int[EntitiesCacheSize] : null;
            _get3 = _allow3 ? new int[EntitiesCacheSize] : null;
            _get4 = _allow4 ? new int[EntitiesCacheSize] : null;
            _get5 = _allow5 ? new int[EntitiesCacheSize] : null;
            IncludedTypeIndices = new[] {
                EcsComponentType<Inc1>.TypeIndex,
                EcsComponentType<Inc2>.TypeIndex,
                EcsComponentType<Inc3>.TypeIndex,
                EcsComponentType<Inc4>.TypeIndex,
                EcsComponentType<Inc5>.TypeIndex
            };
            IncludedTypes = new[] {
                EcsComponentType<Inc1>.Type,
                EcsComponentType<Inc2>.Type,
                EcsComponentType<Inc3>.Type,
                EcsComponentType<Inc4>.Type,
                EcsComponentType<Inc5>.Type
            };
        }

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnAddEntity (in Entity entity) {
            if (AddDelayedOp (true, entity)) { return; }
            if (Entities.Length == EntitiesCount) {
                var newSize = EntitiesCount << 1;
                Array.Resize (ref Entities, newSize);
                if (_allow1) { Array.Resize (ref _get1, newSize); }
                if (_allow2) { Array.Resize (ref _get2, newSize); }
                if (_allow3) { Array.Resize (ref _get3, newSize); }
                if (_allow4) { Array.Resize (ref _get4, newSize); }
                if (_allow5) { Array.Resize (ref _get5, newSize); }
            }
            // inlined and optimized EcsEntity.Get() call.
            ref var entityData = ref entity.Owner.GetEntityData (entity);
            var allow1 = _allow1;
            var allow2 = _allow2;
            var allow3 = _allow3;
            var allow4 = _allow4;
            var allow5 = _allow5;
            for (int i = 0, iMax = entityData.ComponentsCountX2, left = 5; left > 0 && i < iMax; i += 2) {
                var typeIdx = entityData.Components[i];
                var itemIdx = entityData.Components[i + 1];
                if (allow1 && typeIdx == EcsComponentType<Inc1>.TypeIndex) {
                    _get1[EntitiesCount] = itemIdx;
                    allow1 = false;
                    left--;
                    continue;
                }
                if (allow2 && typeIdx == EcsComponentType<Inc2>.TypeIndex) {
                    _get2[EntitiesCount] = itemIdx;
                    allow2 = false;
                    left--;
                    continue;
                }
                if (allow3 && typeIdx == EcsComponentType<Inc3>.TypeIndex) {
                    _get3[EntitiesCount] = itemIdx;
                    allow3 = false;
                    left--;
                    continue;
                }
                if (allow4 && typeIdx == EcsComponentType<Inc4>.TypeIndex) {
                    _get4[EntitiesCount] = itemIdx;
                    allow4 = false;
                    left--;
                    continue;
                }
                if (allow5 && typeIdx == EcsComponentType<Inc5>.TypeIndex) {
                    _get5[EntitiesCount] = itemIdx;
                    allow5 = false;
                    left--;
                }
            }
            EntitiesMap[entity.GetInternalId ()] = EntitiesCount;
            Entities[EntitiesCount++] = entity;
#if LEOECS_FILTER_EVENTS
            ProcessListeners (true, entity);
#endif
        }

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveEntity (in Entity entity) {
            if (AddDelayedOp (false, entity)) { return; }
            var entityId = entity.GetInternalId ();
            var idx = EntitiesMap[entityId];
            EntitiesMap.Remove (entityId);
            EntitiesCount--;
            if (idx < EntitiesCount) {
                Entities[idx] = Entities[EntitiesCount];
                EntitiesMap[Entities[idx].GetInternalId ()] = idx;
                if (_allow1) { _get1[idx] = _get1[EntitiesCount]; }
                if (_allow2) { _get2[idx] = _get2[EntitiesCount]; }
                if (_allow3) { _get3[idx] = _get3[EntitiesCount]; }
                if (_allow4) { _get4[idx] = _get4[EntitiesCount]; }
                if (_allow5) { _get5[idx] = _get5[EntitiesCount]; }
            }
#if LEOECS_FILTER_EVENTS
            ProcessListeners (false, entity);
#endif
        }

        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type
                };
            }
        }

        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : struct
            where Exc2 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex,
                    EcsComponentType<Exc2>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type,
                    EcsComponentType<Exc2>.Type
                };
            }
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
#if UNITY_2019_1_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    public class EcsFilter<Inc1, Inc2, Inc3, Inc4, Inc5, Inc6> : ComponentFilter
        where Inc1 : struct
        where Inc2 : struct
        where Inc3 : struct
        where Inc4 : struct
        where Inc5 : struct
        where Inc6 : struct {
        int[] _get1;
        int[] _get2;
        int[] _get3;
        int[] _get4;
        int[] _get5;
        int[] _get6;

        readonly bool _allow1;
        readonly bool _allow2;
        readonly bool _allow3;
        readonly bool _allow4;
        readonly bool _allow5;
        readonly bool _allow6;

        readonly EcsComponentPool<Inc1> _pool1;
        readonly EcsComponentPool<Inc2> _pool2;
        readonly EcsComponentPool<Inc3> _pool3;
        readonly EcsComponentPool<Inc4> _pool4;
        readonly EcsComponentPool<Inc5> _pool5;
        readonly EcsComponentPool<Inc6> _pool6;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc1 Get1 (in int idx) {
            return ref _pool1.Items[_get1[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc2 Get2 (in int idx) {
            return ref _pool2.Items[_get2[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc3 Get3 (in int idx) {
            return ref _pool3.Items[_get3[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc4 Get4 (in int idx) {
            return ref _pool4.Items[_get4[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc5 Get5 (in int idx) {
            return ref _pool5.Items[_get5[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref Inc6 Get6 (in int idx) {
            return ref _pool6.Items[_get6[idx]];
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc1> Get1Ref (in int idx) {
            return _pool1.Ref (_get1[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc2> Get2Ref (in int idx) {
            return _pool2.Ref (_get2[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc3> Get3Ref (in int idx) {
            return _pool3.Ref (_get3[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc4> Get4Ref (in int idx) {
            return _pool4.Ref (_get4[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc5> Get5Ref (in int idx) {
            return _pool5.Ref (_get5[idx]);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsComponentRef<Inc6> Get6Ref (in int idx) {
            return _pool6.Ref (_get6[idx]);
        }
#if UNITY_2019_1_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        protected EcsFilter (World world) : base (world) {
            _allow1 = !EcsComponentType<Inc1>.IsIgnoreInFilter;
            _allow2 = !EcsComponentType<Inc2>.IsIgnoreInFilter;
            _allow3 = !EcsComponentType<Inc3>.IsIgnoreInFilter;
            _allow4 = !EcsComponentType<Inc4>.IsIgnoreInFilter;
            _allow5 = !EcsComponentType<Inc5>.IsIgnoreInFilter;
            _allow6 = !EcsComponentType<Inc6>.IsIgnoreInFilter;
            _pool1 = world.GetPool<Inc1> ();
            _pool2 = world.GetPool<Inc2> ();
            _pool3 = world.GetPool<Inc3> ();
            _pool4 = world.GetPool<Inc4> ();
            _pool5 = world.GetPool<Inc5> ();
            _pool6 = world.GetPool<Inc6> ();
            _get1 = _allow1 ? new int[EntitiesCacheSize] : null;
            _get2 = _allow2 ? new int[EntitiesCacheSize] : null;
            _get3 = _allow3 ? new int[EntitiesCacheSize] : null;
            _get4 = _allow4 ? new int[EntitiesCacheSize] : null;
            _get5 = _allow5 ? new int[EntitiesCacheSize] : null;
            _get6 = _allow6 ? new int[EntitiesCacheSize] : null;
            IncludedTypeIndices = new[] {
                EcsComponentType<Inc1>.TypeIndex,
                EcsComponentType<Inc2>.TypeIndex,
                EcsComponentType<Inc3>.TypeIndex,
                EcsComponentType<Inc4>.TypeIndex,
                EcsComponentType<Inc5>.TypeIndex,
                EcsComponentType<Inc6>.TypeIndex
            };
            IncludedTypes = new[] {
                EcsComponentType<Inc1>.Type,
                EcsComponentType<Inc2>.Type,
                EcsComponentType<Inc3>.Type,
                EcsComponentType<Inc4>.Type,
                EcsComponentType<Inc5>.Type,
                EcsComponentType<Inc6>.Type
            };
        }

        /// <summary>
        /// Event for adding compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnAddEntity (in Entity entity) {
            if (AddDelayedOp (true, entity)) { return; }
            if (Entities.Length == EntitiesCount) {
                var newSize = EntitiesCount << 1;
                Array.Resize (ref Entities, newSize);
                if (_allow1) { Array.Resize (ref _get1, newSize); }
                if (_allow2) { Array.Resize (ref _get2, newSize); }
                if (_allow3) { Array.Resize (ref _get3, newSize); }
                if (_allow4) { Array.Resize (ref _get4, newSize); }
                if (_allow5) { Array.Resize (ref _get5, newSize); }
                if (_allow6) { Array.Resize (ref _get6, newSize); }
            }
            // inlined and optimized EcsEntity.Get() call.
            ref var entityData = ref entity.Owner.GetEntityData (entity);
            var allow1 = _allow1;
            var allow2 = _allow2;
            var allow3 = _allow3;
            var allow4 = _allow4;
            var allow5 = _allow5;
            var allow6 = _allow6;
            for (int i = 0, iMax = entityData.ComponentsCountX2, left = 6; left > 0 && i < iMax; i += 2) {
                var typeIdx = entityData.Components[i];
                var itemIdx = entityData.Components[i + 1];
                if (allow1 && typeIdx == EcsComponentType<Inc1>.TypeIndex) {
                    _get1[EntitiesCount] = itemIdx;
                    allow1 = false;
                    left--;
                    continue;
                }
                if (allow2 && typeIdx == EcsComponentType<Inc2>.TypeIndex) {
                    _get2[EntitiesCount] = itemIdx;
                    allow2 = false;
                    left--;
                    continue;
                }
                if (allow3 && typeIdx == EcsComponentType<Inc3>.TypeIndex) {
                    _get3[EntitiesCount] = itemIdx;
                    allow3 = false;
                    left--;
                    continue;
                }
                if (allow4 && typeIdx == EcsComponentType<Inc4>.TypeIndex) {
                    _get4[EntitiesCount] = itemIdx;
                    allow4 = false;
                    left--;
                    continue;
                }
                if (allow5 && typeIdx == EcsComponentType<Inc5>.TypeIndex) {
                    _get5[EntitiesCount] = itemIdx;
                    allow5 = false;
                    left--;
                    continue;
                }
                if (allow6 && typeIdx == EcsComponentType<Inc6>.TypeIndex) {
                    _get6[EntitiesCount] = itemIdx;
                    allow6 = false;
                    left--;
                }
            }
            EntitiesMap[entity.GetInternalId ()] = EntitiesCount;
            Entities[EntitiesCount++] = entity;
#if LEOECS_FILTER_EVENTS
            ProcessListeners (true, entity);
#endif
        }

        /// <summary>
        /// Event for removing non-compatible entity to filter.
        /// Warning: Don't call manually!
        /// </summary>
        /// <param name="entity">Entity.</param>
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public override void OnRemoveEntity (in Entity entity) {
            if (AddDelayedOp (false, entity)) { return; }
            var entityId = entity.GetInternalId ();
            var idx = EntitiesMap[entityId];
            EntitiesMap.Remove (entityId);
            EntitiesCount--;
            if (idx < EntitiesCount) {
                Entities[idx] = Entities[EntitiesCount];
                EntitiesMap[Entities[idx].GetInternalId ()] = idx;
                if (_allow1) { _get1[idx] = _get1[EntitiesCount]; }
                if (_allow2) { _get2[idx] = _get2[EntitiesCount]; }
                if (_allow3) { _get3[idx] = _get3[EntitiesCount]; }
                if (_allow4) { _get4[idx] = _get4[EntitiesCount]; }
                if (_allow5) { _get5[idx] = _get5[EntitiesCount]; }
                if (_allow6) { _get6[idx] = _get6[EntitiesCount]; }
            }
#if LEOECS_FILTER_EVENTS
            ProcessListeners (false, entity);
#endif
        }

        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2, Inc3, Inc4, Inc5, Inc6>
            where Exc1 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type
                };
            }
        }

        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2, Inc3, Inc4, Inc5, Inc6>
            where Exc1 : struct
            where Exc2 : struct {
#if UNITY_2019_1_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            protected Exclude (World world) : base (world) {
                ExcludedTypeIndices = new[] {
                    EcsComponentType<Exc1>.TypeIndex,
                    EcsComponentType<Exc2>.TypeIndex
                };
                ExcludedTypes = new[] {
                    EcsComponentType<Exc1>.Type,
                    EcsComponentType<Exc2>.Type
                };
            }
        }
    }
}