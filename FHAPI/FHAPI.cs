using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacketDotNet;
using SharpPcap;
using System.Net.Sockets;
using Spectre.Console;

namespace FHAPILib
{
    public class FHAPI : IDisposable
    {
        public int CapturedPacketsCount => _capturedPackets.Count;
        public int BufferedPacketsCount => _processor.BufferSize;

        private readonly Capturer _capturer;
        private readonly Processor _processor;
        private ConcurrentQueue<RawCapture> _capturedPackets = new ConcurrentQueue<RawCapture>();
        private ConcurrentQueue<RawCapture> CapturedPackets
        {
            get => _capturedPackets;
            set { _capturedPackets = value;  }  
        }
        public int PacketsCount => _capturedPackets.Count;
        public FHAPI()
        {
            _processor = new Processor(ref _capturedPackets);
            _capturer = new Capturer(ref _capturedPackets);
            _capturer.OnStartCapturing += (sender , e) => _processor.StartBuffering();
            _capturer.OnStopCapturing += (sender, e) => _processor.StopBuffering();
        }
        public void Run()
        {
            _capturer.StartCapturing();
        }

        public async Task Monitor(CancellationToken token)
        {
            AnsiConsole.Write(new Rows(new Text($"DEVICE: {_capturer.CaptureDevice?.Name}")));
            // Create the first table
            var table1 = new Table()
                .Border(TableBorder.Ascii2)
                .AddColumns("QUEUE", "COUNT")
                .AddRow("[red]MAIN[/]", "0")
                .AddRow("[green]BUFF[/]", "0");

            // Create the second table
            var table2 = new Table()
                .Border(TableBorder.Ascii2)
                .Width(100)
                .Centered()
                .AddColumns("IP SOURCE", "IP DESTINATION", "SIZE", "DATA");

            await AnsiConsole.Live(new Rows(table1, table2)) // Stack them vertically
                .StartAsync(async ctx =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        // Update table 1 dynamically
                        table1.UpdateCell(0, 1, $"[red]{CapturedPacketsCount}[/]");
                        table1.UpdateCell(1, 1, $"[green]{_processor.BufferSize}[/]");

                        foreach (var packet in GetPackets())
                        {
                            table2.AddRow($"{packet.SourceAddress}", "", "", "", "");
                        }

                        ctx.Refresh();
                    }
                });
        }
        public List<IPv4Packet> GetPackets() => _processor.GetPackets();
        public void Dispose()
        {
            _capturer.StopCapturing();
        }
    }
}
