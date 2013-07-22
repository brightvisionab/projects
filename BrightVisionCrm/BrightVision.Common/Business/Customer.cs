using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using System.Data.Objects;

namespace BrightVision.Common.Business
{
    public class ObjectCustomer
    {
        #region Enumerations
        /// <summary>
        /// Enum for view type use
        /// </summary>
        public enum eViewType
        {
            UserView,
            CustomerView,
            SubCampaignView,
            ComboListView,
            CustomerCampaign_ManagerAdmin,
            CustomerCampaign_ManagerUser
        }
        #endregion

        #region Classes & Properties
        /// <summary>
        /// Gets or sets customer instance
        /// </summary>
        public class CustomerInstance
        {
            public int id { get; set; }
            public string customer_name { get; set; }
            public string org_no { get; set; }
            public string reference_no { get; set; }
            public bool active { get; set; }
            public string owner_name { get; set; }
            public string address { get; set; }
            public string description { get; set; }
        }
        #endregion

        #region Business Methods
        /// <summary>
        /// Get the customer id via customer nr
        /// </summary>
        public static int GetCustomerIdByNo(string CustomerNo)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCustomer = m_objBrightPlatformEntity.customers.Where(i => i.customer_nr.Equals(CustomerNo)).Single();
            return objCustomer.id;
        }

        /// <summary>
        /// Get the customer number via customer id
        /// </summary>
        public static string GetCustomerNo(int CustomerId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCustomer = m_objBrightPlatformEntity.customers.Where(i => i.id == CustomerId).Single();
            return objCustomer.customer_nr;
        }

        /// <summary>
        /// Gets the manager records
        /// </summary>
        public static ObjectQuery GetCustomers(int UserId)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objEntityCustomers =
                from objCustomer in m_objBrightPlatformEntity.customers
                join objCustomerUser in m_objBrightPlatformEntity.user_customers on objCustomer.id equals objCustomerUser.customer_id
                orderby objCustomer.customer_name
                where objCustomerUser.user_id == UserId && objCustomer.customer_name != "" && objCustomer.disabled.Equals("0")
                select new
                {
                    id = objCustomer.id,
                    customer_name = objCustomer.customer_name
                };

