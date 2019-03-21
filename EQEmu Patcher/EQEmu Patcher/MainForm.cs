using EQEmu_Patcher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
//using System.Windows.Shell;

namespace EQEmu_Patcher
{
    
    public partial class MainForm : Form
    {

        /****
         *  EDIT THESE VARIABLES FOR EACH SERVER
         * 
         ****/

        public static string _serverName = "Echoes of Norrath";
        public static string _filelistUrl = "http://www.echoesofnorrath.com/eqemu_client";
        HashSet<string> _rootDirectoy_INIs_To_NOT_Ignore = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { @"\eqlsClient.ini", @"\VoiceChat.ini", @"\eqlsUIConfig.ini" };
        HashSet<string> _rootDirectoryToIgnore = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { @"\Logs", @"\mozilla", @"\userdata", @"\lib" };
        HashSet<string> _rootFilesToIgnore = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { @"\eqemupatcher.exe", @"\eqemupatcher.exe.config", @"\eqemupatcher.pdb", @"\eqemupatcher.png", @"\eqemupatcher.yml", @"\filelist.yml", @"\filelist.ver", @"\texture.txt",@"\UIErrors.txt" };
        byte[] _fileListVerResponse;
        public static string _currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;


        System.Diagnostics.Process _process;
        
        bool _isNeedingPatch;
        private Dictionary<VersionTypes, ClientVersion> clientVersions = new Dictionary<VersionTypes, ClientVersion>();

        VersionTypes currentVersion;

       // TaskbarItemInfo tii = new TaskbarItemInfo();
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Init();
        }



