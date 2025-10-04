using System.Collections.Concurrent;
using System.Net;
using DataHub.Core;

namespace DataHub.Modules
{
    public class VRSourceModule : ModuleBase<string>
    {
        #region PROPERTIES
        private string _url;
        private ConcurrentQueue<CapturedData<string>> _dataQueue = [];
        private HttpListener _listener = new HttpListener();
        public override ModuleType ModuleType => ModuleType.VR;
        #endregion
        #region METHODS
        public VRSourceModule(string URL) 
        {
            _url = URL;
            Init();
        }
        public override IEnumerable<CapturedData<string>> Get(Func<CapturedData<string>, bool>? predicate = null, int? skip = null, int? take = null)
        {
            int skipped = 0;
            int yielded = 0;

            while (_dataQueue.TryDequeue(out var rawItem))
            {
                if (predicate != null && !predicate(rawItem))
                    continue;

                if (skip.HasValue && skipped < skip.Value)
                {
                    skipped++;
                    continue;
                }

                yield return rawItem;
                yielded++;

                if (take.HasValue && yielded >= take.Value)
                    yield break;
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

                        var capturedData = new CapturedData<string>(DateTime.Now, postData, this);
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
