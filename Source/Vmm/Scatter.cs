using DMAW_DND;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace vmmsharp
{
    
        public class ScatterReadMap
        {
            protected List<ScatterReadRound> Rounds { get; } = new();
            protected readonly Dictionary<int, Dictionary<int, IScatterEntry>> _results = new();
            /// <summary>
            /// Contains results from Scatter Read after Execute() is performed. First key is Index, Second Key ID.
            /// </summary>
            public IReadOnlyDictionary<int, Dictionary<int, IScatterEntry>> Results => _results;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="indexCount">Number of indexes in the scatter read loop.</param>
            public ScatterReadMap(int indexCount)
            {
                for (int i = 0; i < indexCount; i++)
                {
                    _results.Add(i, new());
                }
            }

            /// <summary>
            /// Executes Scatter Read operation as defined per the map.
            /// </summary>
            public void Execute(Vmm mem)
            {
                foreach (var round in Rounds)
                {
                    round.Run(mem);
                }
            }

            /// <summary>
            /// (Base)
            /// Add scatter read rounds to the operation. Each round is a successive scatter read, you may need multiple
            /// rounds if you have reads dependent on earlier scatter reads result(s).
            /// </summary>
            /// <param name="pid">Process ID to read from.</param>
            /// <param name="useCache">Use caching for this read (recommended).</param>
            /// <returns></returns>
            public virtual ScatterReadRound AddRound(uint pid, bool useCache = true)
            {
                var round = new ScatterReadRound(pid, _results, useCache);
                Rounds.Add(round);
                return round;
            }
        }

        public class ScatterReadRound
        {
            private const ulong PAGE_SIZE = 0x1000;
            private const int PAGE_SHIFT = 12;

            /// <summary>
            /// The PAGE_ALIGN macro takes a virtual address and returns a page-aligned
            /// virtual address for that page.
            /// </summary>
            private static ulong PAGE_ALIGN(ulong va)
            {
                return (va & ~(PAGE_SIZE - 1));
            }
            /// <summary>
            /// The ADDRESS_AND_SIZE_TO_SPAN_PAGES macro takes a virtual address and size and returns the number of pages spanned by the size.
            /// </summary>
            private static uint ADDRESS_AND_SIZE_TO_SPAN_PAGES(ulong va, uint size)
            {
                return (uint)((BYTE_OFFSET(va) + (size) + (PAGE_SIZE - 1)) >> PAGE_SHIFT);
            }

            /// <summary>
            /// The BYTE_OFFSET macro takes a virtual address and returns the byte offset
            /// of that address within the page.
            /// </summary>
            private static uint BYTE_OFFSET(ulong va)
            {
                return (uint)(va & (PAGE_SIZE - 1));
            }

            private readonly uint _pid;
            private readonly bool _useCache;
            protected Dictionary<int, Dictionary<int, IScatterEntry>> Results { get; }
            protected List<IScatterEntry> Entries { get; } = new();

            /// <summary>
            /// Do not use this constructor directly. Call .AddRound() from the ScatterReadMap.
            /// </summary>
            public ScatterReadRound(uint pid, Dictionary<int, Dictionary<int, IScatterEntry>> results, bool useCache)
            {
                _pid = pid;
                Results = results;
                _useCache = useCache;
            }

            /// <summary>
            /// (Base)
            /// Adds a single Scatter Read 
            /// </summary>
            /// <param name="index">For loop index this is associated with.</param>
            /// <param name="id">Random ID number to identify the entry's purpose.</param>
            /// <param name="addr">Address to read from (you can pass a ScatterReadEntry from an earlier round, 
            /// and it will use the result).</param>
            /// <param name="size">Size of oject to read (ONLY for reference types, value types get size from
            /// Type). You canc pass a ScatterReadEntry from an earlier round and it will use the Result.</param>
            /// <param name="offset">Optional offset to add to address (usually in the event that you pass a
            /// ScatterReadEntry to the Addr field).</param>
            /// <returns>The newly created ScatterReadEntry.</returns>
            public virtual ScatterReadEntry<T> AddEntry<T>(int index, int id, object addr, object size = null, uint offset = 0x0)
            {
                var entry = new ScatterReadEntry<T>()
                {
                    Index = index,
                    Id = id,
                    Addr = addr,
                    Size = size,
                    Offset = offset
                };
                Results[index].Add(id, entry);
                Entries.Add(entry);
                return entry;
            }

            /// <summary>
            /// ** Internal API use only do not use **
            /// </summary>
            internal void Run(Vmm mem)
            {
                var pagesToRead = new HashSet<ulong>(); // Will contain each unique page only once to prevent reading the same page multiple times
                foreach (var entry in Entries) // First loop through all entries - GET INFO
                {
                    // Parse Address and Size properties
                    ulong addr = entry.ParseAddr();
                    uint size = (uint)entry.ParseSize();

                    // INTEGRITY CHECK - Make sure the read is valid
                    if (addr == 0x0 || size == 0)
                    {
                        entry.IsFailed = true;
                        continue;
                    }
                    // location of object
                    ulong readAddress = addr + entry.Offset;
                    // get the number of pages
                    uint numPages = ADDRESS_AND_SIZE_TO_SPAN_PAGES(readAddress, size);
                    ulong basePage = PAGE_ALIGN(readAddress);

                    //loop all the pages we would need
                    for (int p = 0; p < numPages; p++)
                    {
                        ulong page = basePage + PAGE_SIZE * (uint)p;
                        pagesToRead.Add(page);
                    }
                }
                var results = mem.MemReadScatter(_pid, Vmm.FLAG_NOCACHE, Entries, pagesToRead.ToArray());

                foreach (var entry in Entries) // Second loop through all entries - PARSE RESULTS
                {
                    if (entry.IsFailed) // Skip this entry, leaves result as null
                        continue;

                    ulong readAddress = (ulong)entry.Addr + entry.Offset; // location of object
                    uint pageOffset = BYTE_OFFSET(readAddress); // Get object offset from the page start address

                    uint size = (uint)(int)entry.Size;
                    var buffer = new byte[size]; // Alloc result buffer on heap
                    int bytesCopied = 0; // track number of bytes copied to ensure nothing is missed
                    uint cb = Math.Min(size, (uint)PAGE_SIZE - pageOffset); // bytes to read this page

                    uint numPages = ADDRESS_AND_SIZE_TO_SPAN_PAGES(readAddress, size); // number of pages to read from (in case result spans multiple pages)
                    ulong basePage = PAGE_ALIGN(readAddress);

                    for (int p = 0; p < numPages; p++)
                    {
                        ulong page = basePage + PAGE_SIZE * (uint)p; // get current page addr
                        var scatter = results.FirstOrDefault(x => x.qwA == page); // retrieve page of mem needed
                        if (scatter.f) // read succeeded -> copy to buffer
                        {
                            scatter.pb
                                .AsSpan((int)pageOffset, (int)cb)
                                .CopyTo(buffer.AsSpan(bytesCopied, (int)cb)); // Copy bytes to buffer
                            bytesCopied += (int)cb;
                        }
                        else // read failed -> set failed flag
                        {
                            entry.IsFailed = true;
                            break;
                        }

                        cb = (uint)PAGE_SIZE; // set bytes to read next page
                        if (bytesCopied + cb > size) // partial chunk last page
                            cb = size - (uint)bytesCopied;

                        pageOffset = 0x0; // Next page (if any) should start at 0x0
                    }
                    if (bytesCopied != size)
                        entry.IsFailed = true;
                    entry.SetResult(buffer);
                }
            }
        }

        public class ScatterReadEntry<T> : IScatterEntry
        {
            #region Properties

            /// <summary>
            /// Entry Index.
            /// </summary>
            public int Index { get; init; }
            /// <summary>
            /// Entry ID.
            /// </summary>
            public int Id { get; init; }
            /// <summary>
            /// Can be a ulong or another ScatterReadEntry.
            /// </summary>
            public object Addr { get; set; }
            /// <summary>
            /// Offset to the Base Address.
            /// </summary>
            public uint Offset { get; init; }
            /// <summary>
            /// Defines the type based on <typeparamref name="T"/>
            /// </summary>
            public Type Type { get; } = typeof(T);
            /// <summary>
            /// Can be an int32 or another ScatterReadEntry.
            /// </summary>
            public object Size { get; set; }
            /// <summary>
            /// True if the Scatter Read has failed.
            /// </summary>
            public bool IsFailed { get; set; }
            /// <summary>
            /// Scatter Read Result.
            /// </summary>
            protected T Result { get; set; }
            #endregion

            #region Read Prep
            /// <summary>
            /// Parses the address to read for this Scatter Read.
            /// Sets the Addr property for the object.
            /// </summary>
            /// <returns>Virtual address to read.</returns>
            public ulong ParseAddr()
            {
                ulong addr = 0x0;
                if (this.Addr is ulong p1)
                    addr = p1;
                else if (this.Addr is MemPointer p2)
                    addr = p2;
                else if (this.Addr is IScatterEntry ptrObj) // Check if the addr references another ScatterRead Result
                {
                    if (ptrObj.TryGetResult<MemPointer>(out var p3))
                        addr = p3;
                    else
                        ptrObj.TryGetResult(out addr);
                }
                this.Addr = addr;
                return addr;
            }

            /// <summary>
            /// (Base)
            /// Parses the number of bytes to read for this Scatter Read.
            /// Sets the Size property for the object.
            /// Derived classes should call upon this Base.
            /// </summary>
            /// <returns>Size of read.</returns>
            public virtual int ParseSize()
            {
                int size = 0;
                if (this.Type.IsValueType)
                    size = Unsafe.SizeOf<T>();
                else if (this.Size is int sizeInt)
                    size = sizeInt;
                else if (this.Size is IScatterEntry sizeObj) // Check if the size references another ScatterRead Result
                    sizeObj.TryGetResult(out size);
                this.Size = size;
                return size;
            }
            #endregion

            #region Set Result
            /// <summary>
            /// Sets the Result for this Scatter Read.
            /// </summary>
            /// <param name="buffer">Raw memory buffer for this read.</param>
            public void SetResult(byte[] buffer)
            {
                try
                {
                    if (IsFailed)
                        return;
                    if (Type.IsValueType) /// Value Type
                        SetValueResult(buffer);
                    else /// Ref Type
                        SetClassResult(buffer);
                }
                catch
                {
                    IsFailed = true;
                }
            }

            /// <summary>
            /// Set the Result from a Value Type.
            /// </summary>
            /// <param name="buffer">Raw memory buffer for this read.</param>
            private void SetValueResult(byte[] buffer)
            {
                if (buffer.Length != Unsafe.SizeOf<T>()) // Safety Check
                    throw new ArgumentOutOfRangeException(nameof(buffer));
                Result = Unsafe.As<byte, T>(ref buffer[0]);
                if (Result is MemPointer memPtrResult)
                    memPtrResult.Validate();
            }

            /// <summary>
            /// (Base)
            /// Set the Result from a Class Type.
            /// Derived classes should call upon this Base.
            /// </summary>
            /// <param name="buffer">Raw memory buffer for this read.</param>
            protected virtual void SetClassResult(byte[] buffer)
            {
                if (Type == typeof(string))
                {
                    var value = Encoding.Default.GetString(buffer).Split('\0')[0];
                    if (value is T result) // We already know the Types match, this is to satisfy the compiler
                        Result = result;
                }
                else
                    throw new NotImplementedException(nameof(Type));
            }
            #endregion

            #region Get Result
            /// <summary>
            /// Tries to return the Scatter Read Result.
            /// </summary>
            /// <typeparam name="TOut">Type to return.</typeparam>
            /// <param name="result">Result to populate.</param>
            /// <returns>True if successful, otherwise False.</returns>
            public bool TryGetResult<TOut>(out TOut result)
            {
                try
                {
                    if (!IsFailed && Result is TOut tResult)
                    {
                        result = tResult;
                        return true;
                    }
                    result = default;
                    return false;
                }
                catch
                {
                    result = default;
                    return false;
                }
            }
        #endregion
        }

        public readonly struct MemPointer
        {
            public static implicit operator MemPointer(ulong x) => x;
            public static implicit operator ulong(MemPointer x) => x.Va;
            /// <summary>
            /// Virtual Address of this Pointer.
            /// </summary>
            public readonly ulong Va;

            /// <summary>
            /// Validates the Pointer.
            /// </summary>
            /// <exception cref="NullPtrException"></exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Validate()
            {
                if (Va == 0x0)
                    throw new NullPtrException();
            }

            /// <summary>
            /// Convert to string format.
            /// </summary>
            /// <returns>Pointer Address represented in Upper-Case Hex.</returns>
            public readonly override string ToString() => Va.ToString("X");
        }

}