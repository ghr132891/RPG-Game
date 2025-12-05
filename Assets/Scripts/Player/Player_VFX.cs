using System.Collections;

using UnityEngine;

public class Player_VFX : Entity_VFX
{
    [Header("Image Echo VFX")]
    [Range(0.01f, .2f)]
    [SerializeField] private float imageEchointerval = 0.05f;
    [SerializeField] private GameObject imageEchoPrefab;
    private Coroutine imageEchoCo;

    public void DoImageEchoEffect(float duration)
    {
        if (imageEchoCo != null)
            StopCoroutine(imageEchoCo);

        imageEchoCo = StartCoroutine(ImageEchoEffectCo(duration));


    }


    private IEnumerator ImageEchoEffectCo(float duration)

    {
        float timeTracked = 0;

        while (timeTracked < duration)
        {
            CreatImageEcho();
            yield return new WaitForSeconds(imageEchointerval);
            timeTracked += imageEchointerval;
        }


    }

    private void CreatImageEcho()
    {
        GameObject imageEcho = Instantiate(imageEchoPrefab, transform.position, transform.rotation);
        imageEcho.GetComponentInChildren<SpriteRenderer>().sprite = sr.sprite;



    }

}
