using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using vmmsharp;

using static vmmsharp.Vmm;

namespace DMAW_DND
{
    internal static class Memory
    {
        private static Vmm _mem;
        private static readonly Thread _worker;
        private static bool _ready;
        private static bool _running;

        private static uint _pid;
        private static ulong _moduleBase;

        public static Enums.GameStatus GameStatus = Enums.GameStatus.NotFound;
        private static Game _game;
        public static bool Ready
        {
            get => _ready;
        }
        public static uint PID
        {
            get => _pid;
        }
        public static Vmm Mem
        {
            get => _mem;
        }
        public static Game game
        {
            get => _game;
        }
        private static Dictionary<int, string> FNameCache = new Dictionary<int, string>(); //FNameCache
        public static Dictionary<int, Player> Players
        {
            get
            {
                if(_game is null)
                {
                    return new Dictionary<int, Player>();
                }
                return _game.Players;
            }
        }
        public static bool InGame
        {
            get
            {
                Game game = Memory._game;
                return game != null && game.InGame;
            }
        }
        public static ulong ModuleBase
        {
            get => _moduleBase;
        }
        public static Vector3 LocalPlayerLocation 
        { 
            get => _game.LocalPlayerLocation;
        }
        //public static ulong EngineBase
        //{
        //    get => _mem.GetModuleBase(_pid, "engine2.dll");
        //}
        static Memory() // Constructor
        {
            Program.Log($"[INIT] Starting up memory thread");
            try
            {
                //var args = new string[5] { "-printf", "-v", "-device", "fpga", "-waitinitialize" };
                var args = new string[4] { "-v", "-device", "fpga", "-waitinitialize" };

                _mem = new Vmm(args);
                if (_mem is not null)
                {
                    _worker = new Thread(() => Worker())
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.AboveNormal
                    };
                    Memory._running = true;
                    Memory._worker.Start();

                    //_ready = true;
                    Program.Log($"[INIT] DMA Initialised");
                }
            }
            catch (Exception ex)
            {
                Program.Log($"An error occurred during initialization: {ex.Message}");
                _worker?.Interrupt();
                Environment.Exit(-1);
            }
        }

