# Export All Items

一个 Stardew Valley 1.6 / SMAPI 模组：把游戏标准数据中带 `DisplayName` 的实体导出为 JSON / CSV，并支持多语言名称、按类型筛选和分文件夹输出。

当前版本：**1.5.0**

## 功能

- 导出物品、地点、NPC，以及其他带 `DisplayName` 的数据：电影院零食、增益、能力、农场动物、宠物、人体模型、矿车目的地、怪物消灭目标、被动节日。
- 每个条目导出所有已安装语言的显示名称；缺少翻译时回退英文。
- 支持 JSON 和 CSV 两种格式。
- 可以只导出勾选的类型。
- 可以选择按类型拆分导出，每种类型写入独立子文件夹；某一类型失败不会影响其他类型。
- 可选支持 GMCM，在游戏内修改快捷键、语言范围、导出类型和拆分开关。
- 导出成功、失败或部分失败时，通过游戏自带 HUD 提醒；失败详情写入 SMAPI 控制台。
- 不导出 Description、对话、事件、邮件、Tooltips 等长文本。

## 构建与安装

需要能构建 `net6.0` 的 .NET SDK、Stardew Valley 1.6 和 SMAPI 4.x。

```powershell
dotnet build ExportAllItems.sln
```

构建后会自动部署到：

```text
Stardew Valley/Mods/ExportAllItems/
```

发布用 zip 位于 `ExportAllItems/bin/Debug/net6.0/`。

## 使用

1. 启动游戏并进入存档。
2. 按 `F8`，或在 SMAPI 控制台执行：

```text
lia_export
```

快捷键只在玩家可自由行动且没有菜单打开时生效；在暂停菜单、GMCM 改键界面等菜单中按快捷键不会触发导出。

3. 默认写入合并文件：

```text
exports/entities.json
exports/entities.csv
```

开启按类型拆分后，改为：

```text
exports/item/entities.json
exports/locations/entities.json
exports/npcs/entities.json
...
```

每种类型目录内都有自己的 `entities.json` 和 `entities.csv`。

### HUD 提醒

- 全部成功：`导出成功`
- 全部失败：`导出失败，详细信息请查看命令行窗口`
- 拆分模式部分失败：`部分导出失败，详细信息查看请命令行窗口`

提醒文案会跟随游戏语言。

## 设置

可以通过 `config.json` 或 GMCM 修改：

| 设置 | 默认值 | 说明 |
|---|---|---|
| `ExportKey` | `F8` | 导出快捷键；`None` 表示禁用 |
| `ExportAllLanguages` | `true` | 关闭时只导出英文和当前游戏语言 |
| `SplitByType` | `false` | 按类型拆分到独立子文件夹 |
| `ExportItems` | `true` | 导出物品 |
| `ExportLocations` | `true` | 导出地点 |
| `ExportNpcs` | `true` | 导出 NPC |
| `ExportConcessions` | `true` | 导出电影院零食 |
| `ExportBuffs` | `true` | 导出增益 |
| `ExportPowers` | `true` | 导出能力 |
| `ExportFarmAnimals` | `true` | 导出农场动物 |
| `ExportPets` | `true` | 导出宠物 |
| `ExportMannequins` | `true` | 导出人体模型 |
| `ExportMinecartDestinations` | `true` | 导出矿车目的地 |
| `ExportMonsterSlayerQuests` | `true` | 导出怪物消灭目标 |
| `ExportPassiveFestivals` | `true` | 导出被动节日 |

至少选择一个导出类型；全部关闭时不会执行导出。

GMCM 是可选依赖：未安装时模组仍可正常使用，设置通过 `config.json` 修改。GMCM 文案已提供英文及游戏随附的 11 种本地化语言。

## 限制

- 只处理标准数据资源中带 `DisplayName` 字段的条目。
- 不包含 Description、对话、事件、邮件、Tooltips 等文本。
- 多语言名称来自游戏自带的本地化资源；模组自定义语言不在自动检测范围内。
- 必须在进入存档后导出。

## 许可证

本项目采用 [MIT License](LICENSE)。
