using System;
using ExportAllItems.Export;
using ExportAllItems.Integrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

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
                "Export multilingual display names (JSON/CSV) for the selected game data types.",
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
            configMenu.AddBoolOption(
                this.ModManifest,
                () => this.Config.SplitByType,
                value => this.Config.SplitByType = value,
                () => Translate("config.split-by-type.name"),
                () => Translate("config.split-by-type.tooltip")
            );

            configMenu.AddSectionTitle(this.ModManifest, () => Translate("config.section-types"));

            void AddTypeOption(string translationKey, Func<bool> getValue, Action<bool> setValue)
            {
                configMenu.AddBoolOption(
                    this.ModManifest,
                    getValue,
                    setValue,
                    () => Translate(translationKey),
                    () => Translate("config.types.tooltip")
                );
            }

            AddTypeOption("config.types.items", () => this.Config.ExportItems, value => this.Config.ExportItems = value);
            AddTypeOption("config.types.locations", () => this.Config.ExportLocations, value => this.Config.ExportLocations = value);
            AddTypeOption("config.types.npcs", () => this.Config.ExportNpcs, value => this.Config.ExportNpcs = value);
            AddTypeOption("config.types.concessions", () => this.Config.ExportConcessions, value => this.Config.ExportConcessions = value);
            AddTypeOption("config.types.buffs", () => this.Config.ExportBuffs, value => this.Config.ExportBuffs = value);
            AddTypeOption("config.types.powers", () => this.Config.ExportPowers, value => this.Config.ExportPowers = value);
            AddTypeOption("config.types.farm-animals", () => this.Config.ExportFarmAnimals, value => this.Config.ExportFarmAnimals = value);
            AddTypeOption("config.types.pets", () => this.Config.ExportPets, value => this.Config.ExportPets = value);
            AddTypeOption("config.types.mannequins", () => this.Config.ExportMannequins, value => this.Config.ExportMannequins = value);
            AddTypeOption("config.types.minecart-destinations", () => this.Config.ExportMinecartDestinations, value => this.Config.ExportMinecartDestinations = value);
            AddTypeOption("config.types.monster-slayer-quests", () => this.Config.ExportMonsterSlayerQuests, value => this.Config.ExportMonsterSlayerQuests = value);
            AddTypeOption("config.types.passive-festivals", () => this.Config.ExportPassiveFestivals, value => this.Config.ExportPassiveFestivals = value);
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
                HashSet<string> selectedTypes = this.GetSelectedTypes();
                if (selectedTypes.Count == 0)
                {
                    this.Monitor.Log("No export types are selected. Enable at least one type in the mod settings.", LogLevel.Warn);
                    this.ShowHudMessage("hud.export-failure", HUDMessage.error_type);
                    return;
                }

                this.Monitor.Log("Collecting and exporting data...", LogLevel.Info);

                EntityExporter exporter = new(
                    this.Helper,
                    this.Monitor,
                    this.Config.ExportAllLanguages,
                    selectedTypes,
                    this.Config.SplitByType
                );
                ExportResult result = exporter.Export();

                this.Monitor.Log(
                    $"Exported {result.EntityCount} records. Languages: {string.Join(", ", result.Languages)}.",
                    LogLevel.Info
                );
                this.Monitor.Log($"Wrote {result.OutputPaths.Count} file(s):", LogLevel.Info);
                foreach (string outputPath in result.OutputPaths)
                    this.Monitor.Log(outputPath, LogLevel.Info);

                if (result.Failures.Count == 0)
                {
                    this.ShowHudMessage("hud.export-success", 0);
                }
                else
                {
                    this.Monitor.Log($"{result.Failures.Count} type(s) failed to export.", LogLevel.Warn);
                    this.ShowHudMessage(
                        result.OutputPaths.Count > 0 ? "hud.export-partial-failure" : "hud.export-failure",
                        HUDMessage.error_type
                    );
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Export failed: {ex}", LogLevel.Error);
                this.ShowHudMessage("hud.export-failure", HUDMessage.error_type);
            }
        }

        /// <summary>显示游戏自带的 HUD 提醒。</summary>
        private void ShowHudMessage(string translationKey, int messageType)
        {
            try
            {
                string message = this.Helper.Translation.Get(translationKey).ToString();
                Game1.addHUDMessage(new HUDMessage(message, messageType));
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Could not show HUD message: {ex.Message}", LogLevel.Trace);
            }
        }

        /// <summary>根据配置收集选中的导出类型。</summary>
        private HashSet<string> GetSelectedTypes()
        {
            HashSet<string> types = new(StringComparer.Ordinal);

            if (this.Config.ExportItems)
                types.Add(ExportTypes.Item);
            if (this.Config.ExportLocations)
                types.Add(ExportTypes.Location);
            if (this.Config.ExportNpcs)
                types.Add(ExportTypes.Npc);
            if (this.Config.ExportConcessions)
                types.Add(ExportTypes.Concession);
            if (this.Config.ExportBuffs)
                types.Add(ExportTypes.Buff);
            if (this.Config.ExportPowers)
                types.Add(ExportTypes.Power);
            if (this.Config.ExportFarmAnimals)
                types.Add(ExportTypes.FarmAnimal);
            if (this.Config.ExportPets)
                types.Add(ExportTypes.Pet);
            if (this.Config.ExportMannequins)
                types.Add(ExportTypes.Mannequin);
            if (this.Config.ExportMinecartDestinations)
                types.Add(ExportTypes.MinecartDestination);
            if (this.Config.ExportMonsterSlayerQuests)
                types.Add(ExportTypes.MonsterSlayerQuest);
            if (this.Config.ExportPassiveFestivals)
                types.Add(ExportTypes.PassiveFestival);

            return types;
        }
    }
}
