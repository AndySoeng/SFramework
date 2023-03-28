using UnityEngine;
using HighlightPlus;

namespace HighlightPlus.Demos {

    public class ManualSelectionDemo : MonoBehaviour {

        HighlightManager hm;

        public Transform objectToSelect;

        void Start() {
            hm = FindObjectOfType<HighlightManager>();
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                hm.SelectObject(objectToSelect);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                hm.ToggleObject(objectToSelect);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                hm.UnselectObject(objectToSelect);
            }
        }
    }
}