        private static void Worker()
        {
            for (;;)
            {
                Program.Log("[DMA] Finding Process");
                while (true)
                {
                    if (GetPid() && GetModuleBase())
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(15000);
                    }
                }
                Program.Log("[DMA] Found Process");

                while(true)
                {
                    _game  = new Game();
                    try
                    {
                        GameStatus = Enums.GameStatus.Menu;
                        _ready = true;
                        //GameStatus = Enums.GameStatus.InGame;
                        _game.WaitForNewGame();
                        _game.MapReadLoop();
                        while(GameStatus == Enums.GameStatus.InGame)
                        {
                            _game.GameLoop();
                            Thread.SpinWait(125000);
                        }
                        GameStatus = Enums.GameStatus.Menu;
                    }
                    catch (ThreadInterruptedException) { throw; }
                    catch (DMAShutdown) { throw; }
                    catch (Exception ex)
                    {
                        Program.Log($"ERROR: {ex}");
                        _ready = false;
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private static bool GetPid()
        {
            try
            {
                ThrowIfDMAShutdown();
                /*// retrieve all PIDs in the system as a sorted list.
                uint[] dwPidAll = vmm.PidList();*/
                //get pidlist
                uint[] dwPidAll = _mem.PidList();
                uint largestPid = 0;
                uint largestSize = 0;
                for (int i = 0; i < dwPidAll.Length; i++)
                {
                    PROCESS_INFORMATION pi = _mem.ProcessGetInformation(dwPidAll[i]);
                    if (pi.szNameLong == "DungeonCrawler.exe")
                    {
                        var sections = _mem.ProcessGetSections(dwPidAll[i], pi.szNameLong);
                        uint currentSize = 0;
                        foreach (var section in sections)
                        {
                            currentSize += section.MiscPhysicalAddressOrVirtualSize;
                        }
                        if (currentSize > largestSize)
                        {
                            largestSize = currentSize;
                            largestPid = dwPidAll[i];
                        }
                    }
                }
                if (largestPid == 0)
                {
                    return false;
                }
                else
                {
                    _pid = largestPid;
                    Program.Log($"Largest Pid name: {_mem.ProcessGetInformation(largestPid).szNameLong} and id: {_pid}");
                    return true;
                }
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting PID: {ex}");
                return false;
            }
        }

        private static bool GetModuleBase()
        {
            try
            {
                ThrowIfDMAShutdown();
                _moduleBase = _mem.ProcessGetModuleBase(_pid, "DungeonCrawler.exe");
                if (_moduleBase == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting ModuleBase: {ex}");
                return false;
            }
        }

        public static string ReadNullTerminatedString(ulong address, int maxLength)
        {
            List<byte> byteList = new List<byte>();

            for (int i = 0; i < maxLength; i++)
            {
                byte currentByte = Memory.ReadValue<byte>(address + (ulong)i);
                if (currentByte == 0) // null-terminator
                    break;

                byteList.Add(currentByte);
            }

            return Encoding.Unicode.GetString(byteList.ToArray());
        }

        public static ulong ReadPtr(ulong ptr, bool useCache = false)
        {
            try
            {
                return ReadValue<ulong>(ptr);
            }
            catch
            {
                return 0;
            }
        }

        public static ulong ReadPtrChain(ulong ptr, uint[] offsets)
        {
            ulong addr = 0;
            try { addr = ReadPtr(ptr + offsets[0]); }
            catch (Exception ex) { throw new DMAException($"ERROR reading pointer chain at index 0, addr 0x{ptr.ToString("X")} + 0x{offsets[0].ToString("X")}", ex); }
            for (int i = 1; i < offsets.Length; i++)
            {
                try { addr = ReadPtr(addr + offsets[i]); }
                catch (Exception ex) { throw new DMAException($"ERROR reading pointer chain at index {i}, addr 0x{addr.ToString("X")} + 0x{offsets[i].ToString("X")}", ex); }
            }
            return addr;
        }

        public static T ReadUnmanagedStruct<T>(ulong adr) where T : unmanaged
        {
            T output = default;
            output = ReadValue<T>(adr);
            return output;
        }

        public static ViewMatrix ReadViewMatrix(ulong ptr)
        {
            var matrix = new ViewMatrix(4, 4);
            for (int i = 0; i < 4; i++)
            {
                //matrix[i, 0] = new float[4];
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = ReadValue<float>(ptr + (ulong)((i * 4 + j) * 4));
                }
            }

            return matrix;
        }

        public static T ReadValue<T>(ulong addr)
            where T : struct
        {
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ThrowIfDMAShutdown();
                var buf = _mem.MemRead(_pid, addr, (uint)size, Vmm.FLAG_NOCACHE);
                return MemoryMarshal.Read<T>(buf);
            }
            catch (Exception ex)
            {
                throw new DMAException($"ERROR reading {typeof(T)} value at 0x{addr.ToString("X")}", ex);
            }
        }

        private const ulong PAGE_SIZE = 0x1000;
        private const int PAGE_SHIFT = 12;

        public static string ReadString(ulong addr, uint length) // read n bytes (string)
        {
            try
            {
                if (length > PAGE_SIZE) throw new DMAException("String length outside expected bounds!");
                ThrowIfDMAShutdown();
                var buf = _mem.MemRead(_pid, addr, length, Vmm.FLAG_NOCACHE);
                return Encoding.Default.GetString(buf).Split('\0')[0];
            }
            catch (Exception ex)
            {
                throw new DMAException($"ERROR reading string at 0x{addr.ToString("X")}", ex);
            }
        }

        //public static string ReadCSString(ulong addr, int size)
        //{
        //    byte[] bstr = ReadBuffer(addr, size);
        //    int length = 0;
        //    for (int i = 0; i < bstr.Length; i++)
        //    {
        //        if (bstr[i] == 0)
        //        {
        //            break;
        //        }
        //        length++;
        //    }
        //    byte[] str = new byte[length];
        //    for (int i = 0; i < length; i++)
        //    {
        //        str[i] = bstr[i];
        //    }
        //    return Encoding.UTF8.GetString(str);
        //}

        //public static Span<byte> ReadBuffer(ulong addr, int size)
        //{
        //    return _mem.Read(_pid, addr, size, false);
        //}


        public static ulong GetModuleBase(string moduleName)
        {
            return _mem.ProcessGetModuleBase(_pid, moduleName);
        }

        private static void ThrowIfDMAShutdown()
        {
            if (!_running) throw new DMAShutdown("Memory Thread/DMA is shutting down!");
        }

        public static ulong PatternScan(string pattern) // 48 8B 05 ?? ?? ?? ?? 45 ?? ?? ?? ?? 48 8B 48 08 48 85 C9 74 07
        {
            ulong[] results = _mem.MemSearch1(_pid, Encoding.ASCII.GetBytes(pattern), 0, 0x7fffffffffff, 1, Vmm.FLAG_NOCACHE, Encoding.ASCII.GetBytes("??"));
            Console.WriteLine($"Found {results.Length} results");
            
            for(int i = 0; i < results.Length; i++)
            {
                Console.WriteLine($"Result {i}: 0x{results[i].ToString("X")}");
            }

            return 0;
        }
        public static Span<byte> ReadBuffer(ulong addr, int size)
        {
            try
            {
                uint flags = Vmm.FLAG_NOCACHE;
                byte[] array = _mem.MemRead(_pid, addr, (uint)size, flags);
                if (array.Length != size)
                {
                    throw new DMAException("Incomplete memory read!");
                }

                return array;
            }
            catch (Exception inner)
            {
                throw new DMAException("[DMA] ERROR reading buffer at 0x" + addr.ToString("X"), inner);
            }
        }
        public static string ReadName(uint nameIndex)
        {
            if (FNameCache.ContainsKey((int)nameIndex))
            {
                return FNameCache[(int)nameIndex];
            }

            try
            {
                uint block = nameIndex >> 0x10;
                var offset = nameIndex & 0xFFFF;

                ulong FNamePool = ModuleBase + Offsets.GNames + 0x10;
                ulong NamePoolChunk = ReadValue<ulong>(FNamePool + block * 0x8);
                ulong FNameEntry = NamePoolChunk + 0x2 * offset;

                //short FNameEntryHeader = ReadUnmanagedStruct<short>(FNameEntry);
                short FNameEntryHeader = ReadValue<short>(FNameEntry);
                var strLength = FNameEntryHeader >> 0x6;

                var fNameString = ReadUEString(FNameEntry + 0x2, (uint)strLength);
                FNameCache.Add((int)nameIndex, fNameString);
                return fNameString;
            }
            catch
            {
                return null;
            }
        }
        public static string ReadUEString(ulong adr, uint size)
        {
            if (size != 0 && size < 100)
            {
                //byte[] output = new byte[size];
                var output = ReadBuffer(adr, (int)size);
                return Encoding.UTF8.GetString(output);
            }
            return null;
        }
        public static string ReadFString(FString fString)
        {
            int length = fString.Count;
            if (length <= 0)
                return "";

            // Calculate the number of bytes needed (2 bytes per character for UTF-16)
            int size = length * 2;

            // Read the byte buffer from memory
            Span<byte> buffer = ReadBuffer((ulong)fString.Data, size);

            // Convert from UTF-16 bytes to string
            return Encoding.Unicode.GetString(buffer.ToArray()).TrimEnd('\0');
        }

       
    }

}