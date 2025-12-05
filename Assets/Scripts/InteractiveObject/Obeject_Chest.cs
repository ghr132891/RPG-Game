using UnityEngine;

public class Obeject_Chest : MonoBehaviour, IDamagable
{
    private Rigidbody2D rb => GetComponentInChildren<Rigidbody2D>();
    private Animator anim => GetComponentInChildren<Animator>();
    private Entity_VFX vfx =>GetComponent<Entity_VFX>();

    [Header("Open Details")]
    [SerializeField] private Vector2 openSence;
    public bool TakeDamage(float damage, float elementalDamage,ElementType element , Transform damageDealer)
    {
        vfx.PlayerOnDamageVfx();
        anim.SetBool("chestOpen",true);
        rb.linearVelocity = openSence;
        rb.angularVelocity = Random.Range(-200f,200f);

        return true;
    }


}
