using UnityEngine;
using UnityEngine.UI; // 必须引入 UI 命名空间

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Image))] // 强制要求物体上必须有 Image 组件
public class UI_HealthShard_Item : MonoBehaviour
{
    private Animator anim;

    // 声明一个 Image 变量，用来控制 UI 图片的颜色
    private Image shardImage;

    private bool isFull;

    [Header("Glow Settings (HDR)")]
    // 带有 [ColorUsage(true, true)] 标签，开启 HDR 拾色器
    [ColorUsage(true, true)]
    [SerializeField] private Color fullColor = Color.white;  // 满血发光色

    [ColorUsage(true, true)]
    [SerializeField] private Color emptyColor = Color.gray;  // 空血暗淡色

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // 在游戏开始时，获取挂在这个碎片上的 Image 组件
        shardImage = GetComponent<Image>();
    }

    public void Initialize(bool startFull)
    {
        isFull = startFull;
        anim.Play(isFull ? "Full_Idle" : "Empty_Idle");

        // 生成碎片时，立刻赋值一次颜色
        ApplyHDRColor();
    }

    public void UpdateState(bool shouldBeFull)
    {
        if (isFull == shouldBeFull) return;

        isFull = shouldBeFull;

        if (isFull)
            anim.SetTrigger("Restore");
        else
            anim.SetTrigger("Shatter");

        // 玩家受伤或回血导致状态切换时，重新赋值颜色
        ApplyHDRColor();
    }

    // ==========================================
    // 核心赋值方法：将面板里调好亮度的颜色赋给图片
    // ==========================================
    private void ApplyHDRColor()
    {
        // 这一句是一个“三元运算符”，意思等同于：
        // if (isFull) { shardImage.color = fullColor; }
        // else        { shardImage.color = emptyColor; }

        shardImage.color = isFull ? fullColor : emptyColor;
    }
}