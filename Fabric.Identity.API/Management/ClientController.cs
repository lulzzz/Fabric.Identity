﻿using System;
using System.Net;
using Fabric.Identity.API.CouchDb;
using Fabric.Identity.API.Validation;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Fabric.Identity.API.Management
{
    [Route("api/[controller]")]
    public class ClientController : BaseController<Client>
    {
        private readonly IDocumentDbService _documentDbService;
        private readonly ILogger _logger;
        private const string GetClientRouteName = "GetClient";

        public ClientController(IDocumentDbService documentDbService,  ClientValidator validator, ILogger logger) 
            : base(validator, logger)
        {
            _documentDbService = documentDbService;
            _logger = logger;
        }

        // GET api/values/5
        [HttpGet("{id}", Name = GetClientRouteName)]
        public IActionResult Get(string id)
        {
            try
            {
                var client = _documentDbService.GetDocument<Client>(id).Result;

                if (client == null)
                {
                    return CreateFailureResponse($"The specified client with id: {id} was not found",
                        HttpStatusCode.NotFound);
                }

                return Ok(client);
            }
            catch (Exception)
            {
                _logger.Error($"The specified client with id: {id} was not found.");
                return CreateFailureResponse($"The specified client with id: {id} was not found",
                    HttpStatusCode.BadRequest);
            }
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Client value)
        {
            try
            {
                var validationResult = Validate(value);

                if (!validationResult.IsValid)
                {
                    return CreateValidationFailureResponse(validationResult);
                }

                var id = value.ClientId;
                _documentDbService.AddOrUpdateDocument(id, value);

                return CreatedAtRoute(GetClientRouteName, new {id}, value);
            }
            catch (Exception e)
            {
                _logger.Error($"Unable to create a new client. Error: {e.Message}");
                return CreateFailureResponse(e.Message, HttpStatusCode.BadRequest);
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]Client value)
        {
            try
            {
                var validationResult = Validate(value);

                if (!validationResult.IsValid)
                {
                    return CreateValidationFailureResponse(validationResult);
                }
                _documentDbService.AddOrUpdateDocument(id, value);

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.Error($"Unable to update client. Error: {e.Message}");
                return CreateFailureResponse(e.Message, HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _documentDbService.DeleteDocument<Client>(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.Error($"Unable to delete client with id: {id}");
                return CreateFailureResponse(e.Message, HttpStatusCode.BadRequest);
            }
            
        }               
    }
}
