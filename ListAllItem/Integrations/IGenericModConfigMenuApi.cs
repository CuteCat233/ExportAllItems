using StardewModdingAPI;

namespace ListAllItem.Integrations
{
    /// <summary>Generic Mod Config Menu API 中本模组使用的子集。</summary>
    public interface IGenericModConfigMenuApi
    {
        /// <summary>注册模组的配置页面。</summary>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        /// <summary>添加小节标题。</summary>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);

        /// <summary>添加按键绑定选项。</summary>
        void AddKeybind(
            IManifest mod,
            Func<SButton> getValue,
            Action<SButton> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string? fieldId = null
        );

        /// <summary>添加布尔选项。</summary>
        void AddBoolOption(
            IManifest mod,
            Func<bool> getValue,
            Action<bool> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string? fieldId = null
        );
    }
}