        private void Init()
        {
            _currentDirectory = Path.Combine(_currentDirectory, "everquest_rof2");

            System.IO.DirectoryInfo baseDir = new DirectoryInfo(_currentDirectory);
            _currentDirectory = baseDir.FullName;

            txtList.Visible = false;
            splashLogo.Visible = true;
            if (this.Width < 432)
            {
                this.Width = 432;
            }
            if (this.Height < 550)
            {
                this.Height = 550;
            }
            buildClientVersions();
            detectClientVersion();
            Boolean downloadFileList = false;
           
            this.Text = _serverName;


            string webUrl = _filelistUrl + "/filelist.ver";

            System.IO.FileInfo localFileVer = new FileInfo(Path.Combine(_currentDirectory,"filelist.ver"));

            if (localFileVer.Exists)
            {
                try
                {
                    _fileListVerResponse = DownloadFile(webUrl);

                    if (_fileListVerResponse == null || _fileListVerResponse.Length == 0)
                    {
                        //cannot be sure if we need to patch.
                    }
                    else
                    {
                        byte[] localVersionIno = System.IO.File.ReadAllBytes(localFileVer.FullName);

                        if (localVersionIno.Length == _fileListVerResponse.Length)
                        {
                            if (!UtilityLibrary.UnsafeByteArrayManipulation.ByteArraysEqual(localVersionIno, _fileListVerResponse, _fileListVerResponse.Length))
                            {
                                //they are not equal, lets get the file list. 
                                downloadFileList = true;
                            }

                        }

                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to fetch filelist from " + webUrl);
                }
            }
            else
            {
                downloadFileList = true;
                _fileListVerResponse = DownloadFile(webUrl);
            }

            btnStart.Font = new Font("Times New Roman", 24, FontStyle.Bold);
            if (downloadFileList)
            {
                _isNeedingPatch = true;
                btnStart.BackColor = Color.Red;
                btnStart.Text = "Patch";
               
            }
            else
            {
                btnStart.BackColor = Color.CornflowerBlue;
                btnStart.Text = "Play";
            }
          

        }

        private void detectClientVersion()
        {

            try
            {

                var hash = UtilityLibrary.GetEverquestExecutableHash(_currentDirectory);
                if (hash == "")
                {
                    MessageBox.Show("Please run this patcher in your Everquest directory.");
                    this.Close();
                    return;
                }
                switch (hash)
                {
                    case "85218FC053D8B367F2B704BAC5E30ACC":
                        currentVersion = VersionTypes.Secrets_Of_Feydwer;
                        break;
                    case "859E89987AA636D36B1007F11C2CD6E0":
                    case "EF07EE6649C9A2BA2EFFC3F346388E1E78B44B48": //one of the torrented uf clients, used by B&R too
                        currentVersion = VersionTypes.Underfoot;
                        break;
                    case "A9DE1B8CC5C451B32084656FCACF1103": //p99 client
                    case "BB42BC3870F59B6424A56FED3289C6D4": //vanilla titanium
                        currentVersion = VersionTypes.Titanium;
                        break;
                    case "368BB9F425C8A55030A63E606D184445":
                        currentVersion = VersionTypes.Rain_Of_Fear;
                        break;
                    case "240C80800112ADA825C146D7349CE85B":
                    case "A057A23F030BAA1C4910323B131407105ACAD14D": //This is a custom ROF2 from a torrent download
                        currentVersion = VersionTypes.Rain_Of_Fear_2;
                        break;
                    case "6BFAE252C1A64FE8A3E176CAEE7AAE60": //This is one of the live EQ binaries.
                    case "AD970AD6DB97E5BB21141C205CAD6E68": //2016/08/27
                        currentVersion = VersionTypes.Broken_Mirror;
                        break;
                    default:
                        currentVersion = VersionTypes.Unknown;
                        break;

                }
                splashLogo.Image = Properties.Resources.eqemupatcher;

                if (currentVersion == VersionTypes.Unknown)
                {
                    MessageBox.Show("Warning, Unable to recognize the Everquest client in this directory");
                }
              
            }
            catch (UnauthorizedAccessException err)
            {
                MessageBox.Show("You need to run this program with Administrative Privileges" + err.Message);
                return;
            }
        }

        //Build out all client version's dictionary
        private void buildClientVersions()
        {
            clientVersions.Clear();
            clientVersions.Add(VersionTypes.Titanium, new ClientVersion("Titanium", "titanium"));
            clientVersions.Add(VersionTypes.Secrets_Of_Feydwer, new ClientVersion("Secrets Of Feydwer", "sof"));
            clientVersions.Add(VersionTypes.Seeds_Of_Destruction, new ClientVersion("Seeds of Destruction", "sod"));
            clientVersions.Add(VersionTypes.Rain_Of_Fear, new ClientVersion("Rain of Fear", "rof"));
            clientVersions.Add(VersionTypes.Rain_Of_Fear_2, new ClientVersion("Rain of Fear 2", "rof2"));
            clientVersions.Add(VersionTypes.Underfoot, new ClientVersion("Underfoot", "underfoot"));
            clientVersions.Add(VersionTypes.Broken_Mirror, new ClientVersion("Broken Mirror", "brokenmirror"));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            PlayGame();            
        }

        private void PlayGame()
        {



            if (_isNeedingPatch)
            {
                StartPatch();
                return;
            }
          

            try
            {
                _process = UtilityLibrary.StartEverquest(_currentDirectory);
                if (_process != null) this.Close();
                else MessageBox.Show("The process failed to start");
            }
            catch (Exception err)
            {
                MessageBox.Show("An error occured while trying to start everquest: " + err.Message);
            }
        }

 
        bool isPatching = false;

        public object Keyboard { get; private set; }

       
        private byte[] DownloadFile(string url)
        {
            using (WebClient webClient = new WebClient())
            {

                return webClient.DownloadData(url);
            }
          
        }


        private void StartPatch()
        {
            if (isPatching) return;
            isPatching = true;
            btnStart.Text = "Cancel";


            //first lets download the file list
            string fileListURL  = _filelistUrl + "/filelist.yml";


            byte[] fileListResponse = null;
            LogEvent("Downloading file list from patch server...");
            try
            {
                fileListResponse = DownloadFile(fileListURL);
            }
            catch(Exception)
            {
                MessageBox.Show("Cannot download file list from patch server. Try again later.");
                isPatching = false;
                btnStart.Text = "Patch";
                return;
            }
            LogEvent("Download complete. Size:"+fileListResponse.Length);

            if (fileListResponse == null || fileListResponse.Length==0)
            {
                MessageBox.Show("Empty responsefrom patch server. Try again later.");
                isPatching = false;
                btnStart.Text = "Patch";
                return;
            }

            var deserializerBuilder = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention());
            var deserializer = deserializerBuilder.Build();
            FileList externalFileList = null;
            try
            {
                externalFileList = deserializer.Deserialize<FileList>(System.Text.Encoding.UTF8.GetString(fileListResponse));

            }
            catch (Exception)
            {
                MessageBox.Show("Bad file list response from patch server. Try again later.");
                isPatching = false;
                btnStart.Text = "Patch";
                return;
            }

            //got the response, now lets see what is different.
            
            //lets load the current one if it exists.
            FileInfo localFileListFile = new FileInfo(Path.Combine(_currentDirectory, "filelist.yml"));

            FileList localFileList = null;
            if (localFileListFile.Exists)
            {
                LogEvent("Loading local file compairson file....");

                //it exist locally lets load it up
                localFileList = deserializer.Deserialize<FileList>(System.IO.File.ReadAllText(localFileListFile.FullName));

            }
            else
            {
                LogEvent("Local file compairson file not found creating new one. \r\nThis may take a bit of time.....");

                //we have no local file list, we must create one.

                DirectoryInfo rootDirectory = new DirectoryInfo(_currentDirectory);

                List<FileInfo> filelist = rootDirectory.GetFiles().ToList();
                List<DirectoryInfo> foldersInCurrentDirectory = rootDirectory.GetDirectories().ToList();


                List<FileInfo> fileListClean = new List<FileInfo>();

                foreach (var file in filelist)
                {
                    if (!_rootFilesToIgnore.Contains(@"\"+file.Name))
                    {
                        fileListClean.Add(file);
                    }
                }

                filelist = fileListClean;


                foreach (DirectoryInfo dir in foldersInCurrentDirectory)
                {
                    if (_rootDirectoryToIgnore.Contains(@"\"+dir.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    WalkDirectory(filelist, dir);

                }
                System.Collections.Concurrent.ConcurrentDictionary<string, EQEmu_Patcher.Models.FileEntry> modelList = new System.Collections.Concurrent.ConcurrentDictionary<string, EQEmu_Patcher.Models.FileEntry>();

                LogEvent("Processing MD5's please wait....");
               
                foreach(FileInfo x in filelist)
                {
                    LogEvent("MD5ing :"+x.FullName);
                    EQEmu_Patcher.Models.FileEntry entry = new EQEmu_Patcher.Models.FileEntry();
                    entry.date = x.LastWriteTimeUtc.ToString();
                    entry.name = x.FullName.Replace(rootDirectory.FullName, "");
                    entry.size = (int)x.Length;
                    entry.md5 = checkMD5(x.FullName);
                    modelList.TryAdd(entry.name, entry);
                }

                localFileList = new FileList();
                localFileList.downloads = modelList.Values.ToList();

                LogEvent("In memory file compairson created. Going to next step");


            }


            //need to create lookups of local and external to determine what to do
            Dictionary<string, FileEntry> localListHash = localFileList.downloads.ToDictionary(x => x.name, x=>x, StringComparer.OrdinalIgnoreCase);
            Dictionary<string, FileEntry> externalListHash = externalFileList.downloads.ToDictionary(x => x.name, x => x, StringComparer.OrdinalIgnoreCase);


            LogEvent("Finding files that need updating...");

            //we now loop to see what is different to get, then loop the other way to see what we need to delete.
            foreach (KeyValuePair<string, FileEntry> pair in externalListHash)
            {
                //see what is differnt or new
            
                FileEntry localEntry;

                string fileName = pair.Value.name;
                bool isRootFile = false;
                if (fileName.LastIndexOf('\\') == 0)
                {
                    //this is a root file!
                    isRootFile = true;
                }

                if(isRootFile)
                {
                    
                    if (fileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if(_rootFilesToIgnore.Contains(fileName))
                    {
                        continue;
                    }

                }

                bool ignoredirectory = false;
                foreach(var dirToIgnore in _rootDirectoryToIgnore)
                {
                    if(fileName.StartsWith(dirToIgnore))
                    {
                        ignoredirectory = true;
                        break;

                    }
                }

                if(ignoredirectory)
                {
                    continue;
                }
                bool haveFileAndMatches = false;
                if (localListHash.TryGetValue(pair.Key, out localEntry))
                {
                    //we have the file locally
                    //check to see if it matches
                    if(String.Compare(localEntry.md5,pair.Value.md5,true)==0)
                    {
                        //hashes are not the same
                        haveFileAndMatches = true;
                    }
                }

                if(!haveFileAndMatches)
                {
                    //need to go get the file and replace the current one.
                    byte[] filePayload;

                    try

                    {
                        filePayload= DownloadFile(_filelistUrl + fileName.Replace(@"\", @"/"));
                    }
                    catch(Exception)
                    {
                        MessageBox.Show("Could not download file :" + fileName + " try again later.");
                        isPatching = false;
                        btnStart.Text = "Patch";
                        return;
                    }

                    if(filePayload==null )
                    {
                        MessageBox.Show("File not found or came back empty:" + fileName + " try again later.");
                        isPatching = false;
                        btnStart.Text = "Patch";
                        return;
                    }

                    string fileNameToSave = System.IO.Path.Combine(_currentDirectory, fileName.TrimStart(Path.DirectorySeparatorChar));
                    FileInfo fileToSaveInfo = new FileInfo(fileNameToSave);
                    if(!fileToSaveInfo.Directory.Exists)
                    {
                        fileToSaveInfo.Directory.Create();
                    }
                    System.IO.File.Delete(fileNameToSave);
                    LogEvent("Replacing file file:" + pair.Value.name);

                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(_currentDirectory, fileName.TrimStart(Path.DirectorySeparatorChar)), filePayload);
                 
                }
            }
            //see what should be deleted.
            LogEvent("Finding files that need Deleting...");

            foreach (KeyValuePair<string, FileEntry> pair in localListHash)
            {
             
                string fileName = pair.Value.name;
                bool isRootFile = false;
                if (fileName.LastIndexOf('\\') == 0)
                {
                    //this is a root file!
                    isRootFile = true;
                }


                if(isRootFile)
                {   //ignore root INI files
                    //ignore filelist files
                    if (fileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) && !_rootDirectoy_INIs_To_NOT_Ignore.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (fileName.EndsWith("filelist.yml", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (fileName.EndsWith("filelist.var", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                }
                bool ignoredirectory = false;
                foreach (var dirToIgnore in _rootDirectoryToIgnore)
                {
                    if (fileName.StartsWith(dirToIgnore))
                    {
                        ignoredirectory = true;
                        break;

                    }
                }

                if (ignoredirectory)
                {
                    continue;
                }

                if (!externalListHash.ContainsKey(pair.Value.name))
                {
                    LogEvent("Deleteing file:"+pair.Value.name);

                    //we don't have this file. delete
                    System.IO.File.Delete(System.IO.Path.Combine(_currentDirectory, pair.Value.name.TrimStart(Path.DirectorySeparatorChar))); 

                }

            }

            string fileListFileName = System.IO.Path.Combine(_currentDirectory,"filelist.yml");
            string fileListVerFileName = System.IO.Path.Combine(_currentDirectory, "filelist.ver");
            System.IO.File.Delete(fileListFileName);
            System.IO.File.WriteAllBytes(fileListFileName, fileListResponse);
            System.IO.File.Delete(fileListVerFileName);
            System.IO.File.WriteAllBytes(fileListVerFileName, _fileListVerResponse);
            btnStart.BackColor = Color.CornflowerBlue;
            btnStart.Text = "Play";
            _isNeedingPatch = false;
            LogEvent("Patching complete, press play to start.");
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

        private void LogEvent(string text)
        {
            if (!txtList.Visible)
            {
                txtList.Visible = true;
                splashLogo.Visible = false;
            }
            // Console.WriteLine(text);
            txtList.AppendText(text + "\r\n");
            Application.DoEvents();
        }

    }
   
  
}


