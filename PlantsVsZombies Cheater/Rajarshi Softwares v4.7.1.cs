using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Forms;

namespace RajarshiSoftwares//v4.6.9 Stable Release
{
    #region Rajarshi_Softwares
    /// <summary>
    /// This class was original created by an unknown person and included only some of the functions.
    /// Later on, GameMaster Greatee aka Rajarshi Vaidya modified many functions and added many features(methods/functions).
    /// This Memory function is totally different from the original one.
    /// Functions ::-
    /// Performs all functions related to game memory modification.
    /// Requires DotNet 4.0 or later.
    /// No guarantee for any damage caused by the misuse of this file.
    /// Features added against previous version/s::-
    /// --------------------------------------------
    /// -optimized AOB searching method.
    /// -improved array positioning.
    /// -removed deprecated commander method.
    /// -problem regarding reading/writing Int32 with "Any CPU" architecture in Windows 10 is fixed
    /// -all write methods return last accessed address.
    /// 
    /// All functions are tested and are guaranteed to work.                   ------------------------------------NOTE--------------------------------
    /// ---Provided and developed by GameMaster Greatee aka Rajarshi Vaidya.
    /// 
    /// How to use ::-
    /// =====================
    /// 
    /// First make an object of Memory class ::-
    ///     Memory angel = new Memory();    //This must be a global declaration i.e. outside all functions.
    /// 
    /// Now use the code below to find your running game ::-
    /// 
    ///     string gamename = "Name of the game's executable file without its extension i.e. .exe";
    ///     Process game = Process.GetProcessessByName(gamename)[0];    // You may do this in a different way too.
    ///     angel.ReadProcess = game;
    ///     angel.Open();
    /// 
    ///     Now you are ready to use the functions to modify or view the game's memory. Use them as you wish.
    ///     For further help, just ask at the guidedhacking forum, other game cheating forums or directly contact me at guidedhacking ask me.
    /// 
    /// </summary>
    #region Memory Class - Advanced Version 4.6.6 - developed by GameMaster Greatee
    public class Memory
    {
        #region Extra - Not to be used in modifying or traversing memory
        #region Extra Variables
        long doneAOB = 0;
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private Process m_ReadProcess = null;
        private IntPtr m_hProcess = IntPtr.Zero;
        #endregion
        #region All Flags
        [Flags]
        private enum ProcessAccessType
        {
            PROCESS_TERMINATE = (0x0001),
            PROCESS_CREATE_THREAD = (0x0002),
            PROCESS_SET_SESSIONID = (0x0004),
            PROCESS_VM_OPERATION = (0x0008),
            PROCESS_VM_READ = (0x0010),
            PROCESS_ALL_ACCESS = (0x1F0FFF),
            PROCESS_VM_WRITE = (0x0020),
            PROCESS_DUP_HANDLE = (0x0040),
            PROCESS_CREATE_PROCESS = (0x0080),
            PROCESS_SET_QUOTA = (0x0100),
            PROCESS_SET_INFORMATION = (0x0200),
            PROCESS_QUERY_INFORMATION = (0x0400)
        }
        private string Make(byte[] buffer)
        {
            string sTemp = "";

            for (long i = 0; i < buffer.Length; i++)
            {
                if (Convert.ToInt16(buffer[i]) < 10)
                {
                    sTemp = "0" + ToHex(buffer[i]) + sTemp;
                }
                else
                {
                    sTemp = ToHex(buffer[i]) + sTemp;
                }
            }
            return sTemp;
        }
        #endregion
        #region All DLL Imports
        [DllImport("kernel32")]
        private static extern int OpenProcess(int AccessType, int InheritHandle, int ProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect
            );

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, long dwSize, long flNewProtect, out int lpflOldProtect);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);

        [DllImport("user32.dll")]
        private static extern int FindWindow(string sClassName, string sAppName);

        [DllImport("user32.dll")]
        private static extern long GetWindowThreadProcessId(int HWND, out int processId);

        [DllImport("kernel32.dll")]
        private static extern long OpenProcessD(long AccessType, long InheritHandle, long ProcessId);

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(int Handle);
        #endregion
        #endregion
        #region Process and random based functions - 23 functions
        #region Find the process ID - long PID()
        /// <summary>
        /// Obtain the Process ID for the process that is Open.
        /// </summary>
        /// <returns></returns>
        public long PID()
        {
            return m_ReadProcess.Id;
        }
        #endregion
        #region BaseAddressInHexadecimal - string BaseAddressH()
        /// <summary>
        /// Find the base address of the opened process in Hexadecimal.
        /// </summary>
        /// <returns>string - base address in hex.</returns>
        public string BaseAddressH()
        {
            return ToHex(m_ReadProcess.MainModule.BaseAddress.ToInt64());
        }
        #endregion
        #region Contruct Ending Of A Call or Jump instruction - byte[] ConstructEndingOfACallOrJmp
        /// <summary>
        /// Construct a byte array that contains the bytes of instructions to the jump addresses present at any code-cave.
        /// </summary>
        /// <param name="AddressOfLastJmpOrCall">Int64 - The address from where there will be a jump to the starting of the codecave's bytes.</param>
        /// <param name="WhatToInject">byte[] - A byte array containing all the bytes that are to be injected.(Don't include any jump or call instruction's bytes.)</param>
        /// <param name="UseJump_1_OrCall_2">Int32 - 1 or 0 depending on how you want to return to the original address from where the Last Jump or Call was used.</param>
        /// <param name="CodeCaveAddress">Int64 - An address representing where you want to inject the given bytes.</param>
        /// <returns></returns>
        public byte[] ConstructEndingOfACallOrJmp(long AddressOfLastJmpOrCall, byte[] WhatToInject, long UseJump_1_OrCall_2, long CodeCaveAddress)
        {
            long pos = 0;
            long codecavadd = CodeCaveAddress;
            byte[] temp1 = new byte[1], temp12;
            byte[] temp90;
            temp90 = ChangeAddressInJmpOrCallToBytes((codecavadd + WhatToInject.Length + 6), AddressOfLastJmpOrCall + 0x00B);
            if (UseJump_1_OrCall_2 == 1)
            {
                temp12 = new byte[5];
                temp12[0] = 0xE9;
                for (long i = 0; i < temp90.Length; i++)
                {
                    temp12[i + 1] = temp90[i];
                }
                GC.Collect();
            }
            else
            {
                temp12 = new byte[1];
                temp12[0] = 0xC3;
            }
            byte[] temp0 = new byte[WhatToInject.Length + temp12.Length];
            for (long i = 0; i < WhatToInject.Length; i++)
            {
                temp0[pos] = WhatToInject[i];
                pos++;
            }
            for (long i = 0; i < temp12.Length; i++)
            {
                temp0[pos] = temp12[i];
                pos++;
            }
            GC.Collect();
            return temp0;
        }
        #endregion
        #region Convert Bytes to String - string ByteToString(byte[] aass)
        /// <summary>
        /// Convert any byte array to a string for futher manipulation.
        /// </summary>
        /// <param name="aass">The byte array to be converted to string.</param>
        /// <returns>A string containing all values of the byte array.</returns>
        public string ByteToString(byte[] aass)
        {
            return BitConverter.ToString(aass.Reverse().ToArray()).Replace("-", "");
        }
        #endregion
        #region Convert String to Bytes - byte[] StringToByte(string aass)
        /// <summary>
        /// Convert any string into a byte array.(HEX string only)
        /// </summary>
        /// <param name="aass">String - Note that the string must have even no. of characters and all uppercases(for alphabets).</param>
        /// <returns>A byte[] array for the specified string.</returns>
        public byte[] StringToByte(string aass)
        {
            if (aass.Length % 2 == 1)
            {
                aass = "0" + aass;
            }
            byte[] asd = new byte[aass.Length / 2];
            try
            {
                for (long i = 0; i < aass.Length / 2; i++)
                {
                    asd[i] = Convert.ToByte(ToDec(Convert.ToString(aass.ElementAt((int)i * 2)) + Convert.ToString(aass.ElementAt((int)i * 2 + 1))));
                }
            }
            catch
            {
                throw new Exception("The number of characters in the string must be even.");
            }
            return asd;

        }
        #endregion
        #region Address to Byte Conversion for Jump and Call instruction
        /// <summary>
        /// Obtain the bytes for the address in the call or jump instruction.
        /// </summary>
        /// <param name="WhereToInject">The address where the jump or call instruction will be injected.</param>
        /// <param name="AddressToJumpTo">The address specified in the instruction where the call or jump is to take place.</param>
        /// <returns>Byte[] - Returns the bytes in form of a byte array so that these may be directly written to the specified address.(returns 00 in case of a failure.)</returns>
        public byte[] ChangeAddressInJmpOrCallToBytes(long WhereToInject, long AddressToJumpTo)
        {
            byte[] lost = { 0x00 };
            string ans = ToHex((WhereToInject + 5 - AddressToJumpTo) * (-1));
            if (ans.Length >= 9)
            {
                ans = ans.Substring(8);
            }
            if (ans.Length > 8)
            {
                return lost;
            }
            byte[] anstemp = new byte[4];
            for (long i = 1; i > 0; i++)
            {
                if (ans.Length < 8)
                    ans = "0" + ans;
                else
                    i = -7;
            }
            long iii = 0;
            for (long i = 0; i < 4; i++)
            {
                anstemp[3 - i] = Convert.ToByte(ToDec(ans.Substring((int)iii, 2)));
                iii += 2;
            }
            return anstemp;
        }
        #endregion
        #region Generate bytes for a Jump instruction - byte[] ByteForJmpCall(long WhereToInj, long AddToJmpTo, long JmpOrCall)
        /// <summary>
        /// Calculate all bytes for a particular Jump or Call to an address.
        /// </summary>
        /// <param name="WhereToInj">Where the bytes will be written.</param>
        /// <param name="AddToJmpTo">The address where the jump is to be taken.</param>
        /// <param name="JmpOrCall">Decide to use jump(1) or call(any other).</param>
        /// <returns></returns>
        public byte[] ByteForJmpCall(long WhereToInj, long AddToJmpTo, long JmpOrCall)
        {
            byte[] rrtt = new byte[5];
            if (JmpOrCall == 1)
            {
                rrtt[0] = 0xE9;
            }
            else
                rrtt[0] = 0xE8;
            byte[] rn = ChangeAddressInJmpOrCallToBytes(WhereToInj, AddToJmpTo);
            for (long i = 0; i < 4; i++)
            {
                rrtt[i + 1] = rn[i];
            }
            return rrtt;
        }
        #endregion
        #region BaseAddressInDecimal - long BaseAddressD()
        /// <summary>
        /// Find the base address of the opened process in Decimal.
        /// </summary>
        /// <returns>long - base address as integer.</returns>
        public long BaseAddressD()
        {
            return m_ReadProcess.MainModule.BaseAddress.ToInt64();
        }
        #endregion
        #region Read-Write Access Modifier
        /// <summary>
        /// Force a region of memory of the current process to have READ-WRITE access.
        /// </summary>
        /// <param name="startAddress">The starting address from where Read/Write access will be available.</param>
        /// <param name="endAddress">The address until where to make Read/Write access.</param>
        /// <returns>Boolean - true if successsful else false.</returns>
        public bool ReadWriteAccess(long startAddress, long endAddress)
        {
            /*
             * Flags::-
             * ==========================
             * NOACCESS          0x01     
             * READONLY          0x02     
             * READWRITE         0x04     
             * WRITECOPY         0x08
             */
            int xx;
            try
            {
                return VirtualProtectEx(m_hProcess, (IntPtr)startAddress, endAddress - startAddress, 0x04, out xx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occured. Error :: " + ex.Message);
            }
        }
        #endregion
        #region Find the last address(decimal) - long LastAddressD(string ModuleName)
        /// <summary>
        /// Find the last address of the specified module in decimal.
        /// </summary>
        /// <param name="ModuleName">Specify the module name here or null.</param>
        /// <returns>Long - if ModuleName set to null, then returns the last address of the MainModule in decimal.</returns>
        public long LastAddressD(string ModuleName)
        {
            long found = 0;
            if (ModuleName != null)
            {
                long modulelength = 0;
                foreach (ProcessModule mod in m_ReadProcess.Modules)
                {
                    if (mod.ModuleName == ModuleName)
                    {
                        found = mod.BaseAddress.ToInt64();
                        modulelength = mod.ModuleMemorySize;
                    }
                }
                found = found + modulelength - 1;
            }
            else
                found = (m_ReadProcess.MainModule.BaseAddress.ToInt64() + m_ReadProcess.MainModule.ModuleMemorySize - 1);
            if (found != -1)
                return found;
            else
                return found + 1;
        }
        #endregion
        #region Find the last address(hexadecimal) - long LastAddressH(string ModuleName)
        /// <summary>
        /// Find the last address of the specified module in hexadecimal.
        /// </summary>
        /// <param name="ModuleName">Specify the module name here or null.</param>
        /// <returns>String - if ModuleName set to null, then returns the last address of the MainModule in hexadecimal.</returns>
        public string LastAddressH(string ModuleName)
        {
            return ToHex(LastAddressD(ModuleName));
        }
        #endregion
        #region Checkgame by Window Title - long CheckGameT(string WindowTitle)
        /// <summary>
        /// Check if the application of the provided Window Title is currently running or not(No need to use Open method, that activates all functions, for this).
        /// </summary>
        /// <param name="WindowTitle">string WindowTitle - The window title of the any application.</param>
        /// <returns>integer - 1 if found else 0.</returns>
        public bool CheckGameT(string WindowTitle)
        {
            bool result = false;
            checked
            {
                try
                {
                    int Proc;
                    int HWND = FindWindow(null, WindowTitle);
                    GetWindowThreadProcessId(HWND, out Proc);
                    int Handle = OpenProcess(PROCESS_ALL_ACCESS, 0, Proc);
                    if (Handle != 0)
                    {
                        result = true;
                    }
                    CloseHandle(Handle);
                }
                catch
                { }
            }
            return result;
        }
        #endregion
        #region Find the module address - long ModuleAddress(string ModuleName, Process ani)
        /// <summary>
        /// Retrieve the specified module's starting address that is running under the given application.
        /// </summary>
        /// <param name="ModuleName">Enter the full name of the module including the extension.</param>
        /// <returns>An Int64 that specifies the base address of the module in decimal notation.</returns>
        public long ModuleAddress(string ModuleName)
        {
            long found = 0;
            try
            {
                foreach (ProcessModule mod in m_ReadProcess.Modules)
                {
                    if (mod.ModuleName.Contains(ModuleName))
                    {
                        found = mod.BaseAddress.ToInt64();
                    }
                }
            }
            catch
            { }
            return found;
        }
        #endregion
        #region Convert to hex - string ToHex(long Decimal)
        /// <summary>
        /// Convert a decimal(Int64 or Int32) to hexadecimal(String).
        /// </summary>
        /// <param name="Decimal">Enter an integer(Int32 or Int64) to be converted to hex notation.</param>
        /// <returns>A string representing the decimal number provided in hexadecimal notation.</returns>
        public string ToHex(long Decimal)
        {
            return Decimal.ToString("X"); //Convert Decimal to Hexadecimal
        }
        #endregion
        #region Convert hex to decimal - long ToDec(string Hex)
        /// <summary>
        /// Convert a hexadecimal(String) notation into decimal(Int64 - long).
        /// </summary>
        /// <param name="Hex">Enter number in hexadecimal notation as string.</param>
        /// <returns>An Int64 obtained from converting the hex into decimal notation.</returns>
        public long ToDec(string Hex)
        {
            long gg = long.Parse(Hex, System.Globalization.NumberStyles.HexNumber); //Convert Hexadecimal to Decimal
            return gg;
        }
        #endregion
        #region Process Specifier - Process ReadProcess
        /// <summary>
        /// Specifies the process, for which, the memory functions are to be used.
        /// </summary>
        public Process ReadProcess
        {
            get
            {
                return m_ReadProcess;
            }
            set
            {
                m_ReadProcess = value;
            }
        }
        #endregion
        #region Open all memory related functions - void Open()
        /// <summary>
        /// Activate all the functions related to memory(CheckGameT is always ready to use without using this function).
        /// </summary>
        public void Open()
        {
            try
            {
                ProcessAccessType access = ProcessAccessType.PROCESS_VM_READ
                | ProcessAccessType.PROCESS_VM_WRITE
                | ProcessAccessType.PROCESS_VM_OPERATION
                | ProcessAccessType.PROCESS_ALL_ACCESS;
                m_hProcess = OpenProcess((uint)access, 1, (uint)m_ReadProcess.Id);
            }
            catch
            {
                throw new Exception("Please re-initialise the particular object for the new game-name.");
            }
        }
        #endregion
        #region Close all memory functions - void Close()
        /// <summary>
        /// Deactivate all the functions related to memory except CheckGameT.
        /// </summary>
        public void CloseHandle()
        {
            long iRetValue;
            try
            {
                iRetValue = CloseHandle(m_hProcess);
                if (iRetValue == 0)
                {
                    throw new Exception("CloseHandle Failed");
                }
            }
            catch
            { }
        }
        #endregion
        #region Find the address of any AOB pattern - long FindAOBaddress(byte[] AOBpattern,long StartAddress, long LasteAddress) + 1 overload
        /// <summary>
        /// Find the starting address of the given pattern.
        /// </summary>
        /// <param name="AOBpattern">byte[] - The byte array containing the bytes to be searched for.(Array count must be atleast of 8 elements.)</param>
        /// <param name="StartAddress">Int64 - The first address from where the scan is to be started. Enter 0 for the address of MainModule.</param>>
        /// <param name="LastAddress">Int64 - The last address until which the scan is to be done. Enter 0 for the last address of MainModule.</param>
        /// <returns>Int64 address or 0(0 only if pattern not found).</returns>
        public long FindAOBaddress(byte[] AOBpattern, long StartAddress, long LastAddress)
        {
            bool decision = false;
            long found = extraAOB(AOBpattern, ref StartAddress, ref LastAddress, new int[] { }, ref decision);
            found = found + StartAddress;
            if (found >= LastAddress - AOBpattern.Length)
            {
                found = 0;
            }
            GC.Collect();
            return found;
        }

        /// <summary>
        /// Find the starting address of the given pattern.
        /// </summary>
        /// <param name="AOBpattern">byte[] - The byte array containing the bytes to be searched for.(Array count must be atleast of 8 elements.)</param>
        /// <param name="StartAddress">Int64 - The first address from where the scan is to be started. Enter 0 for the address of MainModule.</param>>
        /// <param name="LastAddress">Int64 - The last address until which the scan is to be done. Enter 0 for the address of MainModule.</param>
        /// <param name="pos">Int32 - An integer array containing the list of positions of bytes that are to be skipped in the array search.</param>
        /// <returns>Int64 address or 0(0 only if pattern not found).</returns>
        public long FindAOBaddress(byte[] AOBpattern, long StartAddress, long LastAddress, int[] pos)
        {
            bool decision = true;
            long found = extraAOB(AOBpattern, ref StartAddress, ref LastAddress, pos, ref decision);
            if (found < LastAddress - AOBpattern.Length)
            {
                found = found + StartAddress;
            }
            else
                found = 0;
            GC.Collect();
            return found;
        }

        private long extraAOB(byte[] AOBpattern, ref long StartAddress, ref long LastAddress, int[] pos, ref bool decision)
        {
            if (AOBpattern.Length < 8)
                throw new Exception("Please provide a AOB byte pattern array of size atleast 8");
            long found = 0;
            if (StartAddress == 0)
                StartAddress = BaseAddressD();
            if (LastAddress == 0)
                LastAddress = LastAddressD(null);
            long i;
            for (i = (int)StartAddress; i < LastAddress - AOBpattern.Length; i += 5097152 - AOBpattern.Length)
            {
                if ((LastAddress - i) > 5097152)
                {
                    found = found + commander(Read(i, 5097152), AOBpattern, pos, decision);
                    GC.Collect();
                }
                else
                {
                    found = found + commander(Read(i, Convert.ToInt32(LastAddress - i)), AOBpattern, pos, true);
                    GC.Collect();
                }
                if (doneAOB != 0)
                {
                    doneAOB = 0;
                    break;
                }
            }
            return found;
        }
        private long commander(byte[] scaned, byte[] pattern, int[] pos, bool positional = false)
        {
            if (pos != null)
                positional = true;
            else
                positional = false;
            if (positional)
            {
                //this will run when positional aobs are to be scanned.
                string s1 = ByteToString(scaned), s2 = ByteToString(pattern);
                long i = 0, found = 0;
                for (int jj = 0; jj < pos.Length; jj++)
                {
                    for (int ii = 0; ii < pos.Length; ii++)
                    {
                        if (ii != jj)
                            if (pos[jj] == pos[ii])
                                throw new Exception("No two values of the position vectors must be equal.");
                    }
                }
                int startpos = 0, count = 0;
                for (int ji = 0; ji < pos.Length; ji++)
                {
                    if (ji < pos[ji] - 1)
                    {
                        startpos = ji;
                        count = pos[ji] - ji - 1;
                        break;
                    }
                }
                for (; i < s1.Length - s2.Length; i++)
                {
                    i = s1.IndexOf(s2.Substring(startpos, count * 2));
                    s1 = s1.Substring((int)i - startpos);
                    found += i;
                    if (chkr(s1.Substring(0, s2.Length), s2, pos, startpos + count))
                    {
                        doneAOB = 1;
                        return found / 2;
                    }
                    else
                    {
                        s1 = s1.Substring(startpos + 2);
                        found += 2;
                    }
                }
                return s1.Length / 2;
            }
            else
            {
                //this will run when simple aobs are to be scanned.
                long found = ByteToString(scaned).IndexOf(ByteToString(pattern));
                if (found >= 0)
                {
                    doneAOB = 1;
                    return found / 2;
                }
                else
                    return scaned.Length;
            }
        }
        //Please do not modify the algorithm
        private bool chkr(string s1, string s2, int[] pos, int pos1)
        {
            string ss = "", ss2 = "";
            if (!pos.Contains(pos1 + 1))
            {
                if (s1.ElementAt((pos1) * 2).ToString() + s1.ElementAt(((pos1) * 2 + 1)).ToString() == s2.ElementAt((pos1) * 2).ToString() + s2.ElementAt(((pos1) * 2 + 1)).ToString())
                {
                    ss = s1.ElementAt((pos1) * 2).ToString() + s1.ElementAt(((pos1) * 2 + 1)).ToString();
                    ss2 = s2.ElementAt((pos1) * 2).ToString() + s2.ElementAt(((pos1) * 2 + 1)).ToString();
                    if ((pos1 + 1) * 2 == s2.Length)
                    {
                        return true;
                    }
                    else
                    {
                        return chkr(s1, s2, pos, pos1 + 1);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return chkr(s1, s2, pos, pos1 + 1);
            }
        }
        #endregion
        #region Generate an integer array - long[] intArrayGen(long from, long to)
        /// <summary>
        /// Generate an integer(Int32) array starting and ending from given points. The values will be consecutive.
        /// </summary>
        /// <param name="from">Int32 - The starting value.</param>
        /// <param name="to">Int32 - The last value.</param>
        /// <returns>An integer(Int32) array containing consecutive values including the starting and end values.</returns>
        public int[] intArrayGen(int from, int to)
        {
            if (from >= to)
                return new int[] { };
            int[] p = new int[to - from + 1];
            for (int y = 0; y < p.Length; y++)
            {
                p[y] = from + y;
            }
            return p;
        }
        #endregion
        #region Combine multiple integer arrays into one array - int[] intArrayComb(int[] arr1,int[] arr2,int[] arr3,int[] arr4,int[] arr5,int[] arr6) + 4 overloads
        /// <summary>
        /// Combine multiple integer(Int32) arrays into one array. Maximum no. of arrays that can be combined is 6.
        /// </summary>
        /// <param name="arr1">Enter the first array.</param>
        /// <param name="arr2">Enter the second array.</param>
        /// <returns>An integer(Int32) array containing all the elements of the given arrays starting from arr1 till the last one</returns>
        public int[] intArrayComb(int[] arr1, int[] arr2)
        {
            int[] add = new int[arr1.Length + arr2.Length];
            long i = 0, p = 0;
            for (i = 0; i < arr1.Length; i++)
            {
                add[i] = arr1[i];
            }
            p = i;
            for (i = 0; i < arr2.Length; i++)
            {
                add[p] = arr2[i];
                p++;
            }
            return add;
        }
        /// <summary>
        /// Combine multiple integer(Int32) arrays into one array. Maximum no. of arrays that can be combined is 6.
        /// </summary>
        /// <param name="arr1">Enter the first array.</param>
        /// <param name="arr2">Enter the second array.</param>
        /// <param name="arr3">Enter the third array.</param>
        /// <returns>An integer(Int32) array containing all the elements of the given arrays starting from arr1 till the last one</returns>
        public int[] intArrayComb(int[] arr1, int[] arr2, int[] arr3)
        {
            int[] add = new int[arr1.Length + arr2.Length + arr3.Length];
            long i = 0, p = 0;
            for (i = 0; i < arr1.Length; i++)
            {
                add[i] = arr1[i];
            }
            p = i;
            for (i = 0; i < arr2.Length; i++)
            {
                add[p] = arr2[i];
                p++;
            }
            for (i = 0; i < arr3.Length; i++)
            {
                add[p] = arr3[i];
                p++;
            }
            return add;
        }
        /// <summary>
        /// Combine multiple integer(Int32) arrays into one array. Maximum no. of arrays that can be combined is 6.
        /// </summary>
        /// <param name="arr1">Enter the first array.</param>
        /// <param name="arr2">Enter the second array.</param>
        /// <param name="arr3">Enter the third array.</param>
        /// <param name="arr4">Enter the fourth array.</param>
        /// <returns>An integer(Int32) array containing all the elements of the given arrays starting from arr1 till the last one</returns>
        public int[] intArrayComb(int[] arr1, int[] arr2, int[] arr3, int[] arr4)
        {
            int[] add = new int[arr1.Length + arr2.Length + arr3.Length + arr4.Length];
            long i = 0, p = 0;
            for (i = 0; i < arr1.Length; i++)
            {
                add[i] = arr1[i];
            }
            p = i;
            for (i = 0; i < arr2.Length; i++)
            {
                add[p] = arr2[i];
                p++;
            }
            for (i = 0; i < arr3.Length; i++)
            {
                add[p] = arr3[i];
                p++;
            }
            for (i = 0; i < arr4.Length; i++)
            {
                add[p] = arr4[i];
                p++;
            }
            return add;
        }
        /// <summary>
        /// Combine multiple integer(Int32) arrays into one array. Maximum no. of arrays that can be combined is 6.
        /// </summary>
        /// <param name="arr1">Enter the first array.</param>
        /// <param name="arr2">Enter the second array.</param>
        /// <param name="arr3">Enter the third array.</param>
        /// <param name="arr4">Enter the fourth array.</param>
        /// <param name="arr5">Enter the fifth array.</param>
        /// <returns>An integer(Int32) array containing all the elements of the given arrays starting from arr1 till the last one</returns>
        public int[] intArrayComb(int[] arr1, int[] arr2, int[] arr3, int[] arr4, int[] arr5)
        {
            int[] add = new int[arr1.Length + arr2.Length + arr3.Length + arr4.Length + arr5.Length];
            long i = 0, p = 0;
            for (i = 0; i < arr1.Length; i++)
            {
                add[i] = arr1[i];
            }
            p = i;
            for (i = 0; i < arr2.Length; i++)
            {
                add[p] = arr2[i];
                p++;
            }
            for (i = 0; i < arr3.Length; i++)
            {
                add[p] = arr3[i];
                p++;
            }
            for (i = 0; i < arr4.Length; i++)
            {
                add[p] = arr4[i];
                p++;
            }
            for (i = 0; i < arr5.Length; i++)
            {
                add[p] = arr5[i];
                p++;
            }
            return add;
        }
        /// <summary>
        /// Combine multiple integer(Int32) arrays into one array. Maximum no. of arrays that can be combined is 6.
        /// </summary>
        /// <param name="arr1">Enter the first array.</param>
        /// <param name="arr2">Enter the second array.</param>
        /// <param name="arr3">Enter the third array.</param>
        /// <param name="arr4">Enter the fourth array.</param>
        /// <param name="arr5">Enter the fifth array.</param>
        /// <param name="arr6">Enter the sixth array.</param>
        /// <returns>An integer(Int32) array containing all the elements of the given arrays starting from arr1 till the last one</returns>
        public int[] intArrayComb(int[] arr1, int[] arr2, int[] arr3, int[] arr4, int[] arr5, int[] arr6)
        {
            int[] add = new int[arr1.Length + arr2.Length + arr3.Length + arr4.Length + arr5.Length + arr6.Length];
            long i = 0, p = 0;
            for (i = 0; i < arr1.Length; i++)
            {
                add[i] = arr1[i];
            }
            p = i;
            for (i = 0; i < arr2.Length; i++)
            {
                add[p] = arr2[i];
                p++;
            }
            for (i = 0; i < arr3.Length; i++)
            {
                add[p] = arr3[i];
                p++;
            }
            for (i = 0; i < arr4.Length; i++)
            {
                add[p] = arr4[i];
                p++;
            }
            for (i = 0; i < arr5.Length; i++)
            {
                add[p] = arr5[i];
                p++;
            }
            for (i = 0; i < arr6.Length; i++)
            {
                add[p] = arr6[i];
                p++;
            }
            return add;
        }
        #endregion
        #region Convert Integers to Bytes - byte[] AddToBytes(long add)
        /// <summary>
        /// Convert any decimal(Int64) address to an array of bytes.
        /// </summary>
        /// <param name="add">An integer(Int64) that is to be converted into byte array.</param>
        /// <returns>A byte array of size 4.</returns>
        private byte[] AddToBytes(long add)
        {
            string ad = ToHex(add);
            if (ad.Length < 8)
            {
                for (long i = 0; i <= 8 - ad.Length; i++)//8 is taken for 4 bytes(8 digits of add converted to hex). If you want to use addresses
                {
                    ad = "0" + ad;
                }
            }
            return StringToByte(ad);
        }
        #endregion
        #region Convert Bytes to Integers - long BytesToAdd(byte[] add)
        /// <summary>
        /// Convert an array of bytes into an integer(Int64) address.
        /// </summary>
        /// <param name="add">The byte array, from the combination of whose contents, the integer(Int64) is to be calculated.</param>
        /// <returns>An integer(Int64).</returns>
        private long BytesToAdd(byte[] add)
        {
            string asd = ByteToString(add);
            long a = ToDec(ByteToString(add));
            return ToDec(ByteToString(add));
        }
        #endregion
        #endregion
        #region Functions on reading a value from memory - 13 functions
        #region Read values from any address - byte[] Read(MemoryAddress, uint bytesToRead)
        /// <summary>
        /// Read values starting from any address.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The memory address from where to start.</param>
        /// <param name="bytesToRead">uint bytesToRead - The number of bytes to read.</param>
        /// <returns>byte[] - A byte array containing all bytes that were read.</returns>
        public byte[] Read(long MemoryAddress, int bytesToRead)
        {
            byte[] buffer = new byte[bytesToRead];
            IntPtr ptrBytesRead;
            try
            {
                ReadProcessMemory(m_hProcess, (IntPtr)MemoryAddress, buffer, (uint)bytesToRead, out ptrBytesRead);
            }
            catch
            {
                throw new Exception("Please ensure that a particular process is selected.");
            }
            if ((int)ptrBytesRead == 0)
            {
                if (bytesToRead != 0)
                    throw new Exception("Error!!! The requested memory region could not be accessed. Maybe because of the following reasons ::-\n1.This program is not run in elevated mode.\n2.Requested memory is out of the scope of the target application.\n3.A part or whole of the requested address range is inaccessible.");
            }
            return buffer;
        }
        #endregion
        #region Read an integer from an address - int ReadInt(long MemoryAddress, int size)
        /// <summary>
        /// Read an Integer Value from a specified address. Integer values may be of the type 1Byte, 2 Bytes, 4 Bytes and 8 Bytes.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where reading starts.</param>
        /// <param name="size">The byte size parameter for reading i.e. 1 byte, 2 bytes, 4 bytes or 8 bytes.</param>
        /// <returns>Int32 - The integer value read from the address according to the required parameters.</returns>
        public int ReadInt(long MemoryAddress, int size)
        {
            long Value = 0;
            checked
            {
                try
                {
                    if ((size == 1) || (size == 2) || (size == 4) || (size == 8))
                    {
                        Value = ToDec(ByteToString(Read(MemoryAddress, size)));
                    }
                    else
                        throw new Exception("Byte size of 1, 2, 4 and 8 only supported.");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error reading values.\n\nError :: " + ex.Message);
                }
            }
            return (int)Value;
        }
        #endregion
        #region Read a float from an address - float ReadFloat(long MemoryAddress)
        /// <summary>
        /// Read a Float Value from a specified address.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where reading starts.</param>
        /// <returns>Float - The float value read from the address according to the required parameters.</returns>
        public float ReadFloat(long MemoryAddress)
        {
            float Value = 0;
            checked
            {
                try
                {
                    Value = BitConverter.ToSingle(Read(MemoryAddress, 4), 0);
                }
                catch
                {
                    throw new Exception("Error reading values.");
                }
            }
            return Value;
        }
        #endregion
        #region Read a double from an address - double ReadDouble(long MemoryAddress)
        /// <summary>
        /// Read a Double Value from a specified address.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where reading starts.</param>
        /// <returns>Double - The double value read from the address according to the required parameters.</returns>
        public double ReadDouble(long MemoryAddress)
        {
            double Value = 0;
            checked
            {
                try
                {
                    Value = BitConverter.ToDouble(Read(MemoryAddress, 8), 0);
                }
                catch
                {
                    throw new Exception("Error reading values.");
                }
            }
            return Value;
        }
        #endregion
        #region Read a string from an address - string ReadString(long MemoryAddress, int length)
        /// <summary>
        /// Reads a string from the specified address to the specified length.
        /// </summary>
        /// <param name="MemoryAddress">The address from where to start reading.</param>
        /// <param name="length">The length of characters to be read.</param>
        /// <returns>A string value read from the specified address and appropriate length.</returns>
        public string ReadString(long MemoryAddress, int length)
        {
            string value = "";
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                try
                {
                    chars[i] = Convert.ToChar(Convert.ToByte(ReadInt(MemoryAddress + i, 1)));
                }
                catch
                {
                    goto link;
                }
                link:
                { }
            }
            value = new string(chars);
            return value;
        }
        #endregion
        #region Read a string until null is encountered - string ReadStringTillNull(long MemoryAddress)
        /// <summary>
        /// Reads a string until a null character is encountered.
        /// <para>Maximum length of the string must be 100 characters.</para>
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where to start reading.</param>
        /// <returns>Returns a string.</returns>
        public string ReadStringTillNull(long MemoryAddress)
        {
            string value = "";
            int i;
            for (i = 0; i < 100; i++)
            {
                if (ReadInt(MemoryAddress + i, 1) == 0)
                {
                    break;
                }
            }
            if (i != 0)
            {
                value = ReadString(MemoryAddress, i);
            }
            else
            {
                value = "There is no null(0) character found in the range :: " + MemoryAddress + " to " + (MemoryAddress + 50) + ".";
            }
            return value;
        }
        #endregion
        #region Read value from any pointer - byte[] PointerRead(long MemoryAddress, int[] Offset, int bytesToRead)
        /// <summary>
        /// Reads the value from any given pointer.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The initial address.</param>
        /// <param name="Offset">int[] Offset - The offsets required for computing the actual address.</param>
        /// <param name="bytesToRead">uint bytesToRead - The number of bytes to be read.</param>
        /// <returns>byte[] - The byte array containing all the bytes read.</returns>
        public byte[] PointerRead(long MemoryAddress, int[] Offset, int bytesToRead)
        {
            return Read(PtrAddress(MemoryAddress, Offset), bytesToRead);
        }
        #endregion
        #region Read an integer from a pointer - long PointerReadInt(long MemoryAddress, int[] Offset, int size)
        /// <summary>
        /// Reads an integer value from the given pointer address.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where the pointer starts.</param>
        /// <param name="Offset">An Int32 array consisting of offsets.</param>
        /// <param name="size">The size parameter for the value i.e. 1 bytes, 2 bytes, 4 bytes or 8 bytes.</param>
        /// <returns>An Int32 value obtained from reading the target address.</returns>
        public long PointerReadInt(long MemoryAddress, int[] Offset, int size)
        {
            try
            {
                if ((size == 1) || (size == 2) || (size == 4) || (size == 8))
                {
                    return ReadInt(PtrAddress(MemoryAddress, Offset), size);
                }
                else
                    throw new Exception("The size must be 1, 2, 4 or 8 bytes only");
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occured. Error :: " + ex.Message);
            }
        }
        #endregion
        #region Read float from a pointer - float PointerReadFloat(long MemoryAddress, int[] Offset)
        /// <summary>
        /// Reads a float value from the given pointer address.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where the pointer starts.</param>
        /// <param name="Offset">An Int32 array consisting of offsets.</param>
        /// <returns>A Float value obtained from reading the target address.</returns>
        public float PointerReadFloat(long MemoryAddress, int[] Offset)
        {
            try
            {
                return ReadFloat(PtrAddress(MemoryAddress, Offset));
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occured. Error :: " + ex.Message);
            }
        }
        #endregion
        #region Read a double from a pointer - double PointerReadDouble(long MemoryAddress, int[] Offset)
        /// <summary>
        /// Reads a double value from the given pointer address.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where the pointer starts.</param>
        /// <param name="Offset">An Int32 array consisting of offsets.</param>
        /// <returns>A Double value obtained from reading the target address.</returns>
        public double PointerReadDouble(long MemoryAddress, int[] Offset)
        {
            try
            {
                return ReadDouble(PtrAddress(MemoryAddress, Offset));
            }
            catch
            {
                throw new Exception("Error in finding pointer address or reading values.");
            }
        }
        #endregion
        #region Read string from a pointer - string PtrReadString(long MemoryAddress, int[] Offset, int length)
        /// <summary>
        /// Read a string from a pointer.
        /// </summary>
        /// <param name="MemoryAddress">The memory address required.</param>
        /// <param name="Offset">The integer offset array to calculate the target address.</param>
        /// <param name="length">The length of the string.</param>
        /// <returns>The string read from the specified address.</returns>
        public string PtrReadString(long MemoryAddress, int[] Offset, int length)
        {
            return ReadString(PtrAddress(MemoryAddress, Offset), length);
        }
        #endregion
        #region Read a string from a pointer until null is encountered - string PtrReadStringTillNull(long MemoryAddress,int[] Offset)
        /// <summary>
        /// Reads a string from a pointer target until a null character is encountered.
        /// </summary>
        /// <param name="MemoryAddress">The memory address from where to start traversing.</param>
        /// <param name="Offset">The offsets to calculate the destination of the pointer.</param>
        /// <returns>A string.</returns>
        public string PtrReadStringTillNull(long MemoryAddress, int[] Offset)
        {
            return ReadStringTillNull(PtrAddress(MemoryAddress, Offset));
        }
        #endregion
        #region Find destination address of a pointer - long PtrAddress(long MemoryAddress,int[] Offsets)
        /// <summary>
        /// Find the address pointed by a pointer.
        /// </summary>
        /// <param name="MemoryAddress">Main Address here.</param>
        /// <param name="Offsets">Offsets here.</param>
        /// <returns>Long value i.e. the target address.</returns>
        public long PtrAddress(long MemoryAddress, int[] Offsets)
        {
            try
            {
                foreach (int i in Offsets)
                {
                    MemoryAddress = ReadInt(MemoryAddress, 4);
                    MemoryAddress += i;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occured. Error :: " + ex.Message);
            }
            return MemoryAddress;
        }
        #endregion
        #endregion
        #region Functions on writing to the memory - 14 functions
        #region Write bytes at an address - Write(long MemoryAddress, byte[] bytesToWrite)
        /// <summary>
        /// Write bytes starting from a given address.
        /// </summary>
        /// <param name="MemoryAddress">long MemoryAddress - The address from where to start writing.</param>
        /// <param name="bytesToWrite">byte[] bytesToWrite - The byte array containing bytes that are to be written.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long Write(long MemoryAddress, byte[] bytesToWrite)
        {
            IntPtr ptrBytesWritten;
            try
            {
                WriteProcessMemory(m_hProcess, (IntPtr)MemoryAddress, bytesToWrite, (uint)bytesToWrite.Length, out ptrBytesWritten);
            }
            catch
            {
                throw new Exception("Please ensure that the selected process/game/application is defined.");
            }
            return MemoryAddress + bytesToWrite.Length;
        }
        #endregion
        #region Write an integer to an address - WriteInt(long MemoryAddress, long Value, uint size)
        /// <summary>
        /// Write integer at a given address.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The address from where to start writing.</param>
        /// <param name="Value">Int64 - A long value that is to be written to the address.</param>
        /// <param name="size">The size parameter for the integer value i.e. 1 bytes, 2 bytes, 4 bytes or 8 bytes.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long WriteInt(long MemoryAddress, long Value, uint size)
        {
            try
            {
                if ((size == 1) || (size == 2) || (size == 4) || (size == 8))
                {
                    if (size == 1)
                    {
                        if ((Value <= 255) && (Value >= 0))
                            Write(MemoryAddress, StringToByte(ToHex(Value)));
                        else
                            throw new Exception("A 1 byte value ranges from 0 to 255");
                        return MemoryAddress + 1;
                    }
                    else if (size == 2)
                    {
                        if ((Value <= 65535) && (Value >= 0))
                            Write(MemoryAddress, BitConverter.GetBytes((short)Value));
                        else
                            throw new Exception("A 2 bytes value ranges from 0 to 65535");
                        return MemoryAddress + 2;
                    }
                    else if (size == 4)
                    {
                        if ((Value <= 4294967295) && (Value >= 0))
                            Write(MemoryAddress, BitConverter.GetBytes((int)Value));
                        else
                            throw new Exception("A 4 bytes value ranges from 0 to 4294967295");
                        return MemoryAddress + 4;
                    }
                    else
                    {
                        Write(MemoryAddress, BitConverter.GetBytes(Value));
                        return MemoryAddress + 8;
                    }
                }
                else
                    throw new Exception("Note :: Byte size is limited to 1, 2, 4 and 8 only.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error in writing values. Check if game is running or this application has administrative access.\n\nError ::" + ex.Message);
            }
        }
        #endregion
        #region Write float to an address - WriteFloat(long MemoryAddress, float Value)
        /// <summary>
        /// Write a float value at a given address.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The address from where to start writing.</param>
        /// <param name="Value">Float - a float value that is to be written to the address.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long WriteFloat(long MemoryAddress, float Value)
        {
            try
            {
                Write(MemoryAddress, BitConverter.GetBytes(Value));
                return MemoryAddress + BitConverter.GetBytes(Value).Length;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in writing values. Error :: " + ex.Message);
            }
        }
        #endregion
        #region Write a double value to an address - WriteDouble(long MemoryAddress, double Value)
        /// <summary>
        /// Write a double value at a given address.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The address from where to start writing.</param>
        /// <param name="Value">Double - a double value that is to be written to the address.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long WriteDouble(long MemoryAddress, double Value)
        {
            try
            {
                Write(MemoryAddress, BitConverter.GetBytes(Value));
                return MemoryAddress + BitConverter.GetBytes(Value).Length;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in writing values. Error :: " + ex.Message);
            }
        }
        #endregion
        #region Write string to an address - WriteString(long MemoryAddress, string Value)
        /// <summary>
        /// Write string to an address.
        /// </summary>
        /// <param name="MemoryAddress">The required memory address.</param>
        /// <param name="Value">The string to write to the address.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long WriteString(long MemoryAddress, string Value)
        {
            try
            {
                byte[] val = new byte[Value.Length];
                for (int i = 0; i < val.Length; i++)
                {
                    val[i] = Convert.ToByte(Value.ElementAt(i));
                }
                Write(MemoryAddress, val);
                return MemoryAddress + Value.Length;
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occured. Error :: " + ex.Message);
            }
        }
        #endregion
        #region Make a Jump or Call to a codecave - JumpOrCall(long addToPlaceJmpOrCall,long addWhereToJmpOrCall,long BytesToNopAtJmpOrCall,byte[] BytesToInject,long UseJmpOrCall)
        /// <summary>
        /// Create a Jump or call from an address to any other specified address.
        /// </summary>
        /// <param name="addToPlaceJmpOrCall">The address where the jump or call instruction is to be placed.</param>
        /// <param name="addWhereToJmpOrCall">The address to which the jump will be made.</param>
        /// <param name="BytesToNopAtJmpOrCall">The no. of bytes to be noped, starting with 1 from the address where jump or call instruction is to be placed.</param>
        /// <param name="BytesToInject">byte[] - An array containing all the bytes to be placed at the target address where jump is to be made.</param>
        /// <param name="UseJmpOrCall">Int32 - Whether you want to use Jump(1) or Call(any other value).</param>
        /// <returns>long(Int64) - The address of the last byte where data was written in the codecave.</returns>
        public long JmpOrCall(long addToPlaceJmpOrCall, long addWhereToJmpOrCall, long BytesToNopAtJmpOrCall, byte[] BytesToInject, long UseJmpOrCall)
        {
            noperTill(addToPlaceJmpOrCall, BytesToNopAtJmpOrCall);
            long tada = Write((addWhereToJmpOrCall), ConstructEndingOfACallOrJmp(addToPlaceJmpOrCall, BytesToInject, UseJmpOrCall, addWhereToJmpOrCall));
            Write(addToPlaceJmpOrCall, ByteForJmpCall(addToPlaceJmpOrCall, addWhereToJmpOrCall, UseJmpOrCall));
            return tada;
        }
        #endregion
        #region Write byte/s to a Pointer - PtrWrite(long MemoryAddress, int[] Offset, byte[] bytesToWrite)
        /// <summary>
        /// Writes bytes into a given pointer target.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The initial address.</param>
        /// <param name="bytesToWrite">byte[] bytesToWrite - The byte array that contains the bytes to be written.</param>
        /// <param name="Offset">int[] Offset - An integer array containing the offsets for computing the actual address to be written.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long PtrWrite(long MemoryAddress, int[] Offset, byte[] bytesToWrite)
        {
            return Write(PtrAddress(MemoryAddress, Offset), bytesToWrite);
        }
        #endregion
        #region Write float to a pointer - PtrWriteFloat(long MemoryAddress, int[] Offset, float Value)
        /// <summary>
        /// Writes a float into a given pointer target.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The initial address.</param>
        /// <param name="Offset">int[] Offset - An integer array containing the offsets for computing the actual address to be written.</param>
        /// <param name="Value">float - The value that is to be written to be written.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long PtrWriteFloat(long MemoryAddress, int[] Offset, float Value)
        {
            return Write(PtrAddress(MemoryAddress, Offset), BitConverter.GetBytes(Value));
        }
        #endregion
        #region Write integer to a pointer - PtrWriteInt(long MemoryAddress, int[] Offset, int Value, long size)
        /// <summary>
        /// Writes integer into a given pointer target.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The initial address.</param>
        /// <param name="Offset">int[] Offset - An integer array containing the offsets for computing the actual address to be written.</param>
        /// <param name="Value">int(Int32) - The value that is to be written to be written.</param>
        /// <param name="size">long - The byte size : Byte, 2 Bytes, 4 Bytes, 8 Bytes.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long PtrWriteInt(long MemoryAddress, int[] Offset, int Value, long size)
        {
            return WriteInt(PtrAddress(MemoryAddress, Offset), Value, (uint)size);
        }
        #endregion
        #region Write double to a pointer - PtrWriteDouble(long MemoryAddress, int[] Offset, double Value)
        /// <summary>
        /// Writes a double value into a given pointer target.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The initial address.</param>
        /// <param name="Offset">int[] Offset - An integer array containing the offsets for computing the actual address to be written.</param>
        /// <param name="Value">Double - The value that is to be written to be written.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long PtrWriteDouble(long MemoryAddress, int[] Offset, double Value)
        {
            return WriteDouble(PtrAddress(MemoryAddress, Offset), Value);
        }
        #endregion
        #region Write string to a pointer - PtrWriteString(long MemoryAddress, int[] Offset, string Value)
        /// <summary>
        /// Writes string to a pointer.
        /// </summary>
        /// <param name="MemoryAddress">MemoryAddress - The initial address.</param>
        /// <param name="Offset">int[] Offset - An integer array containing the offsets for computing the actual address to be written.</param>
        /// <param name="Value">String - The string value to write to the destination address.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long PtrWriteString(long MemoryAddress, int[] Offset, string Value)
        {
            return WriteString(PtrAddress(MemoryAddress, Offset), Value);
        }
        #endregion
        #region Create your own codecave - string MakeNewCodeCave()
        /// <summary>
        /// Create your own code cave in the selected process.
        /// <para>The minimum size of the code cave is approx. 3.7... MBytes(depends on page size).</para>
        /// </summary>
        /// <param name="sizeInMBs">Write the size of the code cave that you want to create.</param>
        /// <returns>Returns the long address of the starting point of the created code-cave.</returns>
        public long MakeNewCodeCave(long sizeInMBs = 1)
        {
            long LenWrite = sizeInMBs;
            if (LenWrite >= 3)
                LenWrite = (sizeInMBs - 2) * 1024;
            else
                LenWrite = 1;
            IntPtr AllocMem = (IntPtr)VirtualAllocEx(m_hProcess, (IntPtr)null, (uint)LenWrite, 0x1000, 0x40);
            long address = AllocMem.ToInt64();
            //VirtualFreeEx(m_hProcess, AllocMem , (UIntPtr)0, 0x8000);// requires memory reservation fixation.
            return (AllocMem.ToInt64());
        }
        #endregion
        #region NOP any address - void noper(long address)
        /// <summary>
        /// NOP at a given address.
        /// </summary>
        /// <param name="address">Int64 - The address where NOP is to be put.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long noper(long address)
        {
            byte[] asan = { 0x90 };
            return Write(address, asan);
        }
        #endregion
        #region Nop from a given address to the no. of addresses(count) - void noperTill(long address, long count)
        /// <summary>
        /// NOP a range of addresses.
        /// </summary>
        /// <param name="address">Int64 - The address starting from where NOP is to be put.</param>
        /// <param name="count">Int32 - The index position relative to the address, starting from 1, until which NOP instructions are to be inserted.</param>
        /// <returns>long(Int64) - The address of the last byte where data was written.</returns>
        public long noperTill(long address, long count)
        {
            for (long i = 0; i < count; i++)
            {
                noper(address + i);
            }
            return address + count;
        }
        #endregion
        #endregion
    }
    #endregion
    /// <summary>
    /// Functions ::-
    /// Captures keyboard key presses and releases.
    /// Requires DotNet 4.0
    /// No guarantee for any damage caused by the misuse of this class.
    /// ---Provided and re-compiled by GameMaster Greatee aka Rajarshi Vaidya.
    /// How to use ::--
    /// 1. Make an object for this class.
    /// 2. Create event-handlers for KeyUp and KeyDown, whatever you wish for, and pass the method that is to be called.
    /// 3. Run the Install function.
    /// Example
    ///                 KeyboardHook gmaster = new KeyboardHook(true, true);//first true for mouse, 2nd true for keyboard - change at will
    ///                 gmaster.KeyUp += SomeFunction1;
    ///                 gmaster.KeyDown += SomeFunction2;
    ///                 gmaster.Start();
    /// After this code segment is executed, then the keypresses are all directed to currently focussed application
    /// and to your application(trainer - method SomeFunction1 for KeyUp and SomeFunction2 for KeyDown) too.
    /// To disable, just use
    ///                 gmaster.Stop();
    /// The functions SomeFunction1 and SomeFunction2 will be passed an argument whenever an event occurs...
    ///                 public void SomeFunction1(object sender, System.Windows.Forms.KeyEventArgs e)
    ///                 {
    ///                     Something here ;
    ///                     for ex. :-
    ///                     if(e.KeyData == System.Windows.Forms.Keys.Insert) // the button for Insert key
    ///                     {
    ///                         Do this and that;
    ///                     }
    ///                 }
    ///                 
    /// Note -- Use KeyUp method for better accuracy and perfect response, as a single operation is performed per keypress.
    /// 
    ///                                                                          ---- enjoy cheating, GMaster
    /// </summary>
    #region KeyboardHook Class - Version 2.1 - by GMaster
    public class KeyboardHook
    {
        private int hKeyboardHook;
        private int hMouseHook;
        private static HookProc KeyboardHookProcedure;
        private static HookProc MouseHookProcedure;
        private const byte VK_CAPITAL = 20;
        private const byte VK_NUMLOCK = 0x90;
        private const byte VK_SHIFT = 0x10;
        private const int WH_KEYBOARD = 2;
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE = 7;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_MBUTTONUP = 520;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_MOUSEWHEEL = 0x20a;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_SYSKEYDOWN = 260;
        private const int WM_SYSKEYUP = 0x105;

        public event KeyEventHandler KeyDown;

        public event KeyPressEventHandler KeyPress;

        public event KeyEventHandler KeyUp;

        public event MouseEventHandler OnMouseActivity;

        public KeyboardHook()
        {
            this.Start();
        }

        public KeyboardHook(bool InstallMouseHook, bool InstallKeyboardHook)
        {
            this.Start(InstallMouseHook, InstallKeyboardHook);
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);
        ~KeyboardHook()
        {
            this.Stop(true, true, false);
        }

        [DllImport("user32")]
        private static extern int GetKeyboardState(byte[] pbKeyState);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern short GetKeyState(int vKey);
        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {

            if (this.hKeyboardHook != 0)
            {
                bool flag = false;
                if ((nCode >= 0) && (((this.KeyDown != null) || (this.KeyUp != null)) || (this.KeyPress != null)))
                {
                    KeyboardHookStruct struct2 = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                    if ((this.KeyDown != null) && ((wParam == 0x100) || (wParam == 260)))
                    {
                        KeyEventArgs e = new KeyEventArgs((Keys)struct2.vkCode);
                        this.KeyDown(this, e);
                        flag = flag || e.Handled;
                    }
                    if ((this.KeyPress != null) && (wParam == 0x100))
                    {
                        bool flag2 = (GetKeyState(0x10) & 0x80) == 0x80;
                        bool flag3 = GetKeyState(20) != 0;
                        byte[] pbKeyState = new byte[0x100];
                        GetKeyboardState(pbKeyState);
                        byte[] lpwTransKey = new byte[2];
                        if (ToAscii(struct2.vkCode, struct2.scanCode, pbKeyState, lpwTransKey, struct2.flags) == 1)
                        {
                            char c = (char)lpwTransKey[0];
                            if ((flag3 ^ flag2) && char.IsLetter(c))
                            {
                                c = char.ToUpper(c);
                            }
                            KeyPressEventArgs args2 = new KeyPressEventArgs(c);
                            this.KeyPress(this, args2);
                            flag = flag || args2.Handled;
                        }
                    }
                    if ((this.KeyUp != null) && ((wParam == 0x101) || (wParam == 0x105)))
                    {
                        KeyEventArgs args3 = new KeyEventArgs((Keys)struct2.vkCode);
                        this.KeyUp(this, args3);
                        flag = flag || args3.Handled;
                    }
                }
                if (flag)
                {
                    return 1;
                }
                return CallNextHookEx(this.hKeyboardHook, nCode, wParam, lParam);
            }
            else
            {
                return 0;
            }

        }

        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (this.hMouseHook != 0)
            {
                if ((nCode >= 0) && (this.OnMouseActivity != null))
                {
                    MouseLLHookStruct struct2 = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
                    MouseButtons none = MouseButtons.None;
                    short delta = 0;
                    switch (wParam)
                    {
                        case 0x201:
                            none = MouseButtons.Left;
                            break;

                        case 0x204:
                            none = MouseButtons.Right;
                            break;

                        case 0x20a:
                            delta = (short)((struct2.mouseData >> 0x10) & 0xffff);
                            break;
                    }
                    int clicks = 0;
                    if (none != MouseButtons.None)
                    {
                        if ((wParam == 0x203) || (wParam == 0x206))
                        {
                            clicks = 2;
                        }
                        else
                        {
                            clicks = 1;
                        }
                    }
                    MouseEventArgs e = new MouseEventArgs(none, clicks, struct2.pt.x, struct2.pt.y, delta);
                    this.OnMouseActivity(this, e);
                }
                return CallNextHookEx(this.hMouseHook, nCode, wParam, lParam);
            }
            else
            {
                return 0;
            }

        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);
        public void Start()
        {
            this.Start(true, true);
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void Start(bool InstallMouseHook, bool InstallKeyboardHook)
        {
            if ((this.hMouseHook == 0) && InstallMouseHook)
            {
                MouseHookProcedure = new HookProc(this.MouseHookProc);
                this.hMouseHook = SetWindowsHookEx(14, MouseHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (this.hMouseHook == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    this.Stop(true, false, false);
                    throw new Win32Exception(error);
                }
            }
            if ((this.hKeyboardHook == 0) && InstallKeyboardHook)
            {
                KeyboardHookProcedure = new HookProc(this.KeyboardHookProc);
                this.hKeyboardHook = SetWindowsHookEx(13, KeyboardHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (this.hKeyboardHook == 0)
                {
                    int num2 = Marshal.GetLastWin32Error();
                    this.Stop(false, true, false);
                    throw new Win32Exception(num2);
                }
            }
        }

        public void Stop()
        {
            this.Stop(true, true, true);
        }

        public void Stop(bool UninstallMouseHook, bool UninstallKeyboardHook, bool ThrowExceptions)
        {

            if ((this.hMouseHook != 0) && UninstallMouseHook)
            {
                int num = UnhookWindowsHookEx(1354);

                this.hMouseHook = 0;

                if ((num == 0) && ThrowExceptions)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            if ((this.hKeyboardHook != 0) && UninstallKeyboardHook)
            {
                int num3 = UnhookWindowsHookEx(4578);

                this.hKeyboardHook = 0;

                if ((num3 == 0) && ThrowExceptions)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        [DllImport("user32")]
        private static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MouseHookStruct
        {
            public KeyboardHook.POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MouseLLHookStruct
        {
            public KeyboardHook.POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class POINT
        {
            public int x;
            public int y;
        }
    }
    #endregion
    /// <summary>
    /// Coordinate System for Mouse Events and Extra Info::-
    /// ======================================================
    /// 
    /// The coordinates for mouse events are pretty peculiar.
    /// The whole screen is divided into 65,535 sections both horizontally and  vertically.
    /// 
    /// If an absolute move is required(means you want to place the cursor in a position irrespective of the last position it was)
    /// you need to do that by passing values in the range 0 - 65,535 each for x and y.
    /// 
    /// If you want to move the mouse relative to the last position then keep the following in mind ::-
    ///     for x(horizontal) axis ::
    ///         positive values = moving cursor to the right
    ///         negative values = moving cursor to the left
    ///     for y(vertical) axis ::
    ///         positive values = moving cursor down
    ///         negative values = moving cursor up
    /// 
    /// Relative mouse motion is subject to the effects of the mouse speed and the two mouse threshold(x and y) values.
    /// In Windows 95/98/NT/Vista/7/8/8.1/10, an end user sets them with the Pointer Speed slider of the Control 
    /// Panel's Mouse property sheet.
    /// 
    /// The operating system applies two tests to the specified relative mouse motion.
    /// If the specified distance along either the x or y axis is greater than the first mouse threshold value,
    /// and the mouse speed is not zero, the operating system doubles the distance.
    /// If the specified distance along either the x or y axis is greater than the second mouse threshold value,
    /// and the mouse speed is equal to two, the operating system doubles the distance that resulted from applying the first threshold test.
    /// It is thus possible for the operating system to multiply relatively-specified mouse motion along the x or y axis by up to four times.
    /// 
    ///                                                                                                 ----GameMaster Greatee aka Rajarshi Vaidya
    /// </summary>
    #region MouseHook Class - Version 1.0.2
    public class MouseHook
    {
        #region Imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        #endregion
        #region dwFlags
        public enum MouseFlags
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_HWHEEL = 0x01000,//not needed in most of the cases, so functions are not included. If you need help, ask anytime.
            MOUSEEVENTF_ABSOLUTE = 0x8000,
            WHEEL_DELTA = 0x78
        }
        #endregion
        #region Mouse_events
        #region Mouse - Primitive Functions
        #region LeftDown
        /// <summary>
        /// Set left mouse button down.
        /// </summary>
        public void LeftDown()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }
        #endregion
        #region LeftUp
        /// <summary>
        /// Set left mouse button up.
        /// </summary>
        public void LeftUp()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        #endregion
        #region RightDown
        /// <summary>
        /// Set right mouse button down.
        /// </summary>
        public void RightDown()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        }
        #endregion
        #region RightUp
        /// <summary>
        /// Set right mouse button up.
        /// </summary>
        public void RightUp()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        #endregion
        #region MiddleDown
        /// <summary>
        /// Set middle mouse button down.
        /// </summary>
        public void MiddleDown()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
        }
        #endregion
        #region MiddleUp
        /// <summary>
        /// Set middle mouse button up.
        /// </summary>
        public void MiddleUp()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }
        #endregion
        #region XDown
        /// <summary>
        /// Set X mouse button down.
        /// </summary>
        public void XDown()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_XDOWN, 0, 0, 0, 0);
        }
        #endregion
        #region XUp
        /// <summary>
        /// Set X mouse button up.
        /// </summary>
        public void XUp()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_XUP, 0, 0, 0, 0);
        }
        #endregion
        #endregion
        #region Mouse - Main Functions
        #region MoveAbsolute
        /// <summary>
        /// Move the cursor to a fixed position.
        /// </summary>
        /// <param name="x">Horizontal position(Value must be in the range :: 0-65,535)</param>
        /// <param name="y">Vertical position(Value must be in the range :: 0-65,535)</param>
        public void MoveAbsolute(int x, int y)
        {
            if ((x >= 0) && (x <= 65535) && (y >= 0) && (y <= 65535))
                mouse_event((long)MouseFlags.MOUSEEVENTF_MOVE | (long)MouseFlags.MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
            else
                throw new Exception("Please make sure that the x and y values are within the specified range :: 0-65,535");
        }
        #endregion
        #region MoveRelative
        /// <summary>
        /// Move the cursor relative to the previous position.
        /// </summary>
        /// <param name="dx">Horizontal :: +ve = right, -ve = left</param>
        /// <param name="dy">Vertical :: +ve = down, -ve = up</param>
        public void MoveRelative(int dx, int dy)
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_MOVE, dx, dy, 0, 0);
        }
        #endregion
        #region WheelAway
        /// <summary>
        /// Rotate wheel upwards, away from the user.
        /// </summary>
        public void WheelAway()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_WHEEL, 0, 0, 119, 0);
        }
        #endregion
        #region WheelNear
        /// <summary>
        /// Rotate wheel downwads, towards the user.
        /// </summary>
        public void WheelNear()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_WHEEL, 0, 0, -119, 0);
        }
        #endregion
        #region WheelClick
        /// <summary>
        /// Initiaises a Wheel click.
        /// </summary>
        public void WheelClick()
        {
            mouse_event((long)MouseFlags.MOUSEEVENTF_WHEEL, 0, 0, 0, (long)MouseFlags.WHEEL_DELTA);
        }
        #endregion
        #region LeftClick
        /// <summary>
        /// Initialises a left mouse button click.
        /// </summary>
        public void LeftClick()
        {
            LeftDown();
            LeftUp();
        }
        #endregion
        #region RightClick
        /// <summary>
        /// Initialises a Right mouse button click.
        /// </summary>
        public void RightClick()
        {
            RightDown();
            RightUp();
        }
        #endregion
        #region MiddleClick
        /// <summary>
        /// Initialises a Middle mouse button click.
        /// </summary>
        public void MiddleClick()
        {
            MiddleDown();
            MiddleUp();
        }
        #endregion
        #region XClick
        /// <summary>
        /// Initialises a X mouse button click.
        /// </summary>
        public void XClick()
        {
            XDown();
            XUp();
        }
        #endregion
        #region DLeftClick
        /// <summary>
        /// Make a double left click.
        /// </summary>
        public void DLeftClick()
        {
            LeftClick();
            LeftClick();
        }
        #endregion
        #region DRightClick
        /// <summary>
        /// Make a double right click.
        /// </summary>
        public void DRightClick()
        {
            RightClick();
            RightClick();
        }
        #endregion
        #region DMiddleClick
        /// <summary>
        /// Make a double middle click.
        /// </summary>
        public void DMiddleClick()
        {
            MiddleClick();
            MiddleClick();
        }
        #endregion
        #region DWheelClick
        /// <summary>
        /// Make a double wheel click.
        /// </summary>
        public void DWheelClick()
        {
            WheelClick();
            WheelClick();
        }
        #endregion
        #region DXClick
        /// <summary>
        /// Make a double X mouse button click.
        /// </summary>
        public void DXClick()
        {
            XClick();
            XClick();
        }
        #endregion
        #endregion
        #endregion
    }
    #endregion
    /// Autologger
    /// 
    /// 
    /// 
    /// AutoLog - dated 03-Sunday-May-05-2015
    #endregion
}