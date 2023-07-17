#if !BESTHTTP_DISABLE_PROXY && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace BestHTTP.Proxies.Autodetect
{
    /// <summary>
    /// This is a detector using the .net framework's implementation. It might work not just under Windows but MacOS and Linux too.
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.defaultproxy?view=net-6.0"/>
    sealed class FrameworkProxyDetector : IProxyDetector
    {
        Proxy IProxyDetector.GetProxy(HTTPRequest request)
        {
            var proxy = System.Net.WebRequest.GetSystemWebProxy() as System.Net.WebProxy;
            if (proxy != null && proxy.Address != null)
            {
                var proxyUri = proxy.GetProxy(request.CurrentUri);
                if (proxyUri != null)
                {
                    if (proxyUri.Scheme.StartsWith("socks://", StringComparison.OrdinalIgnoreCase))
                    {
                        return new SOCKSProxy(proxyUri, null);
                    }
                    else if (proxyUri.Scheme.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    {
                        return new HTTPProxy(proxyUri);
                    }
                    else
                    {
                        HTTPManager.Logger.Warning(nameof(FrameworkProxyDetector), $"{nameof(IProxyDetector.GetProxy)} - FindFor returned with unknown format. proxyUri: '{proxyUri}'", request.Context);
                    }
                }
            }

            return null;
        }
    }
}

#endif
