using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public static class ModSyncManager
    {
        // Known vanilla RDR2 files that should NEVER be synced
        private static readonly HashSet<string> VanillaFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Executables
            "RDR2.exe", "PlayRDR2.exe", "RDR2_BE.exe",
            
            // Core DLLs
            "amd_ags_x64.dll", "bink2w64.dll", "dxilconv7.dll", "nvngx_dlss.dll",
            "ffx_fsr2_api_vk_x64.dll", "ffx_fsr2_api_dx12_x64.dll", "ffx_fsr2_api_x64.dll",
            "steam_api64.dll", "NvLowLatencyVk.dll", "oo2core_5_win64.dll",
            
            // System files
            "version.txt", "vulkan-1.dll", "d3d12.dll",
            
            // VDF files
            "installscript.vdf", "installscript_sdk.vdf"
        };

        // Known vanilla RPF archives (these are NEVER mods)
        private static readonly HashSet<string> VanillaRPFs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "anim_0.rpf", "appdata0_update.rpf", "common_0.rpf", "data_0.rpf",
            "hd_0.rpf", "levels_0.rpf", "levels_1.rpf", "levels_2.rpf",
            "levels_3.rpf", "levels_4.rpf", "levels_5.rpf", "levels_6.rpf",
            "levels_7.rpf", "movies_0.rpf", "packs_0.rpf", "packs_1.rpf",
            "rowpack_0.rpf", "shaders_0.rpf", "shaders_1.rpf",
            "textures_0.rpf", "textures_1.rpf",
            "update_1.rpf", "update_2.rpf", "update_3.rpf", "update_4.rpf"
        };

        // Known vanilla folders (should not be synced)
        private static readonly HashSet<string> VanillaFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "12on7", "Redistributables", "x64", "singleplayer"
        };

        // Known MOD folders that indicate actual mods
        private static readonly HashSet<string> ModFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lml", "scripts", "mods", "plugins", "reshade-shaders"
        };

        public static SyncResult ScanAndSync(string gameDirectory, string modDirectory)
        {
            var result = new SyncResult();

            try
            {
                if (!Directory.Exists(gameDirectory))
                {
                    result.Success = false;
                    result.Message = "Game directory not found.";
                    return result;
                }

                if (!Directory.Exists(modDirectory))
                    Directory.CreateDirectory(modDirectory);

                var gameDir = new DirectoryInfo(gameDirectory);
                var filesFound = new List<FileInfo>();
                var foldersFound = new List<DirectoryInfo>();

                // Scan for MOD files (not vanilla files)
                foreach (var file in gameDir.GetFiles())
                {
                    if (IsModFile(file))
                    {
                        filesFound.Add(file);
                    }
                }

                // Scan for MOD folders
                foreach (var folder in gameDir.GetDirectories())
                {
                    if (IsModFolder(folder))
                    {
                        foldersFound.Add(folder);
                    }
                }

                if (filesFound.Count == 0 && foldersFound.Count == 0)
                {
                    result.Success = true;
                    result.Message = "No mods detected in game directory.\n\n" +
                                   "Mods are typically:\n" +
                                   "• .asi files (Script Hook mods)\n" +
                                   "• Folders: lml, scripts, mods\n" +
                                   "• dinput8.dll or other injector DLLs";
                    return result;
                }

                // Show what will be synced
                var confirmMsg = "Found the following mods:\n\n";

                if (filesFound.Count > 0)
                {
                    confirmMsg += "FILES:\n";
                    foreach (var file in filesFound.Take(10))
                    {
                        confirmMsg += $"  • {file.Name}\n";
                    }
                    if (filesFound.Count > 10)
                        confirmMsg += $"  ... and {filesFound.Count - 10} more\n";
                }

                if (foldersFound.Count > 0)
                {
                    confirmMsg += "\nFOLDERS:\n";
                    foreach (var folder in foldersFound)
                    {
                        confirmMsg += $"  • {folder.Name}\n";
                    }
                }

                confirmMsg += "\nCopy these to mod backup folder?";

                var confirmResult = MessageBox.Show(confirmMsg, "Confirm Sync",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    result.Success = false;
                    result.Message = "Sync cancelled by user.";
                    return result;
                }

                // Copy files
                foreach (var file in filesFound)
                {
                    string dest = Path.Combine(modDirectory, file.Name);
                    file.CopyTo(dest, true);
                    result.FilesCopied++;
                }

                // Copy folders
                foreach (var folder in foldersFound)
                {
                    string dest = Path.Combine(modDirectory, folder.Name);
                    CopyDirectory(folder.FullName, dest);
                    result.FoldersCopied++;
                }

                result.Success = true;
                result.Message = $"✅ Sync completed successfully!\n\n" +
                               $"📁 Files synced: {result.FilesCopied}\n" +
                               $"📂 Folders synced: {result.FoldersCopied}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"❌ Sync failed:\n\n{ex.Message}";
            }

            return result;
        }

        private static bool IsModFile(FileInfo file)
        {
            string fileName = file.Name;
            string extension = file.Extension.ToLower();

            // Rule 1: Skip if it's a known vanilla file
            if (VanillaFiles.Contains(fileName))
                return false;

            // Rule 2: Skip if it's a vanilla RPF archive
            if (VanillaRPFs.Contains(fileName))
                return false;

            // Rule 3: .asi files are ALWAYS mods (Script Hook)
            if (extension == ".asi")
                return true;

            // Rule 4: dinput8.dll is a common mod loader
            if (fileName.Equals("dinput8.dll", StringComparison.OrdinalIgnoreCase))
                return true;

            // Rule 5: version.dll is often used by mods
            if (fileName.Equals("version.dll", StringComparison.OrdinalIgnoreCase))
                return true;

            // Rule 6: ScriptHookRDR2.dll is a mod (use IndexOf for .NET Framework 4.7.2)
            if (fileName.IndexOf("ScriptHook", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Rule 7: LML (Lenny's Mod Loader) files
            if (fileName.IndexOf("lml", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Rule 8: Skip all other .dll files (likely vanilla)
            if (extension == ".dll")
                return false;

            // Rule 9: Skip all .rpf files (vanilla archives)
            if (extension == ".rpf")
                return false;

            // Rule 10: Skip executables
            if (extension == ".exe")
                return false;

            // Rule 11: .ini/.cfg/.txt files in root might be mod configs
            if (extension == ".ini" || extension == ".cfg" || extension == ".txt")
            {
                // But skip version.txt (vanilla)
                if (fileName.Equals("version.txt", StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            }

            // Default: Don't sync unknown files to be safe
            return false;
        }

        private static bool IsModFolder(DirectoryInfo folder)
        {
            string folderName = folder.Name;

            // Rule 1: Skip known vanilla folders
            if (VanillaFolders.Contains(folderName))
                return false;

            // Rule 2: Known mod folders
            if (ModFolders.Contains(folderName))
                return true;

            // Rule 3: Folders with "mod" in the name (use IndexOf for .NET Framework 4.7.2)
            if (folderName.IndexOf("mod", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Rule 4: Folders with "script" in the name
            if (folderName.IndexOf("script", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Default: Don't sync unknown folders
            return false;
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string folderName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, folderName);
                CopyDirectory(subDir, destSubDir);
            }
        }
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int FilesCopied { get; set; }
        public int FoldersCopied { get; set; }
    }
}
