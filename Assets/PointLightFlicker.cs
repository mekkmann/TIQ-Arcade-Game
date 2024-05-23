using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PointLightFlicker : MonoBehaviour
{
    [SerializeField] private Light2D _light;
    public bool isTorch = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(nameof(FlickerRoutine));
    }

    // Update is called once per frame
    void Update()
    {

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
