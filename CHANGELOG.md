# Changelog

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
