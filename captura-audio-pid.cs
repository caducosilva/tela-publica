// captura-audio-pid: PCM s16le 48kHz stereo via TCP, so do processo (WASAPI process loopback).
// Uso: captura-audio-pid.exe <pid> include|exclude <porta>
// Windows 10 2004+ / Windows 11. Compilado sob demanda pelo tela-publica.

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

class Program {
  const string VAD = "VAD\\Process_Loopback";
  static readonly Guid IID_IAudioClient = new Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2");
  static readonly Guid IID_IAudioCaptureClient = new Guid("C8ADBD64-E71E-48A0-A4DE-185C395CD317");

  [StructLayout(LayoutKind.Sequential)]
  struct AudioClientProcessLoopbackParams {
    public uint TargetProcessId;
    public uint ProcessLoopbackMode;
  }

  [StructLayout(LayoutKind.Sequential)]
  struct AudioClientActivationParams {
    public uint ActivationType;
    public AudioClientProcessLoopbackParams ProcessLoopbackParams;
  }

  [StructLayout(LayoutKind.Sequential)]
  struct Blob {
    public uint cbSize;
    public IntPtr pBlobData;
  }

  [StructLayout(LayoutKind.Explicit, Size = 24)]
  struct PropVariant {
    [FieldOffset(0)] public ushort vt;
    [FieldOffset(8)] public Blob blob;
  }

  [StructLayout(LayoutKind.Sequential)]
  struct WaveFormatEx {
    public ushort wFormatTag, nChannels;
    public uint nSamplesPerSec, nAvgBytesPerSec;
    public ushort nBlockAlign, wBitsPerSample, cbSize;
  }

