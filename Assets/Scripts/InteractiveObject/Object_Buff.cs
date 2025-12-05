using System.Collections;
using UnityEngine;

[System.Serializable]
public class Buff 
{
    public StatType type;
    public float value;


}


public class Object_Buff : MonoBehaviour
{
    private SpriteRenderer sr;
    private Entity_Stats statToModify;


    [Header("Buff Details")]
    [SerializeField] private Buff[] buffs;
    [SerializeField] private string buffName;
    [SerializeField] private float buffDuartion = 4;
    [SerializeField] private bool canBeUsed = true;




    [Header("Float Movement")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPosition + new Vector3(0, yOffset, 0);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeUsed == false)
            return;

        statToModify = collision.GetComponent<Entity_Stats>();
        StartCoroutine(BuffCo(buffDuartion));
    }

    private IEnumerator BuffCo(float duration)
    {
        canBeUsed = false;
        sr.color = Color.clear;
        ApplyBuff(true);

        yield return new WaitForSeconds(duration);

        Debug.Log("Buff is removed!");
        ApplyBuff(false);

        Destroy(gameObject);
    }

    private void ApplyBuff(bool apply)
    {
        foreach (var buff in buffs)
        {
            if(apply)
                statToModify.GetStatByType(buff.type).AddModifier(buff.value, buffName);
            else
                statToModify.GetStatByType(buff.type).RemoveModifier(buffName);


        }
    }



}
