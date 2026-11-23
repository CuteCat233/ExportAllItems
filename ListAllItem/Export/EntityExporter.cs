using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using LanguageCode = StardewValley.LocalizedContentManager.LanguageCode;

namespace ListAllItem.Export
{
    /// <summary>收集实体并导出 JSON / CSV。</summary>
    internal sealed class EntityExporter
    {
        /*********
        ** 常量
        *********/
        /// <summary>导出目录名（相对于模组目录）。</summary>
        private const string OutputFolderName = "exports";

        /// <summary>JSON 序列化设置。</summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };


        /*********
        ** 字段
        *********/
        /// <summary>SMAPI 助手。</summary>
        private readonly IModHelper Helper;

        /// <summary>日志接口。</summary>
        private readonly IMonitor Monitor;


        /*********
        ** 公共方法
        *********/
        /// <summary>构造导出器。</summary>
        /// <param name="helper">SMAPI 助手。</param>
        /// <param name="monitor">日志接口。</param>
        public EntityExporter(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;
        }

        /// <summary>收集实体、解析多语言名称并写入导出文件。</summary>
        /// <returns>导出结果摘要。</returns>
        public ExportResult Export()
        {
            List<ExportLanguage> languages = GetLanguages(this.Monitor);
            List<RawEntity> rawEntities = this.CollectEntities();

            Dictionary<LanguageCode, LocalizedNameResolver> resolvers = new();
            foreach (ExportLanguage language in languages)
                resolvers[language.Code] = new LocalizedNameResolver(language.Code, this.Monitor);

            // Data/Boots 与 Data/hats 的显示名称直接写在按语言本地化的数据资源里。
            Dictionary<string, Dictionary<string, string>?> legacyDataCache = new(StringComparer.Ordinal);

            List<EntityRecord> records = new(rawEntities.Count);
            foreach (RawEntity entity in rawEntities)
            {
                Dictionary<string, string> names = new(languages.Count, StringComparer.Ordinal);
                foreach (ExportLanguage language in languages)
                {
                    string name = entity.LegacyDataAsset is not null
                        ? this.GetLegacyDisplayName(entity, language.Code, legacyDataCache)
                        : resolvers[language.Code].Resolve(entity.DisplayToken, entity.FallbackName);

                    names[language.Column] = name;
                }

                records.Add(new EntityRecord
                {
                    Type = entity.Type,
                    Key = entity.Key,
                    Id = entity.Id,
                    DisplayNames = names
                });
            }

            // 输出目录固定为 <模组目录>/exports。
            string outputDirectory = Path.Combine(this.Helper.DirectoryPath, OutputFolderName);
            Directory.CreateDirectory(outputDirectory);

            string jsonPath = Path.Combine(outputDirectory, "entities.json");
            string csvPath = Path.Combine(outputDirectory, "entities.csv");

            string[] languageColumns = languages.Select(language => language.Column).ToArray();
            ExportDocument document = new()
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Languages = languageColumns,
                Counts = records
                    .GroupBy(record => record.Type)
                    .ToDictionary(group => group.Key, group => group.Count()),
                Entities = records
            };

            string json = JsonSerializer.Serialize(document, JsonOptions);
            File.WriteAllText(jsonPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.WriteAllText(csvPath, BuildCsv(languages, records), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            return new ExportResult(records.Count, languageColumns, jsonPath, csvPath);
        }


        /*********
        ** 实体收集
        *********/
        /// <summary>从游戏数据资源收集物品、地点与 NPC。</summary>
        private List<RawEntity> CollectEntities()
        {
            List<RawEntity> entities = new();

            // 物品：对象、大物件、武器、工具、裤子、衬衫、饰品、靴子、帽子、家具。
            Dictionary<string, ObjectData>? objects = this.TryLoadData<ObjectData>("Data/Objects");
            if (objects is not null)
            {
                foreach (KeyValuePair<string, ObjectData> entry in objects)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(O)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        GetFallback(entry.Value.Name, entry.Key)
                    ));
                }
            }

