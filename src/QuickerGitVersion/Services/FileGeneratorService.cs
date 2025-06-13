using System.Text.Json;
using System.Text;
using QuickerGitVersion.Models;

namespace QuickerGitVersion.Services;

public class FileGeneratorService
{
    public async Task GenerateJsonFileAsync(VersionInfo versionInfo, string outputPath)
    {
        try
        {
            // 检查目录是否存在，不存在则创建
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            var filePath = Path.Combine(outputPath, "gitversion.json");
            
            // 检查文件是否被锁定
            CheckFileAccess(filePath);
            
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            };
            
            var jsonContent = JsonSerializer.Serialize(versionInfo, jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonContent, Encoding.UTF8);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"没有权限写入文件到目录: {outputPath}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new InvalidOperationException($"输出目录不存在: {outputPath}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"文件IO错误: {ex.Message}", ex);
        }
    }
    
    public async Task GeneratePropsFileAsync(VersionInfo versionInfo, string outputPath)
    {
        try
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            var filePath = Path.Combine(outputPath, "gitversion.props");
            CheckFileAccess(filePath);
            
            var sb = new StringBuilder();
            sb.AppendLine("<Project>");
            sb.AppendLine("  <PropertyGroup>");
            
            // 使用反射获取所有属性并生成MSBuild属性
            var properties = typeof(VersionInfo).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(versionInfo);
                if (value != null)
                {
                    var xmlValue = System.Security.SecurityElement.Escape(value.ToString());
                    sb.AppendLine($"    <GitVersion_{prop.Name}>{xmlValue}</GitVersion_{prop.Name}>");
                }
            }
            
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("</Project>");
            
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
        {
            throw new InvalidOperationException($"生成Props文件失败: {ex.Message}", ex);
        }
    }
    
    public async Task GeneratePropertiesFileAsync(VersionInfo versionInfo, string outputPath)
    {
        try
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            var filePath = Path.Combine(outputPath, "gitversion.properties");
            CheckFileAccess(filePath);
            
            var sb = new StringBuilder();
            sb.AppendLine("# GitVersion Properties");
            sb.AppendLine($"# Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            // 使用反射获取所有属性并生成Java属性格式
            var properties = typeof(VersionInfo).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(versionInfo);
                if (value != null)
                {
                    // 转义Properties文件中的特殊字符
                    var escapedValue = value.ToString()?.Replace("\\", "\\\\").Replace("=", "\\=").Replace(":", "\\:");
                    sb.AppendLine($"GitVersion_{prop.Name}={escapedValue}");
                }
            }
            
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
        {
            throw new InvalidOperationException($"生成Properties文件失败: {ex.Message}", ex);
        }
    }
    
    private void CheckFileAccess(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                // 尝试打开文件检查是否被锁定
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.None);
                stream.Close(); // 显式关闭
            }
            catch (IOException)
            {
                throw new InvalidOperationException($"文件被其他进程占用: {Path.GetFileName(filePath)}");
            }
        }
    }
} 