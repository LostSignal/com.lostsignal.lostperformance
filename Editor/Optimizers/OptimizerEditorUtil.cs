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
        public static void DrawLODButtons(List<OptimizedLOD> optimizedLODs, bool drawCounts)
        {
            if (optimizedLODs.Count == 0)
            {
                return;
            }

            var optimizerCount = optimizedLODs.Select(x => x.Optimizer).Distinct().ToList().Count;

            if (optimizerCount == 1)
            {
                DrawSingleSelect(optimizedLODs, drawCounts, true);
            }
            else
            {
                DrawMultiSelect(optimizedLODs, drawCounts, false);
            }
        }

        private static void DrawMultiSelect(List<OptimizedLOD> optimizedLODs, bool drawCounts, bool forceRecalculate = false)
        {
            int maxLODCount = optimizedLODs.Max(x => x.OptimizerSettings.LODSettings.Count);
            
            float labelWidth = 55.0f;            
            float unoptimizedWidth = drawCounts ? 120.0f : 90.0f;
            float simplygonWidth = drawCounts ? 105.0f : 70.0f;
            float unityMeshSimplifierWidth = drawCounts ? 75.0f : 40.0f;

            // Drawing buttons for each individual LOD
            for (int lod = 1; lod < maxLODCount; lod++)
            {   
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"LOD{lod}:", GUILayout.Width(labelWidth));

                    var nonUnoptimizedLodObjects = optimizedLODs.Where(x => x.LODIndex == lod && (forceRecalculate || x.State != OptimizeState.Unoptimized)).ToList();
                    var unoptimizedLabel = drawCounts ? $"Unoptimized ({nonUnoptimizedLodObjects.Count})" : "Unoptimized";

                    if (GUILayout.Button(unoptimizedLabel, GUILayout.Width(unoptimizedWidth)))
                    {
                        Unoptimize(nonUnoptimizedLodObjects);
                    }

                    var nonUMSOptimizedLodObjects = optimizedLODs.Where(x => x.LODIndex == lod && (forceRecalculate || x.State != OptimizeState.UnityMeshSimplifier)).ToList();
                    var umsLabel = drawCounts ? $"UMS ({nonUMSOptimizedLodObjects.Count})" : "UMS";

                    if (GUILayout.Button(umsLabel, GUILayout.Width(unityMeshSimplifierWidth)))
                    {
                        UnityMeshSimplifierHelper.CalculateLODs(nonUMSOptimizedLodObjects);
                    }

                    var nonSimplygonOptimizedLodObjects = optimizedLODs.Where(x => x.LODIndex == lod && (forceRecalculate || x.State != OptimizeState.Simplygon)).ToList();
                    var simplygonLabel = drawCounts ? $"Simplygon ({nonSimplygonOptimizedLodObjects.Count})" : "Simplygon";

                    if (GUILayout.Button(simplygonLabel, GUILayout.Width(simplygonWidth)))
                    {
                        SimplygonHelper.CalculateLODS(nonSimplygonOptimizedLodObjects);
                    }
                }
            }

            // Drawing Buttons for all LODs
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"All LODs:", GUILayout.Width(labelWidth));

                var nonUnoptimizedLodObjects = optimizedLODs.Where(x => x.LODIndex != 0 && (forceRecalculate || x.State != OptimizeState.Unoptimized)).ToList();
                var unoptimizedLabel = drawCounts ? $"Unoptimized ({nonUnoptimizedLodObjects.Count})" : "Unoptimized";

                if (GUILayout.Button(unoptimizedLabel, GUILayout.Width(unoptimizedWidth)))
                {
                    Unoptimize(nonUnoptimizedLodObjects);
                }

                var nonUMSOoptimizedLodObjects = optimizedLODs.Where(x => x.LODIndex != 0 && (forceRecalculate || x.State != OptimizeState.UnityMeshSimplifier)).ToList();
                var umsLabel = drawCounts ? $"UMS ({nonUMSOoptimizedLodObjects.Count})" : "UMS";

                if (GUILayout.Button(umsLabel, GUILayout.Width(unityMeshSimplifierWidth)))
                {
                    UnityMeshSimplifierHelper.CalculateLODs(nonUMSOoptimizedLodObjects);
                }

                var nonSimplygonOptimizedLodObjects = optimizedLODs.Where(x => x.LODIndex != 0 && (forceRecalculate || x.State != OptimizeState.Simplygon)).ToList();
                var simplygonLabel = drawCounts ? $"Simplygon ({nonSimplygonOptimizedLodObjects.Count})" : "Simplygon";

                if (GUILayout.Button(simplygonLabel, GUILayout.Width(simplygonWidth)))
                {
                    SimplygonHelper.CalculateLODS(nonSimplygonOptimizedLodObjects);
                }
            }
        }

        private static void DrawSingleSelect(List<OptimizedLOD> optimizedLODs, bool drawCounts, bool forceRecalculate = false)
        {
            int maxLODCount = optimizedLODs.Max(x => x.OptimizerSettings.LODSettings.Count);

            float labelWidth = 55.0f;
            float unoptimizedWidth = 90.0f;
            float simplygonWidth = 70.0f;
            float unityMeshSimplifierWidth = 130.0f;

            // Drawing buttons for each individual LOD
            for (int lod = 1; lod < maxLODCount; lod++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"LOD{lod}:", GUILayout.Width(labelWidth));

                    var optimizerLOD = optimizedLODs.First(x => x.LODIndex == lod);

                    if (Button("Unoptimized", unoptimizedWidth, optimizerLOD.State == OptimizeState.Unoptimized))
                    {
                        Unoptimize(new List<OptimizedLOD> { optimizerLOD });
                    }

                    if (Button("Unity Mesh Simplifier", unityMeshSimplifierWidth, optimizerLOD.State == OptimizeState.UnityMeshSimplifier))
                    {
                        UnityMeshSimplifierHelper.CalculateLODs(new List<OptimizedLOD> { optimizerLOD });
                    }

                    if (Button("Simplygon", simplygonWidth, optimizerLOD.State == OptimizeState.Simplygon))
                    {
                        SimplygonHelper.CalculateLODS(new List<OptimizedLOD> { optimizerLOD });
                    }
                }
            }

            GUILayout.Space(10);

            // Drawing Buttons for all LODs
            var allOptimizerLODs = optimizedLODs.Where(x => x.LODIndex != 0).ToList(); 
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"All LODs:", GUILayout.Width(labelWidth));

                if (Button("Unoptimized", unoptimizedWidth, false))
                {
                    Unoptimize(allOptimizerLODs);
                }

                if (Button("Unity Mesh Simplifier", unityMeshSimplifierWidth, false))
                {
                    UnityMeshSimplifierHelper.CalculateLODs(allOptimizerLODs);
                }

                if (Button("Simplygon", simplygonWidth, false))
                {
                    SimplygonHelper.CalculateLODS(allOptimizerLODs);
                }
            }

            bool Button(string text, float width, bool highlighted)
            {
                var oldColor = GUI.backgroundColor;
             
                if (highlighted)
                {
                    GUI.backgroundColor = Color.red;
                }

                bool result = GUILayout.Button(text, GUILayout.Width(width));

                if (highlighted)
                {
                    GUI.backgroundColor = oldColor;
                }

                return result;
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
