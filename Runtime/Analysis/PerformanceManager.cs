//-----------------------------------------------------------------------
// <copyright file="PerformanceManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using Unity.Profiling;
    using UnityEngine;

    public enum ProfilerCategoryType
    {
        Memory,
        Input,
        Vr,
        Loading,
        Network,
        Lighting,
        Particles,
        Video,
        Audio,
        Ai,
        Animation,
        Physics,
        Gui,
        Scripts,
        Render,
        VirtualTexturing,
        Internal,
    }

#if UNITY

    // https://docs.unity3d.com/ScriptReference/FrameTiming.html
    // https://docs.unity3d.com/ScriptReference/FrameTimingManager.html
    // https://forum.unity.com/threads/profilerrecorder-ms.971322/
    // https://forum.unity.com/threads/update-for-frame-timing-manager.1191877/

    // https://docs.unity3d.com/2020.2/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
    // https://docs.unity3d.com/Packages/com.unity.profiling.core@1.0/manual/profilercounter-guide.html
    // https://docs.unity3d.com/Manual/profiler-markers.html
    // https://docs.unity3d.com/ScriptReference/Profiling.Sampler.html
    // https://resources.unity.com/unitenow/onlinesessions/capturing-profiler-stats-at-runtime
    public class PerformanceManager : SingletonMonoBehaviour<PerformanceManager>, IName
    {
        public static readonly ProfilerCategoryType MyProfilerCategory = ProfilerCategoryType.Scripts;

        public string Name => "Performance Manager";

        public static ProfilerCategory GetCategory(ProfilerCategoryType category)
        {
            switch (category)
            {
                case ProfilerCategoryType.Memory: return ProfilerCategory.Memory;
                case ProfilerCategoryType.Input: return ProfilerCategory.Input;
                case ProfilerCategoryType.Vr: return ProfilerCategory.Vr;
                case ProfilerCategoryType.Loading: return ProfilerCategory.Loading;
                case ProfilerCategoryType.Network: return ProfilerCategory.Network;
                case ProfilerCategoryType.Lighting: return ProfilerCategory.Lighting;
                case ProfilerCategoryType.Particles: return ProfilerCategory.Particles;
                case ProfilerCategoryType.Video: return ProfilerCategory.Video;
                case ProfilerCategoryType.Audio: return ProfilerCategory.Audio;
                case ProfilerCategoryType.Ai: return ProfilerCategory.Ai;
                case ProfilerCategoryType.Animation: return ProfilerCategory.Animation;
                case ProfilerCategoryType.Physics: return ProfilerCategory.Physics;
                case ProfilerCategoryType.Gui: return ProfilerCategory.Gui;
                case ProfilerCategoryType.Scripts: return ProfilerCategory.Scripts;
                case ProfilerCategoryType.Render: return ProfilerCategory.Render;
                case ProfilerCategoryType.VirtualTexturing: return ProfilerCategory.VirtualTexturing;
                case ProfilerCategoryType.Internal: return ProfilerCategory.Internal;
            }

            Debug.LogError($"{nameof(PerformanceManager)} Found Unknown Category Type {category}");
            return default;
        }

        //// * Sends Stats/Analytics
        ////     * FPS Durring Level Load X
        ////     * FPS Durring Level Play
        ////     * FPS Durring Chunk Load X
        ////     * Logs Spewed
        ////     * GC Created
        ////     * LoadBalancer Busy Time for Level X
        ////     * LoadBalancer Busy Time for Chunck X
        ////
        ////
        //// This needs to collect stats every tick as well as send events
        ////    Events
        ////        MeshCollider baked at runtime
        ////        Activation time for scene X
        ////        Debug.LogError/Exception/Warning
        ////        GC Collected when Level Manager wasn't doing it
        ////        AwakeManager Processing Time (Scene X)
        ////        StartManager Processing Time (Scene X)
        ////    Stats
        ////        GC Per Frame
        ////        Total GC
        ////        FPS
        ////        Draw Calls
        ////        SetPass Calls
        ////        CPU Time
        ////        GPU Time
        ////        AwakeManager.Count, StartManager.Count
        ////
        ////        UpdateManager (Mircroseconds)
        ////           Every UpdateChannel (Microseconds)
        ////    Unity Stats
        ////        Physics.Contacts
        ////        Physics.TriggerEnterExits
        ////        Physics.TriggerStays
        ////        Rigidbody.SetKinematic
        ////        GC.Alloc
        ////        GC.Collect
        ////        Gfx.PresentFrame - WaitForTargetFPS
        ////

        ////
        //// public static readonly ProfilerCounter<int> EnemyCount = new ProfilerCounter<int>(
        ////     MyProfilerCategory,
        ////     "Enemy Count",
        ////     ProfilerMarkerDataUnit.Count);
        ////
        //// public static ProfilerCounterValue<int> BulletCount = new ProfilerCounterValue<int>(
        ////     MyProfilerCategory,
        ////     "Bullet Count",
        ////     ProfilerMarkerDataUnit.Count,
        ////     ProfilerCounterOptions.FlushOnEndOfFrame);

        //// Is every stat aggregated over X frames?
        //// If so, do we also give min/max/average?

        //// * Stats Collection System (per scene) - To create FPS over time, maybe a FPS overlay system in editor
        ////    * Based on Unity's new way of gathering performance stats
        ////    * Also sends stats like level activation times

        //// Listen to Debug.Log and
    }

#endif
}

#endif
