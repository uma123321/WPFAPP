using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApplication.Model;

namespace WpfApplication.Service


{
    public class OrderEntities



    {
        public static List<DmsEntity> PrioritizeEntities(List<DmsEntity> dmsEntities)


        {

            List<DmsEntity> checkList = new List<DmsEntity>();

            foreach (var entity in dmsEntities)

            {

                foreach (var attribute in entity.Attributes)

                {
                    if (attribute.AttributeType == "Lookup")

                    {
                        if (!checkList.Any(a => a.EntityName == attribute.RelatedEntity))
                        {

                            checkList.Insert(0, new DmsEntity

                            {

                                EntityName = attribute.RelatedEntity,
                                Operation = "Create"

                            });

                        }
                    }

                    if (!checkList.Any(a => a.EntityName == entity.EntityName))

                    {

                        checkList.Add(new DmsEntity

                        {

                            EntityName = entity.EntityName,
                            Operation = "Create"

                        });

                    }

                }
            }


            List<DmsEntity> copyList = checkList.ToList();


            for (var i = 0; i < checkList.Count; i++)

            {
                var check = checkList[i];

                if (dmsEntities.Any(a => a.EntityName == check.EntityName))

                {
                    var matchingEntity = dmsEntities.First(a => a.EntityName == check.EntityName);

                    foreach (var attribute in matchingEntity.Attributes
                        )

                    {

                        for (var j = i + 1; j < checkList.Count; j++)

                        {
                            if (attribute.RelatedEntity == checkList[j].EntityName)

                            {
                                copyList.Add(new DmsEntity

                                {

                                    EntityName = check.EntityName,

                                    Operation = "Update"

                                });

                            }

                        }
                    }

                }

            }

            return copyList;


        }

    }



}
