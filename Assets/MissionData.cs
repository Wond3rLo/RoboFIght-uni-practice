using System.Collections.Generic;
using UnityEngine;

public static class MissionLog
{
    public static List<string> LastCommands = new List<string>();
    public static bool WasSuccessful;
    public static int StepsTaken;
}