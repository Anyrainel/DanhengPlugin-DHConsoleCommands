using System.Text;
using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Internationalization;
using DanhengPlugin.DHConsoleCommands.Data;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Proto;


namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("debug", "debug item equip status", "Usage: /debug <avataritem/avatarrelic/item/relic>")]
public class CommandDebug : ICommand
{

    [CommandMethod("0 avataritem")]
    public async ValueTask DebugAvatarItem(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<string> results = [];
        foreach (var avatar in player.AvatarManager!.AvatarData.Avatars)
        {
            results.Add($"{avatar.AvatarId}: {avatar.GetCurPathInfo().EquipId}");
        }
        results.Sort();
        await arg.SendMsg($@"{string.Join("\n", results)}");
    }

    [CommandMethod("0 avatarrelic")]
    public async ValueTask DebugAvatarRelic(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<string> results = [];
        foreach (var avatar in player.AvatarManager!.AvatarData.Avatars)
        {
            List<string> relics = [];
            foreach (var relic in avatar.GetCurPathInfo().Relic)
            {
                relics.Add($"{relic.Key}: {relic.Value}");
            }
            results.Add($"{avatar.AvatarId}: {string.Join(", ", relics)}");
        }
        results.Sort();
        await arg.SendMsg($@"{string.Join("\n", results)}");
    }

    [CommandMethod("0 item")]
    public async ValueTask DebugItem(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<string> results = [];
        foreach (var item in player.InventoryManager!.Data.EquipmentItems)
        {
            results.Add($"{item.UniqueId}: {item.EquipAvatar} ({item.ItemId})");
        }
        results.Sort();
        await arg.SendMsg($@"{string.Join("\n", results)}");
    }

    [CommandMethod("0 relic")]
    public async ValueTask DebugRelic(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<string> results = [];
        foreach (var item in player.InventoryManager!.Data.RelicItems)
        {
            results.Add($"{item.UniqueId}: {item.EquipAvatar} ({item.ItemId})");
        }
        results.Sort();
        await arg.SendMsg($@"{string.Join("\n", results)}");
    }
}