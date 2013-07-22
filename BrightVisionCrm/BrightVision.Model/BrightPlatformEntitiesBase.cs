using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.Reflection;
using System.Threading;
using System.Data.Metadata.Edm;
using System.Globalization;

namespace BrightVision.Model
{
    public class BrightPlatformEntitiesBase : ObjectContext
    {
       #region Constructors
    
        /// <summary>
        /// Initializes a new BrightPlatformEntities object using the connection string found in the 'BrightPlatformEntities' section of the application configuration file.
        /// </summary>
        public BrightPlatformEntitiesBase(string name, string entity)
            : base(name, entity)
        {
             
        }

       

        /// <summary>
        /// Initialize a new BrightPlatformEntities object.
        /// </summary>
        public BrightPlatformEntitiesBase(EntityConnection connection, string entity)
            : base(connection, entity)
        {
             
        }

 

        void Connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            try
            {
                if (e.CurrentState == System.Data.ConnectionState.Broken)
                {
                    Thread.Sleep(1000);
                    this.Connection.Close();
                    this.Connection.Open();
                }
            }
            catch { }
        }
    
        #endregion

        #region public
        public new ObjectResult<T> ExecuteFunction<T>(string storeProc, params ObjectParameter[] parameter1){
            try
            {
                return base.ExecuteFunction<T>(storeProc, parameter1);
              
            }
            catch {
                Thread.Sleep(1000);
                ExecuteFunction<T>(storeProc, parameter1);
            }
            return null;
        }      
        #endregion
    }

   
        
}
