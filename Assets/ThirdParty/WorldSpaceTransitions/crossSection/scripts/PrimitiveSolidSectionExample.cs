using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PrimitiveSolidSectionExample : MonoBehaviour {
    public GameObject hatchedSphere, hatchedCylinder, hatchedCone, hatchedPrism, hatchedBox, hatchedTetra;

    [System.Serializable]
    public enum SolidType { Ellipsoid, Cylinder, Cone, Prism, Box, Tetra };
    [Space(10)]
    public SolidType solidType;
    [Space(10)]
    private SolidType currentSolidType;

    private GameObject primitivePrefab;
    [Range(1, 64)]
    public int maxPrimCount = 1;
    public Slider primitiveCountSlider;
    public Dropdown solidTypeDropdown;
    public Text maxPrimCountText;
    public Toggle ratioToggle;

    //public float yScalePredefined = 1;

    public bool lockScale;



    private float mcsr = 0.05f; //minimal radius of inscribed sphere;

    private Matrix4x4[] primMatrixes;
    private Vector4[] primScales;
    private int idx = -1;
    public List<GameObject> prims;

    void Start () {
        //Shader.DisableKeyword("CLIP_NONE");
        //Shader.DisableKeyword("CLIP_CUBOID");
        Renderer[] allrenderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allrenderers)
        {
            Material[] mats = r.sharedMaterials;
            foreach (Material m in mats) if (m.shader.name.Substring(0, 13) == "CrossSection/") m.DisableKeyword("CLIP_PLANE");
        }
        Shader.SetGlobalColor("_SectionColor", Color.black);
        primMatrixes = new Matrix4x4[64];
        primScales = new Vector4[64];
        prims = new List<GameObject>();
        SelectPrefab();
        prims.Add(primitivePrefab);
        GUI_Setup();
    }

    void GUI_Setup()
    {
        if (primitiveCountSlider)
        {
            primitiveCountSlider.value = maxPrimCount;
            primitiveCountSlider.onValueChanged.AddListener(delegate {
                PrimitiveMaxChanged((int)primitiveCountSlider.value);
            });
        }
        if (solidTypeDropdown)
        {
            solidTypeDropdown.options.Clear();
            for (int i = 0; i < SolidType.GetNames(typeof(SolidType)).Length; i++)//Populate new Options
            {
                solidTypeDropdown.options.Add(new Dropdown.OptionData(Enum.GetName(typeof(SolidType), i)));
            }
        }
        solidTypeDropdown.value = (int)solidType;
        solidTypeDropdown.captionText.text = Enum.GetName(typeof(SolidType), (int)solidType);
        solidTypeDropdown.onValueChanged.AddListener(delegate {
            SolidTypeChanged((SolidType)solidTypeDropdown.value);
        });

        if (maxPrimCountText) maxPrimCountText.text = maxPrimCount.ToString();

        if (ratioToggle)
        {
            ratioToggle.isOn = lockScale;
            ratioToggle.onValueChanged.AddListener(delegate {
                lockScale = ratioToggle.isOn;
            });
        }
}

    void SolidTypeChanged(SolidType _solidType)
    {
        if (solidType != _solidType)
        {
            solidType = _solidType;
            SelectPrefab();
        }
    }

    void PrimitiveMaxChanged(int n)
    {
        maxPrimCount = n;
        if (maxPrimCountText) maxPrimCountText.text = n.ToString();
        bool prefabNumberDecrease = false;
        if (prims != null)
        {
            prefabNumberDecrease = prims.Count > maxPrimCount;
            if (prims.Count == 1)
            {
                Matrix4x4 mx = Matrix4x4.TRS(prims[0].transform.position, prims[0].transform.rotation, Vector3.one);
                primMatrixes[0] = mx.inverse;
                primScales[0] = prims[0].transform.localScale;
            }
        }

        if (prefabNumberDecrease && maxPrimCount == 1)
        {
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", primMatrixes[0]);
            Shader.SetGlobalVector("_SectionScale", primScales[0]);
        }
        if (prefabNumberDecrease) SelectPrefab();
    }

    void SelectPrefab()
    {
        bool prefabNumberDecrease = false;
        if (prims != null) prefabNumberDecrease = prims.Count > maxPrimCount;
        if (prefabNumberDecrease)
        {
            for (int i = maxPrimCount; i < prims.Count; i++)
            {
                Destroy(prims[i]);
            }
            if (prims.Count > maxPrimCount) prims = prims.GetRange(0, maxPrimCount);
        }


        if (hatchedSphere)
        {
            hatchedSphere.SetActive(false);
            if (solidType == SolidType.Ellipsoid) primitivePrefab = hatchedSphere;
        }
        if (hatchedCylinder)
        {
            hatchedCylinder.SetActive(false);
            if (solidType == SolidType.Cylinder) primitivePrefab = hatchedCylinder;
        }
        if (hatchedCone)
        {
            hatchedCone.SetActive(false);
            if (solidType == SolidType.Cone) primitivePrefab = hatchedCone;
        }
        if (hatchedBox)
        {
            hatchedBox.SetActive(false);
            if (solidType == SolidType.Box) primitivePrefab = hatchedBox;
        }
        if (hatchedPrism)
        {
            hatchedPrism.SetActive(false);
            if (solidType == SolidType.Prism) primitivePrefab = hatchedPrism;
        }
        if (hatchedTetra)
        {
            hatchedTetra.SetActive(false);
            if (solidType == SolidType.Tetra) primitivePrefab = hatchedTetra;
        }
        if (!primitivePrefab)
        {
            Debug.LogWarning("no prefab primitive for this option");
            return;
        }
        if(idx!=-1) idx = idx % maxPrimCount;
        Shader.SetGlobalInt("_primCount", prims.Count);
        primitivePrefab.SetActive(true);
        if (prims == null) return;
        if (prims.Count == 0) return;
        for (int i = 0; i < prims.Count; i++)
        {
            if (prims[i] == hatchedSphere || prims[i] == hatchedCylinder || prims[i] == hatchedCone || prims[i] == hatchedPrism || prims[i] == hatchedBox || prims[i] == hatchedTetra)
            {
                //prims[i].SetActive(false);
                primitivePrefab.transform.position = prims[i].transform.position;
                primitivePrefab.transform.rotation = prims[i].transform.rotation;
                primitivePrefab.transform.localScale = prims[i].transform.localScale;
                prims[i] = primitivePrefab;
            }
            else
            {
                GameObject newPrim = Instantiate(primitivePrefab, prims[i].transform.position, prims[i].transform.rotation);
                newPrim.transform.localScale = prims[i].transform.localScale;
                GameObject oldPrim = prims[i];
                prims[i] = newPrim;
                Destroy(oldPrim);          
            }
            prims[i].SetActive(true);
        }
        
        currentSolidType = solidType;
        if (idx < 0) return;
        SetKeywordsForSingle(maxPrimCount == 1);
        SetKeywordsForMultiple(maxPrimCount > 1);
    }

    private void OnValidate()
    {
        bool prefabNumberDecrease = false;
        if (prims != null) prefabNumberDecrease = prims.Count > maxPrimCount;
        if (prefabNumberDecrease && maxPrimCount == 1)
        {
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", primMatrixes[0]);
            Shader.SetGlobalVector("_SectionScale", primScales[0]);
        }
        if (solidType != currentSolidType||prefabNumberDecrease) SelectPrefab();
    }


    void Update () {
        if (
#if ENABLE_INPUT_SYSTEM
            Mouse.current.leftButton.wasPressedThisFrame
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.GetMouseButtonDown(0) 
#endif
            )
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Ray ray = Camera.main.ScreenPointToRay(
#if ENABLE_INPUT_SYSTEM
            Mouse.current.position.ReadValue()
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                Input.mousePosition 
#endif
                );
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f))
            {
                if (hit.transform.IsChildOf(transform))
                {
                    
                    Vector3 upVector = Vector3.Cross(hit.transform.up, hit.normal).normalized;
                    upVector = Quaternion.AngleAxis(90, hit.normal) * upVector;
                    Quaternion rot = Quaternion.LookRotation(hit.normal, upVector);
                    Matrix4x4 mx = Matrix4x4.TRS(hit.point, rot, Vector3.one);

                    idx = (idx + 1) % maxPrimCount;

                    Debug.Log("hit "+ idx.ToString());

                    if (idx > prims.Count - 1)
                    {
                        GameObject newPrimitive = Instantiate(primitivePrefab);
                        prims.Add(newPrimitive);
                    }

                    if (maxPrimCount == 1)
                    {
                        SetKeywordsForSingle(true);
                        Shader.SetGlobalMatrix("_WorldToObjectMatrix", mx.inverse);
                        Shader.SetGlobalVector("_SectionScale", mcsr * Vector3.one);
                    }

                    if (maxPrimCount > 1)
                    {
                        SetKeywordsForSingle(false);
                        SetKeywordsForMultiple(true);
                        Shader.SetGlobalInt("_primCount", prims.Count);
                        primMatrixes[idx] = mx.inverse;
                        Shader.SetGlobalMatrixArray("_WorldToObjectMatrixes", primMatrixes);
                        primScales[idx] = mcsr * Vector3.one;
                        Shader.SetGlobalVectorArray("_SectionScales", primScales);
                    }

                    if (prims[idx])
                    {
                        prims[idx].transform.position = hit.point;
                        prims[idx].transform.rotation = rot;
                        StartCoroutine(drag(prims[idx].transform));
                    }
                    else
                    {
                        Debug.Log("np "+ idx.ToString());
                    }
                }
            }
        }
	}

    void ChangeSolidType()
    {

    }

    void OnEnable()
    {
        Shader.DisableKeyword("CLIP_NONE");
    }

    void OnDisable()
    {
        SetKeywordsForSingle(false);
        SetKeywordsForMultiple(false);
        Shader.EnableKeyword("CLIP_NONE");
    }

    void SetKeywordsForSingle(bool val)
    {

        if (solidType == SolidType.Ellipsoid && val) { Shader.EnableKeyword("CLIP_ELLIPSOID"); } else { Shader.DisableKeyword("CLIP_ELLIPSOID"); }
        if (solidType == SolidType.Cylinder && val) { Shader.EnableKeyword("CLIP_CYLINDER"); } else { Shader.DisableKeyword("CLIP_CYLINDER"); }
        if (solidType == SolidType.Cone && val) { Shader.EnableKeyword("CLIP_CONE"); } else { Shader.DisableKeyword("CLIP_CONE"); }
        if (solidType == SolidType.Box && val) { Shader.EnableKeyword("CLIP_CUBOID"); } else { Shader.DisableKeyword("CLIP_CUBOID"); }
        if (solidType == SolidType.Prism && val) { Shader.EnableKeyword("CLIP_PRISM"); } else { Shader.DisableKeyword("CLIP_PRISM"); }
        if (solidType == SolidType.Tetra && val) { Shader.EnableKeyword("CLIP_TETRA"); } else { Shader.DisableKeyword("CLIP_TETRA"); }
    }

    void SetKeywordsForMultiple(bool val)
    {
        if (solidType == SolidType.Ellipsoid && val) { Shader.EnableKeyword("CLIP_ELLIPSOIDS"); } else { Shader.DisableKeyword("CLIP_ELLIPSOIDS"); }
        if (solidType == SolidType.Cylinder && val) { Shader.EnableKeyword("CLIP_CYLINDERS"); } else { Shader.DisableKeyword("CLIP_CYLINDERS"); }
        if (solidType == SolidType.Cone && val) { Shader.EnableKeyword("CLIP_CONES"); } else { Shader.DisableKeyword("CLIP_CONES"); }
        if (solidType == SolidType.Box && val) { Shader.EnableKeyword("CLIP_CUBOIDS"); } else { Shader.DisableKeyword("CLIP_CUBOIDS"); }
        if (solidType == SolidType.Prism && val) { Shader.EnableKeyword("CLIP_PRISMS"); } else { Shader.DisableKeyword("CLIP_PRISMS"); }
        if (solidType == SolidType.Tetra && val) { Shader.EnableKeyword("CLIP_TETRAS"); } else { Shader.DisableKeyword("CLIP_TETRAS"); }
    }

    void OnApplicationQuit()
    {
        SetKeywordsForSingle(false);
        SetKeywordsForMultiple(false);
        Shader.EnableKeyword("CLIP_NONE");
    }

    IEnumerator drag(Transform hatchCube)
    {
        float cameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 startPoint = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
            Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.mousePosition.x, Input.mousePosition.y
#endif
            , cameraDistance));
        hatchCube.localScale = 0.1f * Vector3.one;
        hatchCube.gameObject.SetActive(true);
        Vector3 translation = Vector3.zero;
        Camera.main.GetComponent<MaxCamera>().enabled = false;
        while (
#if ENABLE_INPUT_SYSTEM
            Mouse.current.leftButton.isPressed
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.GetMouseButton(0)
#endif
            )
        {
            translation = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
                Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
                Input.mousePosition.x, Input.mousePosition.y
#endif
                , cameraDistance)) - startPoint;

            float m = translation.magnitude;
            if (m > mcsr)
            {
                Vector3 translationY = Vector3.Project(translation, hatchCube.up);
                Vector3 translationX = Vector3.Project(translation, hatchCube.right);
                Vector3 scaleVector = Vector3.one;
                if (lockScale)
                {
                    scaleVector = m * Vector3.one;
                }
                else
                {
                    scaleVector = new Vector3(translationX.magnitude, 2 * translationY.magnitude, m);
                }
                if(maxPrimCount == 1) Shader.SetGlobalVector("_SectionScale", scaleVector);
                if (maxPrimCount > 1)
                {
                    primScales[idx] = scaleVector;
                    Shader.SetGlobalVectorArray("_SectionScales", primScales);
                }
                hatchCube.localScale = scaleVector;
            }
            yield return null;
        }

        Camera.main.GetComponent<MaxCamera>().enabled = true;
        //hatchCube.gameObject.SetActive(false);
    }
}
