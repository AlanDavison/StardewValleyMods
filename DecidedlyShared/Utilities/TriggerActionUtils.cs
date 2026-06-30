using System.Text;
using StardewValley.Delegates;

namespace DecidedlyShared.Utilities;

public class TriggerActionUtils
{
    public static string GatherContext(string initialContextMessage, TriggerActionContext context)
    {
        StringBuilder errorContext = new StringBuilder();

        errorContext.AppendLine(initialContextMessage);
        errorContext.AppendLine($"Trigger: {context.Trigger}");
        foreach (string arg in context.TriggerArgs)
        {
            errorContext.AppendLine($"Trigger arg: {arg}");
        }

        return errorContext.ToString();
    }
}
