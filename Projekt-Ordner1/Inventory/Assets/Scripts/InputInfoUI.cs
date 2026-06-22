using UnityEngine;

public class InputInfoUI : MonoBehaviour {

    [SerializeField] private GameObject inputInfo;

    public void Show() {
        inputInfo.SetActive(true);
    }

    public void Hide() {
        inputInfo.SetActive(false);
    }
}
