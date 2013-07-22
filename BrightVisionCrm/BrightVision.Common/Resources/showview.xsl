<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:regexp="http://exslt.org/regular-expressions"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:msdata="urn:schemas-microsoft-com:xml-msdata"
                exclude-result-prefixes="msxsl"
                xmlns:util="util:xsltextension"
                extension-element-prefixes="xsl regexp util">

  <xsl:output method="xml" indent="yes"/>
  <xsl:template match="/">
    <view>
      <xsl:call-template name="schema"></xsl:call-template>
      <xsl:apply-templates select="view/contacts/item"></xsl:apply-templates>
    </view>
  </xsl:template>

  <xsl:template name="schema">
    <xs:schema id="view" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
      <xs:element name="view" msdata:IsDataSet="true" msdata:UseCurrentLocale="true">
        <xs:complexType>
          <xs:choice minOccurs="0" maxOccurs="unbounded">
            <xs:element name="item">
              <xs:complexType>
                <xs:sequence>
                  <xs:element name="accountid" type="xs:string" minOccurs="0" msdata:Ordinal="0" />
                  <xs:element name="contactid" type="xs:string" minOccurs="0" msdata:Ordinal="1" />
                  <xsl:apply-templates select="view/item" mode="schema">
                    <xsl:sort select="@position" data-type="number"/>
                  </xsl:apply-templates>
                </xs:sequence>
              </xs:complexType>
            </xs:element>
          </xs:choice>
        </xs:complexType>
      </xs:element>
    </xs:schema>
  </xsl:template>

  <xsl:template match="view/item" mode="schema">
    <xs:element name="{util:encodeName(display_name)}" type="xs:string" minOccurs="0" msdata:Ordinal="{position() + 1}" />
  </xsl:template>

  <xsl:template match="view/contacts/item">
    <item>
      <accountid>
        <xsl:value-of select="@account_id"/>
      </accountid>
      <contactid>
        <xsl:value-of select="@contact_id"/>
      </contactid>
      <xsl:apply-templates select="/view/item" mode="data">
        <xsl:with-param name="account_id" select="@account_id"/>
        <xsl:with-param name="contact_id" select="@contact_id"/>
        <xsl:sort select="@position" data-type="number"/>
      </xsl:apply-templates>
    </item>
  </xsl:template>

  <xsl:template match="view/item" mode="data">
    <xsl:param name="account_id"/>
    <xsl:param name="contact_id"/>
    <xsl:variable name="questionlayoutid" select="questionlayout_id"/>
    <xsl:choose>
      <!--<xsl:when test="field_name = 'BVOwnership' or 
                      field_name = 'CustomerOwnership' or 
                      field_name = 'PlotDoneStatus' or 
                      field_name = 'Priority' or 
                      field_name = 'QuestionTextLabel' or 
                      field_name = 'QuestionText' or 
                      field_name = 'QuestionHelp'">
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <xsl:with-param name="value" select="external_value"/>
        </xsl:call-template>
      </xsl:when>-->
      <xsl:when test="source='General'">
        <xsl:choose>
          <xsl:when test="field_name='ExportViewName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/relation/view/@name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='CustomerName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/relation/customer/@name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='CampaignName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/relation/campaign/@name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='SubcampaignName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/relation/subcampaign/@name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='DialogCreatedBy'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@created_by"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='DialogCreatedDate'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@created_date"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='DialogStatus'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@dialog_status"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ListSourceName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/relation/dialog/@list_source_name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='CompanyLeadStatus'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/appointment/@lead_status"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='CompanyStatus'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/miscellaneous/@status"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='CompanyLastChanged'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/miscellaneous/@last_contact"/>
            </xsl:call-template>
          </xsl:when>          
          <xsl:when test="field_name='CompanyStatusLastChanged'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/appointment/@last_updated"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ContactLastChanged'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@last_contact"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ContactStatusLastChanged'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@last_updated"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='AccountSubCampaignCallAttempts'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/account_stats/@sub_campaign_call_attempts"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ContactSubCampaignCallAttempts'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/contact_stats/@sub_campaign_call_attempts"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="field_name='DropboxValue' and component_type='Dropbox'">
        <xsl:variable name="fieldindex" select="field_index+1"/>
        <xsl:variable name="dropboxvalue"
                      select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id][$fieldindex]/subanswer/@text"/>
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <xsl:with-param name="value" select="util:getDropboxValue($dropboxvalue)"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="component_type='MultipleChoice'">
        <xsl:variable name="fieldindex" select="field_index+1"/>
        <xsl:choose>
          <xsl:when test="field_name='QuestionText'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="external_value"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='MultipleChoiceValue'">
            <xsl:variable name="subanswer" select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id][$fieldindex]/subanswer"/>
            <xsl:variable name="checked">
              <xsl:choose>
                <xsl:when test="$subanswer/@index = 1">
                  <xsl:value-of select="'Checked'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'Unchecked'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$checked"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='MultipleChoiceOtherChoiceValue'">
            <xsl:variable name="subanswer" select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id and subanswer/@index=-1][$fieldindex]/subanswer"/>
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$subanswer/@text"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="component_type='Meeting Schedule' or component_type='Webinar Schedule' or component_type='Seminar Schedule'">
        <xsl:variable name="dialog_item"
                  select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id]"/>
        <xsl:variable name="schedule" select="/view/schedules/item"/>
        <xsl:variable name="fieldindex" select="field_index+1"/>
        <xsl:choose>
          <xsl:when test="field_name='QuestionText'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="external_value"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ScheduleType'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@type"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ResourceName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/resource/@resource_name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ResourceID'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/resource/@id"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ScheduleID'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@id"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Subject'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@subject"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Location'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@location"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='StartTime'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@start_time"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EndTime'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@end_time"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Description'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@description"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='AllDayEvent'">
            <xsl:variable name="allday">
              <xsl:choose>
                <xsl:when test="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@alldayevent='1'">
                  <xsl:value-of select="'Yes'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'No'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$allday"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Attendies'">
            <xsl:variable name="attendies">
              <xsl:for-each select="$dialog_item[subanswer/@index='5']">
                <xsl:value-of select="concat(util:getAttendiesValue(subanswer/@text), substring(', ', 2 - (position() != last())))"/>
              </xsl:for-each>
            </xsl:variable>
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$attendies"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ScheduleOtherChoiceValue'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="$dialog_item[subanswer/@index='7'][$fieldindex]/subanswer/@text"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="field_name='InputValue' and component_type='Textbox'">
        <!--[@jeff 05-30-2012]: https://brightvision.jira.com/browse/PLATFORM-1446:
            - added contact_id on textbox to view contact level values by contact.
        -->
        <!--<xsl:variable name="subanswer" select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id]/subanswer"/>-->
        <xsl:variable name="subanswer" select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id]/subanswer"/>
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <xsl:with-param name="value" select="$subanswer/@text"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="field_name='SmartTextValues' and component_type='SmartText'">
       <xsl:variable name="contactTitle" select="/view/contacts/item[@contact_id=$contact_id]/@title"/>
        <xsl:choose>
            <xsl:when test="source = 'Dialog Account Level'">
              
              <xsl:variable name="subanswer" select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and account/@account_level = 1 ]/subanswer/@text"/>


              <xsl:if test="$subanswer !=''">
                <xsl:variable name="xmlsubanswer" select="util:SmartTextValueToXmlNode($subanswer)"/>




                <xsl:variable name="val">
                  <xsl:variable name="contacts" select="/view/contacts"/>
                  <xsl:for-each select="$xmlsubanswer//SmartTextValue" >
                    <xsl:variable name="customerContactId" select="CustomerContactId"/>
                    <xsl:variable name="fname" select="$contacts/item[@contact_id=$customerContactId]/@first_name"/>
                    <xsl:variable name="lname" select="$contacts/item[@contact_id=$customerContactId]/@last_name"/>
                    <xsl:variable name="title" select="$contacts/item[@contact_id=$customerContactId]/@title"/>
                    <xsl:variable name="titleNotConfirmed" select="//view/contacts/item[@contact_id=$customerContactId]/@title_not_confirmed"/>
                    <!--Date part-->
                    <xsl:value-of select="util:dateFormat(CreationDate,'yyyy-MM-dd HH:mm')"/>
                    <xsl:if test="$fname!='' or $lname!=''">, <xsl:value-of select="$fname"/><xsl:text> </xsl:text><xsl:value-of select="$lname"/></xsl:if>
                    <!--Title part-->
                    <xsl:choose>
                      <xsl:when test="$title != ''">, <xsl:value-of select="$title"/></xsl:when>
                      <xsl:when test="$titleNotConfirmed != ''">, <xsl:value-of select="$title"/></xsl:when>
                    </xsl:choose>:<xsl:text>&#xa;</xsl:text>
                    <!--Comment part-->
                    <xsl:value-of select="Comment"/>
                    <xsl:text>&#xa;</xsl:text>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:for-each>
                </xsl:variable>

                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="display_name"/>
                  <!--<xsl:with-param name="value" select="util:getSmartTextValues($subanswer, $contactTitle)"/>-->
                  <xsl:with-param name="value" select="$val"/>

                </xsl:call-template>
              </xsl:if>
            </xsl:when>
            <xsl:when test="source = 'Dialog Contact Level'">
              <!--Format: -->
              <!--Datetime(yyyy-MM-dd HH:mm), Name, Title \n-->
              <!--Comment-->
              <xsl:variable name="subanswer" select="/view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id and account/@account_level = 0 ]/subanswer/@text"/>
              <xsl:if test="$subanswer !=''">
                <xsl:variable name="xmlsubanswer" select="util:SmartTextValueToXmlNode($subanswer)"/>
                <xsl:variable name="fname" select="/view/contacts/item[@contact_id=$contact_id]/@first_name"/>
                <xsl:variable name="lname" select="/view/contacts/item[@contact_id=$contact_id]/@last_name"/>
                <xsl:variable name="title" select="/view/contacts/item[@contact_id=$contact_id]/@title"/>
                <xsl:variable name="titleNotConfirmed" select="/view/contacts/item[@contact_id=$contact_id]/@title_not_confirmed"/>

                <xsl:variable name="val">
                  <xsl:for-each select="$xmlsubanswer//SmartTextValue" >
                    <!--Date part-->
                    <xsl:value-of select="util:dateFormat(CreationDate,'yyyy-MM-dd HH:mm')"/>
                    <xsl:if test="$fname!='' or $lname!=''">, <xsl:value-of select="$fname"/><xsl:text> </xsl:text><xsl:value-of select="$lname"/></xsl:if>
                    <!--Title part-->
                    <xsl:choose>
                      <xsl:when test="$title != ''">, <xsl:value-of select="$title"/></xsl:when>
                      <xsl:when test="$titleNotConfirmed != ''">, <xsl:value-of select="$title"/></xsl:when>
                    </xsl:choose>:<xsl:text>&#xa;</xsl:text>
                    <!--Comment part-->
                    <xsl:value-of select="Comment"/>
                    <xsl:text>&#xa;</xsl:text>
                    <xsl:text>&#xa;</xsl:text>

                  </xsl:for-each>
                </xsl:variable>

                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="display_name"/>
                  <!--<xsl:with-param name="value" select="util:getSmartTextValues($subanswer, $contactTitle)"/>-->
                  <xsl:with-param name="value" select="$val"/>
                </xsl:call-template>

              </xsl:if>
            </xsl:when>
        </xsl:choose>
        
      </xsl:when>
      <xsl:when test="source='Account'">
        <xsl:choose>
          <xsl:when test="field_name='CompanyName'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@company_name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='OrgNo'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@org_no"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='YearEstablished'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@year_established"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ParentCompany'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@parent_company"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Website'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@website"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Telephone'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@telephone"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Telefax'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/info/@telefax"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Box'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@box"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Street'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@street"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ZipCode'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@zip_code"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='City'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@city"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Country'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@country"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='County'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@county"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Municipality'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@municipality"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Region'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/address/@region"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ActivityCode'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/activity/@code1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ActivityCode2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/activity/@code2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Currency'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@currency"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='FiscalYear1' or field_name='FiscalYear'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Turnover1' or field_name='Turnover'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@turnover1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Export1' or field_name='Export'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@export1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Result1' or field_name='Result'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@result1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='SalesAbroad1' or field_name='SalesAbroad'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EmployeesAboad1' or field_name='EmployeesAboad'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EmployeesTotal1' or field_name='EmployeesTotal'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@employees_total1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='FiscalYear2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Turnover2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@turnover2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Export2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@export2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Result2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@result2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='SalesAbroad2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EmployeesAboad2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EmployeesTotal2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@employees_total2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='FiscalYear3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Turnover3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@turnover3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Export3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@export3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Result3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@result3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='SalesAbroad3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EmployeesAboad3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='EmployeesTotal3'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/financial/@employees_total3"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Category'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/miscellaneous/@category"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='BVSource'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/miscellaneous/@bvsource"/>
            </xsl:call-template>
          </xsl:when>         
          <xsl:when test="field_name='Priority'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/miscellaneous/@priority"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='AssignedTo'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/accounts/item[info/@account_id=$account_id]/miscellaneous/@assigned_to"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="source='Contact'">
        <xsl:choose>
          <xsl:when test="field_name='Firstname'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@first_name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Middlename'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@middle_name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Lastname'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@last_name"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='DirectPhone'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@direct_phone"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Mobile'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@mobile"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Email'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@email"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='TitleNotConfirmed'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@title_not_confirmed"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Title'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@title"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Roles'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@role_tags"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Address 1'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@address1"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Address 2'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@address2"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='City'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@city"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='ZipCode'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@zip_code"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Country'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@country"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="field_name='Priority'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="display_name"/>
              <xsl:with-param name="value" select="/view/contacts/item[@contact_id=$contact_id]/@priority"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="merge_data and merge_data != ''">
        <xsl:variable name="nodeSetdata" select="util:toNodeSet(merge_data)"/>
        <xsl:variable name="result">
          <xsl:apply-templates select="$nodeSetdata/merge_view/item" mode="merge_data">
            <xsl:with-param name="account_id" select="$account_id"/>
            <xsl:with-param name="contact_id" select="$contact_id"/>
            <xsl:with-param name="parent_view" select="/view"/>
          </xsl:apply-templates>
        </xsl:variable>
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <!--<xsl:with-param name="value" select="util:substring($result, (string-length($result) - 1))"/>-->
   <!--       <xsl:with-param name="value" select="normalize-space(util:substring($result, (string-length($result) - 1)))"/>
    -->      <xsl:with-param name="value" select="util:substring($result, (string-length($result)))" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <xsl:with-param name="value" select="external_value"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="item" mode="merge_data">
    <xsl:param name="account_id"/>
    <xsl:param name="contact_id"/>
    <xsl:param name="parent_view"/>
    <xsl:variable name="questionlayoutid" select="questionlayout_id"/>
    <xsl:variable name="delimiter" select="../@separator"/>
    <xsl:choose>
      <!--<xsl:when test="field_name = 'BVOwnership' or 
                      field_name = 'CustomerOwnership' or 
                      field_name = 'PlotDoneStatus' or 
                      field_name = 'Priority' or 
                      field_name = 'QuestionTextLabel' or 
                      field_name = 'QuestionText' or 
                      field_name = 'QuestionHelp'">
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <xsl:with-param name="value" select="external_value"/>
        </xsl:call-template>
      </xsl:when>-->
      <xsl:when test="source='General'">
        <xsl:choose>
          <xsl:when test="field_name='ExportViewName' and $parent_view/relation/view/@name != ''">
            <xsl:value-of select="concat($parent_view/relation/view/@name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='CustomerName' and $parent_view/relation/customer/@name != ''">
            <xsl:value-of select="concat($parent_view/relation/customer/@name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='CampaignName' and $parent_view/relation/campaign/@name != ''">
            <xsl:value-of select="concat($parent_view/relation/campaign/@name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='SubcampaignName' and $parent_view/relation/subcampaign/@name != ''">
            <xsl:value-of select="concat($parent_view/relation/subcampaign/@name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='DialogCreatedBy' and $parent_view/contacts/item[@contact_id=$contact_id]/@created_by != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@created_by, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='DialogCreatedDate' and $parent_view/contacts/item[@contact_id=$contact_id]/@created_date != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@created_date, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='DialogStatus' and $parent_view/contacts/item[@contact_id=$contact_id]/@dialog_status != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@dialog_status, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ListSourceName' and $parent_view/relation/dialog/@list_source_name != ''">
            <xsl:value-of select="concat($parent_view/relation/dialog/@list_source_name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='CompanyLeadStatus' and $parent_view/accounts/item[info/@account_id=$account_id]/appointment/@lead_status != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/appointment/@lead_status, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='CompanyStatus' and $parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@status != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@status, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='CompanyLastChanged' and $parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@last_contact != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@last_contact, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='CompanyStatusLastChanged' and $parent_view/accounts/item[info/@account_id=$account_id]/appointment/@last_updated != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/appointment/@last_updated, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ContactLastChanged' and $parent_view/contacts/item[@contact_id=$contact_id]/@last_contact != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@last_contact, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ContactStatusLastChanged' and $parent_view/view/contacts/item[@contact_id=$contact_id]/@last_updated != ''">
            <xsl:value-of select="concat($parent_view/view/contacts/item[@contact_id=$contact_id]/@last_updated, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='AccountSubCampaignCallAttempts' and $parent_view/view/accounts/item[info/@account_id=$account_id]/account_stats/@sub_campaign_call_attempts != ''">
            <xsl:value-of select="concat($parent_view/view/accounts/item[info/@account_id=$account_id]/account_stats/@sub_campaign_call_attempts, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ContactSubCampaignCallAttempts' and $parent_view/view/contacts/item[@contact_id=$contact_id]/contact_stats/@sub_campaign_call_attempts != ''">
            <xsl:value-of select="concat($parent_view/view/contacts/item[@contact_id=$contact_id]/contact_stats/@sub_campaign_call_attempts, $delimiter)"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="field_name='DropboxValue' and component_type='Dropbox'">
        <xsl:variable name="fieldindex" select="field_index+1"/>
        <xsl:variable name="dropboxvalue"
                      select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id][$fieldindex]/subanswer/@text"/>
        <xsl:if test="util:getDropboxValue($dropboxvalue) != ''">
          <xsl:value-of select="concat(util:getDropboxValue($dropboxvalue), $delimiter)"/>
        </xsl:if>
      </xsl:when>
      <xsl:when test="component_type='MultipleChoice'">
        <xsl:variable name="fieldindex" select="field_index+1"/>
        <xsl:choose>
          <xsl:when test="field_name='QuestionText' and external_value != ''">
            <xsl:value-of select="concat(external_value, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='MultipleChoiceValue'">
            <xsl:variable name="subanswer" select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id][$fieldindex]/subanswer"/>
            <xsl:variable name="checked">
              <xsl:choose>
                <xsl:when test="$subanswer/@index = 1">
                  <xsl:value-of select="$subanswer/@text"/>
                </xsl:when>
                <!--<xsl:otherwise>
                  <xsl:value-of select="'Unchecked'"/>
                </xsl:otherwise>-->
              </xsl:choose>
            </xsl:variable>

            <xsl:if test="$checked != ''">
              <xsl:value-of select="concat($checked, $delimiter)"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="field_name='MultipleChoiceOtherChoiceValue'">
            <xsl:variable name="subanswer" select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id and subanswer/@index=-1][$fieldindex]/subanswer"/>
            <xsl:if test="$subanswer/@text != ''">
              <xsl:value-of select="concat($subanswer/@text, $delimiter)"/>
            </xsl:if>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="component_type='Meeting Schedule' or component_type='Webinar Schedule' or component_type='Seminar Schedule'">
        <xsl:variable name="dialog_item"
                  select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id]"/>
        <xsl:variable name="schedule" select="$parent_view/schedules/item"/>
        <xsl:variable name="fieldindex" select="field_index+1"/>
        <xsl:choose>
          <xsl:when test="field_name='QuestionText' and external_value != ''">
            <xsl:value-of select="concat(external_value, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ScheduleType' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@type != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@type, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ResourceName' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/resource/@resource_name != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/resource/@resource_name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ResourceID' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/resource/@id != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/resource/@id, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ScheduleID' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@id != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@id, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Subject' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@subject">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@subject, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Location' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@location != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@location, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='StartTime' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@start_time != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@start_time, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='EndTime' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@end_time != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@end_time, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Description' and $schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@description != ''">
            <xsl:value-of select="concat($schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@description, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='AllDayEvent'">
            <xsl:variable name="allday">
              <xsl:choose>
                <xsl:when test="$schedule[schedule/@id=$dialog_item/schedule/@id]/schedule/@alldayevent='1'">
                  <xsl:value-of select="'All day'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'Not All day'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:value-of select="concat($allday, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Attendies'">
            <xsl:variable name="attendies">
              <xsl:for-each select="$dialog_item[subanswer/@index='5']">
                <xsl:value-of select="concat(util:getAttendiesValue(subanswer/@text), substring(', ', 2 - (position() != last())))"/>
              </xsl:for-each>
            </xsl:variable>
            <xsl:if test="$attendies != ''">
              <xsl:value-of select="concat('&quot;',$attendies,'&quot;',$delimiter)"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="field_name='ScheduleOtherChoiceValue' and $dialog_item[subanswer/@index='7'][$fieldindex]/subanswer/@text != ''">
            <xsl:value-of select="concat($dialog_item[subanswer/@index='7'][$fieldindex]/subanswer/@text, $delimiter)"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="field_name='InputValue' and component_type='Textbox'">
       <!-- <xsl:variable name="subanswer" select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id]/subanswer"/>
        <xsl:if test="$subanswer/@text != ''">
          <xsl:value-of select="concat($subanswer/@text, $delimiter)"/>
        </xsl:if> -->
		<xsl:variable name="subanswer">
          <xsl:choose>
            <xsl:when test="source = 'Dialog Account Level'">
              <xsl:value-of select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and account/@account_level = 1 ]/subanswer/@text"/>
            </xsl:when>
            <xsl:when test="source = 'Dialog Contact Level'">
              <xsl:value-of  select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and account/@account_level = 0 and contact/@id=$contact_id]/subanswer/@text"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="$subanswer != ''">
          <xsl:value-of select="concat($subanswer, $delimiter)"/>
        </xsl:if>
      </xsl:when>
      <xsl:when test="field_name='SmartTextValues' and component_type='SmartText'">
        <!--<xsl:variable name="subanswer" select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id]/subanswer"/>
        <xsl:variable name="smarttextvalues" select="util:getSmartTextValues($subanswer,$contact_id)" />
        <xsl:if test="smarttextvalues != ''">
          <xsl:value-of select="concat(smarttextvalues, $delimiter)"/>
        </xsl:if>-->
        <xsl:choose>
          <xsl:when test="source = 'Dialog Account Level'">
            <xsl:variable name="subanswer" select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and account/@account_level = 1]/subanswer/@text"/>
            <xsl:variable name="smarttextvalues" select="util:getSmartTextValues($subanswer)" />
            <xsl:if test="$smarttextvalues != ''">
              <xsl:value-of select="concat($smarttextvalues, $delimiter)"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="source = 'Dialog Contact Level'">
            <xsl:variable name="subanswer" select="$parent_view/dialog/item[questionlayout/@id=$questionlayoutid and account/@id=$account_id and contact/@id=$contact_id and account/@account_level = 0]/subanswer/@text"/>
            <xsl:variable name="smarttextvalues" select="util:getSmartTextValuesCustomer($subanswer)" />
            <xsl:if test="$smarttextvalues != ''">
              <xsl:value-of select="concat($smarttextvalues, $delimiter)"/>
            </xsl:if>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="source='Account'">
        <xsl:choose>
          <xsl:when test="field_name='CompanyName' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@company_name != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@company_name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='OrgNo' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@org_no != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@org_no, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='YearEstablished' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@year_established != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@year_established, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ParentCompany' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@parent_company != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@parent_company, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Website' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@website != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@website, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Telephone' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@telephone != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@telephone, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Telefax' and $parent_view/accounts/item[info/@account_id=$account_id]/info/@telefax != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/info/@telefax, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Box' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@box != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@box, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Street' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@street != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@street, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ZipCode' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@zip_code != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@zip_code, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='City' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@city != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@city, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Country' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@country != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@country, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='County' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@county != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@county, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Municipality' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@municipality != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@municipality, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Region' and $parent_view/accounts/item[info/@account_id=$account_id]/address/@region != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/address/@region, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ActivityCode' and $parent_view/accounts/item[info/@account_id=$account_id]/activity/@code1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/activity/@code1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ActivityCode2' and $parent_view/accounts/item[info/@account_id=$account_id]/activity/@code2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/activity/@code2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Currency' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@currency != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@currency, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='FiscalYear1' or field_name='FiscalYear') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='Turnover1' or field_name='Turnover') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@turnover1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@turnover1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='Export1' or field_name='Export') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@export1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@export1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='Result1' or field_name='Result') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@result1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@result1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='SalesAbroad1' or field_name='SalesAbroad') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='EmployeesAboad1' or field_name='EmployeesAboad') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad1 != '' ">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="(field_name='EmployeesTotal1' or field_name='EmployeesTotal') and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_total1 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_total1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='FiscalYear2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Turnover2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@turnover2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@turnover2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Export2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@export2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@export2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Result2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@result2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@result2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='SalesAbroad2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='EmployeesAboad2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='EmployeesTotal2' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_total2 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_total2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='FiscalYear3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@fiscal_year3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Turnover3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@turnover3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@turnover3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Export3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@export3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@export3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Result3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@result3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@result3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='SalesAbroad3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@sales_abroad3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='EmployeesAboad3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_abroad3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='EmployeesTotal3' and $parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_total3 != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/financial/@employees_total3, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Category' and $parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@category != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@category, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='BVSource' and $parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@bvsource != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@bvsource, $delimiter)"/>
          </xsl:when>          
          <xsl:when test="field_name='Priority' and $parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@priority != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@priority, $delimiter)"/>
          </xsl:when>
         <xsl:when test="field_name='AssignedTo' and $parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@assigned_to != ''">
            <xsl:value-of select="concat($parent_view/accounts/item[info/@account_id=$account_id]/miscellaneous/@assigned_to, $delimiter)"/>
          </xsl:when>

        </xsl:choose>
      </xsl:when>
      <xsl:when test="source='Contact'">
        <xsl:choose>
          <xsl:when test="field_name='Firstname' and $parent_view/contacts/item[@contact_id=$contact_id]/@first_name != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@first_name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Middlename' and $parent_view/contacts/item[@contact_id=$contact_id]/@middle_name != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@middle_name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Lastname' and $parent_view/contacts/item[@contact_id=$contact_id]/@last_name != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@last_name, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='DirectPhone' and $parent_view/contacts/item[@contact_id=$contact_id]/@direct_phone != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@direct_phone, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Mobile' and $parent_view/contacts/item[@contact_id=$contact_id]/@mobile != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@mobile, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Email' and $parent_view/contacts/item[@contact_id=$contact_id]/@email != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@email, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='TitleNotConfirmed' and $parent_view/contacts/item[@contact_id=$contact_id]/@title_not_confirmed != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@title_not_confirmed, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Title' and $parent_view/contacts/item[@contact_id=$contact_id]/@title != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@title, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Roles' and $parent_view/contacts/item[@contact_id=$contact_id]/@role_tags != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@role_tags, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Address 1' and $parent_view/contacts/item[@contact_id=$contact_id]/@address1 != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@address1, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Address 2' and $parent_view/contacts/item[@contact_id=$contact_id]/@address2 != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@address2, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='City' and $parent_view/contacts/item[@contact_id=$contact_id]/@city != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@city, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='ZipCode' and $parent_view/contacts/item[@contact_id=$contact_id]/@zip_code != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@zip_code, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Country' and $parent_view/contacts/item[@contact_id=$contact_id]/@country != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@country, $delimiter)"/>
          </xsl:when>
          <xsl:when test="field_name='Priority' and $parent_view/contacts/item[@contact_id=$contact_id]/@priority != ''">
            <xsl:value-of select="concat($parent_view/contacts/item[@contact_id=$contact_id]/@priority, $delimiter)"/>
          </xsl:when>          
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="column-display">
          <xsl:with-param name="name" select="display_name"/>
          <xsl:with-param name="value" select="external_value"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="column-display">
    <xsl:param name="name"/>
    <xsl:param name="value"/>
    <xsl:element name="{util:encodeName($name)}">
      <xsl:value-of select="$value"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

