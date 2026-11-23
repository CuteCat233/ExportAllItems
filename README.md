# Export All Items

> 内部项目名和 UniqueID 仍为 `ListAllItem`；程序集/DLL 名称为 `ExportAllItems`，以保持配置与升级路径兼容。

一个 Stardew Valley 1.6 / SMAPI 模组：把游戏标准数据资源中**所有带 DisplayName 的短名称条目**（物品、地点、NPC、电影院零食、增益、能力、农场动物、宠物、人体模型、矿车目的地、怪物消灭目标、被动节日）逐个列出来，并导出每个条目的**多语言显示名称（DisplayName）**。

不包含 Description、对话、事件、邮件、Tooltips 等长文本，只导出实体的键与显示名称。

## 功能

- 支持以下实体：
  - 物品：对象 `(O)`、大物件 `(BC)`、武器 `(W)`、工具 `(T)`、裤子 `(P)`、衬衫 `(S)`、饰品 `(TR)`、靴子 `(B)`、帽子 `(H)`、家具 `(F)`
  - 地点：`Data/Locations` 中的地点
  - NPC：`Data/Characters` 中的角色
  - 其他 DisplayName 数据：电影院零食 `CONCESSION:`、增益 `BUFF:`、能力 `POWER:`、农场动物 `FARMANIMAL:`、宠物 `PET:`、人体模型 `MANNEQUIN:`、矿车目的地 `MINECART:`、怪物消灭目标 `SLAYERQUEST:`、被动节日 `PASSIVEFESTIVAL:`
- 自动检测游戏已安装的语言（英文、中文、日文、德文等），导出所有语言的 DisplayName。
- 可选支持 Generic Mod Config Menu（GMCM），在游戏内修改导出快捷键和语言范围；GMCM 设置项已接入 i18n，包含英文及游戏随附的 11 种本地化语言。
- 同时导出：
  - **JSON**：按实体组织，包含 `displayNames` 多语言映射
  - **CSV**：`Key,语言1,语言2,...`，UTF-8 with BOM，方便 Excel 打开

## 构建

需要能构建 `net6.0` 目标的 .NET SDK（本机使用 .NET 10 SDK 构建通过）、Stardew Valley 1.6 和 SMAPI 4.x。项目使用 `Pathoschild.Stardew.ModBuildConfig` 自动查找游戏目录（本机为 `D:\SteamLibrary\steamapps\common\Stardew Valley`）。

```powershell
dotnet build ListAllItem.sln
```

构建成功后会：

- 生成 `ListAllItem/bin/Debug/net6.0/ExportAllItems.dll`
- 自动部署到 `Stardew Valley/Mods/ListAllItem/`
- 在 `bin/Debug/net6.0/` 下生成发布用 zip

## 使用

1. 启动游戏并**进入一个存档**（部分文本参数需要当前存档的玩家信息）。
2. 按 `F8`，或在 SMAPI 控制台输入：

```text
lia_export
```

3. 导出文件位于模组目录：

```text
Stardew Valley/Mods/ListAllItem/exports/entities.json
Stardew Valley/Mods/ListAllItem/exports/entities.csv
```

每次导出都会覆盖这两个文件。

## 配置

首次运行后会在模组目录生成 `config.json`：

```json
{
  "ExportKey": "F8",
  "ExportAllLanguages": true
}
```

- `ExportKey`：触发导出的按键，默认为 `F8`；设为 `None` 可禁用快捷键（控制台指令仍然可用）。
- `ExportAllLanguages`：默认为 `true`，导出所有已安装语言；设为 `false` 时只导出英文和当前游戏语言。

## 游戏内设置（GMCM）

[Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) 是**可选依赖**：

- 安装后，可以在游戏内修改 `ExportKey` 和 `ExportAllLanguages`。
- GMCM 的选项名称和提示会跟随游戏语言，翻译文件位于 `i18n/`：默认英文，以及 `de`、`es`、`fr`、`hu`、`it`、`ja`、`ko`、`pt`、`ru`、`tr`、`zh`。
- 未安装、未加载或 GMCM API 不可用时，模组仍会正常加载和导出，设置通过 `config.json` 修改。
- GMCM 注册失败只会记录一条警告，不影响导出功能。

## 输出格式

### JSON

