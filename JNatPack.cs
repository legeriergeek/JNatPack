using System;
using System.Diagnostics;
using System.IO.Compression;

class JNatPack
{
    static void Main(string[] args)
    {
        if(args.Length < 4)
        {
            Console.Error.WriteLine(
                "Usage:\n  JNatPack [jarfile.jar] [jre.zip] [output directory] [output name] [for mac only: is arm64? (if yes, put any value)]"
            );
            Environment.Exit(1); 
        }
        String jarPath = args[0];
        String jrePath = args[1];
        byte[] jarByte = File.ReadAllBytes(jarPath);
        byte[] compressedJarByte = deflate(jarByte);
        byte[] jreByte = File.ReadAllBytes(jrePath);
        byte[] compressedJreByte = deflate(jreByte);
        string file = GenerateFile(compressedJarByte, compressedJreByte);
        string temp_csproj = """
        <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net8.0</TargetFramework>
            <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
        </PropertyGroup>
        <ItemGroup>
            <Compile Include="GeneratedFile.cs" />
        </ItemGroup>
        </Project>
        """;
        Process compile;
        if (OperatingSystem.IsWindows())
        {
            File.WriteAllText(args[2] + "\\GeneratedFile.cs", file);
        }
        else
        {
            File.WriteAllText(args[2] + "/GeneratedFile.cs", file);
        }

        if (OperatingSystem.IsWindows())
        {
            File.WriteAllText(args[2] + "\\temp.csproj", temp_csproj);
        }
        else
        {
            File.WriteAllText(args[2] + "/temp.csproj", temp_csproj);
        }

        if (OperatingSystem.IsWindows())
        {
            compile = Process.Start(new ProcessStartInfo("dotnet", $"publish temp.csproj -r win-x64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName={args[3]}.exe /p:PublishDir=.") { WorkingDirectory = args[2] });
        }
        else if (OperatingSystem.IsLinux())
        {
            compile = Process.Start(new ProcessStartInfo("dotnet", $"publish temp.csproj -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName={args[3]} /p:PublishDir=.") { WorkingDirectory = args[2] });
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (args.Length == 4)
            {
                compile = Process.Start(new ProcessStartInfo("dotnet", $"publish temp.csproj -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName={args[3]} /p:PublishDir=.") { WorkingDirectory = args[2] });
            }
            else if (args.Length == 5)
            {
                compile = Process.Start(new ProcessStartInfo("dotnet", $"publish temp.csproj -r osx-arm64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName={args[3]} /p:PublishDir=.") { WorkingDirectory = args[2] });
            } else //lmao
            {
                compile = Process.Start(new ProcessStartInfo("dotnet", $"publish temp.csproj -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName={args[3]} /p:PublishDir=.") { WorkingDirectory = args[2] });
            }

        }
        else
        {
            compile = Process.Start(new ProcessStartInfo("dotnet", $"publish temp.csproj -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:AssemblyName={args[3]} /p:PublishDir=.") { WorkingDirectory = args[2] });
        }
        compile.WaitForExit();
        File.Delete(Path.Combine(args[2], "temp.csproj"));

    }

    static byte[] deflate(byte[] originalFile)
    {
        using (var output = new MemoryStream())
        {
            using (var deflate = new DeflateStream(output, CompressionLevel.Optimal))
            {
                deflate.Write(originalFile, 0, originalFile.Length);
            }
            byte[] compressed;
            compressed = output.ToArray();
            return compressed;
        }

    }

    static string GenerateFile(byte[] jarFile, byte[] jreFile)
    {
        String file = """
        using System;
        using System.IO.Compression;
        using System.IO;
        using System.Diagnostics;
        class FinalFile
        {
            static byte[] jarFile = Convert.FromBase64String(@"
        """ + Convert.ToBase64String(jarFile) + """
        ");
            static byte[] jreFile = Convert.FromBase64String(@"
        """ + Convert.ToBase64String(jreFile) + """
        ");
            static void Main(string[] args)
            {
                byte[] jarBytes = reverseDeflate(jarFile);
                byte[] jreBytes = reverseDeflate(jreFile);
                string temp = Path.GetTempPath();
                Guid g = Guid.NewGuid();
                string path = temp + "JNatPack_" + g;
                Directory.CreateDirectory(path);
                File.WriteAllBytes(path + "/jar.jar", jarBytes);
                File.WriteAllBytes(path + "/jre.zip", jreBytes);
                Process p;
                Process ext;
                if (OperatingSystem.IsWindows())
                {
                    ext = Process.Start("powershell", $"Expand-Archive -Path {"\"" + path + "\\jre.zip" + "\""} -DestinationPath {"\"" + path + "\\jre" + "\""}");
                }
                else
                {
                    ext = Process.Start("unzip", $"{path + "/jre.zip"} -d {path + "/jre"} ");
                }
                ext.WaitForExit();
                if (OperatingSystem.IsWindows())
                {
                    p = Process.Start(new ProcessStartInfo(Path.Combine(path, "jre", "bin", "java.exe"), "-jar jar.jar") { WorkingDirectory = path });
                }
                else
                {
                    p = Process.Start(new ProcessStartInfo(Path.Combine(path, "jre", "bin", "java"), "-jar jar.jar") { WorkingDirectory = path });
                }
                p.WaitForExit();
                try { Directory.Delete(path, true); } catch { }
                Environment.Exit(0);
            }
            
            static byte[] reverseDeflate(byte[] deflated)
            {
                using var input = new MemoryStream(deflated);
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();

                deflate.CopyTo(output);
                return output.ToArray();
            }
        }
        """;
        return file;
    }
}