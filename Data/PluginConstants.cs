﻿using EggLink.DanhengServer.Enums.Avatar;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DanhengPlugin.CharacterBuilder.Data;

public static class PluginConstants
{
    public static Dictionary<RelicTypeEnum, Dictionary<AvatarPropertyTypeEnum, int>> RelicMainAffix = new();
    public static Dictionary<AvatarPropertyTypeEnum, int> RelicSubAffix = new();
};