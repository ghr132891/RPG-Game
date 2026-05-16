using UnityEngine;

public class Enemy_VFX : Entity_VFX
{
    [Header("Counter Attack Window")]
    [SerializeField] private GameObject attackAlert;

    // ===============================
    // 【新增】：弹反/打击高光特效槽位
    // ===============================
    [Header("Impact VFX")]
    [SerializeField] private GameObject impactFlashPrefab;

    public void EnableAttackAlert(bool enbale)
    {
        if (attackAlert == null)
            return;

        attackAlert.SetActive(enbale);
    }

    // 【新增】：播放弹反瞬间的高亮爆闪
    public void PlayParryFlash()
    {
        if (impactFlashPrefab != null)
        {
            // 在怪物中心位置生成光源。
            // (如果怪物中心点在脚下，你想让光在胸口亮起，可以改为 transform.position + new Vector3(0, 1f, 0))
            Instantiate(impactFlashPrefab, transform.position, Quaternion.identity);
        }
    }
}