  [ComImport, Guid("41D949AB-9862-444A-80F6-C261334DA5EB"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IActivateAudioInterfaceCompletionHandler {
    void ActivateCompleted(
      [MarshalAs(UnmanagedType.Interface)] IActivateAudioInterfaceAsyncOperation activateOperation);
  }

  [ComImport, Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IActivateAudioInterfaceAsyncOperation {
    void GetActivateResult(
      out int activateResult,
      [MarshalAs(UnmanagedType.IUnknown)] out object activatedInterface);
  }

  [ComImport, Guid("94EA2B94-E9CC-49E0-C0FF-EE64CA8F5B90"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAgileObject { }

  [DllImport("ole32.dll")]
  static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

  [DllImport("Mmdevapi.dll", ExactSpelling = true, PreserveSig = true, CharSet = CharSet.Unicode)]
  static extern int ActivateAudioInterfaceAsync(
    string deviceInterfacePath,
    ref Guid riid,
    IntPtr activationParams,
    IActivateAudioInterfaceCompletionHandler completionHandler,
    out IActivateAudioInterfaceAsyncOperation activationOperation);

  [ComImport, Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAudioClient {
    [PreserveSig]
    int Initialize(int shareMode, int streamFlags, long bufferDuration,
                   long periodicity, IntPtr format, IntPtr session);
    void GetBufferSize(out uint bufferSize);
    void GetStreamLatency(out long latency);
    void GetCurrentPadding(out uint padding);
    void IsFormatSupported(int shareMode, IntPtr format, out IntPtr closest);
    void GetMixFormat(out IntPtr format);
    void GetDevicePeriod(out long def, out long min);
    void Start();
    void Stop();
    void Reset();
    void SetEventHandle(IntPtr handle);
    void GetService(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object service);
  }

  [ComImport, Guid("C8ADBD64-E71E-48A0-A4DE-185C395CD317"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAudioCaptureClient {
    void GetBuffer(out IntPtr data, out uint numFrames, out int flags,
                   out ulong devicePos, out ulong qpcPos);
    void ReleaseBuffer(uint numFrames);
    void GetNextPacketSize(out uint numFrames);
  }

  [ComVisible(true)]
  class Handler : IActivateAudioInterfaceCompletionHandler, IAgileObject {
    public ManualResetEvent Done = new ManualResetEvent(false);
    public int Hr;
    public object Client;

    public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation op) {
      try {
        object iface;
        int hr;
        op.GetActivateResult(out hr, out iface);
        Hr = hr;
        Client = iface;
      } catch (Exception e) {
        Console.Error.WriteLine("activate err " + e.Message);
        Hr = unchecked((int)0x80004005);
      }
      Done.Set();
    }
  }

  static int Run(uint pid, bool include, int port) {
    CoInitializeEx(IntPtr.Zero, 0);

    var act = new AudioClientActivationParams {
      ActivationType = 1,
      ProcessLoopbackParams = new AudioClientProcessLoopbackParams {
        TargetProcessId = pid,
        ProcessLoopbackMode = include ? 0u : 1u
      }
    };
    int actSize = Marshal.SizeOf(typeof(AudioClientActivationParams));
    IntPtr blob = Marshal.AllocHGlobal(actSize);
    Marshal.StructureToPtr(act, blob, false);
    var pv = new PropVariant {
      vt = 0x0041,
      blob = new Blob { cbSize = (uint)actSize, pBlobData = blob }
    };
    IntPtr ppv = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PropVariant)));
    Marshal.StructureToPtr(pv, ppv, false);

    var handler = new Handler();
    var iid = IID_IAudioClient;
    IActivateAudioInterfaceAsyncOperation op;
    int hr = ActivateAudioInterfaceAsync(VAD, ref iid, ppv, handler, out op);
    if (hr != 0) {
      Console.Error.WriteLine("ActivateAudioInterfaceAsync 0x{0:X8}", hr);
      return 1;
    }
    if (!handler.Done.WaitOne(10000)) {
      Console.Error.WriteLine("timeout");
      return 1;
    }
    if (handler.Hr != 0 || handler.Client == null) {
      Console.Error.WriteLine("activate result 0x{0:X8}", handler.Hr);
      return 1;
    }

    var client = (IAudioClient)handler.Client;
    var wfx = new WaveFormatEx {
      wFormatTag = 1,
      nChannels = 2,
      nSamplesPerSec = 48000,
      wBitsPerSample = 16,
      nBlockAlign = 4,
      nAvgBytesPerSec = 192000,
      cbSize = 0
    };
    IntPtr pWfx = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WaveFormatEx)));
    Marshal.StructureToPtr(wfx, pWfx, false);

    // LOOPBACK | AUTOCONVERTPCM | SRC_DEFAULT_QUALITY
    int flags = unchecked((int)(0x00020000u | 0x80000000u | 0x08000000u));
    hr = client.Initialize(0, flags, 0, 0, pWfx, IntPtr.Zero);
    if (hr != 0) {
      hr = client.Initialize(0, 0x00020000, 0, 0, pWfx, IntPtr.Zero);
      if (hr != 0) {
        Console.Error.WriteLine("Initialize failed 0x{0:X8}", hr);
        return 1;
      }
    }

    object svc;
    var iidCap = IID_IAudioCaptureClient;
    client.GetService(ref iidCap, out svc);
    var cap = (IAudioCaptureClient)svc;
    client.Start();

    var listener = new TcpListener(IPAddress.Loopback, port);
    listener.Start();
    Console.Error.WriteLine("READY " + port);
    Console.Error.Flush();

    try {
      using (var sock = listener.AcceptTcpClient())
      using (var stream = sock.GetStream()) {
        byte[] silence = new byte[48000 / 50 * 4];
        int last = Environment.TickCount;
        while (true) {
          uint pkt;
          cap.GetNextPacketSize(out pkt);
          if (pkt == 0) {
            if (Environment.TickCount - last >= 20) {
              try {
                stream.Write(silence, 0, silence.Length);
                last = Environment.TickCount;
              } catch {
                break;
              }
            }
            Thread.Sleep(4);
            continue;
          }
          IntPtr data;
          uint frames;
          int cflags;
          ulong a, b;
          cap.GetBuffer(out data, out frames, out cflags, out a, out b);
          int nbytes = (int)frames * 4;
          byte[] buf = new byte[nbytes];
          if ((cflags & 2) != 0) Array.Clear(buf, 0, buf.Length);
          else Marshal.Copy(data, buf, 0, nbytes);
          cap.ReleaseBuffer(frames);
          try {
            stream.Write(buf, 0, buf.Length);
            last = Environment.TickCount;
          } catch {
            break;
          }
        }
      }
    } finally {
      try { listener.Stop(); } catch { }
      try { client.Stop(); } catch { }
    }
    return 0;
  }

  static int Main(string[] args) {
    if (args.Length < 3) {
      Console.Error.WriteLine("usage: captura-audio-pid.exe <pid> include|exclude <porta>");
      return 2;
    }
    uint pid = uint.Parse(args[0]);
    bool include = args[1].ToLowerInvariant().StartsWith("inc");
    int port = int.Parse(args[2]);
    int code = 1;
    var t = new Thread(() => { code = Run(pid, include, port); });
    t.SetApartmentState(ApartmentState.MTA);
    t.Start();
    t.Join();
    return code;
  }
}
