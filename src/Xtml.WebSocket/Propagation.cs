namespace Xtml.WebSocket;

internal class Propagation
{
    public int CurrentID { get; set; }
    public int CurrentLevel { get; set; }
    public int SuppressID { get; set; }
    public int SuppressLevel { get; set; }
    public bool IsStopped => CurrentID == SuppressID && CurrentLevel >= SuppressLevel;

    public void Stop()
    {
        SuppressID = CurrentID;
        SuppressLevel = CurrentLevel + 1;
    }

    public void StopImmediate()
    {
        SuppressID = CurrentID;
        SuppressLevel = 0;
    }
}