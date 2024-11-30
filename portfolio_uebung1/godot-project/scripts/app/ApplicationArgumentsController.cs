using System.Collections.Generic;
using Godot;

namespace SA4E.scripts.app;

public class ApplicationArgumentsController
{
    public static Dictionary<string, string> Arguments { get; private set; }
    public static Dictionary<string, string> UserArguments { get; private set; }


    private static Dictionary<string, string> _SplitArguments(string[] args)
    {
        var argsDict = new Dictionary<string, string>();
        
        foreach (var argument in args)
        {
            if (argument. Contains('='))
            {
                string[] keyValue = argument. Split("=");
                argsDict[keyValue[0].TrimPrefix("--")] = keyValue[1];
            }
            else
            {
                // Options without an argument will be present in the dictionary,
                // with the value set to an empty string.
                argsDict[argument. TrimPrefix("--")] = "";
            }
        }
        
        return argsDict;
    }

    public static void ReadArguments()
    {
        Arguments = _SplitArguments(OS.GetCmdlineArgs());
        UserArguments = _SplitArguments(OS.GetCmdlineUserArgs());
    }

    public static bool TryParseArgumentToInt(string name, out int parsedValue)
    {
        if (!Arguments.TryGetValue(name, out string value))
        {
            parsedValue = 0;
            return false;
        }

        return int.TryParse(value, out parsedValue);
    }
    public static bool TryParseArgumentToBool(string name, out bool parsedValue)
    {
        if (!Arguments.TryGetValue(name, out string value))
        {
            parsedValue = false;
            return false;
        }

        return bool.TryParse(value, out parsedValue);
    }
}