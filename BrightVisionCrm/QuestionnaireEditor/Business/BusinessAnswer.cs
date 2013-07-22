using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BrightVision.DQControl.Business;
using BrightVision.Model;

namespace QuestionnaireEditor.Business {
    public static class BusinessAnswer {
        public static CampaignQuestionnaire BindAnswer(CampaignQuestionnaire oQuestionnaire, BrightPlatformEntities BPContext, int question_id) {
            var oQues = oQuestionnaire;
            if (oQues != null) {
                var objAnswer = BPContext.answers.Include("sub_answers").FirstOrDefault(p => p.questions_id == question_id);               
                if (objAnswer != null) {
                    sub_answers subanswer;
                    IDropbox iDropbox;
                    ITextbox iTextbox;
                    IMultipleChoice iMultiplechoice;

                    IEnumerable<sub_answers> subanswersMultipleChoiceValues;
                    List<MultipleChoiceValue> listMCV;
                    List<OtherChoice> listOtherChoice;

                    var settings = oQues.Form.Settings;
                    var answerOptions = oQues.Form.Settings.AnswerOptions;
                    var binding = settings.DataBindings;

                    //set settings
                    settings.BVOwnership = objAnswer.OwnershipBrightvision;
                    settings.CustomerOwnership = objAnswer.OwnershipAccount;

                    //set answeroptions
                    for (int x = 0; x < answerOptions.Count; ++x) {
                        if (oQues.Type.ToLower() == QuestionTypeConstants.Dropbox) {
                            subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                            if (subanswer != null) {
                                iDropbox = answerOptions[x] as IDropbox;
                                iDropbox.SelectionValue = subanswer.sub_answer_index.ToString();
                                iDropbox.SelectionValueIfOther = subanswer.answer_text;
                            }
                        } else if (oQues.Type.ToLower() == QuestionTypeConstants.Textbox) {
                            subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                            if (subanswer != null) {
                                iTextbox = answerOptions[x] as ITextbox;
                                iTextbox.InputValue = subanswer.answer_text;
                            }
                        } else if (oQues.Type.ToLower() == QuestionTypeConstants.MultipleChoice) {
                            //Note: sub_answer_index != -1 if answer is in multiple choice values
                            //      sub_answer_index == -1 if answer is in other choice

                            //set multiplechoicevalues
                            subanswersMultipleChoiceValues =
                                from obj in objAnswer.sub_answers
                                where obj.sub_answer_index != -1
                                select obj;
                            iMultiplechoice = answerOptions[x] as IMultipleChoice;
                            if (subanswersMultipleChoiceValues != null) {                                
                                for (int q = 0; q < subanswersMultipleChoiceValues.Count(); ++q) {
                                    subanswer = subanswersMultipleChoiceValues.ElementAtOrDefault(q);
                                    if (subanswer != null) {
                                        listMCV = iMultiplechoice.MultipleChoiceValues;
                                        listMCV[q].Checked = Convert.ToBoolean(subanswer.sub_answer_index);
                                    }
                                }
                            }
                            //set otherchoice
                            subanswersMultipleChoiceValues =
                                from obj in objAnswer.sub_answers
                                where obj.sub_answer_index == -1
                                select obj;
                            if (subanswersMultipleChoiceValues != null) {
                                for (int j = 0; j < subanswersMultipleChoiceValues.Count(); ++j) {
                                    subanswer = subanswersMultipleChoiceValues.ElementAtOrDefault(j);
                                    if (subanswer != null) {
                                        listOtherChoice = iMultiplechoice.OtherChoices;
                                        listOtherChoice[j].InputValue = subanswer.answer_text;
                                    }
                                }
                            }
                        }
                    }

                    //set data bindings
                    binding.answer_id = objAnswer.id.ToString();
                    binding.account_id = objAnswer.accounts_id.ToString();
                    binding.contact_id = objAnswer.contact_id.ToString();
                    binding.question_id = question_id.ToString();
                    binding.campaign_id = objAnswer.campaigns_id.ToString();
                    binding.dialog_id = objAnswer.dialog_id.ToString();
                }
            }
            return oQues;
        }

