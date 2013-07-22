using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightVision.Model;
using BrightVision.Common.Business;

namespace BrightVision.DQControl.Business 
{
    public static class BusinessAnswer 
    {
        public static CampaignQuestionnaire SetQuestionnaireData(CampaignQuestionnaire pQuestionnaire, answer pData)
        {
            if (pQuestionnaire == null || pData == null)
                return pQuestionnaire;

            if (string.IsNullOrEmpty(pQuestionnaire.Form.Settings.DataBindings.answer_id))
                return pQuestionnaire;

            int _AnswerId = int.Parse(pQuestionnaire.Form.Settings.DataBindings.answer_id);
            if (pData != null) {

                /**
                 * private vars.
                 */
                #region Contents
                sub_answers                 _SubAnswer;
                IDropbox                    _iDropbox;
                ITextbox                    _iTextbox;
                IMultipleChoice             _iMultiplechoice;
                ISchedule                   _iSchedule = null;
                ISmartText                  _iSmartText = null;
                IEnumerable<sub_answers>    _lstSubAnswers = null;
                List<MultipleChoiceValue>   _lstMultipleChoices = null;
                List<OtherChoice>           _lstOtherChoices = null;
                List<Attendie>              _lstAttendies = null;
                List<SmartTextValue>        _lstSmartTexts = null;
                Settings                    _Settings = pQuestionnaire.Form.Settings;
                IList<AnswerOption>         _lstAnswerOptions = pQuestionnaire.Form.Settings.AnswerOptions;
                DataBindings                _BindingData = _Settings.DataBindings;
                DropboxValue                _DropBoxValue = null;
                #endregion

                /**
                 * set respective dialog answers.
                 */
                #region Contents
                _Settings.BVOwnership = pData.OwnershipBrightvision;
                _Settings.CustomerOwnership = pData.OwnershipAccount;
                for (int x = 0; x < _lstAnswerOptions.Count; ++x) {
                    switch (pQuestionnaire.Type.ToLower()) {
                        #region Text Box
                        case QuestionTypeConstants.Textbox:
                            _SubAnswer = pData.sub_answers.ElementAtOrDefault(x);
                            if (_SubAnswer != null) {
                                _iTextbox = _lstAnswerOptions[x] as ITextbox;
                                _iTextbox.InputValue = _SubAnswer.answer_text != null ? _SubAnswer.answer_text : string.Empty;
                                _iTextbox.DefaultInputValue = _iTextbox.InputValue;
                            }
                            break;
                        #endregion
                        #region Drop Box
                        case QuestionTypeConstants.Dropbox:
                            _SubAnswer = pData.sub_answers.ElementAtOrDefault(x);
                            if (_SubAnswer != null) {
                                _DropBoxValue = DropboxValue.Instanciate(_SubAnswer.answer_text);
                                _iDropbox = _lstAnswerOptions[x] as IDropbox;
                                _iDropbox.SelectionValue = _DropBoxValue == null ? "" : _DropBoxValue.SelectionValue;
                                _iDropbox.DefaultSelectionValue = _iDropbox.SelectionValue;
                                _iDropbox.SelectionValueIfOther = _DropBoxValue == null ? "" : _DropBoxValue.OtherValue;
                                _iDropbox.DefaultSelectionValueIfOther = _iDropbox.SelectionValueIfOther;
                            }
                            break;
                        #endregion
                        #region Multiple Choice
                        case QuestionTypeConstants.MultipleChoice:
                            /**
                             * if sub_answer_index != -1 if answer is in multiple choice values
                             * if sub_answer_index == -1 if answer is in other choice
                             */
                            _iMultiplechoice = _lstAnswerOptions[x] as IMultipleChoice;
                            _lstMultipleChoices = _iMultiplechoice.MultipleChoiceValues;
                            _lstSubAnswers = from _obj in pData.sub_answers
                                             where _obj.sub_answer_index != -1
                                             select _obj;

                            if (_lstSubAnswers != null && _lstSubAnswers.Count() > 0) {
                                for (int q = 0; q < _lstSubAnswers.Count(); ++q) {
                                    _SubAnswer = _lstSubAnswers.ElementAtOrDefault(q);
                                    if (_SubAnswer != null) {
                                        _lstMultipleChoices[q].Checked = Convert.ToBoolean(_SubAnswer.sub_answer_index);
                                        _lstMultipleChoices[q].TextPrefix = _SubAnswer.answer_text;
                                    }
                                }
                            }

                            _lstOtherChoices = _iMultiplechoice.OtherChoices;
                            _lstSubAnswers = from _obj in pData.sub_answers
                                             where _obj.sub_answer_index == -1
                                             select _obj;

                            if (_lstSubAnswers != null && _lstSubAnswers.Count() > 0) {
                                for (int j = 0; j < _lstSubAnswers.Count(); ++j) {
                                    _SubAnswer = _lstSubAnswers.ElementAtOrDefault(j);
                                    if (_SubAnswer != null) {
                                        _lstOtherChoices[j].InputValue = _SubAnswer.answer_text;
                                        _lstOtherChoices[j].DefaultInputValue = _SubAnswer.answer_text;
                                    }
                                }
                            }
                            break;
                        #endregion
                        #region Schedule & Booking
                        case QuestionTypeConstants.Schedule:
                            /*********************************************************************
                                * Change Log:
                                Note: sub_answer_index = 1 if answer is CalendarSelectedValue => Booking Type
                                    sub_answer_index = 2 if answer is Schedule date InputValue => Sales Person / Resource
                                    sub_answer_index = 3 if answer is StartTime InputValue => ScheduleValue
                                    sub_answer_index = 4 if answer is EndTime InputValue => Null
                                    sub_answer_index = 5 if answer is Attendies => ContactAttendies            
                                    sub_answer_index = 6 if answer is MeetingPlaceDetails => Null
                                    sub_answer_index = 7 if answer is OtherChoice => OtherChoice                               
                            *********************************************************************/
                            _iSchedule = _lstAnswerOptions[x] as ISchedule;
                            
                            /**
                             * set calendar selected value.
                             */
                            _SubAnswer = pData.sub_answers.Where(sa => sa.sub_answer_index == 1).FirstOrDefault();
                            if (_SubAnswer != null) {
                                if (_iSchedule.ScheduleType == null) 
                                    _iSchedule.ScheduleType = new ScheduleType();

                                _iSchedule.ScheduleType.ScheduleTypeSelectedValue = _SubAnswer.answer_text;
                            }

                            /**
                             * set schedule date input value.
                             */
                            _SubAnswer = pData.sub_answers.Where(sa => sa.sub_answer_index == 2).FirstOrDefault();
                            if (_SubAnswer != null) {
                                SalesPerson _sp = SalesPerson.Instanciate(_SubAnswer.answer_text);
                                //if (_sp != null && _iSchedule.ScheduleSalesPerson == null) { //DAN: Neglect the other as it could not properly set the correct sales person
                                if (_sp != null)
                                {
                                    _iSchedule.ScheduleSalesPerson = new ScheduleSalesPerson();
                                    _iSchedule.ScheduleSalesPerson.SalesPersonSelectedValue = _sp;
                                }
                            }

                            /**
                             * set start time input value.
                             */
                            _SubAnswer = pData.sub_answers.Where(sa => sa.sub_answer_index == 3).FirstOrDefault();
                            if (_SubAnswer != null)
                                _iSchedule.ScheduleValue = ScheduleValue.Instanciate(_SubAnswer.answer_text);

                            /**
                             * set attendies.
                             */
                            _iSchedule.Attendies.Clear();
                            _lstAttendies = _iSchedule.Attendies;
                            _lstSubAnswers = pData.sub_answers.Where(k => k.sub_answer_index == 5);
                            if (_lstSubAnswers != null && _lstSubAnswers.Count() > 0) {
                                for (int q = 0; q < _lstSubAnswers.Count(); ++q) {
                                    _SubAnswer = _lstSubAnswers.ElementAtOrDefault(q);
                                    if (_SubAnswer != null) {
                                        Attendie _Attendie = Attendie.Instanciate(_SubAnswer.answer_text);
                                        if (_Attendie != null)
                                            _lstAttendies.Add(_Attendie);
                                    }
                                }
                            }

                            /**
                             * set other choices.
                             */
                            _lstOtherChoices = _iSchedule.OtherChoices;
                            _lstSubAnswers = pData.sub_answers.Where(k => k.sub_answer_index == 7);
                            if (_lstSubAnswers != null && _lstSubAnswers.Count() > 0) {
                                for (int j = 0; j < _lstSubAnswers.Count(); ++j) {
                                    _SubAnswer = _lstSubAnswers.ElementAtOrDefault(j);
                                    if (_SubAnswer != null) {
                                        _lstOtherChoices[j].InputValue = _SubAnswer.answer_text;
                                        _lstOtherChoices[j].DefaultInputValue = _SubAnswer.answer_text;
                                    }
                                }
                            }
                            break;
                        #endregion
                        #region Smart Text
                        case QuestionTypeConstants.SmartText:
                            _SubAnswer = pData.sub_answers.ElementAtOrDefault(x);
                            if (_SubAnswer != null) {
                                _iSmartText = _lstAnswerOptions[x] as ISmartText;
                                _lstSmartTexts = SmartTextValue.Instanciate(_SubAnswer.answer_text);
                                _iSmartText.SmartTextValues = _lstSmartTexts;
                            }
                            break;
                        #endregion
                    }
                }
                #endregion

                /**
                 * set binding data values.
                 */
                #region Contents
                if (string.IsNullOrEmpty(_BindingData.language_code))
                    _BindingData.language_code = "SE";

                if (!_BindingData.account_level)
                    _BindingData.contact_id = pData.contact_id != null ? pData.contact_id.ToString() : null;
                else {
                    /**
                     * [@jeff 05.25.2012]: https://brightvision.jira.com/browse/PLATFORM-1438
                     * added error validation for null or empty contact ids.
                     */
                    int _AnswerContactId = pData.contact_id == null ? 0 : Convert.ToInt32(pData.contact_id);
                    int _BindingContactId = 0;
                    try { 
                        _BindingContactId = int.Parse(_BindingData.contact_id); 
                    }
                    catch { 
                        _BindingContactId = 0; 
                    }

                    if ((_AnswerContactId > 0 && _BindingContactId > 0) && _AnswerContactId == _BindingContactId)
                        if (!_BindingData.account_level)
                            _BindingData.same_contact_binding = true;
                        else
                            _BindingData.same_contact_binding = false;
                }

                _BindingData.answer_id = pData.id.ToString();
                _BindingData.account_id = pData.accounts_id.ToString();
                _BindingData.questionlayout_id = pData.questionlayout_id.ToString();
                _BindingData.campaign_id = pData.campaigns_id.ToString();
                _BindingData.dialog_id = pData.dialog_id.ToString();
                _BindingData.created_by = pData.created_by.ToString();
                #endregion
            }

            return pQuestionnaire;
        }

