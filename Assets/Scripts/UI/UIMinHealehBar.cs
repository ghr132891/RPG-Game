using UnityEngine;

public class UIMinHealehBar : MonoBehaviour
{
    private Entity entity ;


    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }
    private void OnEnable()
    {
        entity.OnFlipped += Handleflip;
    }

    private void OnDisable()
    {
        entity.OnFlipped -= Handleflip;
    }

    private  void Handleflip()
    {
        transform.rotation = Quaternion.identity;
    }
}
