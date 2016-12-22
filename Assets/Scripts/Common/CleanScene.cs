using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Sgame
{
    public class CleanScene : MonoBehaviour
    {

        Transform _transform;

        void Start() {
            _transform = transform;
            StartCoroutine(StartCleanScene());
        }

        IEnumerator StartCleanScene() {
            DestroyAllGameObjects();
            yield return new WaitForSeconds(0.5f);
            ToStartScene();
        }

        void DestroyAllGameObjects() {
            TriggerEvent<object>.ClearAllEvent();
            Transform[] list = GameObject.FindObjectsOfType<Transform>();
            List<GameObject> rootGoList = new List<GameObject>();
            for (int i = 0, imax = list.Length; i < imax; i++) {
                if (null == list[i].parent && list[i] != _transform) {
                    rootGoList.Add(list[i].gameObject);
                }
            }
            for (int i = 0; i < rootGoList.Count; i++) {
                Destroy(rootGoList[i]);
                rootGoList[i] = null;
            }
            AppManager.ClearOnExitApp();
        }

        void ToStartScene() {
            SceneMgr.Instance.GoToStartScene();
        }
    }
}