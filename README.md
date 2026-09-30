# 一键启动器

一个简单的 Windows 桌面工具，用于按顺序启动自己选择的多个 `.exe` 程序。

项目采用 C#、.NET 8、WPF 和 MVVM，支持 Windows 10 / Windows 11。

## 下载使用

打开 GitHub 仓库的 **Releases** 页面，下载最新版本中的 `OneClickLauncher-win-x64.zip`，解压后双击：

```text
OneClickLauncher.exe
```

这是 Windows x64 自包含单文件版本，普通 Windows 10 / Windows 11 电脑通常不需要另外安装 .NET。

## 使用教程

### 添加启动程序

1. 打开软件。
2. 点击 `添加启动项`，或者首次打开时点击 `添加第一个启动程序`。
3. 在 Windows 文件选择窗口中选择一个 `.exe` 文件。
4. 选择成功后会自动增加一个编号，配置立即保存。

取消文件选择不会增加空白启动项。

### 修改某个编号的程序

在对应编号那一行点击 `选择程序`，选择新的 `.exe` 文件即可。原路径会自动替换并保存。

### 设置延迟和启用状态

- `延迟` 的单位是秒，默认值为 1。
- 取消 `启用` 后，一键启动时会跳过该项目，但路径不会被删除。
- 修改延迟或启用状态后会自动保存。

### 调整顺序

点击对应行的 `↑` 或 `↓`。编号会按照当前顺序自动重新生成，启动顺序也随之改变。

### 删除启动项

点击对应行的 `删除`，确认后即可删除。删除后后面的项目会自动重新编号。

### 一键启动

点击底部的 `▶ 一键启动`：

- 只启动已启用的项目；
- 按编号从小到大启动；
- 按每个项目设置的延迟等待；
- 启动过程中按钮会暂时禁用；
- 路径不存在或某个程序启动失败时，会记录状态并继续处理后面的项目；
- 关闭一键启动器不会关闭已经启动的其他程序。

### 打开程序所在位置

点击 `打开位置` 会打开资源管理器，并尽量选中对应的 `.exe` 文件。

## 配置保存位置

配置保存在当前 Windows 用户的本地应用数据目录：

```text
%LocalAppData%\\OneClickLauncher\\config.json
```

通常对应：

```text
C:\\Users\\你的用户名\\AppData\\Local\\OneClickLauncher\\config.json
```

普通使用不需要打开或修改这个文件。

## 从源码编译

需要安装 .NET 8 SDK 和 Windows 桌面开发组件。

在项目目录执行：

```powershell
dotnet build --configuration Release
```

生成 Windows x64 自包含单文件：

```powershell
dotnet publish --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

输出目录：

```text
bin\\Release\\net8.0-windows\\win-x64\\publish\\
```

## 项目结构

```text
Models/       启动项和配置数据模型
Views/        WPF 界面
ViewModels/   界面状态和命令
Services/     配置保存、文件选择、程序启动、提示框
Helpers/      MVVM 通用辅助类
```

## GitHub Actions 发布

`.github/workflows/build-release.yml` 会：

- 在推送到 `main` 时构建并上传构建产物；
- 在推送 `v*` 标签时构建 `OneClickLauncher-win-x64.zip`；
- 自动创建 GitHub Release，并把 ZIP 附加到 Release。

发布新版本示例：

```powershell
git tag v1.0.0
git push origin v1.0.0
```

## 许可证

本项目使用 MIT License，详见 [LICENSE](LICENSE)。
