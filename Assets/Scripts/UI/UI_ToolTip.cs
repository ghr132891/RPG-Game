using UnityEngine;

public class UI_ToolTip : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private Vector2 offSet = new Vector2(300, 520);

    protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public  virtual void ShowToolTip(bool show, RectTransform targetRect)
    {
        if (show == false)
        {
            rect.position = new Vector2(9999, 9999);
            return;

        }

        UpdatePosistion(targetRect);

    }


    private void UpdatePosistion(RectTransform targetRect)
    {
        float screenCenterX = Screen.width / 2;
        float screenTop = Screen.height;
        float screenBottom = 0;

        Vector2 targetPosition = targetRect.position;

        targetPosition.x = targetPosition.x > screenCenterX ? targetPosition.x - offSet.x : targetPosition.x + offSet.x;

        float toolTipHeightHalf = rect.sizeDelta.y / 2f;
        float topY = targetPosition.y + toolTipHeightHalf;
        float bottomY = targetPosition.y - toolTipHeightHalf;

        if (topY > screenTop)
        {
            targetPosition.y = screenTop - toolTipHeightHalf - offSet.y;
        }
        else if (bottomY < screenBottom)
        {
            targetPosition.y = screenBottom + toolTipHeightHalf + offSet.y;
        }

        rect.position = targetPosition;

    }
    protected string GetColoredText(string color, string text)
    {
        return $"<color={color}>{text}</color>";
    }



}
