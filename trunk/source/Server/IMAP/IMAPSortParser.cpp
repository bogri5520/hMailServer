// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#include "StdAfx.h"
#include ".\imapsortparser.h"
#include "IMAPSimpleCommandParser.h"
#include "IMAPCommand.h"


namespace HM
{
   IMAPSortParser::IMAPSortParser(void)
   {
   }

   IMAPSortParser::~IMAPSortParser(void)
   {
   }

   void 
   IMAPSortParser::Parse(const String &sExpression)
   {
      std::vector<String> vecSortCriterias = StringParser::SplitString(sExpression, " ");
      std::vector<String>::iterator iter = vecSortCriterias.begin();
      while (iter != vecSortCriterias.end())
      {
         String sPart = (*iter);

         sPart.ToUpper();
         
         if (sPart == _T("REVERSE"))
         {
            iter++;
            if (iter != vecSortCriterias.end())
            {
               sPart = (*iter);
               m_vecSortTypes.push_back(std::make_pair(false, sPart));
            }
         }
         else
         {
            m_vecSortTypes.push_back(std::make_pair(true, sPart));
         }

         if (iter != vecSortCriterias.end())
            iter++;
      }
   }
}