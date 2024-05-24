using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PointLightFlicker : MonoBehaviour
{
    [SerializeField] private Light2D _light;
    public bool isTorch = true;

    void Start()
    {
        StartCoroutine(nameof(FlickerRoutine));
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (isTorch)
            {
                _light.intensity = Random.Range(18, 22);
            }
            else
            {
                _light.intensity = Random.Range(10.5f, 14.5f);
            }
        }
    }
}
