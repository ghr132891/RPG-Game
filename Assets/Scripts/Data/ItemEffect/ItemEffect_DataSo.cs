using UnityEngine;

public class ItemEffect_DataSo : ScriptableObject
{
    [TextArea]
    public string effectdescription;

    public virtual bool CanBeUsed()
    {
        return true;
    }
    public virtual void ExecuteEffect()
    {

          
    }

}
