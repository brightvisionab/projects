//FOR SERVER SIDE USED IN PARTICULAR

#region SETTING SQL SERVER
/*
 *
 *--reference: http://www.codeproject.com/Articles/19954/Execute-NET-Code-under-SQL-Server-2005
    ------------------------------------------------------------------------------------------------------
    sp_configure 'clr enable', 1
    GO
    RECONFIGURE
    GO
    ------------------------------------------------------------------------------------------------------
    ALTER DATABASE TestingCLR SET TRUSTWORTHY ON
    ------------------------------------------------------------------------------------------------------
    GO
    IF  EXISTS (SELECT * FROM sys.assemblies WHERE name  = 'RestSharp')
    DROP ASSEMBLY RestSharp
    GO

    GO
    CREATE ASSEMBLY RestSharp
    AUTHORIZATION dbo
    FROM 'D:\WORKING FOLDER\Brightvision\BrightVision.ServerSide.SMS\bin\Debug\RestSharp.dll'
    WITH PERMISSION_SET = UNSAFE
    GO
    ------------------------------------------------------------------------------------------------------
 
    GO
    IF  EXISTS (SELECT * FROM sys.assemblies WHERE name  = 'BrightVisionServerSideSMS')
    DROP ASSEMBLY BrightVisionServerSideSMS
    GO

    CREATE ASSEMBLY BrightVisionServerSideSMS
    AUTHORIZATION dbo
    FROM 'D:\WORKING FOLDER\Brightvision\BrightVision.ServerSide.SMS\bin\Debug\BrightVision.ServerSide.SMS.dll'
    WITH PERMISSION_SET = UNSAFE
    GO
    ------------------------------------------------------------------------------------------------------


    
    IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_UseBrightVisionServerSideSMS]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_UseBrightVisionServerSideSMS]
    GO

    CREATE PROCEDURE usp_UseBrightVisionServerSideSMS
    @to nvarchar(14),
    @body nvarchar(200),
    @msg nvarchar(MAX)OUTPUT
    AS EXTERNAL NAME BrightVisionServerSideSMS.[BrightVision.ServerSide.SMS.BaseFunctionClass].SendSmsMessage
    GO
    ------------------------------------------------------------------------------------------------------


    
    DECLARE @msg varchar(MAX)
    EXEC usp_UseBrightVisionServerSideSMS '+639069163504', 'Testing Sending SMS From Server Side', @msg output
    PRINT @msg
   
 * 
*/
#endregion

#region NAMESPACE
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
#endregion

namespace BrightVision.ServerSide.SMS
{
    public class BaseFunctionClass
    {
        #region "Default Constructor"
        public BaseFunctionClass()
        {

        }
        #endregion

        #region "Send SMS"
        /// <summary>
        /// This function will be called from the SQL Stored Procedure.
        /// </summary>
        /// <param name=""strName"">Name</param>
        /// <returns>Welcome Message</returns>
        [SqlProcedure]
        public static void SendSmsMessage(SqlString to, SqlString body, out SqlString strMessge)
        {
            // instantiate a new Twilio Rest Client
            var client = new TwilioSMS();

            var message = client.SendSmsMessage(to.ToString(), body.ToString());


            strMessge = message.Sid;
        }

        #endregion
    }
}
