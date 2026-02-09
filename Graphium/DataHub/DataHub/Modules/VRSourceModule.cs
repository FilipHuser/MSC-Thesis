using System.Net;
using DataHub.Core;

namespace DataHub.Modules
{
    public class VRSourceModule : ModuleBase<string>
    {
        #region PROPERTIES
        private readonly string _url;
        private HttpListener _listener;
        public override ModuleType ModuleType => ModuleType.VR;
        #endregion
        #region METHODS
        public VRSourceModule(string url)
        {
            _url = url;
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _listener.Start();
        }
        protected override async Task CaptureTask(CancellationToken ct)
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
        public override void Dispose()
        {
            StopCapturing();
            _listener.Close();
        }
        #endregion
    }
}