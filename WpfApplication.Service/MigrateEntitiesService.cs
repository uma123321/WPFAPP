using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Windows;
using WpfApplication.Model;


namespace WpfApplication.Service
{
    public class MigrateEntitiesService

    {

        private static CrmServiceClient targetConnection;


        //       private static object lockObject = new object();

        public static string GetDisplayEntityName(string logicalName)

        {


            int underscoreIndex = logicalName.IndexOf('_');

            if (underscoreIndex >= 0 && underscoreIndex < logicalName.Length - 1)

            {
                string entityNamePart = logicalName.Substring(underscoreIndex + 1);

                return char.ToUpper(entityNamePart[0]) + entityNamePart.Substring(1);
            }

            // If no underscore is found, or logical name is empty, return the original logical name
            return logicalName;


        }

        public static string GetDisplayAttributeName(string logicalName)

        {


            int underscoreIndex = logicalName.IndexOf('_');

            if (underscoreIndex >= 0 && underscoreIndex < logicalName.Length - 1)

            {
                string attributeNamePart = logicalName.Substring(underscoreIndex + 1);

                return char.ToUpper(attributeNamePart[0]) + attributeNamePart.Substring(1);
            }

            // If no underscore is found, or logical name is empty, return the original logical name
            return logicalName;

        }


        public static void MigrateEntities(CrmServiceClient targetSvc, DmsEntity entityToMigrate)


