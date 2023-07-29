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

        public static string ExportPackageClasses( this UnrealPackage package, bool exportScripts = false )
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
                    
                    // Get path
                    string fullPath = package.FullPackageName;
                    
                    // Strip off everything before CookedPC
                    int index = fullPath.IndexOf("CookedPC", StringComparison.Ordinal);
                    string packagePath = fullPath.Substring(index);
                    
                    // Remove the filename
                    packagePath = packagePath.Substring(0, packagePath.LastIndexOf('\\'));
                    
                    string packageName = uClass.Package.PackageName;
                    string outerName = uClass.Outer.Name;
                
                    string outputDirectoryPathFull = Path.Combine(
                        "D:\\ME\\output",
                        // uClass.ExportTable.ClassName,
                        packagePath,
                        packageName,
                        outerName
                        );
                
                    Directory.CreateDirectory(outputDirectoryPathFull);

                    string extension = uClass.ExportTable.ClassName;

                    string fileName = Path.Combine(outputDirectoryPathFull, uClass.Name) +
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
