using UnityEngine;

[System.Serializable]
public abstract class TaskBase : MonoBehaviour
{
    public abstract string GetTaskName();
    public abstract string GetTaskObjective();
    public abstract bool IsTaskComplete();
    public abstract void DeliverLetter(string targetResident);
}