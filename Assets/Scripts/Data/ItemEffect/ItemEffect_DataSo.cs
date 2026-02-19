using UnityEngine;

public class ItemEffect_DataSo : ScriptableObject
{
    [TextArea]
    public string effectdescription;
    protected Player player;
    public virtual bool CanBeUsed(Player player)
    {
        return true;
    }

    public virtual void ExecuteEffect()
    {

          
    }

    public virtual void Subsribe(Player play)
    {
        this.player = play;
    }

    public virtual void Unsubribe()
    {

    }
}
