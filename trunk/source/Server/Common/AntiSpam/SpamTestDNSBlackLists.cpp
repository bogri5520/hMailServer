// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#include "StdAfx.h"
#include "SpamTestDNSBlackLists.h"

#include "SpamTestData.h"
#include "SpamTestResult.h"

#include "../BO/DNSBlackLists.h"

#include "../../SMTP/SPF/SPF.h"
#include "../../SMTP/BLCheck.h"

#ifdef _DEBUG
#define DEBUG_NEW new(_NORMAL_BLOCK, __FILE__, __LINE__)
#define new DEBUG_NEW
#endif

#ifdef _DEBUG
#define DEBUG_NEW new(_NORMAL_BLOCK, __FILE__, __LINE__)
#define new DEBUG_NEW
#endif

namespace HM
{
   String 
   SpamTestDNSBlackLists::GetName() const
   {
      return "SpamTestDNSBlackLists";
   }

   bool 
   SpamTestDNSBlackLists::GetIsEnabled()
   {
      boost::shared_ptr<DNSBlackLists> pDNSBlackLists = Configuration::Instance()->GetAntiSpamConfiguration().GetDNSBlackLists();

      vector<boost::shared_ptr<DNSBlackList> > vec = pDNSBlackLists->GetVector();
      vector<boost::shared_ptr<DNSBlackList> >::iterator iter = vec.begin();
      vector<boost::shared_ptr<DNSBlackList> >::iterator iterEnd = vec.end();
      for (; iter != iterEnd; iter++)
      {
         boost::shared_ptr<DNSBlackList> pDNSBL = (*iter);

         if (pDNSBL->GetIsActive())
            return true;
      }

      return false;
   }

   set<boost::shared_ptr<SpamTestResult> >
   SpamTestDNSBlackLists::RunTest(boost::shared_ptr<SpamTestData> pTestData)
   {
      set<boost::shared_ptr<SpamTestResult> > setSpamTestResults;

      String sMessage = "";
      int iScore = 0;

      const IPAddress &addr = pTestData->GetOriginatingIP();
      if (addr.IsAny())
         return setSpamTestResults;

      boost::shared_ptr<DNSBlackLists> pDNSBlackLists = HM::Configuration::Instance()->GetAntiSpamConfiguration().GetDNSBlackLists();
      
      boost_foreach(boost::shared_ptr<DNSBlackList> pDNSBL, pDNSBlackLists->GetVector())
      {
         if (pDNSBL->GetIsActive() && 
            BLCheck::ClientExistsInDNSBL(addr, pDNSBL->GetDNSHost(), pDNSBL->GetExpectedResult()))
         {
            int iSomeScore = pDNSBL->GetScore();
            boost::shared_ptr<SpamTestResult> pResult = boost::shared_ptr<SpamTestResult>(new SpamTestResult(GetName(), SpamTestResult::Fail, iSomeScore, pDNSBL->GetRejectMessage()));
            
            setSpamTestResults.insert(pResult);
         }
      }

     return setSpamTestResults;
   }

}