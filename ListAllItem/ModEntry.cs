using System;
using ListAllItem.Export;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ListAllItem
{
    /// <summary>模组入口点。</summary>
    public class ModEntry : Mod
    {
        /// <summary>当前配置。</summary>
        private ModConfig Config = new();

        /// <summary>导出快捷键；配置无法识别时为 <c>null</c>。</summary>
        private SButton? ExportKey;

        /*********
        ** 公共方法
        *********/
        /// <summary>模组的入口点，在首次加载模组后自动调用。</summary>
        /// <param name="helper">对象 helper 提供用于编写模组的简化接口。</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.ExportKey = ParseKey(this.Config.ExportKey);

            if (!string.IsNullOrWhiteSpace(this.Config.ExportKey) && this.ExportKey is null)
            {
                this.Monitor.Log($"无法识别快捷键 \"{this.Config.ExportKey}\"，已禁用快捷键；仍可使用控制台指令 lia_export。", LogLevel.Warn);
            }

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.ConsoleCommands.Add(
                "lia_export",
                "导出物品、地点与 NPC 的多语言显示名称（JSON/CSV）。",
                this.OnExportCommand
            );
        }

        /*********
        ** 私有方法
        *********/
        /// <summary>在玩家按下键盘、控制器或鼠标上的按钮后引发。</summary>
        /// <param name="sender">对象 sender 表示调用此方法的对象。</param>
        /// <param name="e">对象 e 表示事件数据。</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (this.ExportKey == e.Button)
                this.Export();
        }

        /// <summary>处理 <c>lia_export</c> 控制台指令。</summary>
        /// <param name="command">指令名称。</param>
        /// <param name="args">指令参数（未使用）。</param>
        private void OnExportCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("请先进入存档再导出。", LogLevel.Warn);
                return;
            }

            this.Export();
        }

        /// <summary>执行导出并向控制台报告结果。</summary>
        private void Export()
        {
            try
            {
                this.Monitor.Log("开始收集并导出数据……", LogLevel.Info);

                EntityExporter exporter = new(this.Helper, this.Monitor);
                ExportResult result = exporter.Export();

                this.Monitor.Log(
                    $"已导出 {result.EntityCount} 条记录；语言：{string.Join(", ", result.Languages)}。",
                    LogLevel.Info
                );
                this.Monitor.Log($"JSON：{result.JsonPath}", LogLevel.Info);
                this.Monitor.Log($"CSV ：{result.CsvPath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"导出失败：{ex}", LogLevel.Error);
            }
        }

        /// <summary>把配置中的文本解析为 SMAPI 按键。</summary>
        /// <param name="value">配置值，例如 <c>F8</c>。</param>
        /// <returns>解析成功时返回按键；否则返回 <c>null</c>。</returns>
        private static SButton? ParseKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Enum.TryParse(value.Trim(), ignoreCase: true, out SButton key) ? key : null;
        }
    }
}
