using UnityEngine;

public interface ICounterable
{
    public bool CanBeCountered { get; }

    public void HandleCounter();

}
