using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace FixRM.Plugins
{
    [CrmPluginRegistration(
        message: "PublishAll",
        entityLogicalName: "none",
        stage: StageEnum.PostOperation,
        executionMode: ExecutionModeEnum.Asynchronous,
        filteringAttributes: null,
        stepName: "FixRM.Plugins.PublishAllHandler",
        executionOrder: 100,
        isolationModel: IsolationModeEnum.Sandbox)]
    public class PublishAllHandler : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            /// Set up context and organization service
            IPluginExecutionContext context = serviceProvider.GetPluginExecutionContext();

            IOrganizationServiceFactory serviceFactory = serviceProvider.GetOrganizationServiceFactory();
            IOrganizationService organizationService = serviceFactory.CreateOrganizationService(context.UserId);

            /// Query rules:
            /// Name should start with "AlwaysOn" 
            /// AND 
            /// State should be "Draft"
            QueryExpression query = new QueryExpression
            {
                EntityName = "duplicaterule",
                ColumnSet = new ColumnSet("duplicateruleid", "name"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions = {
                        new ConditionExpression
                        {
                            AttributeName = "name",
                            Operator = ConditionOperator.BeginsWith,
                            Values = { "AlwaysOn" }
                        },
                        new ConditionExpression
                        {
                            AttributeName = "statuscode",
                            Operator = ConditionOperator.Equal,
                            Values = { 0 }
                        }
                    }
                }
            };

            EntityCollection rules = organizationService.RetrieveMultiple(query);

            /// Publish rules
            foreach (Entity rule in rules.Entities)
            {
                organizationService.Execute(new PublishDuplicateRuleRequest()
                {
                    DuplicateRuleId = rule.Id
                });
            }
        }
    }
}
