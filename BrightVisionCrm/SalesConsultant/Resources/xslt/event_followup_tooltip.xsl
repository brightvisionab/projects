<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
    exclude-result-prefixes="msxsl"  xmlns:util="util:xsltextension"
    extension-element-prefixes="xsl util"
>
  <xsl:output method="html" />

    <xsl:template match="/">
        <xsl:choose>
            <xsl:when test="CTScEventAndFollowUpLog/event_type='Nurture Event'">
              <xsl:call-template name="NurtureEvent"/>
            </xsl:when>
            <xsl:when test="CTScEventAndFollowUpLog/event_type='Nurture Log'">
              <xsl:call-template name="NurtureLog"/>
            </xsl:when>
            <xsl:when test="CTScEventAndFollowUpLog/event_type='Make Call'">
              <xsl:call-template name="Callback"/>      
            </xsl:when>
            <xsl:when test="CTScEventAndFollowUpLog/event_type='Todo'">
              <xsl:call-template name="Todo"/>
            </xsl:when>
            <xsl:when test="CTScEventAndFollowUpLog/event_type='Call Log'">
              <xsl:call-template name="CallLog"/>
            </xsl:when>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="CallLog">
      <xsl:call-template name="CallLogTitle"/>
      <p>
        <xsl:call-template name="ContactTotalTime"/>
      </p>
      <p style="padding-bottom:8px">
        <xsl:call-template name="ContactNumber"/>
      </p>
      <p style="padding-bottom:8px;">
        <xsl:call-template name="Contact"/>
        <xsl:call-template name="ContactComment"/>
      </p>
      <xsl:call-template name="CreatedDate"/>
      <p style="margin-left:20px">
        <xsl:call-template name="CreatedBy"/>
      </p>
      <div style="color:#E4E5F0;margin-top:-10px;height:1px">-----------------------------------------------</div>
  
  </xsl:template>
  
  <xsl:template name="Todo">
    <xsl:apply-templates select="CTScEventAndFollowUpLog/event_type"/>
    <xsl:apply-templates select="CTScEventAndFollowUpLog/done"/>
    <xsl:call-template name="AssignUser"/>
    <p style="padding-bottom:8px">
      <xsl:call-template name="DateTimeTransaction"/>
    </p>
    <p style="padding-bottom:8px;">
      <xsl:call-template name="Contact"/>
      <xsl:call-template name="ContactComment"/>
    </p>
    <xsl:call-template name="CreatedDate"/>
    <p style="margin-left:20px">
      <xsl:call-template name="CreatedBy"/>
    </p>
    <div style="color:#E4E5F0;margin-top:-10px;height:1px">-----------------------------------------------</div>
  </xsl:template>
  
  <xsl:template name="Callback">
    <xsl:apply-templates select="CTScEventAndFollowUpLog/event_type"/>
    <xsl:apply-templates select="CTScEventAndFollowUpLog/done"/>
    <xsl:call-template name="AssignUser"/>
    <p style="padding-bottom:8px">
      <xsl:call-template name="DateTimeTransaction"/>
    </p>
    <p style="padding-bottom:8px;">
      <xsl:call-template name="Contact"/>
      <xsl:call-template name="ContactComment"/>
    </p>
    <xsl:call-template name="CreatedDate"/>
    <p style="margin-left:20px">
      <xsl:call-template name="CreatedBy"/>
    </p>
    <div style="color:#E4E5F0;margin-top:-10px;height:1px">-----------------------------------------------</div>
  </xsl:template>
  
  <xsl:template name="NurtureEvent">
    <xsl:apply-templates select="CTScEventAndFollowUpLog/event_type"/>
    <xsl:apply-templates select="CTScEventAndFollowUpLog/done"/>
    <xsl:call-template name="AssignUser"/>
    <p style="padding-bottom:8px">
      <xsl:call-template name="DateTimeTransaction"/>
    </p>
    <p style="padding-bottom:8px;">
      <xsl:call-template name="Contact"/>
      <xsl:call-template name="ContactComment"/>
    </p>
    <xsl:call-template name="CreatedDate"/>
    <p style="margin-left:20px">
      <xsl:call-template name="CreatedBy"/>
    </p>
    <p style="margin-left:20px">
      <xsl:call-template name="FollowUpSource"/>
    </p>
  </xsl:template>

  <xsl:template name="NurtureLog">
    <xsl:apply-templates select="CTScEventAndFollowUpLog/event_type"/>
    <p style="padding:5px 0px">
      <img src="property:ToolTipResources.make_call" style="width:16px;height:16px"/>
      <xsl:text> </xsl:text><xsl:value-of select="CTScEventAndFollowUpLog/followup_source"/></p>

    <p style="padding-top:8px;">
      <xsl:call-template name="Contact"/>
    </p>
    <p style="padding-bottom:8px;">
      <xsl:call-template name="ContactComment"/>
    </p>
    <xsl:call-template name="DateTimeTransaction"/>
    <xsl:call-template name="CreatedDate"/>
    <p style="margin-left:20px">
      <xsl:call-template name="CreatedBy"/>
    </p>
    <div style="color:#E4E5F0;margin-top:-10px;height:1px">
      <xsl:call-template name="FollowUpSource"/>
    </div>
  </xsl:template>
  
  <xsl:template match="event_type">
    <p>
      <xsl:choose>
        <xsl:when test=".='Nurture Log'">
          <img src="property:ToolTipResources.nurture_log"  style="width:16px;height:16px"/><xsl:text> </xsl:text>Nurture Log
        </xsl:when>
        <xsl:when test=".='Nurture Event'">
            <img src="property:ToolTipResources.nurture" style="width:16px;height:16px"/><xsl:text> </xsl:text>Nurture Task 
        </xsl:when>
        <xsl:when test=".='Todo'">
          <img src="property:ToolTipResources.todo" style="width:16px;height:16px"/><xsl:text> </xsl:text>Todo Task
        </xsl:when>
        <xsl:when test=".='Make Call'">
          <img src="property:ToolTipResources.make_call" style="width:16px;height:16px"/><xsl:text> </xsl:text>Call Back Task
        </xsl:when>
      </xsl:choose>
    </p>
  </xsl:template>
  
  <xsl:template match="done">
    <p>
      <xsl:choose>
        <xsl:when test=".='true'">
          <img src="property:ToolTipResources.checkbox_check"/><xsl:text> </xsl:text>Done
        </xsl:when>
        <xsl:otherwise>
          <img src="property:ToolTipResources.checkbox_uncheck"/><xsl:text> </xsl:text>Open
        </xsl:otherwise>
      </xsl:choose>
    </p>
  </xsl:template>

  <xsl:template name="AssignUser">
    <p>
      <xsl:choose>
        <xsl:when test="CTScEventAndFollowUpLog/fullname = 'Team' or CTScEventAndFollowUpLog/assigned_user ='0'">
          <img src="property:ToolTipResources.assigned_to_team"/><xsl:text> </xsl:text>Team
        </xsl:when>
        <xsl:when test="CTScEventAndFollowUpLog/assigned_user != CTScEventAndFollowUpLog/current_user_id">
          <img src="property:ToolTipResources.assigned_to_other"/><xsl:text> </xsl:text>
          <xsl:value-of select="CTScEventAndFollowUpLog/fullname"/>
        </xsl:when>
        <xsl:when test="CTScEventAndFollowUpLog/assigned_user != CTScEventAndFollowUpLog/current_user_id">
          <img src="property:ToolTipResources.assigned_to_me"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="CTScEventAndFollowUpLog/fullname"/>
        </xsl:when>
      </xsl:choose>
    </p>
  </xsl:template>

  <xsl:template name="DateTimeTransaction">
    <p>
      <img src="property:ToolTipResources.date_go"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="util:dateFormat(CTScEventAndFollowUpLog/date_time_of_transaction,'yyyy-MM-dd HH:mm')"/>
    </p>
  </xsl:template>

  <xsl:template name="ContactComment">
    <xsl:if test="util:len(CTScEventAndFollowUpLog/short_message) > 0">
      <p><xsl:value-of select="CTScEventAndFollowUpLog/short_message"/></p>
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="Contact">
    <p>
      <xsl:value-of select="CTScEventAndFollowUpLog/contact_name"/>
      <xsl:if test="util:len(CTScEventAndFollowUpLog/title) > 0">
        , <xsl:value-of select="CTScEventAndFollowUpLog/title"/>
      </xsl:if>
    </p>
  </xsl:template>
  
  <xsl:template name="CreatedBy">
    <p>
    <xsl:value-of select="CTScEventAndFollowUpLog/created_by"/>
    </p>
  </xsl:template>
  
  <xsl:template name="CreatedDate">
    <p>
      <img src="property:ToolTipResources.date_previous"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="util:dateFormat(CTScEventAndFollowUpLog/date_created,'yyyy-MM-dd HH:mm')"/>
    </p>
  </xsl:template>
  
  <xsl:template name="FollowUpSource">
      <xsl:value-of select="CTScEventAndFollowUpLog/followup_source"/>
  </xsl:template>

  <xsl:template name="CallLogTitle">
    <xsl:variable name="eventStatus" select="CTScEventAndFollowUpLog/event_status"/>
    <xsl:choose>
      <xsl:when test="$eventStatus='Successfull'">
        <img src="property:ToolTipResources.completed"  style="width:16px;height:16px"/><xsl:text> </xsl:text>Completed
      </xsl:when>
      <xsl:when test="$eventStatus='No Answer'">
        <img src="property:ToolTipResources.no_answer"  style="width:16px;height:16px"/><xsl:text> </xsl:text>No Answer
      </xsl:when>
      <xsl:when test="$eventStatus='Busy Signal'">
        <img src="property:ToolTipResources.busy_signal"  style="width:16px;height:16px"/><xsl:text> </xsl:text>Busy Signal
      </xsl:when>
      <xsl:when test="$eventStatus='Call Diverted To' or $eventStatus='Call Referal To'">
        <img src="property:ToolTipResources.call_refered_to"  style="width:16px;height:16px"/><xsl:text> </xsl:text>Call Forwarding
      </xsl:when>
      <xsl:when test="$eventStatus=&quot;Don&apos;t Have Time&quot; or $eventStatus=&quot;Contact not found&quot;">
        <img src="property:ToolTipResources.flag_purple"  style="width:16px;height:16px"/><xsl:text> </xsl:text>Others
      </xsl:when>
     
      <xsl:when test="$eventStatus='No Answer'"></xsl:when>
      <xsl:when test="$eventStatus='No Answer'"></xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="ContactTotalTime">
    <img src="property:ToolTipResources.play"  style="width:16px;height:16px"/><xsl:text> </xsl:text>
    <xsl:value-of select="util:TimeDifference(CTScEventAndFollowUpLog/start_time, CTScEventAndFollowUpLog/end_time,'{0:mm\:ss}')"/> 
  </xsl:template>
  
  <xsl:template name="ContactNumber">
    <img src="property:ToolTipResources.call_mobile"  style="width:16px;height:16px"/>
    <xsl:text> </xsl:text>
    <xsl:value-of select="CTScEventAndFollowUpLog/contact_no"/>
  </xsl:template>
</xsl:stylesheet>
