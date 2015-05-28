using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using System.IO;
using System.Reflection;
using IPlugIn_Test;//加入端口用來動態時的物件轉型用
namespace Test_DynamicLoadDLL
{
    /// <summary>
    /// C# 4.0 動態型別應用例：動態載入 DLL 模組
    /// ref:http://huan-lin.blogspot.com/2009/02/dynamically-loaded-dll-with-cshar-4.html
    /// </summary>
    class Program
    {
        //reflection test
        static void Main1(string[] args)
        {
            /*
            string CodeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string currentDic = Directory.GetCurrentDirectory();
            string directory = Path.GetPathRoot(AppPath);
            string currentDic2 = Environment.CurrentDirectory;
            */
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;// D:\\git\\Test_DynamicLoadDLL\\Test_DynamicLoadDLL\\bin\\Debug\\

            string parantPath = GetProjDir(currentPath, 3);// D:\\git\\Test_DynamicLoadDLL

            string fullPath = parantPath + "\\Lib\\";// D:\\git\\Test_DynamicLoadDLL\\Lib\\

            string DLLFullPath = Directory.GetFiles(fullPath)[0];// D:\\git\\Test_DynamicLoadDLL\\Lib\\PlugIn1.dll

            Assembly asm = Assembly.LoadFrom(DLLFullPath);//, new System.Security.Policy.Evidence());


            DateTime beginTime = DateTime.Now;

            Console.WriteLine("Begin Time:" + beginTime.ToString("HH:mm:ss"));
            var className = asm.GetTypes().FirstOrDefault().FullName;//get all Name(class Name + AssemblyName) include DLL and return the first search;
            //*****************Key Point*************************
            IPlugIn i1 = (IPlugIn)asm.CreateInstance(className);
            //running interface method
            i1.Execute();
            //***************************************************
            DateTime endTime = DateTime.Now;
            Console.WriteLine(" End Time: " + beginTime.ToString("HH:mm:ss"));
            Console.WriteLine("    Total: " + (endTime - beginTime).TotalMilliseconds.ToString() + "(ms)");

            Console.ReadKey();
        }

        //dynamic test
        static void Main(string[] args)
        {
            /*
            string CodeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string currentDic = Directory.GetCurrentDirectory();
            string directory = Path.GetPathRoot(AppPath);
            string currentDic2 = Environment.CurrentDirectory;
            */
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;// D:\\git\\Test_DynamicLoadDLL\\Test_DynamicLoadDLL\\bin\\Debug\\

            string parantPath = GetProjDir(currentPath, 3);// D:\\git\\Test_DynamicLoadDLL

            string fullPath = parantPath + "\\Lib\\";// D:\\git\\Test_DynamicLoadDLL\\Lib\\

            string DLLFullPath = Directory.GetFiles(fullPath)[0];// D:\\git\\Test_DynamicLoadDLL\\Lib\\PlugIn1.dll

            Assembly asm = Assembly.LoadFrom(DLLFullPath);//, new System.Security.Policy.Evidence());


            DateTime beginTime = DateTime.Now;

            Console.WriteLine("Begin Time:" + beginTime.ToString("HH:mm:ss"));
            var className = asm.GetTypes().FirstOrDefault().FullName;//get all Name(class Name + AssemblyName) include DLL and return the first search;
            //*****************Key Point*************************
            dynamic i1 = asm.CreateInstance(className);
            //running dynamic method 動態時才去執行其方法
            i1.Execute();
            //***************************************************
            DateTime endTime = DateTime.Now;
            Console.WriteLine(" End Time: " + beginTime.ToString("HH:mm:ss"));
            Console.WriteLine("    Total: " + (endTime - beginTime).TotalMilliseconds.ToString() + "(ms)");

            Console.ReadKey();
        }

        static string GetProjDir(string path,int forwardCount)
        {
            string tmpPath = "";
            //取得檔案或路徑的父目錄
            DirectoryInfo dirInfo = Directory.GetParent(path);
            //tmpPath = dirInfo.FullName;
            //取得上n層的父目錄
            for (int i = 0; i < forwardCount; i++)
            {
                dirInfo = dirInfo.Parent;
            }
            tmpPath = dirInfo.FullName;//取得路徑string
            return tmpPath;
        }
    }
}
