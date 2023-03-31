/*
 The purpose of this script is to create cross-section material instances
 and - in case of capped sections - to scale the capped section prefabs to fit the model GameObject.

 The script uses threading for axis aligned bound box calculation
 */
#define USE_JOB_THREADS
//#define PHUONG_THREADS
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;
#if  USE_JOB_THREADS
using Unity.Jobs;
#endif
using UnityEngine;
using System.IO;
using System.Linq;
//using MathGeoLib;
#if PHUONG_THREADS
using Threading;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;

namespace WorldSpaceTransitions
{
    [System.Serializable]
    public enum BoundsOrientation { objectOriented, worldOriented };


    [ExecuteInEditMode]

    public class SectionSetup : MonoBehaviour
    {
        //[Tooltip("Reassign after geometry change")]
        [HideInInspector]
        public GameObject model;
        [SerializeField]
        [HideInInspector]
        private GameObject currentModel;
        [SerializeField]
        [HideInInspector]
        private Bounds bounds;
        [HideInInspector]
        public BoundsOrientation boundsMode = BoundsOrientation.worldOriented;
        [HideInInspector]
        public bool accurateBounds = true;
        //[HideInInspector]
        //public bool useMathGeo = true;
        [SerializeField]
        [HideInInspector]
        private bool previousAccurate;
        [SerializeField]
        [HideInInspector]
        private BoundsOrientation boundsModePrevious;
        [SerializeField]
        [HideInInspector]
        public string newMatsPath = "WorldSpaceTransitions/crossSection/NewMaterials", newURPShadersPath = "WorldSpaceTransitions/crossSection (URP)/NewMaterials", newHDRPShadersPath = "WorldSpaceTransitions/crossSection (HDRP)/NewMaterials";

        static string renderPipelineAssetName = "";

#if USE_JOB_THREADS
        NativeArray<Bounds> orientedBoundsResult;
        NativeArray<Matrix4x4> mmatrices;
        NativeArray<Vector3> mvertices;
        NativeArray<int> mcounts;
        //Job Handles
        OrientedBounds.BoundsJob boundsJob;
        JobHandle boundsJobHandle;
#endif
#if PHUONG_THREADS
        private bool mainThreadUpdated = true;
#endif
        private Dictionary<Material, Material> materialsToreplace;
        [HideInInspector]
        public List<ShaderSubstitute> shaderSubstitutes;
        private bool recalculate = false;

        [System.Serializable]
        public struct ShaderSubstitute
        {
#if UNITY_EDITOR
            [ReadOnly]
#endif
            public Shader original;
            public Shader substitute;


            public ShaderSubstitute(Shader orig, Shader subst)
            {
                original = orig; substitute = subst;
            }
        }

#if UNITY_EDITOR
        /*
        private readonly Queue<Action> _actionQueue = new Queue<Action>();
        public Queue<Action> ActionQueue
        {
            get
            {
                lock (Async.GetLock("ActionQueue"))
                {
                    return _actionQueue;
                }
            }
        }
        */

