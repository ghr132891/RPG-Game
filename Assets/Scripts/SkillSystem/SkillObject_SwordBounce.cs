using System.Collections.Generic;
using UnityEngine;

public class SkillObject_SwordBounce : SkillObject_Sword
{
    [SerializeField] private float bounceSpeed = 15f;
    private int bounceCount;

    private Collider2D[] enemyTargets;
    private Transform nextTarget;
    private List<Transform> selectedBegfore = new List<Transform>();


    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        anim?.SetTrigger("spin");
        base.SetupSword(swordManager, direction);

        bounceCount = swordManager.bounceCount;
        bounceSpeed = swordManager.bounceSpeed;


    }
    protected override void Update()
    {
        HandleComeback();
        HandleBounce();
        //Debug.Log(bounceCount);
        
    }


    private void HandleBounce()
    {
        if (nextTarget == null)
            return;

        transform.position = Vector2.MoveTowards(transform.position, nextTarget.position, bounceSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, nextTarget.position) <= .75f)
        {
            DamageEnemiesInRadius(transform, 1);
            BounceToNextTarget();

            if (bounceCount == 0 || nextTarget == null)
            {
                nextTarget = null;
                GetSwordBackToPlayer();

            }

        }

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyTargets == null)
        {
            enemyTargets = GetEnemiesAround(transform, 10);
            rb.simulated = false;
        }

        DamageEnemiesInRadius(transform, 1);

        if (enemyTargets.Length <= 1 || bounceCount == 0)
        {
            GetSwordBackToPlayer();
        }
        else
        {

        }
        {
            selectedBegfore.Add(collision.transform);//触发对象应该加入 selectBefore 列表，否则 GeteNextTarget（）会再次选择触发对象。
            nextTarget = GetNextTarget();
        }

    }
    private void BounceToNextTarget()
    {
        nextTarget = GetNextTarget();

        bounceCount--;

    }

    private Transform GetNextTarget()
    {
        List<Transform> validTargets = GetValidTargets();
        
        if(validTargets.Count > 0)
        {
        int randomIndex = Random.Range(0, validTargets.Count);
        
        Transform nextTarget = validTargets[randomIndex];
        selectedBegfore.Add(nextTarget);

        return nextTarget;
        }
        else
        {
            return null;
        }

    }
    private List<Transform> GetValidTargets()
    {
        List<Transform> validTargets = new List<Transform>();
        List<Transform> aliveTargets = GetAliveTargets();

    

        foreach (var enemy in aliveTargets)
        {
            if (enemy != null && selectedBegfore.Contains(enemy.transform) == false)
                validTargets.Add(enemy.transform);
        }

        if (validTargets.Count > 0)
            return validTargets;
        else
        {
            // 'aliveTargets' 应该在清除之前移除最后一个 'selectedBefore' 的目标，否则最后一个目标会被重新选择。
            if (selectedBegfore.Count > 0)
            {
                Transform lastSelected = selectedBegfore[selectedBegfore.Count - 1];
                if (aliveTargets.Contains(lastSelected))
                    aliveTargets.Remove(lastSelected);
            }

            selectedBegfore.Clear();
            return aliveTargets;
        }


    }
    private List<Transform> GetAliveTargets()
    {
        List<Transform> aliveTargets = new List<Transform>();

        enemyTargets = GetEnemiesAround(transform, 10);
 

        foreach (var enemy in enemyTargets)
        {
            if (enemy != null)
                aliveTargets.Add(enemy.transform);
        }
        return aliveTargets;
    }






}
