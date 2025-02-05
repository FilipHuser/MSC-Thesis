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
        public readonly Processor _processor;
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
            _capturer = new Capturer(ref _capturedPackets , 4);
            _capturer.OnStartCapturing += (sender , e) => _processor.StartBuffering();
            _capturer.OnStopCapturing += (sender, e) => _processor.StopBuffering();
        }
        public void Run()
        {
            _capturer.StartCapturing();
        }

        public async Task Monitor(CancellationToken token)
        {
            var table = new Table();
            table.Title = new TableTitle($"FHAPI ~ dev: {_capturer?.CaptureDevice?.Name}");
            table.Caption = new TableTitle("Press Any Key To Exit...");
            table.Border = TableBorder.Ascii2;
            table.AddColumns("QUEUE" , "COUNT");
            table.AddRow("[red]MAIN[/]", "0");
            table.AddRow("[green]BUFF[/]", "0");
            table.Width = 40;

            await AnsiConsole.Live(table)
                        .StartAsync(async ctx => {

                            while (!token.IsCancellationRequested)
                            {
                                table.UpdateCell(0, 1, $"[red]{CapturedPacketsCount}[/]");
                                table.UpdateCell(1, 1, $"[green]{_processor.BufferSize}[/]");
                                ctx.Refresh();
                                //await Task.Delay(13, token);
                            }
                        });
        }

        public void Dispose()
        {
            _capturer.StopCapturing();
        }
    }
}
