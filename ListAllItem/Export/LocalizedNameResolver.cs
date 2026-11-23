using System.Globalization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TokenizableStrings;
using LanguageCode = StardewValley.LocalizedContentManager.LanguageCode;

namespace ListAllItem.Export
{
    /// <summary>按指定语言解析游戏数据中的 <c>[LocalizedText ...]</c> 显示名称。</summary>
    internal sealed class LocalizedNameResolver
    {
        /*********
        ** 字段
        *********/
        /// <summary>目标语言。</summary>
        private readonly LanguageCode Language;

        /// <summary>日志接口。</summary>
        private readonly IMonitor Monitor;

        /// <summary>已加载的字符串资源（资源名 → 字典；加载失败时为 <c>null</c>）。</summary>
        private readonly Dictionary<string, Dictionary<string, string>?> Cache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>英文回退解析器。</summary>
        private LocalizedNameResolver? EnglishResolver;


        /*********
        ** 公共方法
        *********/
        /// <summary>构造解析器。</summary>
        /// <param name="language">目标语言。</param>
        /// <param name="monitor">日志接口。</param>
        public LocalizedNameResolver(LanguageCode language, IMonitor monitor)
        {
            this.Language = language;
            this.Monitor = monitor;
        }

        /// <summary>解析显示名称。</summary>
        /// <param name="token">原始显示名称（可能是本地化标记或普通文本）。</param>
        /// <param name="fallback">无法解析时使用的回退名称。</param>
        /// <returns>该语言下的显示名称。</returns>
        public string Resolve(string? token, string fallback)
        {
            if (string.IsNullOrWhiteSpace(token))
                return fallback;

            string raw = token.Trim();

            // 普通文本直接返回。
            if (!raw.StartsWith("[LocalizedText ", StringComparison.Ordinal))
                return raw;

            if (!TryParseLocalizedText(raw, out string assetName, out string key, out string arguments))
                return fallback;

            string? text = this.GetString(assetName, key);

            // 标记的键本身可能包含空格；此时把“参数”部分也视为键再试一次。
            if (text is null && arguments.Length > 0)
            {
                string? combined = this.GetString(assetName, key + " " + arguments);
                if (combined is not null)
                {
                    text = combined;
                    arguments = "";
                }
            }

            if (text is null)
                return fallback;

            if (arguments.Length == 0)
                return text;

            string resolvedArguments;
            try
            {
                // 例如 [EscapedText [FarmName]]；这类参数与语言无关，交给游戏解析。
                resolvedArguments = TokenParser.ParseText(arguments);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"解析显示名称参数失败（{assetName}:{key}）：{ex.Message}", LogLevel.Trace);
                return text;
            }

            return ApplyArgument(text, resolvedArguments);
        }


        /*********
        ** 私有方法
        *********/
        /// <summary>从字符串资源中读取文本；目标语言缺少该键时回退英文。</summary>
        private string? GetString(string assetName, string key)
        {
            Dictionary<string, string>? strings = this.LoadStrings(assetName);
            if (strings is not null && strings.TryGetValue(key, out string? value))
                return value;

            if (this.Language == LanguageCode.en)
                return null;

            this.EnglishResolver ??= new LocalizedNameResolver(LanguageCode.en, this.Monitor);
            return this.EnglishResolver.GetString(assetName, key);
        }

        /// <summary>加载指定语言的字符串资源。</summary>
        private Dictionary<string, string>? LoadStrings(string assetName)
        {
            if (this.Cache.TryGetValue(assetName, out Dictionary<string, string>? cached))
                return cached;

            string normalized = assetName.Replace('\\', '/');
            Dictionary<string, string>? data = null;
            try
            {
                data = Game1.content.Load<Dictionary<string, string>>(normalized, this.Language);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"无法加载本地化资源 {normalized}（{this.Language}）：{ex.Message}", LogLevel.Trace);
            }

            this.Cache[assetName] = data;
            return data;
        }

        /// <summary>把参数代入文本中的 <c>{0}</c> 占位符。</summary>
        private static string ApplyArgument(string template, string arguments)
        {
            if (string.IsNullOrEmpty(arguments) || template.IndexOf('{') < 0)
                return template;

            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, arguments);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        /// <summary>解析形如 <c>[LocalizedText 资源:键 参数]</c> 的标记。</summary>
        /// <param name="raw">原始标记。</param>
        /// <param name="assetName">资源名。</param>
        /// <param name="key">文本键。</param>
        /// <param name="arguments">键之后的参数文本（可能为空）。</param>
        /// <returns>是否解析成功。</returns>
        private static bool TryParseLocalizedText(string raw, out string assetName, out string key, out string arguments)
        {
            assetName = "";
            key = "";
            arguments = "";

            if (!raw.StartsWith("[LocalizedText ", StringComparison.Ordinal))
                return false;

            // 找到最外层 [ ... ]，允许参数中包含嵌套标记。
            int depth = 0;
            int end = -1;
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == '[')
                {
                    depth++;
                }
                else if (raw[i] == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        end = i;
                        break;
                    }

                    if (depth < 0)
                        return false;
                }
            }

            if (end < 0)
                return false;

            string inner = raw.Substring(1, end - 1);
            const string prefix = "LocalizedText";
            if (!inner.StartsWith(prefix, StringComparison.Ordinal))
                return false;

            string rest = inner.Substring(prefix.Length).TrimStart();
            int colon = rest.IndexOf(':');
            if (colon <= 0)
                return false;

            assetName = rest.Substring(0, colon).Trim();
            string keyAndArguments = rest.Substring(colon + 1).Trim();
            if (keyAndArguments.Length == 0)
                return false;

            int split = FindTopLevelSpace(keyAndArguments);
            if (split < 0)
            {
                key = keyAndArguments;
            }
            else
            {
                key = keyAndArguments.Substring(0, split).Trim();
                arguments = keyAndArguments.Substring(split + 1).Trim();
            }

            return assetName.Length > 0 && key.Length > 0;
        }

        /// <summary>寻找不在嵌套标记内的第一个空白字符。</summary>
        private static int FindTopLevelSpace(string text)
        {
            int depth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '[')
                {
                    depth++;
                }
                else if (text[i] == ']')
                {
                    depth--;
                }
                else if (depth == 0 && char.IsWhiteSpace(text[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
