// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#pragma once

namespace HM
{
   class GreyListTriplet;

   class PersistentGreyList
   {
   public:
      PersistentGreyList(void);
      ~PersistentGreyList(void);

      static boost::shared_ptr<GreyListTriplet> GetRecord(const String &sSenderAddress, const String &sRecipientAddress, const IPAddress &remoteIP);
      static bool AddObject(boost::shared_ptr<GreyListTriplet> pTriplet);

      static bool ResetDeletionTime(boost::shared_ptr<GreyListTriplet> pTriplet);

      static void IncreaseBlocked(__int64 iTripletID);
      static void ClearExpiredRecords();
      static void ClearAllRecords();
   private:

   };
}