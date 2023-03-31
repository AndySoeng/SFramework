using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class SceneSwitch : MonoBehaviour {
    private static SceneSwitch switchInstance;
    public Dropdown sceneDropdown;

    public void SwitchScene(int val)
    {
        if (val == SceneManager.GetActiveScene().buildIndex) return; //toggle buttons change twice
        SceneManager.LoadSceneAsync(val);
    }
	void Awake () {
        DontDestroyOnLoad(this);
        if(transform.parent!=null) DontDestroyOnLoad(transform.parent);
        if (switchInstance == null)
        {
            switchInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
	}
    void Update()
    {
        if (
#if ENABLE_INPUT_SYSTEM
            Keyboard.current.escapeKey.isPressed
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            Input.GetKey("escape") 
#endif
            )
        {
            //Debug.Log("escape");
            Application.Quit();
        }
    }
}
