using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.CrmConnectControl;
using PowerApps.Samples.LoginUX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApplication.Model;
using WpfApplication.Service;

using static WpfApplication.App;

namespace WpfTutorial
{
    public partial class MainWindow : Window


    {
        private ExampleLoginForm ctrl;
        private static List<string> selectedEntities = new List<string>();// Class-level variable to store selected entities
        private static ObservableCollection<EntityItem> Entities = new ObservableCollection<EntityItem>();
        private List<string> selectedAttributes = new List<string>();
        private ObservableCollection<AttributeItem> Attributes = new ObservableCollection<AttributeItem>();
        private List<DmsAttribute> dmsAttributes = new List<DmsAttribute>();
        private List<DmsEntity> dmsEntities = new List<DmsEntity>();
        private List<string> relatedEntity = new List<string>();
        private List<string> copyRelatedEntity = new List<string>();
        public int currentIndex = -1;


        private void connectcrm(object sender, RoutedEventArgs e)
        {
            ctrl = new ExampleLoginForm();
            ctrl.ConnectionToCrmCompleted += ctrl_ConnectionToCrmCompleted;
            ctrl.ShowDialog();

            if (ctrl.CrmConnectionMgr != null && ctrl.CrmConnectionMgr.CrmSvc != null && ctrl.CrmConnectionMgr.CrmSvc.IsReady)
            {
                //Brushes.LightGreen
                btnSignIn.Content = "Connected";

                btnSignIn.Background = Brushes.Silver;
               // MessageBox.Show("Connected to Dataverse version: " + ctrl.CrmConnectionMgr.CrmSvc.ConnectedOrgVersion.ToString() +
                  //  " Organization: " + ctrl.CrmConnectionMgr.CrmSvc.ConnectedOrgUniqueName, "Connection Status");

                btnRetrieveEntities.IsEnabled = true;

            }
            else
            {
                MessageBox.Show("Cannot connect; try again!", "Connection Status");
            }
        }

        private void ctrl_ConnectionToCrmCompleted(object sender, EventArgs e)
        {
            if (sender is ExampleLoginForm)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ((ExampleLoginForm)sender).Close();
                });
            }
        }

        private void retrieveEntities(object sender, RoutedEventArgs e)

        {
            btnSave.Visibility = Visibility.Visible;

            Entities = MetadataService.RetrieveEntities(ctrl.CrmConnectionMgr);

            EntitiesListBox.ItemsSource = Entities;
        }



        private void retrieveAttributes(object sender, RoutedEventArgs e)

        {

            btnNext.Visibility = Visibility.Visible;

            Attributes = MetadataService.RetrieveAttributes(ctrl.CrmConnectionMgr, selectedEntities, currentIndex);


            AttributesListBox.ItemsSource = Attributes;

            AttributesListBox.Visibility = Visibility.Visible;



        }

        private void orderEntities(object sender, RoutedEventArgs e)

        {

        List<DmsEntity>  orderedEntities =   OrderEntities.PrioritizeEntities(dmsEntities);


            OrderedEntitiesListBox.ItemsSource = orderedEntities;

        }


        private void migrate(object sender, RoutedEventArgs e)

        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)

        {


            selectedEntities = Entities.Where(item => item.IsSelected).Select(item => item.EntityName).ToList();


            EntitiesListBox.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;



            btnRetrieveEntities.IsEnabled = false;
            btnRetrieveAttributes.IsEnabled = true;


        }
        private void btnBack_Click(object sender, RoutedEventArgs e)

        {



            EntitiesListBox.Visibility = Visibility.Visible;
            btnSave.Visibility = Visibility.Visible;
            btnBack.Visibility = Visibility.Hidden;

        }



        private void btnNext_Click(object sender, RoutedEventArgs e)

        {
            currentIndex++;


                string entity = selectedEntities[currentIndex];

                var entityMetadata = MetadataService.GetEntityMetadata(ctrl.CrmConnectionMgr, selectedEntities[currentIndex]); ;

                if (!string.IsNullOrEmpty(entity))

                {
                    selectedAttributes = Attributes
                        .Where(item => item.IsSelected)
                        .Select(item => item.AttributeName)
                        .ToList();


                    foreach (var attribute in selectedAttributes)

                    {

                        DmsAttribute newAttribute = new DmsAttribute
                        {
                            AttributeName = attribute,
                            AttributeType = GetAttributeType(entityMetadata, attribute),
                            RelatedEntity = GetLookupEntity(entityMetadata, attribute)
                        };

                        dmsAttributes.Add(newAttribute);
                    }



                    dmsEntities.Add(new DmsEntity
                    {
                        EntityName = entity,
                        Attributes = new List<DmsAttribute>(dmsAttributes) // Create a new list with the same elements
                    });

                    dmsAttributes.Clear();


                    Attributes.Clear();

                    /* this.Dispatcher.Invoke(() =>
                     {*/
                    //  AttributesListBox.ItemsSource = null;
                    AttributesListBox.ItemsSource = Attributes;
                    /*  });*/


                    if (currentIndex + 1 < selectedEntities.Count)

                    {

                        MetadataService.RetrieveAttributes(ctrl.CrmConnectionMgr, selectedEntities, currentIndex);


                        AttributesListBox.Visibility = Visibility.Visible;
                    }


                    else

                    {

                        foreach (var dmsEntity in dmsEntities)

                        {

                            foreach (var dmsAttribute in dmsEntity.Attributes)

                            {


                                if (dmsAttribute.AttributeType == "Lookup" && !selectedEntities.Contains(dmsAttribute.RelatedEntity))

                                {

                                    relatedEntity.Add(dmsAttribute.RelatedEntity);

                                }

                            }

                        }


               //         MessageBox.Show($"The missing dependencies in the selected entities are {string.Join(", ", relatedEntity)}, Please click on the Back Button to select the missing entities", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);

                        copyRelatedEntity = relatedEntity.ToList();

                        RelatedEntityListBox.ItemsSource = copyRelatedEntity;



                        if (relatedEntity.Count == 0)

                        {
                            btnOrderEntities.IsEnabled = true;
                            btnRetrieveAttributes.IsEnabled = false;
                        }

                        relatedEntity.Clear();

                        btnBack.Visibility = Visibility.Visible;
                        btnNext.Visibility = Visibility.Collapsed;

                        // If there are no more entities, hide the AttributesListBox

                        AttributesListBox.Visibility = Visibility.Collapsed;
                    }
                }
            
        }

        public static string GetAttributeType(EntityMetadata entityMetadata, string attributeName)

        {
            var attribute = entityMetadata.Attributes.FirstOrDefault(attr => attr.LogicalName == attributeName);

            return attribute != null ? attribute.AttributeType.Value.ToString() : "Unknown";

        }

        private static string GetLookupEntity(EntityMetadata entityMetadata, string attributeName)

        {
            var attribute = entityMetadata.Attributes.FirstOrDefault(attr => attr.LogicalName == attributeName);

            if (attribute != null && attribute is LookupAttributeMetadata lookupAttribute)
            {
                return lookupAttribute.Targets.First(); // Assuming a single target for simplicity
            }

            return "Unknown";

        }

    }

}
