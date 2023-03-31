using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class CrossSectionObjectSetup : MonoBehaviour
    {
        public Color sectionColor = Color.red;
        public GameObject model;
        private List<Material> matList;
        private List<Material> clipMatList;
        private Renderer[] renderers;

        private Dictionary<Renderer, int[]> matDict;


        [HideInInspector]
        public Bounds bounds;


        void Awake()
        {
            //makeSectionMaterials();
        }
#if UNITY_EDITOR
        void OnValidate()
        {
            if(model) bounds = CalculateMeshBounds(gameObject);
        }
#endif

        void makeSectionMaterials()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            matList = new List<Material>();
            clipMatList = new List<Material>();
            matDict = new Dictionary<Renderer, int[]>();
            foreach (Renderer rend in renderers)
            {
                Material[] mats = rend.sharedMaterials;
                int[] idx = new int[mats.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    int i = matList.IndexOf(mats[j]);
                    if (i == -1)
                    {
                        matList.Add(mats[j]);
                        i = matList.Count - 1;
                    }
                    idx[j] = i;
                }
                matDict.Add(rend, idx);
            }
            foreach (Material mat in matList)
            {
                string shaderName = mat.shader.name;
                Debug.Log(shaderName);
                if (shaderName.Length > 13)
                {
                    if (shaderName.Substring(0, 13) == "CrossSection/")
                    {
                        clipMatList.Add(mat);
                        continue;
                    }
                }
                Material substitute = new Material(mat);
                //substitute.name = "subst_" + substitute.name;
                shaderName = shaderName.Replace("Legacy Shaders/", "").Replace("(", "").Replace(")", "");
                Shader replacementShader = null;

                if (replacementShader == null) replacementShader = Shader.Find("CrossSection/" + shaderName);
                if (replacementShader == null)
                {
                    if (shaderName.Contains("Transparent/VertexLit"))
                    {
                        replacementShader = Shader.Find("CrossSection/Transparent/Specular");
                    }
                    else if (shaderName.Contains("Transparent"))
                    {
                        replacementShader = Shader.Find("CrossSection/Transparent/Diffuse");
                    }
                    else
                    {
                        replacementShader = Shader.Find("CrossSection/Diffuse");
                    }
                }
                substitute.shader = replacementShader;
                substitute.SetColor("_SectionColor", sectionColor);

                clipMatList.Add(substitute);
            }
            foreach (Renderer rend in renderers)
            {
                int[] idx = matDict[rend];
                Material[] mats = new Material[idx.Length];
                for (int i = 0; i < idx.Length; i++)
                {
                    mats[i] = clipMatList[idx[i]];
                }
                rend.materials = mats;
            }
        }

        public static Bounds CalculateMeshBounds(GameObject g)
        {
            Bounds accurateBounds = new Bounds();
            MeshFilter[] meshes = g.GetComponentsInChildren<MeshFilter>();
            Renderer[] renderers = g.GetComponentsInChildren<Renderer>();
            Debug.Log("cmb " + meshes.Length.ToString() + " | rends " + renderers.Length.ToString());
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh ms = meshes[i].sharedMesh;
                int vc = ms.vertexCount;
                for (int j = 0; j < vc; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        accurateBounds = new Bounds(meshes[i].transform.TransformPoint(ms.vertices[j]), Vector3.zero);
                    }
                    else
                    {
                        accurateBounds.Encapsulate(meshes[i].transform.TransformPoint(ms.vertices[j]));
                    }
                }
            }
            return accurateBounds;
        }

        void OnApplicationQuit()
        {
            Shader.DisableKeyword("CLIP_PLANE");
        }

        public Bounds GetBounds()
        {
            return bounds;
        }

    }
}