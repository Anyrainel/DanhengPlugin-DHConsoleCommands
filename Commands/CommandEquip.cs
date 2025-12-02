using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Kcp;

namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("equip", "Equip a character", "Usage: /equip item <avatarId> <itemId> l<level> r<rank> or /equip relic <avatarId> <relicId> <mainAffixId> <subAffixId*4>:<level*4>")]
public class CommandEquip : ICommand
{
    [CommandMethod("0 item")]
    public async ValueTask EquipItem(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        int avatarId = arg.GetInt(0);
        int itemId = arg.GetInt(1);

        var avatar = player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatar == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        arg.CharacterArgs.TryGetValue("r", out var rankStr);
        levelStr ??= "1";
        rankStr ??= "1";
        if (!int.TryParse(levelStr, out var level) || !int.TryParse(rankStr, out var rank))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var curItemData = player.InventoryManager!.Data.EquipmentItems.Find(x => x.UniqueId == avatar.GetCurPathInfo().EquipId);
        // if the item is already equipped, just update the level and rank
        if (curItemData != null && curItemData.ItemId == itemId)
        {
            int promotion = GameData.GetMinPromotionForLevel(level);
            curItemData.Level = level;
            curItemData.Promotion = promotion;
            curItemData.Rank = rank;
            curItemData.Exp = 0;
            await player.SendPacket(new PacketPlayerSyncScNotify(avatar, curItemData));
            return;
        }
        else // otherwise, add the item to the inventory
        {
            var itemData = await player.InventoryManager!.AddItem(itemId, 1, rank: rank, level: level, sync: false);
            if (itemData == null)
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Give.ItemNotFound"));
                return;
            }

            await player.InventoryManager!.EquipAvatar(avatarId, itemData.UniqueId);
            await player.SendPacket(new BasePacket(CmdIds.DressAvatarScRsp));
        }

        await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.EquipSuccess"));
    }

    [CommandMethod("0 relic")]
    public async ValueTask EquipRelic(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        int avatarId = arg.GetInt(0);
        int relicId = arg.GetInt(1);
        var avatar = player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatar == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }
        GameData.RelicConfigData.TryGetValue(relicId, out var relicConfig);
        if (relicConfig == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.RelicNotFound"));
            return;
        }
        int slotId = (int)relicConfig.Type;

        var startIndex = 2;
        var mainAffixId = 0;
        if (!arg.BasicArgs[startIndex].Contains(':'))
        {
            mainAffixId = int.Parse(arg.BasicArgs[startIndex]);
            startIndex++;
        }

        var subAffixes = new List<(int, int)>();
        for (var ii = startIndex; ii < arg.BasicArgs.Count; ii++)
        {
            var subAffix = arg.BasicArgs[ii].Split(':');
            if (subAffix.Length != 2 || !int.TryParse(subAffix[0], out var subId) ||
                !int.TryParse(subAffix[1], out var subLevel))
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
                return;
            }
            // input is upgrade level, so total level is upgrade level + 1
            subAffixes.Add((subId, subLevel + 1));
        }
        (var ret, var itemData) = await player.InventoryManager!.HandleRelic(
            relicId, ++player.InventoryManager!.Data.NextUniqueId,
            15, mainAffixId, subAffixes);

        switch (ret)
        {
            case 1:
                await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.RelicNotFound"));
                return;
            case 2:
                await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.InvalidMainAffixId"));
                return;
            case 3:
                await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.InvalidSubAffixId"));
                return;
        }

        if (itemData == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.RelicNotFound"));
            return;
        }
        await player.InventoryManager!.EquipRelic(avatarId, itemData.UniqueId, slotId);
        await player.SendPacket(new BasePacket(CmdIds.DressRelicAvatarScRsp));
        await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.EquipSuccess"));
    }

}