using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] TankController target;
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeIntensity = 0.5f;
    Coroutine shake_coroutine;

    // Start is called before the first frame update
    void Start()
    {
        target.OnShoot += ShakeCamera;
    }

    public void ShakeCamera()
    {
        if (shake_coroutine != null) StopCoroutine(shake_coroutine);
        shake_coroutine = StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float timeElasped = 0f;

        while (timeElasped < shakeDuration)
        {
            transform.localPosition = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity), 
                Random.Range(-shakeIntensity, shakeIntensity), 
                Random.Range(-shakeIntensity, shakeIntensity)
            );

            timeElasped += Time.deltaTime;
            yield return timeElasped;
        }

        transform.localPosition = Vector3.zero;
        shake_coroutine = null;
    }
}
