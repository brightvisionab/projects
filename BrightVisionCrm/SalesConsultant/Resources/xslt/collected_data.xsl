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
  <xsl:variable name="data_columns">
    <item name="AccountID"/>
    <item name="ContactID"/>
    <item name="Question"/>
    <item name="Answer"/>
    <item name="Date"/>
    <item name="Contact Name"/>
    <item name="Contact Title"/>
    <item name="Contact Roles"/>
    <item name="Customer>Campaign>SubCampaign"/>
    <item name="BVUser"/>
    <item name="Q-ID"/>
    <item name="Dialog-ID"/>
    <item name="QuestionTags"/>    
    <!--<item name="CustomerOwned"/>-->
    <item name="Public"/>
    <item name="Language"/>
  </xsl:variable>
  <xsl:template match="/">
    <collected_data>
      <xsl:call-template name="schema"/>
      <xsl:apply-templates select="data/collected/item">
      </xsl:apply-templates>
      <xsl:apply-templates select="data/dialogs/json_dialog/item"/>    
    </collected_data>
  </xsl:template>

  <xsl:template name="schema">
    <xs:schema id="collected_data" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
      <xs:element name="collected_data" msdata:IsDataSet="true" msdata:UseCurrentLocale="true">
        <xs:complexType>
          <xs:choice minOccurs="0" maxOccurs="unbounded">
            <xs:element name="item">
              <xs:complexType>
                <xs:sequence>
                  <xsl:for-each select="msxsl:node-set($data_columns)/item">
                    <xs:element name="{util:encodeName(@name)}" type="xs:string" minOccurs="0" msdata:Ordinal="{position()}" />
                  </xsl:for-each>
                </xs:sequence>
              </xs:complexType>
            </xs:element>
          </xs:choice>
        </xs:complexType>
      </xs:element>
    </xs:schema>
  </xsl:template>

  <xsl:template match="data/dialogs/json_dialog/item">
    <xsl:variable name="dialogID" select="../@dialog_id"/>
    <xsl:variable name="is_account_level" select="Form/Settings/DataBindings/account_level"/>
    <xsl:choose>
      <xsl:when test="$is_account_level='true'">
        <xsl:apply-templates select="/data/accounts/item[@dialog_id = $dialogID]">
          <xsl:with-param name="question_text" select="Form/Settings/QuestionText"/>
          <xsl:with-param name="component_type" select="Type"/>
          <xsl:with-param name="question_id" select="Form/Settings/DataBindings/question_id"/>
          <xsl:with-param name="question_text_language_id" select="Form/Settings/DataBindings/questions_text_language_id"/>
          <xsl:with-param name="questionlayout_id" select="Form/Settings/DataBindings/questionlayout_id"/>
          <!--<xsl:with-param name="bv_owned" select="Form/Settings/BVOwnership"/>
      <xsl:with-param name="customer_owned" select="Form/Settings/CustomerOwnership"/>-->
          <xsl:with-param name="schedule_type" select="Form/Settings/AnswerOptions/ScheduleType/ScheduleTypeSelectedValue"/>
          <xsl:with-param name="dialog_id" select="$dialogID" />
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/data/contacts/item[@dialog_id = $dialogID]" mode="data">
          <xsl:with-param name="question_text" select="Form/Settings/QuestionText"/>
          <xsl:with-param name="component_type" select="Type"/>
          <xsl:with-param name="question_id" select="Form/Settings/DataBindings/question_id"/>
          <xsl:with-param name="question_text_language_id" select="Form/Settings/DataBindings/questions_text_language_id"/>
          <xsl:with-param name="questionlayout_id" select="Form/Settings/DataBindings/questionlayout_id"/>
          <!--<xsl:with-param name="bv_owned" select="Form/Settings/BVOwnership"/>
      <xsl:with-param name="customer_owned" select="Form/Settings/CustomerOwnership"/>-->
          <xsl:with-param name="schedule_type" select="Form/Settings/AnswerOptions/ScheduleType/ScheduleTypeSelectedValue"/>
          <xsl:with-param name="dialog_id" select="$dialogID" />
        </xsl:apply-templates>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  
  <xsl:template match="data/accounts/item">
    <xsl:param name="component_type"/>
    <xsl:param name="question_id"/>
    <xsl:param name="question_text_language_id"/>
    <xsl:param name="questionlayout_id"/>
    <xsl:param name="schedule_type"/>
    <xsl:param name="dialog_id" />
    <xsl:variable name="account_id" select="@account_id"/>
    <xsl:variable name="contact_id" select="@contact_id"/>
    <xsl:variable name="contact_name" select="@contact_name"/>
    <xsl:variable name="title" select="@title"/>
    <xsl:variable name="role_tags" select="@role_tags"/>

    <xsl:variable name="subcampaign_customer_name" select="/data/relations/relation[dialog/@id=$dialog_id]/customer/@name"/>
    <xsl:variable name="subcampaign_campaign_name" select="/data/relations/relation[dialog/@id=$dialog_id]/campaign/@name"/>
    <xsl:variable name="subcampaign_name" select="/data/relations/relation[dialog/@id=$dialog_id]/subcampaign/@name"/>
    <xsl:variable name="subcampaign_details" select="concat($subcampaign_customer_name,'>', $subcampaign_campaign_name, '>' ,$subcampaign_name)"/>

    <xsl:variable name="question_text" select="/data/question/item[@language_id=$question_text_language_id]/@text"/>
    <xsl:variable name="question_language" select="/data/question/item[@language_id=$question_text_language_id]/@language_code"/>
    <xsl:variable name="tags" select="/data/question/item[@language_id=$question_text_language_id]/@tags"/>
    <!--<xsl:variable name="answers" select="/data/dialog/item[question/@id=$question_id and question/@layout_id=$questionlayout_id and question/@dialog_id=$dialog_id and question/@language_id=$question_text_language_id and question/@account_id=$account_id and question/@contact_id=$contact_id]"/>-->
    <xsl:variable name="answers" select="/data/dialog/item[question/@id=$question_id and question/@layout_id=$questionlayout_id and question/@language_id=$question_text_language_id and question/@account_id=$account_id and answer/@dialog_id=$dialog_id]"/>
    <xsl:variable name="schedules" select="/data/schedules/item"/>
    <xsl:variable name="bv_owned" select="$answers/answer/@bv_ownership"/>
    <xsl:variable name="customer_owned" select="$answers/answer/@customer_ownership"/>
    <xsl:variable name="bvuser" select="$answers/answer/@modifed_by"/>
    <xsl:variable name="date_modified" select="$answers/answer/@date_modifed"/>
    
    <xsl:choose>
      <xsl:when test="count(/data/dialog/item[question/@account_id=$account_id and answer/@dialog_id=$dialog_id] ) &gt; 0 and count(/data/dialog/item[question/@id = $question_id and  answer/@dialog_id=$dialog_id]) &gt; 0">
        <item>
          <xsl:for-each select="msxsl:node-set($data_columns)/item">
            <xsl:choose>
              <xsl:when test="@name='AccountID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$account_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='ContactID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$contact_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Question'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$question_text"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Dialog-ID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$dialog_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Answer'">
                <xsl:choose>
                  <xsl:when test="$component_type='Textbox'">
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="$answers[1]/answer/@subanswer_text"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='SmartText'">
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="util:getSmartTextValuesCustomer($answers[1]/answer/@subanswer_text)"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='Dropbox'">
                    <xsl:variable name="dropbox_value">
                      <xsl:for-each select="$answers">
                        <xsl:value-of select="util:trim(concat(util:getDropboxValue(answer/@subanswer_text), substring('; ', 2 - (position() != last()))))"/>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="$dropbox_value"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='MultipleChoice'">
                    <xsl:variable name="multiplechoice_value">
                      <xsl:for-each select="$answers[answer/@subanswer_index=1]">
                        <xsl:value-of select="util:trim(concat(answer/@subanswer_text, substring(', ', 2 - (position() != last()))))"/>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:variable name="otherchoice_value">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=-1][1]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=-1][1]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="comment_value">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=-1][2]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=-1][2]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="util:trim(concat('Choices: ', $multiplechoice_value,'; ', 'Other choice: ',$otherchoice_value,'; ','Comment: ', $comment_value))"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='Schedule'">
                    <xsl:variable name="schedule">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=3]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=3]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="attendies">
                      <xsl:for-each select="$answers[answer/@subanswer_index=5]">
                        <xsl:value-of select="concat(util:getAttendiesValue(answer/@subanswer_text), substring(', ', 2 - (position() != last())))"/>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:variable name="comment_value">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=7]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=7]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="scheduleid">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=1]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=1]/answer/@schedule_id"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="rname">
                      <xsl:choose>
                        <xsl:when test="$scheduleid">
                          <xsl:choose>
                            <xsl:when test="$schedules[schedule/@id=$scheduleid]">
                              <xsl:value-of select="$schedules[schedule/@id=$scheduleid]/schedule/resource/@resource_name"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="''"/>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="resource">
                      <xsl:choose>
                        <xsl:when test="string-length($rname) &gt; 0">
                          <xsl:value-of select="concat('; ','Resource: ', $rname)"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="locname">
                      <xsl:choose>
                        <xsl:when test="$scheduleid">
                          <xsl:choose>
                            <xsl:when test="$schedules[schedule/@id=$scheduleid]">
                              <xsl:value-of select="$schedules[schedule/@id=$scheduleid]/schedule/@location"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="''"/>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="location">
                      <xsl:choose>
                        <xsl:when test="string-length($locname) &gt; 0">
                          <xsl:value-of select="concat('; ','Location: ', $locname)"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="util:trim(concat('Schedule Type: ', $schedule_type, $resource, '; ', 'Schedule: ', util:getScheduleValue($schedule), $location, '; ','Attendies: ', util:trim($attendies),'; ', 'Meeting Comment: ', $comment_value))"/>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:when>
              <xsl:when test="@name='Date'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$date_modified"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Contact Name'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$contact_name"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Contact Title'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$title"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Contact Roles'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$role_tags"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Customer>Campaign>SubCampaign'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$subcampaign_details"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='BVUser'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$bvuser"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Q-ID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$question_text_language_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Language'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$question_language"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='QuestionTags'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$tags"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='CustomerOwned'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$customer_owned"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Public'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$bv_owned"/>
                </xsl:call-template>
              </xsl:when>

            </xsl:choose>
          </xsl:for-each>
        </item>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="data/contacts/item" mode="data">
    <xsl:param name="component_type"/>
    <xsl:param name="question_id"/>
    <xsl:param name="question_text_language_id"/>
    <xsl:param name="questionlayout_id"/>    
    <xsl:param name="schedule_type"/>    
	  <xsl:param name="dialog_id" />
    <xsl:variable name="account_id" select="@account_id"/>
    <xsl:variable name="contact_id" select="@contact_id"/>
    <xsl:variable name="contact_name" select="@contact_name"/>
    <xsl:variable name="title" select="@title"/>
    <xsl:variable name="role_tags" select="@role_tags"/>
    
    <xsl:variable name="subcampaign_customer_name" select="/data/relations/relation[dialog/@id=$dialog_id]/customer/@name"/>
    <xsl:variable name="subcampaign_campaign_name" select="/data/relations/relation[dialog/@id=$dialog_id]/campaign/@name"/>
    <xsl:variable name="subcampaign_name" select="/data/relations/relation[dialog/@id=$dialog_id]/subcampaign/@name"/>
    <xsl:variable name="subcampaign_details" select="concat($subcampaign_customer_name,'>', $subcampaign_campaign_name, '>' ,$subcampaign_name)"/>
    
    <xsl:variable name="question_text" select="/data/question/item[@language_id=$question_text_language_id]/@text"/>
    <xsl:variable name="question_language" select="/data/question/item[@language_id=$question_text_language_id]/@language_code"/>
    <xsl:variable name="tags" select="/data/question/item[@language_id=$question_text_language_id]/@tags"/>    
    <!--<xsl:variable name="answers" select="/data/dialog/item[question/@id=$question_id and question/@layout_id=$questionlayout_id and question/@dialog_id=$dialog_id and question/@language_id=$question_text_language_id and question/@account_id=$account_id and question/@contact_id=$contact_id]"/>-->
    <xsl:variable name="answers" select="/data/dialog/item[question/@id=$question_id and question/@layout_id=$questionlayout_id and question/@language_id=$question_text_language_id and question/@account_id=$account_id and question/@contact_id=$contact_id and answer/@dialog_id=$dialog_id]"/>
    <xsl:variable name="schedules" select="/data/schedules/item"/>
    <xsl:variable name="bv_owned" select="$answers/answer/@bv_ownership"/>
    <xsl:variable name="customer_owned" select="$answers/answer/@customer_ownership"/>
    <xsl:variable name="bvuser" select="$answers/answer/@modifed_by"/>
    <xsl:variable name="date_modified" select="$answers/answer/@date_modifed"/>
    <xsl:choose>
      <xsl:when test="count(/data/dialog/item[question/@account_id=$account_id and question/@contact_id=$contact_id and answer/@dialog_id=$dialog_id] ) &gt; 0 and count(/data/dialog/item[question/@id = $question_id and  question/@contact_id=$contact_id  and answer/@dialog_id=$dialog_id]) &gt; 0">
        <item>
          <xsl:for-each select="msxsl:node-set($data_columns)/item">
            <xsl:choose>
              <xsl:when test="@name='AccountID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$account_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='ContactID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$contact_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Question'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$question_text"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Dialog-ID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$dialog_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Answer'">
                <xsl:choose>
                  <xsl:when test="$component_type='Textbox'">
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="$answers[1]/answer/@subanswer_text"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='SmartText'">
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="util:getSmartTextValuesCustomer($answers[1]/answer/@subanswer_text)"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='Dropbox'">
                    <xsl:variable name="dropbox_value">
                      <xsl:for-each select="$answers">
                        <xsl:value-of select="util:trim(concat(util:getDropboxValue(answer/@subanswer_text), substring('; ', 2 - (position() != last()))))"/>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="$dropbox_value"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='MultipleChoice'">
                    <xsl:variable name="multiplechoice_value">
                      <xsl:for-each select="$answers[answer/@subanswer_index=1]">
                        <xsl:value-of select="util:trim(concat(answer/@subanswer_text, substring(', ', 2 - (position() != last()))))"/>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:variable name="otherchoice_value">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=-1][1]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=-1][1]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="comment_value">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=-1][2]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=-1][2]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="util:trim(concat('Choices: ', $multiplechoice_value,'; ', 'Other choice: ',$otherchoice_value,'; ','Comment: ', $comment_value))"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$component_type='Schedule'">
                    <xsl:variable name="schedule">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=3]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=3]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="attendies">
                      <xsl:for-each select="$answers[answer/@subanswer_index=5]">
                        <xsl:value-of select="concat(util:getAttendiesValue(answer/@subanswer_text), substring(', ', 2 - (position() != last())))"/>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:variable name="comment_value">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=7]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=7]/answer/@subanswer_text"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="scheduleid">
                      <xsl:choose>
                        <xsl:when test="$answers[answer/@subanswer_index=1]">
                          <xsl:value-of select="$answers[answer/@subanswer_index=1]/answer/@schedule_id"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="rname">
                      <xsl:choose>
                        <xsl:when test="$scheduleid">
                          <xsl:choose>
                            <xsl:when test="$schedules[schedule/@id=$scheduleid]">
                              <xsl:value-of select="$schedules[schedule/@id=$scheduleid]/schedule/resource/@resource_name"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="''"/>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="resource">
                      <xsl:choose>
                        <xsl:when test="string-length($rname) &gt; 0">
                          <xsl:value-of select="concat('; ','Resource: ', $rname)"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="locname">
                      <xsl:choose>
                        <xsl:when test="$scheduleid">
                          <xsl:choose>
                            <xsl:when test="$schedules[schedule/@id=$scheduleid]">
                              <xsl:value-of select="$schedules[schedule/@id=$scheduleid]/schedule/@location"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="''"/>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="location">
                      <xsl:choose>
                        <xsl:when test="string-length($locname) &gt; 0">
                          <xsl:value-of select="concat('; ','Location: ', $locname)"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="''"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="column-display">
                      <xsl:with-param name="name" select="@name"/>
                      <xsl:with-param name="value" select="util:trim(concat('Schedule Type: ', $schedule_type, $resource, '; ', 'Schedule: ', util:getScheduleValue($schedule), $location, '; ','Attendies: ', util:trim($attendies),'; ', 'Meeting Comment: ', $comment_value))"/>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:when>
              <xsl:when test="@name='Date'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$date_modified"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Contact Name'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$contact_name"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Contact Title'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$title"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Contact Roles'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$role_tags"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Customer>Campaign>SubCampaign'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$subcampaign_details"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='BVUser'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$bvuser"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Q-ID'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$question_text_language_id"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Language'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$question_language"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='QuestionTags'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$tags"/>
                </xsl:call-template>
              </xsl:when>              
              <xsl:when test="@name='CustomerOwned'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$customer_owned"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="@name='Public'">
                <xsl:call-template name="column-display">
                  <xsl:with-param name="name" select="@name"/>
                  <xsl:with-param name="value" select="$bv_owned"/>
                </xsl:call-template>
              </xsl:when>
              
            </xsl:choose>
          </xsl:for-each>
        </item>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="data/collected/item">
    <xsl:variable name="account_id"></xsl:variable>
    <xsl:variable name="question_text" select="@question_text"/>
    <xsl:variable name="answer" select="@answer_text"/>
    <xsl:variable name="question_id" select="@question_id"/>
    <xsl:variable name="subcampaign_details" select="concat(@customer_name,'>', @campaign_name,'>',@subcampaign_name)"/>
    <xsl:variable name="bvuser" select="@created_by"/>
    <xsl:variable name="question_language" select="@language_code"/>
    <xsl:variable name="tags" select="@tags"/>
    <xsl:variable name="date_modified" select="@date_created"/>
    <xsl:variable name="bv_owned" select="@bv_owned"/>
    <xsl:variable name="customer_owned" select="@customer_owned"/>
    <item>
      <xsl:for-each select="msxsl:node-set($data_columns)/item">
        <xsl:choose>
          <xsl:when test="@name='AccountID'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="@account_id"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='ContactID'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="''"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Question'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$question_text"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Answer'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$answer"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Date'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$date_modified"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Contact Name'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="''"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Contact Title'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="''"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Contact Roles'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="''"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Customer>Campaign>SubCampaign'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$subcampaign_details"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='BVUser'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$bvuser"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Q-ID'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$question_id"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Language'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$question_language"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='QuestionTags'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$tags"/>
            </xsl:call-template>
          </xsl:when>          
          <xsl:when test="@name='CustomerOwned'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$customer_owned"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="@name='Public'">
            <xsl:call-template name="column-display">
              <xsl:with-param name="name" select="@name"/>
              <xsl:with-param name="value" select="$bv_owned"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </item>
  </xsl:template>
  
  <xsl:template name="column-display">
    <xsl:param name="name"/>
    <xsl:param name="value"/>
    <xsl:element name="{util:encodeName($name)}">
      <xsl:value-of select="$value"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

