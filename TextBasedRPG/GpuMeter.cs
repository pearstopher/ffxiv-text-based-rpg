using System;
using System.Runtime.InteropServices;

namespace TextBasedRPG;

/// <summary>
/// Measures this process's average GPU use over a period, from Windows' "GPU Engine" performance counters (the same
/// data Task Manager's per-process GPU column uses), read through PDH. Used only by the measurement self-tests.
/// </summary>
/// <remarks>
/// <para>
/// One wildcard counter, <c>\GPU Engine(pid_N_*engtype_3D)\Utilization Percentage</c>, added by its English name so it
/// works on any Windows language. The counter is a rate: <see cref="Start"/> and <see cref="Stop"/> each take one
/// sample, and PDH gives the average between them, so there is no per-frame work. With several 3D engines (several
/// GPUs), the busiest one is reported, like Task Manager.
/// </para>
/// <para>
/// Needs Windows 10 1709+ with a WDDM 2.x driver. Elsewhere (e.g. Wine/Proton) <see cref="TryCreate"/> or
/// <see cref="Stop"/> returns null and the tests show "GPU n/a". PDH declarations follow the Windows SDK's pdh.h.
/// </para>
/// </remarks>
internal sealed partial class GpuMeter : IDisposable
{
    private const uint PdhFmtDouble = 0x00000200;
    private const uint PdhFmtNoCap100 = 0x00008000;
    private const uint PdhMoreData = 0x800007D2;
    private const uint PdhCstatusValidData = 0;
    private const uint PdhCstatusNewData = 1;

    private nint query;
    private nint counter;
    private bool started;

    private GpuMeter(nint query, nint counter)
    {
        this.query = query;
        this.counter = counter;
    }

    /// <summary>Opens the counter for this process, or returns null (with the reason) if it isn't available.</summary>
    internal static GpuMeter? TryCreate(out string? error)
    {
        error = null;
        try
        {
            if (PdhOpenQueryW(null, 0, out var query) != 0)
            {
                error = "PDH query could not be opened.";
                return null;
            }

            var path = $@"\GPU Engine(pid_{Environment.ProcessId}_*engtype_3D)\Utilization Percentage";
            var status = PdhAddEnglishCounterW(query, path, 0, out var counter);
            if (status != 0)
            {
                PdhCloseQuery(query);
                error = $"GPU counter not available (PDH 0x{status:X8}).";
                return null;
            }

            return new GpuMeter(query, counter);
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
            error = "PDH is not available.";
            return null;
        }
    }

    /// <summary>Starts a period (takes the first sample).</summary>
    internal void Start() => this.started = this.query != 0 && PdhCollectQueryData(this.query) == 0;

    /// <summary>Ends the period: the average GPU % since <see cref="Start"/>, or null if it couldn't be read.</summary>
    internal double? Stop()
    {
        if (!this.started || PdhCollectQueryData(this.query) != 0)
            return null;
        this.started = false;
        return this.ReadBusiestEngine();
    }

    private unsafe double? ReadBusiestEngine()
    {
        uint size = 0;
        if (PdhGetFormattedCounterArrayW(this.counter, PdhFmtDouble | PdhFmtNoCap100, ref size, out var count, null) != PdhMoreData || size == 0)
            return null;

        var buffer = new byte[size];
        fixed (byte* items = buffer)
        {
            if (PdhGetFormattedCounterArrayW(this.counter, PdhFmtDouble | PdhFmtNoCap100, ref size, out count, items) != 0)
                return null;

            double? busiest = null;
            var item = (CounterValueItem*)items;
            for (var i = 0; i < count; i++)
            {
                if (item[i].CStatus is PdhCstatusValidData or PdhCstatusNewData)
                    busiest = Math.Max(busiest ?? 0, item[i].Value);
            }

            return busiest is { } b ? Math.Clamp(b, 0, 100) : null;
        }
    }

    public void Dispose()
    {
        if (this.query != 0)
            PdhCloseQuery(this.query);
        this.query = 0;
        this.counter = 0;
    }

    /// <summary>PDH_FMT_COUNTERVALUE_ITEM_W with a PDH_FMT_DOUBLE value (x64 layout).</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct CounterValueItem
    {
        public nint Name;
        public uint CStatus;
        public double Value;
    }

    [LibraryImport("pdh.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint PdhOpenQueryW(string? dataSource, nint userData, out nint query);

    [LibraryImport("pdh.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint PdhAddEnglishCounterW(nint query, string fullCounterPath, nint userData, out nint counter);

    [LibraryImport("pdh.dll")]
    private static partial uint PdhCollectQueryData(nint query);

    [LibraryImport("pdh.dll")]
    private static unsafe partial uint PdhGetFormattedCounterArrayW(nint counter, uint format, ref uint bufferSize, out uint itemCount, byte* itemBuffer);

    [LibraryImport("pdh.dll")]
    private static partial uint PdhCloseQuery(nint query);
}
