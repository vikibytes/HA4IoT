﻿using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class VirtualButtonGroup : ActuatorBase<ActuatorSettings>
    {
        private readonly Dictionary<ActuatorId, VirtualButton> _buttons = new Dictionary<ActuatorId, VirtualButton>();

        public VirtualButtonGroup(ActuatorId id, IApiController apiController)
            : base(id, apiController)
        {
            Settings = new ActuatorSettings(id);
        }

        public VirtualButtonGroup WithButton(ActuatorId id, Action<VirtualButton> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            if (_buttons.ContainsKey(id))
            {
                throw new InvalidOperationException("Button with id " + id + " already part of the button group.");
            }

            var virtualButton = new VirtualButton(id, ApiController);
            initializer(virtualButton);

            _buttons.Add(id, virtualButton);

            return this;
        }

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            var button = apiContext.Request.GetNamedString("button", string.Empty);

            if (string.IsNullOrEmpty(button))
            {
                throw new BadRequestException("Button is not set.");
            }

            VirtualButton virtualButton;
            if (!_buttons.TryGetValue(new ActuatorId(button), out virtualButton))
            {
                throw new BadRequestException("The specified button is unknown.");
            }

            virtualButton.ForwardApiCommand(apiContext);
        }

        public override JsonObject ExportConfigurationToJsonObject()
        {
            JsonObject configuration = base.ExportConfigurationToJsonObject();

            JsonArray buttonIds = new JsonArray();
            foreach (var button in _buttons)
            {
                buttonIds.Add(JsonValue.CreateStringValue(button.Key.Value));
            }
            
            configuration.SetNamedValue("buttons", buttonIds);

            return configuration;
        }
    }
}
