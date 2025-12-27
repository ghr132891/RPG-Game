using UnityEngine;

public class SkillObject_DomainExpansion : SkillObject_Base
{
    private Skill_DomainExpansion domainManager;

    private float duration;
    private float expandSpeed = 2;
    private float slowDownPercent = .9f;

    private Vector3 targetScale;
    private bool isShrinking;

    public void SetUpDomain(Skill_DomainExpansion _DomainExpansion)
    {
        this.domainManager = _DomainExpansion;

        duration = domainManager.GetDomainDuration();
        float maxSize = domainManager.maxDomainSize;
        slowDownPercent = domainManager.GetSlowPercentage();
        expandSpeed = domainManager.expandSpeed;

        targetScale = Vector3.one * maxSize;
        Invoke(nameof(ShrinkDomain), duration);
    }

    private void Update()
    {
        HandleScaling();
    }

    private void HandleScaling()
    {
        float sizeDiffrence = Mathf.Abs(transform.lossyScale.x - targetScale.x);
        bool shouldChangeScale = sizeDiffrence > .1f;

        if (shouldChangeScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, expandSpeed * Time.deltaTime);
        }

        if (isShrinking && sizeDiffrence < .1f)
        {
            TerminateDomain();
        }

    }

    private void TerminateDomain()
    {
        domainManager.ClearTarget();
        Destroy(gameObject);
    }

    private void ShrinkDomain()
    {
        targetScale = Vector3.zero;
        isShrinking = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null)
        {
            return;
        }

        domainManager.AddTarget(enemy);
        enemy.SlowDownEntity(duration, slowDownPercent,true);
        
    }

    
    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy == null)
            return;

        enemy.StopSlowDown();
    }

}
