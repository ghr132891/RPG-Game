using UnityEngine;

public interface IDamagable
{
    public bool TakeDamage(float damage,float elementalDamage,ElementType element ,Transform damageDealer);

}
