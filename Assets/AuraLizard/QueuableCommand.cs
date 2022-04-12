using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QueuableCommand
{
    public abstract bool Execute(Graph target);
}

public class AddEdge : QueuableCommand
{
    string source, destination;

    public override bool Execute(Graph target)
    {
        target.AddEdge(source, destination);
        return true;
    }
}

public class RemoveEdge : QueuableCommand
{
    string source, destination;

    public override bool Execute(Graph target)
    {
        target.RemoveEdge(source, destination);
        return true;
    }
}