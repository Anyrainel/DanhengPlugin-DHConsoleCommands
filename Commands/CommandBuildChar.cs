using System.Text;
using DanhengPlugin.DHConsoleCommands.Data;
using EggLink.DanhengServer.Command;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Proto;

namespace DanhengPlugin.DHConsoleCommands.Commands;

[CommandInfo("buildchar", "Build a character", "Usage: /buildchar (recommend) <avatarId/all>")]
public class CommandBuildChar : ICommand
{

    [CommandMethod("0 all")]
    public async ValueTask BuildAll(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var avatarList = GameData.AvatarConfigData.Values;
        foreach (var avatarConfig in avatarList)
        {
            if (avatarConfig.AvatarID > 2000)
                continue;
            await arg.SendMsg($@"Building avatar {avatarConfig.AvatarID}");
            var avatar = player.AvatarManager!.GetFormalAvatar(avatarConfig.AvatarID);
            if (avatar == null)
                continue;
            await BuildAvatar(avatar, arg);
        }
    }

    [CommandMethod("0 recommend")]
    public async ValueTask BuildRecommend(CommandArg arg)
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

        var id = arg.GetInt(0);
        if (id == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }
        // get avatar data
        var avatar = player.AvatarManager!.GetFormalAvatar(id);
        if (avatar == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }

