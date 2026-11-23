namespace ListAllItem
{
    /// <summary>模组配置。</summary>
    public sealed class ModConfig
    {
        /// <summary>按下此按键时导出。留空字符串可禁用快捷键（仍可使用控制台指令）。</summary>
        public string ExportKey { get; set; } = "F8";
    }
}
