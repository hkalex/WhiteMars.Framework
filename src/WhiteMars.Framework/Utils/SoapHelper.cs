using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WhiteMars.Framework
{
    /// <summary>
    /// A helper class for sending soap
    /// </summary>
    public static class SoapHelper
    {

        const string SOAP_TEMPLATE = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Header />
    <soap:Body />
</soap:Envelope>";

        public static XNamespace SOAP_XML_NAMESPACE = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");

        /// <summary>
        /// Send SOAP with the following format
        /// </summary>
        /// <param name="body"></param>
        /// <param name="webServiceUri"></param>
        /// <param name="soapAction"></param>
        /// <param name="header"></param>
        /// <param name="host"></param>
        /// <param name="httpMethod"></param>
        /// <param name="contentType"></param>
        /// <param name="httpVersion"></param>
        /// <param name="throwExceptionIfHttpException"></param>
        /// <remarks>
        /// 
        /// POST (@webServiceUri) HTTP/(@version)
        /// Host: (@webServiceUri.Host or @host)
        /// Content-Type: (@contentType)
        /// Content-Length: length
        /// SOAPAction: "(@soapAction)"
        /// 
        /// &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        /// &lt;soap:Envelope xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:soap=&quot;http://schemas.xmlsoap.org/soap/envelope/&quot;&gt;
        /// &lt;soap:Header&gt;
        /// (@header)
        /// &lt;/soap:Header&gt;
        /// &lt;soap:Body&gt;
        /// (@body)
        /// &lt;/soap:Body&gt;
        /// &lt;/soap:Envelope&gt;
        /// 
        /// </remarks>
        /// <returns></returns>
        public static SoapResponse Send(SoapRequest soapRequest, bool throwExceptionIfHttpException = true)
        {
            var request = soapRequest.PreparHttpWebRequest(); // this just prepare the HttpHeaders
            var buffer = soapRequest.HttpBytes; // get the byte[] for Soap message

            var requestStream = request.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);


            var response = (HttpWebResponse)request.GetResponse();

            // response
            var result = new SoapResponse(response);

            if (throwExceptionIfHttpException && result.HttpStatusCodeNumber >= 400)
            {
                throw new WhiteMarsException(string.Format("Http Exception: {0} - {1}", result.HttpStatusCodeNumber, result.HttpStatusDescription));
            }

            return result;
        }

        public class SoapRequest
        {


            const string SOAP_TEMPLATE = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Header />
    <soap:Body />
</soap:Envelope>";

            private string httpString = string.Empty;
            private byte[] httpBytes = null;

            public SoapRequest(XElement body,
                Uri webServiceUri,
                string soapAction,
                XElement header = null,
                string host = null,
                string httpMethod = null,
                string contentType = "text/xml; charset=utf-8",
                Version httpVersion = null)
            {
                this.Body = body;
                this.WebServiceUri = webServiceUri;
                this.SoapAction = soapAction;
                this.Header = header;
                this.Host = string.IsNullOrWhiteSpace(host) ? webServiceUri.Host : host;
                this.HttpMethod = string.IsNullOrWhiteSpace(httpMethod) ? "POST" : httpMethod;
                this.ContentType = string.IsNullOrWhiteSpace(contentType) ? "text/xml; charset=utf-8" : contentType;
                this.HttpVersion = httpVersion ?? System.Net.HttpVersion.Version11;


                var xml = XElement.Parse(SOAP_TEMPLATE);

                if (this.Header != null)
                {
                    var headerNode = xml.Descendants(SOAP_XML_NAMESPACE + "Header").First();
                    headerNode.Add(this.Header);
                }

                if (this.Body != null)
                {
                    var bodyNode = xml.Descendants(SOAP_XML_NAMESPACE + "Body").First();
                    bodyNode.Add(this.Body);
                }

                this.httpString = xml.ToString();
                this.httpBytes = Encoding.UTF8.GetBytes(xml.ToString());
            }

            public HttpWebRequest PreparHttpWebRequest()
            {
                var request = (HttpWebRequest)WebRequest.Create(this.WebServiceUri);
                request.Method = this.HttpMethod;
                request.Host = this.Host;
                request.ContentType = this.ContentType;
                request.Headers.Add("SOAPAction: \"" + this.SoapAction + "\"");
                request.ProtocolVersion = this.HttpVersion;

                return request;
            }

            public byte[] HttpBytes
            {
                get { return this.httpBytes; }
            }

            public string HttpString
            {
                get { return this.httpString; }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                sb.AppendLine(string.Format("{0} {1} HTTP/{2}", this.HttpMethod, this.WebServiceUri.ToString(), this.HttpVersion));
                sb.AppendLine(string.Format("Host: {0}", this.Host));
                sb.AppendLine(string.Format("Content-Type: {0}", this.ContentType));
                sb.AppendLine(string.Format("Content-Length: {0}", this.httpBytes.Length));
                sb.AppendLine(string.Format("SOAPAction: {0}", this.SoapAction));
                sb.AppendLine();
                sb.AppendLine(this.httpString);

                return sb.ToString();
            }

            public XElement Body { get; private set; }
            public Uri WebServiceUri { get; private set; }
            public string SoapAction { get; private set; }
            public XElement Header { get; private set; }
            public string Host { get; private set; }
            public string HttpMethod { get; private set; }
            public string ContentType { get; private set; }
            public Version HttpVersion { get; private set; }
        }

        public class SoapResponse
        {
            public SoapResponse(HttpWebResponse httpResponse)
            {
                this.httpResponse = httpResponse;

                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    this.Body = reader.ReadToEnd();
                }

                this.Header = new Dictionary<string, string>();
                foreach (var hk in httpResponse.Headers.AllKeys)
                {
                    var hv = httpResponse.Headers[hk];
                    this.Header[hk] = hv;
                }
            }

            private HttpWebResponse httpResponse;


            public override string ToString()
            {
                /*
                HTTP/1.1 200 OK
                Content-Type: text/xml; charset=utf-8
                Content-Length: length

                <?xml version="1.0" encoding="utf-8"?>
                <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <SendCharFaxResponse xmlns="http://www.interfax.cc">
                      <SendCharFaxResult>long</SendCharFaxResult>
                    </SendCharFaxResponse>
                  </soap:Body>
                </soap:Envelope>
                */

                var sb = new StringBuilder();
                var httpResponse = this.httpResponse;

                sb.AppendLine(string.Format("HTTP/{0} {1} {2}", httpResponse.ProtocolVersion.ToString(), (int)httpResponse.StatusCode, httpResponse.StatusDescription));
                //sb.AppendLine(string.Format("Content-Type: {0}", httpResponse.ContentType));
                //sb.AppendLine(string.Format("Content-Length: {0}", httpResponse.ContentLength));
                foreach (var hk in httpResponse.Headers.AllKeys)
                {
                    var hv = httpResponse.Headers[hk];
                    sb.AppendLine(string.Format("{0}: {1}", hk, hv));
                }
                sb.AppendLine();
                sb.AppendLine(this.Body);

                return sb.ToString();
            }

            public HttpStatusCode HttpStatusCode
            {
                get { return this.httpResponse.StatusCode; }
            }

            public int HttpStatusCodeNumber
            {
                get { return (int)(this.httpResponse.StatusCode); }
            }

            public string HttpStatusDescription
            {
                get { return this.httpResponse.StatusDescription; }
            }

            public string Body { get; private set; }
            public Dictionary<string, string> Header { get; private set; }
        }

    }
}
