using System;
using ExportAllItems.Export;
using ExportAllItems.Integrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ExportAllItems
{
    /// <summary>模组入口点。</summary>
    public class ModEntry : Mod
    {
        /// <summary>GMCM 的 UniqueID；GMCM 是可选依赖。</summary>
        private const string ConfigMenuUniqueId = "spacechase0.GenericModConfigMenu";

        /// <summary>当前配置。</summary>
        private ModConfig Config = new();

        /// <summary>当前导出快捷键；<see cref="SButton.None"/> 表示禁用。</summary>
        private SButton ExportKey = SButton.F8;

        /// <summary>GMCM API；未安装 GMCM 时为 <c>null</c>。</summary>
        private IGenericModConfigMenuApi? ConfigMenu;

        /*********
        ** 公共方法
        *********/
        /// <summary>模组的入口点，在首次加载模组后自动调用。</summary>
        /// <param name="helper">对象 helper 提供用于编写模组的简化接口。</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.ExportKey = this.Config.ExportKey;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.ConsoleCommands.Add(
                "lia_export",
                "Export multilingual display names (JSON/CSV) for items, locations, and NPCs.",
                this.OnExportCommand
            );
        }

        /*********
        ** 私有方法
        *********/
        /// <summary>在游戏加载完成后注册可选的 GMCM 配置页面。</summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">事件数据。</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // GMCM 是可选依赖：未安装、未加载或 API 不可用时都直接跳过。
            if (!this.Helper.ModRegistry.IsLoaded(ConfigMenuUniqueId))
                return;

            try
            {
                this.ConfigMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(ConfigMenuUniqueId);
                if (this.ConfigMenu is null)
                    return;

                this.RegisterConfigMenu();
            }
            catch (Exception ex)
            {
                this.ConfigMenu = null;
                this.Monitor.Log($"Could not initialize Generic Mod Config Menu integration: {ex.Message}", LogLevel.Warn);
            }
        }

        /// <summary>注册 GMCM 设置项。</summary>
        private void RegisterConfigMenu()
        {
            IGenericModConfigMenuApi configMenu = this.ConfigMenu!;
            string Translate(string key) => this.Helper.Translation.Get(key).ToString();

            configMenu.Register(
                this.ModManifest,
                () =>
                {
                    this.Config = new ModConfig();
                    this.ExportKey = this.Config.ExportKey;
                    this.Helper.WriteConfig(this.Config);
                },
                () =>
                {
                    this.Helper.WriteConfig(this.Config);
                    this.ExportKey = this.Config.ExportKey;
                }
            );

            configMenu.AddSectionTitle(this.ModManifest, () => Translate("config.section-title"));
            configMenu.AddKeybind(
                this.ModManifest,
                () => this.Config.ExportKey,
                value =>
                {
                    this.Config.ExportKey = value;
                    this.ExportKey = value;
                },
                () => Translate("config.export-key.name"),
                () => Translate("config.export-key.tooltip")
            );
            configMenu.AddBoolOption(
                this.ModManifest,
                () => this.Config.ExportAllLanguages,
                value => this.Config.ExportAllLanguages = value,
                () => Translate("config.export-all-languages.name"),
                () => Translate("config.export-all-languages.tooltip")
            );
        }

        /// <summary>在玩家按下键盘、控制器或鼠标上的按钮后引发。</summary>
        /// <param name="sender">对象 sender 表示调用此方法的对象。</param>
        /// <param name="e">对象 e 表示事件数据。</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (this.ExportKey != SButton.None && e.Button == this.ExportKey)
                this.Export();
        }

        /// <summary>处理 <c>lia_export</c> 控制台指令。</summary>
        /// <param name="command">指令名称。</param>
        /// <param name="args">指令参数（未使用）。</param>
        private void OnExportCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("Please enter a save before exporting.", LogLevel.Warn);
                return;
            }

            this.Export();
        }

        /// <summary>执行导出并向控制台报告结果。</summary>
        private void Export()
        {
            try
            {
                this.Monitor.Log("Collecting and exporting data...", LogLevel.Info);

                EntityExporter exporter = new(this.Helper, this.Monitor, this.Config.ExportAllLanguages);
                ExportResult result = exporter.Export();

                this.Monitor.Log(
                    $"Exported {result.EntityCount} records. Languages: {string.Join(", ", result.Languages)}.",
                    LogLevel.Info
                );
                this.Monitor.Log($"JSON: {result.JsonPath}", LogLevel.Info);
                this.Monitor.Log($"CSV: {result.CsvPath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Export failed: {ex}", LogLevel.Error);
            }
        }
    }
}
