using BentoContainerManager.Models;
using System.Text;

namespace BentoContainerManager.Services;

public static class ScriptsManager
{
    public static async Task<string> GenerateStartScript(List<Service> services, Platform platform)
    {
        var scriptFileName = platform == Platform.Windows ? "start-services.ps1" : "start-services.sh";
        var scriptContent = new List<string>();

        if (platform == Platform.Linux)
        {
            scriptContent.AddRange(new[]
            {
                "#!/bin/bash",
                "",
                "# Handle SIGTERM for graceful shutdown",
                "trap 'kill $(jobs -p)' SIGTERM",
                ""
            });
        }
        else
        {
            scriptContent.AddRange(new[]
            {
                "$ErrorActionPreference = 'Stop'",
                "",
                "# Handle shutdown",
                "$shutdownJobs = {",
                "    Get-Job | Stop-Job",
                "    Get-Job | Remove-Job",
                "}",
                "",
                "$PSEvent = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action $shutdownJobs",
                ""
            });
        }

        foreach (var service in services)
        {
            if (service.ServiceType == ServiceType.dotnet)
            {
                var serviceName = service.ServiceName;
                if (platform == Platform.Windows)
                {
                    scriptContent.Add($"# Start {service.ServiceName}");
                    scriptContent.Add($"Start-Job -ScriptBlock {{");
                    scriptContent.Add($"    Set-Location '/app/Services/{serviceName}'");
                    scriptContent.Add($"    dotnet {serviceName}.dll");
                    scriptContent.Add($"}}");
                }
                else
                {
                    scriptContent.Add($"# Start {service.ServiceName}");
                    scriptContent.Add($"cd /app/Services/{serviceName}");
                    scriptContent.Add($"dotnet {serviceName}.dll &");
                }
                scriptContent.Add("");
            }
        }

        // Keep container running and handle shutdown
    if (platform == Platform.Windows)
    {
        scriptContent.Add("while ($true) { Start-Sleep -Seconds 1 }");
        // For Windows scripts, use default WriteAllLines behavior
        await File.WriteAllLinesAsync(Path.GetFullPath(@"../" + scriptFileName), scriptContent);
    }
    else
    {
        scriptContent.Add("wait");
        // For Linux scripts, ensure proper line endings (LF only)
        var content = string.Join("\n", scriptContent);
        await File.WriteAllTextAsync(
            Path.GetFullPath(@"../" + scriptFileName), 
            content, 
            new UTF8Encoding(false)); // false = no BOM
    }
    
    return scriptFileName;
    }
}