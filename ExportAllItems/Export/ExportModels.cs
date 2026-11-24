using LanguageCode = StardewValley.LocalizedContentManager.LanguageCode;

namespace ExportAllItems.Export
{
    /// <summary>尚未解析显示名称的实体。</summary>
    internal sealed class RawEntity
    {
        /// <summary>实体类型：<c>Item</c>、<c>Location</c> 或 <c>NPC</c>。</summary>
        public string Type { get; init; } = "";

        /// <summary>导出用的唯一键（物品为限定物品 ID，地点与 NPC 带前缀）。</summary>
        public string Key { get; init; } = "";

        /// <summary>数据资源中的原始键。</summary>
        public string Id { get; init; } = "";

        /// <summary>原始显示名称（通常是 <c>[LocalizedText ...]</c> 标记，也可能是普通文本）。</summary>
        public string DisplayToken { get; init; } = "";

        /// <summary>无法解析显示名称时使用的回退名称。</summary>
        public string FallbackName { get; init; } = "";

        /// <summary>若显示名称来自按语言本地化的数据资源（例如 <c>Data/Boots</c>），则为此资源名。</summary>
        public string? LegacyDataAsset { get; init; }

        /// <summary>本地化数据中以斜杠分隔的显示名称字段索引。</summary>
        public int LegacyNameIndex { get; init; }

        /// <summary>创建使用本地化标记的实体。</summary>
        public static RawEntity FromToken(string type, string key, string id, string? displayToken, string? fallbackName)
        {
            return new RawEntity
            {
                Type = type,
                Key = key,
                Id = id,
                DisplayToken = displayToken ?? "",
                FallbackName = string.IsNullOrWhiteSpace(fallbackName) ? id : fallbackName!
            };
        }

        /// <summary>创建显示名称位于按语言本地化数据资源中的实体。</summary>
        public static RawEntity FromLegacy(string type, string key, string id, string dataAsset, int nameIndex, string? fallbackName)
        {
            return new RawEntity
            {
                Type = type,
                Key = key,
                Id = id,
                DisplayToken = "",
                FallbackName = string.IsNullOrWhiteSpace(fallbackName) ? id : fallbackName!,
                LegacyDataAsset = dataAsset,
                LegacyNameIndex = nameIndex
            };
        }
    }

    /// <summary>已解析多语言显示名称的实体。</summary>
    internal sealed class EntityRecord
    {
        /// <summary>实体类型：<c>Item</c>、<c>Location</c> 或 <c>NPC</c>。</summary>
        public string Type { get; init; } = "";

        /// <summary>导出用的唯一键。</summary>
        public string Key { get; init; } = "";

        /// <summary>数据资源中的原始键。</summary>
        public string Id { get; init; } = "";

        /// <summary>语言代码（如 <c>en</c>、<c>zh-CN</c>）到显示名称的映射。</summary>
        public Dictionary<string, string> DisplayNames { get; init; } = new();
    }

    /// <summary>一个参与导出的语言。</summary>
    /// <param name="Code">游戏语言枚举。</param>
    /// <param name="Column">导出时使用的语言代码，例如 <c>zh-CN</c>。</param>
    internal sealed record ExportLanguage(LanguageCode Code, string Column);

    /// <summary>一次导出的结果摘要。</summary>
    /// <param name="EntityCount">导出的实体数量。</param>
    /// <param name="Languages">导出的语言代码。</param>
    /// <param name="OutputPaths">写出的 JSON/CSV 文件路径。</param>
    internal sealed record ExportResult(int EntityCount, IReadOnlyList<string> Languages, IReadOnlyList<string> OutputPaths);
}
