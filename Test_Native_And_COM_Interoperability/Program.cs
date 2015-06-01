using System;
using System.IO;
//
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Test_Native_And_COM_Interoperability
{
    class Program
    {
        #region 8.
        static void Main(string[] args)
        {
            RegularExpressionTest();
        }
        #endregion

        #region 7.Mapping a Struct to Unmanaged Memory

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct MyShareData
        {
            public int Value;
            public char Letter;
            public fixed float Numbers[50];
        }
        struct qq
        {
            public int i { get; set; }
            public byte b { get; set; }
            public bool boo;
        }
        static unsafe void Main7(string[] args)
        {
            qq q1 = new qq(){i = 100,b = 20, boo = true};
            Console.WriteLine(sizeof (MyShareData));    //208//1 int = 4 byte, 1 float = 4 byte, 1 char = 2 byte => 4(float) * 50(定義的大小) + 4(int) * 1 +  2(char) * 1 = 206 bytes => 因為一定為4byte的倍數所以char會變成4 byte => 208 bytes
            Console.WriteLine("bool:" + sizeof(bool));  //1 byte
            Console.WriteLine("byte:" + sizeof(byte));  //1 byte
            Console.WriteLine("char:" + sizeof(char));  //2 bytes
            Console.WriteLine("int:" + sizeof(int));    //4 bytes
            Console.WriteLine("Int32:" + sizeof(Int32));//4 bytes
            Console.WriteLine("long:" + sizeof(long));  //8 bytes
            Console.WriteLine("Int64:" + sizeof(Int64));//8 bytes
            Console.WriteLine("qq:" + sizeof(qq));      //4 + 4(原始為1 byte,但被進位成4 bytes) + 4(原始為1 byte,但被進位成4 bytes)
            
            MyShareData my;
            MyShareData* myPointer = &my;
            myPointer->Value = 123;
            var i = stackalloc MyShareData[0];
            IntPtr p = (IntPtr)myPointer;
            Console.WriteLine(p.ToString("x"));
            Console.WriteLine("Data:" + Marshal.SizeOf(q1));
            Console.ReadKey();
        }
        #endregion

        #region 6.Shared Memory
        //Ref memory address https://msdn.microsoft.com/en-US/library/zcbcf4ta(v=vs.80).aspx
        //ref http://stackoverflow.com/questions/588817/c-sharp-memory-address-and-variable
        static void Main6(string[] args)
        {
            unsafe
            {
                IntPtr p1 = Marshal.AllocCoTaskMem(500);
                Marshal.FreeCoTaskMem(p1);
                IntPtr p2 = Marshal.AllocHGlobal(50);
                Marshal.FreeHGlobal(p2);
                var i = p1;
            }
            //int i;
            //object o = new Blittable();
            //unsafe
            //{
            //    int* ptr = &i;
            //    IntPtr addr = (IntPtr)ptr;

            //    Console.WriteLine(addr.ToString("x"));

            //    GCHandle h = GCHandle.Alloc(o, GCHandleType.Pinned);
            //    addr = h.AddrOfPinnedObject();
            //    Console.WriteLine(addr.ToString("x"));

            //    h.Free();
            //}
            //這邊要開另一支程序執行並這邊的改成true執行才會去抓共享的記憶體資料
            using (ShareMem sm = new ShareMem("MyShare", false, 1000))//using (ShareMem sm = new ShareMem("MyShare", true, 1000))
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

        static void RegularExpressionTest()
        {
            string ex1 = "{\"MESSAGE_TYPE\":\"0420\",\"ICC_NO\":\"6617151008000010\",\"PROCESSING_CODE\":\"990174\",\"TRANS_DATETIME\":\"0528163756\",\"STAN\":\"043008\",\"RRN\":\"513516043008\",\"RC\":\"00\",\"BANK_CODE\":\"0806\",\"AMOUNT\":\"00000500\",\"STORE_NO\":\"00012345\",\"POS_NO\":\"00000003\",\"MERCHANT_NO\":\"000000012345678\",\"ICC_info\":{\"BIT_MAP\":null,\"STORE_NO\":\"00012345\",\"REG_ID\":\"003\",\"TX_DATETIME\":\"20150528163756\",\"ICC_NO\":\"6617151008000010\",\"AMT\":\"00000500\",\"NECM_ID\":\"860434082AE62080    \",\"RETURN_CODE\":\"00000508\"},\"ORI_dtat\":{\"MESSAGE_TYPE\":\"0100\",\"TRANSACTION_DATE\":\"0528163756\",\"STAN\":\"043007\",\"STORE_NO\":\"00012345\",\"RRN\":\"513516043007\"}}";

            string matchStr = Regex.Match(ex1,  "/^{\"MESSAGE_TYPE\":\"[0-9]{3}0/ig").Value;
            //new string instead old's
            string newStr = Regex.Replace(ex1, "^{\"MESSAGE_TYPE\":\"[0-9]{3}0", new MatchEvaluator((Match s2) =>
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine(s2.Value);
                return s2.Value.Substring(0, s2.Value.Length - 1) + "1";
            }));
            Console.ResetColor();
            Console.WriteLine("old:" + ex1);
            Console.WriteLine("new:" + newStr);

            Console.ReadKey();
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
