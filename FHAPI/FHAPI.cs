using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacketDotNet;
using SharpPcap;
using System.Net.Sockets;
using Spectre.Console;
using Spectre.Console.Rendering;
using FHAPI.Core;
using log4net.Util;

namespace FHAPILib
{
    public class FHAPI : IDisposable
    {
        #region PROPERTIES
        private readonly Capturer _capturer;
        private readonly Processor _processor;
        public CaptureDeviceList CaptureDevices => _capturer.CaptureDevices;
        public int CapturedPacketsCount => _capturedPackets.Count;
        public int BufferedPacketsCount => _processor.BufferSize;
        private ConcurrentQueue<RawCapture> _capturedPackets = new ConcurrentQueue<RawCapture>();
        private ConcurrentQueue<RawCapture> CapturedPackets
        {
            get => _capturedPackets;
            set { _capturedPackets = value;  }  
        }
        public bool IsRunning { get; set; } = false;
        #endregion
        public FHAPI()
        {
            _processor = new Processor(ref _capturedPackets);
            _capturer = new Capturer(ref _capturedPackets);
            _capturer.OnStartCapturing += (sender , e) => _processor.StartBuffering();
            _capturer.OnStopCapturing += (sender, e) => _processor.StopBuffering();
        }
        #region METHODS
        public void StartCapturing()
        {
            _capturer.StartCapturing();
        }
        public void StopCapturing()
        {
            _capturer.StopCapturing();
        }
        public void SetDeviceIndex(int index) => _capturer.DeviceIndex = index;
        public void SetFilter(string filter) => _capturer.Filter = filter;


        public async Task Monitor(CancellationToken token)
        {
            AnsiConsole.Write(new Rows(new Text($"DEVICE: {_capturer.CaptureDevice?.Name}")));

            var table1 = new Table()
                .Border(TableBorder.Ascii2)
                .AddColumns("QUEUE", "COUNT")
                .AddRow("[red]MAIN[/]", "0")
                .AddRow("[green]BUFF[/]", "0");

            var packetLogs = new List<Text>();
            const int maxLogLength = 10;

            await AnsiConsole.Live(new Rows(table1))
                .StartAsync(async ctx =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        table1.UpdateCell(0, 1, $"[red]{CapturedPacketsCount}[/]");
                        table1.UpdateCell(1, 1, $"[green]{_processor.BufferSize}[/]");

                        foreach (var packet in GetPackets())
                        {
                            var packetInfo = new Text($"{packet.SourceAddress} => {packet.DestinationAddress}: [{packet.Payload?.Length}]", new Style(Color.Yellow));
                            packetLogs.Add(packetInfo);
                            if (packetLogs.Count > maxLogLength) { packetLogs.RemoveAt(0); }
                        }
                        var updatedRows = new List<IRenderable> { table1 };
                        updatedRows.AddRange(packetLogs);

                        ctx.UpdateTarget(new Rows(updatedRows));
                        ctx.Refresh();
                        Thread.Sleep(300);
                    }
                });
        }
        public List<FHPacket> GetPackets() =>_processor.GetPackets(x => (x + 1) % 5 == 0);
        public void Dispose() => _capturer.StopCapturing();
        #endregion
    }
}
