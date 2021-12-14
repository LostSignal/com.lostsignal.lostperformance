//-----------------------------------------------------------------------
// <copyright file="MeshCombineMenuItem.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using UnityEditor;
    using UnityEngine;

    public static class MeshCombineMenuItem
    {
        //// [MenuItem("GameObject/Lost Signal/Create MeshCombineVolume", false, 10)]
        //// public static void CreateMeshCombineVolume(MenuCommand menuCommand)
        //// {
        ////     if (menuCommand.context != Selection.activeGameObject)
        ////     {
        ////         return;
        ////     }
        //// 
        ////     // Getting/Creating the parent object
        ////     var parentName = "Mesh Combine Volumes";
        ////     var parent = GameObject.Find(parentName);
        //// 
        ////     if (parent == null)
        ////     {
        ////         parent = new GameObject(parentName);
        ////     }
        //// 
        ////     // Creating the new MeshCombine Volume
        ////     var newGameObject = new GameObject("Mesh Combine Volume", typeof(MeshCombineVolume));
        ////     newGameObject.transform.SetParent(parent.transform);
        ////     newGameObject.transform.position = GetSelectionPosition();
        //// 
        ////     var bounds = new Bounds(newGameObject.transform.position, Vector3.one);
        //// 
        ////     foreach (var obj in Selection.objects)
        ////     {
        ////         if (obj is GameObject gameObject)
        ////         {
        ////             var renderers = gameObject.GetComponentsInChildren<Renderer>();
        //// 
        ////             if (renderers?.Length > 0)
        ////             {
        ////                 foreach (var renderer in renderers)
        ////                 {
        ////                     bounds.Encapsulate(renderer.bounds);
        ////                 }
        ////             }
        ////         }
        ////     }
        //// 
        ////     var meshCombineVolume = newGameObject.GetOrAddComponent<MeshCombineVolume>();
        ////     meshCombineVolume.SetBoundsSize(bounds.size);
        //// 
        ////     Selection.activeGameObject = newGameObject;
        //// }
        //// 
        //// private static Vector3 GetSelectionPosition()
        //// {
        ////     Vector3 selectionPosition = Vector3.zero;
        ////     float transformCount = 0.0f;
        //// 
        ////     foreach (var obj in Selection.objects)
        ////     {
        ////         if (obj is GameObject gameObject)
        ////         {
        ////             transformCount += 1.0f;
        ////             selectionPosition += gameObject.transform.position;
        ////         }
        ////     }
        //// 
        ////     if (transformCount > 0.0f)
        ////     {
        ////         selectionPosition /= transformCount;
        ////     }
        //// 
        ////     return selectionPosition;
        //// }
    }
}
