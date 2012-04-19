﻿using System;
using System.Net;
using Griffin.Networking.Http.Protocol;
using Griffin.Networking.Http.Specification;

namespace Griffin.Networking.Http.Implementation
{
    internal class HttpRequest : HttpMessage, IRequest
    {
        private readonly IHttpCookieCollection<IHttpCookie> _cookies;
        private readonly IHttpFileCollection _files;
        private readonly IParameterCollection _queryString;
        private IParameterCollection _form;
        private string _pathAndQuery;

        public HttpRequest()
        {
            _cookies = new HttpCookieCollection<HttpCookie>();
            _files = new HttpFileCollection();
            _queryString = new ParameterCollection();
            _form = new ParameterCollection();
        }

        public HttpRequest(string httpMethod, string uri, string httpVersion)
            :this()
        {
            if (httpMethod == null) throw new ArgumentNullException("httpMethod");
            if (uri == null) throw new ArgumentNullException("uri");
            if (httpVersion == null) throw new ArgumentNullException("httpVersion");
            Method = httpMethod;
            _pathAndQuery = uri;
            ProtocolVersion = httpVersion;
        }

        #region IRequest Members

        /// <summary>
        /// Gets or sets if connection is being kept alive
        /// </summary>
        public bool KeepAlive
        {
            get { return Headers["Connection"].Value.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Gets content type
        /// </summary>
        /// <remarks>Any extra parameters are stripped. Use <see cref="Headers"/> to get the raw value</remarks>
        public string ContentType
        {
            get { return Headers["Content-Type"].Value; }
        }

        /// <summary>
        /// Gets cookies.
        /// </summary>
        public IHttpCookieCollection<IHttpCookie> Cookies
        {
            get { return _cookies; }
        }

        /// <summary>
        /// Gets all uploaded files.
        /// </summary>
        public IHttpFileCollection Files
        {
            get { return _files; }
        }

        /// <summary>
        /// Gets form parameters.
        /// </summary>
        public IParameterCollection Form
        {
            get { return _form; }
        }

        /// <summary>
        /// Gets where the request originated from.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// Gets if request is an Ajax request.
        /// </summary>
        public bool IsAjax
        {
            get { return Headers["X-Requested-Width"].Value.Equals("Ajax", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Gets or sets HTTP method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets query string.
        /// </summary>
        public IParameterCollection QueryString
        {
            get { return _queryString; }
        }

        /// <summary>
        /// Gets requested URI.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Create a response for the request.
        /// </summary>
        /// <param name="code">Status code</param>
        /// <param name="reason">Gives the remote end point a hint to why the specified status code as used.</param>
        /// <returns>Created response</returns>
        /// <remarks>Can be used by implementations to transfer context specific information. It's prefered that you use this method
        /// instead of instantianting a response directly.</remarks>
        public IResponse CreateResponse(HttpStatusCode code, string reason)
        {
            if (reason == null) throw new ArgumentNullException("reason");
            return new HttpResponse(ProtocolVersion, code, reason);
        }

        #endregion

        public override void AddHeader(string name, string value)
        {
            if (name.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                if (!value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    Uri = new Uri(string.Format("http://{0}{1}", value, _pathAndQuery));
                else
                    Uri = new Uri(string.Format("{0}{1}", value, _pathAndQuery));
            }
            if (name.Equals("Content-Length", StringComparison.CurrentCultureIgnoreCase))
            {
                ContentLength = int.Parse(value);
            }

            base.AddHeader(name, value);
        }
    }
}