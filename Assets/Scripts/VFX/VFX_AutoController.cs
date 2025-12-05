using System.Collections;
using UnityEngine;

public class VFX_AutoController : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private bool autoDestory = true;
    [SerializeField] private float destoryDelay = 1;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private bool randomOffset = true;

    [Header("Can Effect")]
    [SerializeField] private bool canFade;
    [SerializeField] private float fadeSpeed = 1;

    [Header("Random Rotation")]
    [SerializeField] private float minRotation = 0;
    [SerializeField] private float maxRotation = 360;

    [Header("Random Position")]
    [SerializeField] private float xMinOffset = -.3f;
    [SerializeField] private float xMaxOffset = .3f;
    [Space]
    [SerializeField] private float yMinOffset = -.3f;
    [SerializeField] private float yMaxOffset = .3f;


    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {

        if (canFade)
            StartCoroutine(FadeCo());

        ApplyRandomOffset();
        ApplyRandomRotation();
        if (autoDestory)
            Destroy(gameObject, destoryDelay);
    }


    private IEnumerator FadeCo()
    {
        Color targetColor = Color.white;

        while (targetColor.a > 0)
        {
            targetColor.a -= fadeSpeed * Time.deltaTime;
            sr.color = targetColor;
            yield return null;

        }

        sr.color = targetColor;
    }
    private void ApplyRandomOffset()
    {
        if (randomOffset == false)
            return;

        float xOffset = Random.Range(xMinOffset, xMaxOffset);
        float yOffset = Random.Range(yMinOffset, yMaxOffset);

        transform.position = transform.position + new Vector3(xOffset, yOffset);
    }

    private void ApplyRandomRotation()
    {
        if (randomRotation == false)
            return;
        float zRotation = Random.Range(minRotation, maxRotation);
        transform.Rotate(0, 0, zRotation);

    }

}
