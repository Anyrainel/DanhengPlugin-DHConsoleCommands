using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Kcp;

namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("remove", "remove a character or unequipped relics", "Usage: /remove <avatar/relics/equipment> <avatarId>")]
public class CommandRemove : ICommand
{
    [CommandMethod("0 relics")]
    public async ValueTask RemoveRelics(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<ItemData> itemsToRemove = player.InventoryManager!.Data.RelicItems?.FindAll(x => !x.Locked && x.EquipAvatar == 0).ToList() ?? [];
        List<(int ItemId, int Count, int UniqueId)> removeData = [.. itemsToRemove.Select(x => (x.ItemId, x.Count, x.UniqueId))];
        await player.InventoryManager.RemoveItems(removeData, true);

        string output = string.Join("\n", removeData.Select(x => $"{x.UniqueId}: {x.ItemId}x{x.Count}"));
        await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.RemoveRelicsSuccess") + $"\n{output}");
    }

    [CommandMethod("0 equipment")]
    public async ValueTask RemoveEquipment(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        List<ItemData> itemsToRemove = player.InventoryManager!.Data.EquipmentItems?.FindAll(x => !x.Locked && x.EquipAvatar == 0).ToList() ?? [];
        List<(int ItemId, int Count, int UniqueId)> removeData = [.. itemsToRemove.Select(x => (x.ItemId, x.Count, x.UniqueId))];
        await player.InventoryManager.RemoveItems(removeData, true);

        string output = string.Join("\n", removeData.Select(x => $"{x.UniqueId}: {x.ItemId}x{x.Count}"));
        await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.RemoveEquipmentSuccess") + $"\n{output}");
    }

    [CommandMethod("0 avatar")]
    public async ValueTask RemoveAvatar(CommandArg arg)
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
        // get avatar data
        var avatar = player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatar == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }

        List<ItemData> itemsToUnequip = [];
        foreach (var pathInfo in avatar.PathInfos.Values)
        {
            foreach (var relic in pathInfo.Relic)
            {
                var item = player.InventoryManager!.Data.RelicItems.Find(x => x.UniqueId == relic.Value);
                if (item != null)
                {
                    item.EquipAvatar = 0; // Unequip the relic
                    itemsToUnequip.Add(item);
                }
            }

            if (pathInfo.EquipId != 0)
            {
                var equipment = player.InventoryManager!.Data.EquipmentItems.Find(x => x.UniqueId == pathInfo.EquipId);
                if (equipment != null)
                {
                    equipment.EquipAvatar = 0; // Unequip the light cone
                    itemsToUnequip.Add(equipment);
                }
            }
        }
        await player.SendPacket(new PacketPlayerSyncScNotify(itemsToUnequip));
        player.AvatarManager!.AvatarData.FormalAvatars.Remove(avatar);
        DatabaseHelper.SaveInstance(player.AvatarManager!.AvatarData);
        await player.SendPacket(new PacketPlayerSyncScNotify(avatar));

        await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.RemoveAvatarSuccess"));
        await arg.Target!.Player!.SendPacket(new PacketPlayerKickOutScNotify());
        arg.Target!.Stop();
    }

}