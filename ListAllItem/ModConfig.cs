using StardewModdingAPI;

namespace ListAllItem
{
    /// <summary>模组配置。</summary>
    public sealed class ModConfig
    {
        /// <summary>按下此按键时导出；设为 <see cref="SButton.None"/> 可禁用快捷键（仍可使用控制台指令）。</summary>
        public SButton ExportKey { get; set; } = SButton.F8;

        /// <summary>是否导出所有已安装语言；关闭时只导出英文和当前游戏语言。</summary>
        public bool ExportAllLanguages { get; set; } = true;
    }
}
