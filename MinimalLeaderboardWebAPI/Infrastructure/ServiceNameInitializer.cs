﻿using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace MinimalLeaderboardWebAPI.Infrastructure
{
    public class ServiceNameInitializer : ITelemetryInitializer
    {
        /// <inheritdoc />
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null) throw new ArgumentNullException(nameof(telemetry));
            telemetry.Context.Cloud.RoleName = "Leaderboard Web API";
        }
    }
}