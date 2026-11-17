# CZCIT Glass UI

**BeiWuCN · Unity 2022.3 · Built-in Render Pipeline**

C# 组件用于搭建世界空间磨砂玻璃 UI，支持平面布局和可选的 XR 曲面交互。

- [下载 26.11.17](https://github.com/BeiWuCN/CZCIT.GlassUI/releases/tag/26.11.17)
- [图文教程和 C# 示例](Assets/CZCIT%20GlassUI/README.md)
- [Tabler Icons 第三方许可](ThirdParty/TablerIcons.txt)

在 Unity 中选择 **Assets → Import Package → Custom Package**。包会导入到 `Assets/CZCIT GlassUI/`。核心使用 uGUI，XR 扩展使用 XR Interaction Toolkit 2.6.4。磨砂玻璃效果由随包 Shader 实现。

## 效果预览

下列截图取自 2026-11-14 的实际运行录屏，展示同一套玻璃组件如何用于不同的菜单布局。可在空白场景中按教程搭建自己的面板。

![曲面磨砂玻璃主菜单，包含玻璃背景、文字与按钮](Docs/Previews/2026-11-14-curved-menu.png)

**曲面主菜单**。背景、按钮与文字共享同一画布曲率。

![月面展台上的玻璃科普面板](Docs/Previews/2026-11-14-lunar-panel.png)

**展台科普面板**。同一套玻璃样式也可用于靠近展品时阅读的世界空间界面。
