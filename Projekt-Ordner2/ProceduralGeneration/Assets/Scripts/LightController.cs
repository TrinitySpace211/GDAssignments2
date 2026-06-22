using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {
    [SerializeField] private Transform playerTransform;
    public float activationDistance = 15f;
    public float checkInterval = 0.2f;

    private HashSet<Light> dungeonLights = new HashSet<Light>();
    private float sqrActivationDistance;

    private void Start() {
        sqrActivationDistance = activationDistance * activationDistance;

        InvokeRepeating(nameof(CheckLightDistances), 0f, checkInterval);
    }

    public void RegisterLight(Light newLight) {
        dungeonLights.Add(newLight);
    }

    private void CheckLightDistances() {
        if (playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;

        foreach (Light light in dungeonLights) {
            if (light == null) continue;

            float sqrDistance = (light.transform.position - playerPos).sqrMagnitude;

            if (sqrDistance <= sqrActivationDistance) {
                if (!light.enabled) light.enabled = true;
            } else {
                if (light.enabled) light.enabled = false;
            }
        }
    }
}
