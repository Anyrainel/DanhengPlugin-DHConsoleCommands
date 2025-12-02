using System.Text;
using System.Numerics;
using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Data;
using Newtonsoft.Json;
using EggLink.DanhengServer.Util;

namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("gametext", "return in-game translation for a certain language", "Usage: /gametext <avatar/item/mainmission/submission> #<language>")]
public class CommandGameText : ICommand
{

    internal static string currentLanguage = "None";
    internal static Dictionary<BigInteger, string> textMap = new();

    [CommandMethod("0 avatar")]
    public async ValueTask GetAvatarText(CommandArg arg)
    {
        arg.CharacterArgs.TryGetValue("#", out var lang);
        if (lang == null || lang == "")
        {
            await arg.SendMsg("Usage: /gametext avatar #<language>");
            return;
        }

        LoadTextMap(lang);

        StringBuilder output = new();
        foreach (var avatar in GameData.AvatarConfigData.Values)
        {
            var name = textMap.TryGetValue(avatar.AvatarName.Hash, out var value) ? value : $"[{avatar.AvatarName.Hash}]";
            output.AppendLine(avatar.AvatarID + ": " + name);
        }

        await arg.SendMsg(output.ToString());
    }

    [CommandMethod("0 item")]
    public async ValueTask GetItemText(CommandArg arg)
    {
        arg.CharacterArgs.TryGetValue("#", out var lang);
        if (lang == null || lang == "")
        {
            await arg.SendMsg("Usage: /gametext item #<language>");
            return;
        }

        LoadTextMap(lang);

        StringBuilder output = new();
        foreach (var item in GameData.ItemConfigData.Values)
        {
            var name = textMap.TryGetValue(item.ItemName.Hash, out var value) ? value : $"[{item.ItemName.Hash}]";
            output.AppendLine(item.ID + ": " + name);
        }
        await arg.SendMsg(output.ToString());
    }

    [CommandMethod("0 mainmission")]
    public async ValueTask GetMainMissionText(CommandArg arg)
    {
        arg.CharacterArgs.TryGetValue("#", out var lang);
        if (lang == null || lang == "")
        {
            await arg.SendMsg("Usage: /gametext mainmission #<language>");
            return;
        }

        LoadTextMap(lang);

        StringBuilder output = new();
        foreach (var mission in GameData.MainMissionData.Values)
        {
            var name = textMap.TryGetValue(mission.Name.Hash, out var value) ? value : $"[{mission.Name.Hash}]";
            output.AppendLine(mission.MainMissionID + ": " + name);
        }
        await arg.SendMsg(output.ToString());
    }

    [CommandMethod("0 submission")]
    public async ValueTask GetSubmissionText(CommandArg arg)
    {
        arg.CharacterArgs.TryGetValue("#", out var lang);
        if (lang == null || lang == "")
        {
            await arg.SendMsg("Usage: /gametext submission #<language>");
            return;
        }

        LoadTextMap(lang);

        StringBuilder output = new();
        foreach (var mission in GameData.SubMissionData.Values)
        {
            var name = textMap.TryGetValue(mission.TargetText.Hash, out var value) ? value : $"[{mission.TargetText.Hash}]";
            output.AppendLine(mission.SubMissionID + ": " + name);
        }
        await arg.SendMsg(output.ToString());
    }

    [CommandMethod("0 relic")]
    public async ValueTask GetRelicTypes(CommandArg arg)
    {
        StringBuilder output = new();
        foreach (var relic in GameData.RelicConfigData.Values)
        {
            output.AppendLine(relic.ID + ": " + (int)relic.Type);
        }
        await arg.SendMsg(output.ToString());
    }

    [CommandDefault]
    public async ValueTask GetGameText(CommandArg arg)
    {
        await arg.SendMsg("Usage: /gametext <avatar/item/mainmission/submission> #<language>");
    }

    internal static void LoadTextMap(string lang)
    {
        if (lang == currentLanguage) return;
        var textMapPath = ConfigManager.Config.Path.ResourcePath + "/TextMap/TextMap" + lang + ".json";
        if (!File.Exists(textMapPath))
        {
            // TODO: add error handling
            return;
        }
        var textMapData = JsonConvert.DeserializeObject<Dictionary<BigInteger, string>>(File.ReadAllText(textMapPath));
        if (textMapData == null)
        {
            // TODO: add error handling
            return;
        }
        textMap = textMapData;
        currentLanguage = lang;
    }

    public static string GetTranslatedText(BigInteger hash, string lang)
    {
        LoadTextMap(lang); // Ensure the map is loaded for the requested language
        if (textMap.TryGetValue(hash, out var value))
        {
            return value;
        }
        return $"[{hash}]"; // Fallback to ID if not found
    }

}