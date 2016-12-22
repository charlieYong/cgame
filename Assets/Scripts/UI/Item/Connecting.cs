using UnityEngine;
using System.Collections;


namespace Sgame.UI
{
    public class Connecting : MonoBehaviour
    {

        void Start() {
            gameObject.SetActive(false);
        }

        public void Show(bool active) {
            gameObject.SetActive(active);
        }
    }
}