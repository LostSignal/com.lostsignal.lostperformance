//-----------------------------------------------------------------------
// <copyright file="SceneValidator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    //// ### Scene Validator
    ////   * Make custom window that holds all the errors (instead of printing them to the screen)
    ////   * Each Error has an "Ignore" and "Fix" buttons (if it can fix it)
    ////     * Usefull for things like "Uneccessary Raycast Target Validator"
    ////
    //// * Missing Behaviour Errors
    ////   * The referenced script on this Behaviour (Game Object 'Oroshi Footstep Decal') is missing!
    //// * Bad Collider Error
    //// * Bad Scale Error
    //// * FPS Below 60 Error
    //// * General LogError/Exception/Warning Error (if not one of the above)
    ////
    //// ### Build Validation
    //// * If build spits out any Debug.LogError like this, then it needs to be reported
    ////   * Failed to create Physics Mesh from source mesh "Roof_LOD0". One of the triangles is too large. It's recommended to tesselate the large triangles.
    ////     UnityEngine.GUIUtility:ProcessEvent (int,intptr)
    //// * Should this be spit out to a slack channel?
    //// * When entering playmode should we put a popup in the face of the user????
    //// * Any code warnings should be errors that need action items to fix
    //// * Any physics collision runtime rebuilds must be errors in this validation system (must listen to console out to catch these)
    ////   * Can I use Preprocess Build and PostProcess build to catch these?
    ////   * Is the float PreProcessBuild -> Process Scene(s) -> PostProcessBuild????
    ////   * What about scene that live in addressables... Is Process Scene called on them too?
    ////
    //// * Can I make a Scene Validator that finds all UnityEvents in a scene and makes sure if they
    ////   reference a GameObjectState object that the StateName parameter is valid?
    ////
    ////
    ////

    public class SceneValidator : MonoBehaviour
    {
        #if UNITY_EDITOR

        #pragma warning disable 0649
        [SerializeReference] private List<Validator> validators;
        #pragma warning restore 0649

        public List<Validator> Validators => this.validators;

        public void ProcessAll()
        {
            var errors = new List<ValidationError>();

            foreach (var validator in this.validators)
            {
                if (validator.IsActive == false)
                {
                    continue;
                }

                validator.Run(this.gameObject.scene, errors);
            }

            this.PrintErrors(errors);
        }

        public void ForceProcessIndex(Scene scene, int index)
        {
            var errors = new List<ValidationError>();
            this.validators[index].Run(scene, errors);
            this.PrintErrors(errors);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        [EditorEvents.OnProcessScene]
        private static void OnProcessScene(Scene scene)
        {
            var sceneValidators = GameObject.FindObjectsOfType<SceneValidator>();

            foreach (var sceneValidator in sceneValidators)
            {
                if (sceneValidator.gameObject.scene == scene)
                {
                    sceneValidator.ProcessAll();
                }
            }
        }

        private void OnValidate()
        {
            this.validators ??= new List<Validator>();

            var validatorClassTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<Validator>();

            bool validatorsChanged = false;

            foreach (var validatorClassType in validatorClassTypes)
            {
                // Determining if validatorClassType is already in this list
                bool validatorInList = false;
                foreach (var validator in this.validators)
                {
                    if (validator.GetType() == validatorClassType)
                    {
                        validatorInList = true;
                        break;
                    }
                }

                // If it's not in the list, then add a new instance of it
                if (validatorInList == false)
                {
                    var newValidator = Activator.CreateInstance(validatorClassType) as Validator;
                    newValidator.Initialize(this);
                    this.validators.Add(newValidator);
                    validatorsChanged = true;
                }
            }

            if (validatorsChanged)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        private void PrintErrors(List<ValidationError> errors)
        {
            foreach (var error in errors)
            {
                Debug.LogError($"{error.Name}\n{error.Description}", error.AffectedObject);
            }
        }

        [Serializable]
        public abstract class Validator
        {
            #pragma warning disable 0649, CA2235
            [ReadOnly]
            [SerializeField] private SceneValidator parent;
            [SerializeField] private bool isActive;
            [SerializeField] private List<GameObject> objectsToIgnore;
            #pragma warning restore 0649, CA2235

            public abstract string DisplayName { get; }

            public bool IsActive
            {
                get => this.isActive;
                set => this.isActive = value;
            }

            public void Initialize(SceneValidator parent)
            {
                this.parent = parent;
                this.isActive = true;
                this.objectsToIgnore = new List<GameObject>();
            }

            public abstract void Run(Scene scene, List<ValidationError> errorResults);

            protected IEnumerable<T> FindObjectsOfType<T>(bool includeInactive)
                where T : Component
            {
                foreach (var component in GameObject.FindObjectsOfType<T>(includeInactive))
                {
                    // making sure this belongs to the currently active scene and it shouldn't be ignored
                    bool isSameScene = component.gameObject.scene != this.parent.gameObject.scene;
                    bool isIgnoredObject = this.objectsToIgnore.Contains(component.gameObject);

                    if (isSameScene || isIgnoredObject)
                    {
                        continue;
                    }

                    yield return component;
                }
            }
        }

        #endif
    }
}

#endif