```json
{
  "generatedAtUtc": "2026-11-23T06:30:00.0000000Z",
  "languages": ["en", "zh-CN", "de-DE"],
  "counts": {
    "Item": 2207,
    "Location": 90,
    "NPC": 48,
    "Concession": 24,
    "Buff": 29,
    "Power": 36,
    "FarmAnimal": 14,
    "Pet": 3,
    "Mannequin": 4,
    "MinecartDestination": 4,
    "MonsterSlayerQuest": 12,
    "PassiveFestival": 4
  },
  "entities": [
    {
      "type": "Item",
      "key": "(O)16",
      "id": "16",
      "displayNames": {
        "en": "Wild Horseradish",
        "zh-CN": "野山葵",
        "de-DE": "Wilder Meerrettich"
      }
    },
    {
      "type": "NPC",
      "key": "NPC:Abigail",
      "id": "Abigail",
      "displayNames": {
        "en": "Abigail",
        "zh-CN": "阿比盖尔"
      }
    },
    {
      "type": "Location",
      "key": "LOCATION:Town",
      "id": "Town",
      "displayNames": {
        "en": "Pelican Town",
        "zh-CN": "鹈鹕镇"
      }
    },
    {
      "type": "Concession",
      "key": "CONCESSION:0",
      "id": "0",
      "displayNames": {
        "en": "Cotton Candy",
        "zh-CN": "棉花糖"
      }
    }
  ]
}
```

字段说明：

- `type`：`Item` / `Location` / `NPC` / `Concession` / `Buff` / `Power` / `FarmAnimal` / `Pet` / `Mannequin` / `MinecartDestination` / `MonsterSlayerQuest` / `PassiveFestival`
- `key`：导出用的唯一键。物品为限定物品 ID；地点与 NPC 分别带 `LOCATION:`、`NPC:` 前缀；其他类型带 `CONCESSION:`、`BUFF:`、`POWER:`、`FARMANIMAL:`、`PET:`、`MANNEQUIN:`、`SLAYERQUEST:`、`PASSIVEFESTIVAL:` 前缀；矿车目的地为 `MINECART:<network>/<id>`
- `id`：游戏数据资源中的原始键（物品数字 ID、地点内部名、NPC 内部名，矿车目的地为目的地 ID）
- `displayNames`：语言代码 → 该语言下的 DisplayName

### CSV

```csv
"Key","en","zh-CN"
"(O)16","Wild Horseradish","野山葵"
"NPC:Abigail","Abigail","阿比盖尔"
"LOCATION:Town","Pelican Town","鹈鹕镇"
"CONCESSION:0","Cotton Candy","棉花糖"
```

- 第一列为 `Key`，之后每个已安装语言一列
- 所有字段都按 CSV 规则加引号并转义
- 文件带 UTF-8 BOM，便于 Excel 正确识别中文

## 语言检测

模组会检查游戏 `Content/Strings/Objects.<语言代码>.xnb` 是否存在，以此判断该语言是否随游戏安装。英文始终包含。缺少某项翻译时会自动回退到英文。

导出时会直接按 `资源名.语言代码` 加载对应语言的资源（例如 `Strings/Objects.ja-JP`），而不是调用会复用缓存的 `Load(asset, language)`，避免所有语言都返回当前游戏语言。

## 限制

- 只导出 DisplayName；不导出 Description、对话、事件、邮件、Tooltips 等文本。
- 只覆盖标准 GameData 资源中“类型带 DisplayName 字段”的条目；仅有 Name/Title 的数据（如建筑、任务、电影标题）不导出。
- 矿车目的地取自 `Data/Minecarts` 中每个网络的 `Destinations` 嵌套列表。
- 多语言名称来自游戏自带的本地化资源；模组自定义语言（`LanguageCode.mod`）不在自动检测范围内。
- 若其他模组添加了新的 `[LocalizedText]` 资源但未提供对应语言的翻译，回退英文。
- 必须在进入存档后导出；标题界面无法解析依赖当前玩家的文本（例如农场名）。

## 验证说明

本次开发完成了编译验证，并离线校验了新增资源的 DisplayName 标记在英文/中文资源中存在：

```text
dotnet build ListAllItem.sln
已成功生成。0 个警告，0 个错误。
```

未启动游戏进行实际导出测试。

## 许可证

本项目采用 [MIT License](LICENSE)，版权所有 (c) 2026 CuteCat233。
