using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization;

namespace eqemupatcher.filelistceator
{
    class Program
    {
        static void Main(string[] args)
        {


            List<string> rootDirectoryToIgnore = new List<string>() { "Logs", "mozilla", "userdata" };
            HashSet<string> filesToIgnore = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "filelist.yml", "filelist.ver", "Thumbs.db" };
          

             Console.Write("Please enter a directory to work on:");
            string source_directory = Console.ReadLine();

            System.IO.DirectoryInfo rootdirectory = new System.IO.DirectoryInfo(source_directory);

            if(!rootdirectory.Exists)
            {
                Console.WriteLine("Directory does not exist, exiting.");
                return;
            }

            //check for a file you wish to use.

            //create list

            List<FileInfo> filelist = rootdirectory.GetFiles().ToList();
            List<DirectoryInfo> foldersInCurrentDirectory = rootdirectory.GetDirectories().ToList();


          
            

            foreach(DirectoryInfo dir in foldersInCurrentDirectory)
            {
                if( rootDirectoryToIgnore.Contains(dir.Name))
                {
                    continue;
                }

                WalkDirectory(filelist, dir);

            }
            List<FileInfo> fileListClean = new List<FileInfo>();

            foreach (var file in filelist)
            {
              
                if (!filesToIgnore.Contains(file.Name))
                {
                    fileListClean.Add(file);
                }
            }

            filelist = fileListClean;
            //have all the files , lets start compuiting some MD5's!
            System.Collections.Concurrent.ConcurrentDictionary<string, EQEmu_Patcher.Models.FileEntry> modelList = new System.Collections.Concurrent.ConcurrentDictionary<string, EQEmu_Patcher.Models.FileEntry>();

            Console.WriteLine("Processing MD5's please wait....");
           System.Threading.Tasks.Parallel.ForEach(filelist, (x)=>{
               EQEmu_Patcher.Models.FileEntry entry = new EQEmu_Patcher.Models.FileEntry();
               entry.date = x.LastWriteTimeUtc.ToString();
               entry.name = x.FullName.Replace(rootdirectory.FullName, "");
               entry.size = (int)x.Length;
               entry.md5 = checkMD5(x.FullName);
               modelList.TryAdd(entry.name, entry);
           });
            Console.WriteLine("Done processing MD5's writing out files");
            EQEmu_Patcher.Models.FileList finalList = new EQEmu_Patcher.Models.FileList();
            finalList.version = System.Guid.NewGuid().ToString("N");
            finalList.downloads = modelList.Values.ToList();

            var serializer = new Serializer();
            string finaloutput = serializer.Serialize(finalList);
            string fileListName = Path.Combine(rootdirectory.FullName, "filelist.yml");
            string fileListVersionName = Path.Combine(rootdirectory.FullName, "filelist.ver");
            System.IO.File.Delete(fileListVersionName);
            System.IO.File.WriteAllText(fileListVersionName, finalList.version);
            System.IO.File.Delete(fileListName);
            System.IO.File.WriteAllText(fileListName, finaloutput);

        }

        static string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return System.Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }
        static void WalkDirectory(List<FileInfo> filelist, DirectoryInfo currentDirectory)
        {
            filelist.AddRange(currentDirectory.GetFiles().ToList());
            List<DirectoryInfo> foldersInCurrentDirectory = currentDirectory.GetDirectories().ToList();
            foreach (DirectoryInfo dir in foldersInCurrentDirectory)
            {
                WalkDirectory(filelist, dir);
            }
        }

    }
}