        void OnValidate()
        {
            //Debug.Log("onvalidate");
            if (Application.isPlaying) return;
            Setup();
        }
#endif
        void Setup()
        {
            //Debug.Log(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name);
            //Debug.Log(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.defaultShader.name);
            try
            {
                renderPipelineAssetName = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name;
            }
            catch { }

#if PHUONG_THREADS
            if (!mainThreadUpdated) return;
#endif
            ISizedSection csc = GetComponent<ISizedSection>();
            if (csc == null) return;
            if (model)
            {
                transform.rotation = (boundsMode == BoundsOrientation.objectOriented) ? transform.rotation = model.transform.rotation : Quaternion.identity;
                //Debug.Log((model != currentModel).ToString() + " | " + (accurateBounds != previousAccurate).ToString() + " | " + (boundsMode != boundsModePrevious).ToString());
                if (model != currentModel || accurateBounds != previousAccurate || boundsMode != boundsModePrevious || recalculate)
                {
                    bounds = GetBounds(model, boundsMode);

                    csc.Size(bounds, model, boundsMode);

                    if (accurateBounds) AccurateBounds(model, boundsMode);
                    if (!accurateBounds)
                    {
                        currentModel = model;
                        previousAccurate = accurateBounds;
                        boundsModePrevious = boundsMode;
                    }
                }
            }
            else
            {
                currentModel = null;
            }

            Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, UnityEngine.Vector3.one);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
            //hide the box when no model assigned
            foreach (Transform tr in transform)
            {
                //tr.gameObject.SetActive(model);
                try { tr.GetComponent<Renderer>().enabled = model; }
                catch { }
                try { tr.GetComponent<Collider>().enabled = model; }
                catch { }
            }
        }

#if UNITY_EDITOR
        public string CheckShaders()
        {
            //if (GraphicsSettings.renderPipelineAsset.name == "LightweightRenderPipelineAsset") return;
            List<Shader> shaderList = new List<Shader>();
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                foreach (Material m in mats)
                {
                    Shader sh = m.shader;
                    if (!shaderList.Contains(sh)) shaderList.Add(sh);
                }
            }

            string shaderKeywordNeeded = "CLIP_PLANE";
            if (GetComponent<CappedSectionCorner>()) shaderKeywordNeeded = "CLIP_CORNER";
            if (GetComponent<CappedSectionBox>()) shaderKeywordNeeded = "CLIP_BOX";

            shaderSubstitutes.Clear();

            string shaderInfo = "";

            foreach (Shader sh in shaderList)
            {
                bool isKeywordSupported = false;
                Shader substitute = getSubstitute(sh, renderPipelineAssetName, shaderKeywordNeeded, out isKeywordSupported);
                if (substitute != null)
                {
                    shaderSubstitutes.Add(new ShaderSubstitute(sh, substitute));
                    if (!isKeywordSupported) shaderInfo += "Add " + shaderKeywordNeeded + " keyword to " + substitute.name + " shader \n";
                }
                else
                {
                    if (!isKeywordSupported) shaderInfo += "Add " + shaderKeywordNeeded + " keyword to " + sh.name + " shader \n";
                }
                //keywordSupport = keywordSupport && isKeywordSupported;
            }
            if (shaderInfo == "")
                shaderInfo = "check o.k.; all shaders support " + shaderKeywordNeeded + " keyword";
            if (shaderSubstitutes.Count > 0) shaderInfo = "Create and assign section materials using the below shader substitutes. You can change the suggested substitutes to other crossSection shaders";
            return shaderInfo;
        }
