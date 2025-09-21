using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;

namespace DataHub.Modules
{
    public class HTTPModule<S> : ModuleBase
    {
        #region PROPERTIES
        private string _url;
        private ConcurrentQueue<CapturedData<S>> _dataQueue = [];
        private HttpListener _listener = new HttpListener();
        #endregion
        #region METHODS
        public HTTPModule(string URL) 
        {
            _url = URL;
            Init();
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
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _listener.Start();
        }
        public override void Dispose()
        {
            StopCapturing();
            _listener.Close();
        }

        protected override async Task CaptureTask(CancellationToken ct)
        {
            HttpListenerContext? context = null;

            while(!ct.IsCancellationRequested)
            {
                try
                {
                    context = await _listener.GetContextAsync();

                    if (context.Request.HttpMethod == "POST")
                    {
                        using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                        string postData = await reader.ReadToEndAsync();

                        S data = (S)(object)postData;

                        var capturedData = new CapturedData<S>(DateTime.Now, data, this);
                        _dataQueue.Enqueue(capturedData);
                        context.Response.StatusCode = (int)HttpStatusCode.Accepted; // 202
                        context.Response.StatusDescription = "Accepted";
                        context.Response.ContentLength64 = 0;
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
        }
        #endregion
    }
}
