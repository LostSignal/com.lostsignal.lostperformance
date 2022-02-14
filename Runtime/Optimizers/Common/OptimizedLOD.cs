//-----------------------------------------------------------------------
// <copyright file="OptimizedLOD.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Linq;
    using UnityEngine;

    public class OptimizedLOD : MonoBehaviour
    {
        #if UNITY_EDITOR

        #pragma warning disable 0649
        [ReadOnly] [SerializeField] private int lodIndex;
        [ReadOnly] [SerializeField] private OptimizeState state;

        [SerializeField] private bool overrideSimplygonSettings;
        [SerializeField] private bool overrideUnityMeshSimplifierSettings;

        [ShowIf("overrideSimplygonSettings", true)]
        [SerializeField] private SimplygonSettings simplygonSettings;

        [ShowIf("overrideUnityMeshSimplifierSettings", true)]
        [SerializeField] private UnityMeshSimplifierSettings unityMeshSimplifierSettings;
        #pragma warning restore 0649

        public int LODIndex => this.lodIndex;

        public Optimizer Optimizer => this.GetComponentInParent<Optimizer>();

        public OptimizerSettings OptimizerSettings => this.Optimizer.Settings;

        public UnityMeshSimplifierSettings UnityMeshSimplifierSettings => this.overrideUnityMeshSimplifierSettings ? this.unityMeshSimplifierSettings : this.OptimizerSettings.UnityMeshSimplifierSettings;

        public float Quality => this.OptimizerSettings.LODSettings[this.lodIndex].Quality;

        public OptimizeState State
        {
            get => this.state;
            set => this.state = value;
        }

        public void Initialize()
        {
            if (this.Optimizer == null)
            {
                return;
            }

            // Makding sure our LOD Index is set
            var lodSettings = this.OptimizerSettings.LODSettings;
            var lodSetting = lodSettings.FirstOrDefault(x => x.Name == this.name);

            if (lodSetting != null)
            {
                int index = lodSettings.IndexOf(lodSetting);

                if (this.lodIndex != index)
                {
                    this.lodIndex = index;
                }
            }
            else
            {
                Debug.LogError("Unable to find LOD Index", this);
            }

            // Making sure mesh is set
            if (this.state == OptimizeState.None)
            {
                this.Unoptimize();
            }
        }

        public void Unoptimize()
        {
            MeshCombiner.CreateLOD(this);
            this.state = OptimizeState.Unoptimized;
        }

        private void OnValidate()
        {
            // OnValidate gets called in instantiating in editor, make sure we have a parent Optimizer 
            if (this.Optimizer == null)
            {
                return;
            }

            this.Initialize();
        }

        #endif
    }
}
