using System;
using Microsoft.Xrm.Sdk;
using FileWatcherMonitoring.Plugins;

namespace FileWatcherMonitoring.Dataverse
{
    /// <summary>
    /// Backing plugin for Custom API fwm_CheckMissingSla.
    ///
    /// Custom API definition (created in the maker portal or via solution):
    ///   Unique name: fwm_CheckMissingSla
    ///   Request parameter:  InterfaceId (String, required) — business id, e.g. "SA-034"
    ///   Response property:  EventCount  (Integer)
    ///
    /// Called per enabled interface by the scheduled missing-SLA sweep flow. The
    /// sweep itself pages nothing here — one interface per call keeps each
    /// execution far under the 2-minute plugin budget.
    /// </summary>
    public class CheckMissingSlaPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            var interfaceId = context.InputParameters.Contains("InterfaceId")
                ? context.InputParameters["InterfaceId"] as string
                : null;
            if (string.IsNullOrEmpty(interfaceId))
            {
                throw new InvalidPluginExecutionException("fwm_CheckMissingSla requires the InterfaceId parameter.");
            }

            var config = ConfigLoader.LoadByInterfaceId(service, interfaceId);

            var sweep = new MissingSlaSweep(new DataverseStateRepository(service));
            var events = sweep.CheckMissingSla(config, DateTime.UtcNow);

            foreach (var fileEvent in events)
            {
                service.Create(EventWriter.ToEntity(fileEvent));
            }

            context.OutputParameters["EventCount"] = events.Length;
        }
    }
}
