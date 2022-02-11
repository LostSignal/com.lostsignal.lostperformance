//-----------------------------------------------------------------------
// <copyright file="OptimizerEditorUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Lost
{
    public static class OptimizerEditorUtil
    {
        public static void DrawLODButtons(List<OptimizedLOD> optimizedLODs, bool drawCounts, bool forceRecalculate = false)
        {
            if (optimizedLODs.Count == 0)
            {
                return;
            }

            int maxLODCount = optimizedLODs.Max(x => x.OptimizerSettings.LODSettings.Count);
            float labelWidth = drawCounts ? 90.0f : 55.0f;
            float unoptimizedWidth = 90.0f;
            float simplygonWidth = 70.0f;
            float unityMeshSimplifierWidth = 40.0f;

            // Drawing buttons for each individual LOD
            for (int lod = 1; lod < maxLODCount; lod++)
            {
                var lodObjects = optimizedLODs.Where(x => x.LODIndex == lod && (forceRecalculate || x.State == OptimizeState.Unoptimized)).ToList();

                using (new EditorGUILayout.HorizontalScope())
                {
                    string label = drawCounts ? $"LOD{lod} ({lodObjects.Count}):" : $"LOD{lod}:";
                    EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

                    if (GUILayout.Button("Unoptimized", GUILayout.Width(unoptimizedWidth)))
                    {
                        Unoptimize(lodObjects);
                    }

                    if (GUILayout.Button("UMS", GUILayout.Width(unityMeshSimplifierWidth)))
                    {
                        UnityMeshSimplifierHelper.CalculateLODs(lodObjects);
                    }

                    if (GUILayout.Button("Simplygon", GUILayout.Width(simplygonWidth)))
                    {
                        SimplygonHelper.CalculateLODS(lodObjects);
                    }
                }
            }

            // Drawing Buttons for all LODs
            var allObjects = optimizedLODs.Where(x => x.LODIndex != 0 && (forceRecalculate || x.State == OptimizeState.Unoptimized)).ToList();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(drawCounts ? $"All LODs ({allObjects.Count}):" : $"All LODs:", GUILayout.Width(labelWidth));

                if (GUILayout.Button("Unoptimized", GUILayout.Width(unoptimizedWidth)))
                {
                    Unoptimize(allObjects);
                }

                if (GUILayout.Button("UMS", GUILayout.Width(unityMeshSimplifierWidth)))
                {
                    UnityMeshSimplifierHelper.CalculateLODs(allObjects);
                }

                if (GUILayout.Button("Simplygon", GUILayout.Width(simplygonWidth)))
                {
                    SimplygonHelper.CalculateLODS(allObjects);
                }
            }
        }

        private static EditorCoroutine Unoptimize(List<OptimizedLOD> optimizedLODs)
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(Coroutine());

            IEnumerator Coroutine()
            {
                int totalJobs = optimizedLODs.Count;
                var startTime = DateTime.Now;
                int completedJobs = 0;

                foreach (var optimizedLOD in optimizedLODs)
                {
                    optimizedLOD.Unoptimize();
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
            }
        }
    }
}
