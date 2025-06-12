using System.CommandLine;
using Microsoft.Extensions.Logging;
using QuickerGitVersion.Services;

namespace QuickerGitVersion;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "输出目录",
            getDefaultValue: () => Directory.GetCurrentDirectory());
        
        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "详细输出");
        
        var jsonOnlyOption = new Option<bool>("--json-only", "只生成JSON文件");
        var propsOnlyOption = new Option<bool>("--props-only", "只生成Props文件");
        var propertiesOnlyOption = new Option<bool>("--properties-only", "只生成Properties文件");
        
        var rootCommand = new RootCommand("QuickerGitVersion - 一个简单的GitVersion命令行工具实现")
        {
            outputOption,
            verboseOption,
            jsonOnlyOption,
            propsOnlyOption,
            propertiesOnlyOption
        };
        
        rootCommand.SetHandler(async (output, verbose, jsonOnly, propsOnly, propertiesOnly) =>
        {
            await ExecuteAsync(output, verbose, jsonOnly, propsOnly, propertiesOnly);
        }, outputOption, verboseOption, jsonOnlyOption, propsOnlyOption, propertiesOnlyOption);
        
        return await rootCommand.InvokeAsync(args);
    }
    
    static async Task ExecuteAsync(string output, bool verbose, bool jsonOnly, bool propsOnly, bool propertiesOnly)
    {
        // 设置日志
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                options.SingleLine = true;
            });
            
            builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
        });
        
        var logger = loggerFactory.CreateLogger<Program>();
        
        try
        {
            // 验证输出目录
            if (!ValidateOutputDirectory(output, logger))
            {
                Environment.Exit(1);
                return;
            }
            
            // 验证互斥选项
            if (CountTrueOptions(jsonOnly, propsOnly, propertiesOnly) > 1)
            {
                logger.LogError("只能选择一种输出格式选项");
                Environment.Exit(1);
                return;
            }
            
            // 创建服务实例
            var gitService = new GitService();
            var versionService = new VersionService();
            var fileGeneratorService = new FileGeneratorService();
            
            logger.LogInformation("开始获取Git信息...");
            
            // 获取Git信息
            var currentDir = Directory.GetCurrentDirectory();
            var gitInfo = gitService.GetGitInfo(currentDir);
            
            // 显示仓库信息
            if (verbose)
            {
                logger.LogDebug($"当前工作目录: {currentDir}");
                if (gitService.LastFoundRepositoryPath != null && 
                    !string.Equals(gitService.LastFoundRepositoryPath, currentDir, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogDebug($"Git仓库根目录: {gitService.LastFoundRepositoryPath}");
                }
            }
            
            logger.LogInformation($"处理分支: {gitInfo.BranchName}");
            logger.LogDebug($"当前提交: {gitInfo.ShortSha}");
            logger.LogDebug($"自版本源以来的提交数: {gitInfo.CommitsSinceVersionSource}");
            logger.LogDebug($"未提交更改: {gitInfo.UncommittedChanges}");
            
            // 计算版本信息
            logger.LogInformation("计算版本信息...");
            var versionInfo = versionService.CalculateVersion(gitInfo);
            logger.LogInformation($"计算版本: {versionInfo.FullSemVer}");
            
            // 生成文件
            logger.LogInformation($"生成版本文件到: {Path.GetFullPath(output)}");
            
            var tasks = new List<Task>();
            
            if (!propsOnly && !propertiesOnly)
            {
                tasks.Add(GenerateFileWithLogging(
                    () => fileGeneratorService.GenerateJsonFileAsync(versionInfo, output),
                    "gitversion.json",
                    logger));
            }
            
            if (!jsonOnly && !propertiesOnly)
            {
                tasks.Add(GenerateFileWithLogging(
                    () => fileGeneratorService.GeneratePropsFileAsync(versionInfo, output),
                    "gitversion.props",
                    logger));
            }
            
            if (!jsonOnly && !propsOnly)
            {
                tasks.Add(GenerateFileWithLogging(
                    () => fileGeneratorService.GeneratePropertiesFileAsync(versionInfo, output),
                    "gitversion.properties",
                    logger));
            }
            
            // 并行生成所有文件
            await Task.WhenAll(tasks);
            
            logger.LogInformation("版本文件生成完成");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "执行过程中发生错误");
            Environment.Exit(1);
        }
    }
    
    private static async Task GenerateFileWithLogging(Func<Task> generateFunc, string fileName, ILogger logger)
    {
        try
        {
            await generateFunc();
            logger.LogInformation($"已生成 {fileName}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"生成 {fileName} 失败");
            throw;
        }
    }
    
    private static bool ValidateOutputDirectory(string output, ILogger logger)
    {
        try
        {
            // 检查路径是否有效
            if (string.IsNullOrWhiteSpace(output))
            {
                logger.LogError("输出目录不能为空");
                return false;
            }
            
            // 尝试获取绝对路径
            var fullPath = Path.GetFullPath(output);
            
            // 检查是否有写入权限
            if (!HasWritePermission(fullPath))
            {
                logger.LogError($"没有权限写入到目录: {fullPath}");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"输出目录验证失败: {output}");
            return false;
        }
    }
    
    private static bool HasWritePermission(string directoryPath)
    {
        try
        {
            // 如果目录不存在，检查父目录权限
            if (!Directory.Exists(directoryPath))
            {
                var parentDir = Directory.GetParent(directoryPath);
                if (parentDir?.Exists == true)
                {
                    directoryPath = parentDir.FullName;
                }
                else
                {
                    return false;
                }
            }
            
            // 尝试创建临时文件检查权限
            var tempFile = Path.Combine(directoryPath, Path.GetRandomFileName());
            File.WriteAllText(tempFile, "test");
            File.Delete(tempFile);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private static int CountTrueOptions(params bool[] options)
    {
        return options.Count(o => o);
    }
} 