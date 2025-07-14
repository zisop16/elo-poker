using Godot;

public static class Assert
{
    public static void That(bool condition, string message = null)
    {
        if (!condition)
        {
            if(message is not null)
                GD.PushError("Assertion failed:", message);
            else
                GD.PushError("Assertion failed!");
            
            EngineDebugger.Debug(true, true);
        }
    }
}