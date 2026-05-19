using System.Collections;

using UnityEngine;

public class Player_VFX : Entity_VFX
{
    [Header("Time Rewind")]
    [Tooltip("将时间回溯的特效对象（如屏幕滤镜或跟随粒子的GameObject）拖入这里")]
    public GameObject timeRewindVFXObject; // 【新增】：特效对象的引用


    public void CreateEffectOf(GameObject effect, Transform target)
    {
        Instantiate(effect, target.position, Quaternion.identity);
    }

    // 【新增】：开关时间回溯特效
    public void ToggleTimeRewindVFX(bool isActive)
    {
        if (timeRewindVFXObject != null)
        {
            timeRewindVFXObject.SetActive(isActive);
        }
    }

}
