using System.Collections;
using UnityEngine;

[System.Serializable]



public class Object_Buff : MonoBehaviour
{
    private Player_Stats statToModify;


    [Header("Buff Details")]
    [SerializeField] private BuffEffectData[] buffs;
    [SerializeField] private string buffName;
    [SerializeField] private float buffDuartion = 4;




    [Header("Float Movement")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;

    private void Awake()
    {      
        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPosition + new Vector3(0, yOffset, 0);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
       ;

        statToModify = collision.GetComponent<Player_Stats>();

        if (statToModify.CanApplyBuffOf(buffName))
        {
           statToModify.ApplyBuff(buffs,buffDuartion,buffName);
            Destroy(gameObject);
        }
    }

   
}
