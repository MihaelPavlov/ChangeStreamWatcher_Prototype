namespace ChangeStreamWatcher_Blazor.Services
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public class HttpLogEnricher : ILogEnricher
    {
        private string _ipAddress;
        private string _machineName;
        private string _username;

        public HttpLogEnricher(IHttpContextAccessor contextAccessor)
        {
            if (contextAccessor is null)
                throw new ArgumentNullException(nameof(contextAccessor));

            var context = contextAccessor.HttpContext;
            this._ipAddress = context?.Connection?.RemoteIpAddress?.ToString();
            this._username = context?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Anonymous";
            this._machineName = Environment.MachineName;
        }

        public void Enrich(ILogDocument document)
        {
            if (document is null)
                return;

            if (string.IsNullOrWhiteSpace(this._ipAddress) == false)
                document.IpAddress = this._ipAddress;

            if (string.IsNullOrWhiteSpace(this._machineName) == false)
                document.MachineName = this._machineName;

            if (string.IsNullOrWhiteSpace(this._username) == false)
                document.Username = this._username;
        }
    }
}
