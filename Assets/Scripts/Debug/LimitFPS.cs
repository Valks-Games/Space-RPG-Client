using UnityEngine;

namespace SpaceGame.Debugging
{
    public class LimitFPS : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}