        // old implementation
        public static CampaignQuestionnaire BindAnswer(CampaignQuestionnaire oQuestionnaire, answer objAnswer)
        {
            //BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            var oQues = oQuestionnaire;
            if (oQues != null ) {
                string answer_id = oQuestionnaire.Form.Settings.DataBindings.answer_id;
                if (string.IsNullOrEmpty(answer_id)) return oQuestionnaire;
                int int_answer_id = int.Parse(answer_id);
                
               // var objAnswer = BPContext.answers.Include("sub_answers").FirstOrDefault(p => p.id == int_answer_id);
                if (objAnswer != null) {

                    #region AnswerDetails
                    sub_answers subanswer;
                    IDropbox iDropbox;
                    ITextbox iTextbox;
                    IMultipleChoice iMultiplechoice;
                    ISchedule iSchedule = null;
                    ISmartText iSmartText = null;

                    IEnumerable<sub_answers> subanswersValues = null;
                    List<MultipleChoiceValue> listMCV = null;
                    List<OtherChoice> listOtherChoice = null;
                   
                    List<Attendie> listAttendie = null;
                    List<SmartTextValue> listSmartText = null;
                    var settings = oQues.Form.Settings;
                    var answerOptions = oQues.Form.Settings.AnswerOptions;
                    var binding = settings.DataBindings;

                    //set settings
                    settings.BVOwnership = objAnswer.OwnershipBrightvision;
                    settings.CustomerOwnership = objAnswer.OwnershipAccount;
                    
                    #endregion
                    DropboxValue dropboxVal = null;
                    //set answeroptions
                    for (int x = 0; x < answerOptions.Count; ++x) {

                        switch (oQuestionnaire.Type.ToLower()) {
                            #region SubAnswer for Textbox Component
                            case QuestionTypeConstants.Textbox:                                
                                subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                                if(subanswer != null) {
                                    iTextbox = answerOptions[x] as ITextbox;
                                    iTextbox.InputValue = subanswer.answer_text != null ? subanswer.answer_text : string.Empty;
                                    iTextbox.DefaultInputValue = iTextbox.InputValue;
                                }
                                break;
                            #endregion
                            #region SubAnswer for Dropbox Component
                            case QuestionTypeConstants.Dropbox:                                
                                subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                                if (subanswer != null) {
                                    dropboxVal = DropboxValue.Instanciate(subanswer.answer_text);
                                    iDropbox = answerOptions[x] as IDropbox;
                                    iDropbox.SelectionValue = dropboxVal == null? "": dropboxVal.SelectionValue;
                                    iDropbox.DefaultSelectionValue = iDropbox.SelectionValue;
                                    iDropbox.SelectionValueIfOther = dropboxVal == null ? "" : dropboxVal.OtherValue;
                                    iDropbox.DefaultSelectionValueIfOther = iDropbox.SelectionValueIfOther;
                                }
                                break;
                            #endregion
                            #region SubAnswer for MultipleChoice Component
                            case QuestionTypeConstants.MultipleChoice:
                                iMultiplechoice = answerOptions[x] as IMultipleChoice;
                                //Note: sub_answer_index != -1 if answer is in multiple choice values
                                //      sub_answer_index == -1 if answer is in other choice

                                //set multiplechoicevalues                                
                                listMCV = iMultiplechoice.MultipleChoiceValues;
                                subanswersValues =
                                    from obj in objAnswer.sub_answers
                                    where obj.sub_answer_index != -1
                                    select obj;
                                if (subanswersValues != null && subanswersValues.Count() > 0) {
                                    for (int q = 0; q < subanswersValues.Count(); ++q) {
                                        subanswer = subanswersValues.ElementAtOrDefault(q);
                                        if (subanswer != null) {
                                            listMCV[q].Checked = Convert.ToBoolean(subanswer.sub_answer_index);
                                            listMCV[q].TextPrefix = subanswer.answer_text;
                                        }
                                    }
                                }
                                //set otherchoice                                
                                listOtherChoice = iMultiplechoice.OtherChoices;
                                subanswersValues =
                                    from obj in objAnswer.sub_answers
                                    where obj.sub_answer_index == -1
                                    select obj;
                                if (subanswersValues != null && subanswersValues.Count() > 0) {
                                    for (int j = 0; j < subanswersValues.Count(); ++j) {
                                        subanswer = subanswersValues.ElementAtOrDefault(j);
                                        if (subanswer != null) {
                                            listOtherChoice[j].InputValue = subanswer.answer_text;
                                            listOtherChoice[j].DefaultInputValue = subanswer.answer_text;
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region SubAnswer for Schedule Component
                            case QuestionTypeConstants.Schedule:
                                iSchedule = answerOptions[x] as ISchedule;
                                /*********************************************************************
                                 * Change Log:
                                 Note: sub_answer_index = 1 if answer is CalendarSelectedValue => Booking Type
                                       sub_answer_index = 2 if answer is Schedule date InputValue => Sales Person / Resource
                                       sub_answer_index = 3 if answer is StartTime InputValue => ScheduleValue
                                       sub_answer_index = 4 if answer is EndTime InputValue => Null
                                       sub_answer_index = 5 if answer is Attendies => ContactAttendies            
                                       sub_answer_index = 6 if answer is MeetingPlaceDetails => Null
                                       sub_answer_index = 7 if answer is OtherChoice => OtherChoice                               
                                *********************************************************************/
                                //set CalendarSelectedValue
                                subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 1).FirstOrDefault();
                                if (subanswer != null) {
                                    if (iSchedule.ScheduleType == null) iSchedule.ScheduleType = new ScheduleType();                                    
                                    iSchedule.ScheduleType.ScheduleTypeSelectedValue = subanswer.answer_text;
                                }
                                //set Schedule date InputValue
                                subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 2).FirstOrDefault();
                                if (subanswer != null) {
                                    SalesPerson _sp = SalesPerson.Instanciate(subanswer.answer_text);
                                    if (_sp != null && iSchedule.ScheduleSalesPerson == null) {
                                        iSchedule.ScheduleSalesPerson = new ScheduleSalesPerson();
                                        iSchedule.ScheduleSalesPerson.SalesPersonSelectedValue = _sp;
                                        //iSchedule.ScheduleSalesPerson.SalesPersonSelectedValue = SalesPerson.Instanciate(subanswer.answer_text);
                                    }
                                }
                                //set StartTime InputValue
                                subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 3).FirstOrDefault();
                                if (subanswer != null) {                                    
                                    iSchedule.ScheduleValue = ScheduleValue.Instanciate(subanswer.answer_text);
                                }
                                ////set EndTime InputValue
                                //subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 4).FirstOrDefault();
                                //if (subanswer != null) {
                                //    //iSchedule.EndTime.InputValue = subanswer.answer_text;                                    
                                //}
                                //set Attendies
                                iSchedule.Attendies.Clear(); //clear dummy values.
                                listAttendie = iSchedule.Attendies;
                                subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 5);
                                if (subanswersValues != null && subanswersValues.Count() > 0) {
                                    for (int q = 0; q < subanswersValues.Count(); ++q) {
                                        subanswer = subanswersValues.ElementAtOrDefault(q);
                                        if (subanswer != null) {
                                            var attendie = Attendie.Instanciate(subanswer.answer_text);
                                            if(attendie != null)
                                                listAttendie.Add(attendie);
                                        }
                                    }
                                }
                                //set otherchoice
                                listOtherChoice = iSchedule.OtherChoices;
                                subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 7);
                                if (subanswersValues != null && subanswersValues.Count() > 0) {
                                    for (int j = 0; j < subanswersValues.Count(); ++j) {
                                        subanswer = subanswersValues.ElementAtOrDefault(j);
                                        if (subanswer != null) {
                                            listOtherChoice[j].InputValue = subanswer.answer_text;
                                            listOtherChoice[j].DefaultInputValue = subanswer.answer_text;
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region SubAnswer for SmartText
                            case QuestionTypeConstants.SmartText:
                                subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                                if (subanswer != null){
                                    iSmartText = answerOptions[x] as ISmartText;
                                    listSmartText = SmartTextValue.Instanciate(subanswer.answer_text);
                                    iSmartText.SmartTextValues = listSmartText;
                                }
                                break;
                            #endregion
                        }
                    }
                    //set data bindings

                    if(string.IsNullOrEmpty(binding.language_code))
                        binding.language_code = "SE";

                    if (!binding.account_level) //bypass objanswer contact_id if accountLevel and use currently selected contact
                        binding.contact_id = objAnswer.contact_id != null ? objAnswer.contact_id.ToString() : null;
                    else {
                        /**
                         * [@jeff 05.25.2012]: https://brightvision.jira.com/browse/PLATFORM-1438
                         * added error validation for null or empty contact ids.
                         */
                        int _AnswerContactId = objAnswer.contact_id == null ? 0 : Convert.ToInt32(objAnswer.contact_id);
                        int _BindingContactId = 0;
                        try { _BindingContactId = int.Parse(binding.contact_id); } catch { _BindingContactId = 0; }
                        //if (objAnswer.contact_id != null && Convert.ToInt32(objAnswer.contact_id) == int.Parse(binding.contact_id))
                        if ((_AnswerContactId > 0 && _BindingContactId > 0) && _AnswerContactId == _BindingContactId)
                            if(!binding.account_level)
                                binding.same_contact_binding = true;
                            else
                                binding.same_contact_binding = false;
                    }
                    
                    binding.answer_id = objAnswer.id.ToString();
                    binding.account_id = objAnswer.accounts_id.ToString();
                    binding.questionlayout_id = objAnswer.questionlayout_id.ToString();
                    binding.campaign_id = objAnswer.campaigns_id.ToString();
                    binding.dialog_id = objAnswer.dialog_id.ToString();
                    binding.created_by = objAnswer.created_by.ToString();
                }
            }
            return oQues;
        }

        public static void SaveAnswer(CampaignQuestionnaire oQuestionnaire, int pContactId = 0) 
        {
            using (BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection))
            {
                #region Member Variables
                var settings = oQuestionnaire.Form.Settings;
                var answerOptions = oQuestionnaire.Form.Settings.AnswerOptions;
                var binding = settings.DataBindings;
                sub_answers subanswer = null;
                //answer tempAnswer = null;
                //sub_answers tempSubAnswer = null;

                IDropbox iDropbox = null;
                ITextbox iTextbox = null;
                IMultipleChoice iMultiplechoice = null;
                ISchedule iSchedule = null;
                ISmartText iSmartText = null;
                IEnumerable<sub_answers> subanswersValues = null;
                List<MultipleChoiceValue> listMCV = null;
                List<OtherChoice> listOtherChoice = null;
                List<Attendie> listAttendie = null;
                List<SmartTextValue> listSmartText = null;
                bool IsNew = false;
                #endregion

                string answer_id = binding.answer_id;
                int int_answer_id = (!string.IsNullOrEmpty(answer_id) ? int.Parse(answer_id) : 0);
                var objAnswer = BPContext.answers.Include("sub_answers")
                    .FirstOrDefault(p => p.id == int_answer_id);
                if (objAnswer == null)
                {
                    #region Add Answer Details
                    IsNew = true;
                    //if not exists create new answer
                    objAnswer = new answer();
                    //set dummy values
                    //tempAnswer = BPContext.answers.OrderByDescending(a => a.id).FirstOrDefault();
                    //objAnswer.id = tempAnswer != null ? tempAnswer.id + 1 : 1;                
                    objAnswer.account_level = binding.account_level;
                    objAnswer.questionlayout_id = int.Parse(binding.questionlayout_id);
                    objAnswer.accounts_id = int.Parse(binding.account_id);
                    //objAnswer.contact_id = !string.IsNullOrEmpty(binding.contact_id) ? int.Parse(binding.contact_id) : (int?) null;
                    objAnswer.campaigns_id = int.Parse(binding.campaign_id);
                    objAnswer.dialog_id = int.Parse(binding.dialog_id);
                    objAnswer.created_by = UserSession.CurrentUser.UserId;
                    objAnswer.created_timestamp = DateTime.Now;
                    objAnswer.modified_on = DateTime.Now;
                    objAnswer.modified_by = UserSession.CurrentUser.UserId;
                    //set settings
                    objAnswer.OwnershipBrightvision = settings.BVOwnership;
                    objAnswer.OwnershipAccount = settings.CustomerOwnership;
                    #endregion
                }
                //update the value of the schedule_id
                if (!string.IsNullOrEmpty(binding.schedule_id))
                    objAnswer.schedule_id = int.Parse(binding.schedule_id);

                //update account level question to currently selected contact id
                //if (binding.account_level)
                objAnswer.contact_id = !string.IsNullOrEmpty(binding.contact_id) ? int.Parse(binding.contact_id) : (int?)null;

                /**
                 * [@jeff 04.12.2013]: https://brightvision.jira.com/browse/PLATFORM-2742
                 * assign the proper contact id when saving answers.
                 */
                //objAnswer.contact_id = pContactId;

                //bool maxIDset = false;
                for (int x = 0; x < answerOptions.Count; ++x)
                {
                    switch (oQuestionnaire.Type.ToLower())
                    {
                        #region SubAnswer for Textbox Component
                        case QuestionTypeConstants.Textbox:
                            iTextbox = answerOptions[x] as ITextbox;
                            subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                            if (subanswer == null)
                            {
                                //if not exists create new subanswer
                                subanswer = new sub_answers();
                                subanswer.answer_text = iTextbox.InputValue != null ? iTextbox.InputValue : string.Empty;
                                subanswer.sub_answer_index = -1;
                                objAnswer.sub_answers.Add(subanswer);
                            }
                            else
                            {
                                subanswer.answer_text = iTextbox.InputValue != null ? iTextbox.InputValue : string.Empty;
                                subanswer.sub_answer_index = -1;
                            }
                            break;
                        #endregion
                        #region SubAnswer for Dropbox Component
                        case QuestionTypeConstants.Dropbox:
                            iDropbox = answerOptions[x] as IDropbox;
                            subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                            if (subanswer == null)
                            {
                                //if not exists create new subanswer
                                subanswer = new sub_answers();
                                subanswer.answer_text = new DropboxValue()
                                {
                                    SelectionValue = iDropbox.SelectionValue != null ? iDropbox.SelectionValue : string.Empty,
                                    OtherValue = iDropbox.DefaultSelectionValueIfOther != null ? iDropbox.DefaultSelectionValueIfOther : string.Empty
                                    //OtherValue = iDropbox.SelectionValueIfOther != null ? iDropbox.SelectionValueIfOther : string.Empty
                                }.ToJSONString();
                                subanswer.sub_answer_index = 1;
                                objAnswer.sub_answers.Add(subanswer);
                            }
                            else
                            {
                                subanswer.answer_text = new DropboxValue()
                                {
                                    SelectionValue = iDropbox.SelectionValue != null ? iDropbox.SelectionValue : string.Empty,
                                    OtherValue = iDropbox.DefaultSelectionValueIfOther != null ? iDropbox.DefaultSelectionValueIfOther : string.Empty
                                    //OtherValue = iDropbox.SelectionValueIfOther != null ? iDropbox.SelectionValueIfOther : string.Empty
                                }.ToJSONString();
                                subanswer.sub_answer_index = 1;
                            }
                            break;
                        #endregion
                        #region SubAnswer for MultipleChoice Component
                        case QuestionTypeConstants.MultipleChoice:
                            iMultiplechoice = answerOptions[x] as IMultipleChoice;
                            //Note: sub_answer_index != -1 if answer is in multiple choice values
                            //      sub_answer_index == -1 if answer is in other choice

                            //set multiplechoicevalues
                            listMCV = iMultiplechoice.MultipleChoiceValues;
                            subanswersValues =
                                from obj in objAnswer.sub_answers
                                where obj.sub_answer_index != -1
                                select obj;
                            if (subanswersValues != null && subanswersValues.Count() > 0)
                            {
                                for (int q = 0; q < subanswersValues.Count(); ++q)
                                {
                                    subanswer = subanswersValues.ElementAtOrDefault(q);
                                    if (subanswer != null)
                                    {
                                        subanswer.sub_answer_index = Convert.ToInt16(listMCV[q].Checked);
                                        subanswer.answer_text = listMCV[q].TextPrefix;
                                    }
                                }
                            }
                            else
                            {
                                for (int q = 0; q < iMultiplechoice.MultipleChoiceValues.Count; ++q)
                                {
                                    //if not exists create new subanswer
                                    subanswer = new sub_answers();
                                    subanswer.sub_answer_index = Convert.ToInt16(listMCV[q].Checked);
                                    subanswer.answer_text = listMCV[q].TextPrefix;
                                    objAnswer.sub_answers.Add(subanswer);
                                }
                            }
                            //set otherchoice
                            listOtherChoice = iMultiplechoice.OtherChoices;
                            subanswersValues =
                                from obj in objAnswer.sub_answers
                                where obj.sub_answer_index == -1
                                select obj;
                            if (subanswersValues != null && subanswersValues.Count() > 0)
                            {
                                for (int j = 0; j < subanswersValues.Count(); ++j)
                                {
                                    subanswer = subanswersValues.ElementAtOrDefault(j);
                                    if (subanswer != null)
                                    {
                                        subanswer.answer_text = listOtherChoice[j].InputValue;
                                        if (subanswer.answer_text == null)
                                            subanswer.answer_text = string.Empty;
                                        subanswer.sub_answer_index = -1;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < listOtherChoice.Count; ++j)
                                {
                                    //if not exists create new subanswer
                                    subanswer = new sub_answers();
                                    subanswer.answer_text = listOtherChoice[j].InputValue;
                                    if (subanswer.answer_text == null)
                                        subanswer.answer_text = string.Empty;
                                    subanswer.sub_answer_index = -1;
                                    objAnswer.sub_answers.Add(subanswer);
                                }
                            }
                            break;
                        #endregion
                        #region SubAnswer for Schedule Component
                        case QuestionTypeConstants.Schedule:
                            iSchedule = answerOptions[x] as ISchedule;
                            /*********************************************************************
                            * Change Log:
                            Note:   sub_answer_index = 1 if answer is CalendarSelectedValue => Booking Type
                                    sub_answer_index = 2 if answer is Schedule date InputValue => Sales Person / Resource
                                    sub_answer_index = 3 if answer is StartTime InputValue => ScheduleValue
                                    sub_answer_index = 4 if answer is EndTime InputValue => Null
                                    sub_answer_index = 5 if answer is Attendies => ContactAttendies            
                                    sub_answer_index = 6 if answer is MeetingPlaceDetails => Null
                                    sub_answer_index = 7 if answer is OtherChoice => OtherChoice                               
                            *********************************************************************/

                            //set CalendarSelectedValue
                            if (iSchedule.ScheduleType != null)
                            {
                                subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 1).FirstOrDefault();
                                if (subanswer == null)
                                {
                                    //if not exists create new subanswer                            
                                    subanswer = new sub_answers();
                                    subanswer.answer_text = iSchedule.ScheduleType.ScheduleTypeSelectedValue;
                                    subanswer.sub_answer_index = 1;
                                    objAnswer.sub_answers.Add(subanswer);
                                }
                                else
                                {
                                    subanswer.answer_text = iSchedule.ScheduleType.ScheduleTypeSelectedValue;
                                    subanswer.sub_answer_index = 1;
                                }
                            }
                            if (iSchedule.ScheduleSalesPerson != null && iSchedule.ScheduleSalesPerson.SalesPersonSelectedValue != null)
                            {
                                //set Schedule date InputValue
                                subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 2).FirstOrDefault();
                                if (subanswer == null)
                                {
                                    //if not exists create new subanswer                            
                                    subanswer = new sub_answers();
                                    subanswer.answer_text = iSchedule.ScheduleSalesPerson.SalesPersonSelectedValue.ToJSONString();
                                    subanswer.sub_answer_index = 2;
                                    objAnswer.sub_answers.Add(subanswer);
                                }
                                else
                                {
                                    subanswer.answer_text = iSchedule.ScheduleSalesPerson.SalesPersonSelectedValue.ToJSONString();
                                    subanswer.sub_answer_index = 2;
                                }
                            }
                            //set StartTime InputValue
                            if (iSchedule.ScheduleValue != null)
                            {
                                subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 3).FirstOrDefault();
                                if (subanswer == null)
                                {
                                    //if not exists create new subanswer
                                    subanswer = new sub_answers();
                                    subanswer.answer_text = iSchedule.ScheduleValue.ToJSONString();
                                    subanswer.sub_answer_index = 3;
                                    objAnswer.sub_answers.Add(subanswer);
                                }
                                else
                                {
                                    subanswer.answer_text = iSchedule.ScheduleValue.ToJSONString();
                                    subanswer.sub_answer_index = 3;
                                }
                            }
                            //set EndTime InputValue
                            //subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 4).FirstOrDefault();
                            //if (subanswer == null) {
                            //    //if not exists create new subanswer
                            //    subanswer = new sub_answers();
                            //    //subanswer.answer_text = iSchedule.EndTime.InputValue != null ? iSchedule.EndTime.InputValue : string.Empty;
                            //    subanswer.sub_answer_index = 4;
                            //    objAnswer.sub_answers.Add(subanswer);
                            //} else {
                            //    //subanswer.answer_text = iSchedule.EndTime.InputValue != null ? iSchedule.EndTime.InputValue : string.Empty;
                            //    subanswer.sub_answer_index = 4;
                            //}
                            //set Attendies
                            listAttendie = iSchedule.Attendies;
                            subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 5);
                            int subCount = subanswersValues.Count();
                            int attCount = 0;
                            if (listAttendie == null || listAttendie.Count <= 0)
                            {
                                for (int d = 0; d < subCount; ++d)
                                {
                                    var ans = subanswersValues.ElementAtOrDefault(d);
                                    if (ans != null)
                                        BPContext.sub_answers.DeleteObject(ans);
                                }
                            }
                            else
                            {
                                attCount = listAttendie.Count;
                            }
                            subCount = subanswersValues.Count();
                            //remove existing excess
                            if (subCount > attCount)
                            {
                                int diff = subCount - attCount;
                                for (int j = diff - 1; j < subCount; ++j)
                                {
                                    var subAns = subanswersValues.ElementAtOrDefault(j);
                                    if (subAns != null)
                                        BPContext.sub_answers.DeleteObject(subAns);
                                }
                            }

                            if (attCount > 0)
                            {
                                for (int q = 0; q < attCount; ++q)
                                {
                                    subanswer = subanswersValues.ElementAtOrDefault(q);
                                    if (subanswer != null)
                                    {
                                        subanswer.sub_answer_index = 5;
                                        subanswer.answer_text = listAttendie[q].ToJSONString();
                                    }
                                    else
                                    {
                                        subanswer = new sub_answers();
                                        subanswer.sub_answer_index = 5;
                                        subanswer.answer_text = listAttendie[q].ToJSONString();
                                        objAnswer.sub_answers.Add(subanswer);
                                    }
                                }
                            }


                            //set otherchoice
                            listOtherChoice = iSchedule.OtherChoices;
                            subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 7);
                            if (subanswersValues != null && subanswersValues.Count() > 0)
                            {
                                for (int j = 0; j < subanswersValues.Count(); ++j)
                                {
                                    subanswer = subanswersValues.ElementAtOrDefault(j);
                                    if (subanswer != null)
                                    {
                                        subanswer.answer_text = listOtherChoice[j].InputValue;
                                        if (subanswer.answer_text == null)
                                            subanswer.answer_text = string.Empty;
                                        subanswer.sub_answer_index = 7;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < listOtherChoice.Count; ++j)
                                {
                                    //if not exists create new subanswer
                                    subanswer = new sub_answers();
                                    subanswer.answer_text = listOtherChoice[j].InputValue;
                                    if (subanswer.answer_text == null)
                                        subanswer.answer_text = string.Empty;
                                    subanswer.sub_answer_index = 7;
                                    objAnswer.sub_answers.Add(subanswer);
                                }
                            }
                            break;
                        #endregion
                        #region SubAnswer for SmartText
                        case QuestionTypeConstants.SmartText:
                            iSmartText = answerOptions[x] as ISmartText;
                            if (iSmartText.SmartTextValues == null || iSmartText.SmartTextValues.Count() < 1)
                                break;

                            subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                            if (subanswer == null)
                            {
                                subanswer = new sub_answers();
                                subanswer.answer_text = SmartTextValue.ToJSONString(iSmartText.SmartTextValues);
                                subanswer.sub_answer_index = 0;
                                objAnswer.sub_answers.Add(subanswer);
                            }
                            else
                            {
                                subanswer.answer_text = SmartTextValue.ToJSONString(iSmartText.SmartTextValues);
                                subanswer.sub_answer_index = 0;
                            }
                            break;
                        #endregion
                    }
                }
                try
                {
                    //[@jeff 11.03.2011 #PLATFORM-710]: added modified by and dates
                    objAnswer.modified_on = DateTime.Now;
                    objAnswer.modified_by = UserSession.CurrentUser.UserId;

                    if (IsNew)
                    {
                        BPContext.answers.AddObject(objAnswer);
                    }
                    BPContext.SaveChanges();
                    binding.answer_id = objAnswer.id.ToString();
                    BPContext.FIUpdateContactDialogDate(objAnswer.contact_id, objAnswer.modified_by);
                }
                catch (Exception ex)
                {
                    var x = ex;
                }
            }
        }

        /// <summary>
        /// Delete Answers based on answer_ids separated by comma
        /// </summary>
        /// <param name="answer_ids"></param>
        public static void DeleteAnswers(int? dialog_id, int? contact_id, int? campaign_id, int? account_id, bool account_level) {
            BrightPlatformEntities BPContext = new BrightPlatformEntities(UserSession.EntityConnection);
            try {
                BPContext.FIDeleteAnswers(campaign_id, account_id, contact_id, dialog_id, account_level);
            } catch {
                
            }
        }
    }
}