            Dictionary<string, BigCraftableData>? bigCraftables = this.TryLoadData<BigCraftableData>("Data/BigCraftables");
            if (bigCraftables is not null)
            {
                foreach (KeyValuePair<string, BigCraftableData> entry in bigCraftables)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(BC)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        GetFallback(entry.Value.Name, entry.Key)
                    ));
                }
            }

            Dictionary<string, WeaponData>? weapons = this.TryLoadData<WeaponData>("Data/Weapons");
            if (weapons is not null)
            {
                foreach (KeyValuePair<string, WeaponData> entry in weapons)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(W)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        GetFallback(entry.Value.Name, entry.Key)
                    ));
                }
            }

            Dictionary<string, ToolData>? tools = this.TryLoadData<ToolData>("Data/Tools");
            if (tools is not null)
            {
                foreach (KeyValuePair<string, ToolData> entry in tools)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(T)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        GetFallback(entry.Value.Name, entry.Key)
                    ));
                }
            }

            Dictionary<string, PantsData>? pants = this.TryLoadData<PantsData>("Data/Pants");
            if (pants is not null)
            {
                foreach (KeyValuePair<string, PantsData> entry in pants)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(P)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        GetFallback(entry.Value.Name, entry.Key)
                    ));
                }
            }

            Dictionary<string, ShirtData>? shirts = this.TryLoadData<ShirtData>("Data/Shirts");
            if (shirts is not null)
            {
                foreach (KeyValuePair<string, ShirtData> entry in shirts)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(S)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        GetFallback(entry.Value.Name, entry.Key)
                    ));
                }
            }

            Dictionary<string, TrinketData>? trinkets = this.TryLoadData<TrinketData>("Data/Trinkets");
            if (trinkets is not null)
            {
                foreach (KeyValuePair<string, TrinketData> entry in trinkets)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(TR)" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        entry.Key
                    ));
                }
            }

            // 靴子与帽子：显示名称在按语言本地化的数据资源的固定字段里。
            Dictionary<string, string>? boots = this.TryLoadData<string>("Data/Boots");
            if (boots is not null)
            {
                foreach (KeyValuePair<string, string> entry in boots)
                {
                    entities.Add(RawEntity.FromLegacy(
                        "Item",
                        "(B)" + entry.Key,
                        entry.Key,
                        "Data/Boots",
                        6,
                        GetField(entry.Value, 0, entry.Key)
                    ));
                }
            }

            Dictionary<string, string>? hats = this.TryLoadData<string>("Data/hats");
            if (hats is not null)
            {
                foreach (KeyValuePair<string, string> entry in hats)
                {
                    entities.Add(RawEntity.FromLegacy(
                        "Item",
                        "(H)" + entry.Key,
                        entry.Key,
                        "Data/hats",
                        5,
                        GetField(entry.Value, 0, entry.Key)
                    ));
                }
            }

            // 家具的显示名称同样位于固定字段，但它是可本地化的标记。
            Dictionary<string, string>? furniture = this.TryLoadData<string>("Data/Furniture");
            if (furniture is not null)
            {
                foreach (KeyValuePair<string, string> entry in furniture)
                {
                    entities.Add(RawEntity.FromToken(
                        "Item",
                        "(F)" + entry.Key,
                        entry.Key,
                        GetField(entry.Value, 7, ""),
                        GetField(entry.Value, 0, entry.Key)
                    ));
                }
            }

            // NPC：Data/Characters 的键是 NPC 的内部名称。
            Dictionary<string, CharacterData>? characters = this.TryLoadData<CharacterData>("Data/Characters");
            if (characters is not null)
            {
                foreach (KeyValuePair<string, CharacterData> entry in characters)
                {
                    entities.Add(RawEntity.FromToken(
                        "NPC",
                        "NPC:" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        entry.Key
                    ));
                }
            }

            // 地点：Data/Locations 的键是地点内部名称；Default 只是兜底模板，不是实际地点。
            Dictionary<string, LocationData>? locations = this.TryLoadData<LocationData>("Data/Locations");
            if (locations is not null)
            {
                foreach (KeyValuePair<string, LocationData> entry in locations)
                {
                    if (entry.Key == "Default")
                        continue;

                    entities.Add(RawEntity.FromToken(
                        "Location",
                        "LOCATION:" + entry.Key,
                        entry.Key,
                        entry.Value.DisplayName,
                        entry.Key
                    ));
                }
            }

            return entities;
        }

        /// <summary>加载游戏数据资源；加载失败时记录警告并跳过该分类。</summary>
        private Dictionary<string, T>? TryLoadData<T>(string assetName)
        {
            try
            {
                return this.Helper.GameContent.Load<Dictionary<string, T>>(assetName);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"无法加载 {assetName}，将跳过该分类：{ex.Message}", LogLevel.Warn);
                return null;
            }
        }


        /*********
        ** 名称解析
        *********/
        /// <summary>读取按语言本地化的数据资源中的显示名称。</summary>
        private string GetLegacyDisplayName(
            RawEntity entity,
            LanguageCode language,
            Dictionary<string, Dictionary<string, string>?> cache
        )
        {
            string assetName = entity.LegacyDataAsset!;
            string cacheKey = assetName + "|" + (int)language;

            if (!cache.TryGetValue(cacheKey, out Dictionary<string, string>? data))
            {
                data = this.LoadLegacyData(assetName, language);
                cache[cacheKey] = data;
            }

            // 该语言没有本地化数据时回退英文，而不是复用当前语言的数据。
            if (data is null && language != LanguageCode.en)
            {
                string englishCacheKey = assetName + "|" + (int)LanguageCode.en;
                if (!cache.TryGetValue(englishCacheKey, out data))
                {
                    data = this.LoadLegacyData(assetName, LanguageCode.en);
                    cache[englishCacheKey] = data;
                }
            }

            if (data is not null && data.TryGetValue(entity.Id, out string? raw))
            {
                string value = GetField(raw, entity.LegacyNameIndex, "");
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return entity.FallbackName;
        }

        /// <summary>加载按语言本地化的数据资源。</summary>
        private Dictionary<string, string>? LoadLegacyData(string assetName, LanguageCode language)
        {
            try
            {
                return LocalizedAssetLoader.Load<Dictionary<string, string>>(assetName, language);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"无法加载本地化数据 {assetName}（{language}）：{ex.Message}", LogLevel.Trace);
                return null;
            }
        }

        /// <summary>从以斜杠分隔的旧版数据值中读取指定字段。</summary>
        private static string GetField(string? raw, int index, string fallback)
        {
            if (string.IsNullOrEmpty(raw))
                return fallback;

            string[] fields = raw.Split('/');
            if (index >= 0 && index < fields.Length)
            {
                string value = fields[index].Trim();
                if (value.Length > 0)
                    return value;
            }

            return fallback;
        }

        /// <summary>名称缺失时回退到数据键。</summary>
        private static string GetFallback(string? name, string id)
        {
            return string.IsNullOrWhiteSpace(name) ? id : name!;
        }


        /*********
        ** 输出
        *********/
        /// <summary>构建 CSV 文本。</summary>
        private static string BuildCsv(IReadOnlyList<ExportLanguage> languages, IReadOnlyList<EntityRecord> records)
        {
            StringBuilder builder = new();

            builder.Append(CsvField("Key"));
            foreach (ExportLanguage language in languages)
                builder.Append(',').Append(CsvField(language.Column));
            builder.AppendLine();

            foreach (EntityRecord record in records)
            {
                builder.Append(CsvField(record.Key));
                foreach (ExportLanguage language in languages)
                {
                    record.DisplayNames.TryGetValue(language.Column, out string? value);
                    builder.Append(',').Append(CsvField(value ?? ""));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        /// <summary>转义一个 CSV 字段。</summary>
        private static string CsvField(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>检测游戏当前安装的语言。</summary>
        private static List<ExportLanguage> GetLanguages(IMonitor monitor)
        {
            List<ExportLanguage> languages = new()
            {
                new ExportLanguage(LanguageCode.en, "en")
            };

            string? contentRoot = null;
            try
            {
                contentRoot = Game1.content.GetContentRoot();
            }
            catch (Exception ex)
            {
                monitor.Log($"无法获取游戏内容目录，将尝试全部语言：{ex.Message}", LogLevel.Trace);
            }

            foreach (LanguageCode code in Enum.GetValues(typeof(LanguageCode)))
            {
                if (code is LanguageCode.en or LanguageCode.mod)
                    continue;

                string suffix;
                try
                {
                    suffix = LocalizedContentManager.LanguageCodeString(code);
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(suffix))
                    continue;

                // 通过翻译资源是否存在判断该语言是否随游戏安装。
                if (contentRoot is null || HasLocalizedStrings(contentRoot, suffix))
                    languages.Add(new ExportLanguage(code, suffix));
            }

            return languages;
        }

        /// <summary>判断指定语言的字符串资源是否存在。</summary>
        private static bool HasLocalizedStrings(string contentRoot, string suffix)
        {
            string stringsDirectory = Path.Combine(contentRoot, "Strings");
            return File.Exists(Path.Combine(stringsDirectory, "Objects." + suffix + ".xnb"))
                || File.Exists(Path.Combine(stringsDirectory, "Objects." + suffix + ".json"));
        }


        /*********
        ** 内部类型
        *********/
        /// <summary>JSON 导出文档。</summary>
        private sealed class ExportDocument
        {
            /// <summary>生成时间（UTC，ISO 8601）。</summary>
            public string GeneratedAtUtc { get; init; } = "";

            /// <summary>语言代码列表，顺序与 CSV 列一致。</summary>
            public IReadOnlyList<string> Languages { get; init; } = Array.Empty<string>();

            /// <summary>各实体类型的数量。</summary>
            public Dictionary<string, int> Counts { get; init; } = new();

            /// <summary>实体列表。</summary>
            public IReadOnlyList<EntityRecord> Entities { get; init; } = Array.Empty<EntityRecord>();
        }
    }
}
