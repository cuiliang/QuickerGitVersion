# QuickerGitVersion

一个简单的 GitVersion 命令行工具实现，使用 C# 编写。本工具可以从 Git 仓库信息中计算版本号，并生成多种格式的版本文件。

## 功能特性

- ✅ 自动从 Git 历史记录计算版本信息
- ✅ 支持语义版本控制（SemVer）
- ✅ 生成三种格式的版本文件：
  - `gitversion.json` - JSON 格式
  - `gitversion.props` - MSBuild 属性文件
  - `gitversion.properties` - Java 属性文件
- ✅ 支持预发布版本标签
- ✅ 处理分支名称转义
- ✅ 计算提交数量和构建元数据

## 使用方法

### 基本用法

在 Git 仓库目录中运行：

```bash
QuickerGitVersion.exe
```

这将在当前目录生成三个版本文件。

### 命令行选项

```bash
QuickerGitVersion.exe [选项]

选项:
  -o, --output <目录>      输出目录（默认为当前目录）
  -v, --verbose           详细输出
  --json-only             只生成JSON文件
  --props-only            只生成Props文件
  --properties-only       只生成Properties文件
  -h, --help             显示帮助信息
```

### 使用示例

```bash
# 生成所有格式文件到当前目录
QuickerGitVersion.exe

# 生成文件到指定目录
QuickerGitVersion.exe -o ./build

# 只生成JSON文件，显示详细信息
QuickerGitVersion.exe --json-only --verbose

# 生成到特定目录并显示详细输出
QuickerGitVersion.exe -o ./artifacts -v
```

## 输出文件格式

### gitversion.json

```json
{
  "AssemblySemFileVer": "1.100.1.0",
  "AssemblySemVer": "1.100.1.0",
  "BranchName": "v2/try1",
  "BuildMetaData": null,
  "CommitDate": "2025-06-11",
  "CommitsSinceVersionSource": 9,
  "EscapedBranchName": "v2-try1",
  "FullBuildMetaData": "Branch.v2-try1.Sha.7a1c4caa664beb914c154d1aede48b078989e635",
  "FullSemVer": "1.100.1-{BranchName}.9",
  "InformationalVersion": "1.100.1-{BranchName}.9+Branch.v2-try1.Sha.7a1c4caa664beb914c154d1aede48b078989e635",
  "Major": 1,
  "MajorMinorPatch": "1.100.1",
  "Minor": 100,
  "Patch": 1,
  "PreReleaseLabel": "{BranchName}",
  "PreReleaseLabelWithDash": "-{BranchName}",
  "PreReleaseNumber": 9,
  "PreReleaseTag": "{BranchName}.9",
  "PreReleaseTagWithDash": "-{BranchName}.9",
  "SemVer": "1.100.1-{BranchName}.9",
  "Sha": "7a1c4caa664beb914c154d1aede48b078989e635",
  "ShortSha": "7a1c4ca",
  "UncommittedChanges": 4,
  "VersionSourceSha": "ac90b36bd5b1d517b4b8c7572b198225e8cc18cf",
  "WeightedPreReleaseNumber": 9
}
```

### gitversion.props

```xml
<Project>
  <PropertyGroup>
    <GitVersion_AssemblySemFileVer>1.100.1.0</GitVersion_AssemblySemFileVer>
    <GitVersion_AssemblySemVer>1.100.1.0</GitVersion_AssemblySemVer>
    <GitVersion_BranchName>v2/try1</GitVersion_BranchName>
    <!-- 其他属性... -->
  </PropertyGroup>
</Project>
```

### gitversion.properties

```properties
GitVersion.AssemblySemFileVer=1.100.1.0
GitVersion.AssemblySemVer=1.100.1.0
GitVersion.BranchName=v2/try1
# 其他属性...
```

## 构建和开发

### 环境要求

- .NET 8.0 SDK
- Git (用于运行时)

### 构建项目

```bash
# 还原依赖包
dotnet restore

# 构建项目
dotnet build

# 运行测试
dotnet test

# 发布可执行文件
dotnet publish -c Release -o ./publish
```

### 开发和测试

```bash
# 运行主项目
dotnet run --project src/QuickerGitVersion

# 运行测试
dotnet test src/QuickerGitVersion.Tests

# 监听文件变化并自动重新构建
dotnet watch --project src/QuickerGitVersion
```

## 技术栈

- **框架**: .NET 8.0
- **Git 操作**: LibGit2Sharp
- **命令行解析**: System.CommandLine
- **JSON 序列化**: System.Text.Json
- **日志记录**: Microsoft.Extensions.Logging
- **测试框架**: xUnit, FluentAssertions

## 项目结构

```
QuickerGitVersion/
├── src/
│   ├── QuickerGitVersion/           # 主项目
│   │   ├── Models/                  # 数据模型
│   │   ├── Services/                # 业务服务
│   │   ├── Utils/                   # 工具类
│   │   └── Program.cs               # 程序入口
│   └── QuickerGitVersion.Tests/     # 单元测试
├── .cursor/                         # Cursor IDE 规则
├── README.md                        # 项目说明
└── QuickerGitVersion.sln           # 解决方案文件
```

## 版本计算逻辑

1. **基础版本**: 从最新的版本标签解析（当前简化为 1.100.1）
2. **分支处理**: 
   - `main`/`master` 分支：生成稳定版本
   - 其他分支：生成预发布版本，包含分支名和提交数
3. **构建元数据**: 包含分支名、提交 SHA 等信息

## 许可证

MIT License

## 贡献指南

欢迎提交 Issue 和 Pull Request！请确保：

1. 遵循现有的代码风格
2. 添加适当的单元测试
3. 更新相关文档

## 已知限制

- 当前版本计算逻辑较为简化，实际 GitVersion 有更复杂的配置和规则
- 版本标签解析目前支持基本的 `v1.0.0` 格式
- 不支持自定义版本计算配置文件

## 路线图

- [ ] 支持 GitVersion 配置文件 (gitversion.yml)
- [ ] 更复杂的版本计算规则
- [ ] 支持更多输出格式
- [ ] 性能优化（大型仓库）
- [ ] Docker 容器支持 