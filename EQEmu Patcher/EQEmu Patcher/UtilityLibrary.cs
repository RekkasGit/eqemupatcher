using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace EQEmu_Patcher
{
    /* General Utility Methods */
    class UtilityLibrary
    {


        public class UnsafeByteArrayManipulation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static UInt32 GetUInt32FromArray(byte[] source, int index)
            {
                fixed (byte* p = &source[0])
                {
                    return *(UInt32*)(p + index);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static Int32 GetInt32FromArray(byte[] source, int index)
            {
                fixed (byte* p = &source[0])
                {
                    return *(Int32*)(p + index);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static void SetInt32IntoArray(byte[] target, int index, Int32 value)
            {
                fixed (byte* p = &target[index])
                {
                    *((Int32*)p) = value;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static UInt64 GetUInt64FromArray(byte[] source, int index)
            {
                fixed (byte* p = &source[0])
                {
                    return *(UInt64*)(p + index);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static Int64 GetInt64FromArray(byte[] source, int index)
            {
                fixed (byte* p = &source[0])
                {
                    return *(Int64*)(p + index);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static void SetInt64IntoArray(byte[] target, int index, Int64 value)
            {
                fixed (byte* p = &target[index])
                {
                    *((Int64*)p) = value;
                }
            }

            //Originally taken from a microsoft paper, slightly modified so I could pass in arrays that are partially used.
            //This relies on the fact that Intel CPU's actually compare 10 bytes at a time.
            //http://techmikael.blogspot.com/2009/01/fast-byte-array-comparison-in-c.html
            //note, look at the comment section for the correct code.
            //the length2, passed is in case you use arrays from a buffer pool that isn't exactly the size you need.
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            public static unsafe bool ByteArraysEqual(byte[] array1, byte[] array2, Int32 lengthToCheckTo)
            {
                int length = lengthToCheckTo;
                fixed (byte* str = array1)
                {
                    byte* chPtr = str;
                    fixed (byte* str2 = array2)
                    {
                        byte* chPtr2 = str2;
                        while (length >= 20)
                        {
                            if ((((*(((int*)chPtr)) != *(((int*)chPtr2))) ||
                            (*(((int*)(chPtr + 4))) != *(((int*)(chPtr2 + 4))))) ||
                            ((*(((int*)(chPtr + 8))) != *(((int*)(chPtr2 + 8)))) ||
                            (*(((int*)(chPtr + 12))) != *(((int*)(chPtr2 + 12)))))) ||
                            (*(((int*)(chPtr + 16))) != *(((int*)(chPtr2 + 16)))))
                                break;

                            chPtr += 20;
                            chPtr2 += 20;
                            length -= 20;
                        }

                        while (length >= 4)
                        {
                            if (*(((int*)chPtr)) != *(((int*)chPtr2))) break;
                            chPtr += 4;
                            chPtr2 += 4;
                            length -= 4;
                        }

                        while (length > 0)
                        {
                            if (*chPtr != *chPtr2) break;
                            chPtr++;
                            chPtr2++;
                            length--;
                        }

                        return (length <= 0);
                    }
                }
            }
        }

        //Download a file to current directory
        public static string DownloadFile(string url, string outFile)
        {

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.DownloadFile(url, outFile);
                }
            } catch( IOException ie)
            {                
                return "IOException: "+ie.Message;
            } catch (WebException we) {
                if (we.Message == "The remote server returned an error: (404) Not Found.")
                {
                    return "404";
                }
                return "WebException: "+we.Message;  
            } catch (Exception e)
            {
                return "Exception: " + e.Message;
            }
            return "";
        }

        public static string GetMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }

                    return sb.ToString();
                }
            }
        }

        public static string GetJson(string urlPath)
        {
            using (WebClient wc = new WebClient())
            {
                return wc.DownloadString(urlPath);
            }
        }

        public static System.Diagnostics.Process StartEverquest()
        {
            return System.Diagnostics.Process.Start("eqgame.exe", "patchme");
        }


        public static string GetSHA1(string filePath)
        {
            //SHA1 sha = new SHA1CryptoServiceProvider();            
            //var stream = File.OpenRead(filePath);
            //return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty); ;
            /*Encoding enc = Encoding.UTF8;

            var sha = SHA1.Create();
            var stream = File.OpenRead(filePath);

            string hash = "commit " + stream.Length + "\0";
            return enc.GetString(sha.ComputeHash(stream));

            return BitConverter.ToString(sha.ComputeHash(stream));*/
            Encoding enc = Encoding.UTF8;

            string commitBody = File.OpenText(filePath).ReadToEnd();
            StringBuilder sb = new StringBuilder();
            sb.Append("commit " + Encoding.UTF8.GetByteCount(commitBody));
            sb.Append("\0");
            sb.Append(commitBody);

            var sss = SHA1.Create();
            var bytez = Encoding.UTF8.GetBytes(sb.ToString());
            return BitConverter.ToString(sss.ComputeHash(bytez));
            //var myssh = enc.GetString(sss.ComputeHash(bytez));
            //return myssh;
        }
        //Pass the working directory (or later, you can pass another directory) and it returns a hash if the file is found
        public static string GetEverquestExecutableHash(string path)
        {
            var di = new System.IO.DirectoryInfo(path);
            var files = di.GetFiles("eqgame.exe");
            if (files == null || files.Length == 0)
            {
                return "";
            }
            return UtilityLibrary.GetMD5(files[0].FullName);
        }
    }
}