        {

            targetConnection = targetSvc;


            bool a = EntityExists(entityToMigrate.EntityName);


            if (!a)


            {

                CreateEntityRequest createrequest = new CreateEntityRequest

                {

                    //Define the entity
                    Entity = new EntityMetadata

                    {
                        SchemaName = entityToMigrate.EntityName,
                        DisplayName = new Label(GetDisplayEntityName(entityToMigrate.EntityName), 1033),
                        DisplayCollectionName = new Label(GetDisplayEntityName(entityToMigrate.EntityName) + "s", 1033),
                        Description = new Label($"An entity to store information about {GetDisplayEntityName(entityToMigrate.EntityName)}", 1033),
                        OwnershipType = OwnershipTypes.UserOwned,

                        IsActivity = false,

                    },

                    // Define the primary attribute for the entity
                    PrimaryAttribute = new StringAttributeMetadata
                    {
                        SchemaName = "eka_" + GetDisplayEntityName(entityToMigrate.EntityName) + "name",
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                        MaxLength = 100,
                        FormatName = StringFormatName.Text,
                        DisplayName = new Label(GetDisplayEntityName(entityToMigrate.EntityName) + "Name", 1033),
                        Description = new Label($"The primary attribute for the {GetDisplayEntityName(entityToMigrate.EntityName)} entity.", 1033)
                    }

                };

                targetSvc.Execute(createrequest);

                Console.WriteLine($"The {GetDisplayEntityName(entityToMigrate.EntityName)} entity has been created.");



                if (entityToMigrate.Attributes != null && entityToMigrate.Attributes.Count != 0)
                {
                    foreach (var attribute in entityToMigrate.Attributes)

                    {
                        bool b = AttributeExists(attribute.AttributeName);


                        if (!b)

                        {

                            switch (attribute.AttributeType)
                            {

                                case "String":


                                    CreateAttributeRequest createBankNameAttributeRequest = new CreateAttributeRequest
                                    {
                                        EntityName = entityToMigrate.EntityName,
                                        Attribute = new StringAttributeMetadata
                                        {
                                            SchemaName = attribute.AttributeName,
                                            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                                            MaxLength = 100,
                                            FormatName = StringFormatName.Text,
                                            DisplayName = new Label(GetDisplayAttributeName(attribute.AttributeName), 1033),
                                            Description = new Label($"The  {GetDisplayAttributeName(attribute.AttributeName)} of the  {entityToMigrate.EntityName}", 1033)
                                        }
                                    };

                                    targetSvc.Execute(createBankNameAttributeRequest);

                                    break;

                                case "DateTime":

                                    CreateAttributeRequest createCheckedDateRequest = new CreateAttributeRequest
                                    {
                                        EntityName = entityToMigrate.EntityName,
                                        Attribute = new DateTimeAttributeMetadata
                                        {
                                            SchemaName = attribute.AttributeName,
                                            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                                            Format = DateTimeFormat.DateOnly,
                                            DisplayName = new Label(GetDisplayAttributeName(attribute.AttributeName), 1033),
                                            Description = new Label($"The {GetDisplayAttributeName(attribute.AttributeName)} of the {entityToMigrate.EntityName} was last confirmed", 1033)

                                        }
                                    };

                                    targetSvc.Execute(createCheckedDateRequest);
                                    Console.WriteLine("An date attribute has been added to the bank account entity.");

                                    break;

                                case "Lookup":

                                    {

                                        CreateOneToManyRequest req = new CreateOneToManyRequest()
                                        {
                                            Lookup = new LookupAttributeMetadata()
                                            {
                                                Description = new Label("The referral (lead) from the bank account owner", 1033),
                                                DisplayName = new Label("Referral", 1033),
                                                LogicalName = "new_parent_" + attribute.AttributeName,
                                                SchemaName = "New_Parent_" + attribute.AttributeName,
                                                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.Recommended)
                                            },
                                            OneToManyRelationship = new OneToManyRelationshipMetadata()
                                            {
                                                AssociatedMenuConfiguration = new AssociatedMenuConfiguration()
                                                {
                                                    Behavior = AssociatedMenuBehavior.UseCollectionName,
                                                    Group = AssociatedMenuGroup.Details,
                                                    Label = new Label("Bank Accounts", 1033),
                                                    Order = 10000
                                                },
                                                CascadeConfiguration = new CascadeConfiguration()
                                                {
                                                    Assign = CascadeType.Cascade,
                                                    Delete = CascadeType.Cascade,
                                                    Merge = CascadeType.Cascade,
                                                    Reparent = CascadeType.Cascade,
                                                    Share = CascadeType.Cascade,
                                                    Unshare = CascadeType.Cascade
                                                },
                                                ReferencedEntity = attribute.RelatedEntity,
                                                ReferencedAttribute = attribute.AttributeName,
                                                ReferencingEntity = entityToMigrate.EntityName,
                                                SchemaName = attribute.AttributeName
                                            }
                                        };

                                        targetSvc.Execute(req);

                                    }

                                    break;


                                case "Money":

                                    CreateAttributeRequest createBalanceAttributeRequest = new CreateAttributeRequest
                                    {
                                        EntityName = entityToMigrate.EntityName,

                                        Attribute = new MoneyAttributeMetadata
                                        {
                                            SchemaName = "new_balance",
                                            RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                                            PrecisionSource = 2,
                                            DisplayName = new Label("Balance", 1033),
                                            Description = new Label("Account Balance at the last known date", 1033),

                                        }
                                    };

                                    targetSvc.Execute(createBalanceAttributeRequest);


                                    break;

                                default:

                                    break;

                            }

                        }
                        else
                        {
                            Console.WriteLine($"{attribute.AttributeName} already exists");
                        }

                    }
                }



            }

            else

            {
                Console.WriteLine($"{entityToMigrate.EntityName} already exists");

            }


        }

        static bool EntityExists(string entityName)

        {

            EntityMetadata[] entityMetadatas = MetadataService.GetMetadata(targetConnection);


            foreach (var entityMetadata in entityMetadatas)

            {

                if (entityName == entityMetadata.LogicalName)

                {

                    return true;

                }

            }

            return false;
        }


        static bool AttributeExists(string attributeName)

        {




            EntityMetadata[] entityMetadatas = MetadataService.GetMetadata(targetConnection);


            foreach (var entityMetadata in entityMetadatas)

            {

                foreach (var attributeMetadata in entityMetadata.Attributes)

                {

                    if (attributeMetadata.LogicalName == attributeName)

                    {
                        return true;


                    }


                }

            }

            return false;
        }

    }
}





