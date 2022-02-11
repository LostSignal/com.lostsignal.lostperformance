//-----------------------------------------------------------------------
// <copyright file="OptimizedLODGroup.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class OptimizedLODGroup : MonoBehaviour
    {
        public Optimizer Optimizer => this.GetComponentInParent<Optimizer>();

        public OptimizerSettings OptimizerSettings => this.Optimizer.Settings;

        public void UpdateLODGroup()
        {
            var lodGroup = this.GetComponent<LODGroup>();

            if (this.OptimizerSettings.GenerateLODGroup == false)
            {
                if (lodGroup != null)
                {
                    GameObject.DestroyImmediate(lodGroup);
                }

                return;
            }

            if (lodGroup == null)
            {
                lodGroup = this.GetOrAddComponent<LODGroup>();
            }

            var lodGroupLODs = lodGroup.GetLODs();
            var optimizedLODGroups = this.GetComponentsInChildren<OptimizedLOD>().OrderBy(x => x.LODIndex).ToList();
            var lodSettings = this.OptimizerSettings.LODSettings;

            Debug.Assert(lodSettings.Count == optimizedLODGroups.Count, this);

            if (IsLODGroupUpToDate() == false)
            {
                var lods = new List<LOD>();
                
                for (int lodIndex = 0; lodIndex < lodSettings.Count; lodIndex++)
                {
                    lods.Add(new LOD
                    {
                        screenRelativeTransitionHeight = lodSettings[lodIndex].ScreenPercentage,
                        renderers = optimizedLODGroups[lodIndex].GetComponentsInChildren<MeshRenderer>().ToArray(),
                    });
                }
                
                lodGroup.SetLODs(lods.ToArray());
                
                EditorUtil.SetDirty(lodGroup.gameObject);
            }

            bool IsLODGroupUpToDate()
            {
                //// if (lodGroupLODs.Length != optimizedLODGroups.Count)
                //// {
                ////     return false;
                //// }
                //// 
                //// for (int i = 0; i < lodGroupLODs.Length; i++)
                //// {
                ////     if (lodGroupLODs[i].screenRelativeTransitionHeight != optimizedLODGroups[i].OptimizerSettings.LODSettings)
                //// }
                //// 
                return false;
            }
        }
    }
}
