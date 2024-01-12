using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.CrmConnectControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApplication.Model;

namespace WpfApplication.Service
{
    public class MetadataService

    {
        private static ObservableCollection<EntityItem> entityItems = new ObservableCollection<EntityItem>();


        private static  ObservableCollection<AttributeItem> attributeItemsList = new ObservableCollection<AttributeItem>();


        public static ObservableCollection<EntityItem> RetrieveEntities(CrmConnectionManager crmConnectionMgr)

        {
            try
            {
                // Create a request to retrieve all entities
                var request = new RetrieveAllEntitiesRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                };



                // Execute the request
                var response = (RetrieveAllEntitiesResponse)crmConnectionMgr.CrmSvc.Execute(request);

                // Process the response and get the list of entities
                var entities = response.EntityMetadata;

                entityItems = new ObservableCollection<EntityItem>(entities.Select(entity => new EntityItem
                {
                    EntityName = entity.LogicalName,
                    IsSelected = false // Initial state is not selected
                }));


            }

            catch (Exception ex)

            {
                // Handle exception as needed
                MessageBox.Show($"Error retrieving entities: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return entityItems;

        }

    
        public  static ObservableCollection<AttributeItem> RetrieveAttributes(CrmConnectionManager crmConnectionMgr,List<string> selectEntities,int currentIndex)



        {
            currentIndex++;

            string entityName = selectEntities[currentIndex];

            var request = new RetrieveEntityRequest
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            try
            {
                // Execute the request
                 var response = (RetrieveEntityResponse)crmConnectionMgr.CrmSvc.Execute(request);

                var attributes = response.EntityMetadata;  


                        foreach (var attribute in attributes.Attributes)
                        {
                            attributeItemsList.Add(new AttributeItem
                            {
                                AttributeName = attribute.LogicalName,
                                IsSelected = false,
                            });
                        }
                    

                

            }

            catch (Exception ex)

            {
                MessageBox.Show($"Error retrieving attributes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return attributeItemsList;

        }


        public static EntityMetadata GetEntityMetadata(CrmConnectionManager crmConnectionMgr, string entityName)
        {
            var request = new RetrieveEntityRequest
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)crmConnectionMgr.CrmSvc.Execute(request);

            return response.EntityMetadata;
        }


    }
}
