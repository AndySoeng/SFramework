using UnityEngine;

//Global shader properties can be overriden on the material level
namespace WorldSpaceTransitions.Examples
{
    public class AlignSectionPointToCenter : MonoBehaviour
    {
        Material mat;//(if you wanted dynamic)
        void Start()
        {
            //create and apply new material instance
            mat = GetComponent<Renderer>().material;
            mat.SetVector("_SectionPoint", transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            if (mat) mat.SetVector("_SectionPoint", transform.position);//(if you wanted dynamic)
        }
    }
}
