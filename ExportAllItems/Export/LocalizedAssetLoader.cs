using StardewValley;
using LanguageCode = StardewValley.LocalizedContentManager.LanguageCode;

namespace ExportAllItems.Export
{
    /// <summary>按显式本地化资源名加载资源。</summary>
    internal static class LocalizedAssetLoader
    {
        /// <summary>加载指定语言的资源。</summary>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <param name="baseAssetName">基础资源名，例如 <c>Strings/Objects</c>。</param>
        /// <param name="language">目标语言。</param>
        /// <returns>该语言的资源。</returns>
        /// <remarks>
        /// 游戏在运行时会把基础资源名缓存为第一次加载的本地化资源名（例如中文存档缓存为
        /// <c>Strings/Objects.zh-CN</c>），此时 <c>LocalizedContentManager.Load(asset, language)</c>
        /// 会忽略语言参数并复用缓存，导致 ja/ru 等列也返回中文。这里直接传入精确的本地化资源名，
        /// 通过 <c>LoadImpl</c> 加载，从而绕过该缓存。
        /// </remarks>
        public static T Load<T>(string baseAssetName, LanguageCode language)
        {
            string normalized = baseAssetName.Replace('\\', '/');

            // 少数数据（如 Data/MonsterSlayerQuests）使用双反斜杠，归一化为单斜杠。
            while (normalized.Contains("//"))
                normalized = normalized.Replace("//", "/");

            string localizedAssetName = language == LanguageCode.en
                ? normalized
                : normalized + "." + LocalizedContentManager.LanguageCodeString(language);

            return Game1.content.LoadImpl<T>(normalized, localizedAssetName, language);
        }
    }
}
