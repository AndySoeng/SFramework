using UnityEngine;
using HighlightPlus;

namespace HighlightPlus.Demos {
	
	public class SphereHighlightEventExample : MonoBehaviour {

		HighlightEffect effect;

		void Start() {
			effect = GetComponent<HighlightEffect> ();
			effect.OnObjectHighlightStart += ValidateHighlightObject;
		}


		bool ValidateHighlightObject(GameObject obj) {
			// You can return false to cancel highlight on this object
			return true;
		}

		void HighlightStart () {
			Debug.Log ("Gold sphere highlighted!");
		}

		void HighlightEnd () {
			Debug.Log ("Gold sphere not highlighted!");
		}

		void Update() {
			if (InputProxy.GetKeyDown ("space")) {
				effect.HitFX (Color.white, 0.2f);
			}
			if (InputProxy.GetKeyDown("c")) {
				effect.SetGlowColor(new Color(Random.value, Random.value, Random.value));
            }

		}
	}

}