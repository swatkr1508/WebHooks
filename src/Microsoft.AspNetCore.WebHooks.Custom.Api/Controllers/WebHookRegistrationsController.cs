// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Routes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Controllers
{
    /// <summary>
    /// The <see cref="WebHookRegistrationsController"/> allows the caller to create, modify, and manage WebHooks
    /// through a REST-style interface.
    /// </summary>
    [Authorize]
    [Route("api/webhooks/registrations")]
    public class WebHookRegistrationsController : ControllerBase
    {
        private IWebHookRegistrationsManager _registrationsManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Create new WebHookRegistrationsController
        /// </summary>
        public WebHookRegistrationsController(ILogger<WebHookRegistrationsController> logger, IWebHookRegistrationsManager registrationsManager)
        {
            _logger = logger;
            _registrationsManager = registrationsManager;
        }

        /// <summary>
        /// Gets all registered WebHooks for a given user.
        /// </summary>
        /// <returns>A collection containing the registered <see cref="WebHook"/> instances for a given user.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var webHooks = await _registrationsManager.GetWebHooksAsync(User, RemovePrivateFilters);
                return Ok(webHooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Looks up a registered WebHook with the given <paramref name="id"/> for a given user.
        /// </summary>
        /// <returns>The registered <see cref="WebHook"/> instance for a given user.</returns>
        [HttpGet("{id}", Name = WebHookRouteNames.RegistrationLookupAction)]
        public async Task<IActionResult> Lookup(string id)
        {
            try
            {
                var webHook = await _registrationsManager.LookupWebHookAsync(User, id, RemovePrivateFilters);
                if (webHook != null)
                {
                    return Ok(webHook);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Registers a new WebHook for a given user.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to create.</param>
        [HttpPost()]
        [ValidateModel]
        public async Task<IActionResult> Post([FromBody]WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }

            try
            {
                // Validate the provided WebHook ID (or force one to be created on server side)
                var idValidator = HttpContext.RequestServices.GetIdValidator();
                await idValidator.ValidateIdAsync(Request, webHook);

                // Validate other parts of WebHook
                await _registrationsManager.VerifySecretAsync(webHook);
                await _registrationsManager.VerifyFiltersAsync(webHook);
                await _registrationsManager.VerifyAddressAsync(webHook);
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                _logger.LogInformation(message);
                return BadRequest(message);
            }

            try
            {
                // Add WebHook for this user.
                var result = await _registrationsManager.AddWebHookAsync(User, webHook, AddPrivateFilters);
                if (result == StoreResult.Success)
                {
                    return CreatedAtRoute(WebHookRouteNames.RegistrationLookupAction, new { id = webHook.Id }, webHook);
                }
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                _logger.LogError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        /// <param name="webHook">The new <see cref="WebHook"/> to use.</param>
        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Put(string id, [FromBody]WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }
            if (!string.Equals(id, webHook.Id, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            try
            {
                // Validate parts of WebHook
                await _registrationsManager.VerifySecretAsync(webHook);
                await _registrationsManager.VerifyFiltersAsync(webHook);
                await _registrationsManager.VerifyAddressAsync(webHook);
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                _logger.LogInformation(message);
                return BadRequest(message);
            }

            try
            {
                // Update WebHook for this user
                var result = await _registrationsManager.UpdateWebHookAsync(User, webHook, AddPrivateFilters);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_UpdateFailure, ex.Message);
                _logger.LogError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _registrationsManager.DeleteWebHookAsync(User, id);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_DeleteFailure, ex.Message);
                _logger.LogError(message, ex);
                return StatusCode(500, message);
            }
        }

        /// <summary>
        /// Deletes all existing WebHook registrations.
        /// </summary>
        [HttpDelete("")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _registrationsManager.DeleteAllWebHooksAsync(User);
                return Ok();
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_DeleteAllFailure, ex.Message);
                _logger.LogError(message, ex);
                return StatusCode(500, message);
            }
        }

        /// <summary>
        /// Removes all private (server side) filters from the given <paramref name="webHook"/>.
        /// </summary>
        protected virtual Task RemovePrivateFilters(string user, WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var filters = webHook.Filters.Where(f => f.StartsWith(WebHookRegistrar.PrivateFilterPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (var filter in filters)
            {
                webHook.Filters.Remove(filter);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes all <see cref="IWebHookRegistrar"/> instances for server side manipulation, inspection, or
        /// rejection of registrations. This can for example be used to add server side only filters that
        /// are not governed by <see cref="IWebHookFilterManager"/>.
        /// </summary>
        protected virtual async Task AddPrivateFilters(string user, WebHook webHook)
        {
            var registrars = HttpContext.RequestServices.GetRegistrars();
            if (registrars != null)
            {
                foreach (var registrar in registrars)
                {
                    try
                    {
                        await registrar.RegisterAsync(Request, webHook);
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrarException, registrar.GetType().Name, typeof(IWebHookRegistrar).Name, ex.Message);
                        _logger.LogError(message, ex);
                        // TODO
                        //var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                        //throw new HttpResponseException(response);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="IActionResult"/> based on the provided <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result to use when creating the <see cref="IActionResult"/>.</param>
        /// <returns>An initialized <see cref="IActionResult"/>.</returns>
        private IActionResult CreateHttpResult(StoreResult result)
        {
            switch (result)
            {
                case StoreResult.Success:
                    return Ok();

                case StoreResult.Conflict:
                    return StatusCode(409);

                case StoreResult.NotFound:
                    return NotFound();

                case StoreResult.OperationError:
                    return BadRequest();

                default:
                    return StatusCode(500);
            }
        }
    }
}
