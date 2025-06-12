# QuickerGitVersion Global Tool 使用指南

## 🚀 什么是 .NET Global Tool？

.NET Global Tool 是可以从命令行任何位置调用的控制台应用程序。它通过 NuGet 分发，可以像 `git` 或 `dotnet` 命令一样全局使用。

## 📦 打包为 Global Tool

### 1. 项目配置

在 `QuickerGitVersion.csproj` 中添加以下配置：

```xml
<PropertyGroup>
  <!-- Global Tool Configuration -->
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>quickergitversion</ToolCommandName>
  <PackageId>QuickerGitVersion</PackageId>
  <PackageVersion>1.0.0</PackageVersion>
  <Authors>cuiliang</Authors>
  <Description>A fast and lightweight GitVersion alternative for .NET projects</Description>
  <PackageTags>git;version;semantic;versioning;gitversion;tool</PackageTags>
  <PackageProjectUrl>https://github.com/cuiliang/QuickerGitVersion</PackageProjectUrl>
  <RepositoryUrl>https://github.com/cuiliang/QuickerGitVersion</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
</PropertyGroup>
```

### 2. 重要配置说明

- `PackAsTool`: 告诉 .NET 这是一个工具包
- `ToolCommandName`: 安装后的命令名称（用户输入的命令）
- `PackageId`: NuGet 包的唯一标识符
- **不能使用** `PublishSingleFile=true`（与 Global Tool 不兼容）

## 🔧 构建和安装

### 使用提供的脚本

```powershell
# 仅构建和打包
./build-tool.ps1 -Pack

# 构建、打包并本地安装
./build-tool.ps1 -Pack -Install

# 指定版本号
./build-tool.ps1 -Version "1.2.0" -Pack -Install

# 卸载现有版本
./build-tool.ps1 -Uninstall
```

### 手动命令

```bash
# 1. 打包
dotnet pack src/QuickerGitVersion/QuickerGitVersion.csproj -c Release -o output

# 2. 本地安装
dotnet tool install -g QuickerGitVersion --add-source ./output --version 1.0.0

# 3. 卸载
dotnet tool uninstall -g QuickerGitVersion

# 4. 列出已安装的工具
dotnet tool list -g
```

## 📤 发布到 NuGet.org

### 1. 获取 API Key

1. 登录 [NuGet.org](https://www.nuget.org)
2. 转到 Account Settings > API Keys
3. 创建新的 API Key，给予 "Push" 权限

### 2. 发布包

```powershell
# 使用脚本发布
./build-tool.ps1 -Pack -Push -NuGetApiKey "YOUR_API_KEY"

# 或手动发布
dotnet nuget push output/QuickerGitVersion.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## 🌐 用户安装和使用

### 从 NuGet.org 安装

```bash
# 安装
dotnet tool install -g QuickerGitVersion

# 使用
quickergitversion --help
quickergitversion
quickergitversion --json-only
quickergitversion -o ./output

# 更新
dotnet tool update -g QuickerGitVersion

# 卸载
dotnet tool uninstall -g QuickerGitVersion
```

### 从本地包安装（开发测试）

```bash
dotnet tool install -g QuickerGitVersion --add-source ./output --version 1.0.0
```

## 🔍 验证安装

```bash
# 检查工具是否安装
dotnet tool list -g

# 测试工具
quickergitversion --version
quickergitversion --help
```

## 📝 最佳实践

### 1. 版本管理

- 使用语义化版本 (SemVer)：`1.0.0`, `1.1.0`, `2.0.0`
- 每次发布前更新 `PackageVersion`
- 考虑使用预发布版本：`1.0.0-beta1`, `1.0.0-rc1`

### 2. 包信息

- 提供清晰的 `Description`
- 添加相关的 `PackageTags`
- 包含 `PackageProjectUrl` 和 `RepositoryUrl`
- 指定 `PackageLicenseExpression`

### 3. 测试

- 在发布前本地测试工具
- 验证所有命令行参数
- 测试不同操作系统的兼容性

## 🛠️ 故障排除

### 常见问题

1. **"PublishSingleFile 错误"**
   - 解决：移除或注释 `<PublishSingleFile>true</PublishSingleFile>`

2. **"工具已存在"**
   - 解决：先卸载 `dotnet tool uninstall -g QuickerGitVersion`

3. **"包不存在"**
   - 解决：检查包名拼写和版本号

4. **"权限错误"**
   - 解决：确保有管理员权限或使用 `--tool-path` 指定路径

### 调试技巧

```bash
# 查看详细错误信息
dotnet tool install -g QuickerGitVersion --verbosity diagnostic

# 列出所有工具安装位置
dotnet tool list -g

# 清理 NuGet 缓存
dotnet nuget locals all --clear
```

## 🎯 示例：GitVersion 风格的使用

```bash
# 基本使用
quickergitversion

# 指定输出目录
quickergitversion -o ./build

# 仅生成 JSON 文件
quickergitversion --json-only

# 详细输出
quickergitversion --verbose

# 显示版本
quickergitversion --version
```

这样，用户就可以像使用 `gitversion` 一样使用 `quickergitversion` 了！ 