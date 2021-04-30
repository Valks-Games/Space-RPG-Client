using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceGame.Debugging
{
    public class ReloadScene : MonoBehaviour
    {
        public bool enableReload;

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && enableReload)
                SceneManager.LoadScene("Main");
        }
    }
}