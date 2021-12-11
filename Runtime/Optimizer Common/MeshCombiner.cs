//// //-----------------------------------------------------------------------
//// // <copyright file="MeshCombiner.cs" company="Lost Signal LLC">
//// //     Copyright (c) Lost Signal LLC. All rights reserved.
//// // </copyright>
//// //-----------------------------------------------------------------------
//// 
//// 
//// // TODO [bgish]: Track all mesh renders that have been combined and on build, delete them and remove reference
//// // TODO [bgish]: Add check box to delete empty game objects as well
//// 
//// // TODO [bgish]: Make sure we also clean up the temp directory after done importing simplygon assets
//// 
//// namespace Lost
//// {
////     using System;
////     using System.Collections.Generic;
////     using System.Linq;
////     using UnityEngine;
////     using UnityMeshSimplifier;
//// 
////     ////
////     //// Took a lot of inspiration from this blog post http://projectperko.blogspot.com/2016/08/multi-material-mesh-merge-snippet.html
////     ////
//// 
////     public abstract class MeshCombiner : MonoBehaviour
////     {
//// #pragma warning disable 0649
////         [SerializeField] private ObjectOptimizerSettings settings;
//// 
////         [ReadOnly]
////         [SerializeField] private List<GameObject> combinedGameObjects;
//// #pragma warning restore 0649
//// 
////         public abstract List<GameObject> GetGameObjectsToCombine(int lod);
//// 
////         private void OnValidate()
////         {
////             // if (this.settings == null)
////             // {
////             //     this.settings = null; // Get this from the Object Optimizer Settings object
////             // }
////         }
//// 
////         public void CreateLOD(int lodIndex)
////         {
////             var lodTransform = this.GetLODTransform(lodIndex);
////             var finalMeshFilter = lodTransform.gameObject.GetOrAddComponent<MeshFilter>();
////             var finalMeshRenderer = lodTransform.gameObject.GetOrAddComponent<MeshRenderer>();
////             this.SetCombinedMesh(finalMeshFilter, finalMeshRenderer, lodIndex);
////         }
//// 
////         public void SetCombinedMesh(MeshFilter finalMeshFilter, MeshRenderer finalMeshRenderer, int lodIndex)
////         {
////             // This is used for reverting purposes only
////             if (this.combinedGameObjects.IsNullOrEmpty())
////             {
////                 this.combinedGameObjects = this.GetGameObjectsToCombine(0);
////             }
//// 
////             var objectsToCombine = this.GetGameObjectsToCombine(lodIndex);
////             var meshRenderers = objectsToCombine.Select(x => x.GetComponent<MeshRenderer>()).ToList();
////             var meshFilters = objectsToCombine.Select(x => x.GetComponent<MeshFilter>()).ToList();
////             var materials = new List<Material>();
//// 
////             // Making sure our arrays are value
////             if (meshRenderers.Count != meshFilters.Count)
////             {
////                 Debug.LogError("SOMETHING WHEN HORRIBLY WRONG");
////                 return;
////             }
//// 
////             // Collecting all unique materials
////             foreach (MeshRenderer meshRenderer in meshRenderers)
////             {
////                 foreach (Material material in meshRenderer.sharedMaterials)
////                 {
////                     materials.AddIfNotNullAndUnique(material);
////                 }
////             }
//// 
////             var submeshes = new List<Mesh>();
////             var vertexCount = 0;
//// 
////             foreach (Material material in materials)
////             {
////                 // Make a combiner for each (sub)mesh that is mapped to the right material.
////                 var combiners = new List<CombineInstance>();
//// 
////                 for (int i = 0; i < meshRenderers.Count; i++)
////                 {
////                     var meshRenderer = meshRenderers[i];
////                     var meshFilter = meshFilters[i];
////                     int materialIndex = Array.IndexOf(meshRenderer.sharedMaterials, material);
//// 
////                     if (materialIndex != -1 && meshFilter.sharedMesh != null)
////                     {
////                         vertexCount += meshFilter.sharedMesh.vertexCount;
//// 
////                         combiners.Add(new CombineInstance
////                         {
////                             mesh = meshFilter.sharedMesh,
////                             subMeshIndex = materialIndex,
////                             transform = this.transform.worldToLocalMatrix * meshRenderer.transform.localToWorldMatrix,
////                         });
////                     }
////                 }
//// 
////                 // Flatten into a single mesh.
////                 Mesh mesh = new Mesh();
////                 mesh.indexFormat = vertexCount >= ushort.MaxValue ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
////                 mesh.CombineMeshes(combiners.ToArray(), true);
////                 submeshes.Add(mesh);
////             }
//// 
////             int vertCount = 0;
//// 
////             // The final mesh: combine all the material-specific meshes as independent submeshes.
////             var finalCombiners = new List<CombineInstance>();
////             foreach (Mesh mesh in submeshes)
////             {
////                 vertCount += mesh.vertexCount;
//// 
////                 finalCombiners.Add(new CombineInstance
////                 {
////                     mesh = mesh,
////                     subMeshIndex = 0,
////                     transform = Matrix4x4.identity,
////                 });
////             }
//// 
////             // Disabling all the MeshRenderers (MeshRenderers and MeshFilters will be destoryed on build)
////             for (int i = 0; i < meshRenderers.Count; i++)
////             {
////                 meshRenderers[i].enabled = false;
////             }
//// 
////             var finalMesh = new Mesh();
////             finalMesh.indexFormat = vertCount >= ushort.MaxValue ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
////             finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
//// 
////             finalMeshFilter.sharedMesh = finalMesh;
////             finalMeshRenderer.sharedMaterials = materials.ToArray();
//// 
////             #if UNITY_EDITOR
////             UnityEditor.EditorUtility.SetDirty(finalMeshFilter.gameObject);
////             UnityEditor.EditorUtility.SetDirty(finalMeshRenderer.gameObject);
////             #endif
////         }
//// 
////         public void Revert()
////         {
////             foreach (var combinedGameObject in this.combinedGameObjects)
////             {
////                 combinedGameObject.GetComponent<MeshRenderer>().enabled = true;
////             }
//// 
////             this.combinedGameObjects.Clear();
////             var lods = this.transform.Find("LODS");
//// 
////             if (lods != null)
////             {
////                 GameObject.DestroyImmediate(lods.gameObject);
////             }
////         }
//// 
////         public void SimplifyLODMesh(int lodIndex)
////         {
////             if (lodIndex >= this.settings.LODSettings.Count)
////             {
////                 return;
////             }
//// 
////             var lodTransform = this.GetLODTransform(lodIndex);
////             var finalMeshFilter = lodTransform.gameObject.GetOrAddComponent<MeshFilter>();
////             var finalMeshRenderer = lodTransform.gameObject.GetOrAddComponent<MeshRenderer>();
////             this.SetCombinedMesh(finalMeshFilter, finalMeshRenderer, lodIndex);
//// 
////             var lodSetting = this.settings.LODSettings[lodIndex];
//// 
////             if (lodSettings.Quality == 1.0f)
////             {
////                 return;
////             }
//// 
////             foreach (var meshFilter in lodTransform.GetComponentsInChildren<MeshFilter>())
////             {
////                 var newMesh = this.CreateNewMesh(lodSetting, meshFilter);
//// 
////                 if (newMesh != null)
////                 {
////                     meshFilter.mesh = newMesh;
////                 }
////             }
////         }
//// 
////         private Mesh CreateNewMesh(ObjectOptimizerSettings.LODSetting lodSettings, MeshFilter meshFilter)
////         {
////             #if UNITY_EDITOR
//// 
////             if (lodSettings.Simplifier == LODSettings.MeshSimplifier.UnityMeshSimplifier)
////             {
////                 // TODO [bgish]: Need to wrap in USING_UNITY_MESH_SIMPLIFIER and print error (not installed if it fails)
////                 // Also, add "com.whinarn.unitymeshsimplifier": "https://github.com/Whinarn/UnityMeshSimplifier.git#v3.0.1", to your manifest
////                 var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
////                 meshSimplifier.SimplificationOptions = lodSettings.Options;
////                 meshSimplifier.Initialize(meshFilter.mesh);
////                 meshSimplifier.SimplifyMesh(lodSettings.Quality);
////         
////                 var newMesh = meshSimplifier.ToMesh();
////                 newMesh.RecalculateNormals();
////                 newMesh.Optimize();
////                 return newMesh;
////             }
////             else if (lodSettings.Simplifier == LODSettings.MeshSimplifier.Simplygon)
////             {
////                 // if so, initialize Simplygon
////                 using (Simplygon.ISimplygon simplygon = global::Simplygon.Loader.InitSimplygon(out Simplygon.EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
////                 {
////                     // if Simplygon handle is valid, loop all selected objects
////                     // and call Reduce function.
////                     if (simplygonErrorCode == Simplygon.EErrorCodes.NoError)
////                     {
////                         Reduce(meshFilter, simplygon, lodSettings.Quality);
////                     }
//// 
////                     // if invalid handle, output error message to the Unity console
////                     else
////                     {
////                         Debug.Log("Initializing failed!");
////                     }
////                 }
//// 
////                 return null;
////             }
////             else
////             {
////                 Debug.LogError($"Unknown Mesh Simplifier {lodSettings.Simplifier} found!");
////                 return null;
////             }
////             #else
//// 
////             return null;
//// 
////             #endif
////         }
//// 
////         #if UNITY_EDITOR
//// 
////         public static void Reduce(MeshFilter originalMeshFilter, Simplygon.ISimplygon simplygon, float quality)
////         {
////             var gameObject = originalMeshFilter.gameObject;
////             List<GameObject> selectedGameObjects = new List<GameObject>();
////             selectedGameObjects.Add(gameObject);
//// 
////             string exportTempDirectory = Simplygon.Unity.EditorPlugin.SimplygonUtils.GetNewTempDirectory();
////             Debug.Log(exportTempDirectory);
//// 
////             using (Simplygon.spScene sgScene = Simplygon.Unity.EditorPlugin.SimplygonExporter.Export(simplygon, exportTempDirectory, selectedGameObjects))
////             {
////                 using (Simplygon.spReductionPipeline reductionPipeline = simplygon.CreateReductionPipeline())
////                 using (Simplygon.spReductionSettings reductionSettings = reductionPipeline.GetReductionSettings())
////                 {
////                     reductionSettings.SetReductionTargets(Simplygon.EStopCondition.All, true, false, false, false);
////                     reductionSettings.SetReductionTargetTriangleRatio(quality);
//// 
////                     reductionPipeline.RunScene(sgScene, Simplygon.EPipelineRunMode.RunInThisProcess);
//// 
////                     string baseFolder = "Assets/SimpleReductions";
////                     if (UnityEditor.AssetDatabase.IsValidFolder(baseFolder) == false)
////                     {
////                         UnityEditor.AssetDatabase.CreateFolder("Assets", "SimpleReductions");
////                     }
//// 
////                     string assetFolderGuid = UnityEditor.AssetDatabase.CreateFolder(baseFolder, gameObject.name);
////                     string assetFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetFolderGuid);
//// 
////                     List<GameObject> importedGameObjects = new List<GameObject>();
////                     int startingLodIndex = 0;
////                     Simplygon.Unity.EditorPlugin.SimplygonImporter.Import(simplygon, reductionPipeline, ref startingLodIndex, assetFolderPath, gameObject.name, importedGameObjects);
//// 
////                     if (importedGameObjects.Count > 0)
////                     {
////                         var importedMeshFilter = importedGameObjects[0].GetComponent<MeshFilter>();
//// 
////                         if (importedMeshFilter != null && importedMeshFilter.sharedMesh != null)
////                         {
////                             var assetPath = UnityEditor.AssetDatabase.GetAssetPath(importedMeshFilter.sharedMesh);
////                             var newMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
////                             var meshCopy = GameObject.Instantiate(newMesh);
//// 
////                             UnityEditor.EditorApplication.delayCall += () =>
////                             {
////                                 originalMeshFilter.sharedMesh = meshCopy;
////                                 UnityEditor.EditorUtility.SetDirty(originalMeshFilter.gameObject);
////                             };                            
////                         }
////                     }
//// 
////                     foreach (var importedObject in importedGameObjects)
////                     {
////                         GameObject.DestroyImmediate(importedObject);
////                     }
////                 }
////             }
////         }
//// 
////         #endif
//// 
////         public void SeperateMesh(int lodIndex)
////         {
////             if (lodIndex >= this.lodSettings.Count)
////             {
////                 return;
////             }
//// 
////             var lod = this.transform.Find($"LODS/{this.lodSettings[lodIndex].Name}");
//// 
////             if (lod != null)
////             {
////                 this.SeperateMesh(lod);
////             }
////         }
////         
////         private void SeperateMesh(Transform meshTransfrom)
////         {
////             var originalMeshFilter = meshTransfrom.GetComponent<MeshFilter>();
////             var originalMeshRenderer = meshTransfrom.GetComponent<MeshRenderer>();
////             var meshRendererCopies = new List<MeshRenderer>();
////         
////             for (int i = 0; i < originalMeshRenderer.sharedMaterials.Length; i++)
////             {
////                 var subMesh = new GameObject(originalMeshRenderer.sharedMaterials[i].name);
////                 subMesh.transform.SetParent(meshTransfrom);
////                 subMesh.transform.Reset();
//// 
////                 #if UNITY_EDITOR
////                 UnityEditorInternal.ComponentUtility.CopyComponent(originalMeshFilter);
////                 UnityEditorInternal.ComponentUtility.PasteComponentAsNew(subMesh);
////         
////                 UnityEditorInternal.ComponentUtility.CopyComponent(originalMeshRenderer);
////                 UnityEditorInternal.ComponentUtility.PasteComponentAsNew(subMesh);
////                 #endif
//// 
////                 meshRendererCopies.Add(subMesh.GetComponent<MeshRenderer>());
////             }
////         
////             // Making sure each copy only has 1 material that it's concered about
////             for (int i = 0; i < meshRendererCopies.Count; i++)
////             {
////                 meshRendererCopies[i].sharedMaterials = new Material[] { originalMeshRenderer.sharedMaterials[i] };
////         
////                 // Now removing all sub meshes that aren't needed for this new mesh
////                 var meshFilter = meshRendererCopies[i].GetComponent<MeshFilter>();
////                 var newMesh = Mesh.Instantiate(meshFilter.sharedMesh);
////                 newMesh.triangles = meshFilter.sharedMesh.GetTriangles(i);
////                 newMesh.Optimize();
////         
////                 meshFilter.mesh = newMesh;
////             }
////         
////             // Destorying the Originals
////             GameObject.DestroyImmediate(originalMeshRenderer);
////             GameObject.DestroyImmediate(originalMeshFilter);
////         }
////         
////         public void GenerateLODGroup()
////         {
////             var lods = new List<LOD>();
////         
////             for (int lodIndex = 0; lodIndex < this.lodSettings.Count; lodIndex++)
////             {
////                 lods.Add(new LOD
////                 {
////                     screenRelativeTransitionHeight = this.lodSettings[lodIndex].ScreenPercentage,
////                     renderers = this.GetLODTransform(lodIndex).GetComponentsInChildren<MeshRenderer>().ToArray(),
////                 });
////             }
////         
////             var lodGroup = this.GetLODSTransform().gameObject.GetOrAddComponent<LODGroup>();
////             lodGroup.SetLODs(lods.ToArray());
//// 
////             #if UNITY_EDITOR
////             UnityEditor.EditorUtility.SetDirty(this.gameObject);
////             #endif
////         }
//// 
////         private Transform GetLODSTransform()
////         {
////             // Making sure LODS child is created
////             var lods = this.transform.Find("LODS");
//// 
////             if (lods == null)
////             {
////                 lods = new GameObject("LODS", typeof(IgnoreMeshCombine)).transform;
////                 lods.SetParent(this.transform);
////                 lods.Reset();
////             }
//// 
////             return lods;
////         }
//// 
////         private Transform GetLODTransform(int lodIndex)
////         {
////             if (lodIndex >= this.lodSettings.Count)
////             {
////                 return null;
////             }
//// 
////             var lodsTransform = this.GetLODSTransform();
////             var lodName = this.lodSettings[lodIndex].Name;
////             var lod = this.transform.Find($"LODS/{lodName}");
//// 
////             if (lod == null)
////             {
////                 lod = new GameObject(lodName).transform;
////                 lod.SetParent(lodsTransform);
////                 lod.SetSiblingIndex(lodIndex);
////                 lod.Reset();
////             }
//// 
////             return lod;
////         }
//// 
////     }
//// }
