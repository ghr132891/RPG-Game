using UnityEngine;

public class Enemy_MageSpellCastState : EnemyState
{
    private Enemy_Mage enemyMage;
    private bool hasCastFinished;
    private float waitTimer;

    public Enemy_MageSpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();
        enemyMage.SetVelocity(0, 0);
        enemyMage.SetSpellCastPerformed(false);
        hasCastFinished = false;
        waitTimer = 0f;


    }

    public override void Update()
    {
        base.Update();

        // 步骤 1：检测协程是否扔完了所有炸弹
        if (enemyMage.spellCastPerformed && !hasCastFinished)
        {

            hasCastFinished = true;

            anim.SetBool("spellCast_Performed", true);
            waitTimer = 1.5f;
        }

        // 步骤 2：倒计时与退出逻辑
        if (hasCastFinished)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                // 倒计时结束，检查动画是否也播完了
                if (triggerCalled)
                {

                    EvaluateParryResult();
                }
                else
                {
                    // 【终极防卡死】：时间到了但没检测到动画事件！强制退出！
                    
                    EvaluateParryResult();
                }
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("spellCast_Performed", false);

    }

    private void EvaluateParryResult()
    {
        if (enemyMage.bombsParriedCount >= enemyMage.requiredParriesToPerfect)
        {
            // 【核心修改】完美弹反造成的伤害依然生效：临时强制赋予受击权限
            enemyMage.health.SetCanTakeDamage(true);

            IDamagable damagable = enemyMage.GetComponent<IDamagable>();
            if (damagable != null) damagable.TakeDamage(enemyMage.allParriedBonusDamage, 0, ElementType.None, enemyMage.transform);

            stateMachine.ChangeState(enemyMage.mageWeakState);
        }
        else if (enemyMage.bombsParriedCount >= enemyMage.requiredParriesToWeak)
        {
            stateMachine.ChangeState(enemyMage.mageWeakState);
        }
        else
        {
            stateMachine.ChangeState(enemyMage.mageBattleState);
        }
    }
}