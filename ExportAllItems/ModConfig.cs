using StardewModdingAPI;

namespace ExportAllItems
{
    /// <summary>模组配置。</summary>
    public sealed class ModConfig
    {
        /// <summary>按下此按键时导出；设为 <see cref="SButton.None"/> 可禁用快捷键（仍可使用控制台指令）。</summary>
        public SButton ExportKey { get; set; } = SButton.F8;

        /// <summary>是否导出所有已安装语言；关闭时只导出英文和当前游戏语言。</summary>
        public bool ExportAllLanguages { get; set; } = true;

        /// <summary>是否按类型拆分导出到独立子文件夹。</summary>
        public bool SplitByType { get; set; } = false;

        /// <summary>是否导出物品。</summary>
        public bool ExportItems { get; set; } = true;

        /// <summary>是否导出地点。</summary>
        public bool ExportLocations { get; set; } = true;

        /// <summary>是否导出 NPC。</summary>
        public bool ExportNpcs { get; set; } = true;

        /// <summary>是否导出电影院零食。</summary>
        public bool ExportConcessions { get; set; } = true;

        /// <summary>是否导出增益。</summary>
        public bool ExportBuffs { get; set; } = true;

        /// <summary>是否导出能力。</summary>
        public bool ExportPowers { get; set; } = true;

        /// <summary>是否导出农场动物。</summary>
        public bool ExportFarmAnimals { get; set; } = true;

        /// <summary>是否导出宠物。</summary>
        public bool ExportPets { get; set; } = true;

        /// <summary>是否导出人体模型。</summary>
        public bool ExportMannequins { get; set; } = true;

        /// <summary>是否导出矿车目的地。</summary>
        public bool ExportMinecartDestinations { get; set; } = true;

        /// <summary>是否导出怪物消灭目标。</summary>
        public bool ExportMonsterSlayerQuests { get; set; } = true;

        /// <summary>是否导出被动节日。</summary>
        public bool ExportPassiveFestivals { get; set; } = true;
    }
}
