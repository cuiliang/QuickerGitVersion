#!/usr/bin/env pwsh
# QuickerGitVersion Build Script

param(
    [switch]$Pack,
    [switch]$Install,
    [switch]$Uninstall,
    [switch]$Push,
    [string]$NuGetApiKey = ""
)

$ErrorActionPreference = "Stop"

# 从 gitversion.props 读取版本信息
Write-Host "📝 Reading version from gitversion.props..." -ForegroundColor Cyan
$GitVersionPropsFile = "gitversion.props"
if (-not (Test-Path $GitVersionPropsFile)) {
    Write-Error "$GitVersionPropsFile not found. Please run QuickerGitVersion tool first to generate it."
    exit 1
}
[xml]$gitversionProps = Get-Content $GitVersionPropsFile
$Version = $gitversionProps.Project.PropertyGroup.GitVersion_FullSemVer
if (-not $Version) {
    Write-Error "Could not read version from $GitVersionPropsFile"
    exit 1
}

# 项目路径
$ProjectPath = "src/QuickerGitVersion"
$ProjectFile = "$ProjectPath/QuickerGitVersion.csproj"

Write-Host "🔧 QuickerGitVersion Build Script" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Yellow

# 清理输出目录
if (Test-Path "output") {
    Remove-Item "output" -Recurse -Force
}
New-Item -ItemType Directory -Path "output" -Force | Out-Null

# 设置版本号
Write-Host "📝 Setting version to $Version..." -ForegroundColor Cyan
$content = Get-Content $ProjectFile -Raw
$content = $content -replace '<PackageVersion>[^<]*</PackageVersion>', "<PackageVersion>$Version</PackageVersion>"
$content = $content -replace '<AssemblyVersion>[^<]*</AssemblyVersion>', "<AssemblyVersion>$Version.0</AssemblyVersion>"
$content = $content -replace '<FileVersion>[^<]*</FileVersion>', "<FileVersion>$Version.0</FileVersion>"
Set-Content -Path $ProjectFile -Value $content

# 构建项目
Write-Host "🔨 Building project..." -ForegroundColor Cyan
dotnet build $ProjectFile -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

# 打包为 NuGet 包
if ($Pack) {
    Write-Host "📦 Packing as NuGet tool..." -ForegroundColor Cyan
    dotnet pack $ProjectFile -c Release -o output --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Pack failed!"
        exit 1
    }
    
    $packagePath = Get-ChildItem "output/*.nupkg" | Select-Object -First 1
    Write-Host "✅ Package created: $($packagePath.Name)" -ForegroundColor Green
}

# 卸载旧版本
if ($Uninstall) {
    Write-Host "🗑️ Uninstalling previous version..." -ForegroundColor Cyan
    dotnet tool uninstall -g QuickerGitVersion 2>$null
    Write-Host "✅ Uninstalled (if existed)" -ForegroundColor Green
}

# 安装全局工具
if ($Install) {
    Write-Host "🔧 Installing as global tool..." -ForegroundColor Cyan
    
    # 首先尝试卸载
    dotnet tool uninstall -g QuickerGitVersion 2>$null
    
    # 从本地包安装
    $packagePath = Get-ChildItem "output/*.nupkg" | Select-Object -First 1
    if ($packagePath) {
        dotnet tool install -g QuickerGitVersion --add-source output --version $Version
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Global tool installed successfully!" -ForegroundColor Green
            Write-Host "💡 You can now use: quickergitversion" -ForegroundColor Yellow
        } else {
            Write-Error "Installation failed!"
            exit 1
        }
    } else {
        Write-Error "No package found! Run with -Pack first."
        exit 1
    }
}

# 推送到 NuGet
if ($Push) {
    if (-not $NuGetApiKey) {
        Write-Error "NuGet API Key required for push. Use -NuGetApiKey parameter."
        exit 1
    }
    
    Write-Host "🚀 Pushing to NuGet.org..." -ForegroundColor Cyan
    $packagePath = Get-ChildItem "output/*.nupkg" | Select-Object -First 1
    if ($packagePath) {
        dotnet nuget push $packagePath.FullName --api-key $NuGetApiKey --source https://api.nuget.org/v3/index.json
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Package pushed to NuGet.org successfully!" -ForegroundColor Green
        } else {
            Write-Error "Push failed!"
            exit 1
        }
    } else {
        Write-Error "No package found! Run with -Pack first."
        exit 1
    }
}

Write-Host "🎉 Build script completed!" -ForegroundColor Green 