# Changelog

## [Unreleased]

## [3.1.0] - 2026-11-14

- 接入 OpenUPM 官方 tag 发布工作流，自动触发扫描并检查注册表发布结果。

- 统一 Cowart 包版本为 3.1.0。

- 包内分组显式使用 Inspector Groups 框架样式；用户新增分组默认保持普通样式，所有绘制集中在 Inspector Groups。

- Inspector 改为声明字段、状态和操作回调，所有布局与控件由 Inspector Groups 统一绘制，移除包内 Inspector 手工绘制代码。

- 使用类级 HideScript 声明隐藏 Script 行，移除专用 Inspector 中写死的排除规则。

- 修复选中对象在脚本继承关系变更、重编译后旧 Inspector 继续访问失效字段的问题；统一验证目标类型并刷新字段绑定。

- 声明 Inspector Groups 3.1.0 依赖；移除包内 Inspector 自定义样式，业务操作复用统一绘制。 创建系统的物体名统一为 Flow。

- 按功能整理 Cowart 菜单分类与名称，菜单声明保留在所属包内。

- 新增 Hierarchy 右键菜单 `Cowart > 框架 > 创建 Flow 系统`，支持撤销并防止场景内重复创建。

## [1.0.3] - 2026-11-12

- 隐藏 FlowManager Inspector 的 Script 引用行，保留运行状态提示和实时调试信息。
- 编辑器实现保持包内独立，不引入 UI Framework 依赖。

## [1.0.2] - 2026-11-08

- 统一 Cowart 包图标：重新设计为按功能类别配色的圆角图标（状态节点 + 有向切换），同步更新 `Icon.png` 与 `Icon@2x.png`。

## [1.0.1] - 2026-08-23

- 新增包专用图标，并将包内组件脚本的默认图标统一替换为该图标。

## [1.0.0] - 2026-08-12

- 新增同步状态切换与 `Enter`、`Update`、`LateUpdate`、`FixedUpdate`、`Exit` 生命周期。
- 新增 `FlowContext` 当前状态、上一状态、状态历史和运行次数查询。
- 新增状态标识、显示名称、运行结果、备注、开始时间、结束时间和用时记录。
- 新增实际用时、有效用时、业务暂停、应用失焦和后台暂停处理。
- 新增状态切换事件、运行完成事件、历史清理和独立 JSON 快照。
- 新增最后状态结束入口和 `NullState` 占位状态。
- 新增 Unity Editor 只读运行调试窗口。
- 新增 FlowManager Inspector 当前步骤实时信息。
- 当前步骤实时信息新增完整状态类名显示。
- Flow Debugger 入口统一到 `Cowart > 项目开发工具` 菜单。
- 新增 EditMode 自动化测试和可交互的五步仿真实验流程示例。
