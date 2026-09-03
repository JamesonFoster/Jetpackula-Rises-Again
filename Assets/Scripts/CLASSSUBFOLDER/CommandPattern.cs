using UnityEngine;

[DisallowMultipleComponent]
public class CommandPattern : MonoBehaviour
{
    public float moveDistance = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}


public interface ICommand
{
    void Execute();
    void Undo();
}

public class MoveCommand : ICommand
{
    public void Execute()
    {
        
    }

    public void Undo()
    {
        
    }
}