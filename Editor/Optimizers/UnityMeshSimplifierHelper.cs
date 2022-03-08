//-----------------------------------------------------------------------
// <copyright file="UnityMeshSimplifierHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEngine;

    public static class UnityMeshSimplifierHelper
    {
        public static EditorCoroutine CalculateLODs(List<OptimizedLOD> optimizedLODs)
        {
#if !USING_UNITY_MESH_SIMPLIFIER

            var add = EditorUtility.DisplayDialog(
                "Add Unity Mesh Simplifier Package?",
                "Could not find the Unity Mesh Simplifier Package.\nWould you like to add that now?",
                "Yes",
                "No");

            if (add)
            {
                PackageManagerUtil.AddGitPackage("com.whinarn.unitymeshsimplifier", "https://github.com/Whinarn/UnityMeshSimplifier.git#v3.0.1");
            }

            return null;

#else

            return EditorCoroutineUtility.StartCoroutineOwnerless(Coroutine());

            IEnumerator Coroutine()
            {
                // https://docs.unity3d.com/2021.2/Documentation/ScriptReference/Progress.html
                // int progressId = Progress.Start("Running one task", 0);

                int totalJobs = optimizedLODs.Count;
                var startTime = DateTime.Now;
                int completedJobs = 0;

                foreach (var optimizedLOD in optimizedLODs)
                {
                    // Progress.Report(progressId, "any progress update or null if it hasn't changed", frame / 1000.0f);

                    if (optimizedLOD.State != OptimizeState.Unoptimized)
                    {
                        optimizedLOD.Unoptimize();
                    }

                    yield return null;

                    CalculateUnityMeshSimplifier(optimizedLOD);

                    completedJobs++;

                    int jobsRemaining = totalJobs - completedJobs;
                    var totalSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
                    var averageSecondsPerJob = totalSeconds / completedJobs;
                    var secondsRemaining = averageSecondsPerJob * jobsRemaining;
                    var timeRemaining = TimingLogger.GetTimeAsString(TimeSpan.FromSeconds(secondsRemaining));

                    Debug.Log($"Finished {completedJobs} of {totalJobs} - {optimizedLOD.gameObject.GetFullName()}.  {timeRemaining} remaining...");

                    yield return null;
                }

                EditorUtil.SaveAll();

                // Progress.Remove(progressId);
            }

#endif
        }

        private static void CalculateUnityMeshSimplifier(OptimizedLOD optimizedLOD)
        {
            ReduceGameObjectWithUnityMeshSimplifier(optimizedLOD.gameObject, optimizedLOD.Quality, optimizedLOD.UnityMeshSimplifierSettings);
            optimizedLOD.State = OptimizeState.UnityMeshSimplifier;

            void ReduceGameObjectWithUnityMeshSimplifier(GameObject gameObject, float quality, UnityMeshSimplifierSettings settings)
            {
                #if USING_UNITY_MESH_SIMPLIFIER
                var existingMeshFilter = gameObject.GetComponent<MeshFilter>();
                var existingMesh = existingMeshFilter.sharedMesh;
                var existingMeshName = existingMesh.name;

                var options = new UnityMeshSimplifier.SimplificationOptions
                {
                    Agressiveness = settings.Agressiveness,
                    EnableSmartLink = settings.EnableSmartLink,
                    ManualUVComponentCount = settings.ManualUVComponentCount,
                    MaxIterationCount = settings.MaxIterationCount,
                    PreserveBorderEdges = settings.PreserveBorderEdges,
                    PreserveSurfaceCurvature = settings.PreserveSurfaceCurvature,
                    PreserveUVFoldoverEdges = settings.PreserveUVFoldoverEdges,
                    PreserveUVSeamEdges = settings.PreserveUVSeamEdges,
                    UVComponentCount = settings.UVComponentCount,
                    VertexLinkDistance = settings.VertexLinkDistance,
                };

                var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
                meshSimplifier.SimplificationOptions = options;
                meshSimplifier.Initialize(existingMesh);
                meshSimplifier.SimplifyMesh(quality);

                var newMesh = meshSimplifier.ToMesh();
                newMesh.RecalculateNormals();
                newMesh.Optimize();
                newMesh.name = existingMeshName;

                existingMesh.Clear();
                UnityEditor.EditorUtility.CopySerialized(newMesh, existingMesh);
                #endif
            }
        }
    }
}