        await BuildAvatar(avatar, arg, true);
    }

    [CommandDefault]
    public async ValueTask BuildTarget(CommandArg arg)
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

        var id = arg.GetInt(0);
        if (id == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }
        // get avatar data
        var avatar = player.AvatarManager!.GetFormalAvatar(id);
        if (avatar == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }

        await BuildAvatar(avatar, arg);
    }

    public async ValueTask BuildAvatar(FormalAvatarInfo avatar, CommandArg arg, bool dryRun = false)
    {
        // build avatar
        var player = arg.Target!.Player!;
        PluginGameData.AvatarRelicRecommendData.TryGetValue(avatar.AvatarId, out var excel);
        if (excel is null)
        {
            await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.NoRecommend"));
            return;
        }

        List<RelicTypeEnum> relicTypes = [
            RelicTypeEnum.HEAD, RelicTypeEnum.HAND, RelicTypeEnum.BODY, RelicTypeEnum.FOOT,
            RelicTypeEnum.NECK, RelicTypeEnum.OBJECT
        ];

        StringBuilder dryRunOutput = new();

        for (int i = 0; i < relicTypes.Count; i++)
        {
            var type = relicTypes[i];
            var setId = i < 4 ? excel.Set4IDList[0] : excel.Set2IDList[0];
            var relicConfig = GameData.RelicConfigData.Values.FirstOrDefault(x =>
                x.SetID == setId && x.Type == type && x.Rarity == RarityEnum.CombatPowerRelicRarity5);
            if (relicConfig is null)
            {
                await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.NoRecommend"));
                return;
            }

            AvatarPropertyTypeEnum mainAffixProperty = type switch
            {
                RelicTypeEnum.HEAD => AvatarPropertyTypeEnum.HPDelta,
                RelicTypeEnum.HAND => AvatarPropertyTypeEnum.AttackDelta,
                _ => excel.PropertyList.FirstOrDefault(x => x.RelicType == type)?.PropertyType ?? AvatarPropertyTypeEnum.HPAddedRatio,
            };
            var subAffixPropertyList = FillAffixList(excel.SubAffixPropertyList.ToList(), mainAffixProperty, out int numPriorityAffix);
            if (subAffixPropertyList.Count != 4)
            {
                await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.AffixCountError"));
                return;
            }
            if (subAffixPropertyList.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                await arg.SendMsg($@"Unexpected: duplicate affix [{string.Join(", ", subAffixPropertyList)}]");
            }
            var mainAffixId = PluginConstants.RelicMainAffix[type].GetValueOrDefault(mainAffixProperty, 1);
            List<ItemSubAffix> subAffixes = [];
            foreach (var sub in subAffixPropertyList)
            {
                var subAffixId = PluginConstants.RelicSubAffix.GetValueOrDefault(sub, 1);
                var subAffix = new ItemSubAffix(GameData.RelicSubAffixData[5][subAffixId], 1);
                if (subAffix.Id != subAffixId)
                {
                    await arg.SendMsg($@"SubAffix ID mismatch: [{subAffixId}, {subAffix.Id}");
                }
                subAffixes.Add(subAffix);
            }

            var relic = new ItemData
            {
                ItemId = relicConfig.ID,
                Count = 1,
                Level = 15,
                MainAffix = mainAffixId,
                SubAffixes = subAffixes,
                UniqueId = ++player.InventoryManager!.Data.NextUniqueId
            };
            for (int j = 0; j < 5; j++)
            {
                int index = Random.Shared.Next(0, numPriorityAffix);
                var subAffixExcel = GameData.RelicSubAffixData[5][relic.SubAffixes[index].Id];
                relic.SubAffixes[index].IncreaseStep(subAffixExcel.StepNum);
            }

            if (dryRun)
            {
                dryRunOutput.AppendLine($@"[{(int)type}] {GetItemStr(relic)}");
            }
            else
            {
                await arg.SendMsg($@"Building {type}: /relic {GetItemStr(relic)} l15 x1");
                await player.InventoryManager.AddItem(relic, false);
                await player.InventoryManager.EquipRelic(avatar.AvatarId, relic.UniqueId, i + 1);
            }
        }

        if (dryRun)
        {
            await arg.SendMsg(dryRunOutput.ToString());
        }
        else
        {
            await arg.SendMsg(I18NManager.Translate("DHConsoleCommands.BuildSuccess"));
        }
    }

    private static List<AvatarPropertyTypeEnum> FillAffixList(List<AvatarPropertyTypeEnum> subAffixList, AvatarPropertyTypeEnum mainAffix, out int numPriorityAffix)
    {
        if (subAffixList.Contains(mainAffix))
        {
            subAffixList.Remove(mainAffix);
        }
        numPriorityAffix = subAffixList.Count;
        if (subAffixList.Count > 4) {
            numPriorityAffix = 4;
            return subAffixList.GetRange(0, 4);
        }
        if (subAffixList.Count == 4) return subAffixList;

        // First fill with speed and resist if not full
        if (!subAffixList.Contains(AvatarPropertyTypeEnum.SpeedDelta) && mainAffix != AvatarPropertyTypeEnum.SpeedDelta)
        {
            subAffixList.Add(AvatarPropertyTypeEnum.SpeedDelta);
            if (subAffixList.Count >= 4) return subAffixList;
        }
        if (!subAffixList.Contains(AvatarPropertyTypeEnum.StatusResistanceBase) && mainAffix != AvatarPropertyTypeEnum.StatusResistanceBase)
        {
            subAffixList.Add(AvatarPropertyTypeEnum.StatusResistanceBase);
            if (subAffixList.Count >= 4) return subAffixList;
        }

        List<AvatarPropertyTypeEnum> defaultAffixList = [
                AvatarPropertyTypeEnum.HPAddedRatio,
                AvatarPropertyTypeEnum.AttackAddedRatio,
                AvatarPropertyTypeEnum.DefenceAddedRatio,
            ];
        List<AvatarPropertyTypeEnum> fillerAffixList = [
                AvatarPropertyTypeEnum.HPDelta,
                AvatarPropertyTypeEnum.AttackDelta,
                AvatarPropertyTypeEnum.DefenceDelta
            ];
        // Then fill using delta stats of same type
        for (int i = 0; i < 3; i++)
        {
            if ((subAffixList.Contains(defaultAffixList[i]) || mainAffix == defaultAffixList[i]) && !subAffixList.Contains(fillerAffixList[i]))
            {
                subAffixList.Add(fillerAffixList[i]);
                if (subAffixList.Count >= 4) return subAffixList;
            }
        }
        // Lastly fill using ratio version if still not full
        for (int i = 0; i < 3; i++)
        {
            if (!subAffixList.Contains(defaultAffixList[i]) && mainAffix != defaultAffixList[i])
            {
                subAffixList.Add(defaultAffixList[i]);
                if (subAffixList.Count >= 4) return subAffixList;
            }
        }
        return subAffixList;
    }

    private string GetItemStr(ItemData item)
    {
        string itemstr = $@"{item.ItemId} {item.MainAffix}";
        foreach (var sub in item.SubAffixes)
        {
            itemstr += $@" {sub.Id}:{sub.Count}";
        }
        return itemstr;
    }
}