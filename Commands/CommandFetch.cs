using System.Text;
using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Internationalization;
using DanhengPlugin.DHConsoleCommands.Data;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.Util;


namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("fetch", "fetch data about a player", "Usage: /fetch <owned/avatar/inventory/player> ...")]
public class CommandFetch : ICommand
{

    [CommandMethod("0 owned")]
    public async ValueTask FetchOwnedCharacters(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<int> avatarIds = [];
        foreach (var avatar in player.AvatarManager!.AvatarData.FormalAvatars)
        {
            avatarIds.Add(avatar.AvatarId);
        }
        avatarIds.Sort();
        await arg.SendMsg($@"{string.Join(", ", avatarIds)}");
    }

    [CommandMethod("0 player")]
    public async ValueTask FetchPlayer(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var pathId = 0;
        // Find the main character (Traveller)
        var mainCharacter = player.AvatarManager!.AvatarData.FormalAvatars.FirstOrDefault(a => a.BaseAvatarId >= 8001 && a.BaseAvatarId <= 8008);
        
        if (mainCharacter != null)
        {
            pathId = mainCharacter.GetCurPathInfo().PathId;
        }

        await arg.SendMsg($@"level: {player.Data.Level}, gender: {(player.Data.CurrentGender == Gender.Man ? 1 : 2)}, path: {pathId}");
    }

    [CommandMethod("avatar")]
    public async ValueTask FetchAvatar(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }
        if (arg.BasicArgs.Count == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }
        var avatarId = arg.GetInt(0);
        if (avatarId == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }
        var avatar = player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatar == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }
        var path = avatar.GetCurPathInfo();
        StringBuilder output = new StringBuilder();
        output.AppendLine($@"[Character] path: {path.PathId}, level: {avatar.Level}, rank: {path.Rank}");
        output.AppendLine($@"[Talent] {string.Join("|", [.. path.GetSkillTree().Select(x => $"{x.Key}: {x.Value}")])}");
        if (path.EquipId != 0)
        {
            var item = player.InventoryManager!.Data.EquipmentItems.Find(x => x.UniqueId == path.EquipId);
            output.AppendLine($@"[Equip] id: {item?.ItemId}, level: {item?.Level}, rank: {item?.Rank}");
        }
        for (int i = 1; i <= 6; i++)
        {
            path.Relic.TryGetValue(i, out var relicUniqueId);
            if (relicUniqueId == 0) continue;
            var relic = player.InventoryManager!.Data.RelicItems.Find(x => x.UniqueId == relicUniqueId);
            if (relic == null) continue;
            var subAffixes = string.Join("|", relic.SubAffixes.Select(x => $"{GetAffixName(i, false, x.Id)}-{x.Count - 1}+{x.Step}"));
            output.AppendLine($@"[Relic {i}] id: {relic.ItemId}, level: {relic.Level}, mainAffix: {GetAffixName(i, true, relic.MainAffix)}, subAffixes: {subAffixes}");
        }
        await arg.SendMsg(output.ToString());
    }

    private static string GetAffixName(int relicPosition, bool isMainAffix, int affixId)
    {
        RelicTypeEnum relicType = (RelicTypeEnum)relicPosition;
        if (isMainAffix)
        {
            return PluginConstants.RelicMainAffix[relicType].First(x => x.Value == affixId).Key.ToString();
        }
        return PluginConstants.RelicSubAffix.First(x => x.Value == affixId).Key.ToString();
    }

    [CommandMethod("0 inventory")]
    public async ValueTask FetchInventory(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        await arg.SendMsg($@"{string.Join("\n", [.. player.InventoryManager!.Data.MaterialItems.Select(x => $"{x.ItemId}: {x.Count}")])}");
    }

    [CommandMethod("0 scene")]
    public async ValueTask FetchScene(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }
        var scene = player.SceneInstance;
        if (scene == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.SceneNotFound"));
            return;
        }
        var playerPos = player.Data.Pos;
        if (playerPos == null)
        {
            await arg.SendMsg("Player position not found");
            return;
        }
        Dictionary<string, long> output = new Dictionary<string, long>();

        foreach (var entity in scene.Entities.Values)
        {
            if (entity is EntityProp prop)
            {
                try
                {
                    if (prop.Excel.IsHpRecover || prop.Excel.IsMpRecover || prop.PropInfo.AnchorID > 0)
                    {
                        continue;
                    }
                    long distance = GetDistance(playerPos, prop.Position);
                    string type = prop.Excel.IsDoor ? "door" : prop.Excel.PropType.ToString().Replace("PROP_", "").ToLower();
                    string states = string.Join(",", prop.Excel.PropStateList.Select(x => "{x}:{(int)x}"));
                    output.Add($"{entity.GroupId}-{prop.PropInfo.ID}[{distance}]: {type} {prop.Excel.ID} {prop.State}:{(int)prop.State} ({states})", distance);
                }
                catch (Exception ex)
                {
                    output.Add($"Error processing entity {entity.GroupId}-{prop.PropInfo.ID}: {ex.Message}", long.MinValue);
                }
            }
        }
        string sortedOutput = string.Join("\n", output.OrderBy(x => x.Value).Select(x => x.Key));
        await arg.SendMsg($@"{sortedOutput}");
    }

    [CommandMethod("0 npc")]
    public async ValueTask FetchNpc(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }
        var scene = player.SceneInstance;
        if (scene == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.SceneNotFound"));
            return;
        }
        var playerPos = player.Data.Pos;
        if (playerPos == null)
        {
            await arg.SendMsg("Player position not found");
            return;
        }
        Dictionary<string, long> output = new Dictionary<string, long>();

        foreach (var entity in scene.Entities.Values)
        {
            if (entity is EntityNpc npc)
            {
                try
                {
                    long distance = GetDistance(playerPos, npc.Position);
                    output.Add($"{entity.GroupId}-{npc.InstId}[{distance}]: NpcId={npc.NpcId}", distance);
                }
                catch (Exception ex)
                {
                    output.Add($"Error processing entity {entity.GroupId}-{npc.InstId}: {ex.Message}", long.MinValue);
                }
            }
        }
        string sortedOutput = string.Join("\n", output.OrderBy(x => x.Value).Select(x => x.Key));
        await arg.SendMsg($@"{sortedOutput}");
    }

    private static long GetDistance(Position pos1, Position pos2)
    {
        try
        {
            double x = (double)pos1.X - pos2.X;
            double y = (double)pos1.Y - pos2.Y;
            double z = (double)pos1.Z - pos2.Z;
            return Convert.ToInt64(Math.Sqrt(x * x + y * y + z * z));
        }
        catch (OverflowException)
        {
            return long.MaxValue;
        }
    }
}