        public static void SaveAnswer(CampaignQuestionnaire oQuestionnaire, BrightPlatformEntities BPContext, int question_id) {
            #region Member Variables
            BPContext = new BrightPlatformEntities();
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
            ISeminar iSeminar = null;

            IEnumerable<sub_answers> subanswersValues = null;
            List<MultipleChoiceValue> listMCV = null;
            List<OtherChoice> listOtherChoice = null;
            List<MeetingPlaceDetail> listMeetingPlace = null;
            List<Attendie> listAttendie = null;

            bool IsNew = false; 
            #endregion
            var objAnswer = BPContext.answers.Include("sub_answers").FirstOrDefault(p => p.questions_id == question_id);
            if (objAnswer == null) {
                #region Add Answer Details
                IsNew = true;
                //if not exists create new answer
                objAnswer = new answer();
                //set dummy values
                //tempAnswer = BPContext.answers.OrderByDescending(a => a.id).FirstOrDefault();
                //objAnswer.id = tempAnswer != null ? tempAnswer.id + 1 : 1;
                objAnswer.questions_id = question_id;
                objAnswer.accounts_id = 1;
                objAnswer.contact_id = 1;
                objAnswer.campaigns_id = 1;
                objAnswer.dialog_id = 1;
                objAnswer.created_by = 1;
                objAnswer.created_timestamp = DateTime.Now;
                //set settings
                objAnswer.OwnershipBrightvision = settings.BVOwnership;
                objAnswer.OwnershipAccount = settings.CustomerOwnership; 
                #endregion
            }
            //bool maxIDset = false;
            for (int x = 0; x < answerOptions.Count; ++x) {
                switch (oQuestionnaire.Type.ToLower()) {
                    #region SubAnswer for Textbox Component
                    case QuestionTypeConstants.Textbox:
                        iTextbox = answerOptions[x] as ITextbox;
                        subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();
                            subanswer.answer_text = iTextbox.InputValue != null ? iTextbox.InputValue : string.Empty;
                            subanswer.sub_answer_index = -1;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.answer_text = iTextbox.InputValue != null ? iTextbox.InputValue : string.Empty;
                            subanswer.sub_answer_index = -1;
                        }
                        break; 
                    #endregion
                    #region SubAnswer for Dropbox Component
                    case QuestionTypeConstants.Dropbox:
                        iDropbox = answerOptions[x] as IDropbox;
                        subanswer = objAnswer.sub_answers.ElementAtOrDefault(x);
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();                            
                            subanswer.sub_answer_index = short.Parse(iDropbox.SelectionValue);
                            subanswer.answer_text = iDropbox.SelectionValueIfOther;
                            if (subanswer.answer_text == null)
                                subanswer.answer_text = string.Empty;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.sub_answer_index = short.Parse(iDropbox.SelectionValue);
                            if (subanswer.answer_text == null)
                                subanswer.answer_text = string.Empty;
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
                                    subanswer.sub_answer_index = Convert.ToInt16(listMCV[q].Checked);
                                    subanswer.answer_text = listMCV[q].TextPrefix;
                                }
                            }
                        } else {
                            for (int q = 0; q < iMultiplechoice.MultipleChoiceValues.Count; ++q) {
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
                        if (subanswersValues != null && subanswersValues.Count() > 0) {
                            for (int j = 0; j < subanswersValues.Count(); ++j) {
                                subanswer = subanswersValues.ElementAtOrDefault(j);
                                if (subanswer != null) {
                                    subanswer.answer_text = listOtherChoice[j].InputValue;
                                    if (subanswer.answer_text == null)
                                        subanswer.answer_text = string.Empty;
                                    subanswer.sub_answer_index = -1;
                                }
                            }
                        } else {
                            for (int j = 0; j < listOtherChoice.Count; ++j) {
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
                         Note: sub_answer_index = 1 if answer is CalendarSelectedValue
                               sub_answer_index = 2 if answer is Schedule date InputValue
                               sub_answer_index = 3 if answer is StartTime InputValue
                               sub_answer_index = 4 if answer is EndTime InputValue
                               sub_answer_index = 5 if answer is Attendies                                
                               sub_answer_index = 6 if answer is MeetingPlaceDetails
                               sub_answer_index = 7 if answer is OtherChoice                                
                        *********************************************************************/
                        //set CalendarSelectedValue
                        subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 1).FirstOrDefault();
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();
                            subanswer.answer_text = iSchedule.CalendarOption.CalendarSelectedValue != null ? iSchedule.CalendarOption.CalendarSelectedValue : string.Empty;
                            subanswer.sub_answer_index = 1;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.answer_text = iSchedule.CalendarOption.CalendarSelectedValue != null ? iSchedule.CalendarOption.CalendarSelectedValue : string.Empty;
                            subanswer.sub_answer_index = 1;
                        }
                        //set Schedule date InputValue
                        subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 2).FirstOrDefault();
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();
                            subanswer.answer_text = iSchedule.ScheduleDate.InputValue != null ? iSchedule.ScheduleDate.InputValue : string.Empty;
                            subanswer.sub_answer_index = 2;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.answer_text = iSchedule.ScheduleDate.InputValue != null ? iSchedule.ScheduleDate.InputValue : string.Empty;
                            subanswer.sub_answer_index = 2;
                        }
                        //set StartTime InputValue
                        subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 3).FirstOrDefault();
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();
                            subanswer.answer_text = iSchedule.StartTime.InputValue != null ? iSchedule.StartTime.InputValue : string.Empty;
                            subanswer.sub_answer_index = 3;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.answer_text = iSchedule.StartTime.InputValue != null ? iSchedule.StartTime.InputValue : string.Empty;
                            subanswer.sub_answer_index = 3;
                        }
                        //set EndTime InputValue
                        subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 4).FirstOrDefault();
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();
                            subanswer.answer_text = iSchedule.EndTime.InputValue != null ? iSchedule.EndTime.InputValue : string.Empty;
                            subanswer.sub_answer_index = 4;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.answer_text = iSchedule.EndTime.InputValue != null ? iSchedule.EndTime.InputValue : string.Empty;
                            subanswer.sub_answer_index = 4;
                        }
                        //set Attendies
                        listAttendie = iSchedule.Attendies;
                        subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 5); 
                        if (subanswersValues != null && subanswersValues.Count() > 0) {
                            for (int q = 0; q < subanswersValues.Count(); ++q) {
                                subanswer = subanswersValues.ElementAtOrDefault(q);
                                if (subanswer != null) {
                                    subanswer.sub_answer_index = 5;
                                    subanswer.answer_text = listAttendie[q].ToJSONString();
                                }
                            }
                        } else {
                            for (int q = 0; q < listAttendie.Count; ++q) {
                                //if not exists create new subanswer
                                subanswer = new sub_answers();
                                subanswer.sub_answer_index = 5;
                                subanswer.answer_text = listAttendie[q].ToJSONString();
                                objAnswer.sub_answers.Add(subanswer);
                            }
                        }
                        //set MeetingPlaceDetails
                        listMeetingPlace = iSchedule.MeetingPlaceDetails;
                        subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 6);     
                        if (subanswersValues != null && subanswersValues.Count() > 0) {
                            for (int q = 0; q < subanswersValues.Count(); ++q) {
                                subanswer = subanswersValues.ElementAtOrDefault(q);
                                if (subanswer != null) {
                                    subanswer.sub_answer_index = 6;
                                    subanswer.answer_text = listMeetingPlace[q].InputValue;
                                }
                            }
                        } else {
                            for (int q = 0; q < listMeetingPlace.Count; ++q) {
                                //if not exists create new subanswer
                                subanswer = new sub_answers();
                                subanswer.sub_answer_index = 6;
                                subanswer.answer_text = listMeetingPlace[q].InputValue;
                                objAnswer.sub_answers.Add(subanswer);
                            }
                        }
                        //set otherchoice
                        listOtherChoice = iSchedule.OtherChoices;
                        subanswersValues = objAnswer.sub_answers.Where(k=>k.sub_answer_index == 7);                            
                        if (subanswersValues != null && subanswersValues.Count() > 0) {
                            for (int j = 0; j < subanswersValues.Count(); ++j) {
                                subanswer = subanswersValues.ElementAtOrDefault(j);
                                if (subanswer != null) {
                                    subanswer.answer_text = listOtherChoice[j].InputValue;
                                    if (subanswer.answer_text == null)
                                        subanswer.answer_text = string.Empty;
                                    subanswer.sub_answer_index = 7;
                                }
                            }
                        } else {
                            for (int j = 0; j < listOtherChoice.Count; ++j) {
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
                    #region Sub Answer for Seminar Component
                    case QuestionTypeConstants.Seminar:                       
		                iSeminar = answerOptions[x] as ISeminar;
                        /*********************************************************************
                         Note: sub_answer_index = 1 if answer is SelectedValue                              
                               sub_answer_index = 2 if answer is Attendies    
                               sub_answer_index = 3 if answer is OtherChoice                                
                        *********************************************************************/
                        //set SelectedValue
                        subanswer = objAnswer.sub_answers.Where(sa => sa.sub_answer_index == 1).FirstOrDefault();
                        if (subanswer == null) {
                            //if not exists create new subanswer
                            subanswer = new sub_answers();
                            subanswer.answer_text = iSeminar.SeminarSchedule.SelectedValue != null ? iSeminar.SeminarSchedule.SelectedValue : string.Empty;
                            subanswer.sub_answer_index = 1;
                            objAnswer.sub_answers.Add(subanswer);
                        } else {
                            subanswer.answer_text = iSeminar.SeminarSchedule.SelectedValue != null ? iSeminar.SeminarSchedule.SelectedValue : string.Empty;
                            subanswer.sub_answer_index = 1;
                        }
                        //set Attendies
                        listAttendie = iSeminar.Attendies;
                        subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 2); 
                        if (subanswersValues != null && subanswersValues.Count() > 0) {
                            for (int q = 0; q < subanswersValues.Count(); ++q) {
                                subanswer = subanswersValues.ElementAtOrDefault(q);
                                if (subanswer != null) {
                                    subanswer.sub_answer_index = 2;
                                    subanswer.answer_text = listAttendie[q].ToJSONString();
                                }
                            }
                        } else {
                            for (int q = 0; q < listAttendie.Count; ++q) {
                                //if not exists create new subanswer
                                subanswer = new sub_answers();
                                subanswer.sub_answer_index = 2;
                                subanswer.answer_text = listAttendie[q].ToJSONString();
                                objAnswer.sub_answers.Add(subanswer);
                            }
                        }
                        //set otherchoice
                        listOtherChoice = iSeminar.OtherChoices;
                        subanswersValues = objAnswer.sub_answers.Where(k => k.sub_answer_index == 3); 
                        if (subanswersValues != null && subanswersValues.Count() > 0) {
                            for (int j = 0; j < subanswersValues.Count(); ++j) {
                                subanswer = subanswersValues.ElementAtOrDefault(j);
                                if (subanswer != null) {
                                    subanswer.answer_text = listOtherChoice[j].InputValue;
                                    if (subanswer.answer_text == null)
                                        subanswer.answer_text = string.Empty;
                                    subanswer.sub_answer_index = 3;
                                }
                            }
                        } else {
                            for (int j = 0; j < listOtherChoice.Count; ++j) {
                                //if not exists create new subanswer
                                subanswer = new sub_answers();
                                subanswer.answer_text = listOtherChoice[j].InputValue;
                                if (subanswer.answer_text == null)
                                    subanswer.answer_text = string.Empty;
                                subanswer.sub_answer_index = 3;
                                objAnswer.sub_answers.Add(subanswer);
                            }
                        } 	
                        break;
                    #endregion
                }
            }
            try {
                if(IsNew)
                    BPContext.answers.AddObject(objAnswer);
                BPContext.SaveChanges();
            } catch (Exception ex) {
                var x = ex;
            }
        }
    }
}
