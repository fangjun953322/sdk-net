﻿/*
 * Copyright 2021-Present The Serverless Workflow Specification Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using ServerlessWorkflow.Sdk.Models;
using ServerlessWorkflow.Sdk.Services.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerlessWorkflow.Sdk.Services.IO
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowSchemaValidator"/> interface
    /// </summary>
    public class WorkflowSchemaValidator
        : IWorkflowSchemaValidator
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowSchemaValidator"/>
        /// </summary>
        /// <param name="serializer">The service used to serialize and deserialize JSON</param>
        /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
        public WorkflowSchemaValidator(IJsonSerializer serializer, IHttpClientFactory httpClientFactory)
        {
            this.Serializer = serializer;
            this.HttpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Gets the service used to serialize and deserialize JSON
        /// </summary>
        protected IJsonSerializer Serializer { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used to fetch the Serverless Workflow schema
        /// </summary>
        protected HttpClient HttpClient { get; }

        private JSchema _Schema;
        /// <summary>
        /// Gets the <see cref="JSchema"/> used to validate <see cref="WorkflowDefinition"/>s
        /// </summary>
        protected JSchema Schema
        {
            get
            {
                if (this._Schema == null)
                    this._Schema = this.LoadSchemaAsync().GetAwaiter().GetResult();
                return this._Schema;
            }
        }

        /// <inheritdoc/>
        public virtual bool Validate(string workflowJson, out IList<string> validationErrors)
        {
            JObject workflow = this.Serializer.Deserialize<JObject>(workflowJson);
            return workflow.IsValid(this.Schema, out validationErrors);
        }

        /// <summary>
        /// Loads the Serverless Workflow <see cref="JSchema"/>
        /// </summary>
        /// <returns>The Serverless Workflow <see cref="JSchema"/></returns>
        protected virtual async Task<JSchema> LoadSchemaAsync()
        {
            using HttpResponseMessage response = await this.HttpClient.GetAsync("https://raw.githubusercontent.com/serverlessworkflow/specification/main/schema/workflow.json");
            string json = await response.Content?.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return JSchema.Parse(json, new JSchemaUrlResolver());
        }

    }

}
