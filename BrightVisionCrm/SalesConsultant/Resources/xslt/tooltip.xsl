<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
    exclude-result-prefixes="msxsl"  xmlns:util="util:xsltextension"
    extension-element-prefixes="xsl util"
>
  <xsl:output method="html" />

    <xsl:template match="/">
      <!--<p>
        <xsl:apply-templates select="CTMyFollowUps/event_type"/>
      </p>
      <p>
        <xsl:apply-templates select="CTMyFollowUps/done"/>
      </p>
      <p>
        <xsl:call-template name="AssignUser"/>
      </p>
      <p style="padding-bottom:8px">
        <xsl:call-template name="DateTimeTransaction"/>
      </p>
      <p style="padding-bottom:8px;max-width:60px;">
        <xsl:call-template name="ContactComment"/>
      </p>
      <xsl:call-template name="CreatedBy"/>-->

      <xsl:choose>
        <xsl:when test="CTMyFollowUps/event_type='Nurture Event'">
          <xsl:call-template name="NurtureEvent"/>
        </xsl:when>
        <xsl:when test="CTMyFollowUps/event_type='Make Call'">
          <xsl:call-template name="Callback"/>
        </xsl:when>
        <xsl:when test="CTMyFollowUps/event_type='Todo'">
          <xsl:call-template name="Todo"/>
        </xsl:when>
      </xsl:choose>
      
    </xsl:template>

  <xsl:template name="Todo">
    <xsl:apply-templates select="CTMyFollowUps/event_type"/>
    <xsl:apply-templates select="CTMyFollowUps/done"/>
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
    <xsl:apply-templates select="CTMyFollowUps/event_type"/>
    <xsl:apply-templates select="CTMyFollowUps/done"/>
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

  <xsl:template name="Callback">
    <xsl:apply-templates select="CTMyFollowUps/event_type"/>
    <xsl:apply-templates select="CTMyFollowUps/done"/>
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
        <xsl:when test="CTMyFollowUps/assigned_user = 'Team' or CTMyFollowUps/assigned_user ='0' or CTMyFollowUps/assigned_user =''">
          <img src="property:ToolTipResources.assigned_to_team"/><xsl:text> </xsl:text>Team
        </xsl:when>
        <xsl:when test="CTMyFollowUps/assigned_user_id != CTMyFollowUps/current_user_id">
          <img src="property:ToolTipResources.assigned_to_other"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="CTMyFollowUps/assigned_user"/>
        </xsl:when>
        <xsl:when test="CTMyFollowUps/assigned_user_id = CTMyFollowUps/current_user_id">
          <img src="property:ToolTipResources.assigned_to_me"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="CTMyFollowUps/assigned_user"/>
        </xsl:when>
      </xsl:choose>
    </p>
  </xsl:template>
  
  <xsl:template name="DateTimeTransaction">
    <p>
      <img src="property:ToolTipResources.date_go"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="util:dateFormat(CTMyFollowUps/date_time_of_transaction,'yyyy-MM-dd HH:mm')"/>
    </p>
  </xsl:template>
  
  <xsl:template name="ContactComment">
    <xsl:if test="util:len(CTMyFollowUps/short_message) > 0">
      <p>
        <xsl:value-of select="CTMyFollowUps/short_message"/>
      </p>
    </xsl:if>
  </xsl:template>

  <xsl:template name="Contact">
    <p>
      <xsl:value-of select="CTMyFollowUps/contact_name"/>
      <xsl:if test="util:len(CTMyFollowUps/title) > 0">
        , <xsl:value-of select="CTMyFollowUps/title"/>
      </xsl:if>
    </p>
  </xsl:template>
  
  <xsl:template name="CreatedBy">
    <p>
      <xsl:value-of select="CTMyFollowUps/created_by"/>
    </p>
  </xsl:template>

  <xsl:template name="CreatedDate">
    <p>
      <img src="property:ToolTipResources.date_previous"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="util:dateFormat(CTMyFollowUps/date_created,'yyyy-MM-dd HH:mm')"/>
    </p>
  </xsl:template>

  <xsl:template name="FollowUpSource">
    <xsl:value-of select="CTMyFollowUps/followup_source"/>
  </xsl:template>
  
</xsl:stylesheet>
