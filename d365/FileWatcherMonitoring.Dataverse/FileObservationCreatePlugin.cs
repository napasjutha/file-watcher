using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using FileWatcherMonitoring.Plugins;

namespace FileWatcherMonitoring.Dataverse
{
    /// <summary>
    /// The transactional heart of the D365-native watcher. Register SYNCHRONOUS,
    /// PostOperation, on Create of fwm_fileobservation. Because the state upsert
    /// and event insert run on the pipeline IOrganizationService, they commit or
    /// roll back atomically with the observation create — this transaction is why
    /// the old Gateway/outbox pattern is not ported.
    ///
    /// Fail-fast: invalid transitions and interface mismatches surface as
    /// InvalidPluginExecutionException (per-observation isolation is inherent —
    /// each create is its own pipeline).
    /// </summary>
    public class FileObservationCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            if (!(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity target)
                || target.LogicalName != Schema.FileObservationTable.LogicalName)
            {
                return;
            }

            try
            {
                var observation = new FileObservation
                {
                    InterfaceId = target.GetAttributeValue<string>(Schema.FileObservationTable.InterfaceId),
                    Path = target.GetAttributeValue<string>(Schema.FileObservationTable.FilePath),
                    Size = target.GetAttributeValue<long>(Schema.FileObservationTable.FileSizeBytes),
                    Mtime = target.GetAttributeValue<DateTime>(Schema.FileObservationTable.ModifiedAt)
                };

                var config = ConfigLoader.LoadByInterfaceId(service, observation.InterfaceId);

                var engine = new WatcherEngine(new DataverseStateRepository(service));
                var fileEvent = engine.ProcessObservation(observation, config, DateTime.UtcNow);

                if (fileEvent != null)
                {
                    service.Create(EventWriter.ToEntity(fileEvent));
                }
            }
            catch (InterfaceMismatchException ex)
            {
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
            catch (InvalidStateTransitionException ex)
            {
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
        }
    }

    internal static class ConfigLoader
    {
        public static InterfaceConfig LoadByInterfaceId(IOrganizationService service, string interfaceId)
        {
            var query = new QueryExpression(Schema.InterfaceTable.LogicalName)
            {
                ColumnSet = new ColumnSet(true),
                TopCount = 1
            };
            query.Criteria.AddCondition(Schema.InterfaceTable.InterfaceId, ConditionOperator.Equal, interfaceId);

            var results = service.RetrieveMultiple(query);
            if (results.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException("No fwm_interface row found for interface id " + interfaceId);
            }
            var row = results.Entities[0];

            return new InterfaceConfig
            {
                InterfaceId = row.GetAttributeValue<string>(Schema.InterfaceTable.InterfaceId),
                InterfaceName = row.GetAttributeValue<string>(Schema.InterfaceTable.Name),
                InboundPath = row.GetAttributeValue<string>(Schema.InterfaceTable.InboundPath),
                FilePattern = row.GetAttributeValue<string>(Schema.InterfaceTable.FilePattern),
                PollIntervalSeconds = row.GetAttributeValue<int>(Schema.InterfaceTable.PollIntervalSeconds),
                StabilityCheckSeconds = row.GetAttributeValue<int>(Schema.InterfaceTable.StabilityCheckSeconds),
                DuplicateCheckEnabled = row.GetAttributeValue<bool>(Schema.InterfaceTable.DuplicateCheckEnabled),
                StuckThresholdSeconds = row.GetAttributeValue<int>(Schema.InterfaceTable.StuckThresholdSeconds),
                SlaDeadline = row.GetAttributeValue<string>(Schema.InterfaceTable.SlaDeadline),
                EnabledFlag = row.GetAttributeValue<bool>(Schema.InterfaceTable.Enabled)
            };
        }
    }

    internal static class EventWriter
    {
        public static Entity ToEntity(FileEvent fileEvent)
        {
            var entity = new Entity(Schema.FileEventTable.LogicalName);
            entity[Schema.FileEventTable.EventId] = fileEvent.EventId;
            entity[Schema.FileEventTable.EventType] = new OptionSetValue(Schema.ToChoice(fileEvent.EventType));
            entity[Schema.FileEventTable.BatchId] = fileEvent.BatchId;
            entity[Schema.FileEventTable.InterfaceId] = fileEvent.InterfaceId;
            entity[Schema.FileEventTable.FilePath] = fileEvent.FilePath;
            entity[Schema.FileEventTable.OccurredAt] = fileEvent.OccurredAt;
            return entity;
        }
    }
}
