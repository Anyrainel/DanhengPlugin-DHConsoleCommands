using DanhengPlugin.DHConsoleCommands.Commands;
using DanhengPlugin.DHConsoleCommands.Data;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.GameServer.Plugin.Constructor;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Util;

namespace DanhengPlugin.DHConsoleCommands;

[PluginInfo("DHConsoleCommands", "DHConsole is ready to use commands", "1.2.0")]
public class DHConsoleCommands : IPlugin
{
    private readonly Logger _logger = new("DHConsoleCommands");

    public void OnLoad()
    {
        CommandManager.Instance?.RegisterCommand(typeof(CommandBuildChar));
        CommandManager.Instance?.RegisterCommand(typeof(CommandRemove));
        CommandManager.Instance?.RegisterCommand(typeof(CommandGameText));
        CommandManager.Instance?.RegisterCommand(typeof(CommandFetch));
        CommandManager.Instance?.RegisterCommand(typeof(CommandEquip));
        CommandManager.Instance?.RegisterCommand(typeof(CommandDebugLink));
        CommandManager.Instance?.RegisterCommand(typeof(CommandClaim));
        _logger.Info(I18NManager.Translate("DHConsoleCommands.LoadedDHConsoleCommands"));
        // load data
        ResourceManager.LoadSingleExcel<AvatarRelicRecommendExcel>(typeof(AvatarRelicRecommendExcel));

        PluginConstants.RelicMainAffix.Add(RelicTypeEnum.HEAD, []);
        PluginConstants.RelicMainAffix.Add(RelicTypeEnum.HAND, []);
        PluginConstants.RelicMainAffix.Add(RelicTypeEnum.BODY, []);
        PluginConstants.RelicMainAffix.Add(RelicTypeEnum.FOOT, []);
        PluginConstants.RelicMainAffix.Add(RelicTypeEnum.NECK, []);
        PluginConstants.RelicMainAffix.Add(RelicTypeEnum.OBJECT, []);

        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.HPDelta, 1);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.AttackDelta, 2);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.DefenceDelta, 3);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.HPAddedRatio, 4);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.AttackAddedRatio, 5);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.DefenceAddedRatio, 6);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.SpeedDelta, 7);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.CriticalChanceBase, 8);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.CriticalDamageBase, 9);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.StatusProbabilityBase, 10);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.StatusResistanceBase, 11);
        PluginConstants.RelicSubAffix.Add(AvatarPropertyTypeEnum.BreakDamageAddedRatioBase, 12);

        PluginConstants.RelicMainAffix[RelicTypeEnum.HEAD][AvatarPropertyTypeEnum.HPDelta] = 1;

        PluginConstants.RelicMainAffix[RelicTypeEnum.HAND][AvatarPropertyTypeEnum.AttackDelta] = 1;

        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.HPAddedRatio] = 1;
        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.AttackAddedRatio] = 2;
        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.DefenceAddedRatio] = 3;
        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.CriticalChanceBase] = 4;
        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.CriticalDamageBase] = 5;
        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.HealRatioBase] = 6;
        PluginConstants.RelicMainAffix[RelicTypeEnum.BODY][AvatarPropertyTypeEnum.StatusProbabilityBase] = 7;

        PluginConstants.RelicMainAffix[RelicTypeEnum.FOOT][AvatarPropertyTypeEnum.HPAddedRatio] = 1;
        PluginConstants.RelicMainAffix[RelicTypeEnum.FOOT][AvatarPropertyTypeEnum.AttackAddedRatio] = 2;
        PluginConstants.RelicMainAffix[RelicTypeEnum.FOOT][AvatarPropertyTypeEnum.DefenceAddedRatio] = 3;
        PluginConstants.RelicMainAffix[RelicTypeEnum.FOOT][AvatarPropertyTypeEnum.SpeedDelta] = 4;

        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.HPAddedRatio] = 1;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.AttackAddedRatio] = 2;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.DefenceAddedRatio] = 3;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.PhysicalAddedRatio] = 4;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.FireAddedRatio] = 5;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.IceAddedRatio] = 6;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.ThunderAddedRatio] = 7;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.WindAddedRatio] = 8;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.QuantumAddedRatio] = 9;
        PluginConstants.RelicMainAffix[RelicTypeEnum.NECK][AvatarPropertyTypeEnum.ImaginaryAddedRatio] = 10;

        PluginConstants.RelicMainAffix[RelicTypeEnum.OBJECT][AvatarPropertyTypeEnum.BreakDamageAddedRatioBase] = 1;
        PluginConstants.RelicMainAffix[RelicTypeEnum.OBJECT][AvatarPropertyTypeEnum.SPRatioBase] = 2;
        PluginConstants.RelicMainAffix[RelicTypeEnum.OBJECT][AvatarPropertyTypeEnum.HPAddedRatio] = 3;
        PluginConstants.RelicMainAffix[RelicTypeEnum.OBJECT][AvatarPropertyTypeEnum.AttackAddedRatio] = 4;
        PluginConstants.RelicMainAffix[RelicTypeEnum.OBJECT][AvatarPropertyTypeEnum.DefenceAddedRatio] = 5;
    }

    public void OnUnload()
    {
        RemoveCommand("buildchar");
        RemoveCommand("remove");
        RemoveCommand("gametext");
        RemoveCommand("fetch");
        RemoveCommand("equip");
        RemoveCommand("debuglink");
        RemoveCommand("claim");

        PluginConstants.RelicMainAffix.Clear();
        PluginConstants.RelicSubAffix.Clear();

        _logger.Info(I18NManager.Translate("DHConsoleCommands.UnloadedDHConsoleCommands"));
    }

    private void RemoveCommand(string name)
    {
        if (CommandManager.Instance == null) return;

        if (CommandManager.Instance.CommandAlias.TryGetValue(name, out var realName)) name = realName;
        
        if (CommandManager.Instance.Commands.ContainsKey(name))
        {
            CommandManager.Instance.Commands.Remove(name);
            CommandManager.Instance.CommandInfo.Remove(name);
            
            // remove alias
            var aliases = CommandManager.Instance.CommandAlias.Where(x => x.Value == name).Select(x => x.Key).ToList();
            foreach (var alias in aliases)
                CommandManager.Instance.CommandAlias.Remove(alias);
        }
    }
}