//-----------------------------------------------------------------------
// <copyright file="VolumeOptimizer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class VolumeOptimizer : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private List<MeshRenderer> meshRenderers;
        #pragma warning restore 0649
    
        public void SetList(List<MeshRenderer> meshRendereers)
        {
            this.meshRenderers = meshRendereers;
        }
    
        public List<GameObject> GetGameObjectsToCombine(int lodLevel)
        {
            return this.meshRenderers.Select(x => x.gameObject).ToList();
        }
    }
}
