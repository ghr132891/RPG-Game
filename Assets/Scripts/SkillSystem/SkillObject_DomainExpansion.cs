using UnityEngine;

public class SkillObject_DomainExpansion : SkillObject_Base
{
    private Skill_DomainExpansion domaonExpansion;

    private float duration;
    private float expandSpeed = 2;
    private float slowDownPercent = .9f;

    private Vector3 targetScale;
    private bool isShrinking;

    public void SetUpDomain(Skill_DomainExpansion _DomainExpansion)
    {
        this.domaonExpansion = _DomainExpansion;

        duration = domaonExpansion.GetDomainDuration();
        float maxSize = domaonExpansion.maxDomainSize;
        slowDownPercent = domaonExpansion.GetSlowPercentage();
        expandSpeed = domaonExpansion.expandSpeed;

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