#endif

        public void CreateSectionMaterials()
        {
            Dictionary<Shader, Shader> shadersToreplace = new Dictionary<Shader, Shader>();
            foreach (ShaderSubstitute ssub in shaderSubstitutes)
            {
                shadersToreplace.Add(ssub.original, ssub.substitute);
            }
            materialsToreplace = new Dictionary<Material, Material>();
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(model, "crossSection material assign");
#endif
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    Debug.Log(mats[i].name);
                    Shader sh = mats[i].shader;
                    if (shadersToreplace.ContainsKey(sh))
                    {
                        if (!materialsToreplace.ContainsKey(mats[i]))
                        {
                            Material newMaterial;
#if UNITY_EDITOR
                            string fpath = AssetDatabase.GetAssetPath(mats[i]);
                            string newName = mats[i].name + "_cs";
                            //string dirname = Path.GetDirectoryName(fpath);
                            //if (mats[i].name == "Default-Material") dirname = "Assets";
                            string dirname = Path.Combine(Application.dataPath, NewMatsPath);
                            DirectoryInfo di = Directory.CreateDirectory(dirname);
                            Debug.Log(fpath);
                            string newpath = Path.Combine("Assets", NewMatsPath, newName + ".mat");

                            newMaterial = (Material)AssetDatabase.LoadAssetAtPath(newpath, typeof(Material));
                            if (newMaterial == null)
                            {
#endif
                                newMaterial = new Material(mats[i]);
                                newMaterial.SetFloat("_Cull", 0);
#if UNITY_EDITOR
                                newMaterial.name = newName;
                                AssetDatabase.CreateAsset(newMaterial, newpath);
                            }
#endif
                            newMaterial.shader = shadersToreplace[mats[i].shader];
                            materialsToreplace.Add(mats[i], newMaterial);
                        }
                        newMats[i] = materialsToreplace[mats[i]];
                    }
                    else
                    {
                        newMats[i] = mats[i];
                    }
                }
                r.materials = newMats;
            }
        }


        public void SetModel(GameObject _model)
        {
            //if (Application.isPlaying) accurateBounds = false;
            model = _model;
            Setup();
        }
        public static Bounds GetBounds(GameObject go)
        {
            return GetBounds(go, BoundsOrientation.worldOriented);
        }

        public static Bounds GetBounds(GameObject go, BoundsOrientation boundsMode)
        {
            Quaternion quat = go.transform.rotation;//object axis AABB

            Bounds bounds = new Bounds();

            if (boundsMode == BoundsOrientation.objectOriented) go.transform.rotation = Quaternion.identity;

            //Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            MeshRenderer[] mrenderers = go.GetComponentsInChildren<MeshRenderer>();
            //Debug.Log(renderers.Length.ToString() + " | " + mrenderers.Length.ToString());
            if (mrenderers.Length > 0)
            {
                for (int i = 0; i < mrenderers.Length; i++)
                {
                    if (i == 0)
                    {
                        bounds = mrenderers[i].bounds;
                    }
                    else
                    {
                        bounds.Encapsulate(mrenderers[i].bounds);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("CrossSection message", "The object contains no meshRenderers!\n- please reassign", "Continue");
#endif
            }

            UnityEngine.Vector3 localCentre = go.transform.InverseTransformPoint(bounds.center);
            go.transform.rotation = quat;
            bounds.center = go.transform.TransformPoint(localCentre);

            return bounds;
        }

        void AccurateBounds(GameObject go, BoundsOrientation boundsMode)
        {

            MeshFilter[] meshes = go.GetComponentsInChildren<MeshFilter>();
            ISizedSection csc = GetComponent<ISizedSection>();
#if UNITY_EDITOR
            if (meshes.Length == 0)
            {
                EditorUtility.DisplayDialog("CrossSection message", "The object contains no meshes!\n- please reassign", "Continue");
            }
#endif
            VertexData[] vertexData = new VertexData[meshes.Length];
#if USE_JOB_THREADS
            List<Vector3> vertList = new List<Vector3>();
            Matrix4x4[] matrs = new Matrix4x4[meshes.Length];
            int[] vcounts = new int[meshes.Length];
#endif
            //
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh ms = meshes[i].sharedMesh;
                vertexData[i] = new VertexData(ms.vertices, meshes[i].transform.localToWorldMatrix);
#if USE_JOB_THREADS
                vertList.AddRange(ms.vertices);
                vcounts[i] = ms.vertices.Length;
                matrs[i] = meshes[i].transform.localToWorldMatrix;
#endif
            }
            Vector3 v1 = (boundsMode == BoundsOrientation.objectOriented) ? go.transform.right : Vector3.right;
            Vector3 v2 = (boundsMode == BoundsOrientation.objectOriented) ? go.transform.up : Vector3.up;
            Vector3 v3 = (boundsMode == BoundsOrientation.objectOriented) ? go.transform.forward : Vector3.forward;
#if USE_JOB_THREADS
            mvertices = new NativeArray<Vector3>(vertList.ToArray(), Allocator.Persistent);
            mmatrices = new NativeArray<Matrix4x4>(matrs, Allocator.Persistent);
            mcounts = new NativeArray<int>(vcounts, Allocator.Persistent);

            Bounds[] b = new Bounds[1];
            orientedBoundsResult = new NativeArray<Bounds>(b, Allocator.Persistent);

            //Creating a job and assigning the variables within the Job
            boundsJob = new OrientedBounds.BoundsJob()
            {
                result = orientedBoundsResult,
                vertices = mvertices,
                vcounts = mcounts,
                matrices = mmatrices,
                _v1 = v1,
                _v2 = v2,
                _v3 = v3
            };

            //Setup of the job handle
            boundsJobHandle = boundsJob.Schedule();
            //if (Application.isPlaying)
            //{
                StartCoroutine(WaitForBoundBoxThread());
            //}

#endif

#if PHUONG_THREADS
            Async.Run(() =>
            {
                mainThreadUpdated = false;
                Debug.Log("thread start");
                bounds = OrientedBounds.OBB(vertexData, v1, v2, v3);
            }).ContinueInMainThread(() =>
            {
                Debug.Log("back to main thread");
                if (csc != null) csc.Size(bounds, go, boundsMode);
                currentModel = model;
                previousAccurate = accurateBounds;
                boundsModePrevious = boundsMode;
                enabled = false;
                enabled = true;
                recalculate = false;
                //mainThreadUpdated = true;
            });
#endif
#if !PHUONG_THREADS && !USE_JOB_THREADS
            bounds = OrientedBounds.OBB(vertexData, v1, v2, v3);
            Vector3 localCentre = go.transform.InverseTransformPoint(bounds.center);
            bounds.center = go.transform.TransformPoint(localCentre);
            if (csc != null) csc.Size(bounds, go, boundsMode);
#endif
        }

#if UNITY_EDITOR && USE_JOB_THREADS
        private void Update()
        {
            if (Application.isPlaying) return;
            if (!recalculate) return;
            
            boundsJobHandle.Complete();
            Debug.Log("back to main thread ");
            UpdateThreadResult ();
        }
#endif
#if USE_JOB_THREADS
        void UpdateThreadResult()
        {
            bounds = boundsJob.result[0];
            ISizedSection csc = GetComponent<ISizedSection>();
            if (csc != null) csc.Size(bounds, model, boundsMode);
            currentModel = model;
            previousAccurate = accurateBounds;
            boundsModePrevious = boundsMode;
            if (mvertices.IsCreated) mvertices.Dispose();
            if (mmatrices.IsCreated) mmatrices.Dispose();
            if (mcounts.IsCreated) mcounts.Dispose();
            //enabled = false;
            //enabled = true;
            recalculate = false;
        }

        public IEnumerator WaitForBoundBoxThread()
        {
            //while (!boundsJobHandle.IsCompleted)
            //yield return null;
            //boundsJobHandle.Complete(); 
            if (boundsJobHandle.IsCompleted == false)
                yield return new WaitForJobCompleted(boundsJobHandle, isUsingTempJobAllocator: true);
            if (boundsJobHandle.IsCompleted) UpdateThreadResult();
        }
#endif

#if UNITY_EDITOR
        Shader getSubstitute(Shader shader, string pipelineAssetName, string keyword, out bool hasKeyword)
        {
            //Let's define crossSection shader is that containing any of these keywords
            List<string> cs_keywords = new List<string>() { "CLIP_PLANE", "CLIP_BOX", "CLIP_CORNER" };
            List<string> keywordList = shader.keywordSpace.keywordNames.ToList();
            hasKeyword = false;
            if (keywordList.Contains(keyword)) hasKeyword = true;
            if (keywordList.Intersect(cs_keywords).Any()) return null;

            string substituteName = "";
            string defaultshaderName;

            switch (pipelineAssetName)
            {
                case "UniversalRenderPipelineAsset":
                    if (shader.name.Contains("Graphs"))
                    {
                        substituteName = shader.name.Replace("Shader Graphs/", "CrossSectionGraphs/");
                    }
                    else
                    {
                        substituteName = shader.name.Replace("Universal Render Pipeline/", "CrossSectionURP/");
                    }
                    defaultshaderName = "CrossSectionURP/Lit";
                    break;
                case "HDRenderPipelineAsset":
                    if (shader.name.Contains("Graphs"))
                    {
                        substituteName = shader.name.Replace("Shader Graphs/", "CrossSectionGraphs/");
                    }
                    else
                    {
                        substituteName = shader.name.Replace("HDRP/", "CrossSectionHDRP/");
                    }
                    defaultshaderName = "CrossSectionHDRP/Lit";
                    break;
                default:
                    string sname = shader.name.Replace("Legacy Shaders/", "");
                    if (sname.Contains("Transparent/VertexLit"))
                    {
                        sname = sname.Replace("Transparent/VertexLit", "Transparent/Specular");
                    }
                    else if (sname.Contains("Transparent"))
                    {
                        sname = "Transparent/Diffuse";
                    }
                    else
                    {
                        sname = "Standard";
                    }
                    substituteName = "CrossSection/" + sname;
                    defaultshaderName = "CrossSection/Standard";
                    if (keyword == "CLIP_BOX")
                    {
                        substituteName = "CrossSection/Box/" + sname;
                        defaultshaderName = "CrossSection/Box/Standard";
                    }

                    break;
            }

            /*
            //methods to get list of global and local shader keywords are internal and private, only way to call them is via reflection
            var getKeywordsMethod = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
            string[] keywords;

            //check the object shaders for keywords
            keywords = (string[])getKeywordsMethod.Invoke(null, new object[] { shader });
            keywordList = new List<string>(keywords);
            Debug.Log(keywordList[0]);
            hasKeyword = keywordList.Contains(keyword);
            //
            */

            Shader newShader;
            if (Shader.Find(substituteName))
            {
                newShader = Shader.Find(substituteName);
            }
            else
            {
                newShader = Shader.Find(defaultshaderName);
            }
            if (!newShader) Debug.Log(pipelineAssetName + " | " + substituteName + " | " + defaultshaderName + " | alert");
            keywordList = newShader.keywordSpace.keywordNames.ToList();
            if (keywordList.Contains(keyword)) hasKeyword = true;
            return newShader;
        }
#endif
        private void OnEnable()
        {
            //Mulithreading used in bound box calculations
#if PHUONG_THREADS
            mainThreadUpdated = true;
#endif
#if USE_JOB_THREADS
            Bounds[] b = new Bounds[1];
            orientedBoundsResult = new NativeArray<Bounds>(b, Allocator.Persistent);
#endif
            //boundsModePrevious = boundsMode;
        }
        void OnDrawGizmos()
        {
            // Your gizmo drawing thing goes here if required...
            // Update the main thread after the bound box calculations
#if UNITY_EDITOR && PHUONG_THREADS
            // Ensure continuous Update calls.
            if (!Application.isPlaying && !mainThreadUpdated)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        public void RecalculateBounds()
        {
            recalculate = true;
            Setup();
        }


        private void OnDisable()
        {
#if USE_JOB_THREADS
            if (Application.isPlaying) return;
            // make sure to Dispose any NativeArrays when you're done
            if (orientedBoundsResult.IsCreated) orientedBoundsResult.Dispose();
            if (mvertices.IsCreated) mvertices.Dispose();
            if (mmatrices.IsCreated) mmatrices.Dispose();
            if (mcounts.IsCreated) mcounts.Dispose();
#endif
        }

        private void OnDestroy()
        {
#if USE_JOB_THREADS
            // make sure to Dispose any NativeArrays when you're done
            if (orientedBoundsResult.IsCreated) orientedBoundsResult.Dispose();
            if (mvertices.IsCreated) mvertices.Dispose();
            if (mmatrices.IsCreated) mmatrices.Dispose();
            if (mcounts.IsCreated) mcounts.Dispose();
#endif
        }
        public string NewMatsPath
        {
            get
            {
                if (renderPipelineAssetName == "UniversalRenderPipelineAsset")
                { return newURPShadersPath; }
                else if (renderPipelineAssetName == "HDRenderPipelineAsset")
                { return newHDRPShadersPath; }
                else { return newMatsPath; }
            }
            set
            {
                if (renderPipelineAssetName == "UniversalRenderPipelineAsset")
                { newURPShadersPath = value; }
                else if (renderPipelineAssetName == "HDRenderPipelineAsset")
                { newHDRPShadersPath = value; }
                else { newMatsPath = value; }
            }
        }

    }
}
