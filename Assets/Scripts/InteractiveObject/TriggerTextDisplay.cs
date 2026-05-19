using UnityEngine;

public class TriggerTextDisplay : MonoBehaviour
{
    [Header("要控制的文字物体")]
    [Tooltip("把场景里那个隐藏的 TextMeshPro 物体拖到这里")]
    public GameObject textObject;

    [Header("离开区域后是否隐藏")]
    [Tooltip("如果勾选，玩家离开区域后文字会再次消失；不勾选则会永久显示")]
    public bool hideOnExit = true;

    private void Start()
    {
        // 游戏开始时兜底确保文字是隐藏的
        if (textObject != null)
        {
            textObject.SetActive(false);
        }
    }

    // 当有物体进入触发区域时执行
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查进入区域的是不是玩家 (根据你的 Player 脚本或者 Tag 判定)
        if (other.CompareTag("Player") || other.GetComponent<Player>() != null)
        {
            if (textObject != null)
            {
                textObject.SetActive(true); // 显示文字
            }
        }
    }

    // 当物体离开触发区域时执行
    private void OnTriggerExit2D(Collider2D other)
    {
        if (hideOnExit)
        {
            if (other.CompareTag("Player") || other.GetComponent<Player>() != null)
            {
                if (textObject != null)
                {
                    textObject.SetActive(false); // 隐藏文字
                }
            }
        }
    }
}
