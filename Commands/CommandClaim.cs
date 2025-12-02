using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Internationalization;

namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("claim", "claim rewards", "Usage: /claim <promotion>")]
public class CommandClaim : ICommand
{
    [CommandMethod("0 promotion")]
    public async ValueTask ClaimPromotion(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        int count = 0;
        
        foreach (var avatar in player.AvatarManager!.AvatarData.FormalAvatars)
        {
            int[] rewardLevels = { 1, 3, 5 };
            bool changed = false;
            
            foreach (var p in rewardLevels)
            {
                if (avatar.Promotion >= p && !avatar.HasTakenReward(p))
                {
                    avatar.TakeReward(p);
                    count++;
                    changed = true;
                }
            }
            
            if (changed)
            {
                await arg.Target!.SendPacket(new PacketPlayerSyncScNotify(avatar));
            }
        }

        if (count > 0)
        {
            // 101 is Star Rail Pass (Standard Ticket)
            await player.InventoryManager!.AddItem(101, count);
            await arg.SendMsg($"Claimed {count} promotion rewards (Star Rail Pass).");
        }
        else
        {
            await arg.SendMsg("No unclaimed promotion rewards found.");
        }
    }
}