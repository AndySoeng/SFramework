using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Ex
{
    public static class ExCinemachine 
    {
        static ExCinemachine()
        {
            SceneManager.sceneLoaded += ((arg0, mode) =>
            {

                _cvIndex = 11;

            });
        }
        
        private static int _cvIndex = 11;

        private static int cvIndex
        {
            get => ++_cvIndex;
        }

        public static void SetActiveCamera(this CinemachineVirtualCameraBase camera)
        {
            camera.Priority = cvIndex;
        }
        
        
            public static void SetCMFreelookOnlyWhenRightMouseDown()
                {
                    CinemachineCore.GetInputAxis = GetAxisCustom;
                }
                
                private static float GetAxisCustom(string axisName){
                    if(axisName == "Mouse X"){
                        if (Input.GetMouseButton(1)){
                            return UnityEngine.Input.GetAxis("Mouse X");
                        } else{
                            return 0;
                        }
                    }
                    else if (axisName == "Mouse Y"){
                        if (Input.GetMouseButton(1)){
                            return UnityEngine.Input.GetAxis("Mouse Y");
                        } else{
                            return 0;
                        }
                    }
                    return 0;
                }
    }
}
