using Cinemachine;
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

        public static void SetActiveCamera(this CinemachineVirtualCamera camera)
        {
            camera.Priority = cvIndex;
        }
    }
}
