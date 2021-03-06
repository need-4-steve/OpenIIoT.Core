﻿namespace OpenIIoT.Core.Model.WebApi
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using NLog;
    using OpenIIoT.Core.Service.WebApi;
    using OpenIIoT.SDK;
    using OpenIIoT.SDK.Common;
    using OpenIIoT.SDK.Model;

    public class BrowseController : ApiBaseController
    {
        #region Private Fields

        private static string[] conciseSerializationProperties = new[] { "FQN", "Children" };
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static IApplicationManager manager = ApplicationManager.GetInstance();
        private static Item model = manager.GetManager<ModelManager>().Model;
        private static string[] verboseSerializationProperties = new[] { "Parent", "SourceItem", "Lock", "HasChildren", "IsOrphaned" };

        #endregion Private Fields

        #region Public Methods

        [Route("api/browse")]
        [HttpGet]
        public HttpResponseMessage Browse()
        {
            return Browse("verbose");
        }

        [Route("api/browse/{verbosity}")]
        [HttpGet]
        public HttpResponseMessage Browse(string verbosity)
        {
            return Browse(model.FQN, verbosity);
        }

        [Route("api/browse/{fqn}/{verbosity}")]
        [HttpGet]
        public HttpResponseMessage Browse(string fqn, string verbosity = "verbose")
        {
            List<Item> result = new List<Item>();
            result.Add(manager.GetManager<IModelManager>().FindItem(fqn));

            JsonMediaTypeFormatter formatter;

            if (verbosity == "concise")
            {
                formatter = JsonFormatter(ContractResolverType.OptIn, conciseSerializationProperties);
            }
            else
            {
                formatter = JsonFormatter();
            }

            logger.Info("API request; FQN:" + fqn + "; Verbosity: " + verbosity + ". Remote IP: " + Request.GetOwinContext().Request.RemoteIpAddress + "; returning HTTP 200/OK");
            return Request.CreateResponse(HttpStatusCode.OK, result, formatter);
        }

        #endregion Public Methods
    }
}