            return (ObjectQuery) objEntityCustomers;
        }

        /// <summary>
        /// Gets the manager records
        /// </summary>
        public static ObjectQuery GetCustomers(eViewType ViewType)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            ObjectQuery objCustomers = null;

            switch (ViewType)
            {
                #region eViewType.SubCampaignView
                case eViewType.SubCampaignView:
                {
                    var objEntityCustomers = 
                        from objCustomer in m_objBrightPlatformEntity.customers
                        where objCustomer.customer_name != ""
                        orderby objCustomer.customer_name
                        select new
                        {
                            id = objCustomer.id,
                            name = objCustomer.customer_name
                        };

                    objCustomers = (ObjectQuery) objEntityCustomers;
                    break;
                }
                #endregion

                #region eViewType.UserView
                case eViewType.UserView:
                {
                    var objEntityCustomers = 
                        from objCustomer in m_objBrightPlatformEntity.customers
                        orderby objCustomer.customer_name
                        select new
                        {
                            id = objCustomer.id,
                            customer_name = objCustomer.customer_name
                        };

                    objCustomers = (ObjectQuery) objEntityCustomers;
                    break;
                }
                #endregion

                #region eViewType.CustomerView
                case eViewType.CustomerView:
                {
                    var objEntityCustomers = 
                        from objCustomer in m_objBrightPlatformEntity.customers
                        where objCustomer.customer_name != ""
                        orderby objCustomer.customer_name
                        select new ObjectCustomer.CustomerInstance
                        {
                            id = objCustomer.id,
                            customer_name = objCustomer.customer_name,
                            org_no = objCustomer.org_no,
                            reference_no = objCustomer.reference_no,
                            active = objCustomer.disabled.Equals("0")? true: false,
                            owner_name = objCustomer.owner,
                            address = objCustomer.ship_address1 + " " + objCustomer.ship_address2 + " " + objCustomer.ship_address3 + " " + objCustomer.ship_address4 + " " + objCustomer.ship_address5,
                            description = objCustomer.description
                        };

                    objCustomers = (ObjectQuery) objEntityCustomers;
                    break;
                }
                #endregion

                #region eViewType.ComboListView
                case eViewType.ComboListView:
                {
                    var objEntityCustomers =
                        from objCustomer in m_objBrightPlatformEntity.customers
                        orderby objCustomer.customer_name
                        where objCustomer.customer_name != ""
                        select new
                        {
                            id = objCustomer.id,
                            customer_name = objCustomer.customer_name
                        };

                    objCustomers = (ObjectQuery) objEntityCustomers;
                    break;
                }
                #endregion

                #region eViewType.CustomerCampaign_ManagerAdmin
                case eViewType.CustomerCampaign_ManagerAdmin:
                {
                    var objEntityCustomers =
                        from objCustomer in m_objBrightPlatformEntity.customers
                        where objCustomer.customer_name != ""
                        orderby objCustomer.customer_name
                        select new ObjectCustomer.CustomerInstance
                        {
                            id = objCustomer.id,
                            customer_name = objCustomer.customer_name,
                            org_no = objCustomer.org_no,
                            reference_no = objCustomer.reference_no,
                            active = objCustomer.disabled.Equals("0") ? true : false,
                            owner_name = objCustomer.owner,
                            address = objCustomer.ship_address1 + " " + objCustomer.ship_address2 + " " + objCustomer.ship_address3 + " " + objCustomer.ship_address4 + " " + objCustomer.ship_address5,
                            description = objCustomer.description
                        };

                    objCustomers = (ObjectQuery)objEntityCustomers;
                    break;
                }
                #endregion

                #region eViewType.CustomerCampaign_ManagerUser
                case eViewType.CustomerCampaign_ManagerUser:
                {
                    var objEntityCustomers =
                        from objCustomer in m_objBrightPlatformEntity.customers
                        join objUserCustomer in m_objBrightPlatformEntity.user_customers on objCustomer.id equals objUserCustomer.customer_id
                        where objCustomer.customer_name != "" && objUserCustomer.user_id == UserSession.CurrentUser.UserId
                        orderby objCustomer.customer_name
                        select new ObjectCustomer.CustomerInstance
                        {
                            id = objCustomer.id,
                            customer_name = objCustomer.customer_name,
                            org_no = objCustomer.org_no,
                            reference_no = objCustomer.reference_no,
                            active = objCustomer.disabled.Equals("0") ? true : false,
                            owner_name = objCustomer.owner,
                            address = objCustomer.ship_address1 + " " + objCustomer.ship_address2 + " " + objCustomer.ship_address3 + " " + objCustomer.ship_address4 + " " + objCustomer.ship_address5,
                            description = objCustomer.description
                        };

                    objCustomers = (ObjectQuery)objEntityCustomers;
                    break;
                }
                #endregion
            }

            return objCustomers;
        }

        /// <summary>
        /// Save customer record
        /// </summary>
        public static void SaveCustomer(bool IsNew, CustomerInstance objParams)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCustomer = m_objBrightPlatformEntity.customers.CreateObject();
            if (!IsNew)
            {
                objCustomer = null;
                objCustomer = m_objBrightPlatformEntity.customers.Where(objField => objField.id == objParams.id).SingleOrDefault();
            }
            else
                objCustomer.created_date = DateTime.Now;

            objCustomer.customer_name = objParams.customer_name;
            objCustomer.org_no = objParams.org_no;
            objCustomer.reference_no = objParams.reference_no;
            objCustomer.disabled = objParams.active == true? "0": "1";
            objCustomer.owner = objParams.owner_name;
            //objCustomer.address = objParams.address;
            objCustomer.description = objParams.description;
            objCustomer.modified_by = UserSession.CurrentUser.UserId;
            objCustomer.modified_date = DateTime.Now;

            if (IsNew)
                m_objBrightPlatformEntity.customers.AddObject(objCustomer);

            m_objBrightPlatformEntity.SaveChanges();
        }

        /// <summary>
        /// Returns the customer id
        /// </summary>
        public static int GetCustomerId(string CustomerName)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCustomer = m_objBrightPlatformEntity.customers.CreateObject();
            objCustomer = m_objBrightPlatformEntity.customers.Where(objField => objField.customer_name == CustomerName).SingleOrDefault();

            return objCustomer.id;
        }

        /// <summary>
        /// Set customer status (e.g. active, inactive)
        /// </summary>
        public static void SetCustomerStatus(int CustomerId, bool IsActivated)
        {
            BrightPlatformEntities m_objBrightPlatformEntity = new BrightPlatformEntities(UserSession.EntityConnection);
            var objCustomer = m_objBrightPlatformEntity.customers.Where(objField => objField.id == CustomerId).SingleOrDefault();
            objCustomer.disabled = IsActivated? "0": "1";
            objCustomer.modified_by = UserSession.CurrentUser.UserId;
            objCustomer.modified_date = DateTime.Now;
            m_objBrightPlatformEntity.SaveChanges();
        }
        #endregion
    }
}