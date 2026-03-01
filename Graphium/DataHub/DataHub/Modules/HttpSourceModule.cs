using System.Net;
using DataHub.Core;

namespace DataHub.Modules
{
    public class HttpSourceModule : ModuleBase<string>
    {
        #region PROPERTIES
        private readonly string _url;
        private HttpListener? _listener;
        public override double SamplingRate => 10;
        public override ModuleType ModuleType => ModuleType.HTTP;
        #endregion
        #region METHODS
        public HttpSourceModule(string url)
        {
            _url = url;
        }
        protected override async Task CaptureTask(CancellationToken ct)
        {
            _listener?.Close();
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _listener.Start();
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    HttpListenerContext? context = null;
                    try
                    {
                        context = await _listener.GetContextAsync().WaitAsync(ct);
                        if (context.Request.HttpMethod == "POST")
                        {
                            using var reader = new StreamReader(
                                context.Request.InputStream,
                                context.Request.ContentEncoding);
                            string postData = await reader.ReadToEndAsync();
                            Enqueue(new CapturedData<string>(DateTime.Now, postData, this));
                            context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                            context.Response.ContentLength64 = 0;
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception)
                    {
                        if (context != null)
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    finally
                    {
                        context?.Response.Close();
                    }
                }
            }
            finally
            {
                _listener.Close();
                _listener = null;
            }
        }
        public override void Dispose()
        {
            StopCapturing();
            _listener?.Close();
            _listener = null;
        }
        #endregion
    }
}