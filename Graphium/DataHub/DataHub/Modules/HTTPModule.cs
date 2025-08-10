using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;
using SharpPcap;

namespace DataHub.Modules
{
    public class HTTPModule<S> : ModuleBase
    {
        #region PROPERTIES
        private string _url;
        private ConcurrentQueue<CapturedData<S>> _dataQueue = [];
        private HttpListener _listener = new HttpListener();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        #endregion
        #region METHODS
        public HTTPModule(string URL)
        {
            _url = URL;
        }
        public override IEnumerable<CapturedData<T>> Get<T>(Func<CapturedData<T>, bool>? predicate = null, int? skip = null, int? take = null)
        {
            if (typeof(T) != typeof(S)) { yield break; }

            int skipped = 0;
            int yielded = 0;

            while (_dataQueue.TryDequeue(out var rawItem))
            {
                var item = (CapturedData<T>)(object)rawItem;

                if (predicate != null && !predicate(item)) { continue; }

                if (skip.HasValue && skipped < skip.Value)
                {
                    skipped++;
                    continue;
                }

                yield return item;
                yielded++;

                if (take.HasValue && yielded >= take.Value) { yield break; }
            }
        }
        private void Init()
        {
            if (_listener != null)
            {
                return;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _cts = new CancellationTokenSource();
        }
        public override void StartCapturing()
        {
            Init();
            _listener.Start();
            var cancellationToken = _cts.Token;

            _capturingThread = new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext? context = null;
                    try
                    {
                        context = _listener.GetContext(); // blocking call

                        if (context.Request.HttpMethod == "POST")
                        {
                            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                            string postData = reader.ReadToEnd();

                            S data = (S)(object)postData;

                            var capturedData = new CapturedData<S>(DateTime.Now, data, this);
                            _dataQueue.Enqueue(capturedData);

                            string responseString = "POST data received";
                            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                            context.Response.ContentLength64 = buffer.Length;
                            context.Response.ContentType = "text/plain";
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                            context.Response.StatusDescription = "Only POST requests allowed";
                        }
                    }
                    catch (Exception ex)
                    {
                        if (context != null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.StatusDescription = "Internal Server Error";
                        }
                    }
                    finally
                    {
                        context?.Response.Close();
                    }
                }
            });

            _capturingThread.IsBackground = true;
            _capturingThread.Start();
        }
        public override void StopCapturing()
        {
            _cts.Cancel();
            _listener.Stop(); 
            _listener.Close();
            _capturingThread?.Join();
        }
        #endregion
    }
}
