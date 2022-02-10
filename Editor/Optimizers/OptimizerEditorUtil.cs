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
        public static void DrawLODButtons(List<Optimizer> optimizers, bool drawCounts)
        {
            var lodCounts = new Dictionary<int, int>();
            int maxLODs = 0;

            int maxLODCount = 0;

            foreach (var optimizer in optimizers)
            {
                maxLODCount = Math.Max(optimizer.LODs.Count, maxLODCount);

                foreach (var lod in optimizer.LODSToCalculate)
                {
                    if (lodCounts.ContainsKey(lod) == false)
                    {
                        lodCounts.Add(lod, 1);
                    }
                    else
                    {
                        lodCounts[lod]++;
                    }
                }

                maxLODs = Math.Max(maxLODs, optimizer.LODs.Count);
            }

            for (int i = 0; i < maxLODCount; i++)
            {
                if (lodCounts.ContainsKey(i) == false)
                { 
                    lodCounts.Add(i, 0);
                }
            }

            var lods = lodCounts.Keys.OrderBy(x => x).ToList();
            int total = 0;

            float labelWidth = drawCounts ? 90.0f : 55.0f;
            float unoptimizedWidth = 90.0f;
            float simplygonWidth = 70.0f;
            float unityMeshSimplifierWidth = 40.0f;

            // Drawing buttons for each individual LOD
            for (int lod = 1; lod < maxLODs; lod++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string label = drawCounts ? $"LOD{lod} ({lodCounts[lod]}):" : $"LOD{lod}:";
                    EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

                    if (GUILayout.Button("Unoptimized", GUILayout.Width(unoptimizedWidth)))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(CalculateLODs(optimizers, Optimizer.Method.Unoptimized, lod));
                    }

                    if (GUILayout.Button("Simplygon", GUILayout.Width(simplygonWidth)))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(CalculateLODs(optimizers, Optimizer.Method.Simplygon, lod));
                    }

                    if (GUILayout.Button("UMS", GUILayout.Width(unityMeshSimplifierWidth)))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(CalculateLODs(optimizers, Optimizer.Method.UnityMeshSimplifier, lod));
                    }
                }

                total += lodCounts[lod];
            }

            // Drawing Buttons for all LODs
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(drawCounts ? $"All LODs ({total}):" : $"All LODs:", GUILayout.Width(labelWidth));

                if (GUILayout.Button("Unoptimized", GUILayout.Width(unoptimizedWidth)))
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(CalculateLODs(optimizers, Optimizer.Method.Unoptimized));
                }

                if (GUILayout.Button("Simplygon", GUILayout.Width(simplygonWidth)))
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(CalculateLODs(optimizers, Optimizer.Method.Simplygon));
                }

                if (GUILayout.Button("UMS", GUILayout.Width(unityMeshSimplifierWidth)))
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(CalculateLODs(optimizers, Optimizer.Method.UnityMeshSimplifier));
                }
            }
        }

        private static IEnumerator CalculateLODs(List<Optimizer> optimizers, Optimizer.Method method, int forceLOD = -1)
        {
            // https://docs.unity3d.com/2021.2/Documentation/ScriptReference/Progress.html
            // int progressId = Progress.Start("Running one task", 0);

            int totalJobs = 0;

            if (forceLOD != -1)
            {
                totalJobs = optimizers.Sum(x => x.LODSToCalculate.Contains(forceLOD) ? 1 : 0);
            }
            else
            {
                totalJobs = optimizers.Sum(x => x.LODSToCalculateCount);
            }

            var startTime = DateTime.Now;

            for (int i = 0; i < optimizers.Count; i++)
            {
                var optimizer = optimizers[i];
                var gameObject = optimizer.gameObject;

                // Progress.Report(progressId, "any progress update or null if it hasn't changed", frame / 1000.0f);

                List<int> lodsToProcess = new List<int>();

                if (forceLOD == -1)
                {
                    if (method == Optimizer.Method.Unoptimized)
                    {
                        for (int j = 1; j < optimizer.LODs.Count; j++)
                        {
                            lodsToProcess.Add(j);
                        }
                    }
                    else
                    {
                        lodsToProcess.AddRange(optimizer.LODSToCalculate);
                    }
                }
                else
                {
                    lodsToProcess.Add(forceLOD);
                }

                foreach (var lod in lodsToProcess)
                {
                    optimizer.OptimizeLOD(lod, method);
                }

                if (i > 1 && i % 50 == 0)
                {
                    EditorUtil.SaveAll();
                }

                int completedJobs = i + 1;
                int jobsRemaining = totalJobs - completedJobs;
                var totalSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
                var averageSecondsPerJob = totalSeconds / completedJobs;
                var secondsRemaining = averageSecondsPerJob * jobsRemaining;
                var timeRemaining = TimingLogger.GetTimeAsString(TimeSpan.FromSeconds(secondsRemaining));

                Debug.Log($"Finished Simplygon {i + 1} of {totalJobs} - {gameObject.name}.  {timeRemaining} remaining...");

                yield return null;
            }

            yield return null;
            yield return null;
            yield return null;

            EditorUtil.SaveAll();

            yield return null;
            yield return null;
            yield return null;

            SimplygonHelper.DeleteTempSimplygonAssetsFolder();

            // Progress.Remove(progressId);
        }
    }
}
