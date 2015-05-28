using System;
using System.IO;
//
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Test_Native_And_COM_Interoperability
{
    [StructLayout(LayoutKind.Sequential)]
    class Blittable
    {
        int x;
    }

    class Program
    {
        #region 6.Shared Memory
        //Ref memory address https://msdn.microsoft.com/en-US/library/zcbcf4ta(v=vs.80).aspx
        //ref http://stackoverflow.com/questions/588817/c-sharp-memory-address-and-variable
        static void Main(string[] args)
        {
            int i;
            object o = new Blittable();
            unsafe
            {
                int* ptr = &i;
                IntPtr addr = (IntPtr)ptr;

                Console.WriteLine(addr.ToString("x"));

                GCHandle h = GCHandle.Alloc(o, GCHandleType.Pinned);
                addr = h.AddrOfPinnedObject();
                Console.WriteLine(addr.ToString("x"));

                h.Free();
            }
            using (ShareMem sm = new ShareMem("MyShare", false, 1000))
            {
                IntPtr root = sm.Root;
                unsafe
                {
                    IntPtr* o2 = &root;
                    IntPtr p2 = (IntPtr)o2;
                    Console.WriteLine("*root={0} root={1}", *o2, o2->ToString("x"));//
                    Console.WriteLine("p2 address = {0}", p2.ToString("x"));//這個是把o2的記憶體位至顯示出來
                }
                Console.WriteLine(root.ToInt32());
                Console.WriteLine(root.ToString("x"));
                // I have shared memory!

                Console.ReadLine(); // Here's where we start a second app...

            }
        }
        #endregion

        #region 5.Simulating a C Union

        [DllImport("winmm.dll")]
        public static extern uint midiOutShortMsg(IntPtr handle, uint message);

        static void Main5(string[] args)
        {
            NoteMessage n = new NoteMessage();
            //midiOutShortMsg()
            Console.WriteLine(n.PackedMsg); //0
            n.Channel = 10;
            n.Note = 100;
            n.Velocity = 50;
            Console.WriteLine(n.PackedMsg); //3302410
            n.PackedMsg = 3328010;
            Console.WriteLine(n.Note);      //200
            Console.ReadKey();
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct NoteMessage
        {

            [FieldOffset(0)]
            public uint PackedMsg;// 4 bytes long
            [FieldOffset(0)]
            public byte Channel;//FieldOffset also at 0
            [FieldOffset(1)]
            public byte Note;
            [FieldOffset(2)]
            public byte Velocity;

        }
        #endregion

        #region 4.Callback from Unmanaged Code

        delegate bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int EnumWindows(EnumWindowsCallback callback, IntPtr lParam);
        //delegate method
        static bool PrintWindow(IntPtr hWnd,IntPtr lParam)
        {
            Console.WriteLine(hWnd.ToInt64());
            return true;
        }
        static void Main4(string[] args)
        {
            int i = EnumWindows(PrintWindow, IntPtr.Zero);

            Console.ReadKey();
        }
        #endregion

        #region 3.Marshaling Classes and Structs

        [DllImport("kernel32.dll")]
        static extern void GetSystemTime(SystemTime t);
        static void Main3(string[] args)
        {
            
            SystemTime t = new SystemTime();
            GetSystemTime(t);//測試後無法使用方向參數語意 in out ref 
            Console.WriteLine(t.Year + "-" + t.Month + "-" + t.Day + " " + t.Hour + ":" + t.Minute + ":" + t.Second);
            Console.ReadKey();
        }

        /// <summary>
        /// StructLayout属性指示marshaler如何将每一个域映射到非托管代码。LayoutKind.Sequential着我们希望域顺序排列，就如同C结构中一样。这里的域名是无关的；域的顺序才是重要的。
        /// 這是用來接DLL回傳的資料,順序和宣告型態很重要,他會依序塞入欄位,
        /// 例如:第一個欄位為ushort則會收16bit的data=>  會收到                    0000 0111 1101 1111
        /// 若把第一個欄位改成int(32bit),會把回傳的資料=>會收到0000 0000 0000 0101 0000 0111 1101 1111
        ///                                                |---- 5(Month) ----||---- 2015(Year)---|=>329695
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        class SystemTime
        {
            public Int16 Year;//Int32 Yearf;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;
        }
        #endregion

        #region Test Regular Expression

        void RegularExpressionTest()
        {
            string ss = "awqe MESSAGE_TYPE:0121  eqwewq";
            string mathstr = Regex.Match(ss, "MESSAGE_TYPE:[0-9]{3}1").Value;
            bool check = Regex.IsMatch(ss, "MESSAGE_TYPE:[0-9]{3}1");
            //new string instead old's
            string newStr = Regex.Replace(ss, "MESSAGE_TYPE:[0-9]{3}1", new MatchEvaluator((Match s2) =>
            {
                Console.WriteLine(s2.Value);
                return s2.Value.Substring(0, s2.Value.Length - 1) + "0";
            }));
            Console.WriteLine(ss);
            Console.WriteLine(newStr);
            
        }
        #endregion

        #region 2.Marshaling Common Types

        [DllImport("kernel32.dll")]
        static extern int GetWindowsDirectory(StringBuilder sb, int maxChars);

        static void Main2(string[] args)
        {
            StringBuilder sb = new StringBuilder(256);
            int i = GetWindowsDirectory(sb, 256);
            Console.WriteLine("i=" + i + "  sb=>" + sb.ToString());
            
            Console.ReadKey();
        }
        #endregion

        #region 1.Calling into Native DLLs

        [DllImport("user32.dll")]
        static extern int MessageBox(IntPtr hwnd, string text, string caption, int type);

        static void Main1(string[] args)
        {
            int i = MessageBox(IntPtr.Zero, "Please do not press this again ...", "Attention", 0);


            Console.WriteLine("MessageBox return code:" + i);
            
            Console.ReadKey();
        }
        #endregion
    }
}
