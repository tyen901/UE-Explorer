using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UELib;
using UELib.Core;
using UELib.Flags;

namespace UEExplorer
{
    internal static class ExportHelpers
    {
        private const string ExportedDir = "Exported";
        private const string ClassesDir = "Classes";

        public static readonly string PackageExportPath = Path.Combine( Application.StartupPath, ExportedDir );

        public static string InitializeExportDirectory( this UnrealPackage package )
        {
            var exportPath = Path.Combine( PackageExportPath, package.PackageName );
            if( Directory.Exists( exportPath ) )
            {
                var files = Directory.GetFiles( exportPath );
                foreach( var file in files )
                {
                    File.Delete( exportPath + file );
                }			
            }
            var classPath = Path.Combine( exportPath, ClassesDir );
            Directory.CreateDirectory( classPath );
            return classPath;
        }

        public static void CreateUPKGFile( this UnrealPackage package, string exportPath )
        {
            var upkgContent = new[]
            {
                "[Flags]",
                "AllowDownload=" + package.HasPackageFlag( PackageFlags.AllowDownload ),
                "ClientOptional=" + package.HasPackageFlag( PackageFlags.ClientOptional ),
                "ServerSideOnly=" + package.HasPackageFlag( PackageFlags.ServerSideOnly )
            };

            File.WriteAllLines( 
                Path.Combine( exportPath, package.PackageName ) + UnrealExtensions.UnrealFlagsExt, 
                upkgContent 
            );
        }

        public static string ExportPackageClasses( this UnrealPackage package, string outputDirectory, bool exportScripts = false )
        {
            Program.LoadConfig();
            var exportPath = package.InitializeExportDirectory();
            package.NTLPackage = new NativesTablePackage();
            package.NTLPackage.LoadPackage( Path.Combine( Application.StartupPath, "Native Tables", Program.Options.NTLPath ) );
            foreach (var uClass in package.Objects.Where(o => o.ExportTable != null))
            {
                try
                {
                    string[] validClasses = new[]
                    {
                        "Material",
                        "MaterialInstanceConstant",
                        "StaticMesh",
                        "Texture2D",
                    };
                    
                    // Skip if the class is not a material
                    if (validClasses.Contains(uClass.ExportTable.ClassName) == false)
                        continue;
                    
                    if (package.FullPackageName.Contains("CookedPC"))
                    {
                        string fullPath = package.FullPackageName;
                        int index = fullPath.IndexOf("CookedPC", StringComparison.Ordinal);
                        string packagePath = fullPath.Substring(index);
                        packagePath = packagePath.Substring(0, packagePath.LastIndexOf('\\'));
                        outputDirectory = Path.Combine(outputDirectory, packagePath);
                    }
                    
                    if (uClass.Package != null)
                    {
                        string packageName = uClass.Package.PackageName;
                        outputDirectory = Path.Combine(outputDirectory, packageName);
                    }
                        
                    if (uClass.Outer != null)
                    {
                        string outerName = uClass.Outer.Name;
                        outputDirectory = Path.Combine(outputDirectory, outerName);
                    }
                
                    Directory.CreateDirectory(outputDirectory);

                    string extension = uClass.ExportTable.ClassName;

                    string fileName = Path.Combine(outputDirectory, uClass.Name) +
                                      "." + extension;
                                      // UnrealExtensions.UnrealCodeExt;
                    
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    
                    var exportContent = uClass.Decompile();
                    
                    File.WriteAllText(
                        fileName,
                        exportContent,
                        Encoding.ASCII
                    );

                    var buffer = uClass.Buffer;
                    
                    // Write buffer to file
                    if (buffer != null)
                    {
                        string bufferFileName = fileName + ".bin";
                        
                        if (File.Exists(bufferFileName))
                            File.Delete(bufferFileName);
                        
                        File.WriteAllBytes(
                            bufferFileName,
                            buffer.GetBuffer()
                        );
                    }
                }
                catch( Exception e )
                {
                    Console.WriteLine("Couldn't decompile object " + uClass + "\r\n" + e );
                }
            }
            
            package.CreateUPKGFile( exportPath );
            return exportPath;
        }
    }
}
