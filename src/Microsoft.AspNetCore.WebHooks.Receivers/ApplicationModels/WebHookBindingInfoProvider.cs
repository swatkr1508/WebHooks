// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Receivers.Properties;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels;

/// <summary>
/// An <see cref="IApplicationModelProvider"/> implementation that adds model binding information
/// (<see cref="BindingInfo"/> settings similar to <see cref="IBindingSourceMetadata"/> and
/// <see cref="IModelNameProvider"/>) to <see cref="ParameterModel"/>s of WebHook actions.
/// </summary>
public class WebHookBindingInfoProvider : IApplicationModelProvider
{
    private readonly ILogger _logger;

    /// <summary>
    /// Instantiates a new <see cref="WebHookBindingInfoProvider"/> instance with the given
    /// <paramref name="loggerFactory"/>.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    public WebHookBindingInfoProvider(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<WebHookBindingInfoProvider>();
    }

    /// <summary>
    /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
    /// <see cref="WebHookBindingInfoProvider"/> instances. The WebHook <see cref="IApplicationModelProvider"/>
    /// order is
    /// <list type="number">
    /// <item>
    /// Add <see cref="IWebHookMetadata"/> references to the <see cref="ActionModel.Properties"/> collections of
    /// WebHook actions and validate those <see cref="IWebHookMetadata"/> attributes and services (in
    /// <see cref="WebHookActionModelPropertyProvider"/>).
    /// </item>
    /// <item>
    /// Add routing information (<see cref="SelectorModel"/> settings) to <see cref="ActionModel"/>s of WebHook
    /// actions (in <see cref="WebHookSelectorModelProvider"/>).
    /// </item>
    /// <item>
    /// Add filters to the <see cref="ActionModel.Filters"/> collections of WebHook actions (in
    /// <see cref="WebHookActionModelFilterProvider"/>).
    /// </item>
    /// <item>
    /// Add model binding information (<see cref="BindingInfo"/> settings) to <see cref="ParameterModel"/>s of
    /// WebHook actions (in this provider).
    /// </item>
    /// </list>
    /// </summary>
    public static int Order => WebHookActionModelFilterProvider.Order + 10;

    /// <inheritdoc />
    int IApplicationModelProvider.Order => Order;

    /// <inheritdoc />
    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        for (var i = 0; i < context.Result.Controllers.Count; i++)
        {
            var controller = context.Result.Controllers[i];
            for (var j = 0; j < controller.Actions.Count; j++)
            {
                var action = controller.Actions[j];
                var attribute = action.Attributes.OfType<WebHookAttribute>().FirstOrDefault();
                if (attribute == null)
                {
                    // Not a WebHook handler.
                    continue;
                }

                WebHookBodyType? bodyType;
                var properties = action.Properties;
                var bodyTypeMetadataObject = properties[typeof(IWebHookBodyTypeMetadataService)];
                if (bodyTypeMetadataObject is IWebHookBodyTypeMetadataService bodyTypeMetadata)
                {
                    bodyType = bodyTypeMetadata.BodyType;
                }
                else if (properties.TryGetValue(typeof(IWebHookBodyTypeMetadata), out bodyTypeMetadataObject))
                {
                    // Reachable only in [GeneralWebHook(WebHookBodyType)] cases. That attribute implements
                    // IWebHookBodyTypeMetadata and WebHookActionModelPropertyProvider passed it along because its
                    // BodyType is not null.
                    var actionBodyTypeMetadata = (IWebHookBodyTypeMetadata)bodyTypeMetadataObject;
                    bodyType = actionBodyTypeMetadata.BodyType;
                }
                else
                {
                    // Reachable only in [GeneralWebHook] cases. SourceData will warn if a data parameter is found.
                    bodyType = null;
                }

                properties.TryGetValue(typeof(IWebHookBindingMetadata), out var bindingMetadata);
                for (var k = 0; k < action.Parameters.Count; k++)
                {
                    var parameter = action.Parameters[k];
                    Apply((IWebHookBindingMetadata)bindingMetadata, bodyType, parameter);
                }
            }
        }
    }

    /// <inheritdoc />
    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        // No-op
    }

    private void Apply(
        IWebHookBindingMetadata bindingMetadata,
        WebHookBodyType? bodyType,
        ParameterModel parameter)
    {
        var bindingInfo = parameter.BindingInfo;
        if (bindingInfo?.BinderModelName != null ||
            bindingInfo?.BinderType != null ||
            bindingInfo?.BindingSource != null)
        {
            // User was explicit. Nothing to do.
            return;
        }

        if (bindingInfo == null)
        {
            bindingInfo = parameter.BindingInfo = new BindingInfo();
        }

        var parameterName = parameter.ParameterName;
        var parameterType = parameter.ParameterInfo.ParameterType;
        switch (parameterName.ToUpperInvariant())
        {
            case "ACTION":
            case "ACTIONS":
            case "ACTIONNAME":
            case "ACTIONNAMES":
                SourceEvent(bindingInfo, parameterType, parameterName);
                break;

            case "DATA":
                SourceData(bindingInfo, bodyType, parameterName);
                break;

            case "EVENT":
            case "EVENTS":
            case "EVENTNAME":
            case "EVENTNAMES":
                SourceEvent(bindingInfo, parameterType, parameterName);
                break;

            case "ID":
                SourceId(bindingInfo, parameterType, parameterName);
                break;

            case "RECEIVER":
            case "RECEIVERNAME":
                SourceReceiver(bindingInfo, parameterType, parameterName);
                break;

            case "RECEIVERID":
                SourceId(bindingInfo, parameterType, parameterName);
                break;

            case "WEBHOOKRECEIVER":
                SourceReceiver(bindingInfo, parameterType, parameterName);
                break;

            default:
                // If additional parameters are configured and match, map them. If not, treat IFormCollection,
                // JContainer and XElement parameters as data. IsAssignableFrom(...) looks reversed because this
                // check is about model binding system support, not an actual assignment to the parameter.
                if (!TrySourceAdditionalParameter(bindingInfo, bindingMetadata, parameterName) &&
                    (typeof(IFormCollection).IsAssignableFrom(parameterType) ||
                     typeof(JToken).IsAssignableFrom(parameterType) ||
                     typeof(XElement).IsAssignableFrom(parameterType)))
                {
                    SourceData(bindingInfo, bodyType, parameterName);
                }
                break;
        }
    }

    private void SourceData(
        BindingInfo bindingInfo,
        WebHookBodyType? bodyType,
        string parameterName)
    {
        if (!bodyType.HasValue)
        {
            _logger.LogWarning(
                0,
                "Not adding binding information for '{ParameterName}' parameter. WebHookBodyType is not known.",
                parameterName);
            return;
        }

        if (bodyType.Value == WebHookBodyType.Form)
        {
            bindingInfo.BinderModelName = WebHookConstants.ModelStateBodyModelName;
            bindingInfo.BindingSource = BindingSource.Form;
            return;
        }

        bindingInfo.BinderModelName = WebHookConstants.ModelStateBodyModelName;
        bindingInfo.BindingSource = BindingSource.Body;
    }

    private void SourceEvent(BindingInfo bindingInfo, Type parameterType, string parameterName)
    {
        // IsAssignableFrom(...) looks reversed because this check is about model binding system support, not an
        // actual assignment to the parameter.
        if (typeof(string) != parameterType &&
            !typeof(IEnumerable<string>).IsAssignableFrom(parameterType))
        {
            _logger.LogWarning(
                1,
                "Not adding binding information for '{ParameterName}' parameter of unsupported type " +
                "'{ParameterType}'.",
                parameterName,
                parameterType);
            return;
        }

        bindingInfo.BinderModelName = WebHookConstants.EventKeyName;
        bindingInfo.BindingSource = BindingSource.Path;
    }

    private void SourceId(BindingInfo bindingInfo, Type parameterType, string parameterName)
    {
        if (typeof(string) != parameterType)
        {
            _logger.LogWarning(
                2,
                "Not adding binding information for '{ParameterName}' parameter of unsupported type " +
                "'{ParameterType}'.",
                parameterName,
                parameterType);
            return;
        }

        bindingInfo.BinderModelName = WebHookConstants.IdKeyName;
        bindingInfo.BindingSource = BindingSource.Path;
    }

    private void SourceReceiver(BindingInfo bindingInfo, Type parameterType, string parameterName)
    {
        if (typeof(string) != parameterType)
        {
            _logger.LogWarning(
                3,
                "Not adding binding information for '{ParameterName}' parameter of unsupported type " +
                "'{ParameterType}'.",
                parameterName,
                parameterType);
            return;
        }

        bindingInfo.BinderModelName = WebHookConstants.ReceiverKeyName;
        bindingInfo.BindingSource = BindingSource.Path;
    }

    private static bool TrySourceAdditionalParameter(
        BindingInfo bindingInfo,
        IWebHookBindingMetadata bindingMetadata,
        string parameterName)
    {
        var parameter = bindingMetadata?.Parameters
            .FirstOrDefault(item => string.Equals(parameterName, item.Name, StringComparison.OrdinalIgnoreCase));
        if (parameter == null)
        {
            return false;
        }

        bindingInfo.BinderModelName = parameter.SourceName;
        switch (parameter.ParameterType)
        {
            case WebHookParameterType.Header:
                bindingInfo.BindingSource = BindingSource.Header;
                break;

            case WebHookParameterType.RouteValue:
                bindingInfo.BindingSource = BindingSource.Path;
                break;

            case WebHookParameterType.QueryParameter:
                bindingInfo.BindingSource = BindingSource.Query;
                break;

            default:
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.General_InvalidEnumValue,
                    typeof(WebHookParameterType),
                    parameter.ParameterType);
                throw new InvalidOperationException(message);
        }

        return true;
    }
}
