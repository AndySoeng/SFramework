//The purpose of this script is to setup and update the global shader properties for the capped sections 

using UnityEngine;

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class CappedSectionFollow : MonoBehaviour
    {

        private enum Mode { box, corner };
        private Mode sectionMode;

        private Vector3 tempPos;
        private Vector3 tempScale;
        private Quaternion tempRot;

        public bool followPosition = true;
        //public bool followRotation = true;

        void Awake()
        {
            if (gameObject.GetComponent<CappedSectionBox>()) sectionMode = Mode.box;
            if (gameObject.GetComponent<CappedSectionCorner>()) sectionMode = Mode.corner;
        }
        void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            Shader.SetGlobalColor("_SectionColor", Color.black);
            Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
            SetSection();
        }

        void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (tempPos != transform.position || tempRot != transform.rotation || tempScale != transform.localScale)
            {

                tempPos = transform.position;
                tempRot = transform.rotation;
                tempScale = transform.localScale;
                SetSection();
            }
            Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
        }


        void OnDisable()
        {

            Shader.DisableKeyword("CLIP_BOX");
            //Shader.SetGlobalInt("_CLIP_BOX", 0);
            Shader.DisableKeyword("CLIP_CORNER");
            //Shader.SetGlobalInt("_CLIP_CORNER", 0);
            Shader.EnableKeyword("CLIP_NONE");
            Shader.EnableKeyword("__");
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Shader.EnableKeyword("CLIP_NONE");
                Shader.EnableKeyword("__");
                return;
            }
#endif

            if (sectionMode == Mode.box)
            {
                Shader.DisableKeyword("CLIP_NONE");
                Shader.EnableKeyword("CLIP_BOX");
                //Shader.SetGlobalInt("_CLIP_BOX", 1);
            }
            if (sectionMode == Mode.corner)
            {
                Shader.DisableKeyword("CLIP_NONE");
                Shader.EnableKeyword("CLIP_CORNER");
                //Shader.SetGlobalInt("_CLIP_CORNER", 1);
            }
            SetSection();
        }


        void OnApplicationQuit()
        {
            Shader.DisableKeyword("CLIP_BOX");
            //Shader.SetGlobalInt("_CLIP_BOX", 0);
            Shader.DisableKeyword("CLIP_CORNER");
            //Shader.SetGlobalInt("_CLIP_CORNER", 0);
            Shader.EnableKeyword("CLIP_NONE");
        }

        void SetSection()
        {
            if (followPosition)
            {
                Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Shader.SetGlobalMatrix("_WorldToObjectMatrix", m.inverse);
                Shader.SetGlobalVector("_SectionScale", transform.localScale);
            }
        }

    }
}