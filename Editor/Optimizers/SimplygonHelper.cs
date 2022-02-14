//-----------------------------------------------------------------------
// <copyright file="SimplygonHelper.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Lost
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    #if USING_SIMPLYGON
    using Simplygon.Unity.EditorPlugin;
    #endif

    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static class SimplygonHelper
    {
        static SimplygonHelper()
        {
            ProjectDefinesHelper.AddOrRemoveDefine("Simplygon.Unity.EditorPlugin", "USING_SIMPLYGON");
        }

        #if !USING_SIMPLYGON

        public static EditorCoroutine CalculateLODS(List<OptimizedLOD> optimizedLODs)
        {
            SetupSimplygon();
            return null;
        }

        private static void SetupSimplygon()
        {
            var installDirectory = GetInstallDirectory();

            if (installDirectory != null)
            {
                ImportSimplygonDLL(installDirectory);
            }

            static string FindSimplygonInstallFolders()
            {
                var installFolders = new List<string>
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                    Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"),
                };

                foreach (var installFolder in installFolders)
                {
                    var simplygonFolder = System.IO.Path.Combine(installFolder, "Simplygon/9/Unity/bin").Replace("\\", "/");

                    if (System.IO.Directory.Exists(simplygonFolder))
                    {
                        return simplygonFolder;
                    }
                }

                return null;
            }

            static string GetInstallDirectory()
            {
                var simplygonDirectory = FindSimplygonInstallFolders();
                if (simplygonDirectory == null)
                {
                    bool result = EditorUtility.DisplayDialog(
                        "Install Simplygon?",
                        "Unable to find Simplygon install folder, would you like to download it now?",
                        "yes",
                        "no");

                    if (result)
                    {
                        Application.OpenURL("https://www.simplygon.com/Downloads");
                    }
                }

                return simplygonDirectory;
            }

            static void ImportSimplygonDLL(string simplygonInstallDirectory)
            {
                bool result = EditorUtility.DisplayDialog(
                        "Import Simplygon?",
                        "Found Simplygon, but hasn't been added to Unity yet.  Do that now?",
                        "yes",
                        "no");

                if (result)
                {
                    var simplygonEditorDllFileName = "Simplygon.Unity.EditorPlugin.dll";
                    var simplygonDllFilePath = System.IO.Path.Combine(simplygonInstallDirectory, simplygonEditorDllFileName);

                    var pluginsDirectory = "./Assets/Plugins";
                    var editorDirectory = System.IO.Path.Combine(pluginsDirectory, "Editor");
                    var simplygonDirectory = System.IO.Path.Combine(editorDirectory, "Simplygon");
                    var simplygonOutputFilePath = System.IO.Path.Combine(simplygonDirectory, simplygonEditorDllFileName);

                    if (System.IO.Directory.Exists(pluginsDirectory) == false)
                    {
                        System.IO.Directory.CreateDirectory(pluginsDirectory);
                    }

                    if (System.IO.Directory.Exists(editorDirectory) == false)
                    {
                        System.IO.Directory.CreateDirectory(editorDirectory);
                    }

                    if (System.IO.Directory.Exists(simplygonDirectory) == false)
                    {
                        System.IO.Directory.CreateDirectory(simplygonDirectory);
                    }

                    System.IO.File.WriteAllBytes(simplygonOutputFilePath, System.IO.File.ReadAllBytes(simplygonDllFilePath));
                }
            }
        }

        #else

        public static EditorCoroutine CalculateLODS(List<OptimizedLOD> optimizedLODs)
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(Coroutine());

            IEnumerator Coroutine()
            { 
                var startTime = DateTime.Now;
                int totalJobs = optimizedLODs.Count;
                int processedJobs = 0;

                var lods = optimizedLODs.Select(x => x.LODIndex).Distinct().ToList();

                foreach (var lod in lods)
                {
                    var qualities = optimizedLODs.Select(x => x.Quality).Distinct();

                    foreach (var quality in qualities)
                    {
                        var batch = optimizedLODs.Where(x => x.LODIndex == lod && x.Quality == quality).ToList();
                        
                        if (batch.Count == 0)
                        {
                            continue;
                        }

                        batch.ForEach(x => 
                        {
                            if (x.State != OptimizeState.Unoptimized)
                            {
                                x.Unoptimize();
                            }
                        });

                        //// TODO [bgish]: Don't pass in new settings, but do another for loop on distinct settings
                        ReduceGameObjects(batch.Select(x => x.gameObject).ToList(), quality, new SimplygonSettings());

                        processedJobs += batch.Count;

                        // TODO [bgish]: Print out a progress

                        yield return null;
                    }
                }

                yield return null;

                // SimplygonHelper.DeleteTempSimplygonAssetsFolder();                
            }
        }

        private static void ReduceGameObjects(List<GameObject> gameObjects, float quality, SimplygonSettings settings)
        {
            if (gameObjects.Count == 0)
            {
                Debug.LogError("ReduceGameObjects given list of zero game objects.");
                return;
            }

            //// TODO [bgish]: Filter out and print errors if find Meshes with zero verts/tris

            // Creating a single object that will hold copies of the given gameObjects
            var batchObject = new GameObject("Simplygon Batch");
            batchObject.transform.Reset();
            
            var objectsToReduce = new List<GameObject>();
            
            foreach (var gameObject in gameObjects)
            {
                var copy = GameObject.Instantiate(gameObject, batchObject.transform);
                copy.name = gameObject.GetInstanceID().ToString();
                copy.transform.position = gameObject.transform.position;
                copy.transform.rotation = gameObject.transform.rotation;
                copy.transform.localScale = gameObject.transform.localScale;
                
                objectsToReduce.Add(copy);
            }

            // Sending the batch to Simplygon
            GameObject simplygonOutput = null;
            
            simplygonOutput = SimplygonReduce(batchObject, quality, settings);
            
            if (simplygonOutput == null)
            {
                Debug.LogError("Unexcpected error when reducing game objects with Simplygon.");
                return;
            }

            // Updating the original game objects list with the new meshes
            for (int i = 0; i < batchObject.transform.childCount; i++)
            {
                var batchChild = batchObject.transform.GetChild(i);
                var originalInstanceId = int.Parse(batchChild.name);
                var original = (GameObject)EditorUtility.InstanceIDToObject(originalInstanceId);
                var originalMeshFilter = original.GetComponent<MeshFilter>();

                List<GameObject> simplygonTransforms = FindChildGameObject(simplygonOutput.transform, batchChild.name);

                if (simplygonTransforms.Count == 0)
                {
                    Debug.LogError($"Unable to find Simplygon Version of {original.name} with instance id {originalInstanceId}", original);
                    continue;
                }
            
                if (simplygonTransforms.Count > 1)
                {
                    Debug.LogError($"Found too many matching Simplygon Version of {original.name} with instance id {originalInstanceId}", original);
                    continue;
                }
            
                var simplygon = simplygonTransforms.FirstOrDefault();
                var simplygonMeshFilter = simplygon.GetComponent<MeshFilter>();

                if (simplygonMeshFilter == null)
                {
                    Debug.LogError($"Skipping Object {original.name}, simplygon did not produce a mesh filter for this object.", original);
                    continue;
                }

                // Updating Mesh
                var simplygonMesh = simplygonMeshFilter.sharedMesh;
                var originalMesh = originalMeshFilter.sharedMesh;
                UpdateMesh(simplygonMesh, originalMesh);

                // Updating MeshRenderer materials
                var originalMeshRenderer = original.GetComponent<MeshRenderer>();
                var simplygonMeshRenderer = simplygon.GetComponent<MeshRenderer>();
                UpdateMaterials(simplygonMeshRenderer, originalMeshRenderer);

                // Update State
                var optimizedLOD = original.GetComponent<OptimizedLOD>();
                optimizedLOD.State = OptimizeState.Simplygon;

                // Make everything dirty
                EditorUtil.SetDirty(optimizedLOD);
                EditorUtil.SetDirty(original);
                EditorUtil.SetDirty(originalMeshFilter);
                EditorUtil.SetDirty(originalMeshRenderer);
                EditorUtil.SetDirty(originalMesh);
            }

            // Destorying generated game objects
            // GameObject.DestroyImmediate(batchObject);
            // GameObject.DestroyImmediate(simplygonOutput);

            void UpdateMesh(Mesh source, Mesh dest)
            {
                var destName = dest.name;

                dest.Clear();
                EditorUtility.CopySerialized(source, dest);

                dest.name = destName;
            }

            void UpdateMaterials(MeshRenderer simplygon, MeshRenderer original)
            {
                var originalSharedMaterials = original.sharedMaterials;

                var newMaterialsOrder = simplygon.sharedMaterials.Select(x => 
                {
                    int lastIndex = x.name.LastIndexOf('_');
                    var originalMaterialName = x.name.Substring(0, lastIndex);
                    return originalSharedMaterials.First(x => x.name == originalMaterialName);
                });

                original.sharedMaterials = newMaterialsOrder.ToArray();
            }

            List<GameObject> FindChildGameObject(Transform parent, string childToFind)
            {
                List<GameObject> children = new List<GameObject>();
            
                for (int i = 0; i < parent.childCount; i++)
                {
                    var child = parent.GetChild(i);
            
                    if (childToFind.EndsWith(child.name.Replace("_", string.Empty)))
                    {
                        children.Add(child.gameObject);
                    }
                }
            
                return children;
            }
        }

        private static GameObject SimplygonReduce(GameObject gameObject, float quality, SimplygonSettings settings)
        {
            using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
            {
                if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
                {
                    return Reduce(simplygon, quality, gameObject, settings);
                }
                else
                {
                    Debug.Log($"Simplygon Initializing failed with error code {simplygonErrorCode}");
                    return null;
                }
            }
        }

        private static GameObject Reduce(Simplygon.ISimplygon simplygon, float quality, GameObject gameObject, SimplygonSettings settings)
        {
            var exportTempDirectory = SimplygonUtils.GetNewTempDirectory();
            var gameObjects = new List<GameObject> { gameObject };

            using (var sgScene = SimplygonExporter.Export(simplygon, exportTempDirectory, gameObjects))
            using (var reductionPipeline = simplygon.CreateReductionPipeline())
            using (var reductionSettings = reductionPipeline.GetReductionSettings())
            using (var repairSettings = reductionPipeline.GetRepairSettings())
            {
                reductionSettings.SetReductionTargets(Simplygon.EStopCondition.All, true, false, false, false);
                reductionSettings.SetReductionTargetTriangleRatio(quality);

                reductionSettings.SetCurvatureImportance(settings.CurvatureImportance);
                reductionSettings.SetEdgeSetImportance(settings.EdgeSetImportance);
                reductionSettings.SetGeometryImportance(settings.GeometryImportance);
                reductionSettings.SetGroupImportance(settings.GroupImportance);
                reductionSettings.SetMaterialImportance(settings.MaterialImportance);
                reductionSettings.SetShadingImportance(settings.ShadingImportance);
                reductionSettings.SetSkinningImportance(settings.SkinningImportance);
                reductionSettings.SetTextureImportance(settings.TextureImportance);
                reductionSettings.SetVertexColorImportance(settings.VertexColorImportance);

                repairSettings.SetProgressivePasses(settings.ProgressivePasses);
                repairSettings.SetUseWelding(settings.UseWelding);
                repairSettings.SetWeldDist(settings.WeldDist);
                repairSettings.SetUseTJunctionRemover(settings.UseTJunctionRemover);
                repairSettings.SetTJuncDist(settings.TJuncDist);
                repairSettings.SetWeldOnlyBetweenSceneNodes(settings.WeldOnlyBetweenSceneNodes);
                repairSettings.SetWeldOnlyBorderVertices(settings.WeldOnlyBorderVertices);
                repairSettings.SetWeldOnlyWithinMaterial(settings.WeldOnlyWithinMaterial);
                repairSettings.SetWeldOnlyWithinSceneNode(settings.WeldOnlyWithinSceneNode);

                reductionPipeline.RunScene(sgScene, Simplygon.EPipelineRunMode.RunInThisProcess);

                string folderName = "Simplygon Temp Assets";
                string baseFolder = $"Assets/{folderName}";

                if (AssetDatabase.IsValidFolder(baseFolder) == false)
                {
                    AssetDatabase.CreateFolder("Assets", folderName);
                }

                string assetFolderGuid = AssetDatabase.CreateFolder(baseFolder, "Simplygon Output");
                string assetFolderPath = AssetDatabase.GUIDToAssetPath(assetFolderGuid);

                List<GameObject> importedGameObjects = new List<GameObject>();
                int startingLodIndex = 0;
                SimplygonImporter.Import(simplygon, reductionPipeline, ref startingLodIndex, assetFolderPath, "Simplygon Output", importedGameObjects);

                // Cleaning up the temp export folder simplygon generated
                DirectoryUtil.DeleteDirectory(exportTempDirectory);

                //// TODO [bgish]: Need to delete the asset folder created (but might be tricky because we need them to live for a few ticks before destroying them)

                return importedGameObjects.FirstOrDefault();
            }
        }

        public static string GetTempSimplygonAssetsFolder() => $"Assets/Simplygon Temp Assets";

        public static void DeleteTempSimplygonAssetsFolder()
        {
            var folder = GetTempSimplygonAssetsFolder();

            if (AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }
        
        #endif
    }
}

#endif
