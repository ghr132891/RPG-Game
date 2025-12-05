using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    protected SpriteRenderer sr;
    private Entity entity;

    [Header("On Taking Damage VFX")]
    [SerializeField] private Material onDamageMaterial;
    private Material originalMaterial;
    [SerializeField] private float onDamageVfxDuration = .2f;
    private Coroutine onDamageVfxCoroutine;

    [Header("On Doing Damage")]
    [SerializeField] private Color hitOnVFXColor = Color.white;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject critHitVFX;

    [Header("Element Color")]
    [SerializeField] private Color chillVFX = Color.cyan;
    [SerializeField] private Color burnVFX = Color.red;
    [SerializeField] private Color shockVFX = Color.yellow;
    private Color originalHitVFXColor;
    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        entity = GetComponent<Entity>();
        originalMaterial = sr.material;
        originalHitVFXColor = hitOnVFXColor;

    }

    public void StopAllVFX()
    {
        StopAllCoroutines();
        sr.color = Color.white;
        sr.material = originalMaterial;
    }

    public void PlayOnStatusVFX(float duration, ElementType element)
    {
        if (element == ElementType.Ice)
            StartCoroutine(PlayStatusVFXCo(duration, chillVFX));

        if (element == ElementType.Fire)
            StartCoroutine(PlayStatusVFXCo(duration, burnVFX));

        if (element == ElementType.Lightning)
            StartCoroutine(PlayStatusVFXCo(duration, shockVFX));



    }
    private IEnumerator PlayStatusVFXCo(float duration, Color effectColor)
    {
        float tickInterval = .25f;
        float timeHasPassed = 0;

        Color lightColor = effectColor * 1.2f;
        Color darkColor = effectColor * .8f;

        bool trigger = false;

        while (timeHasPassed < duration)
        {
            sr.color = trigger ? lightColor : darkColor;
            trigger = !trigger;

            yield return new WaitForSeconds(tickInterval);
            timeHasPassed += tickInterval;

        }

        sr.color = Color.white;

    }


    public void CreatOnHitVFX(Transform target, bool isCrit,ElementType element)
    {
        GameObject hitPrefab = isCrit ? critHitVFX : hitVFX;
        GameObject vfx = Instantiate(hitPrefab, target.position, Quaternion.identity);
        vfx.GetComponentInChildren<SpriteRenderer>().color = GetElementColor(element);

        if (entity.facingDir == -1 && isCrit)
            vfx.transform.Rotate(0, 180, 0);
    }


    

    public Color GetElementColor(ElementType element)
    {
        switch (element)
        {


            case ElementType.Ice:
                return chillVFX;
            case ElementType.Fire:
                return burnVFX;
            case ElementType.Lightning:
                return shockVFX;

            default:
                return Color.white;


        }
    }



    public void PlayerOnDamageVfx()
    {
        if (onDamageVfxCoroutine != null)
            StopCoroutine(onDamageVfxCoroutine);

        onDamageVfxCoroutine = StartCoroutine(OnDamageVfxCo());
    }
    private IEnumerator OnDamageVfxCo()
    {
        sr.material = onDamageMaterial;
        yield return new WaitForSeconds(onDamageVfxDuration);

        sr.material = originalMaterial;
    }

}
