// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#pragma once

namespace HM
{
   class Message;
   class MessageRecipient;
   class RuleResult;
   class ServerInfo;


   class ExternalDelivery
   {
   public:
      ExternalDelivery(const String &sSendersIP, boost::shared_ptr<Message> message, const RuleResult &globalRuleResult);
      ~ExternalDelivery(void);

      bool Perform(vector<String> &saErrorMessages);

   private:

      void _DeliverToExternalDomain(vector<boost::shared_ptr<MessageRecipient> > &vecRecipients, boost::shared_ptr<ServerInfo> serverInfo);
      void _InitiateExternalConnection(vector<boost::shared_ptr<MessageRecipient> > &vecRecipients, boost::shared_ptr<ServerInfo> serverInfo);

      bool _ResolveRecipientServer(boost::shared_ptr<ServerInfo> &serverInfo, vector<boost::shared_ptr<MessageRecipient> > &vecRecipients, vector<String> &saMailServers);
      bool _RecipientWithNonFatalDeliveryErrorExists(vector<boost::shared_ptr<MessageRecipient> > &vecRecipients);
      void _HandleExternalDeliveryFailure(vector<boost::shared_ptr<MessageRecipient> > &vecRecipients, bool bIsFatal, String &sErrorString);
      void _HandleNoRecipientServers(vector<boost::shared_ptr<MessageRecipient> > &vecRecipients, bool bDNSQueryOK, bool isSpecificRelayServer);
      
      void _CollectDeliveryResult(const String &serverHostName, vector<boost::shared_ptr<MessageRecipient> > &vecRecipients, vector<String> &saErrorMessages, map<String,String> &mapFailedDueToNonFatalError);
      bool _RescheduleDelivery(map<String,String> &mapFailedDueToNonFatalError,vector<String> &saErrorMessages);
      // Type changed from void to bool for use with ETRN.
      // Function not called anywhere else to matter
      bool _GetRetryOptions(map<String,String> &mapFailedDueToNonFatalError, long &lNoOfRetries, long &lMinutesBetween);

      IPAddress _GetLocalAddress();

      const String &_sendersIP;
      const boost::shared_ptr<Message> _originalMessage;
      const RuleResult &_globalRuleResult;   

      int m_iQuickRetries;      
      int m_iQuickRetriesMinutes;      
      int m_iQueueRandomnessMinutes;
      int m_iMXTriesFactor; 
   };
}