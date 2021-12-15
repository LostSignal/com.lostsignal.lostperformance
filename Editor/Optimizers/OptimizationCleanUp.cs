//-----------------------------------------------------------------------
// <copyright file="OptimizationCleanUp.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class OptimizationCleanUp
    {
        [EditorEvents.OnProcessScene]
        public static void CleanUp(Scene scene)
        {
            bool shouldCleanup = Application.isEditor && Application.isPlaying == false && UnityEditor.BuildPipeline.isBuildingPlayer; // && Application.isBatchMode;

            if (shouldCleanup == false)
            {
                return;
            }

            Debug.Log($"OptimizationCleanUp.CleanUp({scene.name}) Started...");

            foreach (var objectOptimizer in GameObject.FindObjectsOfType<ObjectOptimizer>().Where(x => x.gameObject.scene == scene))
            {
                Debug.Log($"OptimizationCleanUp Cleaning Up ObjectOptimizer {objectOptimizer.name}...");
                objectOptimizer.CleanUp();
            }

            foreach (var sceneOptimizer in GameObject.FindObjectsOfType<SceneOptimizer>().Where(x => x.gameObject.scene == scene))
            {
                Debug.Log($"OptimizationCleanUp Cleaning Up SceneOptimizer {sceneOptimizer.name}...");
                sceneOptimizer.CleanUp();
            }
        }
    }
}
