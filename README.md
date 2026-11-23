# ListAllItem

一个 Stardew Valley 1.6 / SMAPI 模组：把游戏数据中的**物品、地点、NPC** 逐个列出来，并导出每个条目的**多语言显示名称（DisplayName）**。

不包含对话、事件、邮件等内容，只导出实体的键与显示名称。

## 功能

- 支持以下实体：
  - 物品：对象 `(O)`、大物件 `(BC)`、武器 `(W)`、工具 `(T)`、裤子 `(P)`、衬衫 `(S)`、饰品 `(TR)`、靴子 `(B)`、帽子 `(H)`、家具 `(F)`
  - 地点：`Data/Locations` 中的地点
  - NPC：`Data/Characters` 中的角色
- 自动检测游戏已安装的语言（英文、中文、日文、德文等），导出所有语言的 DisplayName。
- 同时导出：
  - **JSON**：按实体组织，包含 `displayNames` 多语言映射
  - **CSV**：`Key,语言1,语言2,...`，UTF-8 with BOM，方便 Excel 打开

## 构建

需要能构建 `net6.0` 目标的 .NET SDK（本机使用 .NET 10 SDK 构建通过）、Stardew Valley 1.6 和 SMAPI 4.x。项目使用 `Pathoschild.Stardew.ModBuildConfig` 自动查找游戏目录（本机为 `D:\SteamLibrary\steamapps\common\Stardew Valley`）。

```powershell
dotnet build ListAllItem.sln
```

构建成功后会：

- 生成 `ListAllItem/bin/Debug/net6.0/ListAllItem.dll`
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
  "ExportKey": "F8"
}
```

- `ExportKey`：触发导出的按键，默认为 `F8`；设为空字符串可禁用快捷键（控制台指令仍然可用）。

## 输出格式

### JSON

```json
{
  "generatedAtUtc": "2026-11-23T06:30:00.0000000Z",
  "languages": ["en", "zh-CN", "de-DE"],
  "counts": {
    "Item": 2207,
    "Location": 90,
    "NPC": 48
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
    }
  ]
}
```

字段说明：

- `type`：`Item` / `Location` / `NPC`
- `key`：导出用的唯一键。物品为限定物品 ID；地点与 NPC 分别带 `LOCATION:`、`NPC:` 前缀，避免 CSV 中重名
- `id`：游戏数据资源中的原始键（物品数字 ID、地点内部名、NPC 内部名）
- `displayNames`：语言代码 → 该语言下的 DisplayName

### CSV

```csv
"Key","en","zh-CN"
"(O)16","Wild Horseradish","野山葵"
"NPC:Abigail","Abigail","阿比盖尔"
"LOCATION:Town","Pelican Town","鹈鹕镇"
```

- 第一列为 `Key`，之后每个已安装语言一列
- 所有字段都按 CSV 规则加引号并转义
- 文件带 UTF-8 BOM，便于 Excel 正确识别中文

## 语言检测

模组会检查游戏 `Content/Strings/Objects.<语言代码>.xnb` 是否存在，以此判断该语言是否随游戏安装。英文始终包含。缺少某项翻译时会自动回退到英文。

## 限制

- 只导出显示名称（DisplayName），不导出描述、对话、事件、邮件等文本。
- 多语言名称来自游戏自带的本地化资源；模组自定义语言（`LanguageCode.mod`）不在自动检测范围内。
- 若其他模组添加了新的 `[LocalizedText]` 资源但未提供对应语言的翻译，回退英文。
- 必须在进入存档后导出；标题界面无法解析依赖当前玩家的文本（例如农场名）。

## 验证说明

本次开发完成了编译验证：

```text
dotnet build ListAllItem.sln
已成功生成。0 个警告，0 个错误。
```

未启动游戏进行实际导出测试。
